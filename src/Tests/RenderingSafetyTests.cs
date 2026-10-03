using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Internals;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Tests
{
    [TestClass]
    public sealed class RenderingSafetyTests : TestBase
    {
        private static readonly byte[] Pdf = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "SocialPreview.pdf"));

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        public void IndexBeforeFirstPageReportsPageArgument(int operation)
        {
            using var stream = new MemoryStream(Pdf);
            var error = Assert.ThrowsExactly<ArgumentOutOfRangeException>(() =>
            {
                if (operation == 0)
                    Conversion.GetPageSize(stream, ^2);
                else
                    Conversion.ToImage(stream, ^2).Dispose();
            });
            Assert.AreEqual("page", error.ParamName);
            Assert.IsFalse(stream.CanRead);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void FailedImageEncodingIsReported(int inputKind)
        {
            using var output = new MemoryStream();
            using var input = new MemoryStream(Pdf);
            Assert.ThrowsExactly<IOException>(() =>
            {
                // WebP cannot encode dimensions above 16383 pixels. Rendering this
                // thin page is valid, but the encoder must report its failure.
                var options = new RenderOptions(Dpi: 72, Width: 16384, Height: 1);
                if (inputKind == 0)
                    Conversion.SaveWebp(output, input, options: options);
                else if (inputKind == 1)
                    Conversion.SaveWebp(output, Pdf, options: options);
                else
                    Conversion.SaveWebp(output, Convert.ToBase64String(Pdf), options: options);
            });
            Assert.AreEqual(0L, output.Length);
        }

        public static IEnumerable<object[]> InvalidOptions()
        {
            yield return [new RenderOptions(Dpi: 0)];
            yield return [new RenderOptions(Dpi: -1)];
            yield return [new RenderOptions(Width: 0)];
            yield return [new RenderOptions(Width: -1)];
            yield return [new RenderOptions(Height: 0)];
            yield return [new RenderOptions(Height: -1)];
            yield return [new RenderOptions(Rotation: (PdfRotation)4)];
            yield return [new RenderOptions(Width: int.MaxValue, Height: 1)];
            yield return [new RenderOptions(Width: 50000, Height: 50000)];
            yield return [new RenderOptions(Width: 2147483520, Height: 2147483520)];
            yield return [new RenderOptions(Dpi: int.MaxValue)];
            yield return [new RenderOptions(Bounds: new RectangleF(0, 0, 0, 10))];
            yield return [new RenderOptions(Bounds: new RectangleF(0, 0, 10, -1))];
            yield return [new RenderOptions(Bounds: new RectangleF(float.NaN, 0, 10, 10))];
            yield return [new RenderOptions(Bounds: new RectangleF(0, float.PositiveInfinity, 10, 10))];
            yield return [new RenderOptions(Bounds: new RectangleF(0, 0, float.NaN, 10))];
            yield return [new RenderOptions(Bounds: new RectangleF(0, 0, 10, float.PositiveInfinity))];
            yield return [new RenderOptions(Bounds: new RectangleF(float.MaxValue, 0, 10, 10))];
            yield return [new RenderOptions(Bounds: new RectangleF(0, 0, float.Epsilon, 10))];
            yield return [new RenderOptions(Bounds: new RectangleF(0, 0, float.MaxValue, 10))];
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void InvalidOutputStreamHonorsInputOwnership(bool leaveOpen)
        {
            using var input = new MemoryStream(Pdf);
            Assert.ThrowsExactly<ArgumentNullException>(() => Conversion.SavePng((Stream)null!, input, leaveOpen: leaveOpen));
            Assert.AreEqual(leaveOpen, input.CanRead);
        }

        [TestMethod]
        [DynamicData(nameof(InvalidOptions))]
        public void InvalidRenderOptionsAreRejectedBeforeAllocatingPixels(RenderOptions options)
        {
            using var stream = new MemoryStream(Pdf);
            using var document = PdfDocument.Load(stream, null, disposeStream: false);
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => document.Render(0, options,
                (_, _) => throw new AssertFailedException("Invalid options must be rejected before allocating pixels.")));
        }

#if NET6_0_OR_GREATER
        private static IEnumerable<int> CancelingPages(CancellationTokenSource cancellation)
        {
            yield return 0;
            cancellation.Cancel();
            yield return 0;
            throw new AssertFailedException("Canceled page selection must stop enumerating.");
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task CancelingPageSelectionStopsEnumerationAndHonorsStreamOwnership(bool leaveOpen)
        {
            using var cancellation = new CancellationTokenSource();
            using var stream = new MemoryStream(Pdf);
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await foreach (var image in Conversion.ToImagesAsync(stream, CancelingPages(cancellation), leaveOpen, cancellationToken: cancellation.Token))
                    image.Dispose();
            });
            Assert.AreEqual(leaveOpen, stream.CanRead);
        }

