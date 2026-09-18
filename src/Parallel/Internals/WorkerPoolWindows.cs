using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

namespace PDFtoImage.Parallel.Internals
{
    [SupportedOSPlatform("windows10.0")]
    internal sealed class WorkerPoolWindows : IWorkerPool
    {
        private sealed class Slot
        {
            internal WorkerConnectionWindows? Worker;
        }

        private readonly Lock _gate = new();

        private readonly WindowsJob _job;

        private readonly List<Slot> _workers = [];

        private readonly ConcurrentStack<Slot> _available;

        private readonly SemaphoreSlim _slots;

        private readonly CancellationTokenSource _shutdown = new();

        private readonly TaskCompletionSource _drained = new(TaskCreationOptions.RunContinuationsAsynchronously);

        private bool _disposed;

        private bool _cleanupFinished;

        private int _activeOperations;

        internal WorkerPoolWindows(int workerCount)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(workerCount);
            WorkerCount = workerCount;
            _available = new ConcurrentStack<Slot>();
            _slots = new SemaphoreSlim(workerCount);
            _job = WindowsJob.Create();
        }

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

        private async Task<T> ExecuteAsync<T>(PdfRequest request,
            Func<WorkerConnectionWindows, int, CancellationToken, Task<T>> execute, CancellationToken cancellationToken)
        {
            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_disposed, typeof(ParallelPdfProcessor));
                _activeOperations++;
            }

            Slot? slot = null;
            WorkerConnectionWindows? worker = null;
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
                    worker = await WorkerConnectionWindows.StartAsync(_job, cancellation.Token).ConfigureAwait(false);

                    lock (_gate)
                    {
                        cancellation.Token.ThrowIfCancellationRequested();
                        slot.Worker = worker;
                    }
                }

                var pageCount = await worker.LoadDocumentAsync(request, cancellation.Token).ConfigureAwait(false);

                return await execute(worker, pageCount, cancellation.Token).ConfigureAwait(false);
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
                if (slot != null)
                    _available.Push(slot);

                if (acquired)
                    _slots.Release();

                lock (_gate)
                {
                    _activeOperations--;
                    CompleteDisposalIfDrained();
                }
            }
        }

        public void Dispose()
        {
            WorkerConnectionWindows[] workers;
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

            try
            {
                _shutdown.Cancel();

                // Close the job first, so all processes terminate together.
                _job.Dispose();

                foreach (var worker in workers)
                {
                    worker.Dispose();
                }                    
            }
            finally
            {
                lock (_gate)
                {
                    _cleanupFinished = true;
                    CompleteDisposalIfDrained();
                }
            }
        }

        public async ValueTask DisposeAsync()
        {
            Dispose();
            await _drained.Task.ConfigureAwait(false);
        }

        private void CompleteDisposalIfDrained()
        {
            if (!_cleanupFinished || _activeOperations != 0 || _drained.Task.IsCompleted)
                return;

            _slots.Dispose();
            _shutdown.Dispose();
            _drained.TrySetResult();
        }
    }
}