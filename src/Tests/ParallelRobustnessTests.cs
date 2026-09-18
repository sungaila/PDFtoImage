#if NET9_0_OR_GREATER
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

namespace PDFtoImage.Tests
{
    [TestClass, DoNotParallelize, OSCondition(OperatingSystems.Windows)]
    public sealed class ParallelRobustnessTests : TestBase
    {
        private ParallelPdfProcessor _converter = null!;

        [TestInitialize]
        public void CreateConverter() => _converter = new ParallelPdfProcessor(2);

        [TestCleanup]
        public async Task DisposeConverter() => await _converter.DisposeAsync();

        private static MemoryStream OpenPdf(byte[] bytes) => new(bytes, writable: false);

        private static byte[] Pdf => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "Wikimedia_Commons_web.pdf"));

        private sealed class DisposableResult : IDisposable
        {
            internal bool IsDisposed { get; private set; }

            public void Dispose() => IsDisposed = true;
        }

        private sealed class BlockingReadStream : Stream
        {
            private readonly TaskCompletionSource _readStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly TaskCompletionSource _cancellationObserved = new(TaskCreationOptions.RunContinuationsAsynchronously);
            private readonly TaskCompletionSource _allowCleanup = new(TaskCreationOptions.RunContinuationsAsynchronously);

            internal Task ReadStarted => _readStarted.Task;

            internal Task CancellationObserved => _cancellationObserved.Task;

            internal void AllowCleanup() => _allowCleanup.TrySetResult();

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override void Flush() => throw new NotSupportedException();
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                _readStarted.TrySetResult();
                try
                {
                    await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    _cancellationObserved.TrySetResult();
                    await _allowCleanup.Task;
                    throw;
                }

                return 0;
            }
        }

        private static void ComparePage(SKBitmap bitmap, int page)
        {
            using var output = new MemoryStream();
            bitmap.Encode(output, SKEncodedImageFormat.Png, 100);
            TestUtils.CompareStreams(Path.Combine("..", "Assets", "Expected", "WINDOWS", $"Wikimedia_Commons_web_{page}.png"), output);
        }

        [TestMethod]
        public async Task DifferentDocumentsAndBatchesCanRunConcurrently()
        {
            var otherPdf = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "hundesteuer-anmeldung.pdf"));
            using var otherExpected = global::PDFtoImage.Conversion.ToImage(otherPdf, options: new RenderOptions(Dpi: 40));
            async Task Batch()
            {
                var index = 0;
                int[] pages = [2, 0, 1, 2];
                await foreach (var bitmap in _converter.ToImagesAsync(OpenPdf(Pdf), pages, options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken))
                {
                    using (bitmap)
                        ComparePage(bitmap, pages[index++]);
                }
                Assert.AreEqual(pages.Length, index);
            }
            async Task Singles()
            {
                for (var i = 0; i < 4; i++)
                {
                    using var bitmap = await _converter.ToImageAsync(OpenPdf(otherPdf), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken);
                    CollectionAssert.AreEqual(otherExpected.Bytes, bitmap.Bytes);
                }
            }
            await Task.WhenAll(Batch(), Batch(), Singles());
            Assert.HasCount(2, _converter.WorkerProcessIds);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task ConverterRemainsUsableAfterEndingEnumeration(bool cancel)
        {
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext!.CancellationToken);
            await using (var iterator = _converter.ToImagesAsync(OpenPdf(Pdf), options: new RenderOptions(Dpi: 40), cancellationToken: cancellation.Token).GetAsyncEnumerator())
            {
                Assert.IsTrue(await iterator.MoveNextAsync());
                iterator.Current.Dispose();
                if (cancel)
                {
                    cancellation.Cancel();
                    await Assert.ThrowsAsync<OperationCanceledException>(async () => await iterator.MoveNextAsync());
                }
            }
            using var next = await _converter.ToImageAsync(OpenPdf(Pdf), 1, options: new RenderOptions(Dpi: 40), cancellationToken: TestContext.CancellationToken);
            ComparePage(next, 1);
        }

        [TestMethod]
        public async Task SynchronousDisposeStopsWorkersAndRejectsEnumeration()
        {
            using var image = await _converter.ToImageAsync(OpenPdf(Pdf), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken);
            using var process = System.Diagnostics.Process.GetProcessById(_converter.WorkerProcessIds.Single());
            _converter.Dispose();
            Assert.IsTrue(process.HasExited);
            _converter.Dispose();
            await _converter.DisposeAsync();
            await Assert.ThrowsExactlyAsync<ObjectDisposedException>(async () =>
            {
                await foreach (var bitmap in _converter.ToImagesAsync(OpenPdf(Pdf), Array.Empty<int>()))
                    bitmap.Dispose();
            });
        }

        [TestMethod]
        public async Task CancellingOneBatchDoesNotCancelAnIndependentJob()
        {
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext!.CancellationToken);
            await using var iterator = _converter.ToImagesAsync(OpenPdf(Pdf), options: new RenderOptions(Dpi: 40), cancellationToken: cancellation.Token).GetAsyncEnumerator();
            Assert.IsTrue(await iterator.MoveNextAsync());
            iterator.Current.Dispose();
            var otherJob = _converter.ToImageAsync(OpenPdf(Pdf), 2, options: new RenderOptions(Dpi: 40), cancellationToken: TestContext.CancellationToken);
            cancellation.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await iterator.MoveNextAsync());
            using var image = await otherJob;
            ComparePage(image, 2);
        }

        [TestMethod]
        public async Task ConcurrentDisposalIsIdempotent()
        {
            using var image = await _converter.ToImageAsync(OpenPdf(Pdf), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken);
            using var process = System.Diagnostics.Process.GetProcessById(_converter.WorkerProcessIds.Single());
            await Task.WhenAll(Task.Run(_converter.Dispose), Task.Run(async () => await _converter.DisposeAsync()));
            Assert.IsTrue(process.HasExited);
        }

        [TestMethod]
        public async Task DisposeAsyncCancelsStreamReadAndWaitsForRequestCleanup()
        {
            var processor = new ParallelPdfProcessor(1);
            var stream = new BlockingReadStream();
            var request = processor.ToImageAsync(stream, cancellationToken: TestContext!.CancellationToken);
            await stream.ReadStarted.WaitAsync(TestContext.CancellationToken);

            var dispose = processor.DisposeAsync().AsTask();
            await stream.CancellationObserved.WaitAsync(TestContext.CancellationToken);
            Assert.IsFalse(dispose.IsCompleted, "DisposeAsync must await the public request cleanup.");

            stream.AllowCleanup();
            await Assert.ThrowsAsync<OperationCanceledException>(() => request);
            await dispose;
            await Assert.ThrowsExactlyAsync<ObjectDisposedException>(() => processor.ToImageAsync(OpenPdf(Pdf), cancellationToken: TestContext.CancellationToken));
        }

        [TestMethod]
        public async Task UserCancellationDoesNotDisposeProcessor()
        {
            var stream = new BlockingReadStream();
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext!.CancellationToken);
            var request = _converter.ToImageAsync(stream, cancellationToken: cancellation.Token);
            await stream.ReadStarted.WaitAsync(TestContext.CancellationToken);

            cancellation.Cancel();
            await stream.CancellationObserved.WaitAsync(TestContext.CancellationToken);
            stream.AllowCleanup();
            await Assert.ThrowsAsync<OperationCanceledException>(() => request);

            using var bitmap = await _converter.ToImageAsync(OpenPdf(Pdf), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext.CancellationToken);
            ComparePage(bitmap, 0);
        }

        [TestMethod]
        public async Task CompletedRequestUnloadsDocumentsButReusesWorkerProcess()
        {
            await using var processor = new ParallelPdfProcessor(1);
            await foreach (var pageBitmap in processor.ToImagesAsync(OpenPdf(Pdf), [0, 1, 2], options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken))
                pageBitmap.Dispose();

            var processIds = processor.WorkerProcessIds;
            CollectionAssert.AreEqual(new[] { 1 }, processor.WorkerDocumentLoadCounts);
            Assert.IsTrue(processor.WorkerDocumentIds.All(id => id == null));

            var otherPdf = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "hundesteuer-anmeldung.pdf"));
            using var bitmap = await processor.ToImageAsync(OpenPdf(otherPdf), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext.CancellationToken);
            CollectionAssert.AreEqual(processIds, processor.WorkerProcessIds);
            CollectionAssert.AreEqual(new[] { 2 }, processor.WorkerDocumentLoadCounts);
            Assert.IsTrue(processor.WorkerDocumentIds.All(id => id == null));
        }

        [TestMethod]
        public async Task StartupConnectionWithoutHelloTimesOut()
        {
            var pipeName = "PDFtoImage.Tests." + Guid.NewGuid().ToString("N");
            using var server = new System.IO.Pipes.NamedPipeServerStream(pipeName, System.IO.Pipes.PipeDirection.InOut, 1,
                System.IO.Pipes.PipeTransmissionMode.Byte, System.IO.Pipes.PipeOptions.Asynchronous | System.IO.Pipes.PipeOptions.CurrentUserOnly);
            using var client = new System.IO.Pipes.NamedPipeClientStream(".", pipeName, System.IO.Pipes.PipeDirection.InOut, System.IO.Pipes.PipeOptions.Asynchronous);
            var connect = client.ConnectAsync(TestContext!.CancellationToken);
            await server.WaitForConnectionAsync(TestContext.CancellationToken);
            await connect;

            using var timeout = new CancellationTokenSource(TimeSpan.FromMilliseconds(50));
            await Assert.ThrowsExactlyAsync<TimeoutException>(() => WorkerConnectionWindows.ReadHelloAsync(server, TestContext.CancellationToken, timeout.Token));
        }

        [TestMethod]
        public async Task ConcurrentWorkerConnectionDisposalIsIdempotent()
        {
            using var job = WindowsJob.Create();
            var worker = await WorkerConnectionWindows.StartAsync(job, TestContext!.CancellationToken);
            using var process = System.Diagnostics.Process.GetProcessById(worker.ProcessId);

            await Task.WhenAll(Task.Run(worker.Dispose), Task.Run(worker.Dispose));
            await process.WaitForExitAsync(TestContext.CancellationToken);
            Assert.IsTrue(process.HasExited);
        }

        [TestMethod]
        public async Task PasswordAndRequestIdentityAreNotCachedAcrossCalls()
        {
            var pdf = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "SocialPreview with password 123456 (AES-256).pdf"));
            using var first = await _converter.ToImageAsync(OpenPdf(pdf), password: "123456", options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken);
            await Assert.ThrowsExactlyAsync<ParallelConversionException>(() =>
                _converter.ToImageAsync(OpenPdf(pdf), password: "wrong", cancellationToken: TestContext.CancellationToken));
            using var second = await _converter.ToImageAsync(OpenPdf(pdf), password: "123456", options: new RenderOptions(Dpi: 40), cancellationToken: TestContext.CancellationToken);
            CollectionAssert.AreEqual(first.Bytes, second.Bytes);
        }

        [TestMethod]
        public async Task ConcurrentJobsReuseIdleWorkersWithoutMixingPipeFrames()
        {
            var requests = Enumerable.Range(0, 12).Select(async i =>
            {
                using var bitmap = await _converter.ToImageAsync(OpenPdf(Pdf), i % 3, options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken);
                ComparePage(bitmap, i % 3);
            });
            await Task.WhenAll(requests);
            Assert.HasCount(2, _converter.WorkerProcessIds);
        }

        [TestMethod]
        public async Task SequentialDocumentsReuseTheSameProcess()
        {
            using var first = await _converter.ToImageAsync(OpenPdf(Pdf), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken);
            var ids = _converter.WorkerProcessIds;
            var otherPdf = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "hundesteuer-anmeldung.pdf"));
            using var second = await _converter.ToImageAsync(OpenPdf(otherPdf), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext.CancellationToken);
            using var expected = global::PDFtoImage.Conversion.ToImage(otherPdf, options: new RenderOptions(Dpi: 40));
            CollectionAssert.AreEqual(expected.Bytes, second.Bytes);
            CollectionAssert.AreEqual(ids, _converter.WorkerProcessIds);
            using var third = await _converter.ToImageAsync(OpenPdf(Pdf), 2, options: new RenderOptions(Dpi: 40), cancellationToken: TestContext.CancellationToken);
            ComparePage(third, 2);
            CollectionAssert.AreEqual(ids, _converter.WorkerProcessIds);
        }

        [TestMethod]
        public async Task SinglePageSelectionOnlyStartsOneWorker()
        {
            await using var converter = new ParallelPdfProcessor(int.MaxValue);
            await foreach (var bitmap in converter.ToImagesAsync(OpenPdf(Pdf), new[] { 0 }, options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken))
            {
                using (bitmap)
                    ComparePage(bitmap, 0);
            }
            Assert.HasCount(1, converter.WorkerProcessIds);
        }

        [TestMethod]
        public async Task DisposalCompletesActiveAndQueuedRequests()
        {
            await using var pool = new ParallelPdfProcessor(1);
            var pending = Enumerable.Range(0, 20).Select(async i =>
            {
                using var image = await pool.ToImageAsync(OpenPdf(Pdf), i % 3, options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken);
            }).ToArray();
            await pool.DisposeAsync();
            try { await Task.WhenAll(pending).WaitAsync(TimeSpan.FromSeconds(10), TestContext!.CancellationToken); }
            catch (Exception exception) when (exception is OperationCanceledException or ParallelConversionException or ObjectDisposedException) { }
            Assert.IsTrue(pending.All(task => task.IsCompleted));
            Assert.HasCount(0, pool.WorkerProcessIds);
        }

        [TestMethod]
        public async Task ManagedWorkerErrorDoesNotCorruptFollowingRequest()
        {
            using var first = await _converter.ToImageAsync(OpenPdf(Pdf), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken);
            var ids = _converter.WorkerProcessIds;
            await Assert.ThrowsExactlyAsync<ParallelConversionException>(() =>
                _converter.ToImageAsync(new MemoryStream([1, 2, 3], writable: false), cancellationToken: TestContext.CancellationToken));
            using var bitmap = await _converter.ToImageAsync(OpenPdf(Pdf), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext.CancellationToken);
            ComparePage(bitmap, 0);
            CollectionAssert.AreEqual(ids, _converter.WorkerProcessIds);
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
                    ? _converter.ToImagesAsync(OpenPdf(Pdf), ^2..^0, options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken)
                    : _converter.ToImagesAsync(OpenPdf(Pdf), pages, options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken);
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
            await foreach (var bitmap in _converter.ToImagesAsync(new MemoryStream(Array.Empty<byte>(), writable: false), Array.Empty<int>(), cancellationToken: TestContext!.CancellationToken))
            {
                bitmap.Dispose();
                Assert.Fail("An empty selection must not return bitmaps.");
            }
        }

        [TestMethod]
        public async Task DisposedPoolRejectsNewJobs()
        {
            var pool = new ParallelPdfProcessor(1);
            await pool.DisposeAsync();
            await pool.DisposeAsync();
            await Assert.ThrowsExactlyAsync<ObjectDisposedException>(() => pool.ToImageAsync(OpenPdf(Pdf), cancellationToken: TestContext!.CancellationToken));
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
        public async Task EarlyEnumerationExitDisposesCompletedPendingResults()
        {
            var results = new List<DisposableResult>();
            async Task<DisposableResult> Render(int page, CancellationToken token)
            {
                var result = new DisposableResult();
                results.Add(result);
                if (page == 0)
                    return result;

                try { await Task.Delay(Timeout.Infinite, token); }
                catch (OperationCanceledException) { return result; }
                return result;
            }

            await foreach (var result in OrderedScheduler.RunAsync(Enumerable.Range(0, 10), 4, Render, TestContext!.CancellationToken))
            {
                result.Dispose(); // The yielded result belongs to the caller.
                break;
            }

            Assert.HasCount(4, results);
            Assert.IsTrue(results.All(result => result.IsDisposed));
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
