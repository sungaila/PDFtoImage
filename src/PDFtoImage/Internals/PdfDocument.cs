using PDFtoImage.Exceptions;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;

namespace PDFtoImage.Internals
{
    /// <summary>
    /// Provides functionality to render a PDF document.
    /// </summary>
    internal sealed class PdfDocument : IDisposable
    {
        private bool _disposed;
        private readonly PdfFile _file;

        /// <summary>
        /// Initializes a new instance of the PdfDocument class with the provided stream.
        /// </summary>
        /// <param name="stream">Stream for the PDF document.</param>
        /// <param name="password">Password for the PDF document.</param>
        /// <param name="disposeStream">Decides if <paramref name="stream"/> will closed on dispose as well.</param>
        public static PdfDocument Load(Stream stream, string? password, bool disposeStream)
        {
            return stream != null
                ? new PdfDocument(stream, password, disposeStream)
                : throw new ArgumentNullException(nameof(stream));
        }

        /// <summary>
        /// Size of each page in the PDF document. Each page is measured on first access and then cached.
        /// </summary>
        public IReadOnlyList<SizeF> PageSizes { get; private set; }

        private PdfDocument(Stream stream, string? password, bool disposeStream)
        {
            _file = new PdfFile(stream, password, disposeStream);

            try
            {
                PageSizes = new PdfPageSizes(_file, _file.GetPageCount());
            }
            catch
            {
                _file.Dispose();
                throw;
            }
        }

        private const int MaxTileWidth = 4000;
        private const int MaxTileHeight = 4000;

        internal static NativeMethods.FPDFRenderFlags GetRenderFlags(RenderOptions options)
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

        public SKBitmap Render(int page, float? requestedWidth, float? requestedHeight, float dpiX, float dpiY, PdfRotation rotate, NativeMethods.FPDFRenderFlags flags, bool renderFormFill, SKColor backgroundColor, RectangleF? bounds, bool useTiling, bool withAspectRatio, bool dpiRelativeToBounds, CancellationToken cancellationToken = default)
        {
            SKBitmap? bitmap = null;
            try
            {
                Render(page, requestedWidth, requestedHeight, dpiX, dpiY, rotate, flags, renderFormFill,
                    backgroundColor, bounds, useTiling, withAspectRatio, dpiRelativeToBounds,
                    (width, height) =>
                    {
                        bitmap = new SKBitmap(width, height, SKColorType.Bgra8888, SKAlphaType.Premul);
                        return (bitmap.GetPixels(), bitmap.RowBytes);
                    }, cancellationToken);
                return bitmap!;
            }
            catch
            {
                bitmap?.Dispose();
                throw;
            }
        }

        internal void Render(int page, RenderOptions options, IntPtr pixels, int rowBytes, CancellationToken cancellationToken = default) =>
            Render(page, options, (_, _) => (pixels, rowBytes), cancellationToken);

        // The callback runs after the output size is known so a worker can allocate or map
        // its final destination. PDFium then renders directly into the returned pointer.
        internal void Render(int page, RenderOptions options, Func<int, int, (IntPtr Pixels, int RowBytes)> getPixels, CancellationToken cancellationToken = default)
        {
            if (options == default)
                options = new();

            Render(page, options.Width, options.Height, options.Dpi, options.Dpi, options.Rotation,
                GetRenderFlags(options), options.WithFormFill, options.BackgroundColor ?? SKColors.White,
                options.Bounds, options.UseTiling, options.WithAspectRatio, options.DpiRelativeToBounds,
                getPixels, cancellationToken);
        }

#if NETCOREAPP
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1513")]
#endif
        private void Render(int page, float? requestedWidth, float? requestedHeight, float dpiX, float dpiY, PdfRotation rotate, NativeMethods.FPDFRenderFlags flags, bool renderFormFill, SKColor backgroundColor, RectangleF? bounds, bool useTiling, bool withAspectRatio, bool dpiRelativeToBounds, Func<int, int, (IntPtr Pixels, int RowBytes)> getPixels, CancellationToken cancellationToken)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            cancellationToken.ThrowIfCancellationRequested();

