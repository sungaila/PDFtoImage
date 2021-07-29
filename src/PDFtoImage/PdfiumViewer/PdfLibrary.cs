using System;

namespace PDFtoImage.PdfiumViewer
{
    internal class PdfLibrary : IDisposable
    {
        private static readonly object _syncRoot = new();
        private static PdfLibrary? _library;
        private bool disposedValue;

        public static void EnsureLoaded()
        {
            lock (_syncRoot)
            {
                if (_library == null)
                    _library = new PdfLibrary();
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

        protected virtual void Dispose(bool disposing)
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