using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Exceptions;
using System.IO;
using static PDFtoImage.Conversion;
using static PDFtoImage.Tests.TestUtils;

namespace PDFtoImage.Tests
{
    [TestClass]
    public class PasswordTests : TestBase
    {
        [TestMethod]
        [DataRow("SocialPreview.pdf", null)]
        [DataRow("SocialPreview.pdf", "")]
        [DataRow("SocialPreview.pdf", "this doc needs no password")]
        [DataRow("SocialPreview with password 123456 (RC4-40).pdf", "123456")]
        [DataRow("SocialPreview with password 123456 (RC4-128).pdf", "123456")]
        [DataRow("SocialPreview with password 123456 (AES-128).pdf", "123456")]
        [DataRow("SocialPreview with password 123456 (AES-256).pdf", "123456")]
        public void WithCorrectPassword(string inputFile, string? password = null)
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", inputFile));
            var output = GetPageCount(inputStream, password: password);
            Assert.AreEqual(1, output, "Page count should be 1, if the password was correct.");
        }

        [TestMethod]
        [DataRow("SocialPreview with password 123456 (RC4-40).pdf", "In noreni per ipe")]
        [DataRow("SocialPreview with password 123456 (RC4-128).pdf", "In noreni cora")]
        [DataRow("SocialPreview with password 123456 (AES-128).pdf", "Tira mine per ito")]
        [DataRow("SocialPreview with password 123456 (AES-256).pdf", "Ne domina")]
        public void ThrowsIncorrectPassword(string inputFile, string? password = null)
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", inputFile));
            Assert.ThrowsException<PdfPasswordProtectedException>(() => GetPageCount(inputStream, password: password));
        }
    }
}