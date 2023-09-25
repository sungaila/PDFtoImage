namespace PDFtoImage
{
	/// <summary>
	/// Specifies the rotation of pages shown in the PDF renderer.
	/// </summary>
	public enum PdfRotation
	{
		/// <summary>
		/// No rotation.
		/// </summary>
		Rotate0 = 0,

		/// <summary>
		/// Rotated 90 degrees clockwise.
		/// </summary>
		Rotate90 = 1,

		/// <summary>
		/// Rotated 180 degrees.
		/// </summary>
		Rotate180 = 2,

		/// <summary>
		/// Rotated 90 degrees counter-clockwise.
		/// </summary>
		Rotate270 = 3
	}
}