using PDFtoImage.Internals;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

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
    public static class Conversion
    {
        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
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
        public static void SaveJpeg(Stream imageStream, string pdfAsBase64String, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Jpeg, pdfAsBase64String, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
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
        public static void SaveJpeg(Stream imageStream, byte[] pdfAsByteArray, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Jpeg, pdfAsByteArray, password, page, options);
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
        public static void SaveJpeg(Stream imageStream, Stream pdfStream, bool leaveOpen = false, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Jpeg, pdfStream, leaveOpen, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
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
        public static void SavePng(Stream imageStream, string pdfAsBase64String, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Png, pdfAsBase64String, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
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
        public static void SavePng(Stream imageStream, byte[] pdfAsByteArray, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Png, pdfAsByteArray, password, page, options);
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
        public static void SavePng(Stream imageStream, Stream pdfStream, bool leaveOpen = false, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Png, pdfStream, leaveOpen, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
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
        public static void SaveWebp(Stream imageStream, string pdfAsBase64String, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Webp, pdfAsBase64String, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
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
        public static void SaveWebp(Stream imageStream, byte[] pdfAsByteArray, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Webp, pdfAsByteArray, password, page, options);
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
        public static void SaveWebp(Stream imageStream, Stream pdfStream, bool leaveOpen = false, string? password = null, int page = 0, RenderOptions options = default)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Webp, pdfStream, leaveOpen, password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF into an image.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The converted PDF page as an image.</returns>
        public static SKBitmap ToImage(string pdfAsBase64String, string? password = null, int page = 0, RenderOptions options = default)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            return ToImage(Convert.FromBase64String(pdfAsBase64String), password, page, options);
        }

        /// <summary>
        /// Renders a single page of a given PDF into an image.
        /// </summary>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The converted PDF page as an image.</returns>
        public static SKBitmap ToImage(
            byte[] pdfAsByteArray,
            string? password = null,
            int page = 0,
            RenderOptions options = default)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            return ToImage(pdfStream, false, password, page, options);
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
        public static SKBitmap ToImage(Stream pdfStream, bool leaveOpen = false, string? password = null, int page = 0, RenderOptions options = default)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            if (page < 0)
                throw new ArgumentOutOfRangeException(nameof(page), "The page number must be 0 or greater.");

            if (options == default)
                options = new();

            // correct the width and height for the given dpi
            // but only if both width and height are not specified (so the original sizes are corrected)
            var correctFromDpi = options.Width == null && options.Height == null;

            NativeMethods.FPDF renderFlags = default;

            if (options.WithAnnotations)
                renderFlags |= NativeMethods.FPDF.ANNOT;

            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Text))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHTEXT;
            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Images))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHIMAGE;
            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Paths))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHPATH;

            // Stream -> Internals.PdfDocument
            using var pdfDocument = PdfDocument.Load(pdfStream, password, !leaveOpen);

            if (page >= pdfDocument.PageSizes.Count)
                throw new ArgumentOutOfRangeException(nameof(page), $"The page number {page} does not exist. Highest page number available is {pdfDocument.PageSizes.Count - 1}.");

            var currentWidth = (float?)options.Width;
            var currentHeight = (float?)options.Height;
            var pageSize = pdfDocument.PageSizes[page];

            // correct aspect ratio if requested
            if (options.WithAspectRatio)
                AdjustForAspectRatio(ref currentWidth, ref currentHeight, pageSize);

            // Internals.PdfDocument -> Image
            return pdfDocument.Render(page, currentWidth ?? pageSize.Width, currentHeight ?? pageSize.Height, options.Dpi, options.Dpi, options.Rotation, renderFlags, options.WithFormFill, correctFromDpi, options.BackgroundColor ?? SKColors.White, options.Bounds, options.UseTiling);
        }

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
        /// Returns the PDF page size for a given page number.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="page">The specific page to query the size for.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page size containing width and height.</returns>
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
        public static SizeF GetPageSize(Stream pdfStream, bool leaveOpen = false, int page = 0, string? password = null)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            if (page < 0)
                throw new ArgumentOutOfRangeException(nameof(page), "The page number must be 0 or greater.");

            using var pdfDocument = PdfDocument.Load(pdfStream, password, !leaveOpen);

            return pdfDocument.PageSizes[page];
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
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static IEnumerable<SKBitmap> ToImages(Stream pdfStream, bool leaveOpen = false, string? password = null, RenderOptions options = default)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            if (options == default)
                options = new();

            // correct the width and height for the given dpi
            // but only if both width and height are not specified (so the original sizes are corrected)
            var correctFromDpi = options.Width == null && options.Height == null;

            NativeMethods.FPDF renderFlags = default;

            if (options.WithAnnotations)
                renderFlags |= NativeMethods.FPDF.ANNOT;

            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Text))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHTEXT;
            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Images))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHIMAGE;
            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Paths))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHPATH;

            // Stream -> Internals.PdfDocument
            using var pdfDocument = PdfDocument.Load(pdfStream, password, !leaveOpen);

            for (int i = 0; i < pdfDocument.PageSizes.Count; i++)
            {
                var currentWidth = (float?)options.Width;
                var currentHeight = (float?)options.Height;
                var pageSize = pdfDocument.PageSizes[i];

                // correct aspect ratio if requested
                if (options.WithAspectRatio)
                    AdjustForAspectRatio(ref currentWidth, ref currentHeight, pageSize);

                // Internals.PdfDocument -> Image
                yield return pdfDocument.Render(i, currentWidth ?? pageSize.Width, currentHeight ?? pageSize.Height, options.Dpi, options.Dpi, options.Rotation, renderFlags, options.WithFormFill, correctFromDpi, options.BackgroundColor ?? SKColors.White, options.Bounds, options.UseTiling);
            }
        }

