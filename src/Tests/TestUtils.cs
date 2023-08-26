using Microsoft.VisualStudio.TestTools.UnitTesting;
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

            for (int i = 0; i < expectedStream.Length; i++)
            {
                Assert.AreEqual(expectedStream.ReadByte(), outputStream.ReadByte());
            }
        }

        public static string GetPlatformAsString()
        {
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
        }

        public static FileStream GetInputStream(string filePath)
        {
            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        public static FileStream GetExpectedStream(string filePath)
        {
            if (!File.Exists(filePath))
                Assert.Inconclusive("The expected asset '{0}' could not be found.", filePath);

            return new FileStream(filePath, FileMode.Open, FileAccess.Read);
        }

        private static readonly object _lockObject = new();

        public static Stream CreateOutputStream(string expectedPath)
        {
            var outputPath = expectedPath.Replace("Expected", "Generated");

            lock (_lockObject)
            {
                if (!File.Exists(outputPath) && TestBase.SaveOutputInGeneratedFolder)
                {
                    if (!Directory.Exists(outputPath))
                        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

                    return new FileStream(
                        outputPath,
                        FileMode.Create,
                        FileAccess.ReadWrite,
                        FileShare.Read,
                        4096,
                        FileOptions.SequentialScan);
                }
            }

            return new MemoryStream();
        }
    }
}