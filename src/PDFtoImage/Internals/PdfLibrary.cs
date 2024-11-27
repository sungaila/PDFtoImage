using System;

namespace PDFtoImage.Internals
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
#if NETSTANDARD
                if (_library == null)
                    LibraryLoader.LoadLocalLibrary<PdfDocument>("pdfium");
#else
                // .NET (Core) and Xamarin resolve the pdfium lib on their own
#endif
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter")]
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