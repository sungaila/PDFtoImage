using PDFtoImage.Parallel.Internals;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Parallel
{
    /// <summary>
    /// Renders PDF pages concurrently in isolated worker processes.
    /// </summary>
    [SupportedOSPlatform("windows6.2")]
#if NETCOREAPP
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1510")]
#endif
    public static class Conversion
    {
        /// <summary>
        /// Renders one PDF page as one job in the worker pool.
        /// </summary>
        [SupportedOSPlatform("windows6.2")]
        public static Task<SKBitmap> ToImageAsync(
            byte[] pdfAsByteArray,
            Index page = default,
            string? password = null,
            RenderOptions options = default,
            int? workerCount = null,
            CancellationToken cancellationToken = default)
        {
            return ToImageCoreAsync(pdfAsByteArray, page, password, options, workerCount, cancellationToken);
        }

        /// <summary>
        /// Renders one PDF page from a stream as one job in the worker pool.
        /// </summary>
        [SupportedOSPlatform("windows6.2")]
        public static async Task<SKBitmap> ToImageAsync(
            Stream pdfStream,
            Index page = default,
            bool leaveOpen = false,
            string? password = null,
            RenderOptions options = default,
            int? workerCount = null,
            CancellationToken cancellationToken = default)
        {
            if (pdfStream is null)
                throw new ArgumentNullException(nameof(pdfStream));

            try
            {
                using var memoryStream = new MemoryStream();
                await pdfStream.CopyToAsync(memoryStream, 81920, cancellationToken).ConfigureAwait(false);
                return await ToImageCoreAsync(memoryStream.ToArray(), page, password, options, workerCount, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                if (!leaveOpen)
                    pdfStream.Dispose();
            }
        }

        /// <summary>
        /// Renders one page from a Base64-encoded PDF as one job in the worker pool.
        /// </summary>
        [SupportedOSPlatform("windows6.2")]
        public static Task<SKBitmap> ToImageAsync(
            string pdfAsBase64String,
            Index page = default,
            string? password = null,
            RenderOptions options = default,
            int? workerCount = null,
            CancellationToken cancellationToken = default)
        {
            return pdfAsBase64String is null
                ? throw new ArgumentNullException(nameof(pdfAsBase64String))
                : ToImageCoreAsync(Convert.FromBase64String(pdfAsBase64String), page, password, options, workerCount, cancellationToken);
        }

        /// <summary>
        /// Renders all pages of a PDF into images using worker processes.
        /// </summary>
        [SupportedOSPlatform("windows6.2")]
        public static IAsyncEnumerable<SKBitmap> ToImagesAsync(
            byte[] pdfAsByteArray,
            string? password = null,
            RenderOptions options = default,
            int? workerCount = null,
            CancellationToken cancellationToken = default)
        {
            return ToImagesCoreAsync(pdfAsByteArray, PageSelection.All, password, options, workerCount, cancellationToken);
        }

        /// <summary>
        /// Renders a range of PDF pages into images using worker processes.
        /// </summary>
        [SupportedOSPlatform("windows6.2")]
        public static IAsyncEnumerable<SKBitmap> ToImagesAsync(
            byte[] pdfAsByteArray,
            Range pages,
            string? password = null,
            RenderOptions options = default,
            int? workerCount = null,
            CancellationToken cancellationToken = default)
        {
            return ToImagesCoreAsync(pdfAsByteArray, PageSelection.FromRange(pages), password, options, workerCount, cancellationToken);
        }

        /// <summary>
        /// Renders selected PDF pages into images using worker processes.
        /// </summary>
        [SupportedOSPlatform("windows6.2")]
        public static IAsyncEnumerable<SKBitmap> ToImagesAsync(
            byte[] pdfAsByteArray,
            IEnumerable<int> pages,
            string? password = null,
            RenderOptions options = default,
            int? workerCount = null,
            CancellationToken cancellationToken = default)
        {
            return pages is null
                ? throw new ArgumentNullException(nameof(pages))
                : ToImagesCoreAsync(pdfAsByteArray, PageSelection.FromPages([.. pages]), password, options, workerCount, cancellationToken);
        }

        /// <summary>
        /// Renders all pages of a PDF stream into images using worker processes.
        /// </summary>
        [SupportedOSPlatform("windows6.2")]
        public static IAsyncEnumerable<SKBitmap> ToImagesAsync(
            Stream pdfStream,
            bool leaveOpen = false,
            string? password = null,
            RenderOptions options = default,
            int? workerCount = null,
            CancellationToken cancellationToken = default)
        {
            return ToImagesFromStreamAsync(pdfStream, PageSelection.All, leaveOpen, password, options, workerCount, cancellationToken);
        }

        /// <summary>
        /// Renders a range of PDF pages from a stream into images using worker processes.
        /// </summary>
        [SupportedOSPlatform("windows6.2")]
        public static IAsyncEnumerable<SKBitmap> ToImagesAsync(
            Stream pdfStream,
            Range pages,
            bool leaveOpen = false,
            string? password = null,
            RenderOptions options = default,
            int? workerCount = null,
            CancellationToken cancellationToken = default)
        {
            return ToImagesFromStreamAsync(pdfStream, PageSelection.FromRange(pages), leaveOpen, password, options, workerCount, cancellationToken);
        }

        /// <summary>
        /// Renders selected PDF pages from a stream into images using worker processes.
        /// </summary>
        [SupportedOSPlatform("windows6.2")]
        public static IAsyncEnumerable<SKBitmap> ToImagesAsync(
            Stream pdfStream,
            IEnumerable<int> pages,
            bool leaveOpen = false,
            string? password = null,
            RenderOptions options = default,
            int? workerCount = null,
            CancellationToken cancellationToken = default)
        {
            return pages is null
                ? throw new ArgumentNullException(nameof(pages))
                : ToImagesFromStreamAsync(pdfStream, PageSelection.FromPages([.. pages]), leaveOpen, password, options, workerCount, cancellationToken);
        }

        /// <summary>
        /// Renders all pages of a Base64-encoded PDF into images using worker processes.
        /// </summary>
        [SupportedOSPlatform("windows6.2")]
        public static IAsyncEnumerable<SKBitmap> ToImagesAsync(
            string pdfAsBase64String,
            string? password = null,
            RenderOptions options = default,
            int? workerCount = null,
            CancellationToken cancellationToken = default)
        {
            return pdfAsBase64String is null
                ? throw new ArgumentNullException(nameof(pdfAsBase64String))
                : ToImagesAsync(Convert.FromBase64String(pdfAsBase64String), password, options, workerCount, cancellationToken);
        }

        /// <summary>
        /// Renders a range of pages from a Base64-encoded PDF into images using worker processes.
        /// </summary>
        [SupportedOSPlatform("windows6.2")]
        public static IAsyncEnumerable<SKBitmap> ToImagesAsync(
            string pdfAsBase64String,
            Range pages,
            string? password = null,
            RenderOptions options = default,
            int? workerCount = null,
            CancellationToken cancellationToken = default)
        {
            return pdfAsBase64String is null
                ? throw new ArgumentNullException(nameof(pdfAsBase64String))
                : ToImagesAsync(Convert.FromBase64String(pdfAsBase64String), pages, password, options, workerCount, cancellationToken);
        }

        /// <summary>
        /// Renders selected pages from a Base64-encoded PDF into images using worker processes.
        /// </summary>
        [SupportedOSPlatform("windows6.2")]
        public static IAsyncEnumerable<SKBitmap> ToImagesAsync(
            string pdfAsBase64String,
            IEnumerable<int> pages,
            string? password = null,
            RenderOptions options = default,
            int? workerCount = null,
            CancellationToken cancellationToken = default)
        {
            return pdfAsBase64String is null
                ? throw new ArgumentNullException(nameof(pdfAsBase64String))
                : ToImagesAsync(Convert.FromBase64String(pdfAsBase64String), pages, password, options, workerCount, cancellationToken);
        }

        private static async IAsyncEnumerable<SKBitmap> ToImagesFromStreamAsync(
            Stream pdfStream,
            PageSelection pages,
            bool leaveOpen,
            string? password,
            RenderOptions options,
            int? workerCount,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (pdfStream is null)
                throw new ArgumentNullException(nameof(pdfStream));

            try
            {
                using var memoryStream = new MemoryStream();
                await pdfStream.CopyToAsync(memoryStream, 81920, cancellationToken).ConfigureAwait(false);

                await foreach (var image in ToImagesCoreAsync(memoryStream.ToArray(), pages, password, options, workerCount, cancellationToken).ConfigureAwait(false))
                    yield return image;
            }
            finally
            {
                if (!leaveOpen)
                    pdfStream.Dispose();
            }
        }

        private static async Task<SKBitmap> ToImageCoreAsync(
            byte[] pdfAsByteArray,
            Index page,
            string? password,
            RenderOptions options,
            int? workerCount,
            CancellationToken cancellationToken)
        {
            if (pdfAsByteArray is null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            EnsureWindows();
            var actualWorkerCount = GetWorkerCount(workerCount);

            await using var pool = await WorkerPool.CreateAsync(actualWorkerCount, pdfAsByteArray, password, cancellationToken).ConfigureAwait(false);
            var pageNumber = page.GetOffset(pool.PageCount);

            if (pageNumber < 0 || pageNumber >= pool.PageCount)
                throw new ArgumentOutOfRangeException(nameof(page), $"The page number must be between 0 and {pool.PageCount - 1}. The PDF has {pool.PageCount} pages in total.");

            var bitmapBytes = await pool.RenderPageAsync(pageNumber, options, cancellationToken).ConfigureAwait(false);
            return PipeProtocol.ReadBitmap(bitmapBytes);
        }

        private static async IAsyncEnumerable<SKBitmap> ToImagesCoreAsync(
            byte[] pdfAsByteArray,
            PageSelection pages,
            string? password,
            RenderOptions options,
            int? workerCount,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (pdfAsByteArray is null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            EnsureWindows();
            var actualWorkerCount = GetWorkerCount(workerCount);

            WorkerPool? pool = null;
            var pending = new Queue<Task<byte[]>>();

            try
            {
                pool = await WorkerPool.CreateAsync(actualWorkerCount, pdfAsByteArray, password, cancellationToken).ConfigureAwait(false);
                var pageNumbers = pages.Resolve(pool.PageCount);
                using var pageEnumerator = ((IEnumerable<int>)pageNumbers).GetEnumerator();

                for (var i = 0; i < actualWorkerCount && pageEnumerator.MoveNext(); i++)
                    pending.Enqueue(pool.RenderPageAsync(pageEnumerator.Current, options, cancellationToken));

                while (pending.Count > 0)
                {
                    var bitmapBytes = await pending.Dequeue().ConfigureAwait(false);

                    if (pageEnumerator.MoveNext())
                        pending.Enqueue(pool.RenderPageAsync(pageEnumerator.Current, options, cancellationToken));

                    yield return PipeProtocol.ReadBitmap(bitmapBytes);
                }
            }
            finally
            {
                if (pool != null)
                    await pool.DisposeAsync().ConfigureAwait(false);

                while (pending.Count > 0)
                {
                    try
                    {
                        await pending.Dequeue().ConfigureAwait(false);
                    }
                    catch
                    {
                        // The worker pool is being torn down; pending failures are expected here.
                    }
                }
            }
        }

        private static void EnsureWindows()
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new PlatformNotSupportedException("PDFtoImage.Parallel currently supports Windows only.");
        }

        private static int GetWorkerCount(int? workerCount)
        {
            var actualWorkerCount = workerCount ?? Environment.ProcessorCount;
            return actualWorkerCount <= 0
                ? throw new ArgumentOutOfRangeException(nameof(workerCount), "The worker count must be greater than zero.")
                : actualWorkerCount;
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

            internal int[] Resolve(int pageCount)
            {
                if (_pages != null)
                {
                    return _pages.Any(page => page < 0 || page >= pageCount)
                        ? throw new ArgumentOutOfRangeException(nameof(pageCount), $"The page numbers must be between 0 and {pageCount - 1}. The PDF has {pageCount} pages in total.")
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