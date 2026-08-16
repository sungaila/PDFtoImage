using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Exceptions;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static PDFtoImage.Tests.TestUtils;

namespace PDFtoImage.Tests
{
    [TestClass]
    public class AvailabilityTests : TestBase
    {
        [TestMethod]
        public void GetPageCountHandlesPartialStreamReads()
        {
            using var inputStream = new PartialReadStream(ReadAsset("SocialPreview.pdf"), maxReadSize: 7);

            var pageCount = Conversion.GetPageCount(inputStream, leaveOpen: true);

            Assert.AreEqual(1, pageCount);
            Assert.IsTrue(inputStream.CanRead);
        }

        [TestMethod]
        public void ToImageHandlesPartialStreamReads()
        {
            using var inputStream = new PartialReadStream(ReadAsset("SocialPreview.pdf"), maxReadSize: 7);

            using var bitmap = Conversion.ToImage(inputStream, leaveOpen: true);

            Assert.IsGreaterThan(0, bitmap.ByteCount);
            Assert.IsTrue(inputStream.CanRead);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void StreamReadFailureHonorsLeaveOpen(bool leaveOpen)
        {
            using var inputStream = new FailingReadStream(ReadAsset("SocialPreview.pdf"));

            AssertPdfException(() => Conversion.GetPageCount(inputStream, leaveOpen: leaveOpen));
            Assert.AreEqual(leaveOpen, inputStream.CanRead, "The stream state should match leaveOpen after a file-access callback fails.");
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void NonSeekableStreamHonorsLeaveOpen(bool leaveOpen)
        {
            using var inputStream = new NonSeekableStream(ReadAsset("SocialPreview.pdf"));

            Assert.ThrowsExactly<ArgumentException>(() => Conversion.GetPageCount(inputStream, leaveOpen: leaveOpen));
            Assert.AreEqual(leaveOpen, inputStream.CanRead, "The stream state should match leaveOpen when stream validation fails.");
        }

        [TestMethod]
        public void SelectedPagesDisposeOwnedStreamOnce()
        {
            using var inputStream = new DisposeCountingStream(ReadAsset("SocialPreview.pdf"));

            var bitmaps = Conversion.ToImages(inputStream, pages: [0], leaveOpen: false).ToList();

            try
            {
                Assert.AreEqual(1, inputStream.DisposeCount, "A selected-page conversion should own only one PDF document over the stream.");
            }
            finally
            {
                bitmaps.ForEach(bitmap => bitmap.Dispose());
            }
        }

        [TestMethod]
        public void PageRangeDisposesOwnedStreamOnce()
        {
            using var inputStream = new DisposeCountingStream(ReadAsset("SocialPreview.pdf"));

            var bitmaps = Conversion.ToImages(inputStream, pages: 0..1, leaveOpen: false).ToList();

            try
            {
                Assert.AreEqual(1, inputStream.DisposeCount, "A page-range conversion should own only one PDF document over the stream.");
            }
            finally
            {
                bitmaps.ForEach(bitmap => bitmap.Dispose());
            }
        }

#if NET6_0_OR_GREATER
        [TestMethod]
        public async Task SelectedPagesAsyncDisposeOwnedStreamOnce()
        {
            using var inputStream = new DisposeCountingStream(ReadAsset("SocialPreview.pdf"));

            await foreach (var bitmap in Conversion.ToImagesAsync(inputStream, pages: [0], leaveOpen: false, cancellationToken: TestContext!.CancellationToken))
                bitmap.Dispose();

            Assert.AreEqual(1, inputStream.DisposeCount, "An async selected-page conversion should own only one PDF document over the stream.");
        }
#endif

        [TestMethod]
        public void FailedOpenDoesNotAffectNextDocument()
        {
            using (var failingStream = new FailingReadStream(ReadAsset("SocialPreview.pdf")))
                AssertPdfException(() => Conversion.GetPageCount(failingStream));

            using var validStream = GetInputStream(Path.Combine("..", "Assets", "SocialPreview.pdf"));
            Assert.AreEqual(1, Conversion.GetPageCount(validStream));
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void NonReadableStreamHonorsLeaveOpen(bool leaveOpen)
        {
            using var inputStream = new CapabilityStream(canRead: false, canSeek: true);

            var exception = Assert.ThrowsExactly<ArgumentException>(() => Conversion.GetPageCount(inputStream, leaveOpen: leaveOpen));

            Assert.AreEqual("stream", exception.ParamName);
            Assert.AreEqual(leaveOpen, inputStream.IsOpen, "The stream state should match leaveOpen after capability validation fails.");
            Assert.AreEqual(0, inputStream.LengthReadCount, "Length should not be queried for a non-readable stream.");
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void NonSeekableStreamHonorsLeaveOpen(bool leaveOpen)
        {
            using var inputStream = new CapabilityStream(canRead: true, canSeek: false);

            var exception = Assert.ThrowsExactly<ArgumentException>(() => Conversion.GetPageCount(inputStream, leaveOpen: leaveOpen));

            Assert.AreEqual("stream", exception.ParamName);
            Assert.AreEqual(leaveOpen, inputStream.IsOpen, "The stream state should match leaveOpen after capability validation fails.");
            Assert.AreEqual(0, inputStream.LengthReadCount, "Length should not be queried for a non-seekable stream.");
        }

        [TestMethod]
        public void StreamLengthIsReadOnlyOnceWhenOpeningDocument()
        {
            using var inputStream = new LengthCountingStream(ReadAsset("SocialPreview.pdf"));

            Assert.AreEqual(1, Conversion.GetPageCount(inputStream, leaveOpen: true));
            Assert.AreEqual(1, inputStream.LengthReadCount, "The stream length should be captured once and reused for FPDF_FILEACCESS.");
        }

        private static byte[] ReadAsset(string fileName)
        {
            using var inputStream = GetInputStream(Path.Combine("..", "Assets", fileName));
            using var buffer = new MemoryStream();
            inputStream.CopyTo(buffer);
            return buffer.ToArray();
        }

        private static void AssertPdfException(Action action)
        {
            try
            {
                action();
                Assert.Fail("Expected a PDF exception.");
            }
            catch (PdfException)
            {
            }
        }

        private sealed class PartialReadStream(byte[] buffer, int maxReadSize) : MemoryStream(buffer, writable: false)
        {
            private readonly int _maxReadSize = maxReadSize;

            public override int Read(byte[] buffer, int offset, int count)
                => base.Read(buffer, offset, Math.Min(count, _maxReadSize));
        }

        private sealed class NonSeekableStream(byte[] buffer) : MemoryStream(buffer, writable: false)
        {
            public override bool CanSeek => false;

            public override long Length => throw new NotSupportedException();
        }

        private sealed class DisposeCountingStream(byte[] buffer) : MemoryStream(buffer, writable: false)
        {
            public int DisposeCount { get; private set; }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    DisposeCount++;

                base.Dispose(disposing);
            }
        }

        private sealed class FailingReadStream(byte[] buffer) : MemoryStream(buffer, writable: false)
        {
            public override int Read(byte[] buffer, int offset, int count)
                => throw new IOException("Simulated stream read failure.");
        }

        private sealed class LengthCountingStream(byte[] buffer) : MemoryStream(buffer, writable: false)
        {
            public int LengthReadCount { get; private set; }

            public override long Length
            {
                get
                {
                    LengthReadCount++;
                    return base.Length;
                }
            }
        }

        private sealed class CapabilityStream(bool canRead, bool canSeek) : Stream
        {
            public bool IsOpen { get; private set; } = true;

            public int LengthReadCount { get; private set; }

            public override bool CanRead => IsOpen && canRead;

            public override bool CanSeek => IsOpen && canSeek;

            public override bool CanWrite => false;

            public override long Length
            {
                get
                {
                    LengthReadCount++;
                    throw new InvalidOperationException("Length must not be queried before stream capability validation.");
                }
            }

            public override long Position
            {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }

            public override void Flush() => throw new NotSupportedException();

            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value) => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    IsOpen = false;

                base.Dispose(disposing);
            }
        }
    }
}