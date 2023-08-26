using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Tests;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static PDFtoImage.Conversion;
using static PDFtoImage.Tests.TestUtils;
[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace Tests
{
    [TestClass]
    public class ComparisonTests : TestBase
    {
        [TestMethod]
        [DataRow(0, DisplayName = "Page 1")]
        [DataRow(1, DisplayName = "Page 2")]
        [DataRow(2, DisplayName = "Page 3")]
        [DataRow(3, DisplayName = "Page 4")]
        [DataRow(4, DisplayName = "Page 5")]
        [DataRow(5, DisplayName = "Page 6")]
        [DataRow(6, DisplayName = "Page 7")]
        [DataRow(7, DisplayName = "Page 8")]
        [DataRow(8, DisplayName = "Page 9")]
        [DataRow(9, DisplayName = "Page 10")]
        [DataRow(10, DisplayName = "Page 11")]
        [DataRow(11, DisplayName = "Page 12")]
        [DataRow(12, DisplayName = "Page 13")]
        [DataRow(13, DisplayName = "Page 14")]
        [DataRow(14, DisplayName = "Page 15")]
        [DataRow(15, DisplayName = "Page 16")]
        [DataRow(16, DisplayName = "Page 17")]
        [DataRow(17, DisplayName = "Page 18")]
        [DataRow(18, DisplayName = "Page 19")]
        [DataRow(19, DisplayName = "Page 20")]
        [DataRow(0, true, DisplayName = "Page 1 (with annotations)")]
        [DataRow(1, true, DisplayName = "Page 2 (with annotations)")]
        [DataRow(2, true, DisplayName = "Page 3 (with annotations)")]
        [DataRow(3, true, DisplayName = "Page 4 (with annotations)")]
        [DataRow(4, true, DisplayName = "Page 5 (with annotations)")]
        [DataRow(5, true, DisplayName = "Page 6 (with annotations)")]
        [DataRow(6, true, DisplayName = "Page 7 (with annotations)")]
        [DataRow(7, true, DisplayName = "Page 8 (with annotations)")]
        [DataRow(8, true, DisplayName = "Page 9 (with annotations)")]
        [DataRow(9, true, DisplayName = "Page 10 (with annotations)")]
        [DataRow(10, true, DisplayName = "Page 11 (with annotations)")]
        [DataRow(11, true, DisplayName = "Page 12 (with annotations)")]
        [DataRow(12, true, DisplayName = "Page 13 (with annotations)")]
        [DataRow(13, true, DisplayName = "Page 14 (with annotations)")]
        [DataRow(14, true, DisplayName = "Page 15 (with annotations)")]
        [DataRow(15, true, DisplayName = "Page 16 (with annotations)")]
        [DataRow(16, true, DisplayName = "Page 17 (with annotations)")]
        [DataRow(17, true, DisplayName = "Page 18 (with annotations)")]
        [DataRow(18, true, DisplayName = "Page 19 (with annotations)")]
        [DataRow(19, true, DisplayName = "Page 20 (with annotations)")]
        public void SaveWebpPageNumber(int page, bool withAnnotations = false)
        {
            var expectedPath = Path.Combine("Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{page}{(withAnnotations ? "_ANNOT" : string.Empty)}.webp");

            using var inputStream = new FileStream(Path.Combine("Assets", "Wikimedia_Commons_web.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(expectedPath, FileMode.Open, FileAccess.Read);
            using var outputStream = CreateOutputStream(expectedPath);

            SaveWebp(outputStream, inputStream, page: page, dpi: 40, withAnnotations: withAnnotations);

            CompareStreams(expectedStream, outputStream);
        }

        [TestMethod]
        [DataRow(false, DisplayName = "Without annotations")]
        [DataRow(true, DisplayName = "With annotations")]
        public void SaveWebpPages(bool withAnnotations = false)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "Wikimedia_Commons_web.pdf"), FileMode.Open, FileAccess.Read);

            int page = 0;

            foreach (var image in ToImages(inputStream, dpi: 40, withAnnotations: withAnnotations))
            {
                var expectedPath = Path.Combine("Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{page}{(withAnnotations ? "_ANNOT" : string.Empty)}.webp");

                using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{page}{(withAnnotations ? "_ANNOT" : string.Empty)}.webp"), FileMode.Open, FileAccess.Read);
                using var outputStream = CreateOutputStream(expectedPath);
                image.Encode(outputStream, SKEncodedImageFormat.Webp, 100);

                CompareStreams(expectedStream, outputStream);

                page++;
            }
        }

#if NET6_0_OR_GREATER
        [TestMethod]
        [DataRow(false, DisplayName = "Without annotations")]
        [DataRow(true, DisplayName = "With annotations")]
        public async Task SaveWebpPagesAsync(bool withAnnotations = false)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "Wikimedia_Commons_web.pdf"), FileMode.Open, FileAccess.Read);

            int page = 0;

            await foreach (var image in ToImagesAsync(inputStream, dpi: 40, withAnnotations: withAnnotations))
            {
                var expectedPath = Path.Combine("Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{page}{(withAnnotations ? "_ANNOT" : string.Empty)}.webp");

                using var expectedStream = new FileStream(expectedPath, FileMode.Open, FileAccess.Read);
                using var outputStream = CreateOutputStream(expectedPath);
                image.Encode(outputStream, SKEncodedImageFormat.Webp, 100);

                CompareStreams(expectedStream, outputStream);

                page++;
            }
        }
