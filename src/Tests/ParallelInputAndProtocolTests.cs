#if NET11_0_OR_GREATER
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Parallel;
using PDFtoImage.Parallel.Internals;
using SkiaSharp;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using static PDFtoImage.Tests.TestUtils;

namespace PDFtoImage.Tests
{
    [TestClass, DoNotParallelize]
    public sealed class ParallelInputAndProtocolTests : TestBase
    {
        private static readonly byte[] Pdf = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "SocialPreview.pdf"));

        private sealed class ChunkedInputStream : Stream
        {
            private readonly MemoryStream _inner;
            private readonly bool _seekable;
            private readonly Action? _afterRead;
            internal int ReadCalls;
            internal int ExtraLength;

            internal ChunkedInputStream(bool seekable, Action? afterRead = null)
            {
                _inner = new MemoryStream([0, 1, 2, .. Pdf], writable: false) { Position = 3 };
                _seekable = seekable;
                _afterRead = afterRead;
            }

            public override bool CanRead => _inner.CanRead;
            public override bool CanSeek => _seekable;
            public override bool CanWrite => false;
            public override long Length => _seekable ? _inner.Length + ExtraLength : throw new NotSupportedException();
            public override long Position { get => _inner.Position; set => _inner.Position = value; }
            public override long Seek(long offset, SeekOrigin origin) => _inner.Seek(offset, origin);
            public override void Flush() => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

            public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            {
                ReadCalls++;
                var count = await _inner.ReadAsync(buffer[..Math.Min(buffer.Length, 97)], cancellationToken);
                _afterRead?.Invoke();
                return count;
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    _inner.Dispose();
                base.Dispose(disposing);
            }
        }

        [TestMethod]
        [DataRow(false, false)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        [DataRow(true, true)]
        public async Task ShortReadsFromCurrentPositionPreservePdfAndOwnership(bool seekable, bool leaveOpen)
        {
            await using var processor = new ParallelPdfProcessor(1);
            using var input = new ChunkedInputStream(seekable);
            var options = new RenderOptions(Dpi: 40);
            using var actual = await processor.ToImageAsync(input, leaveOpen: leaveOpen, options: options, cancellationToken: TestContext!.CancellationToken);
            using var expected = Conversion.ToImage(Pdf, options: options);
            AssertBitmapsEqual(expected, actual);
            Assert.IsTrue(input.ReadCalls > 1);
            Assert.AreEqual(leaveOpen, input.CanRead);
            if (leaveOpen)
                Assert.AreEqual(Pdf.Length + 3L, input.Position);
        }

        [TestMethod]
        [DataRow(false, false, false)]
        [DataRow(false, true, false)]
        [DataRow(true, false, false)]
        [DataRow(true, true, false)]
        [DataRow(false, false, true)]
        [DataRow(false, true, true)]
        [DataRow(true, false, true)]
        [DataRow(true, true, true)]
        public async Task CancelledReadsPreserveOwnership(bool seekable, bool leaveOpen, bool cancelBeforeRead)
        {
            await using var processor = new ParallelPdfProcessor(1);
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext!.CancellationToken);
            using var input = new ChunkedInputStream(seekable, cancellation.Cancel);
            if (cancelBeforeRead)
                cancellation.Cancel();
            await Assert.ThrowsAsync<OperationCanceledException>(() => processor.ToImageAsync(input, leaveOpen: leaveOpen, cancellationToken: cancellation.Token));
            Assert.AreEqual(leaveOpen, input.CanRead);
            Assert.IsEmpty(processor.WorkerProcessIds);
            if (cancelBeforeRead)
                Assert.AreEqual(0, input.ReadCalls);
        }

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public async Task TruncatedSeekableStreamFailsWithoutStartingWorker(bool leaveOpen)
        {
            await using var processor = new ParallelPdfProcessor(1);
            using var input = new ChunkedInputStream(seekable: true) { ExtraLength = 1 };
            await Assert.ThrowsExactlyAsync<EndOfStreamException>(() => processor.ToImageAsync(input, leaveOpen: leaveOpen, cancellationToken: TestContext!.CancellationToken));
            Assert.AreEqual(leaveOpen, input.CanRead);
            Assert.IsEmpty(processor.WorkerProcessIds);
        }

        private sealed class ReplayStream(byte[] requests, Stream responses) : MemoryStream(requests, writable: false)
        {
            public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) =>
                responses.WriteAsync(buffer, cancellationToken);
        }

        [TestMethod]
        public void RenderOptionsRoundTripUsesSourceGeneratedJson()
        {
            var expected = new RenderOptions(
                Dpi: 144,
                Width: 320,
                Height: null,
                WithAnnotations: true,
                WithFormFill: true,
                WithAspectRatio: true,
                Rotation: PdfRotation.Rotate270,
                AntiAliasing: PdfAntiAliasing.Text | PdfAntiAliasing.Paths,
                BackgroundColor: new SKColor(1, 2, 3, 4),
                Bounds: new RectangleF(1.25f, 2.5f, 300.75f, 400.5f),
                UseTiling: true,
                DpiRelativeToBounds: true,
                Grayscale: true);

            var message = WorkerProtocol.CreateMessage(writer => WorkerProtocol.WriteRenderOptions(writer, expected));
            using var reader = WorkerProtocol.CreateReader(message);

            var actual = WorkerProtocol.ReadRenderOptions(reader);

            Assert.AreEqual(expected, actual);
            Assert.AreEqual(reader.BaseStream.Length, reader.BaseStream.Position);
        }

        [TestMethod]
        public async Task RenderRequestRejectsTrailingBytesAndKeepsNextFrameSynchronized()
        {
            using var requests = new MemoryStream();
            var load = WorkerProtocol.CreateMessage(writer =>
            {
                writer.Write((byte)WorkerCommand.LoadDocument);
                WorkerProtocol.WriteNullableString(writer, null);
                writer.Write(Pdf.Length);
                writer.Write(Guid.NewGuid().ToByteArray());
            });
            await WorkerProtocol.WriteMessageAsync(requests, load, Pdf, TestContext!.CancellationToken);
            foreach (var trailingData in new[] { true, false })
            {
                var render = WorkerProtocol.CreateMessage(writer =>
                {
                    writer.Write((byte)WorkerCommand.RenderPage);
                    writer.Write(0);
                    WorkerProtocol.WriteRenderOptions(writer, new RenderOptions(Dpi: 40));
                    if (trailingData)
                        writer.Write((byte)42);
                });
                await WorkerProtocol.WriteMessageAsync(requests, render, TestContext.CancellationToken);
            }

            using var responses = new MemoryStream();
            using var stream = new ReplayStream(requests.ToArray(), responses);
            Assert.AreEqual(0, await WorkerHost.RunAsync(stream));
            responses.Position = 0;
            await WorkerConnection.ReadHelloAsync(responses, TestContext.CancellationToken, CancellationToken.None);
            var loaded = await WorkerProtocol.ReadMessageAsync(responses, TestContext.CancellationToken);
            using (var reader = WorkerProtocol.CreateReader(loaded!))
                WorkerProtocol.ThrowIfError(reader);
            var rejected = await WorkerProtocol.ReadMessageAsync(responses, TestContext.CancellationToken);
            using (var reader = WorkerProtocol.CreateReader(rejected!))
            {
                var error = Assert.ThrowsExactly<ParallelConversionException>(() => WorkerProtocol.ThrowIfError(reader));
                Assert.AreEqual(typeof(InvalidDataException).FullName, error.RemoteExceptionType);
            }
            var rendered = await WorkerProtocol.ReadMessageAsync(responses, TestContext.CancellationToken);
            using var bitmap = WorkerProtocol.ReadBitmap(rendered!, 1);
            using var expected = Conversion.ToImage(Pdf, options: new RenderOptions(Dpi: 40));
            AssertBitmapsEqual(expected, bitmap);
            Assert.AreEqual(responses.Length, responses.Position);
        }
    }
}
#endif