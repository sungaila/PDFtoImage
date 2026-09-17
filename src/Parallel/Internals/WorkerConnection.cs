using PDFtoImage;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Versioning;

namespace PDFtoImage.Parallel.Internals
{
    [SupportedOSPlatform("windows6.2")]
    internal sealed class WorkerConnection : IDisposable
    {
        private readonly NamedPipeServerStream _pipe;
        private Process? _process;

        private WorkerConnection(NamedPipeServerStream pipe)
        {
            _pipe = pipe;
        }

        internal int ProcessId => _process?.Id ?? 0;

        internal static async Task<(WorkerConnection Worker, int PageCount)> StartAsync(
            WindowsJob job,
            byte[] pdf,
            string? password,
            CancellationToken cancellationToken)
        {
            var pipeName = "PDFtoImage.Parallel." + Guid.NewGuid().ToString("N");
            var pipe = new NamedPipeServerStream(
                pipeName,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

            var worker = new WorkerConnection(pipe);

            try
            {
                worker._process = WorkerProcessLauncher.StartSuspendedAndAssign(job, pipeName);

                using var startupCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                startupCancellation.CancelAfter(TimeSpan.FromSeconds(30));

                try
                {
                    await pipe.WaitForConnectionAsync(startupCancellation.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    if (worker._process.HasExited)
                        throw new TimeoutException("The PDF conversion worker exited before connecting.");

                    throw new TimeoutException("The PDF conversion worker did not connect within 30 seconds.");
                }

                var helloMessage = await PipeProtocol.ReadMessageAsync(pipe, startupCancellation.Token).ConfigureAwait(false);
                if (helloMessage == null)
                    throw new EndOfStreamException("The PDF conversion worker exited during startup.");

                using (var helloReader = PipeProtocol.CreateReader(helloMessage))
                {
                    if ((WorkerResponse)helloReader.ReadByte() != WorkerResponse.Hello || helloReader.ReadInt32() != PipeProtocol.Version)
                        throw new InvalidDataException("The PDF conversion worker uses an incompatible protocol version.");
                }

                var loadMessage = PipeProtocol.CreateMessage(writer =>
                {
                    writer.Write((byte)WorkerCommand.LoadDocument);
                    PipeProtocol.WriteNullableString(writer, password);
                    writer.Write(pdf.Length);
                    writer.Write(pdf);
                });

                await PipeProtocol.WriteMessageAsync(pipe, loadMessage, cancellationToken).ConfigureAwait(false);
                var loadResponse = await ReadRequiredMessageAsync(pipe, cancellationToken).ConfigureAwait(false);
                using var loadReader = PipeProtocol.CreateReader(loadResponse);
                PipeProtocol.ThrowIfError(loadReader);

                return (worker, loadReader.ReadInt32());
            }
            catch
            {
                worker.Dispose();
                throw;
            }
        }

        internal async Task<byte[]> RenderPageAsync(int page, RenderOptions options, CancellationToken cancellationToken)
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
                return reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
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
            _pipe.Dispose();
            _process?.Dispose();
            _process = null;
        }

        private static async Task<byte[]> ReadRequiredMessageAsync(Stream stream, CancellationToken cancellationToken)
        {
            return await PipeProtocol.ReadMessageAsync(stream, cancellationToken).ConfigureAwait(false)
                ?? throw new EndOfStreamException("The PDF conversion worker closed its IPC pipe unexpectedly.");
        }
    }
}
