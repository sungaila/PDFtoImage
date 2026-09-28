using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Parallel
{
    /// <summary>Renders PDF pages with isolated worker processes.</summary>
    // These overloads mirror the existing preview API, whose page selectors have distinct types.
#pragma warning disable RS0026
    public interface IParallelPdfProcessor : IDisposable, IAsyncDisposable
    {
        /// <summary>Renders a single page.</summary>
        Task<SKBitmap> ToImageAsync(Stream pdfStream, Index page = default, bool leaveOpen = false, string? password = null, RenderOptions options = default, CancellationToken cancellationToken = default);

        /// <summary>Renders every page.</summary>
        IAsyncEnumerable<SKBitmap> ToImagesAsync(Stream pdfStream, bool leaveOpen = false, string? password = null, RenderOptions options = default, CancellationToken cancellationToken = default);

        /// <summary>Renders a page range.</summary>
        IAsyncEnumerable<SKBitmap> ToImagesAsync(Stream pdfStream, Range pages, bool leaveOpen = false, string? password = null, RenderOptions options = default, CancellationToken cancellationToken = default);

        /// <summary>Renders selected pages in the requested order.</summary>
        IAsyncEnumerable<SKBitmap> ToImagesAsync(Stream pdfStream, IEnumerable<int> pages, bool leaveOpen = false, string? password = null, RenderOptions options = default, CancellationToken cancellationToken = default);
    }
#pragma warning restore RS0026
}