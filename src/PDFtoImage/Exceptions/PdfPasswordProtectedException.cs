using static PDFtoImage.PdfiumViewer.NativeMethods;

namespace PDFtoImage.Exceptions
{
	/// <summary>
	/// Thrown if the PDF requires a password and the given password was not given or incorrect.
	/// </summary>
	public class PdfPasswordProtectedException : PdfException
	{
		internal override FPDF_ERR Error => FPDF_ERR.PASSWORD;

		/// <inheritdoc/>
		public PdfPasswordProtectedException() : base("Password required or incorrect password.") { }
	}
}