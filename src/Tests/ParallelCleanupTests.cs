#if NET11_0_OR_GREATER
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Parallel;
using PDFtoImage.Parallel.Internals;
using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Tests
{
    [TestClass, DoNotParallelize]
    public sealed class ParallelCleanupTests : TestBase
    {
        private static readonly byte[] Pdf = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "SocialPreview.pdf"));
        private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;

        private sealed class ObservedWorker(WorkerConnection? worker, bool fail) : WorkerConnection(Stream.Null)
        {
            internal bool Disposed { get; private set; }

            public override void Dispose()
            {
                Disposed = true;
                worker?.Dispose();
                if (fail)
                    throw new IOException("worker cleanup");
            }
        }

        private static ObservedWorker[] ObserveWorkers(WorkerPool pool)
        {
            var slots = (IEnumerable)typeof(WorkerPool).GetField("_workers", Fields)!.GetValue(pool)!;
            return slots.Cast<object>().Select((slot, index) =>
            {
                var field = slot.GetType().GetField("Worker", Fields)!;
                var observed = new ObservedWorker((WorkerConnection)field.GetValue(slot)!, fail: index == 0);
                field.SetValue(slot, observed);
                return observed;
            }).ToArray();
        }

        private static void AssertSynchronizationResourcesDisposed(WorkerPool pool)
        {
            var slots = (SemaphoreSlim)typeof(WorkerPool).GetField("_slots", Fields)!.GetValue(pool)!;
            var shutdown = (CancellationTokenSource)typeof(WorkerPool).GetField("_shutdown", Fields)!.GetValue(pool)!;
            Assert.ThrowsExactly<ObjectDisposedException>(() => slots.Wait(0));
            Assert.ThrowsExactly<ObjectDisposedException>(() => _ = shutdown.Token);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task FailingWorkerDoesNotPreventOtherWorkersFromBeingDisposed(bool asynchronous)
        {
            var pool = new WorkerPool(2);
            Process[] processes = [];
            try
            {
                var request = new PdfRequest(Pdf, null);
                await Task.WhenAll(pool.GetPageCountAsync(request, TestContext!.CancellationToken), pool.GetPageCountAsync(request, TestContext.CancellationToken));
                processes = pool.WorkerProcessIds.Select(Process.GetProcessById).ToArray();
                Assert.HasCount(2, processes);
                var observed = ObserveWorkers(pool);
                var error = asynchronous
                    ? await Assert.ThrowsExactlyAsync<AggregateException>(() => pool.DisposeAsync().AsTask())
                    : Assert.ThrowsExactly<AggregateException>(pool.Dispose);

                Assert.HasCount(1, error.InnerExceptions);
                Assert.AreEqual("worker cleanup", error.InnerExceptions[0].Message);
                Assert.IsTrue(observed.All(worker => worker.Disposed));
                Assert.IsTrue(processes.All(process => process.HasExited));
                AssertSynchronizationResourcesDisposed(pool);
                pool.Dispose();
                await Assert.ThrowsExactlyAsync<AggregateException>(() => pool.DisposeAsync().AsTask());
            }
            finally
            {
                pool.Dispose();
                foreach (var process in processes)
                {
                    if (!process.HasExited)
                        process.Kill();
                    process.Dispose();
                }
            }
        }

        private sealed class FailingStopPool : WorkerPool
        {
            internal readonly TaskCompletionSource Started = new(TaskCreationOptions.RunContinuationsAsynchronously);
            internal readonly TaskCompletionSource AllowCleanup = new(TaskCreationOptions.RunContinuationsAsynchronously);
            internal bool ResourcesDisposed;

            internal FailingStopPool() : base(1) { }

            internal void AddWorker(WorkerConnection worker)
            {
                var slots = (IList)typeof(WorkerPool).GetField("_workers", Fields)!.GetValue(this)!;
                slots.Add(new Slot { Worker = worker });
            }

            protected override async Task<WorkerConnection> StartWorkerAsync(CancellationToken cancellationToken)
            {
                Started.TrySetResult();
                await AllowCleanup.Task;
                cancellationToken.ThrowIfCancellationRequested();
                throw new InvalidOperationException("The test must cancel startup.");
            }

            protected override void StopWorkers() => throw new IOException("stop workers");

            protected override void DisposeResources()
            {
                ResourcesDisposed = true;
                throw new IOException("pool resources");
            }
        }

        [TestMethod]
        public async Task StopFailureStillDrainsOperationsAndAggregatesResourceErrors()
        {
            var pool = new FailingStopPool();
            var first = new ObservedWorker(null, fail: true);
            var second = new ObservedWorker(null, fail: false);
            pool.AddWorker(first);
            pool.AddWorker(second);
            var request = pool.GetPageCountAsync(new PdfRequest(Pdf, null), TestContext!.CancellationToken);
            await pool.Started.Task.WaitAsync(TestContext.CancellationToken);
            var dispose = pool.DisposeAsync().AsTask();
            try
            {
                Assert.IsFalse(dispose.IsCompleted, "DisposeAsync must drain even after StopWorkers throws.");
                Assert.IsFalse(pool.ResourcesDisposed);
            }
            finally
            {
                pool.AllowCleanup.TrySetResult();
            }
            await Assert.ThrowsAsync<OperationCanceledException>(() => request);
            var error = await Assert.ThrowsExactlyAsync<AggregateException>(() => dispose.WaitAsync(TimeSpan.FromSeconds(10), TestContext.CancellationToken));
            Assert.AreSequenceEqual(new[] { "stop workers", "worker cleanup", "pool resources" }, error.InnerExceptions.Select(exception => exception.Message).ToArray());
            Assert.IsTrue(first.Disposed && second.Disposed);
            Assert.IsTrue(pool.ResourcesDisposed);
            AssertSynchronizationResourcesDisposed(pool);
        }

        private sealed class DelayedCancellationStream : MemoryStream
        {
            internal readonly TaskCompletionSource Started = new(TaskCreationOptions.RunContinuationsAsynchronously);
            internal readonly TaskCompletionSource AllowCleanup = new(TaskCreationOptions.RunContinuationsAsynchronously);

            internal DelayedCancellationStream() : base(new byte[1]) { }

            public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                Started.TrySetResult();
                try { await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken); }
                finally { await AllowCleanup.Task; }
                return 0;
            }
        }

        [TestMethod]
        public async Task ProcessorDisposeFailureStillWaitsForPublicRequestCleanup()
        {
            var processor = new ParallelPdfProcessor(1);
            using var stream = new DelayedCancellationStream();
            try
            {
                using var warmup = await processor.ToImageAsync(new MemoryStream(Pdf), options: new RenderOptions(Dpi: 40), cancellationToken: TestContext!.CancellationToken);
                var pool = (WorkerPool)typeof(ParallelPdfProcessor).GetField("_pool", Fields)!.GetValue(processor)!;
                ObserveWorkers(pool);
                var request = processor.ToImageAsync(stream, cancellationToken: TestContext.CancellationToken);
                await stream.Started.Task.WaitAsync(TestContext.CancellationToken);
                var dispose = processor.DisposeAsync().AsTask();
                try
                {
                    Assert.IsFalse(dispose.IsCompleted, "A worker cleanup error must not bypass public request draining.");
                }
                finally
                {
                    stream.AllowCleanup.TrySetResult();
                }
                await Assert.ThrowsAsync<OperationCanceledException>(() => request);
                await Assert.ThrowsExactlyAsync<AggregateException>(() => dispose.WaitAsync(TimeSpan.FromSeconds(10), TestContext.CancellationToken));
                Assert.IsFalse(stream.CanRead);
            }
            finally
            {
                stream.AllowCleanup.TrySetResult();
                processor.Dispose();
            }
        }
    }
}
#endif