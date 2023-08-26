using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Tests;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static PDFtoImage.Conversion;

namespace Tests
{
    [TestClass]
    public class ApiTests : TestBase
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
        public void GetPageSizePdfStringNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => GetPageSize((string)null!, 0));
        }

        [TestMethod]
        public void GetPageSizePdfArrayNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => GetPageSize((byte[])null!, 0));
        }

        [TestMethod]
        public void GetPageSizePdfStreamNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => GetPageSize((Stream)null!, 0));
        }

        [TestMethod]
        public void GetPageSizesPdfStringNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => GetPageSizes((string)null!));
        }

        [TestMethod]
        public void GetPageSizesPdfArrayNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => GetPageSizes((byte[])null!));
        }

        [TestMethod]
        public void GetPageSizesPdfStreamNullException()
        {
            Assert.ThrowsException<ArgumentNullException>(() => GetPageSizes((Stream)null!));
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

#if NET6_0_OR_GREATER
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

        [TestMethod]
        public void ToImageStreamLeaveOpenDefault()
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            Assert.IsTrue(inputStream.CanRead);

            ToImage(inputStream);
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling the default overload.");
        }

        [TestMethod]
        public void ToImageStreamLeaveOpenFalse()
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            Assert.IsTrue(inputStream.CanRead);

            ToImage(inputStream, false);
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling leaveOpen with false.");
        }

        [TestMethod]
        public void ToImageStreamLeaveOpenTrue()
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            Assert.IsTrue(inputStream.CanRead);

            ToImage(inputStream, true);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open when calling leaveOpen with true.");
        }

        [TestMethod]
        public void ToImagesStreamLeaveOpenDefault()
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            Assert.IsTrue(inputStream.CanRead);

            var result = ToImages(inputStream);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open as long as the iterator is not used yet.");

            foreach (var _ in result) ;
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling the default overload.");
        }

        [TestMethod]
        public void ToImagesStreamLeaveOpenFalse()
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            Assert.IsTrue(inputStream.CanRead);

            var result = ToImages(inputStream, false);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open as long as the iterator is not used yet.");

            foreach (var _ in result) ;
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling leaveOpen with false.");
        }

        [TestMethod]
        public void ToImagesStreamLeaveOpenTrue()
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            Assert.IsTrue(inputStream.CanRead);

            var result = ToImages(inputStream, true);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open as long as the iterator is not used yet.");

            foreach (var _ in result) ;
            Assert.IsTrue(inputStream.CanRead, "The stream should be open when calling leaveOpen with true.");
        }

#if NET6_0_OR_GREATER
        [TestMethod]
        public async Task ToImagesAsyncStreamLeaveOpenDefault()
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            Assert.IsTrue(inputStream.CanRead);

            var result = ToImagesAsync(inputStream);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open as long as the iterator is not used yet.");

            await foreach (var _ in result) ;
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling the default overload.");
        }

        [TestMethod]
        public async Task ToImagesAsyncStreamLeaveOpenFalse()
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            Assert.IsTrue(inputStream.CanRead);

            var result = ToImagesAsync(inputStream, false);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open as long as the iterator is not used yet.");

            await foreach (var _ in result) ;
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling leaveOpen with false.");
        }

        [TestMethod]
        public async Task ToImagesAsyncStreamLeaveOpenTrue()
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            Assert.IsTrue(inputStream.CanRead);

            var result = ToImagesAsync(inputStream, true);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open as long as the iterator is not used yet.");

            await foreach (var _ in result) ;
            Assert.IsTrue(inputStream.CanRead, "The stream should be open when calling leaveOpen with true.");
        }
#endif

        [TestMethod]
        public void GetPageCountStreamLeaveOpenDefault()
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            Assert.IsTrue(inputStream.CanRead);

            GetPageCount(inputStream);
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling the default overload.");
        }

        [TestMethod]
        public void GetPageCountStreamLeaveOpenFalse()
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            Assert.IsTrue(inputStream.CanRead);

            GetPageCount(inputStream, false);
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling leaveOpen with false.");
        }

        [TestMethod]
        public void GetPageCountStreamLeaveOpenTrue()
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            Assert.IsTrue(inputStream.CanRead);

            GetPageCount(inputStream, true);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open when calling leaveOpen with true.");
        }

        [TestMethod]
        public void GetPageSizeStreamLeaveOpenDefault()
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            Assert.IsTrue(inputStream.CanRead);

            GetPageSize(inputStream, 0);
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling the default overload.");
        }

        [TestMethod]
        public void GetPageSizeStreamLeaveOpenFalse()
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            Assert.IsTrue(inputStream.CanRead);

            GetPageSize(inputStream, false, 0);
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling leaveOpen with false.");
        }

        [TestMethod]
        public void GetPageSizeStreamLeaveOpenTrue()
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            Assert.IsTrue(inputStream.CanRead);

            GetPageSize(inputStream, true, 0);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open when calling leaveOpen with true.");
        }

        [TestMethod]
        public void GetPageSizesStreamLeaveOpenDefault()
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            Assert.IsTrue(inputStream.CanRead);

            GetPageSizes(inputStream);
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling the default overload.");
        }

        [TestMethod]
        public void GetPageSizesStreamLeaveOpenFalse()
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            Assert.IsTrue(inputStream.CanRead);

            GetPageSizes(inputStream, false);
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling leaveOpen with false.");
        }

        [TestMethod]
        public void GetPageSizesStreamLeaveOpenTrue()
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            Assert.IsTrue(inputStream.CanRead);

            GetPageSizes(inputStream, true);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open when calling leaveOpen with true.");
        }

        [TestMethod]
        public void StreamMultipleCallsLeaveOpen()
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);
            Assert.IsTrue(inputStream.CanRead);

            GetPageCount(inputStream, true);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open when calling leaveOpen with true.");

            GetPageSizes(inputStream, true);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open when calling leaveOpen with true.");

            var image1 = ToImage(inputStream, true);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open when calling leaveOpen with true.");

            var image2 = ToImage(inputStream, true);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open when calling leaveOpen with true.");

            Assert.IsTrue(image1.ByteCount > 0, "The rendered image should have content.");
            Assert.AreEqual(image1.ByteCount, image2.ByteCount, "Both images should be equal (in byte size).");

            GetPageSizes(inputStream, false);
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling leaveOpen with false.");

            Assert.ThrowsException<ObjectDisposedException>(() => GetPageCount(inputStream, false), "The stream should be closed and throw an exception.");
        }
    }
}