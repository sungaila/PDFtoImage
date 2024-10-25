using PDFtoImage.Internals;
using SkiaSharp;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
#if NET6_0_OR_GREATER
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
#endif

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
    [SupportedOSPlatform("iOS13.6")]
    [SupportedOSPlatform("Android31.0")]
#endif
    public static partial class Conversion
    {
        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SaveJpeg with a System.Index instead.")]
#endif
        public static void SaveJpeg(string imageFilename, string pdfAsBase64String, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Jpeg, pdfAsBase64String, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SaveJpeg with a System.Index instead.")]
#endif
        public static void SaveJpeg(Stream imageStream, string pdfAsBase64String, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Jpeg, pdfAsBase64String, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SavePng with a System.Index instead.")]
#endif
        public static void SavePng(string imageFilename, string pdfAsBase64String, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Png, pdfAsBase64String, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SavePng with a System.Index instead.")]
#endif
        public static void SavePng(Stream imageStream, string pdfAsBase64String, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Png, pdfAsBase64String, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SaveWebp with a System.Index instead.")]
#endif
        public static void SaveWebp(string imageFilename, string pdfAsBase64String, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Webp, pdfAsBase64String, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SaveWebp with a System.Index instead.")]
#endif
        public static void SaveWebp(Stream imageStream, string pdfAsBase64String, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Webp, pdfAsBase64String, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF into an image.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The converted PDF page as an image.</returns>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use ToImage with a System.Index instead.")]
#endif
        public static SKBitmap ToImage(string pdfAsBase64String, string? password = null, int page = 0, RenderOptions options = default)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            return ToImage(Convert.FromBase64String(pdfAsBase64String), password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SaveJpeg with a System.Index instead.")]
#endif
        public static void SaveJpeg(string imageFilename, byte[] pdfAsByteArray, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Jpeg, pdfAsByteArray, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SaveJpeg with a System.Index instead.")]
#endif
        public static void SaveJpeg(Stream imageStream, byte[] pdfAsByteArray, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Jpeg, pdfAsByteArray, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SavePng with a System.Index instead.")]
#endif
        public static void SavePng(string imageFilename, byte[] pdfAsByteArray, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Png, pdfAsByteArray, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SavePng with a System.Index instead.")]
#endif
        public static void SavePng(Stream imageStream, byte[] pdfAsByteArray, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Png, pdfAsByteArray, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SaveWebp with a System.Index instead.")]
#endif
        public static void SaveWebp(string imageFilename, byte[] pdfAsByteArray, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Webp, pdfAsByteArray, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SaveWebp with a System.Index instead.")]
#endif
        public static void SaveWebp(Stream imageStream, byte[] pdfAsByteArray, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Webp, pdfAsByteArray, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF into an image.
        /// </summary>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The converted PDF page as an image.</returns>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use ToImage with a System.Index instead.")]
#endif
        public static SKBitmap ToImage(byte[] pdfAsByteArray, string? password = null, int page = 0, RenderOptions options = default)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            return ToImage(pdfStream, false, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SaveJpeg with a System.Index instead.")]
#endif
        public static void SaveJpeg(string imageFilename, Stream pdfStream, bool leaveOpen = false, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Jpeg, pdfStream, leaveOpen, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SaveJpeg with a System.Index instead.")]
#endif
        public static void SaveJpeg(Stream imageStream, Stream pdfStream, bool leaveOpen = false, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Jpeg, pdfStream, leaveOpen, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SavePng with a System.Index instead.")]
#endif
        public static void SavePng(string imageFilename, Stream pdfStream, bool leaveOpen = false, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Png, pdfStream, leaveOpen, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SavePng with a System.Index instead.")]
#endif
        public static void SavePng(Stream imageStream, Stream pdfStream, bool leaveOpen = false, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Png, pdfStream, leaveOpen, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SaveWebp with a System.Index instead.")]
#endif
        public static void SaveWebp(string imageFilename, Stream pdfStream, bool leaveOpen = false, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Webp, pdfStream, leaveOpen, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SaveWebp with a System.Index instead.")]
#endif
        public static void SaveWebp(Stream imageStream, Stream pdfStream, bool leaveOpen = false, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Webp, pdfStream, leaveOpen, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF into an image.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The rendered PDF page as an image.</returns>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use ToImage with a System.Index instead.")]
#endif
        public static SKBitmap ToImage(Stream pdfStream, bool leaveOpen = false, string? password = null, int page = 0, RenderOptions options = default)
        {
            return ToImagesImpl(pdfStream, leaveOpen, password, options, [page]).First();
        }

        /// <summary>
        /// Returns the PDF page size for a given page number.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="page">The specific page to query the size for.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page size containing width and height.</returns>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use GetPageSize with a System.Index instead.")]
#endif
        public static SizeF GetPageSize(string pdfAsBase64String, int page, string? password = null)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            return GetPageSize(Convert.FromBase64String(pdfAsBase64String), page, password);
        }

        /// <summary>
        /// Returns the PDF page size for a given page number.
        /// </summary>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="page">The specific page to query the size for.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page size containing width and height.</returns>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use GetPageSize with a System.Index instead.")]
#endif
        public static SizeF GetPageSize(byte[] pdfAsByteArray, int page, string? password = null)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            return GetPageSize(pdfStream, false, page, password);
        }

        /// <summary>
        /// Returns the PDF page size for a given page number.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="page">The specific page to query the size for.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page size containing width and height.</returns>
#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use GetPageSize with a System.Index instead.")]
#endif
        public static SizeF GetPageSize(Stream pdfStream, bool leaveOpen = false, int page = 0, string? password = null)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            if (page < 0)
                throw new ArgumentOutOfRangeException(nameof(page), "The page number must be 0 or greater.");

            using var pdfDocument = PdfDocument.Load(pdfStream, password, !leaveOpen);

            return pdfDocument.PageSizes[page];
        }

#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SaveImpl with a System.Index instead.")]
#endif
        internal static void SaveImpl(string filename, SKEncodedImageFormat format, Stream pdfStream, bool leaveOpen = false, string? password = null, int page = 0, RenderOptions options = default)
        {
            using var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            SaveImpl(fileStream, format, pdfStream, leaveOpen, password, page, options);
        }

#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SaveImpl with a System.Index instead.")]
#endif
        internal static void SaveImpl(Stream stream, SKEncodedImageFormat format, Stream pdfStream, bool leaveOpen = false, string? password = null, int page = 0, RenderOptions options = default)
        {
            using var bitmap = ToImage(pdfStream, leaveOpen, password, page, options);
            bitmap.Encode(stream, format, 100);
        }

#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SaveImpl with a System.Index instead.")]
#endif
        internal static void SaveImpl(string imageFilename, SKEncodedImageFormat format, byte[] pdfAsByteArray, string? password = null, int page = 0, RenderOptions options = default)
        {
            if (imageFilename == null)
                throw new ArgumentNullException(nameof(imageFilename));

            using var fileStream = new FileStream(imageFilename, FileMode.Create, FileAccess.Write);
            SaveImpl(fileStream, format, pdfAsByteArray, password, page, options);
        }

#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SaveImpl with a System.Index instead.")]
#endif
        internal static void SaveImpl(Stream imageStream, SKEncodedImageFormat format, byte[] pdfAsByteArray, string? password = null, int page = 0, RenderOptions options = default)
        {
            if (imageStream == null)
                throw new ArgumentNullException(nameof(imageStream));

            using var bitmap = ToImage(pdfAsByteArray, password, page, options);
            bitmap.Encode(imageStream, format, 100);
        }

#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SaveImpl with a System.Index instead.")]
#endif
        internal static void SaveImpl(string imageFilename, SKEncodedImageFormat format, string pdfAsBase64String, string? password = null, int page = 0, RenderOptions options = default)
        {
            if (imageFilename == null)
                throw new ArgumentNullException(nameof(imageFilename));

            using var fileStream = new FileStream(imageFilename, FileMode.Create, FileAccess.Write);
            SaveImpl(fileStream, format, pdfAsBase64String, password, page, options);
        }

#if NET6_0_OR_GREATER
        [Obsolete("This method is deprecated and will be removed in a future release. Use SaveImpl with a System.Index instead.")]
#endif
        internal static void SaveImpl(Stream imageStream, SKEncodedImageFormat format, string pdfAsBase64String, string? password = null, int page = 0, RenderOptions options = default)
        {
            if (imageStream == null)
                throw new ArgumentNullException(nameof(imageStream));

            using var bitmap = ToImage(pdfAsBase64String, password, page, options);
            bitmap.Encode(imageStream, format, 100);
        }
    }
}