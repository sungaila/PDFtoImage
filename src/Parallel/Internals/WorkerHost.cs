using SkiaSharp;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Versioning;

namespace PDFtoImage.Parallel.Internals
{
    [SupportedOSPlatform("windows6.2")]
    internal static class WorkerHost
    {
        internal static async Task<int> RunAsync(string pipeName)
        {
            try
            {
                using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                await pipe.ConnectAsync(cancellation.Token).ConfigureAwait(false);

                await PipeProtocol.WriteMessageAsync(
                    pipe,
                    PipeProtocol.CreateMessage(writer =>
                    {
                        writer.Write((byte)WorkerResponse.Hello);
                        writer.Write(PipeProtocol.Version);
                    }),
                    CancellationToken.None).ConfigureAwait(false);

                byte[]? pdf = null;
                string? password = null;

                while (true)
                {
                    var message = await PipeProtocol.ReadMessageAsync(pipe, CancellationToken.None).ConfigureAwait(false);
                    if (message == null)
                        return 0;

                    using var reader = PipeProtocol.CreateReader(message);
                    var command = (WorkerCommand)reader.ReadByte();

                    if (command == WorkerCommand.Shutdown)
                        return 0;

                    try
                    {
                        byte[] response;
                        switch (command)
                        {
                            case WorkerCommand.LoadDocument:
                                password = PipeProtocol.ReadNullableString(reader);
                                var length = reader.ReadInt32();
                                if (length < 0 || length > message.Length)
                                    throw new InvalidDataException("The PDF payload has an invalid length.");

                                pdf = reader.ReadBytes(length);
                                if (pdf.Length != length)
                                    throw new EndOfStreamException("The PDF payload is incomplete.");

                                var pageCount = global::PDFtoImage.Conversion.GetPageCount(pdf, password);
                                response = PipeProtocol.CreateMessage(writer =>
                                {
                                    writer.Write((byte)WorkerResponse.Success);
                                    writer.Write(pageCount);
                                });
                                break;

                            case WorkerCommand.RenderPage:
                                if (pdf == null)
                                    throw new InvalidOperationException("No PDF document has been loaded.");

                                var page = reader.ReadInt32();
                                var options = PipeProtocol.ReadRenderOptions(reader);
                                using (var bitmap = global::PDFtoImage.Conversion.ToImage(pdf, page, password, options))
                                {
                                    response = PipeProtocol.CreateMessage(writer =>
                                    {
                                        writer.Write((byte)WorkerResponse.Success);
                                        PipeProtocol.WriteBitmap(writer, bitmap);
                                    });
                                }
                                break;

                            default:
                                throw new InvalidDataException("The worker received an unknown command.");
                        }

                        await PipeProtocol.WriteMessageAsync(pipe, response, CancellationToken.None).ConfigureAwait(false);
                    }
                    catch (Exception exception)
                    {
                        await PipeProtocol.WriteMessageAsync(pipe, PipeProtocol.CreateErrorResponse(exception), CancellationToken.None).ConfigureAwait(false);
                    }
                }
            }
            catch
            {
                return 1;
            }
        }
    }
}
