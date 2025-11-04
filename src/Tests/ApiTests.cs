using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static PDFtoImage.Conversion;
using static PDFtoImage.Tests.TestUtils;

namespace PDFtoImage.Tests
{
    [TestClass]
    public class ApiTests : TestBase
    {
        [TestMethod]
        public void SaveWebpStringNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => SaveWebp((string)null!, (string)null!));
        }

        [TestMethod]
        public void SaveWebpStreamNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => SaveWebp((Stream)null!, (string)null!));
        }

        [TestMethod]
        public void SavePngStringNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => SavePng((string)null!, (string)null!));
        }

        [TestMethod]
        public void SavePngStreamNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => SavePng((Stream)null!, (string)null!));
        }

        [TestMethod]
        public void SaveJpegStringNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => SaveJpeg((string)null!, (string)null!));
        }

        [TestMethod]
        public void SaveJpegStreamNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => SaveJpeg((Stream)null!, (string)null!));
        }

        [TestMethod]
        public void ToImagePdfStringNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => ToImage((string)null!));
        }

        [TestMethod]
        public void ToImagePdfArrayNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => ToImage((byte[])null!));
        }

        [TestMethod]
        public void ToImagePdfStreamNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => ToImage((Stream)null!));
        }

        [TestMethod]
        public void GetPageCountPdfStringNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => GetPageCount((string)null!));
        }

        [TestMethod]
        public void GetPageCountPdfArrayNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => GetPageCount((byte[])null!));
        }

        [TestMethod]
        public void GetPageCountPdfStreamNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => GetPageCount((Stream)null!));
        }

        [TestMethod]
        public void GetPageSizePdfStringNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => GetPageSize((string)null!, 0));
        }

        [TestMethod]
        public void GetPageSizePdfArrayNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => GetPageSize((byte[])null!, 0));
        }

        [TestMethod]
        public void GetPageSizePdfStreamNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => GetPageSize((Stream)null!, page: 0));
        }

        [TestMethod]
        public void GetPageSizesPdfStringNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => GetPageSizes((string)null!));
        }

        [TestMethod]
        public void GetPageSizesPdfArrayNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => GetPageSizes((byte[])null!));
        }

        [TestMethod]
        public void GetPageSizesPdfStreamNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => GetPageSizes((Stream)null!));
        }

        [TestMethod]
        public void ToImagesPdfStringNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => ToImages((string)null!).ToList());
        }

        [TestMethod]
        public void ToImagesPdfArrayNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => ToImages((byte[])null!).ToList());
        }

        [TestMethod]
        public void ToImagesPdfStreamNullException()
        {
            Assert.ThrowsExactly<ArgumentNullException>(() => ToImages((Stream)null!).ToList());
        }

#if NET6_0_OR_GREATER
        [TestMethod]
        public async Task ToImagesAsyncPdfStringNullException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () =>
            {
                await foreach (var page in ToImagesAsync((string)null!, cancellationToken: TestContext!.CancellationToken))
                {
                }
            });
        }

        [TestMethod]
        public async Task ToImagesAsyncPdfArrayNullException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () =>
            {
                await foreach (var page in ToImagesAsync((byte[])null!, cancellationToken: TestContext!.CancellationToken))
                {
                }
            });
        }

        [TestMethod]
        public async Task ToImagesAsyncPdfStreamNullException()
        {
            await Assert.ThrowsExactlyAsync<ArgumentNullException>(async () =>
            {
                await foreach (var page in ToImagesAsync((Stream)null!, cancellationToken: TestContext!.CancellationToken))
                {
                }
            });
        }
