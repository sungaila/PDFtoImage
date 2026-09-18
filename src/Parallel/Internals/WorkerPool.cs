using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Parallel.Internals
{
    internal class WorkerPool : IDisposable, IAsyncDisposable
    {
        protected sealed class Slot
        {
            internal WorkerConnection? Worker;
        }

        private readonly Lock _gate = new();

        private readonly List<Slot> _workers = [];

        protected readonly ConcurrentStack<Slot> _available;

        protected readonly SemaphoreSlim _slots;

        private readonly CancellationTokenSource _shutdown = new();

        private readonly TaskCompletionSource _drained = new(TaskCreationOptions.RunContinuationsAsynchronously);

        private bool _disposed;

        private bool _cleanupFinished;

        private List<Exception>? _cleanupErrors;

        private int _activeOperations;

        internal WorkerPool(int workerCount)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(workerCount);
            WorkerCount = workerCount;
            _available = new ConcurrentStack<Slot>();
            _slots = new SemaphoreSlim(workerCount);
        }

        protected virtual Task<WorkerConnection> StartWorkerAsync(CancellationToken cancellationToken) =>
            WorkerConnection.StartAsync(cancellationToken);

        protected virtual void StopWorkers() { }

        protected virtual void DisposeResources() { }

        public int WorkerCount { get; }

        public int[] WorkerProcessIds
        {
            get
            {
                lock (_gate)
                {
                    return [.. _workers.Where(slot => slot.Worker != null).Select(slot => slot.Worker!.ProcessId)];
                }
            }
        }

        public void ThrowIfDisposed()
        {
            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_disposed, typeof(ParallelPdfProcessor));
            }
        }

        public Task<int> GetPageCountAsync(PdfRequest request, CancellationToken cancellationToken) =>
            ExecuteAsync(request, static (_, pageCount, _) => Task.FromResult(pageCount), cancellationToken);

        public Task<SKBitmap> RenderPageAsync(PdfRequest request, Index page, RenderOptions options, CancellationToken cancellationToken) =>
            ExecuteAsync(request, (worker, count, token) =>
            {
                var offset = page.GetOffset(count);

                if (offset < 0 || offset >= count)
                    throw new ArgumentOutOfRangeException(nameof(page), $"The page must be between 0 and {count - 1}.");

                return worker.RenderPageAsync(offset, options, token);
            }, cancellationToken);

        public async Task ReleaseDocumentAsync(PdfRequest request)
        {
            var idleSlots = new List<Slot>();

            lock (_gate)
            {
                if (_disposed)
                    return;

                _activeOperations++;

                // A zero-timeout lease is the essential part of request cleanup:
                // workers that are now busy with another request are left alone.
                var attempts = _available.Count;
                for (var i = 0; i < attempts; i++)
                {
                    if (!_slots.Wait(0) || !_available.TryPop(out var slot))
                        break;

                    idleSlots.Add(slot);
                }
            }

            try
            {
                await Task.WhenAll(idleSlots.Select(slot => ReleaseDocumentFromIdleSlotAsync(slot, request))).ConfigureAwait(false);
            }
            finally
            {
                lock (_gate)
                {
                    _activeOperations--;
                    CompleteDisposalIfDrained();
                }
            }
        }

        public Guid?[] WorkerDocumentIds
        {
            get
            {
                lock (_gate)
                    return [.. _workers.Where(slot => slot.Worker != null).Select(slot => slot.Worker!.DocumentId)];
            }
        }

        public int[] WorkerDocumentLoadCounts
        {
            get
            {
                lock (_gate)
                    return [.. _workers.Where(slot => slot.Worker != null).Select(slot => slot.Worker!.DocumentLoadCount)];
            }
        }

        private async Task<T> ExecuteAsync<T>(PdfRequest request,
            Func<WorkerConnection, int, CancellationToken, Task<T>> execute, CancellationToken cancellationToken)
        {
            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_disposed, typeof(ParallelPdfProcessor));
                _activeOperations++;
            }

            Slot? slot = null;
            WorkerConnection? worker = null;
            var acquired = false;

            try
            {
                using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _shutdown.Token);

                await _slots.WaitAsync(cancellation.Token).ConfigureAwait(false);
                acquired = true;

                lock (_gate)
                {
                    cancellation.Token.ThrowIfCancellationRequested();
                    if (!_available.TryPop(out slot))
                    {
                        slot = new Slot();
                        _workers.Add(slot);
                    }
                    worker = slot.Worker;
                }

                if (worker == null)
                {
                    worker = await StartWorkerAsync(cancellation.Token).ConfigureAwait(false);

                    lock (_gate)
                    {
                        cancellation.Token.ThrowIfCancellationRequested();
                        slot.Worker = worker;
                    }
                }

                return await worker.ExecuteAsync(request,
                    (pageCount, token) => execute(worker, pageCount, token), cancellation.Token).ConfigureAwait(false);
            }
            catch (ParallelConversionException exception) when (exception.RemoteExceptionType != "WorkerProcessTerminated")
            {
                // A complete remote error frame leaves the connection synchronized.
                throw;
            }
            catch (ArgumentOutOfRangeException)
            {
                // Local page validation has not altered the IPC stream.
                throw;
            }
            catch
            {
                // A cancelled/failed frame cannot be reused. Only this lease is
                // discarded; a later job lazily creates its replacement.
                if (slot != null)
                {
                    lock (_gate)
                    {
                        slot.Worker = null;
                    }
                }

                worker?.Dispose();

                throw;
            }
            finally
            {
                lock (_gate)
                {
                    // Publishing a free slot is one state transition. ReleaseDocumentAsync
                    // observes the stack and semaphore under the same gate, so it must never
                    // see a slot before its semaphore permit (or vice versa).
                    if (slot != null)
                        _available.Push(slot);

                    if (acquired)
                        _slots.Release();

                    _activeOperations--;
                    CompleteDisposalIfDrained();
                }
            }
        }

        public void Dispose()
        {
            WorkerConnection[] workers;
            lock (_gate)
            {
                if (_disposed)
                    return;

                _disposed = true;
                workers = [.. _workers.Where(slot => slot.Worker != null).Select(slot => slot.Worker!)];

                foreach (var slot in _workers)
                {
                    slot.Worker = null;
                }
            }

            var errors = new List<Exception>();

            TryCleanup(_shutdown.Cancel, errors);
            TryCleanup(StopWorkers, errors);

            foreach (var worker in workers)
                TryCleanup(worker.Dispose, errors);

            lock (_gate)
            {
                _cleanupErrors = errors;
                _cleanupFinished = true;

                CompleteDisposalIfDrained();

                if (errors.Count > 0)
                    throw new AggregateException("Worker pool cleanup failed.", errors);
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                Dispose();
            }
            catch (AggregateException) { /* Report all cleanup errors after draining below. */ }

            await _drained.Task.ConfigureAwait(false);
        }

        private async Task ReleaseDocumentFromIdleSlotAsync(Slot slot, PdfRequest request)
        {
            try
            {
                var worker = slot.Worker;

                if (worker != null)
                    await worker.UnloadDocumentAsync(request.Id, _shutdown.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (_shutdown.IsCancellationRequested)
            {
                // Pool disposal is already terminating the worker.
            }
            catch
            {
                var worker = slot.Worker;

                lock (_gate)
                {
                    slot.Worker = null;
                }

                worker?.Dispose();
            }
            finally
            {
                lock (_gate)
                {
                    _available.Push(slot);
                    _slots.Release();
                }
            }
        }

        private void CompleteDisposalIfDrained()
        {
            if (!_cleanupFinished || _activeOperations != 0 || _drained.Task.IsCompleted)
                return;

            var errors = _cleanupErrors!;

            TryCleanup(DisposeResources, errors);
            TryCleanup(_slots.Dispose, errors);
            TryCleanup(_shutdown.Dispose, errors);

            if (errors.Count == 0)
                _drained.TrySetResult();
            else
                _drained.TrySetException(new AggregateException("Worker pool cleanup failed.", errors));
        }

        private static void TryCleanup(Action cleanup, List<Exception> errors)
        {
            try
            {
                cleanup();
            }
            catch (Exception exception)
            {
                errors.Add(exception);
            }
        }
    }
}