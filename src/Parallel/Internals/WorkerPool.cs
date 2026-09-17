using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Parallel.Internals
{
    [SupportedOSPlatform("windows10.0")]
    internal sealed class WorkerPool : IAsyncDisposable
    {
        private readonly WindowsJob _job;
        private readonly WorkerConnection[] _workers;
        private readonly ConcurrentQueue<WorkerConnection> _available;
        private readonly SemaphoreSlim _slots;
        private readonly CancellationTokenSource _shutdown = new();
        private int _disposed;
        private ParallelConversionException? _failure;

        private WorkerPool(WindowsJob job, WorkerConnection[] workers, int pageCount)
        {
            _job = job;
            _workers = workers;
            _available = new ConcurrentQueue<WorkerConnection>(workers);
            _slots = new SemaphoreSlim(workers.Length);
            PageCount = pageCount;
        }

        internal int PageCount { get; }
        internal int WorkerCount => _workers.Length;

        internal int[] WorkerProcessIds => [.. _workers.Select(worker => worker.ProcessId)];

        internal static async Task<WorkerPool> CreateAsync(
            int workerCount,
            byte[] pdf,
            string? password,
            CancellationToken cancellationToken,
            Func<int, int>? getWorkItemCount = null)
        {
            if (workerCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(workerCount));
            cancellationToken.ThrowIfCancellationRequested();
            var job = WindowsJob.Create();
            using var startupCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            var workers = new ConcurrentBag<WorkerConnection>();

            try
            {
                var first = await StartWorkerAsync().ConfigureAwait(false);
                var count = Math.Max(1, Math.Min(Math.Min(workerCount, first.PageCount), getWorkItemCount?.Invoke(first.PageCount) ?? int.MaxValue));
                var startupResults = await Task.WhenAll(Enumerable.Range(1, count - 1)
                    .Select(_ => StartWorkerAsync())).ConfigureAwait(false);
                if (startupResults.Any(result => result.PageCount != first.PageCount))
                    throw new InvalidDataException("The worker processes reported inconsistent PDF page counts.");

                return new WorkerPool(job, [first.Worker, .. startupResults.Select(result => result.Worker)], first.PageCount);

                async Task<(WorkerConnection Worker, int PageCount)> StartWorkerAsync()
                {
                    try
                    {
                        var result = await WorkerConnection.StartAsync(job, pdf, password, startupCancellation.Token).ConfigureAwait(false);
                        workers.Add(result.Worker);
                        return result;
                    }
                    catch
                    {
                        startupCancellation.Cancel();
                        throw;
                    }
                }
            }
            catch
            {
                foreach (var worker in workers)
                    worker.Dispose();
                job.Dispose();
                throw;
            }
        }

        internal async Task<byte[]> RenderPageAsync(int page, RenderOptions options, CancellationToken cancellationToken)
        {
            if (Volatile.Read(ref _disposed) != 0)
                throw new ObjectDisposedException(nameof(WorkerPool));

            using var requestCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _shutdown.Token);
            WorkerConnection? worker = null;
            var acquired = false;
            try
            {
                await _slots.WaitAsync(requestCancellation.Token).ConfigureAwait(false);
                acquired = true;
                if (!_available.TryDequeue(out worker))
                    throw new InvalidOperationException("No idle PDF worker is available.");
                return await worker.RenderPageAsync(page, options, requestCancellation.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // A partially written/read frame cannot safely be reused.
                if (acquired)
                    await DisposeAsync().ConfigureAwait(false);
                if (!cancellationToken.IsCancellationRequested && _failure != null)
                    ExceptionDispatchInfo.Capture(_failure).Throw();
                throw;
            }
            catch (ParallelConversionException exception) when (exception.RemoteExceptionType == "WorkerProcessTerminated")
            {
                Interlocked.CompareExchange(ref _failure, exception, null);
                await DisposeAsync().ConfigureAwait(false);
                throw;
            }
            catch (ObjectDisposedException) when (Volatile.Read(ref _disposed) != 0)
            {
                if (!cancellationToken.IsCancellationRequested && _failure != null)
                    ExceptionDispatchInfo.Capture(_failure).Throw();
                throw new OperationCanceledException("The worker pool was stopped.", requestCancellation.Token);
            }
            finally
            {
                if (worker != null)
                    _available.Enqueue(worker);
                if (acquired)
                    _slots.Release();
            }
        }

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return default;

            _shutdown.Cancel();

            foreach (var worker in _workers)
                worker.Dispose();

            _job.Dispose();
            return default;
        }
    }
}
