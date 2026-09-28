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

            string? startupHookAssemblyName = null;

            if (useStartupHook)
            {
                startupHookAssemblyName = typeof(StartupHook).Assembly.GetName().Name;

                if (string.IsNullOrWhiteSpace(startupHookAssemblyName))
                    throw new InvalidOperationException("The PDFtoImage.Parallel startup hook assembly name could not be determined.");
            }

            // Native AOT cannot use startup hooks; its eager module initializer enters the
            // worker bootstrap before the application's Main method instead. Single-file
            // CoreCLR applications likewise have no managed entry assembly location, so both
            // forms re-execute the application executable resolved from argv[0].
            if (!useStartupHook)
                return new WorkerLaunchCommand(GetApplicationExecutablePath(), startupHookAssemblyName, []);

            var entryAssemblyPath = GetManagedEntryAssemblyPath();

            if (string.IsNullOrWhiteSpace(entryAssemblyPath))
                return new WorkerLaunchCommand(GetApplicationExecutablePath(), startupHookAssemblyName, []);

            // Prefer the application's own apphost over the process currently hosting the
            // runtime. This is important for hosting models such as ASP.NET Core IIS in-process,
            // where the process executable is w3wp.exe rather than the application.
            var appHostPath = FindApplicationAppHost(entryAssemblyPath, AppContext.BaseDirectory);

            if (appHostPath != null)
                return new WorkerLaunchCommand(appHostPath, startupHookAssemblyName, []);

            // UseAppHost=false has no application executable to re-run. Resolve the framework
            // host through PATH instead of re-executing the process currently hosting CoreCLR.
            var command = new WorkerLaunchCommand("dotnet", startupHookAssemblyName, []);
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
            "Single-file applications return an empty Location and use argv[0] instead.")]
        private static string? GetManagedEntryAssemblyPath()
        {
            var entryAssemblyPath = Assembly.GetEntryAssembly()?.Location;

            if (!string.IsNullOrWhiteSpace(entryAssemblyPath))
                return entryAssemblyPath;

            // Native hosts can leave GetEntryAssembly() unset. In that case, recover the
            // application assembly from its deps file. Single-file applications normally have
            // no adjacent managed entry assembly and therefore continue to the argv[0] path.
            if (AppContext.GetData("APP_CONTEXT_DEPS_FILES") is not string depsFiles)
                return null;

            foreach (var depsFile in depsFiles.Split(Path.PathSeparator))
            {
                var candidate = GetManagedAssemblyPathFromDepsFile(depsFile);

                if (candidate != null && File.Exists(candidate))
                    return candidate;
            }

            return null;
        }

        private static string? GetManagedAssemblyPathFromDepsFile(string depsFile)
        {
            const string depsSuffix = ".deps.json";

            if (!depsFile.EndsWith(depsSuffix, StringComparison.OrdinalIgnoreCase))
                return null;

            return string.Concat(depsFile.AsSpan(0, depsFile.Length - depsSuffix.Length), ".dll");
        }

        internal static string? FindApplicationAppHost(string entryAssemblyPath, string baseDirectory)
        {
            var applicationName = Path.GetFileNameWithoutExtension(entryAssemblyPath);

            if (string.IsNullOrWhiteSpace(applicationName))
                return null;

            var appHostName = OperatingSystem.IsWindows()
                ? applicationName + ".exe"
                : applicationName;
            var appHostPath = Path.Combine(baseDirectory, appHostName);

            return File.Exists(appHostPath) ? appHostPath : null;
        }

        internal static string ResolveApplicationExecutablePath(string commandLineExecutable, string baseDirectory, string currentDirectory)
        {
            if (string.IsNullOrWhiteSpace(commandLineExecutable))
                throw new InvalidOperationException("The application executable could not be determined from the command line.");

            if (Path.IsPathFullyQualified(commandLineExecutable))
                return commandLineExecutable;

            var currentDirectoryCandidate = Path.GetFullPath(commandLineExecutable, currentDirectory);

            if (File.Exists(currentDirectoryCandidate))
                return currentDirectoryCandidate;

            var executableName = Path.GetFileName(commandLineExecutable);
            var baseDirectoryCandidate = Path.Combine(baseDirectory, executableName);

            if (File.Exists(baseDirectoryCandidate))
                return baseDirectoryCandidate;

            if (OperatingSystem.IsWindows() && string.IsNullOrEmpty(Path.GetExtension(executableName)))
            {
                var executableCandidate = baseDirectoryCandidate + ".exe";

                if (File.Exists(executableCandidate))
                    return executableCandidate;
            }

            throw new InvalidOperationException($"The application executable '{commandLineExecutable}' could not be resolved.");
        }

        private static string GetApplicationExecutablePath()
        {
            var arguments = Environment.GetCommandLineArgs();

            if (arguments.Length == 0)
                throw new InvalidOperationException("The application executable could not be determined from the command line.");

            return ResolveApplicationExecutablePath(arguments[0], AppContext.BaseDirectory, Environment.CurrentDirectory);
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
