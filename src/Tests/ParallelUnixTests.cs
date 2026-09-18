#if NET11_0_OR_GREATER
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Parallel;
using PDFtoImage.Parallel.Internals;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static PDFtoImage.Tests.TestUtils;

namespace PDFtoImage.Tests
{
    [TestClass, DoNotParallelize, OSCondition(OperatingSystems.Linux | OperatingSystems.OSX)]
    public sealed class ParallelUnixTests : TestBase
    {
        private static readonly byte[] Pdf = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "Wikimedia_Commons_web.pdf"));

        private static readonly byte[] OtherPdf = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "hundesteuer-anmeldung.pdf"));

        [TestMethod]
        public async Task ConcurrentDocumentsAndOrderedPagesMatchSerialRendering()
        {
            await using var processor = new ParallelPdfProcessor(2);
            var options = new RenderOptions(Dpi: 40);
            async Task Render(byte[] pdf)
            {
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

            // Repeat the overlap so request cleanup repeatedly races with workers becoming
            // idle. A free slot and its semaphore permit must be published atomically.
            for (var iteration = 0; iteration < 8; iteration++)
            {
                await Task.WhenAll(Render(Pdf), Render(OtherPdf));
                Assert.HasCount(2, processor.WorkerProcessIds);
                Assert.IsTrue(processor.WorkerDocumentIds.All(id => id == null),
                    $"A worker retained a document after concurrent cleanup in iteration {iteration}.");
            }
        }

        [TestMethod]
        public async Task WorkerCrashIsReportedAndReplacementIsLazy()
        {
            await using var processor = new ParallelPdfProcessor(1);
            using var image = await processor.ToImageAsync(new MemoryStream(Pdf), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken);
            using var process = Process.GetProcessById(processor.WorkerProcessIds.Single());
            process.Kill();
            await process.WaitForExitAsync(TestContext.CancellationToken);
            var error = await Assert.ThrowsExactlyAsync<ParallelConversionException>(() =>
                processor.ToImageAsync(new MemoryStream(Pdf), cancellationToken: TestContext.CancellationToken));
            Assert.AreEqual("WorkerProcessTerminated", error.RemoteExceptionType);
            Assert.IsEmpty(processor.WorkerProcessIds);
            using var replacement = await processor.ToImageAsync(new MemoryStream(Pdf), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext.CancellationToken);
            AssertBitmapsEqual(image, replacement);
            Assert.AreNotEqual(process.Id, processor.WorkerProcessIds.Single());
        }

        [TestMethod]
        public async Task CancellationAndDisposalInterruptActiveRendering()
        {
            await using var pool = new WorkerPool(2);
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
            }
            finally
            {
                foreach (var worker in workers)
                    worker.Dispose();
            }
        }

        [TestMethod]
        public async Task ProcessorInstancesRemainIndependent()
        {
            await using var first = new ParallelPdfProcessor(1);
            await using var second = new ParallelPdfProcessor(1);
            using var firstImage = await first.ToImageAsync(new MemoryStream(Pdf), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken);
            using var secondImage = await second.ToImageAsync(new MemoryStream(Pdf), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext.CancellationToken);
            AssertBitmapsEqual(firstImage, secondImage);
            Assert.AreNotEqual(first.WorkerProcessIds.Single(), second.WorkerProcessIds.Single());

            await first.DisposeAsync();
            using var stillWorking = await second.ToImageAsync(new MemoryStream(OtherPdf), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext.CancellationToken);
            using var expected = Conversion.ToImage(OtherPdf, options: new RenderOptions(Dpi: 40));
            AssertBitmapsEqual(expected, stillWorking);
        }

        [TestMethod]
        public async Task DisposalDuringStartupDrainsAttempts()
        {
            await using var pool = new WorkerPool(4);
            var request = new PdfRequest(Pdf, null);
            var pending = Enumerable.Range(0, 4).Select(_ => pool.GetPageCountAsync(request, TestContext!.CancellationToken)).ToArray();
            await Task.WhenAll(Task.Run(pool.Dispose, TestContext!.CancellationToken), pool.DisposeAsync().AsTask());
            try { await Task.WhenAll(pending); }
            catch (OperationCanceledException) { }
            Assert.IsTrue(pending.All(task => task.IsCompleted));
            Assert.IsEmpty(pool.WorkerProcessIds);
        }

        [TestMethod]
        public async Task CancelledStartupDoesNotCreateReusableWorker()
        {
            using var cancelled = new CancellationTokenSource();
            cancelled.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => WorkerConnection.StartAsync(cancelled.Token));
        }
    }
}
#endif