            ValidatePositiveFinite(dpiX, nameof(dpiX));
            ValidatePositiveFinite(dpiY, nameof(dpiY));

            if (requestedWidth.HasValue)
                ValidatePositiveFinite(requestedWidth.Value, nameof(requestedWidth));

            if (requestedHeight.HasValue)
                ValidatePositiveFinite(requestedHeight.Value, nameof(requestedHeight));

            if (rotate < PdfRotation.Rotate0 || rotate > PdfRotation.Rotate270)
                throw new ArgumentOutOfRangeException(nameof(rotate));

            if (bounds.HasValue)
            {
                ValidatePositiveFinite(bounds.Value.Width, nameof(bounds));
                ValidatePositiveFinite(bounds.Value.Height, nameof(bounds));

                if (!IsFinite(bounds.Value.X) || !IsFinite(bounds.Value.Y))
                    throw new ArgumentOutOfRangeException(nameof(bounds), "Bounds coordinates must be finite.");
            }

            // correct the width and height for the given dpi
            // but only if both width and height are not specified (so the original sizes are corrected)
            var correctFromDpi = requestedWidth == null && requestedHeight == null;

            var originalWidth = PageSizes[page].Width;
            var originalHeight = PageSizes[page].Height;

            if (!IsFinite(originalWidth) || !IsFinite(originalHeight) || originalWidth <= 0 || originalHeight <= 0)
                throw new PdfInvalidFormatException();

            if (withAspectRatio && !(dpiRelativeToBounds && bounds.HasValue))
            {
                AdjustForAspectRatio(ref requestedWidth, ref requestedHeight, PageSizes[page]);
            }

            float width = requestedWidth ?? originalWidth;
            float height = requestedHeight ?? originalHeight;

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

            if (dpiRelativeToBounds && bounds.HasValue)
            {
                float? boundsWidth = requestedWidth != null ? requestedWidth : null;
                float? boundsHeight = requestedHeight != null ? requestedHeight : null;

                if (withAspectRatio)
                {
                    AdjustForAspectRatio(ref boundsWidth, ref boundsHeight, new SizeF(bounds.Value.Width, bounds.Value.Height));
                }

                var remainderX = 0f;
                var remainderY = 0f;

                if (requestedWidth == null)
                {
                    var newWidth = boundsWidth ?? bounds.Value.Width;

#if NETFRAMEWORK
                    var roundedWidth = (float)Math.Ceiling(newWidth);
#else
                    var roundedWidth = MathF.Ceiling(newWidth);
#endif

                    remainderX = roundedWidth - newWidth;
                    width = roundedWidth;
                }

                if (requestedHeight == null)
                {
                    var newHeight = boundsHeight ?? bounds.Value.Height;

#if NETFRAMEWORK
                    var roundedHeight = (float)Math.Ceiling(newHeight);
#else
                    var roundedHeight = MathF.Ceiling(newHeight);
#endif

                    remainderY = roundedHeight - newHeight;
                    height = roundedHeight;
                }

                bounds = new RectangleF(
                    bounds.Value.X * (width / originalWidth),
                    bounds.Value.Y * (height / originalHeight),
                    bounds.Value.Width + remainderX,
                    bounds.Value.Height + remainderY);

                remainderX = bounds.Value.X % 1;
                remainderY = bounds.Value.Y % 1;

                bounds = new RectangleF(
                    bounds.Value.X,
                    bounds.Value.Y,
                    bounds.Value.Width + remainderX,
                    bounds.Value.Height + remainderY);
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

            // All consumers use signed 32-bit strides and byte counts. Validate before
            // allocating pixels or allowing float-to-int overflow to reach PDFium.
            var (bitmapWidth, bitmapHeight) = ValidateBitmapDimensions(width, height);
            _ = GetRenderBounds(width, height, bounds, originalWidth, originalHeight);
            cancellationToken.ThrowIfCancellationRequested();
            var (pixels, rowBytes) = getPixels(bitmapWidth, bitmapHeight);
            if (pixels == IntPtr.Zero || bitmapWidth <= 0 || bitmapHeight <= 0 || rowBytes < checked(bitmapWidth * 4))
                throw new ArgumentException("The destination must provide writable BGRA pixels for the rendered page.", nameof(getPixels));

            int horizontalTileCount = (int)Math.Ceiling(width / MaxTileWidth);
            int verticalTileCount = (int)Math.Ceiling(height / MaxTileHeight);

            if (!useTiling || (horizontalTileCount == 1 && verticalTileCount == 1))
            {
                RenderSubset(_file, page, width, height, rotate, flags, renderFormFill, backgroundColor,
                    bounds, originalWidth, originalHeight, pixels, rowBytes, cancellationToken);
            }
            else
            {
                using var bitmap = new SKBitmap();
                if (!bitmap.InstallPixels(new SKImageInfo(bitmapWidth, bitmapHeight, SKColorType.Bgra8888, SKAlphaType.Premul), pixels, rowBytes))
                    throw new InvalidOperationException("Skia could not use the destination pixel buffer for tiled rendering.");

                cancellationToken.ThrowIfCancellationRequested();

                float currentTileWidth = width / horizontalTileCount;
                float currentTileHeight = height / verticalTileCount;
                float boundsWidthFactor = bounds != null ? bounds.Value.Width / originalWidth : 0f;
                float boundsHeightFactor = bounds != null ? bounds.Value.Height / originalHeight : 0f;

                using var canvas = new SKCanvas(bitmap);
                // Each tile already contains its background. Copy translucent tiles
                // without blending that background into the destination a second time.
                using var paint = backgroundColor.Alpha == byte.MaxValue
                    ? null
                    : new SKPaint { BlendMode = SKBlendMode.Src };
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

                        canvas.DrawBitmap(
                            subsetBitmap,
                            new SKRect(
                                (float)Math.Floor(x * currentTileWidth),
                                (float)Math.Floor(y * currentTileHeight),
                                (float)Math.Floor(x * currentTileWidth + currentTileWidth),
                                (float)Math.Floor(y * currentTileHeight + currentTileHeight)),
                            SKSamplingOptions.Default, paint);
                    }
                }

