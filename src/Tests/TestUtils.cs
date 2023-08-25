using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Runtime.InteropServices;

namespace PDFtoImage.Tests
{
    public static class TestUtils
    {
        public static void CompareStreams(FileStream expectedStream, MemoryStream outputStream)
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
    }
}