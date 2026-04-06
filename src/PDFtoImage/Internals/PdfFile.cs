using PDFtoImage.Exceptions;
using System;
using System.Collections.Generic;
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

            var document = NativeMethods.LoadCustomDocument(stream, password, _id);
            if (document == IntPtr.Zero)
                throw PdfException.CreateException(NativeMethods.GetLastError())!;

            _document = document;

            var ffi = new NativeMethods.FPDF_FORMFILLINFO(1);

            _formFillInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf<NativeMethods.FPDF_FORMFILLINFO>());
            Marshal.StructureToPtr(ffi, _formFillInfoPtr, false);

            _form = NativeMethods.Doc_InitFormFillEnvironment(_document, _formFillInfoPtr);

            if (_form == IntPtr.Zero)
                throw PdfException.CreateException(NativeMethods.GetLastError())!;

            NativeMethods.SetFormFieldHighlightColor(_form, 0, 0xFFE4DD);
            NativeMethods.SetFormFieldHighlightAlpha(_form, 100);
        }

        public bool RenderPDFPageToBitmap(int pageNumber, IntPtr bitmapHandle, int boundsOriginX, int boundsOriginY, int boundsWidth, int boundsHeight, int rotate, NativeMethods.FPDFRenderFlags flags, bool renderFormFill)
        {
            ThrowIfDisposed();

            using var pageData = new PageData(_document, _form, pageNumber);

            NativeMethods.RenderPageBitmap(bitmapHandle, pageData.Page, boundsOriginX, boundsOriginY, boundsWidth, boundsHeight, rotate, flags);

            if (renderFormFill)
            {
                NativeMethods.RemoveFormFieldHighlight(_form);
                NativeMethods.FFLDraw(_form, bitmapHandle, pageData.Page, boundsOriginX, boundsOriginY, boundsWidth, boundsHeight, rotate, flags);
            }

            return true;
        }

        public List<SizeF> GetPDFDocInfo()
        {
            ThrowIfDisposed();

            int pageCount = NativeMethods.GetPageCount(_document);
            var result = new List<SizeF>(pageCount);

            for (int i = 0; i < pageCount; i++)
            {
                result.Add(GetPDFDocInfo(i));
            }

            return result;
        }

        public SizeF GetPDFDocInfo(int pageNumber)
        {
            ThrowIfDisposed();

            NativeMethods.GetPageSizeByIndex(_document, pageNumber, out double width, out double height);

            return new SizeF((float)width, (float)height);
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
            StreamManager.Unregister(_id);

            if (_form != IntPtr.Zero)
            {
                NativeMethods.Doc_ExitFormFillEnvironment(_form);
                _form = IntPtr.Zero;
            }

            if (_document != IntPtr.Zero)
            {
                NativeMethods.CloseDocument(_document);
                _document = IntPtr.Zero;
            }

            if (_formFillInfoPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(_formFillInfoPtr);
                _formFillInfoPtr = IntPtr.Zero;
            }

            if (disposing && _disposeStream && _stream is not null)
            {
                _stream.Dispose();
                _stream = null;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(PdfFile));
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

                Page = NativeMethods.LoadPage(document, pageNumber);
                TextPage = NativeMethods.Text_LoadPage(Page);
                NativeMethods.OnAfterLoadPage(Page, form);

                Width = NativeMethods.GetPageWidth(Page);
                Height = NativeMethods.GetPageHeight(Page);
            }

            public void Dispose()
            {
                if (!_disposed)
                {
                    NativeMethods.Form_OnBeforeClosePage(Page, _form);
                    NativeMethods.Text_ClosePage(TextPage);
                    NativeMethods.ClosePage(Page);

                    _disposed = true;
                }
            }
        }
    }
}