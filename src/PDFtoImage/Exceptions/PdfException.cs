using System;
using static PDFtoImage.PdfiumViewer.NativeMethods;

namespace PDFtoImage.Exceptions
{
	/// <summary>
	/// Base class for all PDF related exceptions.
	/// </summary>
	public abstract class PdfException : Exception
	{
		internal abstract FPDF_ERR Error { get; }

		/// <inheritdoc/>
		protected PdfException(string message) : base(message) { }

		internal static PdfException? CreateException(FPDF_ERR error) => error switch
		{
			FPDF_ERR.SUCCESS => null,
			FPDF_ERR.UNKNOWN => new PdfUnknownException(),
			FPDF_ERR.FILE => new PdfCannotOpenFileException(),
			FPDF_ERR.FORMAT => new PdfInvalidFormatException(),
			FPDF_ERR.PASSWORD => new PdfPasswordProtectedException(),
			FPDF_ERR.SECURITY => new PdfUnsupportedSecuritySchemeException(),
			FPDF_ERR.PAGE => new PdfPageNotFoundException(),
			_ => new PdfUnknownException()
		};
	}
}