#endif

        [TestMethod]
        [DataRow(0, DisplayName = "Page 1")]
        [DataRow(1, DisplayName = "Page 2")]
        [DataRow(2, DisplayName = "Page 3")]
        [DataRow(3, DisplayName = "Page 4")]
        [DataRow(4, DisplayName = "Page 5")]
        [DataRow(5, DisplayName = "Page 6")]
        [DataRow(6, DisplayName = "Page 7")]
        [DataRow(7, DisplayName = "Page 8")]
        [DataRow(8, DisplayName = "Page 9")]
        [DataRow(9, DisplayName = "Page 10")]
        [DataRow(10, DisplayName = "Page 11")]
        [DataRow(11, DisplayName = "Page 12")]
        [DataRow(12, DisplayName = "Page 13")]
        [DataRow(13, DisplayName = "Page 14")]
        [DataRow(14, DisplayName = "Page 15")]
        [DataRow(15, DisplayName = "Page 16")]
        [DataRow(16, DisplayName = "Page 17")]
        [DataRow(17, DisplayName = "Page 18")]
        [DataRow(18, DisplayName = "Page 19")]
        [DataRow(19, DisplayName = "Page 20")]
        [DataRow(0, true, DisplayName = "Page 1 (with annotations)")]
        [DataRow(1, true, DisplayName = "Page 2 (with annotations)")]
        [DataRow(2, true, DisplayName = "Page 3 (with annotations)")]
        [DataRow(3, true, DisplayName = "Page 4 (with annotations)")]
        [DataRow(4, true, DisplayName = "Page 5 (with annotations)")]
        [DataRow(5, true, DisplayName = "Page 6 (with annotations)")]
        [DataRow(6, true, DisplayName = "Page 7 (with annotations)")]
        [DataRow(7, true, DisplayName = "Page 8 (with annotations)")]
        [DataRow(8, true, DisplayName = "Page 9 (with annotations)")]
        [DataRow(9, true, DisplayName = "Page 10 (with annotations)")]
        [DataRow(10, true, DisplayName = "Page 11 (with annotations)")]
        [DataRow(11, true, DisplayName = "Page 12 (with annotations)")]
        [DataRow(12, true, DisplayName = "Page 13 (with annotations)")]
        [DataRow(13, true, DisplayName = "Page 14 (with annotations)")]
        [DataRow(14, true, DisplayName = "Page 15 (with annotations)")]
        [DataRow(15, true, DisplayName = "Page 16 (with annotations)")]
        [DataRow(16, true, DisplayName = "Page 17 (with annotations)")]
        [DataRow(17, true, DisplayName = "Page 18 (with annotations)")]
        [DataRow(18, true, DisplayName = "Page 19 (with annotations)")]
        [DataRow(19, true, DisplayName = "Page 20 (with annotations)")]
        public void SavePngPageNumber(int page, bool withAnnotations = false)
        {
            var expectedPath = Path.Combine("Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{page}{(withAnnotations ? "_ANNOT" : string.Empty)}.png");

            using var inputStream = new FileStream(Path.Combine("Assets", "Wikimedia_Commons_web.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(expectedPath, FileMode.Open, FileAccess.Read);
            using var outputStream = CreateOutputStream(expectedPath);

            SavePng(outputStream, inputStream, page: page, dpi: 40, withAnnotations: withAnnotations);

            CompareStreams(expectedStream, outputStream);
        }

        [TestMethod]
        [DataRow(false, DisplayName = "Without annotations")]
        [DataRow(true, DisplayName = "With annotations")]
        public void SavePngPages(bool withAnnotations = false)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "Wikimedia_Commons_web.pdf"), FileMode.Open, FileAccess.Read);

            int page = 0;

            foreach (var image in ToImages(inputStream, dpi: 40, withAnnotations: withAnnotations))
            {
                var expectedPath = Path.Combine("Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{page}{(withAnnotations ? "_ANNOT" : string.Empty)}.png");

                using var expectedStream = new FileStream(expectedPath, FileMode.Open, FileAccess.Read);
                using var outputStream = CreateOutputStream(expectedPath);
                image.Encode(outputStream, SKEncodedImageFormat.Png, 100);

                CompareStreams(expectedStream, outputStream);

                page++;
            }
        }

