#if NET8_0_OR_GREATER
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
using ParallelConversion = PDFtoImage.Parallel.Conversion;

namespace PDFtoImage.Tests
{
    [TestClass]
    [DoNotParallelize]
    [OSCondition(OperatingSystems.Windows)]
    public sealed class ParallelConversionTests : TestBase
    {
        private static readonly RenderOptions TestRenderOptions = new(Dpi: 40);

        [TestMethod]
        public async Task ToImageAsyncRendersSingleJob()
        {
            var expectedPath = GetExpectedPagePath(1);
            using var inputStream = OpenAsset("Wikimedia_Commons_web.pdf");
            using var actual = await ParallelConversion.ToImageAsync(
                inputStream,
                page: 1,
                leaveOpen: true,
                options: TestRenderOptions,
                workerCount: 2,
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

            await foreach (var bitmap in ParallelConversion.ToImagesAsync(
                pdf,
                pages,
                options: TestRenderOptions,
                workerCount: 2,
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
        public async Task ToImageAsyncSupportsBase64Input()
        {
            var expectedPath = GetExpectedPagePath(0);
            var pdfAsBase64 = Convert.ToBase64String(ReadAsset("Wikimedia_Commons_web.pdf"));
            using var actual = await ParallelConversion.ToImageAsync(
                pdfAsBase64,
                options: TestRenderOptions,
                workerCount: 1,
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
                ParallelConversion.ToImageAsync(
                    invalidPdf,
                    workerCount: 1,
                    cancellationToken: TestContext!.CancellationToken));

            Assert.AreEqual(typeof(PdfInvalidFormatException).FullName, exception.RemoteExceptionType);
        }

        [TestMethod]
        public async Task KilledWorkerProducesParallelConversionException()
        {
            var pdf = ReadAsset("hundesteuer-anmeldung.pdf");
            await using var pool = await WorkerPool.CreateAsync(1, pdf, null, TestContext!.CancellationToken);
            using var worker = Process.GetProcessById(pool.WorkerProcessIds.Single());

            worker.Kill();
            await worker.WaitForExitAsync(TestContext.CancellationToken);

            var exception = await Assert.ThrowsExactlyAsync<ParallelConversionException>(() =>
                pool.RenderPageAsync(0, TestRenderOptions, TestContext.CancellationToken));

            Assert.AreEqual("WorkerProcessTerminated", exception.RemoteExceptionType);
            Assert.IsInstanceOfType<IOException>(exception.InnerException);
        }

        [TestMethod]
        public async Task DisposingPoolTerminatesWorkers()
        {
            var pdf = ReadAsset("hundesteuer-anmeldung.pdf");
            var pool = await WorkerPool.CreateAsync(2, pdf, null, TestContext!.CancellationToken);
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
                ParallelConversion.ToImageAsync(
                    inputStream,
                    password: "wrong",
                    workerCount: 1,
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
                await foreach (var bitmap in ParallelConversion.ToImagesAsync(
                    pdf,
                    [-1, 0],
                    workerCount: 1,
                    cancellationToken: TestContext!.CancellationToken))
                {
                    bitmap.Dispose();
                }
            });
        }

        [TestMethod]
        public async Task InvalidWorkerCountIsRejected()
        {
            var pdf = ReadAsset("hundesteuer-anmeldung.pdf");

            await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() =>
                ParallelConversion.ToImageAsync(
                    pdf,
                    workerCount: 0,
                    cancellationToken: TestContext!.CancellationToken));
        }

        [TestMethod]
        public async Task CancellationStopsAsyncEnumeration()
        {
            var pdf = ReadAsset("hundesteuer-anmeldung.pdf");
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext!.CancellationToken);
            cancellation.Cancel();

            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await foreach (var bitmap in ParallelConversion.ToImagesAsync(
                    pdf,
                    options: TestRenderOptions,
                    workerCount: 2,
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
