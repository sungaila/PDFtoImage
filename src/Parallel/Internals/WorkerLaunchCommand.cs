using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace PDFtoImage.Parallel.Internals
{
    internal sealed record WorkerLaunchCommand(string ProcessPath, string? StartupHookAssemblyName, List<string> Arguments)
    {
        [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(StartupHook))]
        internal static WorkerLaunchCommand Create()
        {
            var useStartupHook = RuntimeFeature.IsDynamicCodeSupported;

            if (useStartupHook && AppContext.TryGetSwitch("System.StartupHookProvider.IsSupported", out var hooksSupported) && !hooksSupported)
                throw new PlatformNotSupportedException("PDFtoImage.Parallel requires enabled .NET startup hooks when running on CoreCLR.");

            var processPath = Environment.ProcessPath;
            if (string.IsNullOrWhiteSpace(processPath))
                throw new InvalidOperationException("The current process executable could not be determined.");

            string? startupHookAssemblyName = null;

            if (useStartupHook)
            {
                startupHookAssemblyName = typeof(StartupHook).Assembly.GetName().Name;

                if (string.IsNullOrWhiteSpace(startupHookAssemblyName))
                    throw new InvalidOperationException("The PDFtoImage.Parallel startup hook assembly name could not be determined.");
            }

            // CoreCLR loads the worker bootstrap through DOTNET_STARTUP_HOOKS. Native AOT
            // cannot use startup hooks; its eager module initializer enters the same bootstrap
            // before the application's Main method instead.
            var command = new WorkerLaunchCommand(processPath, startupHookAssemblyName, []);

            if (!string.Equals(Path.GetFileNameWithoutExtension(processPath), "dotnet", StringComparison.OrdinalIgnoreCase))
                return command;

            var entryAssemblyPath = GetManagedEntryAssemblyPath();

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


        [UnconditionalSuppressMessage("SingleFile", "IL3000", Justification =
            "This path is only used when Environment.ProcessPath is the dotnet host. Single-file applications use their apphost and return before reaching it.")]
        private static string? GetManagedEntryAssemblyPath() => Assembly.GetEntryAssembly()?.Location;

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