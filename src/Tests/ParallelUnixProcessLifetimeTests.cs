#if NET11_0_OR_GREATER
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Parallel.Internals;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Tests
{
    [TestClass, DoNotParallelize, OSCondition(OperatingSystems.Linux | OperatingSystems.OSX)]
    public sealed class ParallelUnixProcessLifetimeTests : TestBase
    {
        private static ProcessStartInfo StartInfo(bool dotnetHost)
        {
            var assembly = Assembly.GetExecutingAssembly().Location;
            var start = new ProcessStartInfo(dotnetHost ? Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet" : Path.ChangeExtension(assembly, null))
            {
                UseShellExecute = false
            };
            if (dotnetHost)
                start.ArgumentList.Add(assembly);
            return start;
        }

        [TestMethod]
        [DataRow(false, false)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        [DataRow(true, true)]
        public async Task ParentExitDuringParallelRenderingTerminatesWorkers(bool killParent, bool dotnetHost)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext!.CancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(60));
            var pipeName = Guid.NewGuid().ToString("N");
            using var pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
            var start = StartInfo(dotnetHost);
            start.Environment["DOTNET_STARTUP_HOOKS"] = Assembly.GetExecutingAssembly().Location;
            start.Environment[ParallelUnixProcessTestHook.ParentPipeVariable] = pipeName;
            using var parent = Process.Start(start)!;
            Process[] workers = [];
            try
            {
                await pipe.WaitForConnectionAsync(timeout.Token);
                using var reader = new StreamReader(pipe, leaveOpen: true);
                using var writer = new StreamWriter(pipe, leaveOpen: true) { AutoFlush = true };
                var ids = await reader.ReadLineAsync(timeout.Token);
                Assert.IsNotNull(ids);
                workers = [.. ids.Split(',').Select(int.Parse).Select(Process.GetProcessById)];
                Assert.HasCount(2, workers);
                if (killParent)
                    parent.Kill(); // Only the parent. Worker parent-death handling must do the rest.
                else
                    await writer.WriteLineAsync("dispose");
                await parent.WaitForExitAsync(timeout.Token);
                foreach (var worker in workers)
                    await worker.WaitForExitAsync(timeout.Token).WaitAsync(TimeSpan.FromSeconds(10), timeout.Token);
                if (!killParent)
                    Assert.AreEqual(0, parent.ExitCode);
            }
            finally
            {
                if (!parent.HasExited)
                    parent.Kill(entireProcessTree: true);
                foreach (var worker in workers)
                {
                    if (!worker.HasExited)
                        worker.Kill();
                    worker.Dispose();
                }
            }
        }

        [TestMethod]
        public async Task MacLifetimeDisconnectTerminatesWorkerWithOpenCommandConnection()
        {
            if (!OperatingSystem.IsMacOS())
            {
                Assert.Inconclusive("The explicit inherited lifetime pipe is only required on macOS.");
                return;
            }

            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext!.CancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(15));
            var pipeName = Guid.NewGuid().ToString("N");
            using var pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
            using var worker = WorkerProcessLauncher.Start(pipeName, out var lifetime);
            using var lifetimeHandle = lifetime ?? throw new InvalidOperationException("macOS workers require a parent lifetime handle.");
            await pipe.WaitForConnectionAsync(timeout.Token);
            await WorkerConnection.ReadHelloAsync(pipe, timeout.Token, timeout.Token);
            lifetimeHandle.Dispose();
            await worker.WaitForExitAsync(timeout.Token);
        }
    }
}
#endif
