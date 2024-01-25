﻿namespace PDFtoImage.Internals
{
	internal static class PdfiumResolver
	{
		public static event PdfiumResolveEventHandler? Resolve;

		private static void OnResolve(PdfiumResolveEventArgs e)
		{
			Resolve?.Invoke(null, e);
		}

		internal static string? GetPdfiumFileName()
		{
			var e = new PdfiumResolveEventArgs();
			OnResolve(e);
			return e.PdfiumFileName;
		}
	}
}