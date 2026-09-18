using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PDFtoImage.Tests
{
    public static class TestUtils
    {
        public static void CompareStreams(string expectedFilePath, Stream outputStream)
        {
            using var expectedStream = GetExpectedStream(expectedFilePath);
            CompareStreams(expectedStream, outputStream);
        }

        public static void CompareStreams(Stream expectedStream, Stream outputStream)
        {
            Assert.IsNotNull(outputStream);
            Assert.AreNotEqual(0, outputStream.Length);
            Assert.AreEqual(expectedStream.Length, outputStream.Length);

            expectedStream.Position = 0;
            outputStream.Position = 0;

            const int BufferSize = 64 * 1024;

            var expectedBuffer = System.Buffers.ArrayPool<byte>.Shared.Rent(BufferSize);
            var outputBuffer = System.Buffers.ArrayPool<byte>.Shared.Rent(BufferSize);

            try
            {
                long position = 0;

                while (position < expectedStream.Length)
                {
                    var count = (int)Math.Min(BufferSize, expectedStream.Length - position);

                    ReadExactly(expectedStream, expectedBuffer, count);
                    ReadExactly(outputStream, outputBuffer, count);

                    if (!expectedBuffer.AsSpan(0, count).SequenceEqual(outputBuffer.AsSpan(0, count)))
                    {
                        for (var i = 0; i < count; i++)
                        {
                            if (expectedBuffer[i] != outputBuffer[i])
                            {
                                Assert.Fail(
                                    $"Streams differ at byte position {position + i}. " +
                                    $"Expected: {expectedBuffer[i]}, Actual: {outputBuffer[i]}.");
                            }
                        }
                    }

                    position += count;
                }
            }
            finally
            {
                System.Buffers.ArrayPool<byte>.Shared.Return(expectedBuffer);
                System.Buffers.ArrayPool<byte>.Shared.Return(outputBuffer);
            }
        }

        private static void ReadExactly(Stream stream, byte[] buffer, int count)
        {
#if NET7_0_OR_GREATER
            stream.ReadExactly(buffer, 0, count);
#else
            var offset = 0;

            while (offset < count)
            {
                var read = stream.Read(
                    buffer,
                    offset,
                    count - offset);

                if (read == 0)
                    throw new EndOfStreamException();

                offset += read;
            }
#endif
        }

        public static void AssertBitmapsEqual(SKBitmap expected, SKBitmap actual)
        {
            Assert.AreEqual(expected.Width, actual.Width);
            Assert.AreEqual(expected.Height, actual.Height);
            Assert.AreEqual(expected.ColorType, actual.ColorType);
            Assert.AreEqual(expected.AlphaType, actual.AlphaType);
            Assert.AreEqual(expected.RowBytes, actual.RowBytes);

            if (!expected.GetPixelSpan().SequenceEqual(actual.GetPixelSpan()))
            {
                Assert.Fail("The bitmap pixel data differs.");
            }
        }

        public static string GetPlatformAsString()
        {
#if NET471_OR_GREATER || NETCOREAPP
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return OSPlatform.Windows.ToString();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return OSPlatform.Linux.ToString();
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return OSPlatform.OSX.ToString();
            }


            throw new PlatformNotSupportedException();
#else
            return Environment.OSVersion.Platform == PlatformID.Win32NT
                ? "WINDOWS"
                : "LINUX";
#endif
        }

        public static FileStream GetInputStream(string filePath)
        {
            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
        }

        public static FileStream GetExpectedStream(string filePath)
        {
            if (!File.Exists(filePath))
                Assert.Inconclusive($"The expected asset '{filePath}' could not be found.");

            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);
        }

#if NET9_0_OR_GREATER
        private static readonly System.Threading.Lock _lockObject = new();
#else
        private static readonly object _lockObject = new();
#endif

        public static Stream CreateOutputStream(string expectedPath)
        {
            if (!TestBase.SaveOutputInGeneratedFolder)
                return new MemoryStream();

            var outputPath = expectedPath.Replace("Expected", "Generated");

            lock (_lockObject)
            {
                if (!File.Exists(outputPath))
                {
                    if (!Directory.Exists(outputPath))
                        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

                    return new FileStream(
                        outputPath,
                        FileMode.CreateNew,
                        FileAccess.ReadWrite,
                        FileShare.None,
                        4096,
                        FileOptions.SequentialScan);
                }
            }

            return new MemoryStream();
        }
    }
}