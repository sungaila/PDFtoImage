using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static PDFtoImage.Conversion;

namespace Tests
{
    [TestClass]
    public class ApiTests
    {
        [TestMethod]
        public void SaveWebpStringNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => SaveWebp((string)null!, (string)null!));
        }

        [TestMethod]
        public void SaveWebpStreamNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => SaveWebp((Stream)null!, (string)null!));
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

        [TestMethod]
        public void GetPageCountPdfStringNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => GetPageCount((string)null!));
        }

        [TestMethod]
        public void GetPageCountPdfArrayNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => GetPageCount((byte[])null!));
        }

        [TestMethod]
        public void GetPageCountPdfStreamNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => GetPageCount((Stream)null!));
        }

        [TestMethod]
        public void ToImagesPdfStringNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => ToImages((string)null!).ToList());
        }

        [TestMethod]
        public void ToImagesPdfArrayNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => ToImages((byte[])null!).ToList());
        }

        [TestMethod]
        public void ToImagesPdfStreamNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => ToImages((Stream)null!).ToList());
        }

#if NETCOREAPP3_0_OR_GREATER
        [TestMethod]
        public async Task ToImagesAsyncPdfStringNullException()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
            {
                await foreach (var page in ToImagesAsync((string)null!))
                {
                }
            });
        }

        [TestMethod]
        public async Task ToImagesAsyncPdfArrayNullException()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
            {
                await foreach (var page in ToImagesAsync((byte[])null!))
                {
                }
            });
        }

        [TestMethod]
        public async Task ToImagesAsyncPdfStreamNullException()
        {
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () =>
            {
                await foreach (var page in ToImagesAsync((Stream)null!))
                {
                }
            });
        }
#endif
    }
}