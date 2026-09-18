using Microsoft.Win32.SafeHandles;
using System;
using System.Globalization;
using System.IO.Pipes;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Parallel.Internals
{
    internal static class WorkerBootstrap
    {
#pragma warning disable CA2255 // Module initializers are intentional: Native AOT needs to enter the worker before Main().
        [ModuleInitializer]
#pragma warning restore CA2255
        internal static void InitializeModule()
        {
            if (!RuntimeFeature.IsDynamicCodeSupported)
                RunIfWorker();
        }

        internal static void RunIfWorker()
        {
            if (!OperatingSystem.IsWindowsVersionAtLeast(10) && !OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
                return;

            var pipeName = Environment.GetEnvironmentVariable(WorkerProcessLauncher.WorkerPipeEnvironmentVariable);

            if (string.IsNullOrEmpty(pipeName))
                return;

            var lifetimeValue = Environment.GetEnvironmentVariable(WorkerProcessLauncher.WorkerLifetimeEnvironmentVariable);

            Environment.SetEnvironmentVariable(WorkerProcessLauncher.WorkerPipeEnvironmentVariable, null);
            Environment.SetEnvironmentVariable(WorkerProcessLauncher.WorkerLifetimeEnvironmentVariable, null);

            if (!string.IsNullOrEmpty(lifetimeValue))
            {
                if (!long.TryParse(lifetimeValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var rawHandle) || rawHandle < 0)
                    Environment.Exit(1);

                WorkerLifetime.StartWatchdog(new SafeFileHandle(new IntPtr(rawHandle), ownsHandle: true));
            }

            Environment.Exit(RunAsync(pipeName).GetAwaiter().GetResult());
        }

        [System.Runtime.Versioning.SupportedOSPlatform("windows10.0")]
        [System.Runtime.Versioning.SupportedOSPlatform("linux")]
        [System.Runtime.Versioning.SupportedOSPlatform("macos")]
        private static async Task<int> RunAsync(string pipeName)
        {
            try
            {
                using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                await pipe.ConnectAsync(timeout.Token).ConfigureAwait(false);

                return await WorkerHost.RunAsync(pipe).ConfigureAwait(false);
            }
            catch
            {
                return 1;
            }
        }
    }
}