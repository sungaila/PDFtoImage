using PDFtoImage.Internals;
using SkiaSharp;
using System;
using System.Collections.Generic;
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
    [SupportedOSPlatform("MacCatalyst13.5")]
    [SupportedOSPlatform("Android31.0")]
#endif
    public static partial class Conversion
    {
        /// <summary>
        /// Returns the page count of a given PDF.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page count of the given PDF.</returns>
        public static int GetPageCount(Stream pdfStream, bool leaveOpen = false, string? password = null)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            using var pdfDocument = PdfDocument.Load(pdfStream, password, !leaveOpen);

            return pdfDocument.PageSizes.Count;
        }

        /// <summary>
        /// Returns the sizes of all PDF pages.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page sizes containing width and height.</returns>
        public static IList<SizeF> GetPageSizes(Stream pdfStream, bool leaveOpen = false, string? password = null)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            using var pdfDocument = PdfDocument.Load(pdfStream, password, !leaveOpen);

            return pdfDocument.PageSizes.ToList().AsReadOnly();
        }

        /// <summary>
        /// Renders all pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static IEnumerable<SKBitmap> ToImages(Stream pdfStream, bool leaveOpen = false, string? password = null, RenderOptions options = default)
        {
            return ToImagesImpl(pdfStream, leaveOpen, password, options, null);
        }

        /// <summary>
        /// Renders a selection of pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="pages">The specific pages to be converted.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static IEnumerable<SKBitmap> ToImages(Stream pdfStream, IEnumerable<int> pages, bool leaveOpen = false, string? password = null, RenderOptions options = default)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            // Stream -> Internals.PdfDocument
            using var pdfDocument = PdfDocument.Load(pdfStream, password, !leaveOpen);

            var pageCount = pdfDocument.PageSizes.Count;

            if (pages.Any(p => p >= pageCount))
                throw new ArgumentOutOfRangeException(nameof(pages), $"The page numbers must be between 0 and {pageCount - 1}. The PDF has {pageCount} pages in total.");

            foreach (var bitmap in ToImagesImpl(pdfStream, leaveOpen, password, options, pages))
            {
                yield return bitmap;
            }
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        public static void SaveJpeg(string imageFilename, Stream pdfStream, Index page, bool leaveOpen = false, string? password = null, RenderOptions options = default)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Jpeg, pdfStream, page, leaveOpen, password, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        public static void SaveJpeg(Stream imageStream, Stream pdfStream, Index page, bool leaveOpen = false, string? password = null, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Jpeg, pdfStream, page, leaveOpen, password, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        public static void SavePng(string imageFilename, Stream pdfStream, Index page, bool leaveOpen = false, string? password = null, RenderOptions options = default)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Png, pdfStream, page, leaveOpen, password, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        public static void SavePng(Stream imageStream, Stream pdfStream, Index page, bool leaveOpen = false, string? password = null, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Png, pdfStream, page, leaveOpen, password, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        public static void SaveWebp(string imageFilename, Stream pdfStream, Index page, bool leaveOpen = false, string? password = null, RenderOptions options = default)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Webp, pdfStream, page, leaveOpen, password, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        public static void SaveWebp(Stream imageStream, Stream pdfStream, Index page, bool leaveOpen = false, string? password = null, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Webp, pdfStream, page, leaveOpen, password, options);
        }

        /// <summary>
        /// Returns the PDF page size for a given page number.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="page">The specific page to query the size for.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page size containing width and height.</returns>
        public static SizeF GetPageSize(Stream pdfStream, Index page, bool leaveOpen = false, string? password = null)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            using var pdfDocument = PdfDocument.Load(pdfStream, password, !leaveOpen);

            var pageCount = pdfDocument.PageSizes.Count;
            var offset = page.GetOffset(pageCount);

            if (offset >= pageCount)
                throw new ArgumentOutOfRangeException(nameof(page), $"The page number must be between 0 and {pageCount - 1}. The PDF has {pageCount} pages in total.");

            return pdfDocument.PageSizes[offset];
        }

        /// <summary>
        /// Renders a single page of a given PDF into an image.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The rendered PDF page as an image.</returns>
        public static SKBitmap ToImage(Stream pdfStream, Index page, bool leaveOpen = false, string? password = null, RenderOptions options = default)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            // Stream -> Internals.PdfDocument
            using var pdfDocument = PdfDocument.Load(pdfStream, password, !leaveOpen);

            var pageCount = pdfDocument.PageSizes.Count;
            var offset = page.GetOffset(pageCount);

            if (offset >= pageCount)
                throw new ArgumentOutOfRangeException(nameof(page), $"The page number must be between 0 and {pageCount - 1}. The PDF has {pageCount} pages in total.");

            return ToImagesImpl(pdfDocument, options, [offset]).First();
        }

        /// <summary>
        /// Renders a range of pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="pages">The specific pages to be converted.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static IEnumerable<SKBitmap> ToImages(Stream pdfStream, Range pages, bool leaveOpen = false, string? password = null, RenderOptions options = default)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            // Stream -> Internals.PdfDocument
            using var pdfDocument = PdfDocument.Load(pdfStream, password, !leaveOpen);

            var pageCount = pdfDocument.PageSizes.Count;
            var (offset, length) = pages.GetOffsetAndLength(pageCount);

            if (offset + length > pageCount)
                throw new ArgumentOutOfRangeException(nameof(pages), $"The page numbers must be between 0 and {pageCount - 1}. The PDF has {pageCount} pages in total.");

            var pageNumbers = Enumerable.Range(offset, length);

            foreach (var bitmap in ToImagesImpl(pdfStream, leaveOpen, password, options, pageNumbers))
            {
                yield return bitmap;
            }
        }

        /// <summary>
        /// Renders a range of pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="pages">The specific pages to be converted.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the conversion. Please note that an ongoing rendering cannot be cancelled (the next page will not be rendered though).</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(Stream pdfStream, Range pages, bool leaveOpen = false, string? password = null, RenderOptions options = default, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            // Stream -> Internals.PdfDocument
            using var pdfDocument = PdfDocument.Load(pdfStream, password, !leaveOpen);

            var pageCount = pdfDocument.PageSizes.Count;
            var (offset, length) = pages.GetOffsetAndLength(pageCount);

            if (offset + length > pageCount)
                throw new ArgumentOutOfRangeException(nameof(pages), $"The page numbers must be between 0 and {pageCount - 1}. The PDF has {pageCount} pages in total.");

            var pageNumbers = Enumerable.Range(offset, length);

            await foreach (var bitmap in ToImagesImplAsync(pdfStream, leaveOpen, password, options, pageNumbers, cancellationToken))
            {
                yield return bitmap;
            }
        }

        /// <summary>
        /// Renders a selection of pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="pages">The specific pages to be converted.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the conversion. Please note that an ongoing rendering cannot be cancelled (the next page will not be rendered though).</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(Stream pdfStream, IEnumerable<int> pages, bool leaveOpen = false, string? password = null, RenderOptions options = default, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            // Stream -> Internals.PdfDocument
            using var pdfDocument = PdfDocument.Load(pdfStream, password, !leaveOpen);

            var pageCount = pdfDocument.PageSizes.Count;

            if (pages.Any(p => p >= pageCount))
                throw new ArgumentOutOfRangeException(nameof(pages), $"The page numbers must be between 0 and {pageCount - 1}. The PDF has {pageCount} pages in total.");

            await foreach (var bitmap in ToImagesImplAsync(pdfStream, leaveOpen, password, options, pages, cancellationToken))
            {
                yield return bitmap;
            }
        }

        /// <summary>
        /// Renders all pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the conversion. Please note that an ongoing rendering cannot be cancelled (the next page will not be rendered though).</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(Stream pdfStream, bool leaveOpen = false, string? password = null, RenderOptions options = default, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var bitmap in ToImagesImplAsync(pdfStream, leaveOpen, password, options, null, cancellationToken))
            {
                yield return bitmap;
            }
        }

        internal static void SaveImpl(string filename, SKEncodedImageFormat format, Stream pdfStream, Index page, bool leaveOpen = false, string? password = null, RenderOptions options = default)
        {
            using var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            SaveImpl(fileStream, format, pdfStream, page, leaveOpen, password, options);
        }

        internal static void SaveImpl(Stream stream, SKEncodedImageFormat format, Stream pdfStream, Index page, bool leaveOpen = false, string? password = null, RenderOptions options = default)
        {
            using var bitmap = ToImage(pdfStream, page, leaveOpen, password, options);
            bitmap.Encode(stream, format, 100);
        }
#endif
    }
}