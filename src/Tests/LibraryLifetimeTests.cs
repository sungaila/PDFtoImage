#if NET11_0_OR_GREATER
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Internals;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Tests
{
    [TestClass, DoNotParallelize]
    public sealed class LibraryLifetimeTests : TestBase
    {
        [TestMethod]
        public async Task FailedNativeInitializationDoesNotCrashFinalizerThread()
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext!.CancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(20));
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var start = new ProcessStartInfo(Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet")
            {
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            start.ArgumentList.Add(assemblyPath);
            start.Environment["DOTNET_STARTUP_HOOKS"] = assemblyPath;
            start.Environment[LibraryInitializationTestHook.EnvironmentVariable] = "1";
            using var process = Process.Start(start)!;
            var error = process.StandardError.ReadToEndAsync(timeout.Token);
            var output = process.StandardOutput.ReadToEndAsync(timeout.Token);
            try
            {
                await process.WaitForExitAsync(timeout.Token);
                await output;
                Assert.AreEqual(0, process.ExitCode, await error);
            }
            finally
            {
                if (!process.HasExited)
                    process.Kill(entireProcessTree: true);
            }
        }
    }

    internal static class LibraryInitializationTestHook
    {
        internal const string EnvironmentVariable = "PDFTOIMAGE_TEST_MISSING_NATIVE_LIBRARY";

        internal static void Initialize()
        {
            if (Environment.GetEnvironmentVariable(EnvironmentVariable) != "1")
                return;
            Environment.SetEnvironmentVariable(EnvironmentVariable, null);

            NativeLibrary.SetDllImportResolver(typeof(Conversion).Assembly, (name, _, _) =>
                name == "pdfium" ? throw new DllNotFoundException("Simulated missing PDFium.") : IntPtr.Zero);

            var failedAsExpected = TryInitialize();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            Environment.Exit(failedAsExpected ? 0 : 1);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool TryInitialize()
        {
            try
            {
                PdfLibrary.EnsureLoaded();
                return false;
            }
            catch (DllNotFoundException)
            {
                return true;
            }
        }
    }
}
#endif