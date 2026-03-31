#if !NET6_0_OR_GREATER
using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace PDFtoImage.Internals
{
    internal static partial class NativeMethods
    {
        public static void SetFormFieldHighlightColor(IntPtr hHandle, int fieldType, uint color)
        {
            lock (LockString)
            {
                Imports.FPDF_SetFormFieldHighlightColor(hHandle, fieldType, color);
            }
        }

        public static bool Bitmap_FillRect(IntPtr bitmapHandle, int left, int top, int width, int height, uint color)
        {
            lock (LockString)
            {
                return Imports.FPDFBitmap_FillRect(bitmapHandle, left, top, width, height, color) != 0;
            }
        }

        public static int GetPageSizeByIndex(IntPtr document, int page_index, out double width, out double height)
        {
            lock (LockString)
            {
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
            var getBlock = Marshal.GetFunctionPointerForDelegate(_getBlockDelegate);
            var access = new FPDF_FILEACCESS((uint)input.Length, getBlock, (IntPtr)id);

            var size = Marshal.SizeOf<FPDF_FILEACCESS>();
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(access, ptr, false);

            byte[]? passwordBytes = password != null
               ? Encoding.UTF8.GetBytes(password + '\0')
               : null;

            try
            {
                fixed (byte* passwordPointer = passwordBytes)
                {
                    lock (LockString)
                    {
                        return Imports.FPDF_LoadCustomDocument(ptr, (IntPtr)passwordPointer);
                    }
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public static FPDF_ERR GetLastError()
        {
            lock (LockString)
            {
                return (FPDF_ERR)Imports.FPDF_GetLastError();
            }
        }

        private static int FPDF_GetBlock(IntPtr param, uint position, IntPtr buffer, uint size)
        {
            long positionConverted;
            int sizeConverted;

            positionConverted = position;
            sizeConverted = (int)size;

            var stream = StreamManager.Get((int)param);

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
#pragma warning disable IDE0079
#pragma warning disable SYSLIB1054
            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDF_InitLibrary();

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDF_DestroyLibrary();

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDF_CloseDocument(IntPtr document);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern int FPDF_GetPageCount(IntPtr document);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDF_SetFormFieldHighlightColor(IntPtr hHandle, int fieldType, uint color);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDF_SetFormFieldHighlightAlpha(IntPtr hHandle, byte alpha);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr FPDF_LoadPage(IntPtr document, int page_index);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr FPDFText_LoadPage(IntPtr page);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FORM_OnAfterLoadPage(IntPtr page, IntPtr _form);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern double FPDF_GetPageWidth(IntPtr page);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern double FPDF_GetPageHeight(IntPtr page);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FORM_OnBeforeClosePage(IntPtr page, IntPtr _form);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDFText_ClosePage(IntPtr text_page);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDF_ClosePage(IntPtr page);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDF_RenderPageBitmap(IntPtr bitmap, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, int flags);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern int FPDF_GetPageSizeByIndex(IntPtr document, int page_index, out double width, out double height);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr FPDFBitmap_CreateEx(int width, int height, int format, IntPtr first_scan, int stride);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern int FPDFBitmap_FillRect(IntPtr bitmapHandle, int left, int top, int width, int height, uint color);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDFBitmap_Destroy(IntPtr bitmapHandle);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern uint FPDF_GetLastError();

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDF_FFLDraw(IntPtr form, IntPtr bitmap, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, int flags);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDF_RemoveFormFieldHighlight(IntPtr form);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA2101")]
            public static extern IntPtr FPDF_LoadCustomDocument(IntPtr access, IntPtr password);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr FPDFDOC_InitFormFillEnvironment(IntPtr document, IntPtr formInfo);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDFDOC_ExitFormFillEnvironment(IntPtr handle);
#pragma warning restore SYSLIB1054
#pragma warning restore IDE0079
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int FPDF_GetBlockDelegate(IntPtr param, uint position, IntPtr buffer, uint size);

        private static readonly FPDF_GetBlockDelegate _getBlockDelegate = FPDF_GetBlock;

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct FPDF_FILEACCESS(uint m_FileLen, IntPtr m_GetBlock, IntPtr m_Param)
        {
            private readonly uint m_FileLen = m_FileLen;
            private readonly IntPtr m_GetBlock = m_GetBlock;
            private readonly IntPtr m_Param = m_Param;
        }
    }
}
#endif