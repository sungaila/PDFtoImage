#if NET8_0_OR_GREATER
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Parallel;
using PDFtoImage.Parallel.Internals;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ParallelConversion = PDFtoImage.Parallel.Conversion;

namespace PDFtoImage.Tests
{
    [TestClass, DoNotParallelize, OSCondition(OperatingSystems.Windows)]
    public sealed class ParallelRobustnessTests : TestBase
    {
        private static byte[] Pdf => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "Wikimedia_Commons_web.pdf"));

        private static void ComparePage(SKBitmap bitmap, int page)
        {
            using var output = new MemoryStream();
            bitmap.Encode(output, SKEncodedImageFormat.Png, 100);
            TestUtils.CompareStreams(Path.Combine("..", "Assets", "Expected", "WINDOWS", $"Wikimedia_Commons_web_{page}.png"), output);
        }

        [TestMethod]
        public async Task ConcurrentJobsReuseIdleWorkersWithoutMixingPipeFrames()
        {
            await using var pool = await WorkerPool.CreateAsync(2, Pdf, null, TestContext!.CancellationToken);
            var requests = Enumerable.Range(0, 12).Select(async i =>
            {
                var bytes = await pool.RenderPageAsync(i % 3, new RenderOptions(Dpi: 40), TestContext.CancellationToken);
                using var bitmap = PipeProtocol.ReadBitmap(bytes, 1);
                ComparePage(bitmap, i % 3);
            });
            await Task.WhenAll(requests);
        }

        [TestMethod]
        public async Task WorkerCountIsCappedAtDocumentPageCount()
        {
            await using var pool = await WorkerPool.CreateAsync(int.MaxValue, Pdf, null, TestContext!.CancellationToken);
            Assert.AreEqual(pool.PageCount, pool.WorkerCount);
        }

        [TestMethod]
        public async Task WorkerCountIsCappedAtSelectionSize()
        {
            await using var pool = await WorkerPool.CreateAsync(int.MaxValue, Pdf, null, TestContext!.CancellationToken, _ => 1);
            Assert.AreEqual(1, pool.WorkerCount);
        }

        [TestMethod]
        public async Task DisposalCompletesActiveAndQueuedRequests()
        {
            var pool = await WorkerPool.CreateAsync(1, Pdf, null, TestContext!.CancellationToken);
            var pending = Enumerable.Range(0, 20)
                .Select(i => pool.RenderPageAsync(i % 3, new RenderOptions(Dpi: 40), TestContext.CancellationToken)).ToArray();
            await pool.DisposeAsync();
            try { await Task.WhenAll(pending).WaitAsync(TimeSpan.FromSeconds(10), TestContext.CancellationToken); }
            catch (Exception exception) when (exception is OperationCanceledException or ParallelConversionException) { }
            Assert.IsTrue(pending.All(task => task.IsCompleted));
        }

        [TestMethod]
        public async Task ManagedWorkerErrorDoesNotCorruptFollowingRequest()
        {
            await using var pool = await WorkerPool.CreateAsync(1, Pdf, null, TestContext!.CancellationToken);
            var exception = await Assert.ThrowsExactlyAsync<ParallelConversionException>(() =>
                pool.RenderPageAsync(-1, new RenderOptions(Dpi: 40), TestContext.CancellationToken));
            Assert.AreEqual(typeof(ArgumentOutOfRangeException).FullName, exception.RemoteExceptionType);
            using var bitmap = PipeProtocol.ReadBitmap(await pool.RenderPageAsync(0, new RenderOptions(Dpi: 40), TestContext.CancellationToken), 1);
            ComparePage(bitmap, 0);
        }

        [TestMethod]
        public void WorkerDocumentSupportsChangingOptionsAndOwnsItsStream()
        {
            using var stream = new MemoryStream(Pdf);
            using (var document = new WorkerDocument(stream, null))
            {
                using var first = document.Render(2, new RenderOptions(Dpi: 40));
                using var rotated = document.Render(0, new RenderOptions(Dpi: 30, Grayscale: true));
                using var expected = global::PDFtoImage.Conversion.ToImage(Pdf, 0, options: new RenderOptions(Dpi: 30, Grayscale: true));
                CollectionAssert.AreEqual(expected.Bytes, rotated.Bytes);
                using var third = document.Render(1, new RenderOptions(Dpi: 40));
                ComparePage(first, 2);
                ComparePage(third, 1);
                Assert.IsTrue(stream.CanRead);
            }
            Assert.IsFalse(stream.CanRead);
        }

        [TestMethod]
        public async Task DuplicatesAndFromEndRangeMatchOriginalBytes()
        {
            var pageCount = global::PDFtoImage.Conversion.GetPageCount(Pdf);
            foreach (var pages in new[] { new[] { 2, 0, 2, 1 }, new[] { pageCount - 2, pageCount - 1 } })
            {
                var results = pages.Length == 2
                    ? ParallelConversion.ToImagesAsync(Pdf, ^2..^0, options: new RenderOptions(Dpi: 40), workerCount: 2, cancellationToken: TestContext!.CancellationToken)
                    : ParallelConversion.ToImagesAsync(Pdf, pages, options: new RenderOptions(Dpi: 40), workerCount: 2, cancellationToken: TestContext!.CancellationToken);
                var count = 0;
                await foreach (var bitmap in results)
                {
                    using (bitmap)
                        ComparePage(bitmap, pages[count++]);
                }
                Assert.AreEqual(pages.Length, count);
            }
        }

        [TestMethod]
        public async Task EmptySelectionDoesNotLaunchWorkersOrParsePdf()
        {
            await foreach (var bitmap in ParallelConversion.ToImagesAsync(Array.Empty<byte>(), Array.Empty<int>(), cancellationToken: TestContext!.CancellationToken))
            {
                bitmap.Dispose();
                Assert.Fail("An empty selection must not return bitmaps.");
            }
        }

        [TestMethod]
        public async Task DisposedPoolRejectsNewJobs()
        {
            var pool = await WorkerPool.CreateAsync(1, Pdf, null, TestContext!.CancellationToken);
            await pool.DisposeAsync();
            await pool.DisposeAsync();
            await Assert.ThrowsExactlyAsync<ObjectDisposedException>(() => pool.RenderPageAsync(0, default, TestContext.CancellationToken));
        }

        [TestMethod]
        public async Task SlowFirstPageAllowsBoundedLookAheadButPreservesOrder()
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext!.CancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(10));
            var first = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var fourth = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var workers = new SemaphoreSlim(2);
            var started = 0;
            var output = new List<int>();
            async Task<int> Render(int page, CancellationToken token)
            {
                await workers.WaitAsync(token);
                try
                {
                    Interlocked.Increment(ref started);
                    if (page == 0)
                        await first.Task.WaitAsync(token);
                    if (page == 3)
                        fourth.TrySetResult();
                    return page;
                }
                finally { workers.Release(); }
            }
            async Task Consume()
            {
                await foreach (var item in OrderedScheduler.RunAsync(Enumerable.Range(0, 9), 4, Render, timeout.Token))
                    output.Add(item);
            }
            var consume = Consume();
            try
            {
                await fourth.Task.WaitAsync(timeout.Token);
                Assert.AreEqual(4, Volatile.Read(ref started), "The look-ahead window must be bounded.");
                Assert.HasCount(0, output, "Output must wait for the first page.");
            }
            finally { first.TrySetResult(); }
            await consume;
            CollectionAssert.AreEqual(Enumerable.Range(0, 9).ToArray(), output);
        }

        [TestMethod]
        public async Task EarlyEnumerationExitCancelsAndObservesPendingJobs()
        {
            var cancelled = 0;
            async Task<int> Render(int page, CancellationToken token)
            {
                if (page == 0)
                    return page;
                try { await Task.Delay(Timeout.Infinite, token); }
                catch (OperationCanceledException) { Interlocked.Increment(ref cancelled); throw; }
                return page;
            }
            await foreach (var _ in OrderedScheduler.RunAsync(Enumerable.Range(0, 10), 4, Render, TestContext!.CancellationToken))
                break;
            Assert.AreEqual(3, cancelled);
        }

        [TestMethod]
        [DataRow(-1)]
        [DataRow(0)]
        [DataRow(int.MaxValue)]
        public async Task InvalidFrameLengthsAreRejected(int length)
        {
            using var stream = new MemoryStream(BitConverter.GetBytes(length));
            await Assert.ThrowsExactlyAsync<InvalidDataException>(() => PipeProtocol.ReadMessageAsync(stream, TestContext!.CancellationToken));
        }

        [TestMethod]
        public async Task TruncatedFrameIsRejected()
        {
            using var stream = new MemoryStream([4, 0, 0, 0, 1]);
            await Assert.ThrowsExactlyAsync<EndOfStreamException>(() => PipeProtocol.ReadMessageAsync(stream, TestContext!.CancellationToken));
        }

        [TestMethod]
        public void InvalidBitmapDimensionsAreRejectedBeforeNativeAllocation()
        {
            var payload = PipeProtocol.CreateMessage(writer =>
            {
                writer.Write(int.MaxValue);
                writer.Write(int.MaxValue);
                writer.Write((int)SKColorType.Bgra8888);
                writer.Write((int)SKAlphaType.Premul);
                writer.Write(4);
                writer.Write(4);
                writer.Write(0);
            });
            Assert.ThrowsExactly<InvalidDataException>(() => PipeProtocol.ReadBitmap(payload));
            Assert.ThrowsExactly<InvalidDataException>(() => PipeProtocol.ReadBitmap([1, 2, 3]));
        }

        [TestMethod]
        public async Task ChunkedBitmapTransferPreservesEveryPixelByte()
        {
            using var original = new SKBitmap(320, 240, SKColorType.Bgra8888, SKAlphaType.Premul);
            original.Erase(new SKColor(40, 80, 120, 160));
            using var pipe = new MemoryStream();
            await PipeProtocol.WriteBitmapResponseAsync(pipe, original, TestContext!.CancellationToken);
            pipe.Position = 0;
            var response = await PipeProtocol.ReadMessageAsync(pipe, TestContext.CancellationToken);
            Assert.IsNotNull(response);
            using var decoded = PipeProtocol.ReadBitmap(response, 1);
            CollectionAssert.AreEqual(original.Bytes, decoded.Bytes);
            Assert.AreEqual(pipe.Length, pipe.Position);
        }
    }
}
#endif
