using System;

namespace PDFtoImage.PdfiumViewer
{
	internal sealed class PdfiumResolveEventArgs : EventArgs
	{
		public string? PdfiumFileName { get; set; }
	}

	internal delegate void PdfiumResolveEventHandler(object? sender, PdfiumResolveEventArgs e);
}