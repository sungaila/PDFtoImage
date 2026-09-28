using System;
using System.IO;
using System.Runtime.Versioning;

namespace PDFtoImage.Parallel.Internals
{
    [SupportedOSPlatform("windows10.0")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    internal static class WorkerHost
    {
        internal static int Run(Stream stream)
        {
            try
            {
                WorkerProtocol.WriteMessage(
                    stream,
                    WorkerProtocol.CreateMessage(writer =>
                    {
                        writer.Write((byte)WorkerResponse.Hello);
                        writer.Write(WorkerProtocol.Version);
                    }));

                WorkerDocument? document = null;
                Guid? documentId = null;

                try
                {
                    while (true)
                    {
                        var message = WorkerProtocol.ReadMessage(stream);

                        if (message == null)
                            return 0;

                        using var reader = WorkerProtocol.CreateReader(message);
                        var command = (WorkerCommand)reader.ReadByte();

                        try
                        {
                            byte[] response;

                            switch (command)
                            {
                                case WorkerCommand.LoadDocument:
                                    var password = WorkerProtocol.ReadNullableString(reader);
                                    var length = reader.ReadInt32();
                                    var documentIdBytes = reader.ReadBytes(16);

                                    if (length < 0 || documentIdBytes.Length != 16 || length != message.Length - reader.BaseStream.Position)
                                        throw new InvalidDataException("The PDF request has an invalid payload or identifier.");

                                    var loadedDocumentId = new Guid(documentIdBytes);

                                    document?.Dispose();
                                    document = null;
                                    documentId = null;

                                    var pdfStream = new MemoryStream(message, (int)reader.BaseStream.Position, length, false);

                                    try
                                    {
                                        document = new WorkerDocument(pdfStream, password);
                                        documentId = loadedDocumentId;
                                    }
                                    catch
                                    {
                                        document?.Dispose();
                                        document = null;
                                        pdfStream.Dispose();
                                        throw;
                                    }
                                    response = WorkerProtocol.CreateMessage(writer =>
                                    {
                                        writer.Write((byte)WorkerResponse.Success);
                                        writer.Write(document.PageCount);
                                    });

                                    break;

                                case WorkerCommand.LoadDocumentFile:
                                    var filePassword = WorkerProtocol.ReadNullableString(reader);
                                    var fileDocumentIdBytes = reader.ReadBytes(16);
                                    var filePath = reader.ReadString();
                                    if (fileDocumentIdBytes.Length != 16 || reader.BaseStream.Position != reader.BaseStream.Length)
                                        throw new InvalidDataException("The file PDF request has an invalid identifier or path.");

                                    document?.Dispose();
                                    document = null;
                                    documentId = null;
                                    var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read | FileShare.Delete, 4096, FileOptions.RandomAccess);
                                    try
                                    {
                                        document = new WorkerDocument(fileStream, filePassword);
                                        documentId = new Guid(fileDocumentIdBytes);
                                    }
                                    catch
                                    {
                                        document?.Dispose();
                                        document = null;
                                        fileStream.Dispose();
                                        throw;
                                    }

                                    response = WorkerProtocol.CreateMessage(writer =>
                                    {
                                        writer.Write((byte)WorkerResponse.Success);
                                        writer.Write(document.PageCount);
                                    });
                                    break;

                                case WorkerCommand.RenderPage:
                                    if (document == null)
                                        throw new InvalidOperationException("No PDF document has been loaded.");

                                    var page = reader.ReadInt32();
                                    var bitmapPath = WorkerProtocol.ReadNullableString(reader);
                                    var options = WorkerProtocol.ReadRenderOptions(reader);

                                    if (reader.BaseStream.Position != reader.BaseStream.Length)
                                        throw new InvalidDataException("The render request contains unexpected trailing data.");

                                    WorkerBitmapRenderer.Render(stream, document, page, options, bitmapPath);

                                    continue;

                                case WorkerCommand.UnloadDocument:
                                    var unloadDocumentIdBytes = reader.ReadBytes(16);
                                    if (unloadDocumentIdBytes.Length != 16 || reader.BaseStream.Position != reader.BaseStream.Length)
                                        throw new InvalidDataException("The unload request has an invalid identifier.");

                                    if (documentId == new Guid(unloadDocumentIdBytes))
                                    {
                                        document?.Dispose();
                                        document = null;
                                        documentId = null;
                                    }

                                    response = WorkerProtocol.CreateMessage(writer => writer.Write((byte)WorkerResponse.Success));
                                    break;

                                default:
                                    throw new InvalidDataException("The worker received an unknown command.");
                            }

                            WorkerProtocol.WriteMessage(stream, response);
                        }
                        catch (Exception exception)
                        {
                            WorkerProtocol.WriteMessage(stream, WorkerProtocol.CreateErrorResponse(exception));
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