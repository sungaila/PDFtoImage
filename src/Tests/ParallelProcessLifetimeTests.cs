#if NET8_0_OR_GREATER
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
    [TestClass, DoNotParallelize, OSCondition(OperatingSystems.Windows)]
    public sealed class ParallelProcessLifetimeTests : TestBase
    {
        [TestMethod]
        [DataRow(false, false)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        [DataRow(true, true)]
        public async Task ParentExitTerminatesItsWorkers(bool killParent, bool dotnetHost)
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext!.CancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(20));
            var assemblyPath = Assembly.GetExecutingAssembly().Location;
            var pipeName = "PDFtoImage.Tests." + Guid.NewGuid().ToString("N");
            using var pipe = new NamedPipeServerStream(pipeName, PipeDirection.InOut, 1, PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
            var start = new ProcessStartInfo
            {
                FileName = dotnetHost ? Environment.GetEnvironmentVariable("DOTNET_HOST_PATH") ?? "dotnet" : Path.ChangeExtension(assemblyPath, ".exe"),
                UseShellExecute = false,
                CreateNoWindow = true
            };
            if (dotnetHost)
                start.ArgumentList.Add(assemblyPath);
            start.Environment["DOTNET_STARTUP_HOOKS"] = assemblyPath;
            start.Environment["PDFTOIMAGE_TEST_PARENT_PIPE"] = pipeName;
            using var parent = Process.Start(start)!;
            Process[] workers = [];
            try
            {
                await pipe.WaitForConnectionAsync(timeout.Token);
                using var reader = new StreamReader(pipe, leaveOpen: true);
                using var writer = new StreamWriter(pipe, leaveOpen: true) { AutoFlush = true };
                var ids = await reader.ReadLineAsync(timeout.Token);
                Assert.IsNotNull(ids);
                workers = ids.Split(',').Select(int.Parse).Select(Process.GetProcessById).ToArray();
                Assert.HasCount(2, workers);

                if (killParent)
                    parent.Kill(); // Deliberately NOT Kill(entireProcessTree: true).
                else
                    await writer.WriteLineAsync("dispose");
                await parent.WaitForExitAsync(timeout.Token);
                foreach (var worker in workers)
                {
                    await worker.WaitForExitAsync(timeout.Token);
                    Assert.IsTrue(worker.HasExited);
                }
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
    }
}
#endif