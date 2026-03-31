#if NET6_0_OR_GREATER
using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PDFtoImage.Internals
{
    internal static partial class NativeMethods
    {
        public static void SetFormFieldHighlightColor(IntPtr hHandle, int fieldType, uint color)
        {
            lock (LockString)
            {
                Imports.FPDF_SetFormFieldHighlightColor(hHandle, fieldType, new CULong(color));
            }
        }

        public static bool Bitmap_FillRect(IntPtr bitmapHandle, int left, int top, int width, int height, uint color)
        {
            lock (LockString)
            {
                return Imports.FPDFBitmap_FillRect(bitmapHandle, left, top, width, height, new CULong(color)) != 0;
            }
        }

        private static readonly bool _isBrowserFallbackUsed = OperatingSystem.IsBrowser();

        public static int GetPageSizeByIndex(IntPtr document, int page_index, out double width, out double height)
        {
            lock (LockString)
            {
                if (_isBrowserFallbackUsed)
                    return Imports.FPDF_GetPageSizeByIndex_Fallback(document, page_index, out width, out height);

                return Imports.FPDF_GetPageSizeByIndex(document, page_index, out width, out height);
            }
        }

        /// <summary>
        /// Opens a document using a .NET Stream. Allows opening huge
        /// PDFs without loading them into memory first.
        /// </summary>
        /// <param name="input">The input Stream. Don't dispose prior to closing the pdf.</param>
        /// <param name="password">Password, if the PDF is protected. Can be null.</param>
        /// <param name="id">Retrieves an IntPtr to the COM object for the Stream. The caller must release this with Marshal.Release prior to Disposing the Stream.</param>
        /// <returns>An IntPtr to the FPDF_DOCUMENT object.</returns>
        public unsafe static IntPtr LoadCustomDocument(Stream input, string? password, int id)
        {
            delegate* unmanaged[Cdecl]<IntPtr, CULong, IntPtr, CULong, int> getBlock = &FPDF_GetBlock;
            var access = new FPDF_FILEACCESS(new CULong((uint)input.Length), getBlock, id);

            lock (LockString)
            {
                return Imports.FPDF_LoadCustomDocument(in access, password);
            }
        }

        public static FPDF_ERR GetLastError()
        {
            lock (LockString)
            {
                return (FPDF_ERR)checked((uint)Imports.FPDF_GetLastError().Value.ToUInt64());
            }
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        private static int FPDF_GetBlock(IntPtr param, CULong position, IntPtr buffer, CULong size)
        {
            long positionConverted;
            int sizeConverted;

            positionConverted = checked((uint)position.Value.ToUInt64());
            sizeConverted = (int)checked((uint)size.Value.ToUInt64());

            var stream = StreamManager.Get(checked((int)param));

            if (stream == null)
                return 0;

            stream.Position = positionConverted;

            byte[] rentedBuffer = ArrayPool<byte>.Shared.Rent(sizeConverted);

            try
            {
                int read = stream.Read(rentedBuffer, 0, sizeConverted);

                if (read != sizeConverted)
                    return 0;

                Marshal.Copy(rentedBuffer, 0, buffer, sizeConverted);
                return 1;
            }
            catch
            {
                return 0;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(rentedBuffer, clearArray: false);
            }
        }

        private static partial class Imports
        {
            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDF_InitLibrary();

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDF_DestroyLibrary();

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDF_CloseDocument(IntPtr document);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial int FPDF_GetPageCount(IntPtr document);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDF_SetFormFieldHighlightColor(IntPtr hHandle, int fieldType, CULong color);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDF_SetFormFieldHighlightAlpha(IntPtr hHandle, byte alpha);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial IntPtr FPDF_LoadPage(IntPtr document, int page_index);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial IntPtr FPDFText_LoadPage(IntPtr page);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FORM_OnAfterLoadPage(IntPtr page, IntPtr _form);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial double FPDF_GetPageWidth(IntPtr page);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial double FPDF_GetPageHeight(IntPtr page);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FORM_OnBeforeClosePage(IntPtr page, IntPtr _form);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDFText_ClosePage(IntPtr text_page);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDF_ClosePage(IntPtr page);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDF_RenderPageBitmap(IntPtr bitmap, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, int flags);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial int FPDF_GetPageSizeByIndex(IntPtr document, int page_index, out double width, out double height);

            [DllImport("pdfium", EntryPoint = "FPDF_GetPageSizeByIndex", CallingConvention = CallingConvention.Cdecl)]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "SYSLIB1054")]
            public static extern int FPDF_GetPageSizeByIndex_Fallback(IntPtr document, int page_index, out double width, out double height);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial IntPtr FPDFBitmap_CreateEx(int width, int height, int format, IntPtr first_scan, int stride);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial int FPDFBitmap_FillRect(IntPtr bitmapHandle, int left, int top, int width, int height, CULong color);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDFBitmap_Destroy(IntPtr bitmapHandle);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial CULong FPDF_GetLastError();

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDF_FFLDraw(IntPtr form, IntPtr bitmap, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, int flags);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDF_RemoveFormFieldHighlight(IntPtr form);

            [LibraryImport("pdfium", StringMarshalling = StringMarshalling.Utf8)]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial IntPtr FPDF_LoadCustomDocument(in FPDF_FILEACCESS access, string? password);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial IntPtr FPDFDOC_InitFormFillEnvironment(IntPtr document, IntPtr formInfo);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDFDOC_ExitFormFillEnvironment(IntPtr handle);
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe readonly struct FPDF_FILEACCESS(CULong m_FileLen, delegate* unmanaged[Cdecl]<IntPtr, CULong, IntPtr, CULong, int> m_GetBlock, IntPtr m_Param)
        {
            private readonly CULong m_FileLen = m_FileLen;
            private readonly delegate* unmanaged[Cdecl]<IntPtr, CULong, IntPtr, CULong, int> m_GetBlock = m_GetBlock;
            private readonly IntPtr m_Param = m_Param;
        }
    }
}
#endif