using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using static PDFtoImage.Conversion;

namespace Tests
{
    [TestClass]
    public class ApiTests
    {
        [TestMethod]
        public void SaveBmpStringNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => SaveBmp((string)null!, (string)null!));
        }

        [TestMethod]
        public void SaveBmpStreamNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => SaveBmp((Stream)null!, (string)null!));
        }

        [TestMethod]
        public void SavePngStringNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => SavePng((string)null!, (string)null!));
        }

        [TestMethod]
        public void SavePngStreamNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => SavePng((Stream)null!, (string)null!));
        }

        [TestMethod]
        public void SaveGifStringNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => SaveGif((string)null!, (string)null!));
        }

        [TestMethod]
        public void SaveGifStreamNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => SaveGif((Stream)null!, (string)null!));
        }

        [TestMethod]
        public void SaveJpegStringNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => SaveJpeg((string)null!, (string)null!));
        }

        [TestMethod]
        public void SaveJpegStreamNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => SaveJpeg((Stream)null!, (string)null!));
        }

        [TestMethod]
        public void SaveTiffStringNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => SaveTiff((string)null!, (string)null!));
        }

        [TestMethod]
        public void SaveTiffStreamNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => SaveTiff((Stream)null!, (string)null!));
        }

        [TestMethod]
        public void ToImagePdfStringNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => ToImage((string)null!));
        }

        [TestMethod]
        public void ToImagePdfArrayNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => ToImage((byte[])null!));
        }

        [TestMethod]
        public void ToImagePdfStreamNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => ToImage((Stream)null!));
        }
    }
}