                canvas.Flush();
            }

        }

        private static void AdjustForAspectRatio(ref float? width, ref float? height, SizeF pageSize)
        {
            if (width == null && height != null)
            {
                width = pageSize.Width / pageSize.Height * height.Value;
            }
            else if (width != null && height == null)
            {
                height = pageSize.Height / pageSize.Width * width.Value;
            }
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private static void ValidatePositiveFinite(float value, string parameterName)
        {
            if (!IsFinite(value) || value <= 0)
                throw new ArgumentOutOfRangeException(parameterName, "The value must be positive and finite.");
        }

        private static (int Width, int Height) ValidateBitmapDimensions(float width, float height)
        {
            if (!IsFinite(width) || !IsFinite(height) || width < 1 || height < 1 ||
                (double)width > int.MaxValue || (double)height > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(width), "The rendered page dimensions must fit positive 32-bit pixel counts.");

            var bitmapWidth = (int)width;
            var bitmapHeight = (int)height;
            if ((long)bitmapWidth * bitmapHeight > int.MaxValue / 4)
                throw new ArgumentOutOfRangeException(nameof(width), "The rendered BGRA bitmap must fit a signed 32-bit byte count.");
            return (bitmapWidth, bitmapHeight);
        }

        private static (int X, int Y, int Width, int Height) GetRenderBounds(float width, float height, RectangleF? bounds, float originalWidth, float originalHeight)
        {
            if (bounds.HasValue)
            {
                ValidatePositiveFinite(bounds.Value.Width, nameof(bounds));
                ValidatePositiveFinite(bounds.Value.Height, nameof(bounds));
                if (!IsFinite(bounds.Value.X) || !IsFinite(bounds.Value.Y))
                    throw new ArgumentOutOfRangeException(nameof(bounds), "Scaled bounds coordinates must be finite.");
            }

            return (
                ToRenderCoordinate(bounds.HasValue ? -Math.Floor(bounds.Value.X * (originalWidth / bounds.Value.Width)) : 0),
                ToRenderCoordinate(bounds.HasValue ? -Math.Floor(bounds.Value.Y * (originalHeight / bounds.Value.Height)) : 0),
                ToRenderCoordinate(bounds.HasValue ? Math.Ceiling(originalWidth * (width / bounds.Value.Width)) : Math.Ceiling(width)),
                ToRenderCoordinate(bounds.HasValue ? Math.Ceiling(originalHeight * (height / bounds.Value.Height)) : Math.Ceiling(height)));
        }

        private static int ToRenderCoordinate(double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value < int.MinValue || value > int.MaxValue)
                throw new ArgumentOutOfRangeException(nameof(value), "The scaled bounds must fit PDFium's signed 32-bit coordinates.");
            return (int)value;
        }

        private static SKBitmap RenderSubset(PdfFile file, int page, float width, float height, PdfRotation rotate, NativeMethods.FPDFRenderFlags flags, bool renderFormFill, SKColor backgroundColor, RectangleF? bounds, float originalWidth, float originalHeight, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var bitmap = new SKBitmap((int)width, (int)height, SKColorType.Bgra8888, SKAlphaType.Premul);
            try
            {
                RenderSubset(file, page, width, height, rotate, flags, renderFormFill, backgroundColor,
                    bounds, originalWidth, originalHeight, bitmap.GetPixels(), bitmap.RowBytes, cancellationToken);
                return bitmap;
            }
            catch
            {
                bitmap.Dispose();
                throw;
            }
        }

        private static void RenderSubset(PdfFile file, int page, float width, float height, PdfRotation rotate, NativeMethods.FPDFRenderFlags flags, bool renderFormFill, SKColor backgroundColor, RectangleF? bounds, float originalWidth, float originalHeight, IntPtr pixels, int rowBytes, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var renderBounds = GetRenderBounds(width, height, bounds, originalWidth, originalHeight);
            IntPtr handle = IntPtr.Zero;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                handle = NativeMethods.Bitmap_CreateEx((int)width, (int)height, NativeMethods.FPDFBitmap.BGRA, pixels, rowBytes, out var error);

                if (handle == IntPtr.Zero)
                    throw PdfException.CreateException(error) ?? new PdfUnknownException();

                cancellationToken.ThrowIfCancellationRequested();

                if (!NativeMethods.Bitmap_FillRect(handle, 0, 0, (int)width, (int)height, (uint)backgroundColor))
                    throw new InvalidOperationException("PDFium could not initialize the rendering bitmap.");

                cancellationToken.ThrowIfCancellationRequested();

                file.RenderPDFPageToBitmap(
                    page,
                    handle,
                    renderBounds.X,
                    renderBounds.Y,
                    renderBounds.Width,
                    renderBounds.Height,
                    (int)rotate,
                    flags,
                    renderFormFill
                );
            }
            finally
            {
                if (handle != IntPtr.Zero)
                    NativeMethods.Bitmap_Destroy(handle);
            }

            // FPDFBitmap_BGRA uses straight alpha. Skia bitmaps and the worker protocol
            // use premultiplied alpha, including tiles consumed by SKCanvas.
            if (backgroundColor.Alpha != byte.MaxValue)
                PremultiplyPixels(pixels, (int)width, (int)height, rowBytes, cancellationToken);
            cancellationToken.ThrowIfCancellationRequested();
        }

        private static unsafe void PremultiplyPixels(IntPtr pixels, int width, int height, int rowBytes, CancellationToken cancellationToken)
        {
            for (var y = 0; y < height; y++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var row = (byte*)pixels + (long)y * rowBytes;
                for (var x = 0; x < width; x++)
                {
                    var pixel = row + x * 4;
                    var alpha = pixel[3];
                    if (alpha == byte.MaxValue)
                        continue;
                    pixel[0] = (byte)((pixel[0] * alpha + 127) / 255);
                    pixel[1] = (byte)((pixel[1] * alpha + 127) / 255);
                    pixel[2] = (byte)((pixel[2] * alpha + 127) / 255);
                }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _file.Dispose();
        }
    }
}