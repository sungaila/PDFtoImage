using SkiaSharp;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Parallel.Internals
{
    [SupportedOSPlatform("windows10.0")]
    internal sealed class WorkerConnectionWindows : IDisposable
    {
        private readonly NamedPipeServerStream _pipe;

        private Process? _process;

        private int _disposed;

        private Guid? _documentId;

        private int _pageCount;

        private int _documentLoadCount;

        private WorkerConnectionWindows(NamedPipeServerStream pipe)
        {
            _pipe = pipe;
        }

        internal int ProcessId => _process?.Id ?? 0;

        internal Guid? DocumentId => _documentId;

        internal int DocumentLoadCount => _documentLoadCount;

        internal static async Task<WorkerConnectionWindows> StartAsync(WindowsJob job, CancellationToken cancellationToken)
        {
            var pipeName = "PDFtoImage.Parallel." + Guid.NewGuid().ToString("N");

            var pipe = new NamedPipeServerStream(
                pipeName,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

            var worker = new WorkerConnectionWindows(pipe);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                worker._process = WorkerProcessLauncherWindows.StartSuspendedAndAssign(job, pipeName);

                using var startupTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                using var startupCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, startupTimeout.Token);
                worker._process.EnableRaisingEvents = true;

                void onExit(object? _1, EventArgs _2)
                {
                    // Unsubscribing cannot retract an already queued Exited callback.
                    try
                    {
                        startupCancellation.Cancel();
                    }
                    catch (ObjectDisposedException) { }
                }
                worker._process.Exited += onExit;

                try
                {
                    if (worker._process.HasExited)
                        throw new EndOfStreamException("The PDF conversion worker exited before connecting.");

                    await pipe.WaitForConnectionAsync(startupCancellation.Token).ConfigureAwait(false);

                    await ReadHelloAsync(pipe, cancellationToken, startupTimeout.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (TimeoutException) when (worker._process.HasExited)
                {
                    throw new EndOfStreamException("The PDF conversion worker exited during startup.");
                }
                catch (OperationCanceledException) when (worker._process.HasExited)
                {
                    throw new EndOfStreamException("The PDF conversion worker exited during startup.");
                }
                catch (OperationCanceledException) when (startupTimeout.IsCancellationRequested)
                {
                    throw new TimeoutException("The PDF conversion worker did not complete startup within 30 seconds.");
                }
                finally
                {
                    worker._process.Exited -= onExit;
                }

                return worker;
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

            var request = PipeProtocol.CreateMessage(writer =>
            {
                writer.Write((byte)WorkerCommand.UnloadDocument);
                writer.Write(requestId.ToByteArray());
            });
            await PipeProtocol.WriteMessageAsync(_pipe, request, cancellationToken).ConfigureAwait(false);
            var response = await ReadRequiredMessageAsync(_pipe, cancellationToken).ConfigureAwait(false);
            using var reader = PipeProtocol.CreateReader(response);
            PipeProtocol.ThrowIfError(reader);
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

            var header = PipeProtocol.CreateMessage(writer =>
            {
                writer.Write((byte)WorkerCommand.LoadDocument);
                PipeProtocol.WriteNullableString(writer, request.Password);
                writer.Write(request.Bytes.Length);
                writer.Write(request.Id.ToByteArray());
            });

            try
            {
                await PipeProtocol.WriteMessageAsync(_pipe, header, request.Bytes, cancellationToken).ConfigureAwait(false);

                var response = await ReadRequiredMessageAsync(_pipe, cancellationToken).ConfigureAwait(false);

                using var reader = PipeProtocol.CreateReader(response);

                PipeProtocol.ThrowIfError(reader);
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
                var request = PipeProtocol.CreateMessage(writer =>
                {
                    writer.Write((byte)WorkerCommand.RenderPage);
                    writer.Write(page);
                    PipeProtocol.WriteRenderOptions(writer, options);
                });

                await PipeProtocol.WriteMessageAsync(_pipe, request, cancellationToken).ConfigureAwait(false);

                var response = await ReadRequiredMessageAsync(_pipe, cancellationToken).ConfigureAwait(false);
                using var reader = PipeProtocol.CreateReader(response);

                PipeProtocol.ThrowIfError(reader);

                return PipeProtocol.ReadBitmap(response, checked((int)reader.BaseStream.Position));
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

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            _pipe.Dispose();

            var process = Interlocked.Exchange(ref _process, null);

            if (process == null)
                return;

            try
            {
                if (!process.HasExited)
                    process.Kill();

                process.WaitForExit();
            }
            catch (InvalidOperationException) { /* The process already exited. */ }
            catch (Win32Exception) when (process.HasExited) { /* The job terminated it concurrently. */ }
            finally
            {
                process.Dispose();
            }
        }

        private static async Task<byte[]> ReadRequiredMessageAsync(Stream stream, CancellationToken cancellationToken)
        {
            return await PipeProtocol.ReadMessageAsync(stream, cancellationToken).ConfigureAwait(false)
                ?? throw new EndOfStreamException("The PDF conversion worker closed its IPC pipe unexpectedly.");
        }

        internal static async Task ReadHelloAsync(Stream stream, CancellationToken cancellationToken, CancellationToken startupTimeoutToken)
        {
            using var startupCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, startupTimeoutToken);
            try
            {
                var helloMessage = await PipeProtocol.ReadMessageAsync(stream, startupCancellation.Token).ConfigureAwait(false)
                    ?? throw new EndOfStreamException("The PDF conversion worker exited during startup.");

                using var helloReader = PipeProtocol.CreateReader(helloMessage);
                if ((WorkerResponse)helloReader.ReadByte() != WorkerResponse.Hello || helloReader.ReadInt32() != PipeProtocol.Version ||
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