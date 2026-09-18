#if NET9_0_OR_GREATER
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Exceptions;
using PDFtoImage.Parallel;
using PDFtoImage.Parallel.Internals;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static PDFtoImage.Tests.TestUtils;

namespace PDFtoImage.Tests
{
    [TestClass]
    [DoNotParallelize]
    [OSCondition(OperatingSystems.Windows)]
    public sealed class ParallelConversionTests : TestBase
    {
        private ParallelPdfProcessor _converter = null!;

        [TestInitialize]
        public void CreateConverter() => _converter = new ParallelPdfProcessor(2);

        [TestCleanup]
        public async Task DisposeConverter() => await _converter.DisposeAsync();

        private static MemoryStream OpenPdf(byte[] bytes) => new(bytes, writable: false);

        private static readonly RenderOptions TestRenderOptions = new(Dpi: 40);

        [TestMethod]
        public async Task ToImageAsyncRendersSingleJob()
        {
            var expectedPath = GetExpectedPagePath(1);

            using var inputStream = OpenAsset("Wikimedia_Commons_web.pdf");
            using var actual = await _converter.ToImageAsync(
                inputStream,
                page: 1,
                leaveOpen: true,
                options: TestRenderOptions,

                cancellationToken: TestContext!.CancellationToken);
            using var outputStream = CreateOutputStream(expectedPath);

            actual.Encode(outputStream, SKEncodedImageFormat.Png, 100);
            CompareStreams(expectedPath, outputStream);
            Assert.IsTrue(inputStream.CanRead, "The input stream should remain open when leaveOpen is true.");
        }

        [TestMethod]
        public async Task ToImagesAsyncPreservesRequestedOrder()
        {
            var pdf = ReadAsset("Wikimedia_Commons_web.pdf");
            int[] pages = [2, 0, 1];
            var actualPageCount = 0;

            await foreach (var bitmap in _converter.ToImagesAsync(
                OpenPdf(pdf),
                pages,
                options: TestRenderOptions,

                cancellationToken: TestContext!.CancellationToken))
            {
                using (bitmap)
                using (var outputStream = CreateOutputStream(GetExpectedPagePath(pages[actualPageCount])))
                {
                    bitmap.Encode(outputStream, SKEncodedImageFormat.Png, 100);
                    CompareStreams(GetExpectedPagePath(pages[actualPageCount]), outputStream);
                    actualPageCount++;
                }
            }

            Assert.AreEqual(pages.Length, actualPageCount);
        }

        [TestMethod]
        public async Task ToImageAsyncSupportsStreamInput()
        {
            var expectedPath = GetExpectedPagePath(0);
            using var inputStream = OpenAsset("Wikimedia_Commons_web.pdf");
            using var actual = await _converter.ToImageAsync(
                inputStream,
                options: TestRenderOptions,

                cancellationToken: TestContext!.CancellationToken);
            using var outputStream = CreateOutputStream(expectedPath);

            actual.Encode(outputStream, SKEncodedImageFormat.Png, 100);
            CompareStreams(expectedPath, outputStream);
        }

        [TestMethod]
        public async Task InvalidPdfPropagatesWorkerException()
        {
            var invalidPdf = ReadAsset("DummyImage.png");
            var exception = await Assert.ThrowsExactlyAsync<ParallelConversionException>(() =>
                _converter.ToImageAsync(
                    OpenPdf(invalidPdf),

                    cancellationToken: TestContext!.CancellationToken));

            Assert.AreEqual(typeof(PdfInvalidFormatException).FullName, exception.RemoteExceptionType);
        }

