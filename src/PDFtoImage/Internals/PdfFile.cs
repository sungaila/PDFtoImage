using PDFtoImage.Exceptions;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace PDFtoImage.Internals
{
    internal sealed class PdfFile : IDisposable
    {
        private IntPtr _document;
        private IntPtr _form;
        private bool _disposed;

        // PDFium retains the FPDF_FORMFILLINFO pointer until the form handle is closed.
        private IntPtr _formFillInfoPtr;
        private readonly int _id;
        private Stream? _stream;
        private readonly bool _disposeStream;

        // Keep the availability provider, its native FPDF_FILEACCESS storage and the
        // StreamManager registration alive until the document has been closed. Availability-backed
        // PDFium parsing may continue to use the file-access callbacks while the document is alive.
        private IntPtr _avail;
        private IntPtr _fileAccessState;

        public PdfFile(Stream stream, string? password, bool disposeStream)
        {
            _stream = stream ?? throw new ArgumentNullException(nameof(stream));
            _disposeStream = disposeStream;

            try
            {
                if (!_stream.CanRead)
                    throw new ArgumentException("The given stream does not support reading.", nameof(stream));

                if (!_stream.CanSeek)
                    throw new ArgumentException("The given stream does not support seeking.", nameof(stream));

                // Read Length only once. Besides validating the stream, this exact value is passed
                // to FPDF_FILEACCESS so custom streams cannot report a different length later.
                var length = _stream.Length;

                if (length > uint.MaxValue)
                {
#if BROWSER
                    throw new NotSupportedException("PDF streams larger than 4 GiB are not supported on WebAssembly.");
#elif !NET6_0_OR_GREATER
                    throw new NotSupportedException("PDF streams larger than 4 GiB are not supported by the legacy interop bindings used by this target framework.");
#else
                    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        throw new NotSupportedException("PDF streams larger than 4 GiB cannot be accessed on Windows. This is a technical limitation of PDFium's FPDF_FILEACCESS API.");

                    if (IntPtr.Size == 4)
                        throw new NotSupportedException("PDF streams larger than 4 GiB are not supported on 32-bit platforms.");
#endif
                }

                PdfLibrary.EnsureLoaded();

                _id = StreamManager.Register(stream);

                _avail = NativeMethods.Avail_Create(length, _id, out _fileAccessState);

                if (_avail == IntPtr.Zero)
                    throw new PdfUnknownException();

                // Give PDFium's availability parser one document pass before opening it.
                // PDFtoImage only exposes a complete seekable stream, so IsDataAvail always reports
                // requested ranges as present and download hints cannot cause additional data to
                // arrive. Keep GetDocument() as the conclusive open/error operation.
                _ = NativeMethods.Avail_IsDocAvail(_avail);

                _document = NativeMethods.Avail_GetDocument(_avail, password);

                if (_document == IntPtr.Zero)
                    throw PdfException.CreateException(NativeMethods.GetLastError()) ?? new PdfUnknownException();

                // Let the same availability context process form-related data before initializing
                // the form-fill environment. PDF_FORM_NOTEXIST is a normal result, and there is no
                // download cycle to drive because the complete stream is already available.
                _ = NativeMethods.Avail_IsFormAvail(_avail);

                (_form, _formFillInfoPtr) = CreateFormEnvironment(_document);
            }
            catch
            {
                Cleanup(disposing: true);
                throw;
            }
        }

        private static (IntPtr form, IntPtr formFillInfoPtr) CreateFormEnvironment(IntPtr document)
        {
            var ffi = new NativeMethods.FPDF_FORMFILLINFO(1);

            var formFillInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<NativeMethods.FPDF_FORMFILLINFO>());

            try
            {
                Marshal.StructureToPtr(ffi, formFillInfoPtr, false);

                var form = NativeMethods.Doc_InitFormFillEnvironment(document, formFillInfoPtr);

                if (form == IntPtr.Zero)
                    throw new PdfUnknownException();

                return (form, formFillInfoPtr);
            }
            catch
            {
                Marshal.FreeHGlobal(formFillInfoPtr);
                throw;
            }
        }

        private static void DestroyFormEnvironment(ref IntPtr form, ref IntPtr formFillInfoPtr)
        {
            try
            {
                if (form != IntPtr.Zero)
                {
                    NativeMethods.Doc_ExitFormFillEnvironment(form);
                    form = IntPtr.Zero;
                }
            }
            finally
            {
                if (formFillInfoPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(formFillInfoPtr);
                    formFillInfoPtr = IntPtr.Zero;
                }
            }
        }

        public void RenderPDFPageToBitmap(int pageNumber, IntPtr bitmapHandle, int boundsOriginX, int boundsOriginY, int boundsWidth, int boundsHeight, int rotate, NativeMethods.FPDFRenderFlags flags, bool renderFormFill)
        {
            ThrowIfDisposed();

            using var pageData = new PageData(this, pageNumber);

            NativeMethods.RenderPageBitmap(bitmapHandle, pageData.Page, boundsOriginX, boundsOriginY, boundsWidth, boundsHeight, rotate, flags);

            if (renderFormFill)
            {
                NativeMethods.RemoveFormFieldHighlight(_form);
                NativeMethods.FFLDraw(_form, bitmapHandle, pageData.Page, boundsOriginX, boundsOriginY, boundsWidth, boundsHeight, rotate, flags);
            }
        }

        public int GetPageCount()
        {
            ThrowIfDisposed();

            return NativeMethods.GetPageCount(_document);
        }

        public SizeF GetPDFDocInfo(int pageNumber)
        {
            ThrowIfDisposed();

            ResolvePage(pageNumber);
            if (NativeMethods.GetPageSizeByIndex(_document, pageNumber, out double width, out double height))
                return new SizeF((float)width, (float)height);

            throw new PdfPageNotFoundException();
        }

        private IntPtr LoadPage(int pageNumber)
        {
            ResolvePage(pageNumber);

            var page = NativeMethods.LoadPage(_document, pageNumber);
            if (page != IntPtr.Zero)
                return page;

            var error = NativeMethods.GetLastError();
            throw PdfException.CreateException(error) ?? new PdfPageNotFoundException();
        }

        // Run the page-availability pass before the regular page APIs. For problematic linearized
        // PDFs this lets PDFium process availability metadata that can make pages reachable even when
        // the normal /Pages walk cannot resolve them. Because the full stream is already present,
        // download hints cannot change availability; the following page API remains the conclusive
        // test of whether the requested page can actually be used.
        private void ResolvePage(int pageNumber)
        {
            if (pageNumber >= 0)
                _ = NativeMethods.Avail_IsPageAvail(_avail, pageNumber);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Cleanup(disposing: true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private void Cleanup(bool disposing)
        {
            try
            {
                DestroyFormEnvironment(ref _form, ref _formFillInfoPtr);
            }
            finally
            {
                try
                {
                    if (_document != IntPtr.Zero)
                    {
                        NativeMethods.CloseDocument(_document);
                        _document = IntPtr.Zero;
                    }
                }
                finally
                {
                    try
                    {
                        // The document uses the availability provider's file-access callbacks.
                        // Close the document first, then destroy the provider, and only then release
                        // the unmanaged FPDF_FILEACCESS storage passed to FPDFAvail_Create().
                        if (_avail != IntPtr.Zero || _fileAccessState != IntPtr.Zero)
                        {
                            NativeMethods.Avail_Destroy(_avail, _fileAccessState);
                            _avail = IntPtr.Zero;
                            _fileAccessState = IntPtr.Zero;
                        }
                    }
                    finally
                    {
                        StreamManager.Unregister(_id);

                        if (disposing && _disposeStream)
                        {
                            _stream?.Dispose();
                            _stream = null;
                        }
                    }
                }
            }
        }

        private void ThrowIfDisposed()
        {
#if NET6_0_OR_GREATER
            ObjectDisposedException.ThrowIf(_disposed, this);
#else
            if (_disposed)
                throw new ObjectDisposedException(nameof(PdfFile));
#endif
        }

        private sealed class PageData : IDisposable
        {
            private readonly IntPtr _form;
            private bool _disposed;

            public IntPtr Page { get; private set; }

            public double Width { get; private set; }

            public double Height { get; private set; }

            public PageData(PdfFile file, int pageNumber)
            {
                var page = file.LoadPage(pageNumber);
                _form = file._form;

                try
                {
                    Page = page;
                    NativeMethods.OnAfterLoadPage(Page, _form);

                    Width = NativeMethods.GetPageWidth(Page);
                    Height = NativeMethods.GetPageHeight(Page);
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }

            public void Dispose()
            {
                if (_disposed)
                    return;

                var page = Page;

                Page = IntPtr.Zero;
                _disposed = true;

                if (page == IntPtr.Zero)
                    return;

                try
                {
                    NativeMethods.Form_OnBeforeClosePage(page, _form);
                }
                finally
                {
                    NativeMethods.ClosePage(page);
                }
            }
        }
    }
}