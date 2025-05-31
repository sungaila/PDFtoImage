using PDFtoImage.Exceptions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace PDFtoImage.Internals
{
    internal struct PdfFile : IDisposable
    {
        private IntPtr _document;
        private IntPtr _form;
        private bool _disposed;
        private IntPtr _formFillInfoPtr;
        private readonly int _id;
        private Stream? _stream;
        private readonly bool _disposeStream;

        public PdfFile(Stream stream, string? password, bool disposeStream)
        {
            PdfLibrary.EnsureLoaded();

            _stream = stream ?? throw new ArgumentNullException(nameof(stream));

            try
            {
                // test if the given stream is seekable by getting its length
                var _ = _stream.Length;
            }
            catch (NotSupportedException ex)
            {
                if (!_stream.CanSeek)
                    throw new ArgumentException("The given stream does not support seeking.", nameof(stream), ex);

                throw;
            }

            _id = StreamManager.Register(stream);
            _disposeStream = disposeStream;

            var document = NativeMethods.FPDF_LoadCustomDocument(stream, password, _id);
            if (document == IntPtr.Zero)
                throw PdfException.CreateException(NativeMethods.FPDF_GetLastError())!;

            _disposeStream = disposeStream;

            _document = document;

            NativeMethods.FPDF_GetDocPermissions(_document);

            var ffi = new NativeMethods.FPDF_FORMFILLINFO(1);

            _formFillInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<NativeMethods.FPDF_FORMFILLINFO>());
            Marshal.StructureToPtr(ffi, _formFillInfoPtr, false);

            _form = NativeMethods.FPDFDOC_InitFormFillEnvironment(_document, _formFillInfoPtr);

            if (_form == IntPtr.Zero)
                throw PdfException.CreateException(NativeMethods.FPDF_GetLastError())!;

            NativeMethods.FPDF_SetFormFieldHighlightColor(_form, 0, 0xFFE4DD);
            NativeMethods.FPDF_SetFormFieldHighlightAlpha(_form, 100);
        }

        public readonly bool RenderPDFPageToBitmap(int pageNumber, IntPtr bitmapHandle, int boundsOriginX, int boundsOriginY, int boundsWidth, int boundsHeight, int rotate, NativeMethods.FPDF flags, bool renderFormFill)
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            using var pageData = new PageData(_document, _form, pageNumber);

            NativeMethods.FPDF_RenderPageBitmap(bitmapHandle, pageData.Page, boundsOriginX, boundsOriginY, boundsWidth, boundsHeight, rotate, flags);

            if (renderFormFill)
            {
                NativeMethods.FPDF_RemoveFormFieldHighlight(_form);
                NativeMethods.FPDF_FFLDraw(_form, bitmapHandle, pageData.Page, boundsOriginX, boundsOriginY, boundsWidth, boundsHeight, rotate, flags);
            }

            return true;
        }

        public readonly List<SizeF> GetPDFDocInfo()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);

            int pageCount = NativeMethods.FPDF_GetPageCount(_document);
            var result = new List<SizeF>(pageCount);

            for (int i = 0; i < pageCount; i++)
            {
                result.Add(GetPDFDocInfo(i));
            }

            return result;
        }

        public readonly SizeF GetPDFDocInfo(int pageNumber)
        {
            NativeMethods.FPDF_GetPageSizeByIndex(_document, pageNumber, out double width, out double height);

            return new SizeF((float)width, (float)height);
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter")]
        private void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            StreamManager.Unregister(_id);

            if (_form != IntPtr.Zero)
            {
                NativeMethods.FPDFDOC_ExitFormFillEnvironment(_form);
                _form = IntPtr.Zero;
            }

            if (_document != IntPtr.Zero)
            {
                NativeMethods.FPDF_CloseDocument(_document);
                _document = IntPtr.Zero;
            }

            if (_formFillInfoPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_formFillInfoPtr);
                _formFillInfoPtr = IntPtr.Zero;
            }

            if (_stream != null && _disposeStream)
            {
                _stream.Dispose();
                _stream = null;
            }

            _disposed = true;
        }

        private sealed class PageData : IDisposable
        {
            private readonly IntPtr _form;
            private bool _disposed;

            public IntPtr Page { get; private set; }

            public IntPtr TextPage { get; private set; }

            public double Width { get; private set; }

            public double Height { get; private set; }

            public PageData(IntPtr document, IntPtr form, int pageNumber)
            {
                _form = form;

                Page = NativeMethods.FPDF_LoadPage(document, pageNumber);
                TextPage = NativeMethods.FPDFText_LoadPage(Page);
                NativeMethods.FORM_OnAfterLoadPage(Page, form);

                Width = NativeMethods.FPDF_GetPageWidth(Page);
                Height = NativeMethods.FPDF_GetPageHeight(Page);
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    NativeMethods.FORM_OnBeforeClosePage(Page, _form);
                    NativeMethods.FPDFText_ClosePage(TextPage);
                    NativeMethods.FPDF_ClosePage(Page);

                    _disposed = true;
                }
            }
        }
    }
}