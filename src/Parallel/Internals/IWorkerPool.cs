using SkiaSharp;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Parallel.Internals
{
    // Platform implementations own process lifetime and exclusive worker leases.
    // They must isolate failed/cancelled requests without poisoning other leases.
    internal interface IWorkerPool : IDisposable, IAsyncDisposable
    {
        int WorkerCount { get; }

        int[] WorkerProcessIds { get; }

        Guid?[] WorkerDocumentIds { get; }

        int[] WorkerDocumentLoadCounts { get; }

        void ThrowIfDisposed();

        Task<int> GetPageCountAsync(PdfRequest request, CancellationToken cancellationToken);

        Task<SKBitmap> RenderPageAsync(PdfRequest request, Index page, RenderOptions options, CancellationToken cancellationToken);

        Task ReleaseDocumentAsync(PdfRequest request);
    }

    // Identity belongs to a conversion operation, not the mutable input array.
    // The same request is shared by all page jobs of one async enumeration.
    internal sealed record PdfRequest(byte[] Bytes, string? Password)
    {
        internal Guid Id { get; } = Guid.NewGuid();
    }
}