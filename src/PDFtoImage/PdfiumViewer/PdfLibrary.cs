using System;

namespace PDFtoImage.PdfiumViewer
{
	internal sealed class PdfLibrary : IDisposable
	{
		private static readonly object _syncRoot = new();
		private static PdfLibrary? _library;
		private bool disposedValue;

		public static void EnsureLoaded()
		{
			lock (_syncRoot)
			{
				_library ??= new PdfLibrary();
			}
		}

		private PdfLibrary()
		{
			NativeMethods.FPDF_InitLibrary();
		}

		~PdfLibrary()
		{
			Dispose(disposing: false);
		}

		private void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				NativeMethods.FPDF_DestroyLibrary();
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}