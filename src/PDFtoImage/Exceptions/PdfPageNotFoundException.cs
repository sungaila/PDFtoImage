using static PDFtoImage.Internals.NativeMethods;

namespace PDFtoImage.Exceptions
{
    /// <summary>
    /// Thrown if the PDF does not contain the given page number.
    /// </summary>
    public class PdfPageNotFoundException : PdfException
    {
        internal override FPDF_ERR Error => FPDF_ERR.PAGE;

        /// <inheritdoc/>
        public PdfPageNotFoundException() : base("Page not found or content error.") { }
    }
}