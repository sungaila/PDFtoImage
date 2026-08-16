using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace PDFtoImage.Tests
{
    [TestClass]
    public class CppUnsignedLongTests : TestBase
    {
        private const long Exactly4GiB = 1L << 32;
        private const long LargePdfXrefOffset = (long)uint.MaxValue + 4096;

        private static unsafe void MarkSparse(FileStream stream)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                return;

            uint bytesReturned;

            if (!PInvoke.DeviceIoControl(
                (HANDLE)stream.SafeFileHandle.DangerousGetHandle(),
                PInvoke.FSCTL_SET_SPARSE,
                null,
                0,
                null,
                0,
                &bytesReturned,
                null))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        private static FileStream CreatePdfWithXrefOffset(long xrefOffset, out string path)
        {
            var stream = CreateTemporaryFile(out path);

            try
            {
                var offsets = WritePdfObjects(stream);

                stream.Position = xrefOffset;
                WriteAscii(stream, CreateXrefAndTrailer(xrefOffset, offsets));

                stream.Flush();
                stream.Position = 0;

                return stream;
            }
            catch
            {
                stream.Dispose();
                File.Delete(path);
                throw;
            }
        }

        private static FileStream CreatePdfWithExactLength(long targetLength, out string path)
        {
            var stream = CreateTemporaryFile(out path);

            try
            {
                var offsets = WritePdfObjects(stream);

                // startxref is part of the trailer, so the trailer length depends on the
                // decimal representation of xrefOffset. Iterate until the computed offset
                // makes the file end at exactly targetLength.
                var xrefOffset = targetLength - 256;

                while (true)
                {
                    var trailer = CreateXrefAndTrailer(xrefOffset, offsets);
                    var nextXrefOffset = targetLength - Encoding.ASCII.GetByteCount(trailer);

                    if (nextXrefOffset == xrefOffset)
                    {
                        stream.Position = xrefOffset;
                        WriteAscii(stream, trailer);
                        break;
                    }

                    xrefOffset = nextXrefOffset;
                }

                stream.Flush();

                Assert.AreEqual(targetLength, stream.Length, "The generated PDF must have exactly the requested logical file length.");

                stream.Position = 0;

                return stream;
            }
            catch
            {
                stream.Dispose();
                File.Delete(path);
                throw;
            }
        }

        private static FileStream CreateTemporaryFile(out string path)
        {
            path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.pdf");

            var stream = new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None);

            // Windows requires an explicit request for sparse-file behavior.
            // Keep it best-effort: if the temporary file system rejects FSCTL_SET_SPARSE,
            // continue with the regular FileStream as requested.
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                try
                {
                    MarkSparse(stream);
                }
                catch (Win32Exception)
                {
                    // Ignore failures to mark the file as sparse.
                    // The test will still run, but may take longer and use more disk space.
                }
            }

            // On Unix no platform-specific sparse-file API is needed here. Seeking beyond
            // EOF and subsequently writing creates the logical zero-filled gap; file systems
            // such as ext4 and APFS store such gaps sparsely.
            return stream;
        }

        private static long[] WritePdfObjects(Stream stream)
        {
            var offsets = new long[5];

            WriteAscii(stream, "%PDF-1.4\n");

            offsets[1] = stream.Position;
            WriteAscii(stream,
                "1 0 obj\n" +
                "<< /Type /Catalog /Pages 2 0 R >>\n" +
                "endobj\n");

            offsets[2] = stream.Position;
            WriteAscii(stream,
                "2 0 obj\n" +
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>\n" +
                "endobj\n");

            offsets[3] = stream.Position;
            WriteAscii(stream,
                "3 0 obj\n" +
                "<< /Type /Page /Parent 2 0 R " +
                "/MediaBox [0 0 72 72] " +
                "/Resources << >> " +
                "/Contents 4 0 R >>\n" +
                "endobj\n");

            offsets[4] = stream.Position;
            WriteAscii(stream,
                "4 0 obj\n" +
                "<< /Length 0 >>\n" +
                "stream\n" +
                "endstream\n" +
                "endobj\n");

            return offsets;
        }

        private static string CreateXrefAndTrailer(long xrefOffset, long[] offsets) =>
            "xref\n" +
            "0 5\n" +
            "0000000000 65535 f \n" +
            $"{offsets[1]:D10} 00000 n \n" +
            $"{offsets[2]:D10} 00000 n \n" +
            $"{offsets[3]:D10} 00000 n \n" +
            $"{offsets[4]:D10} 00000 n \n" +
            "trailer\n" +
            "<< /Size 5 /Root 1 0 R >>\n" +
            "startxref\n" +
            $"{xrefOffset}\n" +
            "%%EOF\n";

        private static void WriteAscii(Stream stream, string value)
        {
            var bytes = Encoding.ASCII.GetBytes(value);
            stream.Write(bytes, 0, bytes.Length);
        }

        private static bool IsAboveUInt32Unsupported
        {
            get
            {
#if !NET6_0_OR_GREATER
                return true;
#else
                return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || IntPtr.Size == 4;
#endif
            }
        }

        private sealed class LargeLengthStream : Stream
        {
            private bool _disposed;

            public override bool CanRead => !_disposed;
            public override bool CanSeek => !_disposed;
            public override bool CanWrite => false;
            public override long Length => Exactly4GiB;

            public override long Position
            {
                get => 0;
                set => throw new NotSupportedException();
            }

            public override void Flush() => throw new NotSupportedException();

            public override int Read(byte[] buffer, int offset, int count)
                => throw new NotSupportedException();

            public override long Seek(long offset, SeekOrigin origin)
                => throw new NotSupportedException();

            public override void SetLength(long value)
                => throw new NotSupportedException();

            public override void Write(byte[] buffer, int offset, int count)
                => throw new NotSupportedException();

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    _disposed = true;

                base.Dispose(disposing);
            }
        }

        private static void AssertRenders(FileStream stream)
        {
            Assert.AreEqual(1, Conversion.GetPageCount(stream, leaveOpen: true));

            stream.Position = 0;

            using var bitmap = Conversion.ToImage(stream, leaveOpen: true);

            Assert.IsGreaterThan(1, (long)bitmap.Width * bitmap.Height, $"Expected more than one rendered pixel, got {bitmap.Width}x{bitmap.Height} instead.");
        }

        private static void AssertUnsupportedAboveUInt32(FileStream stream)
        {
#if !NET6_0_OR_GREATER
            Assert.Throws<NotSupportedException>(() => Conversion.GetPageCount(stream, leaveOpen: true));
            return;
#else
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) || IntPtr.Size == 4)
            {
                Assert.Throws<NotSupportedException>(() => Conversion.GetPageCount(stream, leaveOpen: true));
                return;
            }

            AssertRenders(stream);
