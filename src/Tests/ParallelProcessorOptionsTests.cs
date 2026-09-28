#if NET11_0_OR_GREATER
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Parallel;
using PDFtoImage.Parallel.Internals;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static PDFtoImage.Tests.TestUtils;

namespace PDFtoImage.Tests
{
    [TestClass, DoNotParallelize]
    public sealed class ParallelProcessorOptionsTests : TestBase
    {
        private static readonly byte[] Pdf = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "Wikimedia_Commons_web.pdf"));

        private static MemoryStream OpenPdf() => new(Pdf, writable: false);

        private sealed class CancelledCopyStream(CancellationTokenSource cancellation) : MemoryStream(Pdf, writable: false)
        {
            public override async Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
            {
                await destination.WriteAsync(Pdf.AsMemory(0, 128), cancellationToken);
                cancellation.Cancel();
                cancellationToken.ThrowIfCancellationRequested();
            }
        }

        [TestMethod]
        public async Task PublicInterfaceAndDefaultOptionsRenderWithIpc()
        {
            IProcessorOptions defaults = new ProcessorOptions();
            Assert.IsNull(defaults.WorkerCount);
            Assert.IsNull(defaults.SlotCount);
            Assert.IsNull(defaults.TempDirectory);
            Assert.AreEqual(ProcessorTransferMode.Ipc, defaults.TransferMode);

            await using IParallelPdfProcessor processor = new ParallelPdfProcessor(new ProcessorOptions { WorkerCount = 1 });
            using var actual = await processor.ToImageAsync(OpenPdf(), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken);
            using var expected = Conversion.ToImage(Pdf, options: new RenderOptions(Dpi: 40));
            AssertBitmapsEqual(expected, actual);
        }

        [TestMethod]
        public void InvalidProcessorOptionsAreRejected()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => new ParallelPdfProcessor((ProcessorOptions)null!));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ParallelPdfProcessor(new ProcessorOptions { WorkerCount = 0 }));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ParallelPdfProcessor(new ProcessorOptions { SlotCount = 0 }));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ParallelPdfProcessor(new ProcessorOptions { SlotCount = -1 }));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ParallelPdfProcessor(new ProcessorOptions { TransferMode = (ProcessorTransferMode)99 }));
            using var ipc = new ParallelPdfProcessor(new ProcessorOptions { TempDirectory = "\0" });
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public async Task MappedModeRendersOrderedPagesAndDeletesTemporaryFiles(int slotCount)
        {
            var root = Path.Combine(Path.GetTempPath(), "PDFtoImage.Parallel.Tests." + Guid.NewGuid().ToString("N"));
            var directory = Path.Combine(root, "nested", "files");
            try
            {
                await using var processor = new ParallelPdfProcessor(new ProcessorOptions
                {
                    WorkerCount = 2,
                    SlotCount = slotCount == 0 ? null : slotCount,
                    TransferMode = ProcessorTransferMode.MemoryMappedFile,
                    TempDirectory = directory
                });
                var pages = new[] { 2, 0, 1 };
                var index = 0;
                await foreach (var image in processor.ToImagesAsync(OpenPdf(), pages, options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken))
                {
                    using (image)
                    using (var expected = Conversion.ToImage(Pdf, pages[index++], options: new RenderOptions(Dpi: 40)))
                        AssertBitmapsEqual(expected, image);

                    var paths = processor.TemporaryPdfPaths;
                    Assert.HasCount(1, paths);
                    Assert.AreEqual(directory, Path.GetDirectoryName(paths[0]));
                    Assert.IsTrue(paths[0].EndsWith(".pdf", StringComparison.Ordinal));
                    Assert.IsTrue(File.Exists(paths[0]), "The PDF must remain available to every worker until enumeration finishes.");
                    if (OperatingSystem.IsWindows())
                        Assert.IsTrue(File.GetAttributes(paths[0]).HasFlag(FileAttributes.Temporary));
                }

                Assert.AreEqual(pages.Length, index);
                Assert.IsEmpty(processor.TemporaryPdfPaths);
                Assert.IsEmpty(Directory.GetFiles(directory), "The host must delete every PDF and mapped bitmap when enumeration finishes.");
            }
            finally
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory);
                if (Directory.Exists(Path.Combine(root, "nested")))
                    Directory.Delete(Path.Combine(root, "nested"));
                if (Directory.Exists(root))
                    Directory.Delete(root);
            }
        }

        [TestMethod]
        public async Task MappedSinglePageDeletesBitmapBeforeReturning()
        {
            var root = Path.Combine(Path.GetTempPath(), "PDFtoImage.Parallel.Tests." + Guid.NewGuid().ToString("N"));
            var directory = Path.Combine(root, "nested", "files");
            try
            {
                await using var processor = new ParallelPdfProcessor(new ProcessorOptions
                {
                    WorkerCount = 1,
                    TransferMode = ProcessorTransferMode.MemoryMappedFile,
                    TempDirectory = directory
                });
                using var image = await processor.ToImageAsync(OpenPdf(), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken);
                Assert.IsEmpty(Directory.GetFiles(directory));
            }
            finally
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory);
                if (Directory.Exists(Path.Combine(root, "nested")))
                    Directory.Delete(Path.Combine(root, "nested"));
                if (Directory.Exists(root))
                    Directory.Delete(root);
            }
        }

        [TestMethod]
        public async Task MappedModeDeletesPdfAfterEarlyExitAndWorkerError()
        {
            await using var processor = new ParallelPdfProcessor(new ProcessorOptions { WorkerCount = 2, TransferMode = ProcessorTransferMode.MemoryMappedFile });
            await foreach (var image in processor.ToImagesAsync(OpenPdf(), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken))
            {
                image.Dispose();
                break;
            }
            Assert.IsEmpty(processor.TemporaryPdfPaths);

            await Assert.ThrowsExactlyAsync<ParallelConversionException>(() => processor.ToImageAsync(new MemoryStream([1, 2, 3]), cancellationToken: TestContext.CancellationToken));
            Assert.IsEmpty(processor.TemporaryPdfPaths);
        }

        [TestMethod]
        public async Task MappedModeDeletesPartialPdfAfterCopyCancellation()
        {
            var root = Path.Combine(Path.GetTempPath(), "PDFtoImage.Parallel.Tests." + Guid.NewGuid().ToString("N"));
            var directory = Path.Combine(root, "nested", "files");
            try
            {
                await using var processor = new ParallelPdfProcessor(new ProcessorOptions
                {
                    WorkerCount = 1,
                    TransferMode = ProcessorTransferMode.MemoryMappedFile,
                    TempDirectory = directory
                });
                using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext!.CancellationToken);
                await Assert.ThrowsAsync<OperationCanceledException>(() => processor.ToImageAsync(
                    new CancelledCopyStream(cancellation), cancellationToken: cancellation.Token));
                Assert.IsEmpty(processor.TemporaryPdfPaths);
                Assert.IsEmpty(Directory.GetFiles(directory));
                Assert.IsEmpty(processor.WorkerProcessIds);
            }
            finally
            {
                if (Directory.Exists(directory))
                    Directory.Delete(directory);
                if (Directory.Exists(Path.Combine(root, "nested")))
                    Directory.Delete(Path.Combine(root, "nested"));
                if (Directory.Exists(root))
                    Directory.Delete(root);
            }
        }

        [TestMethod]
        public async Task ProcessorDisposalDeletesPdfHeldByPausedEnumeration()
        {
            var processor = new ParallelPdfProcessor(new ProcessorOptions { WorkerCount = 1, TransferMode = ProcessorTransferMode.MemoryMappedFile });
            await using var iterator = processor.ToImagesAsync(OpenPdf(), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken).GetAsyncEnumerator();
            Assert.IsTrue(await iterator.MoveNextAsync());
            iterator.Current.Dispose();
            var path = processor.TemporaryPdfPaths.Single();
            Assert.IsTrue(File.Exists(path));
            await processor.DisposeAsync();
            Assert.IsFalse(File.Exists(path));
        }

        [TestMethod]
        public void RawMappedBitmapRoundTripPreservesPixels()
        {
            var path = Path.Combine(Path.GetTempPath(), "PDFtoImage.Parallel." + Guid.NewGuid().ToString("N") + ".bitmap.raw");
            try
            {
                using var original = new SKBitmap(320, 240, SKColorType.Bgra8888, SKAlphaType.Premul);
                original.Erase(new SKColor(40, 80, 120, 160));
                using var pipe = new MemoryStream();
                using (var creator = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None)) { }
                WorkerProtocol.WriteMappedBitmapResponse(pipe, original, path);
                pipe.Position = 0;
                var response = WorkerProtocol.ReadMessage(pipe)!;
                using var file = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
                using var decoded = WorkerProtocol.ReadMappedBitmap(response, 1, file);
                AssertBitmapsEqual(original, decoded);
                Assert.AreEqual(original.ByteCount, new FileInfo(path).Length);
                Assert.ThrowsExactly<InvalidDataException>(() => WorkerProtocol.ReadMappedBitmap([1, 2, 3], 0, file));
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}
#endif