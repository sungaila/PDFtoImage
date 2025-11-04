using SkiaSharp;
using System.Drawing;

namespace PDFtoImage
{
    /// <summary>
    /// Contains all relevant information to render a PDF page into an image.
    /// </summary>
    public interface IRenderOptions
    {
        /// <summary>
        /// The DPI scaling to use for rasterization of the PDF.
        /// </summary>
        int Dpi { get; init; }

        /// <summary>
        /// The width of the desired page. Use <see langword="null"/> if the original width should be used.
        /// </summary>
        int? Width { get; init; }

        /// <summary>
        /// The height of the desired page. Use <see langword="null"/> if the original height should be used.
        /// </summary>
        int? Height { get; init; }

        /// <summary>
        /// Specifies whether annotations be rendered.
        /// </summary>
        bool WithAnnotations { get; init; }

        /// <summary>
        /// Specifies whether form filling will be rendered.
        /// </summary>
        bool WithFormFill { get; init; }

        /// <summary>
        /// Specifies that <see cref="Width"/> or <see cref="Height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).
        /// </summary>
        bool WithAspectRatio { get; init; }

        /// <summary>
        /// Specifies the rotation at 90 degree intervals.
        /// </summary>
        PdfRotation Rotation { get; init; }

        /// <summary>
        /// Specifies which parts of the PDF should be anti-aliasing for rendering.
        /// </summary>
        PdfAntiAliasing AntiAliasing { get; init; }

        /// <summary>
        /// Specifies the background color. Defaults to <see cref="SKColors.White"/>.
        /// </summary>
        SKColor? BackgroundColor { get; init; }

        /// <summary>
        /// Specifies the bounds for the page relative to <see cref="Conversion.GetPageSizes(string,string)"/>. This can be used for clipping (bounds inside of page) or additional margins (bounds outside of page). The bound units are relative to the PDF size (at 72 DPI).
        /// </summary>
        RectangleF? Bounds { get; init; }

        /// <summary>
        /// Specifies that the PDF should be rendered as several segments and merged into the final image. This can help in cases where the output image is too large, causing corrupted images (e.g. missing text) or crashes.
        /// </summary>
        bool UseTiling { get; init; }

        /// <summary>
        /// Specifies that <see cref="Dpi"/> and <see cref="WithAspectRatio"/> will be calculated relative to <see cref="Bounds"/> instead of the original PDF.
        /// </summary>
        bool DpiRelativeToBounds { get; init; }

        /// <summary>
        /// Specifies that the PDF should be rendered in Grayscale mode.
        /// </summary>
        bool Grayscale { get; init; }
    }
}