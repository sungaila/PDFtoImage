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
        /// <param name="bounds">Specifies the bounds for the page relative to <see cref="Conversion.GetPageSizes(string,string)"/>. This can be used for clipping (bounds inside of page) or additional margins (bounds outside of page).</param>
        /// <returns>The rendered image.</returns>
        public SKBitmap Render(int page, float width, float height, float dpiX, float dpiY, PdfRotation rotate, NativeMethods.FPDF flags, bool renderFormFill, bool correctFromDpi, SKColor backgroundColor, RectangleF? bounds)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            if (rotate == PdfRotation.Rotate90 || rotate == PdfRotation.Rotate270)
            {
                (width, height) = (height, width);
                (dpiX, dpiY) = (dpiY, dpiX);
            }

            var pageWidth = PageSizes[page].Width;
            var pageHeight = PageSizes[page].Height;

            if (correctFromDpi)
            {
                width *= dpiX / 72f;
                height *= dpiY / 72f;

                pageWidth *= dpiX / 72f;
                pageHeight *= dpiY / 72f;

                if (bounds != null)
                {
                    bounds = new RectangleF(
                        bounds.Value.X * (dpiX / 72f),
                        bounds.Value.Y * (dpiY / 72f),
                        bounds.Value.Width * (dpiX / 72f),
                        bounds.Value.Height * (dpiY / 72f)
                    );
                }
            }

            if (bounds != null)
            {
                if (rotate == PdfRotation.Rotate90)
                {
                    bounds = new RectangleF(
                        width - bounds.Value.Height - bounds.Value.Y,
                        bounds.Value.X,
                        bounds.Value.Width,
                        bounds.Value.Height
                        );
                }
                else if (rotate == PdfRotation.Rotate270)
                {
                    bounds = new RectangleF(
                        bounds.Value.Y,
                        height - bounds.Value.Width - bounds.Value.X,
                        bounds.Value.Width,
                        bounds.Value.Height
                        );
                }
                else if (rotate == PdfRotation.Rotate180)
                {
                    bounds = new RectangleF(
                        width - bounds.Value.Width - bounds.Value.X,
                        height - bounds.Value.Height - bounds.Value.Y,
                        bounds.Value.Width,
                        bounds.Value.Height
                        );
                }
            }

            width = (float)Math.Round(width);
            height = (float)Math.Round(height);

            var bitmap = new SKBitmap((int)width, (int)height, SKColorType.Bgra8888, SKAlphaType.Premul);
            var handle = NativeMethods.FPDFBitmap_CreateEx((int)width, (int)height, NativeMethods.FPDFBitmap.BGRA, bitmap.GetPixels(), (int)width * 4);

            try
            {
                NativeMethods.FPDFBitmap_FillRect(handle, 0, 0, (int)width, (int)height, (uint)backgroundColor);

                bool success = _file!.RenderPDFPageToBitmap(
                    page,
                    handle,
                    bounds != null ? -(int)Math.Round(bounds.Value.X * (pageWidth / bounds.Value.Width)) : 0,
                    bounds != null ? -(int)Math.Round(bounds.Value.Y * (pageHeight / bounds.Value.Height)) : 0,
                    bounds != null ? (int)Math.Round(pageWidth * (width / bounds.Value.Width)) : (int)width,
                    bounds != null ? (int)Math.Round(pageHeight * (height / bounds.Value.Height)) : (int)height,
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
                _file?.Dispose();
                _file = null;

                _disposed = true;
            }
        }
    }
}