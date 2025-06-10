using PDFtoImage.Internals;
using SkiaSharp;
using System;
using System.Collections.Generic;
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
#pragma warning disable IDE0079
#pragma warning disable CA1510
#pragma warning restore IDE0079
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
        internal static IEnumerable<SKBitmap> ToImagesImpl(Stream pdfStream, bool leaveOpen, string? password, RenderOptions options, IEnumerable<int>? pages)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            // Stream -> Internals.PdfDocument
            using var pdfDocument = PdfDocument.Load(pdfStream, password, !leaveOpen);

            foreach (var bitmap in ToImagesImpl(pdfDocument, options, pages))
            {
                yield return bitmap;
            }
        }

        internal static IEnumerable<SKBitmap> ToImagesImpl(PdfDocument pdfDocument, RenderOptions options, IEnumerable<int>? pages)
        {
            if (options == default)
                options = new();

            pages ??= Enumerable.Range(0, pdfDocument.PageSizes.Count);

            foreach (var page in pages.OrderBy(i => i).Distinct())
            {
                // Internals.PdfDocument -> Image
                yield return RenderImpl(pdfDocument, page, GetRenderFlags(options), options);
            }
        }

#if NET6_0_OR_GREATER
        internal static async IAsyncEnumerable<SKBitmap> ToImagesImplAsync(Stream pdfStream, bool leaveOpen, string? password, RenderOptions options, IEnumerable<int>? pages, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            if (options == default)
                options = new();

            // Stream -> Internals.PdfDocument
            using var pdfDocument = await Task.Run(() => PdfDocument.Load(pdfStream, password, !leaveOpen), cancellationToken);

            pages ??= Enumerable.Range(0, pdfDocument.PageSizes.Count);

            foreach (var page in pages.OrderBy(i => i).Distinct())
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Internals.PdfDocument -> Image
                yield return await Task.Run(() => RenderImpl(pdfDocument, page, GetRenderFlags(options), options), cancellationToken);
            }
        }
#endif

        private static NativeMethods.FPDF GetRenderFlags(RenderOptions options)
        {
            NativeMethods.FPDF renderFlags = default;

            if (options.WithAnnotations)
                renderFlags |= NativeMethods.FPDF.ANNOT;

            if (options.Grayscale)
                renderFlags |= NativeMethods.FPDF.GRAYSCALE;

            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Text))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHTEXT;
            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Images))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHIMAGE;
            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Paths))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHPATH;

            return renderFlags;
        }

        private static SKBitmap RenderImpl(PdfDocument pdfDocument, int page, NativeMethods.FPDF renderFlags, RenderOptions options)
        {
            return pdfDocument.Render(
                    page,
                    options.Width,
                    options.Height,
                    options.Dpi,
                    options.Dpi,
                    options.Rotation,
                    renderFlags,
                    options.WithFormFill,
                    options.BackgroundColor ?? SKColors.White,
                    options.Bounds,
                    options.UseTiling,
                    options.WithAspectRatio,
                    options.DpiRelativeToBounds);
        }
    }
}