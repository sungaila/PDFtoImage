﻿using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Threading;

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

        private const int MaxTileWidth = 4000;
        private const int MaxTileHeight = 4000;

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
        /// <param name="useTiling"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>The rendered image.</returns>
        public SKBitmap Render(int page, float width, float height, float dpiX, float dpiY, PdfRotation rotate, NativeMethods.FPDF flags, bool renderFormFill, bool correctFromDpi, SKColor backgroundColor, RectangleF? bounds, bool useTiling, CancellationToken cancellationToken = default)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            var originalWidth = PageSizes[page].Width;
            var originalHeight = PageSizes[page].Height;

            if (rotate == PdfRotation.Rotate90 || rotate == PdfRotation.Rotate270)
            {
                (width, height) = (height, width);
                (originalWidth, originalHeight) = (originalHeight, originalWidth);
                (dpiX, dpiY) = (dpiY, dpiX);
            }

            if (correctFromDpi)
            {
                width *= dpiX / 72f;
                height *= dpiY / 72f;

                originalWidth *= dpiX / 72f;
                originalHeight *= dpiY / 72f;

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
                var factorX = width / originalWidth;
                var factorY = height / originalHeight;

                if (rotate == PdfRotation.Rotate90)
                {
                    bounds = new RectangleF(
                        ((originalWidth - bounds.Value.Height) * factorX) - bounds.Value.Y,
                        bounds.Value.X,
                        bounds.Value.Height,
                        bounds.Value.Width
                        );
                }
                else if (rotate == PdfRotation.Rotate270)
                {
                    bounds = new RectangleF(
                        bounds.Value.Y,
                        ((originalHeight - bounds.Value.Width) * factorY) - bounds.Value.X,
                        bounds.Value.Height,
                        bounds.Value.Width
                        );
                }
                else if (rotate == PdfRotation.Rotate180)
                {
                    bounds = new RectangleF(
                        ((originalWidth - bounds.Value.Width) * factorX) - bounds.Value.X,
                        ((originalHeight - bounds.Value.Height) * factorY) - bounds.Value.Y,
                        bounds.Value.Width,
                        bounds.Value.Height
                        );
                }
            }

            SKBitmap bitmap;

            int horizontalTileCount = (int)Math.Ceiling(width / MaxTileWidth);
            int verticalTileCount = (int)Math.Ceiling(height / MaxTileHeight);

            if (!useTiling || (horizontalTileCount == 1 && verticalTileCount == 1))
            {
                bitmap = RenderSubset(_file!, page, width, height, rotate, flags, renderFormFill, backgroundColor, bounds, originalWidth, originalHeight, cancellationToken);
            }
            else
            {
                bitmap = new SKBitmap((int)width, (int)height, SKColorType.Bgra8888, SKAlphaType.Premul);

                cancellationToken.ThrowIfCancellationRequested();

                float currentTileWidth = width / horizontalTileCount;
                float currentTileHeight = height / verticalTileCount;
                float boundsWidthFactor = bounds != null ? bounds.Value.Width / originalWidth : 0f;
                float boundsHeightFactor = bounds != null ? bounds.Value.Height / originalHeight : 0f;

                using var canvas = new SKCanvas(bitmap);
                canvas.Clear(backgroundColor);

                for (int y = 0; y < verticalTileCount; y++)
                {
                    for (int x = 0; x < horizontalTileCount; x++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        RectangleF currentBounds;

                        if (bounds != null)
                        {
                            currentBounds = new(
                                (bounds.Value.X * (currentTileWidth / width)) + (currentTileWidth / horizontalTileCount * x * boundsWidthFactor),
                                (bounds.Value.Y * (currentTileHeight / height)) + (currentTileHeight / verticalTileCount * y * boundsHeightFactor),
                                currentTileWidth * boundsWidthFactor,
                                currentTileHeight * boundsHeightFactor);
                        }
                        else
                        {
                            currentBounds = new(
                                currentTileWidth / horizontalTileCount * x,
                                currentTileHeight / verticalTileCount * y,
                                currentTileWidth,
                                currentTileHeight);
                        }

                        using var subsetBitmap = RenderSubset(_file!, page, currentTileWidth, currentTileHeight, rotate, flags, renderFormFill, backgroundColor, currentBounds, width, height, cancellationToken);

                        cancellationToken.ThrowIfCancellationRequested();

                        canvas.DrawBitmap(subsetBitmap, new SKRect(
                            (float)Math.Floor(x * currentTileWidth),
                            (float)Math.Floor(y * currentTileHeight),
                            (float)Math.Floor(x * currentTileWidth + currentTileWidth),
                            (float)Math.Floor(y * currentTileHeight + currentTileHeight)));
                        canvas.Flush();
                    }
                }
            }

            return bitmap;
        }

        private static SKBitmap RenderSubset(PdfFile file, int page, float width, float height, PdfRotation rotate, NativeMethods.FPDF flags, bool renderFormFill, SKColor backgroundColor, RectangleF? bounds, float originalWidth, float originalHeight, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bitmap = new SKBitmap((int)width, (int)height, SKColorType.Bgra8888, SKAlphaType.Premul);
            IntPtr handle = IntPtr.Zero;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                handle = NativeMethods.FPDFBitmap_CreateEx((int)width, (int)height, NativeMethods.FPDFBitmap.BGRA, bitmap.GetPixels(), (int)width * 4);

                cancellationToken.ThrowIfCancellationRequested();
                NativeMethods.FPDFBitmap_FillRect(handle, 0, 0, (int)width, (int)height, (uint)backgroundColor);

                cancellationToken.ThrowIfCancellationRequested();
                bool success = file.RenderPDFPageToBitmap(
                    page,
                    handle,
                    bounds != null ? -(int)Math.Floor(bounds.Value.X * (originalWidth / bounds.Value.Width)) : 0,
                    bounds != null ? -(int)Math.Floor(bounds.Value.Y * (originalHeight / bounds.Value.Height)) : 0,
                    bounds != null ? (int)Math.Ceiling(originalWidth * (width / bounds.Value.Width)) : (int)Math.Ceiling(width),
                    bounds != null ? (int)Math.Ceiling(originalHeight * (height / bounds.Value.Height)) : (int)Math.Ceiling(height),
                    (int)rotate,
                    flags,
                    renderFormFill
                );

                if (!success)
                    throw new Win32Exception();
            }
            catch
            {
                bitmap?.Dispose();
                throw;
            }
            finally
            {
                if (handle != IntPtr.Zero)
                    NativeMethods.FPDFBitmap_Destroy(handle);
            }

            return bitmap;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <param name="disposing">Whether this method is called from <see cref="Dispose()"/>.</param>
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