using System;

namespace PDFtoImage
{
    /// <summary>
    /// Specifies which parts of the PDF should be rendered with anti-aliasing.
    /// </summary>
    [Flags]
    public enum PdfAntiAliasing
    {
        /// <summary>
        /// No anti-aliasing.
        /// </summary>
        None = 0,

        /// <summary>
        /// Use anti-aliasing on text.
        /// </summary>
        Text = 1 << 0,

        /// <summary>
        /// Use anti-aliasing on images.
        /// </summary>
        Images = 1 << 1,

        /// <summary>
        /// Use anti-aliasing on paths.
        /// </summary>
        Paths = 1 << 2,

        /// <summary>
        /// Use anti-aliasing on everything.
        /// </summary>
        All = Text | Images | Paths
    }
}