#if NET6_0_OR_GREATER
        [TestMethod]
        [DataRow(false, DisplayName = "Without annotations")]
        [DataRow(true, DisplayName = "With annotations")]
        public async Task SavePngPagesAsync(bool withAnnotations = false)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "Wikimedia_Commons_web.pdf"), FileMode.Open, FileAccess.Read);

            int page = 0;

            await foreach (var image in ToImagesAsync(inputStream, dpi: 40, withAnnotations: withAnnotations))
            {
                var expectedPath = Path.Combine("Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{page}{(withAnnotations ? "_ANNOT" : string.Empty)}.png");

                using var expectedStream = new FileStream(expectedPath, FileMode.Open, FileAccess.Read);
                using var outputStream = CreateOutputStream(expectedPath);
                image.Encode(outputStream, SKEncodedImageFormat.Png, 100);

                CompareStreams(expectedStream, outputStream);

                page++;
            }
        }
#endif

        [TestMethod]
        [DataRow(0, DisplayName = "Page 1")]
        [DataRow(1, DisplayName = "Page 2")]
        [DataRow(2, DisplayName = "Page 3")]
        [DataRow(3, DisplayName = "Page 4")]
        [DataRow(4, DisplayName = "Page 5")]
        [DataRow(5, DisplayName = "Page 6")]
        [DataRow(6, DisplayName = "Page 7")]
        [DataRow(7, DisplayName = "Page 8")]
        [DataRow(8, DisplayName = "Page 9")]
        [DataRow(9, DisplayName = "Page 10")]
        [DataRow(10, DisplayName = "Page 11")]
        [DataRow(11, DisplayName = "Page 12")]
        [DataRow(12, DisplayName = "Page 13")]
        [DataRow(13, DisplayName = "Page 14")]
        [DataRow(14, DisplayName = "Page 15")]
        [DataRow(15, DisplayName = "Page 16")]
        [DataRow(16, DisplayName = "Page 17")]
        [DataRow(17, DisplayName = "Page 18")]
        [DataRow(18, DisplayName = "Page 19")]
        [DataRow(19, DisplayName = "Page 20")]
        [DataRow(0, true, DisplayName = "Page 1 (with annotations)")]
        [DataRow(1, true, DisplayName = "Page 2 (with annotations)")]
        [DataRow(2, true, DisplayName = "Page 3 (with annotations)")]
        [DataRow(3, true, DisplayName = "Page 4 (with annotations)")]
        [DataRow(4, true, DisplayName = "Page 5 (with annotations)")]
        [DataRow(5, true, DisplayName = "Page 6 (with annotations)")]
        [DataRow(6, true, DisplayName = "Page 7 (with annotations)")]
        [DataRow(7, true, DisplayName = "Page 8 (with annotations)")]
        [DataRow(8, true, DisplayName = "Page 9 (with annotations)")]
        [DataRow(9, true, DisplayName = "Page 10 (with annotations)")]
        [DataRow(10, true, DisplayName = "Page 11 (with annotations)")]
        [DataRow(11, true, DisplayName = "Page 12 (with annotations)")]
        [DataRow(12, true, DisplayName = "Page 13 (with annotations)")]
        [DataRow(13, true, DisplayName = "Page 14 (with annotations)")]
        [DataRow(14, true, DisplayName = "Page 15 (with annotations)")]
        [DataRow(15, true, DisplayName = "Page 16 (with annotations)")]
        [DataRow(16, true, DisplayName = "Page 17 (with annotations)")]
        [DataRow(17, true, DisplayName = "Page 18 (with annotations)")]
        [DataRow(18, true, DisplayName = "Page 19 (with annotations)")]
        [DataRow(19, true, DisplayName = "Page 20 (with annotations)")]
        public void SaveJpegPageNumber(int page, bool withAnnotations = false)
        {
            var expectedPath = Path.Combine("Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{page}{(withAnnotations ? "_ANNOT" : string.Empty)}.jpg");

            using var inputStream = new FileStream(Path.Combine("Assets", "Wikimedia_Commons_web.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(expectedPath, FileMode.Open, FileAccess.Read);
            using var outputStream = CreateOutputStream(expectedPath);

            SaveJpeg(outputStream, inputStream, page: page, dpi: 40, withAnnotations: withAnnotations);

            CompareStreams(expectedStream, outputStream);
        }

        [TestMethod]
        [DataRow(false, DisplayName = "Without annotations")]
        [DataRow(true, DisplayName = "With annotations")]
        public void SaveJpegPages(bool withAnnotations = false)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "Wikimedia_Commons_web.pdf"), FileMode.Open, FileAccess.Read);

            int page = 0;

            foreach (var image in ToImages(inputStream, dpi: 40, withAnnotations: withAnnotations))
            {
                var expectedPath = Path.Combine("Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{page}{(withAnnotations ? "_ANNOT" : string.Empty)}.jpg");

                using var expectedStream = new FileStream(expectedPath, FileMode.Open, FileAccess.Read);
                using var outputStream = CreateOutputStream(expectedPath);
                image.Encode(outputStream, SKEncodedImageFormat.Jpeg, 100);

                CompareStreams(expectedStream, outputStream);

                page++;
            }
        }

