using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace PDFtoImage.Parallel.Internals
{
    internal sealed record WorkerLaunchCommand(string ProcessPath, string StartupHookPath, List<string> Arguments)
    {
        internal static WorkerLaunchCommand Create()
        {
            if (AppContext.TryGetSwitch("System.StartupHookProvider.IsSupported", out var hooksSupported) && !hooksSupported)
                throw new PlatformNotSupportedException("PDFtoImage.Parallel requires enabled .NET startup hooks.");

            var processPath = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(processPath))
                throw new InvalidOperationException("The current process executable could not be determined.");

            var startupHookPath = typeof(StartupHook).Assembly.Location;
            if (string.IsNullOrWhiteSpace(startupHookPath))
                throw new PlatformNotSupportedException("Single-file applications are not supported by PDFtoImage.Parallel workers.");

            var command = new WorkerLaunchCommand(processPath, startupHookPath, []);
            if (!string.Equals(Path.GetFileNameWithoutExtension(processPath), "dotnet", StringComparison.OrdinalIgnoreCase))
                return command;

            var entryAssemblyPath = Assembly.GetEntryAssembly()?.Location;
            if (string.IsNullOrWhiteSpace(entryAssemblyPath))
                throw new InvalidOperationException("The managed entry assembly could not be determined.");

            command.Arguments.Add("exec");
            var depsFile = FindApplicationDepsFile(entryAssemblyPath);
            var runtimeConfig = GetRuntimeConfigFile(depsFile);
            if (runtimeConfig != null)
                command.Arguments.AddRange(["--runtimeconfig", runtimeConfig]);
            if (depsFile != null)
                command.Arguments.AddRange(["--depsfile", depsFile]);
            command.Arguments.Add(entryAssemblyPath);
            return command;
        }

        private static string? FindApplicationDepsFile(string? entryAssemblyPath)
        {
            if (!string.IsNullOrWhiteSpace(entryAssemblyPath))
            {
                var adjacentDepsFile = Path.ChangeExtension(entryAssemblyPath, ".deps.json");

                if (File.Exists(adjacentDepsFile))
                    return adjacentDepsFile;
            }

            if (AppContext.GetData("APP_CONTEXT_DEPS_FILES") is not string depsFiles)
                return null;

            foreach (var depsFile in depsFiles.Split(Path.PathSeparator))
            {
                if (File.Exists(depsFile) && !string.Equals(Path.GetFileName(depsFile), "Microsoft.NETCore.App.deps.json", StringComparison.OrdinalIgnoreCase))
                    return depsFile;
            }

            return null;
        }

        private static string? GetRuntimeConfigFile(string? depsFile)
        {
            const string depsSuffix = ".deps.json";

            if (depsFile == null || !depsFile.EndsWith(depsSuffix, StringComparison.OrdinalIgnoreCase))
                return null;

            var runtimeConfig = string.Concat(depsFile.AsSpan(0, depsFile.Length - depsSuffix.Length), ".runtimeconfig.json");

            return File.Exists(runtimeConfig) ? runtimeConfig : null;
        }
    }
}