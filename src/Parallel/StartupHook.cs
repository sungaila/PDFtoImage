using PDFtoImage.Parallel.Internals;
using System;
using System.Runtime.Versioning;

/// <summary>
/// Bootstraps a worker before the host application's entry point is invoked.
/// </summary>
[SupportedOSPlatform("windows10.0")]
#if NETCOREAPP
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1050")]
#endif
public static class StartupHook
{
    /// <summary>
    /// Runs the worker host when this process was started by PDFtoImage.Parallel.
    /// </summary>
    public static void Initialize()
    {
        var pipeName = Environment.GetEnvironmentVariable(WorkerProcessLauncher.WorkerPipeEnvironmentVariable);
        if (string.IsNullOrEmpty(pipeName))
            return;

        Environment.SetEnvironmentVariable(WorkerProcessLauncher.WorkerPipeEnvironmentVariable, null);

        var exitCode = WorkerHost.RunAsync(pipeName).GetAwaiter().GetResult();
        Environment.Exit(exitCode);
    }
}