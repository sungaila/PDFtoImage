using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;

namespace PDFtoImage.Internals
{
#if NET8_0_OR_GREATER
#pragma warning disable CA1510 // Use ArgumentNullException throw helper
#pragma warning disable CA1513 // Use ObjectDisposedException throw helper
#endif
    /// <summary>
    /// Provides functionality to render a PDF document.
    /// </summary>
    internal sealed class PdfDocument : IDisposable
    {
        private bool _disposed;
        private PdfFile? _file;

        /// <summary>
        /// Initializes a new instance of the PdfDocument class with the provided stream.
        /// </summary>
        /// <param name="stream">Stream for the PDF document.</param>
        /// <param name="password">Password for the PDF document.</param>
        /// <param name="disposeStream">Decides if <paramref name="stream"/> will closed on dispose as well.</param>
        public static PdfDocument Load(Stream stream, string? password, bool disposeStream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            return new PdfDocument(stream, password, disposeStream);
        }

        /// <summary>
        /// Size of each page in the PDF document.
        /// </summary>
        public IList<SizeF> PageSizes { get; private set; }

        private PdfDocument(Stream stream, string? password, bool disposeStream)
        {
            _file = new PdfFile(stream, password, disposeStream);

            var pageSizes = _file.GetPDFDocInfo() ?? throw new Win32Exception();
            PageSizes = new ReadOnlyCollection<SizeF>(pageSizes);
        }

        /// <summary>
        /// Renders a page of the PDF document to an image.
        /// </summary>
        /// <param name="page">Number of the page to render.</param>
        /// <param name="width">Width of the rendered image.</param>
        /// <param name="height">Height of the rendered image.</param>
        /// <param name="dpiX">Horizontal DPI.</param>
        /// <param name="dpiY">Vertical DPI.</param>
        /// <param name="rotate">Rotation.</param>
        /// <param name="flags">Flags used to influence the rendering.</param>
        /// <param name="renderFormFill">Render form fills.</param>
        /// <param name="correctFromDpi">Change <paramref name="width"/> and <paramref name="height"/> depending on the given <paramref name="dpiX"/> and <paramref name="dpiY"/>.</param>
        /// <param name="backgroundColor">The background color used for the output.</param>
        /// <returns>The rendered image.</returns>
        public SKBitmap Render(int page, int width, int height, float dpiX, float dpiY, PdfRotation rotate, NativeMethods.FPDF flags, bool renderFormFill, bool correctFromDpi, SKColor backgroundColor)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            if (rotate == PdfRotation.Rotate90 || rotate == PdfRotation.Rotate270)
            {
                (width, height) = (height, width);
                (dpiX, dpiY) = (dpiY, dpiX);
            }

            if (correctFromDpi)
            {
                width = width * (int)dpiX / 72;
                height = height * (int)dpiY / 72;
            }

            var bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
            var handle = NativeMethods.FPDFBitmap_CreateEx(width, height, NativeMethods.FPDFBitmap.BGRA, bitmap.GetPixels(), width * 4);

            try
            {
                NativeMethods.FPDFBitmap_FillRect(handle, 0, 0, width, height, (uint)backgroundColor);

                bool success = _file!.RenderPDFPageToBitmap(
                    page,
                    handle,
                    0, 0, width, height,
                    (int)rotate,
                    flags,
                    renderFormFill
                );

                if (!success)
                    throw new Win32Exception();
            }
            finally
            {
                NativeMethods.FPDFBitmap_Destroy(handle);
            }

            return bitmap;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        /// <param name="disposing">Whether this method is called from Dispose.</param>
        private void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                if (_file != null)
                {
                    _file.Dispose();
                    _file = null;
                }

                _disposed = true;
            }
        }
    }
}