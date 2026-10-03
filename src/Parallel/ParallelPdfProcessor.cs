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
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    public sealed class ParallelPdfProcessor : IParallelPdfProcessor
    {
        private readonly WorkerPool _pool;

        private readonly ProcessorTransferMode _transferMode;

        private readonly bool _reuseFileStream;

        private readonly int? _maxParallelism;

        private readonly string _tempDirectory;

        private readonly CancellationTokenSource _shutdown = new();

        private readonly Lock _gate = new();

        private readonly TaskCompletionSource _requestsDrained = new(TaskCreationOptions.RunContinuationsAsynchronously);

        private readonly HashSet<PdfRequest> _fileRequests = [];

        private bool _disposed;

        private bool _cleanupFinished;

        private List<Exception>? _cleanupErrors;

        private int _activeRequests;

        /// <summary>Creates a reusable pool with default options.</summary>
        public ParallelPdfProcessor() : this(new ProcessorOptions()) { }

        /// <summary>Creates a reusable pool with the specified worker and transfer options.</summary>
        /// <param name="options">Settings read once when the processor is constructed.</param>
        public ParallelPdfProcessor(ProcessorOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            var count = options.WorkerCount ?? Environment.ProcessorCount;

            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count, nameof(options.WorkerCount));

            if (options.SlotCount is int maximum)
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximum, nameof(options.SlotCount));

            if (!Enum.IsDefined(options.TransferMode))
                throw new ArgumentOutOfRangeException(nameof(options.TransferMode));

            if (options.TransferMode == ProcessorTransferMode.MemoryMappedFile)
            {
                _tempDirectory = Path.GetFullPath(options.TempDirectory ?? Path.GetTempPath());
                Directory.CreateDirectory(_tempDirectory);
            }
            else
            {
                _tempDirectory = Path.GetTempPath();
            }

            _transferMode = options.TransferMode;
            _reuseFileStream = options.ReuseFileStream;
            _maxParallelism = options.SlotCount;
            _pool = new WorkerPool(count, options.SlotCount, options.TransferMode, _tempDirectory);
        }

        internal int[] WorkerProcessIds => _pool.WorkerProcessIds;

        internal Guid?[] WorkerDocumentIds => _pool.WorkerDocumentIds;

        internal int[] WorkerDocumentLoadCounts => _pool.WorkerDocumentLoadCounts;

        internal string[] TemporaryPdfPaths
        {
            get
            {
                lock (_gate)
                    return [.. _fileRequests.Where(request => request.IsTemporaryFile).Select(request => request.FilePath!)];
            }
        }

        /// <summary>Stops all workers and cancels active and queued requests. Safe to call repeatedly.</summary>
        public void Dispose()
        {
            lock (_gate)
            {
                if (_disposed)
                    return;

                _disposed = true;
            }

            var errors = new List<Exception>();
            TryCleanup(_shutdown.Cancel, errors);
            TryCleanup(_pool.Dispose, errors);
            TryCleanup(CleanupFileRequests, errors);

            lock (_gate)
            {
                _cleanupErrors = errors;
                _cleanupFinished = true;
                CompleteDisposalIfDrained();

                if (errors.Count > 0)
                    throw new AggregateException("PDF processor cleanup failed.", errors);
            }
        }

        /// <summary>Stops all workers and waits for outstanding worker requests and cleanup.</summary>
        public async ValueTask DisposeAsync()
        {
            try
            {
                Dispose();
            }
            catch (AggregateException) { /* Report all cleanup errors after draining below. */ }

            await _requestsDrained.Task.ConfigureAwait(false);
            var errors = new List<Exception>(_cleanupErrors!);
            try
            {
                await _pool.DisposeAsync().ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                errors.Add(exception);
            }

            // A previous deletion may have failed while a worker still held the file.
            TryCleanup(CleanupFileRequests, errors);

            if (errors.Count > 0)
            {
                // The pool reports its synchronous failures again after draining.
                var failures = new AggregateException(errors).Flatten().InnerExceptions.Distinct();
                throw new AggregateException("PDF processor cleanup failed.", failures);
            }
        }

        private static void TryCleanup(Action cleanup, List<Exception> errors)
        {
            try
            {
                cleanup();
            }
            catch (Exception exception)
            {
                errors.Add(exception);
            }
        }

        /// <inheritdoc />
        public async Task<SKBitmap> ToImageAsync(Stream pdfStream, Index page = default, bool leaveOpen = false, string? password = null, RenderOptions options = default, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(pdfStream);
            SKBitmap? bitmap = null;
            try
            {
                var (request, pdf) = await BeginAndReadPdfAsync(pdfStream, leaveOpen, password, cancellationToken).ConfigureAwait(false);
                using (request)
                using (pdf)
                {
                    bitmap = await ToImageCoreAsync(pdf, page, options, request.Token).ConfigureAwait(false);
                    return bitmap;
                }
            }
            catch
            {
                // A result cannot be delivered if PDF or input stream cleanup fails.
                bitmap?.Dispose();
                throw;
            }
        }

        /// <inheritdoc />
        public IAsyncEnumerable<SKBitmap> ToImagesAsync(Stream pdfStream, bool leaveOpen = false, string? password = null, RenderOptions options = default, CancellationToken cancellationToken = default) =>
            ToImagesFromStreamAsync(pdfStream, PageSelection.All, leaveOpen, password, options, cancellationToken);

        /// <inheritdoc />
        public IAsyncEnumerable<SKBitmap> ToImagesAsync(Stream pdfStream, Range pages, bool leaveOpen = false, string? password = null, RenderOptions options = default, CancellationToken cancellationToken = default) =>
            ToImagesFromStreamAsync(pdfStream, PageSelection.FromRange(pages), leaveOpen, password, options, cancellationToken);

        /// <inheritdoc />
        public IAsyncEnumerable<SKBitmap> ToImagesAsync(Stream pdfStream, IEnumerable<int> pages, bool leaveOpen = false, string? password = null, RenderOptions options = default, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(pages);

            return ToImagesFromPagesAsync(pdfStream, pages, leaveOpen, password, options, cancellationToken);
        }

        private async IAsyncEnumerable<SKBitmap> ToImagesFromPagesAsync(Stream pdfStream, IEnumerable<int> pages, bool leaveOpen, string? password, RenderOptions options, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(pdfStream);
            PageSelection selection;
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                selection = PageSelection.FromPages(global::PDFtoImage.Conversion.SnapshotPages(pages, cancellationToken));
            }
            catch
            {
                if (!leaveOpen)
                    await pdfStream.DisposeAsync().ConfigureAwait(false);
                throw;
            }

            await foreach (var bitmap in ToImagesFromStreamAsync(pdfStream, selection, leaveOpen, password, options, cancellationToken).ConfigureAwait(false))
                yield return bitmap;
        }

        private async IAsyncEnumerable<SKBitmap> ToImagesFromStreamAsync(Stream pdfStream, PageSelection pages, bool leaveOpen, string? password, RenderOptions options, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(pdfStream);
            CancellationTokenSource enumerationCancellation;

            var (initialRequest, openedPdf) = await BeginAndReadPdfAsync(pdfStream, leaveOpen, password, cancellationToken).ConfigureAwait(false);
            using var pdf = openedPdf;
            using (var request = initialRequest)
            {
                enumerationCancellation = CreateEnumerationCancellation(cancellationToken);
            }

            using (enumerationCancellation)
            {
                var iterator = ToImagesCoreAsync(pdf, pages, options, enumerationCancellation.Token)
                    .GetAsyncEnumerator(enumerationCancellation.Token);
                await using var iteratorCleanup = iterator.ConfigureAwait(false);

                while (true)
                {
                    SKBitmap image;

                    // A paused async iterator must not count as an active public request.
                    // The enumeration-level token still cancels queued/pending worker work
                    // when the processor is disposed.
                    using (var request = BeginRequest(enumerationCancellation.Token))
                    {
                        if (!await iterator.MoveNextAsync().ConfigureAwait(false))
                            yield break;

                        image = iterator.Current;
                    }

                    yield return image;
                }
            }
        }

        private async Task<(RequestCancellation Request, PdfRequest Pdf)> BeginAndReadPdfAsync(Stream pdfStream, bool leaveOpen, string? password, CancellationToken cancellationToken)
        {
            RequestCancellation? request = null;
            PdfRequest? pdf = null;
            try
            {
                try
                {
                    request = BeginRequest(cancellationToken);
                    pdf = await ReadPdfAsync(pdfStream, password, request.Token).ConfigureAwait(false);
                }
                finally
                {
                    if (!leaveOpen)
                        await pdfStream.DisposeAsync().ConfigureAwait(false);
                }

                return (request, pdf);
            }
            catch
            {
                try
                {
                    pdf?.Dispose();
                }
                finally
                {
                    request?.Dispose();
                }
                throw;
            }
        }

        private async Task<PdfRequest> ReadPdfAsync(Stream pdfStream, string? password, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(pdfStream);
            PdfRequest? result = null;
            try
            {
                _pool.ThrowIfDisposed();
                cancellationToken.ThrowIfCancellationRequested();
                if (_transferMode == ProcessorTransferMode.MemoryMappedFile)
                {
                    if (_reuseFileStream && pdfStream is FileStream source && PdfInputReader.TryOpenSourceFile(source) is FileStream readable)
                    {
                        result = new PdfRequest(readable.Name, readable, password, ReleaseFileRequest, deleteFile: false);
                        RegisterFileRequest(result);
                        return result;
                    }

                    var (path, lifetime) = await PdfInputReader.WriteTempFileAsync(pdfStream, _tempDirectory, cancellationToken).ConfigureAwait(false);
                    result = new PdfRequest(path, lifetime, password, ReleaseFileRequest, deleteOnClose: OperatingSystem.IsWindows());
                    RegisterFileRequest(result);
                    return result;
                }

                result = new PdfRequest(await PdfInputReader.ReadAsync(pdfStream, WorkerProtocol.GetMaximumPdfLength(password), cancellationToken).ConfigureAwait(false), password);
                return result;
            }
            catch
            {
                result?.Dispose();
                throw;
            }
        }

        private void RegisterFileRequest(PdfRequest request)
        {
            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                _fileRequests.Add(request);
            }
        }

        private void ReleaseFileRequest(PdfRequest request)
        {
            lock (_gate)
                _fileRequests.Remove(request);
        }

        private void CleanupFileRequests()
        {
            PdfRequest[] requests;
            lock (_gate)
                requests = [.. _fileRequests];

            List<Exception>? errors = null;
            foreach (var request in requests)
            {
                try
                {
                    request.Dispose();
                }
                catch (Exception exception)
                {
                    (errors ??= []).Add(exception);
                }
            }

            if (errors != null)
                throw new AggregateException("PDF file request cleanup failed.", errors);
        }

        private async Task<SKBitmap> ToImageCoreAsync(PdfRequest request, Index page, RenderOptions options, CancellationToken cancellationToken)
        {
            _pool.ThrowIfDisposed();
            SKBitmap? bitmap = null;
            try
            {
                try
                {
                    bitmap = await _pool.RenderPageAsync(request, page, options, cancellationToken).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                    return bitmap;
                }
                finally
                {
                    await _pool.ReleaseDocumentAsync(request).ConfigureAwait(false);
                }
            }
            catch
            {
                bitmap?.Dispose();
                throw;
            }
        }

        private async IAsyncEnumerable<SKBitmap> ToImagesCoreAsync(PdfRequest request, PageSelection pages, RenderOptions options, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            _pool.ThrowIfDisposed();
            cancellationToken.ThrowIfCancellationRequested();

            if (pages.MaximumCount == 0)
                yield break;

            try
            {
                var pageCount = await _pool.GetPageCountAsync(request, cancellationToken).ConfigureAwait(false);
                var pageNumbers = pages.Resolve(pageCount);

                if (pageNumbers.Length == 0)
                    yield break;

                await foreach (var bitmap in OrderedScheduler.RunAsync(
                    pageNumbers, (int)Math.Min(pageNumbers.Length, (long)Math.Min(_pool.WorkerCount, _maxParallelism ?? int.MaxValue) * 2),
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
            finally
            {
                await _pool.ReleaseDocumentAsync(request).ConfigureAwait(false);
            }
        }

        private CancellationTokenSource CreateEnumerationCancellation(CancellationToken cancellationToken)
        {
            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                return CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _shutdown.Token);
            }
        }

        private RequestCancellation BeginRequest(CancellationToken cancellationToken)
        {
            lock (_gate)
            {
                ObjectDisposedException.ThrowIf(_disposed, this);
                _activeRequests++;
            }

            try
            {
                return new RequestCancellation(this, CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _shutdown.Token));
            }
            catch
            {
                EndRequest();
                throw;
            }
        }

        private void EndRequest()
        {
            lock (_gate)
            {
                _activeRequests--;
                CompleteDisposalIfDrained();
            }
        }

        private void CompleteDisposalIfDrained()
        {
            if (!_cleanupFinished || _activeRequests != 0 || _requestsDrained.Task.IsCompleted)
                return;

            TryCleanup(_shutdown.Dispose, _cleanupErrors!);
            _requestsDrained.TrySetResult();
        }

        private sealed class RequestCancellation : IDisposable
        {
            private ParallelPdfProcessor? _processor;

            internal RequestCancellation(ParallelPdfProcessor processor, CancellationTokenSource source)
            {
                _processor = processor;
                Source = source;
            }

            private CancellationTokenSource Source { get; }

            internal CancellationToken Token => Source.Token;

            public void Dispose()
            {
                Source.Dispose();
                Interlocked.Exchange(ref _processor, null)?.EndRequest();
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

            [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2208")]
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