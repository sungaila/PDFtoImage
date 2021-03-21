using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Runtime.InteropServices;
using static PDFtoImage.Conversion;

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

        private static void CompareStreams(FileStream expectedStream, MemoryStream outputStream)
        {
#if NETCOREAPP3_0_OR_GREATER
            // the expected images are for Windows
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;
#endif

            Assert.AreEqual(expectedStream.Length, outputStream.Length);

            expectedStream.Position = 0;
            outputStream.Position = 0;

            for (int i = 0; i < expectedStream.Length; i++)
            {
                Assert.AreEqual(expectedStream.ReadByte(), outputStream.ReadByte());
            }
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
        public void SaveBmpPageNumber(int page)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "Wikimedia_Commons_web.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", $"Wikimedia_Commons_web_{page}.bmp"), FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();

            SaveBmp(outputStream, inputStream, page: page, dpi: 40);

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
        public void SavePngPageNumber(int page)
        {
#if !NETCOREAPP3_0_OR_GREATER
            if (page == 4 || page == 13)
                Assert.Inconclusive("Different results for .NET Framework 4.6.1.");
#endif
            using var inputStream = new FileStream(Path.Combine("Assets", "Wikimedia_Commons_web.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", $"Wikimedia_Commons_web_{page}.png"), FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();

            SavePng(outputStream, inputStream, page: page, dpi: 40);

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
        public void SaveGifPageNumber(int page)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "Wikimedia_Commons_web.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", $"Wikimedia_Commons_web_{page}.gif"), FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();

            SaveGif(outputStream, inputStream, page: page, dpi: 40);

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
        public void SaveJpegPageNumber(int page)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "Wikimedia_Commons_web.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", $"Wikimedia_Commons_web_{page}.jpg"), FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();

            SaveJpeg(outputStream, inputStream, page: page, dpi: 40);

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
        public void SaveTiffPageNumber(int page)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "Wikimedia_Commons_web.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", $"Wikimedia_Commons_web_{page}.tif"), FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();

            SaveTiff(outputStream, inputStream, page: page, dpi: 40);

            CompareStreams(expectedStream, outputStream);
        }

        [TestMethod]
        [DataRow(10)]
        [DataRow(30)]
        [DataRow(100)]
        [DataRow(300)]
        [DataRow(600)]
        [DataRow(1200)]
        public void SavePngDpi(int dpi)
        {
            using var pdfStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            using var image = ToImage(pdfStream, dpi: dpi);

            Assert.IsNotNull(image);
            Assert.AreEqual(dpi, image.HorizontalResolution);
            Assert.AreEqual(dpi, image.VerticalResolution);
        }
    }
}