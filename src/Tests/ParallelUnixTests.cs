#if NET9_0_OR_GREATER
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Parallel;
using PDFtoImage.Parallel.Internals;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using static PDFtoImage.Tests.TestUtils;

namespace PDFtoImage.Tests
{
    [TestClass, DoNotParallelize, OSCondition(OperatingSystems.Linux | OperatingSystems.OSX)]
    public sealed class ParallelUnixTests : TestBase
    {
        private static byte[] Pdf(string name = "Wikimedia_Commons_web.pdf") =>
            File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "..", "Assets", name));

        [TestMethod]
        public async Task ConcurrentDocumentsAndOrderedPagesMatchSerialRendering()
        {
            await using var processor = new ParallelPdfProcessor(2);
            var options = new RenderOptions(Dpi: 40);
            async Task Render(string name)
            {
                var pdf = Pdf(name);
                int[] pages = [1, 0, 1];
                var index = 0;
                await foreach (var bitmap in processor.ToImagesAsync(new MemoryStream(pdf), pages, options: options, cancellationToken: TestContext!.CancellationToken))
                {
                    using (bitmap)
                    using (var expected = Conversion.ToImage(pdf, pages[index++], options: options))
                        AssertBitmapsEqual(expected, bitmap);
                }
                Assert.AreEqual(pages.Length, index);
            }

            await Task.WhenAll(Render("Wikimedia_Commons_web.pdf"), Render("hundesteuer-anmeldung.pdf"));
            Assert.HasCount(2, processor.WorkerProcessIds);
            Assert.IsTrue(processor.WorkerDocumentIds.All(id => id == null));
        }

        [TestMethod]
        public async Task WorkerCrashIsReportedAndReplacementIsLazy()
        {
            await using var processor = new ParallelPdfProcessor(1);
            using var image = await processor.ToImageAsync(new MemoryStream(Pdf()), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken);
            using var process = Process.GetProcessById(processor.WorkerProcessIds.Single());
            process.Kill();
            await process.WaitForExitAsync(TestContext.CancellationToken);
            var error = await Assert.ThrowsExactlyAsync<ParallelConversionException>(() =>
                processor.ToImageAsync(new MemoryStream(Pdf()), cancellationToken: TestContext.CancellationToken));
            Assert.AreEqual("WorkerProcessTerminated", error.RemoteExceptionType);
            Assert.IsEmpty(processor.WorkerProcessIds);
            using var replacement = await processor.ToImageAsync(new MemoryStream(Pdf()), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext.CancellationToken);
            AssertBitmapsEqual(image, replacement);
            Assert.AreNotEqual(process.Id, processor.WorkerProcessIds.Single());
        }

        [TestMethod]
        public async Task CancellationAndDisposalInterruptActiveRendering()
        {
            await using var pool = new WorkerPoolUnix(2);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext!.CancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(30));
            var request = new PdfRequest(ParallelUnixProcessTestHook.SlowPdf(), null);
            await Task.WhenAll(pool.GetPageCountAsync(request, timeout.Token), pool.GetPageCountAsync(request, timeout.Token));
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token);
            var cancelled = pool.RenderPageAsync(request, 0, new RenderOptions(Dpi: 72), cancellation.Token);
            var independent = pool.RenderPageAsync(request, 0, new RenderOptions(Dpi: 72), timeout.Token);
            var workers = pool.WorkerProcessIds.Select(Process.GetProcessById).ToArray();
            try
            {
                await ParallelUnixProcessTestHook.WaitForRenderingAsync(workers, [cancelled, independent], timeout.Token);
                cancellation.Cancel();
                await Assert.ThrowsAsync<OperationCanceledException>(() => cancelled);
                Assert.IsFalse(independent.IsCompleted, "Cancelling one lease must not terminate another worker.");
                await pool.DisposeAsync();
                await Assert.ThrowsAsync<OperationCanceledException>(() => independent);
                Assert.IsTrue(workers.All(worker => worker.HasExited));
                Assert.IsFalse(Directory.Exists(pool.SocketDirectory));
            }
            finally
            {
                foreach (var worker in workers)
                    worker.Dispose();
            }
        }

        [TestMethod]
        public async Task ProcessorDirectoriesArePrivateIndependentAndRemovedAfterDisposal()
        {
            await using var first = new WorkerPoolUnix(1);
            await using var second = new WorkerPoolUnix(1);
            Assert.AreNotEqual(first.SocketDirectory, second.SocketDirectory);
            Assert.AreEqual(UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute, File.GetUnixFileMode(first.SocketDirectory));
            var request = new PdfRequest(Pdf(), null);
            await Task.WhenAll(first.GetPageCountAsync(request, TestContext!.CancellationToken), second.GetPageCountAsync(request, TestContext.CancellationToken));
            Assert.AreNotEqual(first.WorkerProcessIds.Single(), second.WorkerProcessIds.Single());
            Assert.IsEmpty(Directory.GetFileSystemEntries(first.SocketDirectory), "Listener paths should be unlinked after startup.");
            await first.DisposeAsync();
            using var bitmap = await second.RenderPageAsync(request, 0, new RenderOptions(Dpi: 40), TestContext.CancellationToken);
            Assert.IsFalse(Directory.Exists(first.SocketDirectory));
            Assert.IsTrue(Directory.Exists(second.SocketDirectory));
            await second.DisposeAsync();
            Assert.IsFalse(Directory.Exists(second.SocketDirectory));
        }

        [TestMethod]
        public async Task UnixSocketWithoutHelloTimesOut()
        {
            await using var pool = new WorkerPoolUnix(1);
            var path = Path.Combine(pool.SocketDirectory, "hello");
            using var listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            using var client = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            try
            {
                listener.Bind(new UnixDomainSocketEndPoint(path));
                listener.Listen(1);
                await client.ConnectAsync(new UnixDomainSocketEndPoint(path), TestContext!.CancellationToken);
                using var connection = await listener.AcceptAsync(TestContext.CancellationToken);
                using var stream = new NetworkStream(connection);
                using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
                await Assert.ThrowsExactlyAsync<TimeoutException>(() => WorkerConnection.ReadHelloAsync(stream, TestContext.CancellationToken, timeout.Token));
            }
            finally
            {
                listener.Dispose();
                File.Delete(path);
            }
        }

        [TestMethod]
        public async Task DisposalDuringStartupDrainsAttemptsAndRemovesDirectory()
        {
            await using var pool = new WorkerPoolUnix(4);
            var request = new PdfRequest(Pdf(), null);
            var pending = Enumerable.Range(0, 4).Select(_ => pool.GetPageCountAsync(request, TestContext!.CancellationToken)).ToArray();
            await Task.WhenAll(Task.Run(pool.Dispose, TestContext!.CancellationToken), pool.DisposeAsync().AsTask());
            try { await Task.WhenAll(pending); }
            catch (OperationCanceledException) { }
            Assert.IsTrue(pending.All(task => task.IsCompleted));
            Assert.IsEmpty(pool.WorkerProcessIds);
            Assert.IsFalse(Directory.Exists(pool.SocketDirectory));
        }

        [TestMethod]
        public async Task FailedAndCancelledStartupLeaveNoSocketFiles()
        {
            await using var pool = new WorkerPoolUnix(1);
            // Binding fails before any worker can be launched.
            var invalidPath = Path.Combine(pool.SocketDirectory, "missing", "command");
            await Assert.ThrowsAsync<SocketException>(() => WorkerConnectionUnix.StartAsync(invalidPath, Path.Combine(pool.SocketDirectory, "life"), TestContext!.CancellationToken));
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => WorkerConnectionUnix.StartAsync(Path.Combine(pool.SocketDirectory, "command"), Path.Combine(pool.SocketDirectory, "life"), cancelled.Token));
            Assert.IsEmpty(Directory.GetFileSystemEntries(pool.SocketDirectory));
        }
    }
}
#endif