        [TestMethod]
        public async Task KilledWorkerProducesParallelConversionException()
        {
            var pdf = ReadAsset("hundesteuer-anmeldung.pdf");
            await using var pool = new ParallelPdfProcessor(1);
            using var warmup = await pool.ToImageAsync(OpenPdf(pdf), options: TestRenderOptions, cancellationToken: TestContext!.CancellationToken);
            using var worker = Process.GetProcessById(pool.WorkerProcessIds.Single());

            worker.Kill();
            await worker.WaitForExitAsync(TestContext.CancellationToken);

            var exception = await Assert.ThrowsExactlyAsync<ParallelConversionException>(() =>
                pool.ToImageAsync(OpenPdf(pdf), options: TestRenderOptions, cancellationToken: TestContext.CancellationToken));

            Assert.AreEqual("WorkerProcessTerminated", exception.RemoteExceptionType);
            Assert.IsInstanceOfType<IOException>(exception.InnerException);
            using var recovered = await pool.ToImageAsync(OpenPdf(pdf), options: TestRenderOptions, cancellationToken: TestContext.CancellationToken);
            Assert.AreNotEqual(worker.Id, pool.WorkerProcessIds.Single());
            using var expected = global::PDFtoImage.Conversion.ToImage(pdf, options: TestRenderOptions);
            CollectionAssert.AreEqual(expected.Bytes, recovered.Bytes);
        }

        [TestMethod]
        public async Task DisposingPoolTerminatesWorkers()
        {
            var pdf = ReadAsset("hundesteuer-anmeldung.pdf");
            var pool = new ParallelPdfProcessor(2);
            using var warmup = await pool.ToImageAsync(OpenPdf(pdf), options: TestRenderOptions, cancellationToken: TestContext!.CancellationToken);
            var workers = pool.WorkerProcessIds.Select(Process.GetProcessById).ToArray();

            try
            {
                await pool.DisposeAsync();

                foreach (var worker in workers)
                {
                    await worker.WaitForExitAsync(TestContext.CancellationToken);
                    Assert.IsTrue(worker.HasExited);
                }
            }
            finally
            {
                await pool.DisposeAsync();
                foreach (var worker in workers)
                    worker.Dispose();
            }
        }

        [TestMethod]
        public async Task WrongPasswordPropagatesWorkerExceptionAndClosesOwnedStream()
        {
            var inputStream = OpenAsset("SocialPreview with password 123456 (AES-256).pdf");

            var exception = await Assert.ThrowsExactlyAsync<ParallelConversionException>(() =>
                _converter.ToImageAsync(
                    inputStream,
                    password: "wrong",

                    cancellationToken: TestContext!.CancellationToken));

            Assert.AreEqual(typeof(PdfPasswordProtectedException).FullName, exception.RemoteExceptionType);
            Assert.IsFalse(inputStream.CanRead, "An owned input stream should be closed after a worker error.");
        }

        [TestMethod]
        public async Task OutOfRangePageSelectionIsRejected()
        {
            var pdf = ReadAsset("hundesteuer-anmeldung.pdf");

            await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(async () =>
            {
                await foreach (var bitmap in _converter.ToImagesAsync(
                    OpenPdf(pdf),
                    [-1, 0],

                    cancellationToken: TestContext!.CancellationToken))
                {
                    bitmap.Dispose();
                }
            });
        }

        [TestMethod]
        public void InvalidWorkerCountIsRejected()
        {
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ParallelPdfProcessor(0));
            Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ParallelPdfProcessor(-1));
        }

        [TestMethod]
        public async Task CancellationStopsAsyncEnumeration()
        {
            var pdf = ReadAsset("hundesteuer-anmeldung.pdf");
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext!.CancellationToken);
            cancellation.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await foreach (var bitmap in _converter.ToImagesAsync(
                    OpenPdf(pdf),
                    options: TestRenderOptions,

                    cancellationToken: cancellation.Token))
                {
                    bitmap.Dispose();
                }
            });
        }

        private static FileStream OpenAsset(string fileName)
        {
            return GetInputStream(Path.Combine("..", "Assets", fileName));
        }

        private static byte[] ReadAsset(string fileName)
        {
            using var inputStream = OpenAsset(fileName);
            using var memoryStream = new MemoryStream();
            inputStream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }

        private static string GetExpectedPagePath(int page) =>
            Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{page}.png");
    }
}
#endif