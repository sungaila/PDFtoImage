using PDFtoImage.Internals;
using SkiaSharp;
using System;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;

namespace PDFtoImage.Parallel.Internals
{
    [SupportedOSPlatform("windows10.0")]
    internal sealed class WorkerDocument : IDisposable
    {
        private readonly PdfDocument _document;

        internal WorkerDocument(Stream stream, string? password)
        {
            _document = PdfDocument.Load(stream, password, disposeStream: true);
        }

        internal int PageCount => _document.PageSizes.Count;

        internal SKBitmap Render(int page, RenderOptions options)
        {
            if (page < 0 || page >= PageCount)
                throw new ArgumentOutOfRangeException(nameof(page));

            // Reuse both the native document and PDFtoImage's rendering logic.
            return global::PDFtoImage.Conversion.ToImagesImpl(_document, options, [page]).Single();
        }

        public void Dispose()
        {
            _document.Dispose();
        }
    }
}