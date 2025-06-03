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
#pragma warning disable IDE0079
#pragma warning disable CA1510
#pragma warning restore IDE0079
    public static partial class Conversion
    {
        /// <summary>
        /// Returns the page count of a given PDF.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page count of the given PDF.</returns>
        public static int GetPageCount(string pdfAsBase64String, string? password = null)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            return GetPageCount(Convert.FromBase64String(pdfAsBase64String), password);
        }

        /// <summary>
        /// Returns the sizes of all PDF pages.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page sizes containing width and height.</returns>
        public static IList<SizeF> GetPageSizes(string pdfAsBase64String, string? password = null)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            return GetPageSizes(Convert.FromBase64String(pdfAsBase64String), password);
        }

        /// <summary>
        /// Renders all pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static IEnumerable<SKBitmap> ToImages(string pdfAsBase64String, string? password = null, RenderOptions options = default)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            foreach (var image in ToImages(Convert.FromBase64String(pdfAsBase64String), password, options))
            {
                yield return image;
            }
        }

        /// <summary>
        /// Renders all pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="pages">The specific pages to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static IEnumerable<SKBitmap> ToImages(string pdfAsBase64String, IEnumerable<int> pages, string? password = null, RenderOptions options = default)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            foreach (var image in ToImages(Convert.FromBase64String(pdfAsBase64String), pages, password, options))
            {
                yield return image;
            }
        }

        /// <summary>
        /// Returns the PDF page size for a given page number.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="page">The specific page to query the size for.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page size containing width and height.</returns>
        public static SizeF GetPageSize(string pdfAsBase64String, Index page = default, string? password = null)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            return GetPageSize(Convert.FromBase64String(pdfAsBase64String), page, password);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        public static void SaveJpeg(string imageFilename, string pdfAsBase64String, Index page = default, string? password = null, RenderOptions options = default)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Jpeg, pdfAsBase64String, page, password, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        public static void SaveJpeg(Stream imageStream, string pdfAsBase64String, Index page = default, string? password = null, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Jpeg, pdfAsBase64String, page, password, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        public static void SavePng(string imageFilename, string pdfAsBase64String, Index page = default, string? password = null, RenderOptions options = default)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Png, pdfAsBase64String, page, password, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        public static void SavePng(Stream imageStream, string pdfAsBase64String, Index page = default, string? password = null, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Png, pdfAsBase64String, page, password, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        public static void SaveWebp(string imageFilename, string pdfAsBase64String, Index page = default, string? password = null, RenderOptions options = default)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Webp, pdfAsBase64String, page, password, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        public static void SaveWebp(Stream imageStream, string pdfAsBase64String, Index page = default, string? password = null, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Webp, pdfAsBase64String, page, password, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF into an image.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The converted PDF page as an image.</returns>
        public static SKBitmap ToImage(string pdfAsBase64String, Index page = default, string? password = null, RenderOptions options = default)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            return ToImage(Convert.FromBase64String(pdfAsBase64String), page, password, options);
        }

        /// <summary>
        /// Renders a range of pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="pages">The specific pages to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static IEnumerable<SKBitmap> ToImages(string pdfAsBase64String, Range pages, string? password = null, RenderOptions options = default)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            foreach (var image in ToImages(Convert.FromBase64String(pdfAsBase64String), pages, password, options))
            {
                yield return image;
            }
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Renders a range of pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="pages">The specific pages to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the conversion. Please note that an ongoing rendering cannot be cancelled (the next page will not be rendered though).</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(string pdfAsBase64String, Range pages, string? password = null, RenderOptions options = default, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            await foreach (var image in ToImagesAsync(Convert.FromBase64String(pdfAsBase64String), pages, password, options, cancellationToken))
            {
                yield return image;
            }
        }

        /// <summary>
        /// Renders a range of pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="pages">The specific pages to be converted.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the conversion. Please note that an ongoing rendering cannot be cancelled (the next page will not be rendered though).</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(string pdfAsBase64String, IEnumerable<int> pages, string? password = null, RenderOptions options = default, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            await foreach (var image in ToImagesAsync(Convert.FromBase64String(pdfAsBase64String), pages, password, options, cancellationToken))
            {
                yield return image;
            }
        }

        /// <summary>
        /// Renders all pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the conversion. Please note that an ongoing rendering cannot be cancelled (the next page will not be rendered though).</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(string pdfAsBase64String, string? password = null, RenderOptions options = default, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            await foreach (var image in ToImagesAsync(Convert.FromBase64String(pdfAsBase64String), password, options, cancellationToken))
            {
                yield return image;
            }
        }
#endif

        internal static void SaveImpl(string imageFilename, SKEncodedImageFormat format, string pdfAsBase64String, Index page = default, string? password = null, RenderOptions options = default)
        {
            if (imageFilename == null)
                throw new ArgumentNullException(nameof(imageFilename));

            using var fileStream = new FileStream(imageFilename, FileMode.Create, FileAccess.Write);
            SaveImpl(fileStream, format, pdfAsBase64String, page, password, options);
        }

        internal static void SaveImpl(Stream imageStream, SKEncodedImageFormat format, string pdfAsBase64String, Index page = default, string? password = null, RenderOptions options = default)
        {
            if (imageStream == null)
                throw new ArgumentNullException(nameof(imageStream));

            using var bitmap = ToImage(pdfAsBase64String, page, password, options);
            bitmap.Encode(imageStream, format, 100);
        }
    }
}