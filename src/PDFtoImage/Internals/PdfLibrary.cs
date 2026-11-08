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

        private bool _initialized;

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

        // PDFium is shared by all AppDomains/load contexts.
        // Only an explicit shutdown, after all native objects have been closed, may destroy its global state.
        public void Dispose()
        {
            lock (_syncRoot)
            {
                if (!_initialized)
                    return;

                NativeMethods.DestroyLibrary();
                _initialized = false;

                if (ReferenceEquals(_library, this))
                    _library = null;
            }
        }
    }
}