using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace PDFtoImage.Parallel.Internals
{
    internal static class WorkerProcessLauncher
    {
        internal const string WorkerPipeEnvironmentVariable = "PDFTOIMAGE_PARALLEL_WORKER_PIPE";
        internal const string WorkerLifetimeEnvironmentVariable = "PDFTOIMAGE_PARALLEL_WORKER_LIFETIME_HANDLE";

        internal static SafeProcessHandle Start(string pipeName, out SafeFileHandle? lifetime)
        {
            var command = WorkerLaunchCommand.Create();
            var start = new ProcessStartInfo(command.ProcessPath)
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = Environment.CurrentDirectory,
                InheritedHandles = []
            };

            foreach (var argument in command.Arguments)
                start.ArgumentList.Add(argument);

            if (command.StartupHookAssemblyName is { } startupHookAssemblyName)
            {
                start.Environment.TryGetValue("DOTNET_STARTUP_HOOKS", out var existingHooks);
                start.Environment["DOTNET_STARTUP_HOOKS"] = string.IsNullOrEmpty(existingHooks)
                    ? startupHookAssemblyName
                    : startupHookAssemblyName + Path.PathSeparator + existingHooks;
            }
            else
            {
                // Native AOT enters worker mode through WorkerBootstrap's module initializer.
                start.Environment.Remove("DOTNET_STARTUP_HOOKS");
            }

            start.Environment[WorkerPipeEnvironmentVariable] = pipeName;
            start.Environment.Remove(WorkerLifetimeEnvironmentVariable);

            SafeFileHandle? childLifetime = null;
            SafeFileHandle? parentLifetime = null;

            try
            {
                if (OperatingSystem.IsWindowsVersionAtLeast(10))
                {
                    start.KillOnParentExit = true;
                }
                else if (OperatingSystem.IsLinux())
                {
                    start.KillOnParentExit = true;
                }
                else if (OperatingSystem.IsMacOS())
                {
                    // .NET 11 does not expose KillOnParentExit on macOS. A single
                    // inherited pipe provides the same parent-death guarantee without
                    // native interop: EOF means the parent process disappeared.
                    SafeFileHandle.CreateAnonymousPipe(out childLifetime, out parentLifetime);
                    start.InheritedHandles!.Add(childLifetime!);
                    start.Environment[WorkerLifetimeEnvironmentVariable] =
                        childLifetime!.DangerousGetHandle().ToInt64().ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    throw new PlatformNotSupportedException("PDFtoImage.Parallel requires Windows 10 / Windows Server 2016 or newer, Linux, or macOS.");
                }

                var process = SafeProcessHandle.Start(start);
                childLifetime?.Dispose();
                childLifetime = null;
                lifetime = parentLifetime;
                return process;
            }
            catch
            {
                childLifetime?.Dispose();
                parentLifetime?.Dispose();
                throw;
            }
        }
    }
}
