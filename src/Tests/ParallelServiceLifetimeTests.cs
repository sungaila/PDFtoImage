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
    [TestClass, DoNotParallelize]
    public sealed class ParallelServiceLifetimeTests : TestBase
    {
        private static readonly byte[] Pdf = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "SocialPreview.pdf"));

        private sealed class TestDirectory : IDisposable
        {
            private readonly string _root = Path.Combine(Path.GetTempPath(), "PDFtoImage.Tests." + Guid.NewGuid().ToString("N"));
            internal string PathName => Path.Combine(_root, "nested");

            internal TestDirectory() => Directory.CreateDirectory(PathName);

            public void Dispose()
            {
                Directory.Delete(PathName);
                Directory.Delete(_root);
            }
        }

        private sealed class BlockedStartupPool() : WorkerPool(4, 2)
        {
            internal readonly TaskCompletionSource TwoStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
            internal readonly TaskCompletionSource ThreeStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
            internal int Started;

            protected override async Task<WorkerConnection> StartWorkerAsync(CancellationToken cancellationToken)
            {
                var count = Interlocked.Increment(ref Started);
                if (count == 2)
                    TwoStarted.TrySetResult();
                if (count == 3)
                    ThreeStarted.TrySetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                throw new InvalidOperationException("Startup must be cancelled by the test.");
            }
        }

        private sealed class FailingDisposeStream() : MemoryStream(Pdf)
        {
            public override ValueTask DisposeAsync()
            {
                Dispose();
                return ValueTask.FromException(new IOException("input disposal failed"));
            }
        }

        [TestMethod]
        public async Task InputDisposalFailureDeletesBufferedPdfAndDoesNotPoisonProcessor()
        {
            using var directory = new TestDirectory();
            await using var processor = new ParallelPdfProcessor(new ProcessorOptions
            {
                WorkerCount = 1, TransferMode = ProcessorTransferMode.MemoryMappedFile, TempDirectory = directory.PathName
            });
            await Assert.ThrowsExactlyAsync<IOException>(() => processor.ToImageAsync(new FailingDisposeStream(), cancellationToken: TestContext!.CancellationToken));
            Assert.IsEmpty(Directory.GetFiles(directory.PathName));
            Assert.IsEmpty(processor.TemporaryPdfPaths);
            using var next = await processor.ToImageAsync(new MemoryStream(Pdf), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken);
            Assert.IsEmpty(Directory.GetFiles(directory.PathName));
        }

        [TestMethod]
        public async Task ConcurrentMappedDocumentsStayIndependentAcrossRepeatedRequests()
        {
            using var directory = new TestDirectory();
            await using var processor = new ParallelPdfProcessor(new ProcessorOptions
            {
                WorkerCount = 3, SlotCount = 2, TransferMode = ProcessorTransferMode.MemoryMappedFile, TempDirectory = directory.PathName
            });
            var otherPdf = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "Wikimedia_Commons_web.pdf"));
            var options = new RenderOptions(Dpi: 40);
            async Task Render(byte[] bytes, int[] pages)
            {
                var index = 0;
                await foreach (var image in processor.ToImagesAsync(new MemoryStream(bytes), pages, options: options, cancellationToken: TestContext!.CancellationToken))
                {
                    using (image)
                    using (var expected = Conversion.ToImage(bytes, pages[index++], options: options))
                        AssertBitmapsEqual(expected, image);
                }
                Assert.AreEqual(pages.Length, index);
            }
            for (var iteration = 0; iteration < 4; iteration++)
            {
                await Task.WhenAll(Render(Pdf, [0, 0, 0]), Render(otherPdf, [2, 0, 1]));
                Assert.IsEmpty(Directory.GetFiles(directory.PathName));
                Assert.IsEmpty(processor.TemporaryPdfPaths);
                Assert.IsTrue(processor.WorkerDocumentIds.All(id => id == null));
            }
        }

        [TestMethod]
        public async Task SlotLimitBoundsConcurrentWorkAndCancellationReleasesPermit()
        {
            await using var pool = new BlockedStartupPool();
            using var firstCancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext!.CancellationToken);
            var pdf = new PdfRequest(Pdf, null);
            var first = pool.GetPageCountAsync(pdf, firstCancellation.Token);
            var second = pool.GetPageCountAsync(pdf, TestContext.CancellationToken);
            var third = pool.GetPageCountAsync(pdf, TestContext.CancellationToken);
            try
            {
                await pool.TwoStarted.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.CancellationToken);
                Assert.AreEqual(2, Volatile.Read(ref pool.Started));
                Assert.IsFalse(pool.ThreeStarted.Task.IsCompleted);
                firstCancellation.Cancel();
                await pool.ThreeStarted.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.CancellationToken);
                await Assert.ThrowsAsync<OperationCanceledException>(() => first);
            }
            finally
            {
                await pool.DisposeAsync();
                await Assert.ThrowsAsync<OperationCanceledException>(() => second);
                await Assert.ThrowsAsync<OperationCanceledException>(() => third);
            }
        }

        [TestMethod]
        public async Task CleanupDoesNotWaitForUnrelatedRenderSlots()
        {
            await using var pool = new BlockedStartupPool();
            var pdf = new PdfRequest(Pdf, null);
            var first = pool.GetPageCountAsync(pdf, TestContext!.CancellationToken);
            var second = pool.GetPageCountAsync(pdf, TestContext.CancellationToken);
            try
            {
                await pool.TwoStarted.Task.WaitAsync(TimeSpan.FromSeconds(10), TestContext.CancellationToken);
                await pool.ReleaseDocumentAsync(new PdfRequest(Pdf, null)).WaitAsync(TimeSpan.FromSeconds(2), TestContext.CancellationToken);
            }
            finally
            {
                await pool.DisposeAsync();
                await Assert.ThrowsAsync<OperationCanceledException>(() => first);
                await Assert.ThrowsAsync<OperationCanceledException>(() => second);
            }
        }

        [TestMethod]
        public async Task MappedRenderErrorKeepsWorkerUsableAndDeletesBitmapFile()
        {
            using var directory = new TestDirectory();
            using var worker = await WorkerConnection.StartAsync(TestContext!.CancellationToken);
            var request = new PdfRequest(Pdf, null);
            var processId = worker.ProcessId;
            await worker.ExecuteAsync(request, (_, _) => Task.FromResult(0), TestContext.CancellationToken);
            await Assert.ThrowsExactlyAsync<ParallelConversionException>(() => worker.RenderPageAsync(-1, new RenderOptions(Dpi: 40),
                ProcessorTransferMode.MemoryMappedFile, directory.PathName, TestContext.CancellationToken));
            Assert.IsEmpty(Directory.GetFiles(directory.PathName));
            Assert.AreEqual(processId, worker.ProcessId);
            using var bitmap = await worker.RenderPageAsync(0, new RenderOptions(Dpi: 40),
                ProcessorTransferMode.MemoryMappedFile, directory.PathName, TestContext.CancellationToken);
            Assert.IsEmpty(Directory.GetFiles(directory.PathName));
        }

        [TestMethod, OSCondition(OperatingSystems.Windows)]
        public void DeleteOnClosePdfDoesNotDeleteAgainWhileWorkerStillHasHandle()
        {
            using var directory = new TestDirectory();
            var path = Path.Combine(directory.PathName, "pending.pdf");
            File.WriteAllBytes(path, Pdf);
            var lifetime = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, 4096, FileOptions.DeleteOnClose);
            using var request = new PdfRequest(path, lifetime, null, deleteOnClose: true);
            using (var reader = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete))
                request.Dispose();
            Assert.IsFalse(File.Exists(path));
        }

        [TestMethod, OSCondition(OperatingSystems.Windows)]
        public void FailedPdfDeletionCanBeRetried()
        {
            using var directory = new TestDirectory();
            var path = Path.Combine(directory.PathName, "locked.pdf");
            File.WriteAllBytes(path, Pdf);
            var lifetime = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var removed = false;
            var request = new PdfRequest(path, lifetime, null, _ => removed = true);
            using (var blocker = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                Assert.ThrowsExactly<IOException>(request.Dispose);
                Assert.IsFalse(removed, "Failed cleanup must remain registered so processor disposal can retry it.");
            }
            request.Dispose();
            Assert.IsTrue(removed);
            Assert.IsFalse(File.Exists(path));
        }

        [TestMethod]
        [DataRow("cancel")]
        [DataRow("crash")]
        [DataRow("dispose")]
        public async Task InterruptedMappedRenderDeletesAllFiles(string interruption)
        {
            using var directory = new TestDirectory();
            await using var processor = new ParallelPdfProcessor(new ProcessorOptions
            {
                WorkerCount = 1, SlotCount = 1, TransferMode = ProcessorTransferMode.MemoryMappedFile, TempDirectory = directory.PathName
            });
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext!.CancellationToken);
            cancellation.CancelAfter(TimeSpan.FromSeconds(30));
            var render = processor.ToImageAsync(new MemoryStream(ParallelUnixProcessTestHook.SlowPdf()),
                options: new RenderOptions(Dpi: 72), cancellationToken: cancellation.Token);
            try
            {
                while (Directory.GetFiles(directory.PathName, "*.bitmap.raw").Length == 0)
                {
                    Assert.IsFalse(render.IsCompleted, "The slow render must remain active until it is interrupted.");
                    await Task.Delay(10, cancellation.Token);
                }
                if (!OperatingSystem.IsWindows())
                {
                    var otherPermissions = UnixFileMode.GroupRead | UnixFileMode.GroupWrite | UnixFileMode.GroupExecute |
                        UnixFileMode.OtherRead | UnixFileMode.OtherWrite | UnixFileMode.OtherExecute;
                    foreach (var path in Directory.GetFiles(directory.PathName))
                        Assert.AreEqual(UnixFileMode.None, File.GetUnixFileMode(path) & otherPermissions);
                }
                if (interruption == "cancel")
                    cancellation.Cancel();
                else if (interruption == "crash")
                {
                    using var process = Process.GetProcessById(processor.WorkerProcessIds.Single());
                    process.Kill();
                }
                else
                    await processor.DisposeAsync();

                try
                {
                    using var unexpected = await render.WaitAsync(TimeSpan.FromSeconds(10), TestContext.CancellationToken);
                    Assert.Fail("The interrupted render must fail.");
                }
                catch (Exception exception) when (exception is OperationCanceledException or ParallelConversionException or ObjectDisposedException) { }
                Assert.IsEmpty(Directory.GetFiles(directory.PathName));
                if (interruption != "dispose")
                {
                    using var next = await processor.ToImageAsync(new MemoryStream(Pdf), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext.CancellationToken);
                    Assert.IsEmpty(Directory.GetFiles(directory.PathName));
                }
            }
            finally
            {
                await processor.DisposeAsync();
                try { (await render).Dispose(); }
                catch (Exception exception) when (exception is OperationCanceledException or ParallelConversionException or ObjectDisposedException) { }
            }
        }
    }
}
#endif