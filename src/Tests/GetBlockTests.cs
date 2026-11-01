using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Internals;
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PDFtoImage.Tests
{
    [TestClass]
    public class GetBlockTests : TestBase
    {
        [TestMethod]
        [DataRow(1, int.MaxValue)]
        [DataRow(NativeMethods.RentedBufferSize - 1, int.MaxValue)]
        [DataRow(NativeMethods.RentedBufferSize, int.MaxValue)]
        [DataRow(NativeMethods.RentedBufferSize + 1, int.MaxValue)]
        [DataRow(3 * NativeMethods.RentedBufferSize + 17, 137)]
        public void CopiesRequestedRangeWithBoundedBuffer(int count, int maxReadSize)
        {
            const int position = 29;
            var data = new byte[position + count + 1];
            new Random(42).NextBytes(data);
            using var stream = new TrackingReadStream(data, maxReadSize);
            var id = StreamManager.Register(stream);
            var destination = Marshal.AllocHGlobal(count + 2);

            try
            {
                Marshal.WriteByte(destination, 0, 0xA5);
                Marshal.WriteByte(destination, count + 1, 0xA5);

                Assert.AreEqual(1, GetBlock(id, position, IntPtr.Add(destination, 1), count));

                var actual = new byte[count];
                Marshal.Copy(IntPtr.Add(destination, 1), actual, 0, count);
                Assert.AreSequenceEqual(data.AsSpan(position, count).ToArray(), actual);
                Assert.AreEqual((byte)0xA5, Marshal.ReadByte(destination, 0));
                Assert.AreEqual((byte)0xA5, Marshal.ReadByte(destination, count + 1));
                Assert.AreEqual((long)position + count, stream.Position);
                AssertBoundedReads(stream);
            }
            finally
            {
                Marshal.FreeHGlobal(destination);
                StreamManager.Unregister(id);
            }
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void IncompleteReadReturnsFailureAndAllowsNextRequest(bool throwAtEnd)
        {
            using var stream = new TrackingReadStream(new byte[NativeMethods.RentedBufferSize + 137], 137, throwAtEnd);
            var id = StreamManager.Register(stream);
            var destination = Marshal.AllocHGlobal(3 * NativeMethods.RentedBufferSize);

            try
            {
                Assert.AreEqual(0, GetBlock(id, 0, destination, 3 * NativeMethods.RentedBufferSize));
                Assert.AreEqual(stream.Length, stream.Position, "Failure must occur after reading the available data.");
                Assert.AreEqual(1, GetBlock(id, 29, destination, NativeMethods.RentedBufferSize));
                AssertBoundedReads(stream);
            }
            finally
            {
                Marshal.FreeHGlobal(destination);
                StreamManager.Unregister(id);
            }
        }

        [TestMethod]
        [DataRow(int.MaxValue)]
        [DataRow(137)]
        public void BoundedReadsPreservePageSizesAndPixels(int maxReadSize)
        {
            var pdf = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "hundesteuer-anmeldung.pdf"));
            using var stream = new TrackingReadStream(pdf, maxReadSize);

            Assert.AreSequenceEqual(Conversion.GetPageSizes(pdf), Conversion.GetPageSizes(stream, leaveOpen: true));

            var options = new RenderOptions(Dpi: 40);
            using var expected = Conversion.ToImage(pdf, options: options);
            using var actual = Conversion.ToImage(stream, leaveOpen: true, options: options);
            TestUtils.AssertBitmapsEqual(expected, actual);
            AssertBoundedReads(stream);
        }

        private static void AssertBoundedReads(TrackingReadStream stream)
        {
            Assert.IsGreaterThan(0, stream.MaximumBufferLength);
            Assert.IsLessThanOrEqualTo(NativeMethods.RentedBufferSize, stream.MaximumBufferLength, "The pooled buffer must not grow with the native request size.");
            Assert.IsLessThanOrEqualTo(NativeMethods.RentedBufferSize, stream.MaximumRequestedCount, "Each stream read must be bounded.");
        }

        private static unsafe int GetBlock(int id, int position, IntPtr buffer, int size)
        {
            var method = typeof(NativeMethods).GetMethod("FPDF_GetBlock", BindingFlags.Static | BindingFlags.NonPublic)!;
#if NET6_0_OR_GREATER
            // UnmanagedCallersOnly callbacks must be invoked through their unmanaged entry point.
            var callback = (delegate* unmanaged[Cdecl]<IntPtr, CULong, IntPtr, CULong, int>)method.MethodHandle.GetFunctionPointer();
            return callback((IntPtr)id, new CULong((nuint)position), buffer, new CULong((nuint)size));
#else
            return (int)method.Invoke(null, [(IntPtr)id, new UIntPtr((uint)position), buffer, new UIntPtr((uint)size)])!;
#endif
        }

        private sealed class TrackingReadStream(byte[] data, int maxReadSize, bool throwAtEnd = false) : MemoryStream(data, writable: false)
        {
            public int MaximumBufferLength { get; private set; }
            public int MaximumRequestedCount { get; private set; }

            public override int Read(byte[] buffer, int offset, int count)
            {
                MaximumBufferLength = Math.Max(MaximumBufferLength, buffer.Length);
                MaximumRequestedCount = Math.Max(MaximumRequestedCount, count);

                if (throwAtEnd && Position == Length)
                    throw new IOException("Simulated stream read failure after partial data.");

                return base.Read(buffer, offset, Math.Min(count, maxReadSize));
            }
        }
    }
}