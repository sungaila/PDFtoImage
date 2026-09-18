using System;

namespace PDFtoImage.Parallel.Internals
{
    internal static class WorkerPoolFactory
    {
        internal static IWorkerPool Create(int workerCount)
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(10))
                return new WorkerPoolWindows(workerCount);

            if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
                return new WorkerPoolUnix(workerCount);

            throw new PlatformNotSupportedException("PDFtoImage.Parallel requires Windows 10 / Windows Server 2016 or newer, Linux, or macOS.");
        }
    }
}