#endif

        [TestMethod]
        public void ToImageStreamLeaveOpenDefault()
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
            Assert.IsTrue(inputStream.CanRead);

            ToImage(inputStream);
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling the default overload.");
        }

        [TestMethod]
        public void ToImageStreamLeaveOpenFalse()
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
            Assert.IsTrue(inputStream.CanRead);

            ToImage(inputStream, leaveOpen: false);
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling leaveOpen with false.");
        }

        [TestMethod]
        public void ToImageStreamLeaveOpenTrue()
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
            Assert.IsTrue(inputStream.CanRead);

            ToImage(inputStream, leaveOpen: true);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open when calling leaveOpen with true.");
        }

        [TestMethod]
        public void ToImagesStreamLeaveOpenDefault()
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
            Assert.IsTrue(inputStream.CanRead);

            var result = ToImages(inputStream);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open as long as the iterator is not used yet.");

            foreach (var _ in result) ;
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling the default overload.");
        }

        [TestMethod]
        public void ToImagesStreamLeaveOpenFalse()
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
            Assert.IsTrue(inputStream.CanRead);

            var result = ToImages(inputStream, false);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open as long as the iterator is not used yet.");

            foreach (var _ in result) ;
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling leaveOpen with false.");
        }

        [TestMethod]
        public void ToImagesStreamLeaveOpenTrue()
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
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
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
            Assert.IsTrue(inputStream.CanRead);

            var result = ToImagesAsync(inputStream, cancellationToken: TestContext!.CancellationToken);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open as long as the iterator is not used yet.");

            await foreach (var _ in result) ;
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling the default overload.");
        }

        [TestMethod]
        public async Task ToImagesAsyncStreamLeaveOpenFalse()
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
            Assert.IsTrue(inputStream.CanRead);

            var result = ToImagesAsync(inputStream, false, cancellationToken: TestContext!.CancellationToken);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open as long as the iterator is not used yet.");

            await foreach (var _ in result) ;
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling leaveOpen with false.");
        }

        [TestMethod]
        public async Task ToImagesAsyncStreamLeaveOpenTrue()
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
            Assert.IsTrue(inputStream.CanRead);

            var result = ToImagesAsync(inputStream, true, cancellationToken: TestContext!.CancellationToken);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open as long as the iterator is not used yet.");

            await foreach (var _ in result) ;
            Assert.IsTrue(inputStream.CanRead, "The stream should be open when calling leaveOpen with true.");
        }
#endif

        [TestMethod]
        public void GetPageCountStreamLeaveOpenDefault()
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
            Assert.IsTrue(inputStream.CanRead);

            GetPageCount(inputStream);
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling the default overload.");
        }

        [TestMethod]
        public void GetPageCountStreamLeaveOpenFalse()
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
            Assert.IsTrue(inputStream.CanRead);

            GetPageCount(inputStream, false);
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling leaveOpen with false.");
        }

        [TestMethod]
        public void GetPageCountStreamLeaveOpenTrue()
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
            Assert.IsTrue(inputStream.CanRead);

            GetPageCount(inputStream, true);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open when calling leaveOpen with true.");
        }

        [TestMethod]
        public void GetPageSizeStreamLeaveOpenDefault()
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
            Assert.IsTrue(inputStream.CanRead);

            GetPageSize(inputStream, page: 0);
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling the default overload.");
        }

        [TestMethod]
        public void GetPageSizeStreamLeaveOpenFalse()
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
            Assert.IsTrue(inputStream.CanRead);

            GetPageSize(inputStream, leaveOpen: false);
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling leaveOpen with false.");
        }

        [TestMethod]
        public void GetPageSizeStreamLeaveOpenTrue()
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
            Assert.IsTrue(inputStream.CanRead);

            GetPageSize(inputStream, leaveOpen: true);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open when calling leaveOpen with true.");
        }

        [TestMethod]
        public void GetPageSizesStreamLeaveOpenDefault()
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
            Assert.IsTrue(inputStream.CanRead);

            GetPageSizes(inputStream);
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling the default overload.");
        }

        [TestMethod]
        public void GetPageSizesStreamLeaveOpenFalse()
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
            Assert.IsTrue(inputStream.CanRead);

            GetPageSizes(inputStream, false);
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling leaveOpen with false.");
        }

        [TestMethod]
        public void GetPageSizesStreamLeaveOpenTrue()
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
            Assert.IsTrue(inputStream.CanRead);

            GetPageSizes(inputStream, true);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open when calling leaveOpen with true.");
        }

        [TestMethod]
        public void StreamMultipleCallsLeaveOpen()
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
            Assert.IsTrue(inputStream.CanRead);

            GetPageCount(inputStream, true);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open when calling leaveOpen with true.");

            GetPageSizes(inputStream, true);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open when calling leaveOpen with true.");

            var image1 = ToImage(inputStream, leaveOpen: true);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open when calling leaveOpen with true.");

            var image2 = ToImage(inputStream, leaveOpen: true);
            Assert.IsTrue(inputStream.CanRead, "The stream should be open when calling leaveOpen with true.");

            Assert.IsGreaterThan(0, image1.ByteCount, "The rendered image should have content.");
            Assert.AreEqual(image1.ByteCount, image2.ByteCount, "Both images should be equal (in byte size).");

            GetPageSizes(inputStream, false);
            Assert.IsFalse(inputStream.CanRead, "The stream should be closed when calling leaveOpen with false.");

            Assert.ThrowsExactly<ObjectDisposedException>(() => GetPageCount(inputStream, false), "The stream should be closed and throw an exception.");
        }
    }
}