using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.Versioning;

namespace PDFtoImage.Parallel.Internals
{
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    internal static class WorkerProcessLauncherUnix
    {
        internal const string WorkerSocketEnvironmentVariable = "PDFTOIMAGE_PARALLEL_WORKER_SOCKET";
        internal const string WorkerLifetimeEnvironmentVariable = "PDFTOIMAGE_PARALLEL_WORKER_LIFETIME";

        internal static Process Start(string commandPath, string lifetimePath)
        {
            var command = WorkerLaunchCommand.Create();
            var start = new ProcessStartInfo(command.ProcessPath) { UseShellExecute = false };
            foreach (var argument in command.Arguments)
                start.ArgumentList.Add(argument);

            start.Environment.TryGetValue("DOTNET_STARTUP_HOOKS", out var existingHooks);
            start.Environment["DOTNET_STARTUP_HOOKS"] = string.IsNullOrEmpty(existingHooks)
                ? command.StartupHookPath
                : command.StartupHookPath + Path.PathSeparator + existingHooks;
            start.Environment[WorkerSocketEnvironmentVariable] = commandPath;
            start.Environment[WorkerLifetimeEnvironmentVariable] = lifetimePath;
            return Process.Start(start) ?? throw new InvalidOperationException("Could not create a PDF conversion worker process.");
        }
    }
}