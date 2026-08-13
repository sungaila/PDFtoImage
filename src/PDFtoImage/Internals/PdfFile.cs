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
        private IntPtr _formFillInfoPtr;
        private readonly int _id;
        private Stream? _stream;
        private readonly bool _disposeStream;

        private IntPtr _avail;
        private IntPtr _availState;
        private bool _availAttempted;
        private bool _usesAvail;

        // PDFium needs it again if the document is reopened through the availability API.
        private readonly string? _password;

        public PdfFile(Stream stream, string? password, bool disposeStream)
        {
            PdfLibrary.EnsureLoaded();

            _stream = stream ?? throw new ArgumentNullException(nameof(stream));

            try
            {
                // test if the given stream is seekable by getting its length
                _ = _stream.Length;
            }
            catch (NotSupportedException ex)
            {
                if (!_stream.CanSeek)
                    throw new ArgumentException("The given stream does not support seeking.", nameof(stream), ex);

                throw;
            }

            if (stream.Length > uint.MaxValue)
            {
                throw new NotSupportedException("PDF streams larger than 4 GiB are not supported.");
            }

            try
            {
                _id = StreamManager.Register(stream);
                _disposeStream = disposeStream;
                _password = password;

                var document = NativeMethods.LoadCustomDocument(stream, password, _id);

                if (document == IntPtr.Zero)
                    throw PdfException.CreateException(NativeMethods.GetLastError())!;

                _document = document;

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
                    throw PdfException.CreateException(NativeMethods.GetLastError())!;

                NativeMethods.SetFormFieldHighlightColor(form, 0, 0xFFE4DD);
                NativeMethods.SetFormFieldHighlightAlpha(form, 100);

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

            if (TryGetPageSize(pageNumber, out var size))
                return size;

            if (TryReopenThroughAvailability() && TryGetPageSize(pageNumber, out size))
                return size;

            throw new PdfPageNotFoundException();
        }

        private bool TryGetPageSize(int pageNumber, out SizeF size)
        {
            ResolvePage(pageNumber);

            if (NativeMethods.GetPageSizeByIndex(_document, pageNumber, out double width, out double height))
            {
                size = new SizeF((float)width, (float)height);
                return true;
            }

            size = default;
            return false;
        }

        private IntPtr LoadPageWithFallback(int pageNumber)
        {
            ResolvePage(pageNumber);

            var page = NativeMethods.LoadPage(_document, pageNumber);

            if (page != IntPtr.Zero)
                return page;

            // Captured before the retry, which resets it.
            var error = NativeMethods.GetLastError();

            if (TryReopenThroughAvailability())
            {
                ResolvePage(pageNumber);

                page = NativeMethods.LoadPage(_document, pageNumber);

                if (page != IntPtr.Zero)
                    return page;
            }

            // PDFium can refuse a page without recording an error for it.
            throw PdfException.CreateException(error) ?? new PdfPageNotFoundException();
        }

        // Resolves the page through the availability layer, which is what reaches pages the
        // /Pages walk cannot. A readable stream is reported as not available indefinitely, so
        // the answer is deliberately ignored: only the page call that follows is conclusive.
        private void ResolvePage(int pageNumber)
        {
            if (_usesAvail && pageNumber >= 0)
                _ = NativeMethods.Avail_IsPageAvail(_avail, pageNumber);
        }

        // Reopens through the availability API, which resolves pages of a linearized document
        // through its hint tables instead of by walking /Pages. At most once per document.
        private bool TryReopenThroughAvailability()
        {
            if (_usesAvail || _availAttempted || _stream == null)
                return false;

            _availAttempted = true;

            var avail = IntPtr.Zero;
            var availState = IntPtr.Zero;
            var document = IntPtr.Zero;
            var form = IntPtr.Zero;
            var formFillInfoPtr = IntPtr.Zero;
            var reopened = false;

            try
            {
                avail = NativeMethods.Avail_Create(_stream, _id, out availState);

                if (avail == IntPtr.Zero)
                    return false;

                // Only a linearized document has the hint tables this depends on. An
                // undecided answer still tries; only a definite no rules the reopen out.
                if (NativeMethods.Avail_IsLinearized(avail) == NativeMethods.FPDF_LINEARIZATION.NOT_LINEARIZED)
                    return false;

                // Advances the parser onto the cross reference data. Its error codes are
                // vaguer than the load's, so only the load is acted on.
                _ = NativeMethods.Avail_IsDocAvail(avail);

                document = NativeMethods.Avail_GetDocument(avail, _password);

                if (document == IntPtr.Zero)
                    return false;

                (form, formFillInfoPtr) = CreateFormEnvironment(document);

                // Nothing below can fail, so the document is never left half swapped.
                DestroyFormEnvironment(ref _form, ref _formFillInfoPtr);
                NativeMethods.CloseDocument(_document);

                _document = document;
                _form = form;
                _formFillInfoPtr = formFillInfoPtr;
                _avail = avail;
                _availState = availState;
                _usesAvail = true;
                reopened = true;

                return true;
            }
            catch (PdfException)
            {
                // The open document still serves the pages it can reach.
                return false;
            }
            finally
            {
                if (!reopened)
                {
                    DestroyFormEnvironment(ref form, ref formFillInfoPtr);

                    if (document != IntPtr.Zero)
                        NativeMethods.CloseDocument(document);

                    NativeMethods.Avail_Destroy(avail, availState);
                }
            }
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
                        // After the document, which was created from it.
                        if (_avail != IntPtr.Zero || _availState != IntPtr.Zero)
                        {
                            NativeMethods.Avail_Destroy(_avail, _availState);
                            _avail = IntPtr.Zero;
                            _availState = IntPtr.Zero;
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
                var page = file.LoadPageWithFallback(pageNumber);

                // Read after the load, which may have reopened the document onto a new one.
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