#if NET11_0_OR_GREATER
        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task ParallelCancelingPageSelectionStopsEnumerationAndHonorsStreamOwnership(bool leaveOpen)
        {
            await using var processor = new Parallel.ParallelPdfProcessor(new Parallel.ProcessorOptions { WorkerCount = 1 });
            using var cancellation = new CancellationTokenSource();
            using var stream = new MemoryStream(Pdf);
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await foreach (var image in processor.ToImagesAsync(stream, CancelingPages(cancellation), leaveOpen, cancellationToken: cancellation.Token))
                    image.Dispose();
            });
            Assert.AreEqual(leaveOpen, stream.CanRead);
            Assert.IsEmpty(processor.WorkerProcessIds);
        }
#endif
#endif

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void SemiTransparentBackgroundHasValidPremultipliedPixels(bool useTiling)
        {
            var background = new SKColor(40, 80, 120, 160);
            using var image = Conversion.ToImage(Pdf, options: new RenderOptions(Dpi: 72, Width: useTiling ? 4004 : 32, Height: 32,
                BackgroundColor: background, Bounds: new RectangleF(-10000, -10000, 100, 100), UseTiling: useTiling));
            Assert.AreEqual(SKAlphaType.Premul, image.AlphaType);
            AssertPremultipliedPixels(image);
            Assert.AreEqual(background, image.GetPixel(0, 0));
            Assert.AreEqual(background, image.GetPixel(image.Width - 1, image.Height - 1));
        }

        [TestMethod]
        public void RenderedContentHasValidPremultipliedPixels()
        {
            var pdf = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "hundesteuer-anmeldung.pdf"));
            using var image = Conversion.ToImage(pdf, options: new RenderOptions(Dpi: 40, BackgroundColor: new SKColor(0x64FFFFFF)));
            AssertPremultipliedPixels(image);
        }

        private static void AssertPremultipliedPixels(SKBitmap image)
        {
            var pixels = image.GetPixelSpan();
            for (var offset = 0; offset < pixels.Length; offset += 4)
            {
                var alpha = pixels[offset + 3];
                Assert.IsTrue(pixels[offset] <= alpha && pixels[offset + 1] <= alpha && pixels[offset + 2] <= alpha,
                    "Premultiplied RGB components must not exceed alpha.");
            }
        }

#if NET11_0_OR_GREATER
        [TestMethod]
        [DataRow(Parallel.ProcessorTransferMode.Ipc, false)]
        [DataRow(Parallel.ProcessorTransferMode.Ipc, true)]
        [DataRow(Parallel.ProcessorTransferMode.MemoryMappedFile, false)]
        [DataRow(Parallel.ProcessorTransferMode.MemoryMappedFile, true)]
        public async Task ParallelTransferPreservesPremultipliedPixels(Parallel.ProcessorTransferMode transferMode, bool useTiling)
        {
            await using var processor = new Parallel.ParallelPdfProcessor(new Parallel.ProcessorOptions
            {
                WorkerCount = 1,
                TransferMode = transferMode
            });
            var background = new SKColor(40, 80, 120, 160);
            var options = new RenderOptions(Dpi: 72, Width: useTiling ? 4004 : 32, Height: 32,
                BackgroundColor: background, Bounds: new RectangleF(-10000, -10000, 100, 100), UseTiling: useTiling);
            using var actual = await processor.ToImageAsync(new MemoryStream(Pdf), options: options, cancellationToken: TestContext!.CancellationToken);
            using var expected = new SKBitmap(actual.Width, actual.Height, SKColorType.Bgra8888, SKAlphaType.Premul);
            expected.Erase(background);
            TestUtils.AssertBitmapsEqual(expected, actual);
        }

        [TestMethod]
        [DataRow(Parallel.ProcessorTransferMode.Ipc)]
        [DataRow(Parallel.ProcessorTransferMode.MemoryMappedFile)]
        public async Task InvalidParallelRenderDoesNotPoisonWorker(Parallel.ProcessorTransferMode transferMode)
        {
            await using var processor = new Parallel.ParallelPdfProcessor(new Parallel.ProcessorOptions
            {
                WorkerCount = 1,
                TransferMode = transferMode
            });
            var error = await Assert.ThrowsExactlyAsync<Parallel.ParallelConversionException>(() => processor.ToImageAsync(
                new MemoryStream(Pdf), options: new RenderOptions(Width: 50000, Height: 50000), cancellationToken: TestContext!.CancellationToken));
            Assert.AreEqual(typeof(ArgumentOutOfRangeException).FullName, error.RemoteExceptionType);
            var processIds = processor.WorkerProcessIds;
            using var image = await processor.ToImageAsync(new MemoryStream(Pdf), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken);
            Assert.AreSequenceEqual(processIds, processor.WorkerProcessIds);
            Assert.IsEmpty(processor.TemporaryPdfPaths);
        }
#endif
    }
}