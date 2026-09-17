using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Parallel.Internals
{
    [SupportedOSPlatform("windows10.0")]
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

                WorkerDocument? document = null;

                try
                {
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
                                    var password = PipeProtocol.ReadNullableString(reader);
                                    var length = reader.ReadInt32();
                                    if (length < 0 || length != message.Length - reader.BaseStream.Position)
                                        throw new InvalidDataException("The PDF payload has an invalid length.");

                                    document?.Dispose();
                                    document = null;
                                    var pdfStream = new MemoryStream(message, (int)reader.BaseStream.Position, length, false);
                                    try
                                    {
                                        document = new WorkerDocument(pdfStream, password);
                                    }
                                    catch
                                    {
                                        pdfStream.Dispose();
                                        throw;
                                    }
                                    response = PipeProtocol.CreateMessage(writer =>
                                    {
                                        writer.Write((byte)WorkerResponse.Success);
                                        writer.Write(document.PageCount);
                                    });
                                    break;

                                case WorkerCommand.RenderPage:
                                    if (document == null)
                                        throw new InvalidOperationException("No PDF document has been loaded.");

                                    var page = reader.ReadInt32();
                                    var options = PipeProtocol.ReadRenderOptions(reader);
                                    using (var bitmap = document.Render(page, options))
                                    {
                                        await PipeProtocol.WriteBitmapResponseAsync(pipe, bitmap, CancellationToken.None).ConfigureAwait(false);
                                    }
                                    continue;

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
                finally
                {
                    document?.Dispose();
                }
            }
            catch
            {
                return 1;
            }
        }
    }
}
