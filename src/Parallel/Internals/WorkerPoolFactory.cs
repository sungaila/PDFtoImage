using System;

namespace PDFtoImage.Parallel.Internals
{
    internal static class WorkerPoolFactory
    {
        internal static IWorkerPool Create(int workerCount)
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(10))
                return new WorkerPoolWindows(workerCount);

            // Future Linux/macOS implementations plug into the same contract.
            throw new PlatformNotSupportedException("PDFtoImage.Parallel requires Windows 10 / Windows Server 2016 or newer.");
        }
    }
}