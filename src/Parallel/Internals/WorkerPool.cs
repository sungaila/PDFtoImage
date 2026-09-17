using PDFtoImage;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Versioning;

namespace PDFtoImage.Parallel.Internals
{
    [SupportedOSPlatform("windows6.2")]
    internal sealed class WorkerPool : IAsyncDisposable
    {
        private readonly WindowsJob _job;
        private readonly WorkerConnection[] _workers;
        private int _nextWorker;
        private bool _disposed;

        private WorkerPool(WindowsJob job, WorkerConnection[] workers, int pageCount)
        {
            _job = job;
            _workers = workers;
            PageCount = pageCount;
        }

        internal int PageCount { get; }

        internal int[] WorkerProcessIds => [.. _workers.Select(worker => worker.ProcessId)];

        internal static async Task<WorkerPool> CreateAsync(
            int workerCount,
            byte[] pdf,
            string? password,
            CancellationToken cancellationToken)
        {
            var job = WindowsJob.Create();

            try
            {
                var startupTasks = Enumerable.Range(0, workerCount)
                    .Select(_ => WorkerConnection.StartAsync(job, pdf, password, cancellationToken))
                    .ToArray();

                (WorkerConnection Worker, int PageCount)[] startupResults;
                try
                {
                    startupResults = await Task.WhenAll(startupTasks).ConfigureAwait(false);
                }
                catch
                {
                    foreach (var startupTask in startupTasks)
                    {
                        if (startupTask.Status == TaskStatus.RanToCompletion)
                            startupTask.Result.Worker.Dispose();
                    }

                    throw;
                }

                var pageCount = startupResults[0].PageCount;

                return startupResults.Any(result => result.PageCount != pageCount)
                    ? throw new InvalidDataException("The worker processes reported inconsistent PDF page counts.")
                    : new WorkerPool(job, [.. startupResults.Select(result => result.Worker)], pageCount);
            }
            catch
            {
                job.Dispose();
                throw;
            }
        }

        internal Task<byte[]> RenderPageAsync(int page, RenderOptions options, CancellationToken cancellationToken)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(WorkerPool));

            var worker = _workers[_nextWorker];
            _nextWorker = (_nextWorker + 1) % _workers.Length;
            return worker.RenderPageAsync(page, options, cancellationToken);
        }

        public ValueTask DisposeAsync()
        {
            if (_disposed)
                return default;

            _disposed = true;

            foreach (var worker in _workers)
                worker.Dispose();

            _job.Dispose();
            return default;
        }
    }
}
