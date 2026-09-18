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
    [SupportedOSPlatform("windows10.0")]
    internal static class WorkerProcessLauncherWindows
    {
        internal const string WorkerPipeEnvironmentVariable = "PDFTOIMAGE_PARALLEL_WORKER_PIPE";

        internal static unsafe Process StartSuspendedAndAssign(WindowsJob job, string pipeName)
        {
            if (AppContext.TryGetSwitch("System.StartupHookProvider.IsSupported", out var hooksSupported) && !hooksSupported)
                throw new PlatformNotSupportedException("PDFtoImage.Parallel requires enabled .NET startup hooks.");

            var processPath = Environment.ProcessPath;

            if (string.IsNullOrWhiteSpace(processPath))
                throw new InvalidOperationException("The current process executable could not be determined.");

            var startupHookPath = typeof(StartupHook).Assembly.Location;

            if (string.IsNullOrWhiteSpace(startupHookPath))
                throw new PlatformNotSupportedException("Single-file applications are not supported by PDFtoImage.Parallel workers.");

            var commandLine = CreateCommandLine(processPath);
            var commandLineBuffer = (commandLine + '\0').ToCharArray();
            var commandLineSpan = commandLineBuffer.AsSpan();
            var environment = CreateEnvironmentBlock(startupHookPath, pipeName);
            var startupInfo = new STARTUPINFOEXW();

            startupInfo.StartupInfo.cb = (uint)sizeof(STARTUPINFOEXW);

            nuint attributeSize = 0;

            // This first call intentionally queries the buffer size required for the attribute list.
            ParallelPInvoke.InitializeProcThreadAttributeList(default, 1, 0, &attributeSize);

            var attributeMemory = Marshal.AllocHGlobal(checked((IntPtr)(long)attributeSize));
            var initialized = false;
            var jobReference = false;

            try
            {
                startupInfo.lpAttributeList = new LPPROC_THREAD_ATTRIBUTE_LIST((void*)attributeMemory);

                if (!ParallelPInvoke.InitializeProcThreadAttributeList(startupInfo.lpAttributeList, 1, 0, &attributeSize))
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                initialized = true;
                job.Handle.DangerousAddRef(ref jobReference);

                var jobHandle = job.Handle.DangerousGetHandle();

                if (!ParallelPInvoke.UpdateProcThreadAttribute(startupInfo.lpAttributeList, 0,
                    ParallelPInvoke.PROC_THREAD_ATTRIBUTE_JOB_LIST, &jobHandle, (nuint)sizeof(IntPtr), null, null))
                    throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not configure atomic worker job assignment.");

                fixed (char* environmentPointer = environment)
                {
                    if (!ParallelPInvoke.CreateProcess(
                        processPath,
                        ref commandLineSpan,
                        null,
                        null,
                        false,
                        PROCESS_CREATION_FLAGS.CREATE_SUSPENDED |
                        PROCESS_CREATION_FLAGS.EXTENDED_STARTUPINFO_PRESENT |
                        PROCESS_CREATION_FLAGS.CREATE_NO_WINDOW |
                        PROCESS_CREATION_FLAGS.CREATE_UNICODE_ENVIRONMENT,
                        environmentPointer,
                        Environment.CurrentDirectory,
                        in startupInfo.StartupInfo,
                        out var processInformation))
                    {
                        throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not create a PDF conversion worker process.");
                    }

                    using var processHandle = new SafeProcessHandle((IntPtr)processInformation.hProcess, true);
                    using var threadHandle = new SafeFileHandle((IntPtr)processInformation.hThread, true);
                    Process? process = null;

                    try
                    {
                        process = Process.GetProcessById(checked((int)processInformation.dwProcessId));

                        if (ParallelPInvoke.ResumeThread(threadHandle) == uint.MaxValue)
                        {
                            var error = Marshal.GetLastWin32Error();

                            throw new Win32Exception(error, "Could not resume the PDF conversion worker process.");
                        }

                        return process;
                    }
                    catch
                    {
                        ParallelPInvoke.TerminateProcess(processHandle, 1);
                        process?.Dispose();
                        throw;
                    }
                }
            }
            finally
            {
                if (initialized)
                    ParallelPInvoke.DeleteProcThreadAttributeList(startupInfo.lpAttributeList);

                Marshal.FreeHGlobal(attributeMemory);

                if (jobReference)
                    job.Handle.DangerousRelease();
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

            if (!isDotnetHost)
                return commandLine.ToString();

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

            var runtimeConfig = string.Concat(depsFile.AsSpan(0, depsFile.Length - depsSuffix.Length), ".runtimeconfig.json");

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