#if NET6_0_OR_GREATER
        [TestMethod]
        [DataRow(false, DisplayName = "Without annotations")]
        [DataRow(true, DisplayName = "With annotations")]
        public async Task SaveJpegPagesAsync(bool withAnnotations = false)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "Wikimedia_Commons_web.pdf"), FileMode.Open, FileAccess.Read);

            int page = 0;

            await foreach (var image in ToImagesAsync(inputStream, dpi: 40, withAnnotations: withAnnotations))
            {
                var expectedPath = Path.Combine("Assets", "Expected", GetPlatformAsString(), $"Wikimedia_Commons_web_{page}{(withAnnotations ? "_ANNOT" : string.Empty)}.jpg");

                using var expectedStream = new FileStream(expectedPath, FileMode.Open, FileAccess.Read);
                using var outputStream = CreateOutputStream(expectedPath);
                image.Encode(outputStream, SKEncodedImageFormat.Jpeg, 100);

                CompareStreams(expectedStream, outputStream);

                page++;
            }
        }
#endif

        [TestMethod]
        [DataRow(10, DisplayName = "10 DPI")]
        [DataRow(30, DisplayName = "30 DPI")]
        [DataRow(100, DisplayName = "100 DPI")]
        [DataRow(300, DisplayName = "300 DPI")]
        [DataRow(600, DisplayName = "600 DPI")]
        [DataRow(1200, DisplayName = "1200 DPI")]
        [DataRow(10, true, DisplayName = "10 DPI (with annotations)")]
        [DataRow(30, true, DisplayName = "30 DPI (with annotations)")]
        [DataRow(100, true, DisplayName = "100 DPI (with annotations)")]
        [DataRow(300, true, DisplayName = "300 DPI (with annotations)")]
        [DataRow(600, true, DisplayName = "600 DPI (with annotations)")]
        [DataRow(1200, true, DisplayName = "1200 DPI (with annotations)")]
        public void SavePngDpi(int dpi, bool withAnnotations = false)
        {
            using var pdfStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            using var image = ToImage(pdfStream, dpi: dpi, withAnnotations: withAnnotations);

            using var pdfStream2 = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            using var image2 = ToImage(pdfStream2, dpi: 300, withAnnotations: withAnnotations);

            Assert.IsNotNull(image);
            Assert.IsTrue(Math.Abs(image.Width - image2.Width * (dpi / 300.0)) < 3);
            Assert.IsTrue(Math.Abs(image.Height - image2.Height * (dpi / 300.0)) < 3);
        }

        [TestMethod]
        [DataRow(10, DisplayName = "10 DPI")]
        [DataRow(30, DisplayName = "30 DPI")]
        [DataRow(100, DisplayName = "100 DPI")]
        [DataRow(300, DisplayName = "300 DPI")]
        [DataRow(600, DisplayName = "600 DPI")]
        [DataRow(1200, DisplayName = "1200 DPI")]
        [DataRow(10, true, DisplayName = "10 DPI (with annotations)")]
        [DataRow(30, true, DisplayName = "30 DPI (with annotations)")]
        [DataRow(100, true, DisplayName = "100 DPI (with annotations)")]
        [DataRow(300, true, DisplayName = "300 DPI (with annotations)")]
        [DataRow(600, true, DisplayName = "600 DPI (with annotations)")]
        [DataRow(1200, true, DisplayName = "1200 DPI (with annotations)")]
        public void SavePngDpiImages(int dpi, bool withAnnotations = false)
        {
            using var pdfStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            using var image = ToImages(pdfStream, dpi: dpi, withAnnotations: withAnnotations).Single();

            using var pdfStream2 = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            using var image2 = ToImages(pdfStream2, dpi: 300, withAnnotations: withAnnotations).Single();

            Assert.IsNotNull(image);
            Assert.IsTrue(Math.Abs(image.Width - image2.Width * (dpi / 300.0)) < 3);
            Assert.IsTrue(Math.Abs(image.Height - image2.Height * (dpi / 300.0)) < 3);
        }

