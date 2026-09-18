using Microsoft.Win32.SafeHandles;
using PDFtoImage.Parallel.Internals;
using System;
using System.Globalization;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Bootstraps a worker before the host application's entry point is invoked.
/// </summary>
internal static class StartupHook
{
    /// <summary>
    /// Runs the worker host when this process was started by PDFtoImage.Parallel.
    /// </summary>
    public static void Initialize()
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
