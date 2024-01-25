using static PDFtoImage.Internals.NativeMethods;

namespace PDFtoImage.Exceptions
{
	/// <summary>
	/// Thrown if the PDF format is invalid or corrupted.
	/// </summary>
	public class PdfInvalidFormatException : PdfException
	{
		internal override FPDF_ERR Error => FPDF_ERR.FORMAT;

		/// <inheritdoc/>
		public PdfInvalidFormatException() : base("File not in PDF format or corrupted.") { }
	}
}