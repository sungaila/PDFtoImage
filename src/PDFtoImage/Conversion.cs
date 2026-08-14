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
    [SupportedOSPlatform("browser")]
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

            foreach (var page in pages)
            {
                // Internals.PdfDocument -> Image
                yield return RenderImpl(pdfDocument, page, GetRenderFlags(options), options, CancellationToken.None);
            }
        }

#if NET6_0_OR_GREATER
        internal static async IAsyncEnumerable<SKBitmap> ToImagesImplAsync(Stream pdfStream, bool leaveOpen, string? password, RenderOptions options, IEnumerable<int>? pages, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            // Stream -> Internals.PdfDocument
            using var pdfDocument = await Task.Run(() => PdfDocument.Load(pdfStream, password, !leaveOpen), cancellationToken);

            await foreach (var bitmap in ToImagesImplAsync(pdfDocument, options, pages, cancellationToken))
            {
                yield return bitmap;
            }
        }

        internal static async IAsyncEnumerable<SKBitmap> ToImagesImplAsync(PdfDocument pdfDocument, RenderOptions options, IEnumerable<int>? pages, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (options == default)
                options = new();

            pages ??= Enumerable.Range(0, pdfDocument.PageSizes.Count);

            foreach (var page in pages)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Internals.PdfDocument -> Image
                yield return await Task.Run(() => RenderImpl(pdfDocument, page, GetRenderFlags(options), options, cancellationToken), cancellationToken);
            }
        }
#endif

        private static NativeMethods.FPDFRenderFlags GetRenderFlags(RenderOptions options)
        {
            NativeMethods.FPDFRenderFlags renderFlags = default;

            if (options.WithAnnotations)
                renderFlags |= NativeMethods.FPDFRenderFlags.ANNOT;

            if (options.Grayscale)
                renderFlags |= NativeMethods.FPDFRenderFlags.GRAYSCALE;

            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Text))
                renderFlags |= NativeMethods.FPDFRenderFlags.RENDER_NO_SMOOTHTEXT;
            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Images))
                renderFlags |= NativeMethods.FPDFRenderFlags.RENDER_NO_SMOOTHIMAGE;
            if (!options.AntiAliasing.HasFlag(PdfAntiAliasing.Paths))
                renderFlags |= NativeMethods.FPDFRenderFlags.RENDER_NO_SMOOTHPATH;

            return renderFlags;
        }

        private static SKBitmap RenderImpl(PdfDocument pdfDocument, int page, NativeMethods.FPDFRenderFlags renderFlags, RenderOptions options, CancellationToken cancellationToken)
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
                    options.DpiRelativeToBounds,
                    cancellationToken);
        }
    }
}