#if NET6_0_OR_GREATER
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

        /// <summary>
        /// Renders all pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the conversion. Please note that an ongoing rendering cannot be cancelled (the next page will not be rendered though).</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="options">Additional options for PDF rendering.</param>
        /// <returns>The rendered PDF pages as images.</returns>
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(Stream pdfStream, bool leaveOpen = false, string? password = null, RenderOptions options = default, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            if (options == default)
                options = new();

            // correct the width and height for the given dpi
            // but only if both width and height are not specified (so the original sizes are corrected)
            var correctFromDpi = options.Width == null && options.Height == null;

            NativeMethods.FPDF renderFlags = default;

            if (options.WithAnnotations)
                renderFlags |= NativeMethods.FPDF.ANNOT;

            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Text))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHTEXT;
            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Images))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHIMAGE;
            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Paths))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHPATH;

            // Stream -> Internals.PdfDocument
            using var pdfDocument = await Task.Run(() => PdfDocument.Load(pdfStream, password, !leaveOpen), cancellationToken);

            for (int i = 0; i < pdfDocument.PageSizes.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var currentWidth = (float?)options.Width;
                var currentHeight = (float?)options.Height;
                var pageSize = pdfDocument.PageSizes[i];

                // correct aspect ratio if requested
                if (options.WithAspectRatio)
                    AdjustForAspectRatio(ref currentWidth, ref currentHeight, pageSize);

                // Internals.PdfDocument -> Image
                yield return await Task.Run(() => pdfDocument.Render(i, currentWidth ?? pageSize.Width, currentHeight ?? pageSize.Height, options.Dpi, options.Dpi, options.Rotation, renderFlags, options.WithFormFill, correctFromDpi, options.BackgroundColor ?? SKColors.White, options.Bounds, options.UseTiling, cancellationToken), cancellationToken);
            }
        }
#endif

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

        private static void AdjustForAspectRatio(ref float? width, ref float? height, SizeF pageSize)
        {
            if (width == null && height != null)
            {
                width = pageSize.Width / pageSize.Height * height.Value;
            }
            else if (width != null && height == null)
            {
                height = pageSize.Height / pageSize.Width * width.Value;
            }
        }
    }
}