using PDFtoImage.Parallel.Internals;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Parallel
{
    /// <summary>
    /// Renders PDF pages concurrently in isolated worker processes.
    /// </summary>
    [SupportedOSPlatform("windows10.0")]
#pragma warning disable RS0026 // First-release overloads mirror PDFtoImage's input shapes.
    public sealed class ParallelPdfProcessor : IDisposable, IAsyncDisposable
    {
        private readonly IWorkerPool _pool;

        /// <summary>
        /// Creates a reusable pool. Workers start on demand, up to the specified limit.
        /// </summary>
        /// <param name="workerCount">Maximum concurrent workers; defaults to <see cref="Environment.ProcessorCount"/>.</param>
        public ParallelPdfProcessor(int? workerCount = null)
        {
            var count = workerCount ?? Environment.ProcessorCount;

            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count, nameof(workerCount));

            _pool = WorkerPoolFactory.Create(count);
        }

        internal int[] WorkerProcessIds => _pool.WorkerProcessIds;

        /// <summary>Stops all workers and cancels active and queued requests. Safe to call repeatedly.</summary>
        public void Dispose() => _pool.Dispose();

        /// <summary>Stops all workers and waits for outstanding worker requests and cleanup.</summary>
        public ValueTask DisposeAsync() => _pool.DisposeAsync();

        /// <summary>
        /// Renders one page from a PDF stream as one job in the worker pool.
        /// </summary>
        /// <param name="pdfStream">The PDF to render.</param>
        /// <param name="page">The zero-based page index.</param>
        /// <param name="leaveOpen">Whether to leave <paramref name="pdfStream"/> open after it has been read.</param>
        /// <param name="password">The optional PDF password.</param>
        /// <param name="options">Rendering options.</param>
        /// <param name="cancellationToken">Cancels reading or rendering the request.</param>
        [SupportedOSPlatform("windows10.0")]
        public async Task<SKBitmap> ToImageAsync(Stream pdfStream, Index page = default, bool leaveOpen = false, string? password = null, RenderOptions options = default, CancellationToken cancellationToken = default)
        {
            var pdf = await ReadPdfAsync(pdfStream, leaveOpen, cancellationToken).ConfigureAwait(false);
            return await ToImageCoreAsync(pdf, page, password, options, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Renders all pages from a PDF stream into images using worker processes.
        /// </summary>
        [SupportedOSPlatform("windows10.0")]
        public IAsyncEnumerable<SKBitmap> ToImagesAsync(Stream pdfStream, bool leaveOpen = false, string? password = null, RenderOptions options = default, CancellationToken cancellationToken = default) =>
            ToImagesFromStreamAsync(pdfStream, PageSelection.All, leaveOpen, password, options, cancellationToken);

        /// <summary>
        /// Renders a range of pages from a PDF stream into images using worker processes.
        /// </summary>
        [SupportedOSPlatform("windows10.0")]
        public IAsyncEnumerable<SKBitmap> ToImagesAsync(Stream pdfStream, Range pages, bool leaveOpen = false, string? password = null, RenderOptions options = default, CancellationToken cancellationToken = default) =>
            ToImagesFromStreamAsync(pdfStream, PageSelection.FromRange(pages), leaveOpen, password, options, cancellationToken);

        /// <summary>
        /// Renders selected pages from a PDF stream into images using worker processes.
        /// </summary>
        [SupportedOSPlatform("windows10.0")]
        public IAsyncEnumerable<SKBitmap> ToImagesAsync(Stream pdfStream, IEnumerable<int> pages, bool leaveOpen = false, string? password = null, RenderOptions options = default, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(pages);

            return ToImagesFromStreamAsync(pdfStream, PageSelection.FromPages([.. pages]), leaveOpen, password, options, cancellationToken);
        }

#pragma warning restore RS0026

        private async IAsyncEnumerable<SKBitmap> ToImagesFromStreamAsync(Stream pdfStream, PageSelection pages, bool leaveOpen, string? password, RenderOptions options, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var pdf = await ReadPdfAsync(pdfStream, leaveOpen, cancellationToken).ConfigureAwait(false);

            await foreach (var image in ToImagesCoreAsync(pdf, pages, password, options, cancellationToken).ConfigureAwait(false))
                yield return image;
        }

        private async Task<byte[]> ReadPdfAsync(Stream pdfStream, bool leaveOpen, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(pdfStream);
            try
            {
                _pool.ThrowIfDisposed();
                using var memoryStream = new MemoryStream();
                await pdfStream.CopyToAsync(memoryStream, 81920, cancellationToken).ConfigureAwait(false);
                return memoryStream.ToArray();
            }
            finally
            {
                if (!leaveOpen)
                    pdfStream.Dispose();
            }
        }

        private async Task<SKBitmap> ToImageCoreAsync(byte[] pdf, Index page, string? password, RenderOptions options, CancellationToken cancellationToken)
        {
            _pool.ThrowIfDisposed();
            var request = new PdfRequest(pdf, password);
            var bitmap = await _pool.RenderPageAsync(request, page, options, cancellationToken).ConfigureAwait(false);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                return bitmap;
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }
        }

        private async IAsyncEnumerable<SKBitmap> ToImagesCoreAsync(byte[] pdf, PageSelection pages, string? password, RenderOptions options, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _pool.ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            if (pages.MaximumCount == 0)
                yield break;

            var request = new PdfRequest(pdf, password);
            var pageCount = await _pool.GetPageCountAsync(request, cancellationToken).ConfigureAwait(false);
            var pageNumbers = pages.Resolve(pageCount);

            if (pageNumbers.Length == 0)
                yield break;

            await foreach (var bitmap in OrderedScheduler.RunAsync(
                pageNumbers, (int)Math.Min(pageNumbers.Length, (long)_pool.WorkerCount * 2),
                (page, token) => _pool.RenderPageAsync(request, page, options, token), cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    _pool.ThrowIfDisposed();
                }
                catch
                {
                    bitmap.Dispose();
                    throw;
                }

                yield return bitmap;
            }
        }

        private readonly struct PageSelection
        {
            private readonly Range? _range;

            private readonly int[]? _pages;

            private PageSelection(Range? range, int[]? pages)
            {
                _range = range;
                _pages = pages;
            }

            internal static PageSelection All => new(null, null);

            internal static PageSelection FromRange(Range range) => new(range, null);

            internal static PageSelection FromPages(int[] pages) => new(null, pages);

            internal int MaximumCount => _pages?.Length ?? int.MaxValue;

            internal int[] Resolve(int pageCount)
            {
                if (_pages != null)
                {
                    return _pages.Any(page => page < 0 || page >= pageCount)
                        ? throw new ArgumentOutOfRangeException("pages", $"The page numbers must be between 0 and {pageCount - 1}. The PDF has {pageCount} pages in total.")
                        : _pages;
                }

                if (_range.HasValue)
                {
                    var (offset, length) = _range.Value.GetOffsetAndLength(pageCount);
                    return [.. Enumerable.Range(offset, length)];
                }

                return [.. Enumerable.Range(0, pageCount)];
            }
        }
    }
}