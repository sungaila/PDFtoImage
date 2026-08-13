using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Exceptions;
using System.IO;
using System.Linq;
using static PDFtoImage.Tests.TestUtils;

namespace PDFtoImage.Tests
{
    [TestClass]
    public class UnreachablePageTests : TestBase
    {
        // hundesteuer-anmeldung.pdf with object streams expanded and relinearized
        // (qpdf --object-streams=disable --linearize), then /Kids [ 943 0 R 1 0 R 12 0 R ]
        // rewritten to [ 943 0 R 9 0 R 12 0 R ]: same length, so every xref offset stays
        // valid and object 9 is not a page.
        private const string UnreachablePage = "hundesteuer-anmeldung (unreachable page 2).pdf";

        private static Stream OpenAsset() => GetInputStream(Path.Combine("..", "Assets", UnreachablePage));

        [TestMethod]
        public void GetPageCount()
        {
            using var inputStream = OpenAsset();

            Assert.AreEqual(3, Conversion.GetPageCount(inputStream), "Expected and actual PDF page count differs.");
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(2)]
        public void GetPageSize(int page)
        {
            using var inputStream = OpenAsset();

            var result = Conversion.GetPageSize(inputStream, page: page);

            Assert.AreEqual(595.56f, result.Width, 0.0001f, "Expected and actual PDF page width differs.");
            Assert.AreEqual(842.04f, result.Height, 0.0001f, "Expected and actual PDF page height differs.");
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(2)]
        public void ToImage(int page)
        {
            using var inputStream = OpenAsset();

            using var bitmap = Conversion.ToImage(inputStream, page: page);

            Assert.IsNotNull(bitmap);
        }

        [TestMethod]
        public void ToImages()
        {
            using var inputStream = OpenAsset();

            var bitmaps = Conversion.ToImages(inputStream, pages: [0, 2]).ToList();

            try
            {
                Assert.AreEqual(2, bitmaps.Count, "Expected and actual rendered page count differs.");
            }
            finally
            {
                bitmaps.ForEach(bitmap => bitmap.Dispose());
            }
        }

        [TestMethod]
        public void GetPageSizeThrowsPageNotFound()
        {
            using var inputStream = OpenAsset();

            Assert.ThrowsExactly<PdfPageNotFoundException>(() => Conversion.GetPageSize(inputStream, page: 1));
        }

        [TestMethod]
        public void ToImageThrowsPageNotFound()
        {
            using var inputStream = OpenAsset();

            Assert.ThrowsExactly<PdfPageNotFoundException>(() => Conversion.ToImage(inputStream, page: 1));
        }

        [TestMethod]
        public void GetPageSizesThrowsPageNotFound()
        {
            using var inputStream = OpenAsset();

            Assert.ThrowsExactly<PdfPageNotFoundException>(() => Conversion.GetPageSizes(inputStream));
        }

        [TestMethod]
        public void ToImagesThrowsPageNotFound()
        {
            using var inputStream = OpenAsset();

            using var pages = Conversion.ToImages(inputStream, pages: [0, 1], leaveOpen: true).GetEnumerator();

            Assert.IsTrue(pages.MoveNext(), "The page before the unreachable one should still be rendered.");
            pages.Current.Dispose();

            Assert.ThrowsExactly<PdfPageNotFoundException>(() => pages.MoveNext());
        }
    }
}
