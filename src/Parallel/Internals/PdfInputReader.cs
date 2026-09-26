using System;
using System.Buffers;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Parallel.Internals
{
    internal static class PdfInputReader
    {
        private const int BufferSize = 81920;

        internal static async Task<byte[]> ReadAsync(Stream stream, int maximumLength, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(stream);
            ArgumentOutOfRangeException.ThrowIfNegative(maximumLength);

            if (stream.CanSeek && stream.CanRead)
            {
                var remaining = stream.Length - stream.Position;

                if (remaining >= 0)
                {
                    ThrowIfTooLarge(remaining, maximumLength);

                    var bytes = new byte[(int)remaining];
                    await stream.ReadExactlyAsync(bytes, cancellationToken).ConfigureAwait(false);
                    return bytes;
                }
            }

            using var output = new MemoryStream();
            var buffer = ArrayPool<byte>.Shared.Rent(BufferSize);

            try
            {
                while (output.Length < maximumLength)
                {
                    var count = (int)Math.Min(buffer.Length, maximumLength - output.Length);
                    var read = await stream.ReadAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);

                    if (read == 0)
                        return output.ToArray();

                    output.Write(buffer, 0, read);
                }

                // The payload may be exactly at the limit. Probe one more byte so an
                // oversized unknown-length stream is rejected instead of silently truncated.
                if (await stream.ReadAsync(buffer.AsMemory(0, 1), cancellationToken).ConfigureAwait(false) != 0)
                    throw CreateTooLargeException(maximumLength);

                return output.ToArray();
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private static void ThrowIfTooLarge(long length, int maximumLength)
        {
            if (length > maximumLength)
                throw CreateTooLargeException(maximumLength);
        }

        private static InvalidDataException CreateTooLargeException(int maximumLength) =>
            new($"The PDF exceeds the maximum transferable size of {maximumLength} bytes.");
    }
}
