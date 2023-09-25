using static PDFtoImage.PdfiumViewer.NativeMethods;

namespace PDFtoImage.Exceptions
{
	/// <summary>
	/// Thrown on unknown PDF errors.
	/// </summary>
	public class PdfUnknownException : PdfException
	{
		internal override FPDF_ERR Error => FPDF_ERR.UNKNOWN;

		/// <inheritdoc/>
		public PdfUnknownException() : base("Unknown error.") { }
	}
}