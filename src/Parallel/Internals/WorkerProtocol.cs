using SkiaSharp;
using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Parallel.Internals
{
    internal enum WorkerCommand : byte
    {
        LoadDocument = 1,

        RenderPage = 2,

        UnloadDocument = 3
    }

    internal enum WorkerResponse : byte
    {
        Success = 1,

        Error = 2,

        Hello = 3
    }

    internal static class WorkerProtocol
    {
        internal const int Version = 1;

        private const int MaximumMessageLength = 1024 * 1024 * 1024;

        internal static byte[] CreateMessage(Action<BinaryWriter> write)
        {
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
            {
                write(writer);
            }

            return stream.ToArray();
        }

        internal static Task WriteMessageAsync(Stream stream, byte[] message, CancellationToken cancellationToken) =>
            WriteMessageAsync(stream, message, ReadOnlyMemory<byte>.Empty, cancellationToken);

        internal static async Task WriteMessageAsync(Stream stream, byte[] message, ReadOnlyMemory<byte> suffix, CancellationToken cancellationToken)
        {
            if ((long)message.Length + suffix.Length > MaximumMessageLength)
                throw new InvalidDataException("The IPC message is too large.");

            var header = BitConverter.GetBytes(message.Length + suffix.Length);

            await stream.WriteAsync(header, cancellationToken).ConfigureAwait(false);
            await stream.WriteAsync(message, cancellationToken).ConfigureAwait(false);

            if (!suffix.IsEmpty)
                await stream.WriteAsync(suffix, cancellationToken).ConfigureAwait(false);

            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }

        internal static async Task<byte[]?> ReadMessageAsync(Stream stream, CancellationToken cancellationToken)
        {
            var header = new byte[sizeof(int)];
            var firstRead = await stream.ReadAsync(header, cancellationToken).ConfigureAwait(false);

            if (firstRead == 0)
                return null;

            await ReadExactlyAsync(stream, header, firstRead, header.Length - firstRead, cancellationToken).ConfigureAwait(false);

            var messageLength = BitConverter.ToInt32(header, 0);

            if (messageLength <= 0 || messageLength > MaximumMessageLength)
                throw new InvalidDataException("The IPC message has an invalid length.");

            var message = new byte[messageLength];

            await ReadExactlyAsync(stream, message, 0, message.Length, cancellationToken).ConfigureAwait(false);

            return message;
        }

        internal static BinaryReader CreateReader(byte[] message)
        {
            return new BinaryReader(new MemoryStream(message, false), Encoding.UTF8, false);
        }

        internal static void WriteNullableString(BinaryWriter writer, string? value)
        {
            writer.Write(value != null);

            if (value != null)
                writer.Write(value);
        }

        internal static string? ReadNullableString(BinaryReader reader)
        {
            return reader.ReadBoolean() ? reader.ReadString() : null;
        }

        internal static void WriteRenderOptions(BinaryWriter writer, RenderOptions options) =>
            JsonSerializer.Serialize(writer.BaseStream, options, WorkerJsonSerializerContext.Default.RenderOptions);

        internal static RenderOptions ReadRenderOptions(BinaryReader reader)
        {
            try
            {
                return JsonSerializer.Deserialize(reader.BaseStream, WorkerJsonSerializerContext.Default.RenderOptions);
            }
            catch (JsonException exception)
            {
                throw new InvalidDataException("The render options payload is invalid.", exception);
            }
        }

        internal static unsafe void WriteBitmap(BinaryWriter writer, SKBitmap bitmap)
        {
            writer.Write(bitmap.Width);
            writer.Write(bitmap.Height);
            writer.Write((int)bitmap.ColorType);
            writer.Write((int)bitmap.AlphaType);
            writer.Write(bitmap.RowBytes);
            writer.Write(bitmap.ByteCount);

            writer.Write(new ReadOnlySpan<byte>((void*)bitmap.GetPixels(), bitmap.ByteCount));
        }

        internal static async Task WriteBitmapResponseAsync(Stream stream, SKBitmap bitmap, CancellationToken cancellationToken)
        {
            var metadata = CreateMessage(writer =>
            {
                writer.Write((byte)WorkerResponse.Success);
                writer.Write(bitmap.Width);
                writer.Write(bitmap.Height);
                writer.Write((int)bitmap.ColorType);
                writer.Write((int)bitmap.AlphaType);
                writer.Write(bitmap.RowBytes);
                writer.Write(bitmap.ByteCount);
            });

            if ((long)metadata.Length + bitmap.ByteCount > MaximumMessageLength)
                throw new InvalidDataException("The rendered bitmap exceeds the IPC message limit.");

            await stream.WriteAsync(BitConverter.GetBytes(metadata.Length + bitmap.ByteCount), cancellationToken).ConfigureAwait(false);
            await stream.WriteAsync(metadata, cancellationToken).ConfigureAwait(false);

            var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);

            try
            {
                for (var offset = 0; offset < bitmap.ByteCount;)
                {
                    var count = Math.Min(buffer.Length, bitmap.ByteCount - offset);
                    Marshal.Copy(IntPtr.Add(bitmap.GetPixels(), offset), buffer, 0, count);
                    await stream.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
                    offset += count;
                }

                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer, clearArray: true);
            }
        }

        internal static SKBitmap ReadBitmap(byte[] payload, int offset = 0)
        {
            const int metadataSize = 6 * sizeof(int);

            if (offset < 0 || offset > payload.Length - metadataSize)
                throw new InvalidDataException("A worker returned incomplete bitmap metadata.");

            using var reader = CreateReader(payload);
            reader.BaseStream.Position = offset;

            var width = reader.ReadInt32();
            var height = reader.ReadInt32();
            var colorType = (SKColorType)reader.ReadInt32();
            var alphaType = (SKAlphaType)reader.ReadInt32();
            var rowBytes = reader.ReadInt32();
            var byteCount = reader.ReadInt32();

            // Validate using wide arithmetic BEFORE allocating native memory.
            if (width <= 0 || height <= 0 || colorType != SKColorType.Bgra8888 || alphaType != SKAlphaType.Premul ||
                (long)width * 4 != rowBytes || (long)rowBytes * height != byteCount ||
                byteCount != payload.Length - offset - metadataSize)
                throw new InvalidDataException("A worker returned invalid bitmap metadata.");

            var bitmap = new SKBitmap(width, height, colorType, alphaType);

            try
            {
                if (bitmap.RowBytes != rowBytes || bitmap.ByteCount != byteCount)
                    throw new InvalidDataException("A worker returned incompatible bitmap metadata.");

                Marshal.Copy(payload, offset + metadataSize, bitmap.GetPixels(), byteCount);
                return bitmap;
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }
        }

        internal static byte[] CreateErrorResponse(Exception exception)
        {
            return CreateMessage(writer =>
            {
                writer.Write((byte)WorkerResponse.Error);
                writer.Write(exception.GetType().FullName ?? exception.GetType().Name);
                writer.Write(exception.Message);
                WriteNullableString(writer, exception.StackTrace);
            });
        }

        internal static void ThrowIfError(BinaryReader reader)
        {
            var response = (WorkerResponse)reader.ReadByte();

            if (response == WorkerResponse.Success)
                return;

            if (response != WorkerResponse.Error)
                throw new InvalidDataException("The worker returned an invalid response.");

            throw new ParallelConversionException(reader.ReadString(), reader.ReadString(), ReadNullableString(reader));
        }

        private static async Task ReadExactlyAsync(Stream stream, byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            while (count > 0)
            {
                var read = await stream.ReadAsync(buffer.AsMemory(offset, count), cancellationToken).ConfigureAwait(false);

                if (read == 0)
                    throw new EndOfStreamException("The worker closed its IPC pipe unexpectedly.");

                offset += read;
                count -= read;
            }
        }
    }
}