#if NET6_0_OR_GREATER
        [TestMethod]
        [DataRow(10, DisplayName = "10 DPI")]
        [DataRow(30, DisplayName = "30 DPI")]
        [DataRow(100, DisplayName = "100 DPI")]
        [DataRow(300, DisplayName = "300 DPI")]
        [DataRow(600, DisplayName = "600 DPI")]
        [DataRow(1200, DisplayName = "1200 DPI")]
        [DataRow(10, true, DisplayName = "10 DPI (with annotations)")]
        [DataRow(30, true, DisplayName = "30 DPI (with annotations)")]
        [DataRow(100, true, DisplayName = "100 DPI (with annotations)")]
        [DataRow(300, true, DisplayName = "300 DPI (with annotations)")]
        [DataRow(600, true, DisplayName = "600 DPI (with annotations)")]
        [DataRow(1200, true, DisplayName = "1200 DPI (with annotations)")]
        public async Task SavePngDpiImagesAsync(int dpi, bool withAnnotations = false)
        {
            using var pdfStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);

            await foreach (var image in ToImagesAsync(pdfStream, dpi: dpi, withAnnotations: withAnnotations))
            {
                Assert.IsNotNull(image);
            }
        }
#endif

        [TestMethod]
        [DataRow("SocialPreview.pdf", 1)]
        [DataRow("hundesteuer-anmeldung.pdf", 3)]
        [DataRow("Wikimedia_Commons_web.pdf", 20)]
        public void GetPageCountTest(string pdfFileName, int expectedPageCount)
        {
            using var pdfStream = new FileStream(Path.Combine("Assets", pdfFileName), FileMode.Open, FileAccess.Read);
            Assert.AreEqual(expectedPageCount, GetPageCount(pdfStream), $"The expected and actual page count for the file {pdfFileName} are not equal.");
        }

#if NET6_0_OR_GREATER
        [TestMethod]
        [DataRow("SocialPreview.pdf")]
        [DataRow("hundesteuer-anmeldung.pdf")]
        [DataRow("Wikimedia_Commons_web.pdf")]
        public async Task ToImagesAsyncTaskCanceledException(string pdfFileName)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", pdfFileName), FileMode.Open, FileAccess.Read);
            var token = new CancellationTokenSource();

            await Assert.ThrowsExceptionAsync<TaskCanceledException>(async () =>
            {
                token.Cancel();

                await foreach (var image in ToImagesAsync(inputStream, dpi: 1200, cancellationToken: token.Token))
                {
                }
            });
        }

        [TestMethod]
        [DataRow("SocialPreview.pdf")]
        [DataRow("hundesteuer-anmeldung.pdf")]
        [DataRow("Wikimedia_Commons_web.pdf")]
        public async Task ToImagesAsyncOperationCanceledException(string pdfFileName)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", pdfFileName), FileMode.Open, FileAccess.Read);
            var token = new CancellationTokenSource();
            var pageCount = GetPageCount(inputStream);

            using var inputStream2 = new FileStream(Path.Combine("Assets", pdfFileName), FileMode.Open, FileAccess.Read);

            if (pageCount < 2)
            {
                // no OperationCanceledException should be thrown if there are not multiple pages to iterate through
                await foreach (var image in ToImagesAsync(inputStream2, dpi: 1200, cancellationToken: token.Token))
                {
                    token.Cancel();
                }

                return;
            }

            await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () =>
            {
                await foreach (var image in ToImagesAsync(inputStream2, dpi: 1200, cancellationToken: token.Token))
                {
                    token.Cancel();
                }
            });
        }
#endif
    }
}