#endif
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void UnsupportedLargeStreamHonorsLeaveOpen(bool leaveOpen)
        {
            if (!IsAboveUInt32Unsupported)
                return;

            using var stream = new LargeLengthStream();

            Assert.ThrowsExactly<NotSupportedException>(() => Conversion.GetPageCount(stream, leaveOpen: leaveOpen));
            Assert.AreEqual(leaveOpen, stream.CanRead, "The stream state should match leaveOpen when the platform size guard rejects the PDF.");
        }

        [TestMethod]
        public void PdfStreamAtMaximumUInt32Length()
        {
            // uint.MaxValue is the largest file length representable by the 32-bit
            // unsigned long used by FPDF_FILEACCESS on Windows and other 32-bit ABIs.
            // It is exactly one byte smaller than 4 GiB.
            var stream = CreatePdfWithExactLength(uint.MaxValue, out var path);

            try
            {
                Assert.AreEqual((long)uint.MaxValue, stream.Length);
                AssertRenders(stream);
            }
            finally
            {
                stream.Dispose();
                File.Delete(path);
            }
        }

        [TestMethod]
        public void PdfStreamExactly4GiB()
        {
            // Exactly 4 GiB is 2^32 bytes and therefore already one byte beyond the
            // maximum value representable by a 32-bit unsigned long. It cannot work
            // through FPDF_FILEACCESS on Windows/32-bit ABIs, but must work on modern
            // .NET with a 64-bit Unix C unsigned long.
            var stream = CreatePdfWithExactLength(Exactly4GiB, out var path);

            try
            {
                Assert.AreEqual(Exactly4GiB, stream.Length);
                AssertUnsupportedAboveUInt32(stream);
            }
            finally
            {
                stream.Dispose();
                File.Delete(path);
            }
        }

        [TestMethod]
        public void PdfStreamLargerThan4GiB()
        {
            // Place xref itself beyond uint.MaxValue so a successful 64-bit Unix run
            // proves that the FPDF_FILEACCESS callback receives and uses a >32-bit position.
            var stream = CreatePdfWithXrefOffset(LargePdfXrefOffset, out var path);

            try
            {
                Assert.IsGreaterThan(uint.MaxValue, stream.Length, "The generated PDF must be logically larger than 4 GiB.");
                AssertUnsupportedAboveUInt32(stream);
            }
            finally
            {
                stream.Dispose();
                File.Delete(path);
            }
        }
    }
}