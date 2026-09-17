using PDFtoImage;
using SkiaSharp;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Parallel.Internals
{
    internal enum WorkerCommand : byte
    {
        LoadDocument = 1,
        RenderPage = 2,
        Shutdown = 3
    }

    internal enum WorkerResponse : byte
    {
        Success = 1,
        Error = 2,
        Hello = 3
    }

    internal static class PipeProtocol
    {
        internal const int Version = 2;
        private const int MaximumMessageLength = 1024 * 1024 * 1024;

        internal static byte[] CreateMessage(Action<BinaryWriter> write)
        {
            using var stream = new MemoryStream();
            using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                write(writer);

            return stream.ToArray();
        }

        internal static async Task WriteMessageAsync(Stream stream, byte[] message, CancellationToken cancellationToken)
        {
            if (message.Length > MaximumMessageLength)
                throw new InvalidDataException("The IPC message is too large.");

            var header = BitConverter.GetBytes(message.Length);
            await stream.WriteAsync(header, cancellationToken).ConfigureAwait(false);
            await stream.WriteAsync(message, cancellationToken).ConfigureAwait(false);
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
            if (messageLength < 0 || messageLength > MaximumMessageLength)
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

        internal static void WriteRenderOptions(BinaryWriter writer, RenderOptions options)
        {
            writer.Write(options.Dpi);
            WriteNullableInt32(writer, options.Width);
            WriteNullableInt32(writer, options.Height);
            writer.Write(options.WithAnnotations);
            writer.Write(options.WithFormFill);
            writer.Write(options.WithAspectRatio);
            writer.Write((int)options.Rotation);
            writer.Write((int)options.AntiAliasing);

            writer.Write(options.BackgroundColor.HasValue);
            if (options.BackgroundColor.HasValue)
            {
                var color = options.BackgroundColor.Value;
                writer.Write(color.Red);
                writer.Write(color.Green);
                writer.Write(color.Blue);
                writer.Write(color.Alpha);
            }

            writer.Write(options.Bounds.HasValue);
            if (options.Bounds.HasValue)
            {
                var bounds = options.Bounds.Value;
                writer.Write(bounds.X);
                writer.Write(bounds.Y);
                writer.Write(bounds.Width);
                writer.Write(bounds.Height);
            }

            writer.Write(options.UseTiling);
            writer.Write(options.DpiRelativeToBounds);
            writer.Write(options.Grayscale);
        }

        internal static RenderOptions ReadRenderOptions(BinaryReader reader)
        {
            var dpi = reader.ReadInt32();
            var width = ReadNullableInt32(reader);
            var height = ReadNullableInt32(reader);
            var withAnnotations = reader.ReadBoolean();
            var withFormFill = reader.ReadBoolean();
            var withAspectRatio = reader.ReadBoolean();
            var rotation = (PdfRotation)reader.ReadInt32();
            var antiAliasing = (PdfAntiAliasing)reader.ReadInt32();

            SKColor? backgroundColor = null;
            if (reader.ReadBoolean())
                backgroundColor = new SKColor(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());

            RectangleF? bounds = null;
            if (reader.ReadBoolean())
                bounds = new RectangleF(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            return new RenderOptions(
                dpi,
                width,
                height,
                withAnnotations,
                withFormFill,
                withAspectRatio,
                rotation,
                antiAliasing,
                backgroundColor,
                bounds,
                reader.ReadBoolean(),
                reader.ReadBoolean(),
                reader.ReadBoolean());
        }

        internal static void WriteBitmap(BinaryWriter writer, SKBitmap bitmap)
        {
            writer.Write(bitmap.Width);
            writer.Write(bitmap.Height);
            writer.Write((int)bitmap.ColorType);
            writer.Write((int)bitmap.AlphaType);
            writer.Write(bitmap.RowBytes);
            writer.Write(bitmap.ByteCount);

            var pixels = new byte[bitmap.ByteCount];
            Marshal.Copy(bitmap.GetPixels(), pixels, 0, pixels.Length);
            writer.Write(pixels);
        }

        internal static SKBitmap ReadBitmap(byte[] payload)
        {
            using var reader = CreateReader(payload);
            var width = reader.ReadInt32();
            var height = reader.ReadInt32();
            var colorType = (SKColorType)reader.ReadInt32();
            var alphaType = (SKAlphaType)reader.ReadInt32();
            var rowBytes = reader.ReadInt32();
            var byteCount = reader.ReadInt32();

            if (width <= 0 || height <= 0 || rowBytes <= 0 || byteCount <= 0 || byteCount > payload.Length)
                throw new InvalidDataException("A worker returned invalid bitmap metadata.");

            var bitmap = new SKBitmap(width, height, colorType, alphaType);
            try
            {
                if (bitmap.RowBytes != rowBytes || bitmap.ByteCount != byteCount)
                    throw new InvalidDataException("A worker returned incompatible bitmap metadata.");

                var pixels = reader.ReadBytes(byteCount);
                if (pixels.Length != byteCount || reader.BaseStream.Position != reader.BaseStream.Length)
                    throw new InvalidDataException("A worker returned incomplete bitmap data.");

                Marshal.Copy(pixels, 0, bitmap.GetPixels(), pixels.Length);
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

        private static void WriteNullableInt32(BinaryWriter writer, int? value)
        {
            writer.Write(value.HasValue);
            if (value.HasValue)
                writer.Write(value.Value);
        }

        private static int? ReadNullableInt32(BinaryReader reader)
        {
            return reader.ReadBoolean() ? reader.ReadInt32() : null;
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
