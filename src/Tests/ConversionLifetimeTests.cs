using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Exceptions;
using PDFtoImage.Internals;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Tests
{
    [TestClass, DoNotParallelize]
    public sealed class ConversionLifetimeTests : TestBase
    {
        private static readonly byte[] Pdf = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "SocialPreview.pdf"));
        private const BindingFlags Fields = BindingFlags.NonPublic | BindingFlags.Instance;

        private sealed class TrackingStream(byte[] bytes) : MemoryStream(bytes, writable: false)
        {
            internal int DisposeCount;
            internal Action? OnDispose;
            internal Action? OnLength { get; set; }

            public override long Length
            {
                get
                {
                    OnLength?.Invoke();
                    return base.Length;
                }
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                if (disposing)
                {
                    DisposeCount++;
                    OnDispose?.Invoke();
                }
            }
        }

        private static void AssertFileReleased(PdfFile file)
        {
            var id = (int)typeof(PdfFile).GetField("_id", Fields)!.GetValue(file)!;
            Assert.IsNull(StreamManager.Get(id));
            foreach (var name in new[] { "_document", "_form", "_formFillInfoPtr", "_avail", "_fileAccessState" })
                Assert.AreEqual(IntPtr.Zero, (IntPtr)typeof(PdfFile).GetField(name, Fields)!.GetValue(file)!);
            Assert.IsNull(typeof(PdfFile).GetField("_stream", Fields)!.GetValue(file));
            Assert.ThrowsExactly<ObjectDisposedException>(() => file.GetPageCount());
        }

        [TestMethod]
        public void ThrowingStreamDisposeLeavesDocumentAndFileDisposed()
        {
            var stream = new TrackingStream(Pdf) { OnDispose = () => throw new IOException("dispose failure") };
            var document = PdfDocument.Load(stream, null, disposeStream: true);
            var file = (PdfFile)typeof(PdfDocument).GetField("_file", Fields)!.GetValue(document)!;
            Assert.ThrowsExactly<IOException>(document.Dispose);
            AssertFileReleased(file);
            Assert.ThrowsExactly<ObjectDisposedException>(() => document.Render(0, new RenderOptions(Dpi: 40), (_, _) => throw new AssertFailedException(), TestContext!.CancellationToken));
            document.Dispose();
            file.Dispose();
            Assert.AreEqual(1, stream.DisposeCount);
        }

        [TestMethod]
        public void ToImagePropagatesInputCleanupFailure()
        {
            var stream = new TrackingStream(Pdf) { OnDispose = () => throw new IOException("dispose failure") };
            var error = Assert.ThrowsExactly<IOException>(() => Conversion.ToImage(stream, options: new RenderOptions(Dpi: 40)));
            Assert.AreEqual("dispose failure", error.Message);
            Assert.AreEqual(1, stream.DisposeCount);
        }

        [TestMethod]
        public void IteratorCleanupFailureDoesNotDisposeDeliveredBitmap()
        {
            var stream = new TrackingStream(Pdf) { OnDispose = () => throw new IOException("dispose failure") };
            using var iterator = Conversion.ToImages(stream, options: new RenderOptions(Dpi: 40)).GetEnumerator();
            Assert.IsTrue(iterator.MoveNext());
            using var bitmap = iterator.Current;
            Assert.ThrowsExactly<IOException>(iterator.Dispose);
            Assert.AreNotEqual(IntPtr.Zero, bitmap.Handle, "The caller owns an already delivered bitmap.");
            Assert.AreEqual(1, stream.DisposeCount);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void SaveJpegClosesOwnedInputWhenOutputCannotOpen(bool leaveOpen)
        {
            using var stream = new TrackingStream(Pdf);
            var missingFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"), "image.jpg");
            Assert.ThrowsExactly<DirectoryNotFoundException>(() => Conversion.SaveJpeg(missingFile, stream, leaveOpen: leaveOpen));
            Assert.AreEqual(leaveOpen ? 0 : 1, stream.DisposeCount);
        }

        private static IEnumerable<int> FailingPages()
        {
            yield return 0;
            throw new IOException("page selection failure");
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void FailingPageSelectionClosesOnlyOwnedStream(bool leaveOpen)
        {
            using var stream = new TrackingStream(Pdf);
            Assert.ThrowsExactly<IOException>(() =>
            {
                foreach (var bitmap in Conversion.ToImages(stream, FailingPages(), leaveOpen))
                    bitmap.Dispose();
            });
            Assert.AreEqual(leaveOpen ? 0 : 1, stream.DisposeCount);
        }

#if NET6_0_OR_GREATER
        private sealed class AsyncDisposeStream() : MemoryStream(Pdf)
        {
            internal bool Disposed;

            public override async ValueTask DisposeAsync()
            {
                await Task.Delay(10).ConfigureAwait(false);
                Disposed = true;
                await base.DisposeAsync().ConfigureAwait(false);
            }
        }

        private sealed class CountingSynchronizationContext : SynchronizationContext
        {
            private int _posts;
            internal int Posts => Volatile.Read(ref _posts);

            public override void Post(SendOrPostCallback callback, object? state)
            {
                Interlocked.Increment(ref _posts);
                ThreadPool.QueueUserWorkItem(_ => callback(state));
            }
        }

        private static IAsyncEnumerable<SKBitmap> ConvertAsync(Stream stream, int selection, bool leaveOpen, CancellationToken token) => selection switch
        {
            0 => Conversion.ToImagesAsync(stream, leaveOpen, options: new RenderOptions(Dpi: 40), cancellationToken: token),
            1 => Conversion.ToImagesAsync(stream, 0..1, leaveOpen, options: new RenderOptions(Dpi: 40), cancellationToken: token),
            _ => Conversion.ToImagesAsync(stream, [0], leaveOpen, options: new RenderOptions(Dpi: 40), cancellationToken: token)
        };

        [TestMethod]
        [DataRow(0, false)]
        [DataRow(0, true)]
        [DataRow(1, false)]
        [DataRow(1, true)]
        [DataRow(2, false)]
        [DataRow(2, true)]
        public async Task PreCanceledAsyncConversionHonorsStreamOwnership(int selection, bool leaveOpen)
        {
            using var stream = new TrackingStream(Pdf) { OnLength = () => Assert.Fail("A canceled load must not inspect the stream.") };
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await foreach (var bitmap in ConvertAsync(stream, selection, leaveOpen, new CancellationToken(true)))
                    bitmap.Dispose();
            });
            Assert.AreEqual(leaveOpen ? 0 : 1, stream.DisposeCount);
        }

        [TestMethod]
        [DataRow(0, false)]
        [DataRow(0, true)]
        [DataRow(1, false)]
        [DataRow(1, true)]
        [DataRow(2, false)]
        [DataRow(2, true)]
        public async Task AsyncConversionHonorsOwnershipOnFailureAndEarlyExit(int selection, bool leaveOpen)
        {
            using var invalid = new TrackingStream([1, 2, 3]);
            await Assert.ThrowsExactlyAsync<PdfInvalidFormatException>(async () =>
            {
                await foreach (var bitmap in ConvertAsync(invalid, selection, leaveOpen, TestContext!.CancellationToken))
                    bitmap.Dispose();
            });
            Assert.AreEqual(leaveOpen ? 0 : 1, invalid.DisposeCount);

            using var valid = new TrackingStream(Pdf);
            await foreach (var bitmap in ConvertAsync(valid, selection, leaveOpen, TestContext!.CancellationToken))
            {
                bitmap.Dispose();
                break;
            }
            Assert.AreEqual(leaveOpen ? 0 : 1, valid.DisposeCount);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task CancellationDuringLoadClosesOnlyOwnedStream(bool leaveOpen)
        {
            using var cancellation = new CancellationTokenSource();
            using var stream = new TrackingStream(Pdf) { OnLength = cancellation.Cancel };
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await foreach (var bitmap in ConvertAsync(stream, 0, leaveOpen, cancellation.Token))
                    bitmap.Dispose();
            });
            Assert.AreEqual(leaveOpen ? 0 : 1, stream.DisposeCount);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task FailingAsyncPageSelectionClosesOnlyOwnedStream(bool leaveOpen)
        {
            using var stream = new TrackingStream(Pdf);
            await Assert.ThrowsExactlyAsync<IOException>(async () =>
            {
                await foreach (var bitmap in Conversion.ToImagesAsync(stream, FailingPages(), leaveOpen, cancellationToken: TestContext!.CancellationToken))
                    bitmap.Dispose();
            });
            Assert.AreEqual(leaveOpen ? 0 : 1, stream.DisposeCount);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public async Task AsyncCleanupFailurePreservesDeliveredBitmap(int selection)
        {
            var stream = new TrackingStream(Pdf) { OnDispose = () => throw new IOException("dispose failure") };
            await using var iterator = ConvertAsync(stream, selection, false, TestContext!.CancellationToken).GetAsyncEnumerator(TestContext!.CancellationToken);
            Assert.IsTrue(await iterator.MoveNextAsync());
            using var bitmap = iterator.Current;
            await Assert.ThrowsExactlyAsync<IOException>(() => iterator.DisposeAsync().AsTask());
            Assert.AreNotEqual(IntPtr.Zero, bitmap.Handle);
            Assert.AreEqual(1, stream.DisposeCount);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public void AsyncStreamCleanupDoesNotCaptureCallerContext(int selection)
        {
            var previousContext = SynchronizationContext.Current;
            var context = new CountingSynchronizationContext();
            SynchronizationContext.SetSynchronizationContext(context);
            try
            {
                using var stream = new AsyncDisposeStream();
                var iterator = ConvertAsync(stream, selection, leaveOpen: false, CancellationToken.None).GetAsyncEnumerator(TestContext!.CancellationToken);
                try
                {
                    Assert.IsTrue(iterator.MoveNextAsync().AsTask().GetAwaiter().GetResult());
                    iterator.Current.Dispose();
                }
                finally
                {
                    iterator.DisposeAsync().AsTask().GetAwaiter().GetResult();
                }

                Assert.IsTrue(stream.Disposed);
                Assert.AreEqual(0, context.Posts);
            }
            finally
            {
                SynchronizationContext.SetSynchronizationContext(previousContext);
            }
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(1)]
        [DataRow(2)]
        public async Task PreCanceledBase64ConversionDoesNotDecode(int selection)
        {
            var token = new CancellationToken(true);
            var images = selection switch
            {
                0 => Conversion.ToImagesAsync("invalid base64", cancellationToken: token),
                1 => Conversion.ToImagesAsync("invalid base64", 0..1, cancellationToken: token),
                _ => Conversion.ToImagesAsync("invalid base64", [0], cancellationToken: token)
            };
            await Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                await foreach (var bitmap in images)
                    bitmap.Dispose();
            });
        }
#endif
    }
}