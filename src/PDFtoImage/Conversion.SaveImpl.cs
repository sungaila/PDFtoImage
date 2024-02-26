using SkiaSharp;
using System;
using System.IO;
using System.Runtime.Versioning;

namespace PDFtoImage
{
    /// <summary>
    /// Provides methods to render PDFs into images.
    /// </summary>
#if NET8_0_OR_GREATER
#pragma warning disable CA1510 // Use ArgumentNullException throw helper
#endif
#if NET6_0_OR_GREATER
    [SupportedOSPlatform("Windows")]
    [SupportedOSPlatform("Linux")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Android31.0")]
#endif
    public static partial class Conversion
    {
        internal static void SaveImpl(string imageFilename, SKEncodedImageFormat format, string pdfAsBase64String, string? password = null, int page = 0, RenderOptions options = default)
        {
            if (imageFilename == null)
                throw new ArgumentNullException(nameof(imageFilename));

            using var fileStream = new FileStream(imageFilename, FileMode.Create, FileAccess.Write);
            SaveImpl(fileStream, format, pdfAsBase64String, password, page, options);
        }

        internal static void SaveImpl(Stream imageStream, SKEncodedImageFormat format, string pdfAsBase64String, string? password = null, int page = 0, RenderOptions options = default)
        {
            if (imageStream == null)
                throw new ArgumentNullException(nameof(imageStream));

            using var bitmap = ToImage(pdfAsBase64String, password, page, options);
            bitmap.Encode(imageStream, format, 100);
        }

        internal static void SaveImpl(string imageFilename, SKEncodedImageFormat format, byte[] pdfAsByteArray, string? password = null, int page = 0, RenderOptions options = default)
        {
            if (imageFilename == null)
                throw new ArgumentNullException(nameof(imageFilename));

            using var fileStream = new FileStream(imageFilename, FileMode.Create, FileAccess.Write);
            SaveImpl(fileStream, format, pdfAsByteArray, password, page, options);
        }

        internal static void SaveImpl(string filename, SKEncodedImageFormat format, Stream pdfStream, bool leaveOpen = false, string? password = null, int page = 0, RenderOptions options = default)
        {
            using var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            SaveImpl(fileStream, format, pdfStream, leaveOpen, password, page, options);
        }

        internal static void SaveImpl(Stream imageStream, SKEncodedImageFormat format, byte[] pdfAsByteArray, string? password = null, int page = 0, RenderOptions options = default)
        {
            if (imageStream == null)
                throw new ArgumentNullException(nameof(imageStream));

            using var bitmap = ToImage(pdfAsByteArray, password, page, options);
            bitmap.Encode(imageStream, format, 100);
        }

        internal static void SaveImpl(Stream stream, SKEncodedImageFormat format, Stream pdfStream, bool leaveOpen = false, string? password = null, int page = 0, RenderOptions options = default)
        {
            using var bitmap = ToImage(pdfStream, leaveOpen, password, page, options);
            bitmap.Encode(stream, format, 100);
        }
    }
}