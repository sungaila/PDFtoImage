using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Tests
{
    [TestClass]
    public class QueryTests
    {
        [TestInitialize]
        public void Initialize()
        {
#if NET6_0_OR_GREATER
            if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
                Assert.Inconclusive("This test must run on Windows, Linux or macOS.");
#endif
        }

        [TestMethod]
        [DataRow("hundesteuer-anmeldung.pdf", 3)]
        [DataRow("SocialPreview.pdf", 1)]
        [DataRow("Wikimedia_Commons_web.pdf", 20)]
        public void GetPageCount(string pdfFileName, int expectedPageCount)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", pdfFileName), FileMode.Open, FileAccess.Read);

            Assert.AreEqual(expectedPageCount, PDFtoImage.Conversion.GetPageCount(inputStream), "Expected and actual PDF page count differs.");
        }

        [TestMethod]
        [DataRow("hundesteuer-anmeldung.pdf", 0, 595.56f, 842.04f)]
        [DataRow("hundesteuer-anmeldung.pdf", 1, 595.56f, 842.04f)]
        [DataRow("hundesteuer-anmeldung.pdf", 2, 595.56f, 842.04f)]
        [DataRow("SocialPreview.pdf", 0, 1280f, 640f)]
        [DataRow("Wikimedia_Commons_web.pdf", 0, 419.528f, 595.276f)]
        [DataRow("Wikimedia_Commons_web.pdf", 1, 419.528f, 595.276f)]
        [DataRow("Wikimedia_Commons_web.pdf", 2, 419.528f, 595.276f)]
        [DataRow("Wikimedia_Commons_web.pdf", 3, 419.528f, 595.276f)]
        [DataRow("Wikimedia_Commons_web.pdf", 4, 419.528f, 595.276f)]
        [DataRow("Wikimedia_Commons_web.pdf", 5, 419.528f, 595.276f)]
        [DataRow("Wikimedia_Commons_web.pdf", 6, 419.528f, 595.276f)]
        [DataRow("Wikimedia_Commons_web.pdf", 7, 419.528f, 595.276f)]
        [DataRow("Wikimedia_Commons_web.pdf", 8, 419.528f, 595.276f)]
        [DataRow("Wikimedia_Commons_web.pdf", 9, 419.528f, 595.276f)]
        [DataRow("Wikimedia_Commons_web.pdf", 10, 419.528f, 595.276f)]
        [DataRow("Wikimedia_Commons_web.pdf", 11, 419.528f, 595.276f)]
        [DataRow("Wikimedia_Commons_web.pdf", 12, 419.528f, 595.276f)]
        [DataRow("Wikimedia_Commons_web.pdf", 13, 419.528f, 595.276f)]
        [DataRow("Wikimedia_Commons_web.pdf", 14, 419.528f, 595.276f)]
        [DataRow("Wikimedia_Commons_web.pdf", 15, 419.528f, 595.276f)]
        [DataRow("Wikimedia_Commons_web.pdf", 16, 419.528f, 595.276f)]
        [DataRow("Wikimedia_Commons_web.pdf", 17, 419.528f, 595.276f)]
        [DataRow("Wikimedia_Commons_web.pdf", 18, 419.528f, 595.276f)]
        [DataRow("Wikimedia_Commons_web.pdf", 19, 419.528f, 595.276f)]
        public void GetPageSize(string pdfFileName, int page, float expectedPageWidth, float expectedPageHeight)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", pdfFileName), FileMode.Open, FileAccess.Read);

            var result = PDFtoImage.Conversion.GetPageSize(inputStream, page);

            Assert.AreEqual(expectedPageWidth, result.Width, 0.0001f, "Expected and actual PDF page width differs.");
            Assert.AreEqual(expectedPageHeight, result.Height, 0.0001f, "Expected and actual PDF page height differs.");
        }

        [TestMethod]
        [DataRow("hundesteuer-anmeldung.pdf", 3)]
        [DataRow("SocialPreview.pdf", 1)]
        [DataRow("Wikimedia_Commons_web.pdf", 20)]
        public void GetPageSizes(string pdfFileName, int expectedSizeCount)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", pdfFileName), FileMode.Open, FileAccess.Read);

            var result = PDFtoImage.Conversion.GetPageSizes(inputStream);

            foreach (var size in result)
            {
                Assert.IsFalse(size.IsEmpty, "PDF page size cannot be empty.");
            }

            Assert.AreEqual(expectedSizeCount, result.Count, "Expected and actual PDF size count differs.");
        }
    }
}