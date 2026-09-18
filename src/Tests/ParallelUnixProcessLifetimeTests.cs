#if NET9_0_OR_GREATER
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Parallel.Internals;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
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
            var pipeName = "pti-test-" + Guid.NewGuid().ToString("N");
            using var pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
            var start = StartInfo(dotnetHost);
            start.Environment["DOTNET_STARTUP_HOOKS"] = Assembly.GetExecutingAssembly().Location;
            start.Environment[ParallelUnixProcessTestHook.ParentPipeVariable] = pipeName;
            using var parent = Process.Start(start)!;
            Process[] workers = [];
            string? socketDirectory = null;
            try
            {
                await pipe.WaitForConnectionAsync(timeout.Token);
                using var reader = new StreamReader(pipe, leaveOpen: true);
                using var writer = new StreamWriter(pipe, leaveOpen: true) { AutoFlush = true };
                var ids = await reader.ReadLineAsync(timeout.Token);
                Assert.IsNotNull(ids);
                workers = [.. ids.Split(',').Select(int.Parse).Select(Process.GetProcessById)];
                socketDirectory = await reader.ReadLineAsync(timeout.Token);
                Assert.IsNotNull(socketDirectory);
                Assert.HasCount(2, workers);
                if (killParent)
                    parent.Kill(); // Only the parent. The workers must detect EOF themselves.
                else
                    await writer.WriteLineAsync("dispose");
                await parent.WaitForExitAsync(timeout.Token);
                foreach (var worker in workers)
                    await worker.WaitForExitAsync(timeout.Token).WaitAsync(TimeSpan.FromSeconds(10), timeout.Token);
                if (!killParent)
                {
                    Assert.AreEqual(0, parent.ExitCode);
                    Assert.IsFalse(Directory.Exists(socketDirectory));
                }
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
                // SIGKILL cannot run the parent's directory cleanup. No listener
                // path remains after startup; remove its empty directory here.
                if (socketDirectory != null && Directory.Exists(socketDirectory))
                    Directory.Delete(socketDirectory);
            }
        }

        [TestMethod]
        public async Task LifetimeDisconnectTerminatesWorkerWithOpenCommandConnection()
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext!.CancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(15));
            await using var pool = new WorkerPoolUnix(1);
            var lifetimePath = Path.Combine(pool.SocketDirectory, "l");
            var commandPath = Path.Combine(pool.SocketDirectory, "c");
            using var listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            using var commandListener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            listener.Bind(new UnixDomainSocketEndPoint(lifetimePath));
            listener.Listen(1);
            commandListener.Bind(new UnixDomainSocketEndPoint(commandPath));
            commandListener.Listen(1);
            var start = StartInfo(true);
            start.Environment["DOTNET_STARTUP_HOOKS"] = typeof(WorkerPoolUnix).Assembly.Location;
            start.Environment[WorkerProcessLauncherUnix.WorkerSocketEnvironmentVariable] = commandPath;
            start.Environment[WorkerProcessLauncherUnix.WorkerLifetimeEnvironmentVariable] = lifetimePath;
            using var worker = Process.Start(start)!;
            try
            {
                using var lifetime = await listener.AcceptAsync(timeout.Token);
                using var command = await commandListener.AcceptAsync(timeout.Token);
                using var stream = new NetworkStream(command);
                await WorkerConnection.ReadHelloAsync(stream, timeout.Token, timeout.Token);
                lifetime.Dispose();
                await worker.WaitForExitAsync(timeout.Token);
            }
            finally
            {
                if (!worker.HasExited)
                    worker.Kill();
                listener.Dispose();
                commandListener.Dispose();
                File.Delete(lifetimePath);
                File.Delete(commandPath);
            }
        }
    }
}
#endif