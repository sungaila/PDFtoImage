using PDFtoImage.Parallel.Internals;
using System;
using System.IO.Pipes;
using System.Net.Sockets;
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
        if (OperatingSystem.IsWindowsVersionAtLeast(10))
        {
            var pipeName = Environment.GetEnvironmentVariable(WorkerProcessLauncherWindows.WorkerPipeEnvironmentVariable);
            if (string.IsNullOrEmpty(pipeName))
                return;
            Environment.SetEnvironmentVariable(WorkerProcessLauncherWindows.WorkerPipeEnvironmentVariable, null);
            Environment.Exit(RunWindowsAsync(pipeName).GetAwaiter().GetResult());
        }
        else if (OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
        {
            var commandPath = Environment.GetEnvironmentVariable(WorkerProcessLauncherUnix.WorkerSocketEnvironmentVariable);
            var lifetimePath = Environment.GetEnvironmentVariable(WorkerProcessLauncherUnix.WorkerLifetimeEnvironmentVariable);
            if (string.IsNullOrEmpty(commandPath) && string.IsNullOrEmpty(lifetimePath))
                return;
            Environment.SetEnvironmentVariable(WorkerProcessLauncherUnix.WorkerSocketEnvironmentVariable, null);
            Environment.SetEnvironmentVariable(WorkerProcessLauncherUnix.WorkerLifetimeEnvironmentVariable, null);
            if (string.IsNullOrEmpty(commandPath) || string.IsNullOrEmpty(lifetimePath))
                Environment.Exit(1);
            Environment.Exit(RunUnixAsync(commandPath!, lifetimePath!).GetAwaiter().GetResult());
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows10.0")]
    private static async Task<int> RunWindowsAsync(string pipeName)
    {
        try
        {
            using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            await pipe.ConnectAsync(timeout.Token).ConfigureAwait(false);
            return await WorkerHost.RunAsync(pipe).ConfigureAwait(false);
        }
        catch { return 1; }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("linux")]
    [System.Runtime.Versioning.SupportedOSPlatform("macos")]
    private static async Task<int> RunUnixAsync(string commandPath, string lifetimePath)
    {
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            // The watchdog owns this socket until process exit. Disposing it here
            // could race the hook's normal exit with the watchdog's emergency exit.
            var lifetime = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            try
            {
                await lifetime.ConnectAsync(new UnixDomainSocketEndPoint(lifetimePath), timeout.Token).ConfigureAwait(false);
                WorkerLifetimeUnix.StartWatchdog(lifetime);
            }
            catch
            {
                lifetime.Dispose();
                throw;
            }

            using var command = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            await command.ConnectAsync(new UnixDomainSocketEndPoint(commandPath), timeout.Token).ConfigureAwait(false);
            using var stream = new NetworkStream(command, ownsSocket: false);
            return await WorkerHost.RunAsync(stream).ConfigureAwait(false);
        }
        catch { return 1; }
    }
}