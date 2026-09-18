using Microsoft.Win32.SafeHandles;
using SkiaSharp;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Parallel.Internals
{
    internal class WorkerConnection : IDisposable
    {
        protected Stream _stream;

        protected SafeProcessHandle? _process;

        protected int _disposed;

        private SafeFileHandle? _lifetime;

        private Guid? _documentId;

        private int _pageCount;

        private int _documentLoadCount;

        protected WorkerConnection(Stream stream)
        {
            _stream = stream;
        }

        internal int ProcessId => _process?.ProcessId ?? 0;

        internal Guid? DocumentId => _documentId;

        internal int DocumentLoadCount => _documentLoadCount;

        internal static async Task<WorkerConnection> StartAsync(CancellationToken cancellationToken)
        {
            var pipeName = Guid.NewGuid().ToString("N");
            var pipe = new NamedPipeServerStream(
                pipeName,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
            var worker = new WorkerConnection(pipe);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                var process = WorkerProcessLauncher.Start(pipeName, out worker._lifetime);
                worker._process = process;

                using var startupTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                using var startupCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, startupTimeout.Token);

                var connectionTask = pipe.WaitForConnectionAsync(startupCancellation.Token);
                var exitTask = process.WaitForExitAsync(CancellationToken.None);
                var completed = await Task.WhenAny(connectionTask, exitTask).ConfigureAwait(false);

                if (completed == exitTask)
                {
                    await exitTask.ConfigureAwait(false);
                    throw new EndOfStreamException("The PDF conversion worker exited before connecting.");
                }

                await connectionTask.ConfigureAwait(false);
                await ReadHelloAsync(pipe, cancellationToken, startupTimeout.Token).ConfigureAwait(false);
                return worker;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                worker.Dispose();
                throw;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                worker.Dispose();
                throw new TimeoutException("The PDF conversion worker did not complete startup within 30 seconds.");
            }
            catch
            {
                worker.Dispose();
                throw;
            }
        }

        internal async Task<T> ExecuteAsync<T>(PdfRequest request,
            Func<int, CancellationToken, Task<T>> execute, CancellationToken cancellationToken)
        {
            var pageCount = await LoadDocumentAsync(request, cancellationToken).ConfigureAwait(false);
            return await execute(pageCount, cancellationToken).ConfigureAwait(false);
        }

        internal async Task UnloadDocumentAsync(Guid requestId, CancellationToken cancellationToken)
        {
            if (_documentId != requestId)
                return;

            var request = WorkerProtocol.CreateMessage(writer =>
            {
                writer.Write((byte)WorkerCommand.UnloadDocument);
                writer.Write(requestId.ToByteArray());
            });
            await WorkerProtocol.WriteMessageAsync(_stream, request, cancellationToken).ConfigureAwait(false);
            var response = await ReadRequiredMessageAsync(_stream, cancellationToken).ConfigureAwait(false);
            using var reader = WorkerProtocol.CreateReader(response);
            WorkerProtocol.ThrowIfError(reader);
            if (reader.BaseStream.Position != reader.BaseStream.Length)
                throw new InvalidDataException("The worker returned an invalid unload response.");
            _documentId = null;
            _pageCount = 0;
        }

        private async Task<int> LoadDocumentAsync(PdfRequest request, CancellationToken cancellationToken)
        {
            if (_documentId == request.Id)
                return _pageCount;

            _documentId = null;

            var header = WorkerProtocol.CreateMessage(writer =>
            {
                writer.Write((byte)WorkerCommand.LoadDocument);
                WorkerProtocol.WriteNullableString(writer, request.Password);
                writer.Write(request.Bytes.Length);
                writer.Write(request.Id.ToByteArray());
            });

            try
            {
                await WorkerProtocol.WriteMessageAsync(_stream, header, request.Bytes, cancellationToken).ConfigureAwait(false);

                var response = await ReadRequiredMessageAsync(_stream, cancellationToken).ConfigureAwait(false);

                using var reader = WorkerProtocol.CreateReader(response);

                WorkerProtocol.ThrowIfError(reader);
                _pageCount = reader.ReadInt32();

                if (_pageCount < 0 || reader.BaseStream.Position != reader.BaseStream.Length)
                    throw new InvalidDataException("The worker returned an invalid page count.");

                _documentId = request.Id;
                _documentLoadCount++;

                return _pageCount;
            }
            catch (IOException exception)
            {
                throw new ParallelConversionException("WorkerProcessTerminated",
                    "The PDF conversion worker failed while loading a document.", null, exception);
            }
        }

        internal async Task<SKBitmap> RenderPageAsync(int page, RenderOptions options, CancellationToken cancellationToken)
        {
            try
            {
                var request = WorkerProtocol.CreateMessage(writer =>
                {
                    writer.Write((byte)WorkerCommand.RenderPage);
                    writer.Write(page);
                    WorkerProtocol.WriteRenderOptions(writer, options);
                });

                await WorkerProtocol.WriteMessageAsync(_stream, request, cancellationToken).ConfigureAwait(false);

                var response = await ReadRequiredMessageAsync(_stream, cancellationToken).ConfigureAwait(false);
                using var reader = WorkerProtocol.CreateReader(response);

                WorkerProtocol.ThrowIfError(reader);

                return WorkerProtocol.ReadBitmap(response, checked((int)reader.BaseStream.Position));
            }
            catch (IOException exception)
            {
                throw new ParallelConversionException(
                    "WorkerProcessTerminated",
                    "The PDF conversion worker terminated unexpectedly.",
                    null,
                    exception);
            }
        }

        public virtual void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            _stream.Dispose();
            Interlocked.Exchange(ref _lifetime, null)?.Dispose();

            var process = Interlocked.Exchange(ref _process, null);
            if (process == null)
                return;

            try
            {
                process.Kill();
                process.WaitForExit();
            }
            finally
            {
                process.Dispose();
            }
        }

        private static async Task<byte[]> ReadRequiredMessageAsync(Stream stream, CancellationToken cancellationToken)
        {
            return await WorkerProtocol.ReadMessageAsync(stream, cancellationToken).ConfigureAwait(false)
                ?? throw new EndOfStreamException("The PDF conversion worker closed its IPC connection unexpectedly.");
        }

        internal static async Task ReadHelloAsync(Stream stream, CancellationToken cancellationToken, CancellationToken startupTimeoutToken)
        {
            using var startupCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, startupTimeoutToken);
            try
            {
                var helloMessage = await WorkerProtocol.ReadMessageAsync(stream, startupCancellation.Token).ConfigureAwait(false)
                    ?? throw new EndOfStreamException("The PDF conversion worker exited during startup.");

                using var helloReader = WorkerProtocol.CreateReader(helloMessage);
                if ((WorkerResponse)helloReader.ReadByte() != WorkerResponse.Hello || helloReader.ReadInt32() != WorkerProtocol.Version ||
                    helloReader.BaseStream.Position != helloReader.BaseStream.Length)
                    throw new InvalidDataException("The PDF conversion worker uses an incompatible protocol version.");
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (OperationCanceledException) when (startupTimeoutToken.IsCancellationRequested)
            {
                throw new TimeoutException("The PDF conversion worker did not complete startup within 30 seconds.");
            }
        }
    }
}
