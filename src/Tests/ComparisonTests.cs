using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using static PDFtoImage.Conversion;
using static PDFtoImage.Tests.TestUtils;
[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace Tests
{
    [TestClass]
    public class ComparisonTests
    {
        [TestInitialize]
        public void Initialize()
        {
#if NET5_0_OR_GREATER
            if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
                Assert.Inconclusive("This test must run on Windows, Linux or macOS.");
#endif
        }

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
        public void SaveBmpPageNumber(int page, bool withAnnotations = false)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "Wikimedia_Commons_web.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", $"Wikimedia_Commons_web_{page}{(withAnnotations ? "_ANNOT" : string.Empty)}.bmp"), FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();

            SaveBmp(outputStream, inputStream, page: page, dpi: 40, withAnnotations: withAnnotations);

            CompareStreams(expectedStream, outputStream);
        }

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
#if !NETCOREAPP3_0_OR_GREATER
            if (page == 4 || page == 13)
                Assert.Inconclusive("Different results for .NET Framework 4.6.1.");
#endif
            using var inputStream = new FileStream(Path.Combine("Assets", "Wikimedia_Commons_web.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", $"Wikimedia_Commons_web_{page}{(withAnnotations ? "_ANNOT" : string.Empty)}.png"), FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();

            SavePng(outputStream, inputStream, page: page, dpi: 40, withAnnotations: withAnnotations);

            CompareStreams(expectedStream, outputStream);
        }

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
        public void SaveGifPageNumber(int page, bool withAnnotations = false)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "Wikimedia_Commons_web.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", $"Wikimedia_Commons_web_{page}{(withAnnotations ? "_ANNOT" : string.Empty)}.gif"), FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();

            SaveGif(outputStream, inputStream, page: page, dpi: 40, withAnnotations: withAnnotations);

            CompareStreams(expectedStream, outputStream);
        }

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
            using var inputStream = new FileStream(Path.Combine("Assets", "Wikimedia_Commons_web.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", $"Wikimedia_Commons_web_{page}{(withAnnotations ? "_ANNOT" : string.Empty)}.jpg"), FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();

            SaveJpeg(outputStream, inputStream, page: page, dpi: 40, withAnnotations: withAnnotations);

            CompareStreams(expectedStream, outputStream);
        }

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
        public void SaveTiffPageNumber(int page, bool withAnnotations = false)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "Wikimedia_Commons_web.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", $"Wikimedia_Commons_web_{page}{(withAnnotations ? "_ANNOT" : string.Empty)}.tif"), FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();

            SaveTiff(outputStream, inputStream, page: page, dpi: 40, withAnnotations: withAnnotations);

            CompareStreams(expectedStream, outputStream);
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
        public void SavePngDpi(int dpi, bool withAnnotations = false)
        {
            using var pdfStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            using var image = ToImage(pdfStream, dpi: dpi, withAnnotations: withAnnotations);

            Assert.IsNotNull(image);
            Assert.AreEqual(dpi, image.HorizontalResolution);
            Assert.AreEqual(dpi, image.VerticalResolution);
        }
    }
}