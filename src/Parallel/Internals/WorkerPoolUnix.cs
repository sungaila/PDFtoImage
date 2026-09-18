using System.IO;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Parallel.Internals
{
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    internal sealed class WorkerPoolUnix : WorkerPool
    {
        private readonly DirectoryInfo _directory;
        private long _nextWorker;

        internal WorkerPoolUnix(int workerCount) : base(workerCount)
        {
            // .NET creates this directory atomically with mode 0700 on Unix.
            // Short names also fit macOS's smaller sockaddr_un.sun_path.
            _directory = Directory.CreateTempSubdirectory("pti-");
        }

        internal string SocketDirectory => _directory.FullName;

        protected override async Task<WorkerConnection> StartWorkerAsync(CancellationToken cancellationToken)
        {
            var name = Interlocked.Increment(ref _nextWorker).ToString("x");
            return await WorkerConnectionUnix.StartAsync(
                Path.Combine(SocketDirectory, name + "c"),
                Path.Combine(SocketDirectory, name + "l"), cancellationToken).ConfigureAwait(false);
        }

        // Startup attempts own their listener paths until they finish. Only remove
        // the directory once those attempts (including cancelled ones) have drained.
        protected override void DisposeResources() => _directory.Delete();
    }
}