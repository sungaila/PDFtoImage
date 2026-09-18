using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Parallel.Internals
{
    [SupportedOSPlatform("windows10.0")]
    internal sealed class WorkerPoolWindows : WorkerPool
    {
        private readonly WindowsJob _job;

        internal WorkerPoolWindows(int workerCount) : base(workerCount)
        {
            _job = WindowsJob.Create();
        }

        protected override async Task<WorkerConnection> StartWorkerAsync(CancellationToken cancellationToken) =>
            await WorkerConnectionWindows.StartAsync(_job, cancellationToken).ConfigureAwait(false);

        // Close the job first, so all processes terminate together.
        protected override void StopWorkers() => _job.Dispose();
    }
}