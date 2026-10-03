using System;

namespace PDFtoImage.Internals
{
    internal sealed class PdfLibrary : IDisposable
    {
#if NET9_0_OR_GREATER
        private static readonly System.Threading.Lock _syncRoot = new();
#else
        private static readonly object _syncRoot = new();
#endif
        private static PdfLibrary? _library;

        private readonly bool _initialized;

        private bool disposedValue;

        public static void EnsureLoaded()
        {
            lock (_syncRoot)
            {
#if NETFRAMEWORK
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
            NativeMethods.InitLibrary();
            _initialized = true;
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
                disposedValue = true;
                // A finalizer also runs for an object whose constructor threw. In
                // particular, a missing PDFium binary must not cause another native
                // load failure on the finalizer thread and terminate the host process.
                if (_initialized)
                    NativeMethods.DestroyLibrary();
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}