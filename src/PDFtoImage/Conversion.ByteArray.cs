using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace PDFtoImage
{
    /// <summary>
    /// Provides methods to render PDFs into images.
    /// </summary>
#if NET8_0_OR_GREATER
#pragma warning disable CA1510 // Use ArgumentNullException throw helper
#endif
    public static partial class Conversion
    {
        /// <summary>
        /// Returns the page count of a given PDF.
        /// </summary>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page count of the given PDF.</returns>
        public static int GetPageCount(byte[] pdfAsByteArray, string? password = null)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            return GetPageCount(pdfStream, false, password);
        }

        /// <summary>
        /// Returns the sizes of all PDF pages.
        /// </summary>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page sizes containing width and height.</returns>
        public static IList<SizeF> GetPageSizes(byte[] pdfAsByteArray, string? password = null)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            return GetPageSizes(pdfStream, false, password);
        }

        /// <summary>
        /// Renders all pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static IEnumerable<SKBitmap> ToImages(byte[] pdfAsByteArray, string? password = null, RenderOptions options = default)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            foreach (var image in ToImages(pdfStream, false, password, options))
            {
                yield return image;
            }
        }

        /// <summary>
        /// Renders all pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="pages">The specific pages to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static IEnumerable<SKBitmap> ToImages(byte[] pdfAsByteArray, IEnumerable<int> pages, string? password = null, RenderOptions options = default)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            foreach (var image in ToImages(pdfStream, pages, false, password, options))
            {
                yield return image;
            }
        }

        /// <summary>
        /// Returns the PDF page size for a given page number.
        /// </summary>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="page">The specific page to query the size for.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page size containing width and height.</returns>
        public static SizeF GetPageSize(byte[] pdfAsByteArray, Index page = default, string? password = null)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            return GetPageSize(pdfStream, page, false, password);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        public static void SaveJpeg(string imageFilename, byte[] pdfAsByteArray, Index page = default, string? password = null, RenderOptions options = default)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Jpeg, pdfAsByteArray, page, password, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        public static void SaveJpeg(Stream imageStream, byte[] pdfAsByteArray, Index page = default, string? password = null, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Jpeg, pdfAsByteArray, page, password, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        public static void SavePng(string imageFilename, byte[] pdfAsByteArray, Index page = default, string? password = null, RenderOptions options = default)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Png, pdfAsByteArray, page, password, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        public static void SavePng(Stream imageStream, byte[] pdfAsByteArray, Index page = default, string? password = null, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Png, pdfAsByteArray, page, password, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        public static void SaveWebp(string imageFilename, byte[] pdfAsByteArray, Index page = default, string? password = null, RenderOptions options = default)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Webp, pdfAsByteArray, page, password, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        public static void SaveWebp(Stream imageStream, byte[] pdfAsByteArray, Index page = default, string? password = null, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Webp, pdfAsByteArray, page, password, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF into an image.
        /// </summary>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The converted PDF page as an image.</returns>
        public static SKBitmap ToImage(byte[] pdfAsByteArray, Index page = default, string? password = null, RenderOptions options = default)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            return ToImage(pdfStream, page, false, password, options);
        }

        /// <summary>
        /// Renders a range of pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="pages">The specific pages to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static IEnumerable<SKBitmap> ToImages(byte[] pdfAsByteArray, Range pages, string? password = null, RenderOptions options = default)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            foreach (var image in ToImages(pdfStream, pages, false, password, options))
            {
                yield return image;
            }
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Renders a range of pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="pages">The specific pages to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the conversion. Please note that an ongoing rendering cannot be cancelled (the next page will not be rendered though).</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(byte[] pdfAsByteArray, Range pages, string? password = null, RenderOptions options = default, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            await foreach (var image in ToImagesAsync(pdfStream, pages, false, password, options, cancellationToken))
            {
                yield return image;
            }
        }

        /// <summary>
        /// Renders a range of pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="pages">The specific pages to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the conversion. Please note that an ongoing rendering cannot be cancelled (the next page will not be rendered though).</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(byte[] pdfAsByteArray, IEnumerable<int> pages, string? password = null, RenderOptions options = default, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            await foreach (var image in ToImagesAsync(pdfStream, pages, false, password, options, cancellationToken))
            {
                yield return image;
            }
        }

        /// <summary>
        /// Renders all pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the conversion. Please note that an ongoing rendering cannot be cancelled (the next page will not be rendered though).</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(byte[] pdfAsByteArray, string? password = null, RenderOptions options = default, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            await foreach (var image in ToImagesAsync(pdfStream, false, password, options, cancellationToken))
            {
                yield return image;
            }
        }
#endif

        internal static void SaveImpl(string imageFilename, SKEncodedImageFormat format, byte[] pdfAsByteArray, Index page = default, string? password = null, RenderOptions options = default)
        {
            if (imageFilename == null)
                throw new ArgumentNullException(nameof(imageFilename));

            using var fileStream = new FileStream(imageFilename, FileMode.Create, FileAccess.Write);
            SaveImpl(fileStream, format, pdfAsByteArray, page, password, options);
        }

        internal static void SaveImpl(Stream imageStream, SKEncodedImageFormat format, byte[] pdfAsByteArray, Index page = default, string? password = null, RenderOptions options = default)
        {
            if (imageStream == null)
                throw new ArgumentNullException(nameof(imageStream));

            using var bitmap = ToImage(pdfAsByteArray, page, password, options);
            bitmap.Encode(imageStream, format, 100);
        }
    }
}