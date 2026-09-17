using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;
using Windows.Win32;
using Windows.Win32.System.Threading;

namespace PDFtoImage.Parallel.Internals
{
    [SupportedOSPlatform("windows6.2")]
    internal static class WorkerProcessLauncher
    {
        internal const string WorkerPipeEnvironmentVariable = "PDFTOIMAGE_PARALLEL_WORKER_PIPE";

        internal static unsafe Process StartSuspendedAndAssign(WindowsJob job, string pipeName)
        {
            var processPath = Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrWhiteSpace(processPath))
                throw new InvalidOperationException("The current process executable could not be determined.");

            var startupHookPath = typeof(StartupHook).Assembly.Location;
            if (string.IsNullOrWhiteSpace(startupHookPath))
                throw new PlatformNotSupportedException("Single-file applications are not supported by PDFtoImage.Parallel workers.");

            var commandLine = CreateCommandLine(processPath);
            var commandLineBuffer = (commandLine + '\0').ToCharArray();
            var commandLineSpan = commandLineBuffer.AsSpan();
            var environment = CreateEnvironmentBlock(startupHookPath, pipeName);
            var startupInfo = new STARTUPINFOW { cb = (uint)sizeof(STARTUPINFOW) };

            fixed (char* environmentPointer = environment)
            {
                if (!ParallelPInvoke.CreateProcess(
                    processPath,
                    ref commandLineSpan,
                    null,
                    null,
                    false,
                    PROCESS_CREATION_FLAGS.CREATE_SUSPENDED |
                    PROCESS_CREATION_FLAGS.CREATE_NO_WINDOW |
                    PROCESS_CREATION_FLAGS.CREATE_UNICODE_ENVIRONMENT,
                    environmentPointer,
                    Environment.CurrentDirectory,
                    startupInfo,
                    out var processInformation))
                {
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not create a PDF conversion worker process.");
                }

                using var processHandle = new SafeFileHandle((IntPtr)processInformation.hProcess, true);
                using var threadHandle = new SafeFileHandle((IntPtr)processInformation.hThread, true);
                var process = Process.GetProcessById(checked((int)processInformation.dwProcessId));

                try
                {
                    if (!ParallelPInvoke.AssignProcessToJobObject(job.Handle, processHandle))
                    {
                        var error = Marshal.GetLastWin32Error();
                        ParallelPInvoke.TerminateProcess(processHandle, 1);
                        throw new Win32Exception(error, "Could not assign the PDF conversion worker to its Windows job object.");
                    }

                    if (ParallelPInvoke.ResumeThread(threadHandle) == uint.MaxValue)
                    {
                        var error = Marshal.GetLastWin32Error();
                        ParallelPInvoke.TerminateProcess(processHandle, 1);
                        throw new Win32Exception(error, "Could not resume the PDF conversion worker process.");
                    }

                    return process;
                }
                catch
                {
                    process.Dispose();
                    throw;
                }
            }
        }

        private static char[] CreateEnvironmentBlock(string startupHookPath, string pipeName)
        {
            var variables = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (DictionaryEntry entry in Environment.GetEnvironmentVariables())
            {
                if (entry.Key is string name && entry.Value is string value)
                    variables[name] = value;
            }

            variables.TryGetValue("DOTNET_STARTUP_HOOKS", out var existingStartupHooks);
            variables["DOTNET_STARTUP_HOOKS"] = string.IsNullOrEmpty(existingStartupHooks)
                ? startupHookPath
                : startupHookPath + Path.PathSeparator + existingStartupHooks;
            variables[WorkerPipeEnvironmentVariable] = pipeName;

            var block = new StringBuilder();
            foreach (var variable in variables)
            {
                block.Append(variable.Key);
                block.Append('=');
                block.Append(variable.Value);
                block.Append('\0');
            }

            block.Append('\0');
            return block.ToString().ToCharArray();
        }

        private static string CreateCommandLine(string processPath)
        {
            var commandLine = new StringBuilder();
            AppendArgument(commandLine, processPath);

            var entryAssemblyPath = Assembly.GetEntryAssembly()?.Location;
            var isDotnetHost = string.Equals(Path.GetFileNameWithoutExtension(processPath), "dotnet", StringComparison.OrdinalIgnoreCase);
            if (isDotnetHost)
                AppendArgument(commandLine, "exec");

            var depsFile = FindApplicationDepsFile(entryAssemblyPath);
            var runtimeConfig = GetRuntimeConfigFile(depsFile);
            if (runtimeConfig != null)
            {
                AppendArgument(commandLine, "--runtimeconfig");
                AppendArgument(commandLine, runtimeConfig);
            }

            if (depsFile != null)
            {
                AppendArgument(commandLine, "--depsfile");
                AppendArgument(commandLine, depsFile);
            }

            if (isDotnetHost)
            {
                if (string.IsNullOrWhiteSpace(entryAssemblyPath))
                    throw new InvalidOperationException("The managed entry assembly could not be determined.");

                AppendArgument(commandLine, entryAssemblyPath);
            }

            var arguments = Environment.GetCommandLineArgs();
            for (var index = 1; index < arguments.Length; index++)
                AppendArgument(commandLine, arguments[index]);

            return commandLine.ToString();
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

            var runtimeConfig = depsFile.Substring(0, depsFile.Length - depsSuffix.Length) + ".runtimeconfig.json";
            return File.Exists(runtimeConfig) ? runtimeConfig : null;
        }

        private static void AppendArgument(StringBuilder commandLine, string argument)
        {
            if (commandLine.Length > 0)
                commandLine.Append(' ');

            commandLine.Append(QuoteArgument(argument));
        }

        private static string QuoteArgument(string argument)
        {
            var quoted = new StringBuilder(argument.Length + 2);
            quoted.Append('"');
            var backslashCount = 0;

            foreach (var character in argument)
            {
                if (character == '\\')
                {
                    backslashCount++;
                    continue;
                }

                if (character == '"')
                {
                    quoted.Append('\\', backslashCount * 2 + 1);
                    quoted.Append('"');
                    backslashCount = 0;
                    continue;
                }

                quoted.Append('\\', backslashCount);
                quoted.Append(character);
                backslashCount = 0;
            }

            quoted.Append('\\', backslashCount * 2);
            quoted.Append('"');
            return quoted.ToString();
        }
    }
}
