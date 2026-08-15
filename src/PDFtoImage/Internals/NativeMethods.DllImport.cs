#if !NET6_0_OR_GREATER || BROWSER
using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace PDFtoImage.Internals
{
    internal static partial class NativeMethods
    {
        public static bool Bitmap_FillRect(IntPtr bitmapHandle, int left, int top, int width, int height, uint color)
        {
            lock (LockString)
            {
                return Imports.FPDFBitmap_FillRect(bitmapHandle, left, top, width, height, color) != 0;
            }
        }

        public static bool GetPageSizeByIndex(IntPtr document, int page_index, out double width, out double height)
        {
            lock (LockString)
            {
                return Imports.FPDF_GetPageSizeByIndex(document, page_index, out width, out height) != 0;
            }
        }

        public static FPDF_ERR GetLastError()
        {
            lock (LockString)
            {
                return (FPDF_ERR)Imports.FPDF_GetLastError();
            }
        }

        private unsafe static IntPtr CreateAvailFileAccessState(Stream input, int id)
        {
#if BROWSER
            delegate* unmanaged[Cdecl]<IntPtr, uint, IntPtr, uint, int> getBlock = &FPDF_GetBlock;
            var access = new FPDF_FILEACCESS((uint)input.Length, (IntPtr)getBlock, id);
#else
            var getBlock = Marshal.GetFunctionPointerForDelegate(_getBlockDelegate);
            var access = new FPDF_FILEACCESS((uint)input.Length, getBlock, (IntPtr)id);
#endif

            var fileAccessState = Marshal.AllocHGlobal(Marshal.SizeOf<FPDF_FILEACCESS>());

            try
            {
                Marshal.StructureToPtr(access, fileAccessState, false);
                return fileAccessState;
            }
            catch
            {
                Marshal.FreeHGlobal(fileAccessState);
                throw;
            }
        }

        private unsafe static IntPtr Avail_GetDocumentCore(IntPtr avail, string? password)
        {
            byte[]? passwordBytes = password != null
               ? Encoding.UTF8.GetBytes(password + '\0')
               : null;

            fixed (byte* passwordPointer = passwordBytes)
            {
                return Imports.FPDFAvail_GetDocument(avail, (IntPtr)passwordPointer);
            }
        }

        private unsafe static IntPtr GetIsDataAvailCallbackPointer()
        {
#if BROWSER
            delegate* unmanaged[Cdecl]<IntPtr, UIntPtr, UIntPtr, int> callback = &FX_IsDataAvail;
            return (IntPtr)callback;
#else
            return Marshal.GetFunctionPointerForDelegate(_isDataAvailDelegate);
#endif
        }

        private unsafe static IntPtr GetAddSegmentCallbackPointer()
        {
#if BROWSER
            delegate* unmanaged[Cdecl]<IntPtr, UIntPtr, UIntPtr, void> callback = &FX_AddSegment;
            return (IntPtr)callback;
#else
            return Marshal.GetFunctionPointerForDelegate(_addSegmentDelegate);
#endif
        }

        // PDFtoImage gives PDFium a complete seekable stream, matching pdfium_test's local-file
        // availability provider: all requested byte ranges are reported as present and download
        // hints are intentionally ignored.
#if BROWSER
[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
        // needed for Unity IL2CPP compilation
        [AOT.MonoPInvokeCallback(typeof(FX_IsDataAvailDelegate))]
#endif
        private static int FX_IsDataAvail(IntPtr param, UIntPtr offset, UIntPtr size) => 1;

#if BROWSER
[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
        // needed for Unity IL2CPP compilation
        [AOT.MonoPInvokeCallback(typeof(FX_AddSegmentDelegate))]
#endif
        private static void FX_AddSegment(IntPtr param, UIntPtr offset, UIntPtr size) { }

#if BROWSER
[UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#else
        // needed for Unity IL2CPP compilation
        [AOT.MonoPInvokeCallback(typeof(FPDF_GetBlockDelegate))]
#endif
        private static int FPDF_GetBlock(IntPtr param, uint position, IntPtr buffer, uint size)
        {
            byte[]? rentedBuffer = null;

            try
            {
                var streamId = checked((int)param.ToInt64());
                var positionConverted = (long)position;

                if (size > int.MaxValue)
                    return 0;

                var sizeConverted = (int)size;

                if (sizeConverted == 0)
                    return 1;

                if (buffer == IntPtr.Zero)
                    return 0;

                var stream = StreamManager.Get(streamId);

                if (stream == null || !stream.CanRead || !stream.CanSeek)
                    return 0;

                stream.Position = positionConverted;

                rentedBuffer = ArrayPool<byte>.Shared.Rent(sizeConverted);

                var totalRead = 0;

                while (totalRead < sizeConverted)
                {
                    var read = stream.Read(rentedBuffer, totalRead, sizeConverted - totalRead);

                    if (read <= 0)
                        return 0;

                    totalRead += read;
                }

                Marshal.Copy(rentedBuffer, 0, buffer, sizeConverted);

                return 1;
            }
            catch
            {
                return 0;
            }
            finally
            {
                if (rentedBuffer != null)
                {
                    try
                    {
                        ArrayPool<byte>.Shared.Return(rentedBuffer, clearArray: false);
                    }
                    catch { }
                }
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
            public static extern IntPtr FPDF_LoadPage(IntPtr document, int page_index);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FORM_OnAfterLoadPage(IntPtr page, IntPtr _form);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern double FPDF_GetPageWidth(IntPtr page);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern double FPDF_GetPageHeight(IntPtr page);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FORM_OnBeforeClosePage(IntPtr page, IntPtr _form);

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
            public static extern IntPtr FPDFAvail_Create(IntPtr file_avail, IntPtr file);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDFAvail_Destroy(IntPtr avail);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern int FPDFAvail_IsDocAvail(IntPtr avail, IntPtr hints);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern int FPDFAvail_IsPageAvail(IntPtr avail, int page_index, IntPtr hints);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern int FPDFAvail_IsFormAvail(IntPtr avail, IntPtr hints);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA2101")]
            public static extern IntPtr FPDFAvail_GetDocument(IntPtr avail, IntPtr password);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr FPDFDOC_InitFormFillEnvironment(IntPtr document, IntPtr formInfo);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDFDOC_ExitFormFillEnvironment(IntPtr handle);
#pragma warning restore SYSLIB1054
#pragma warning restore IDE0079
        }

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int FPDF_GetBlockDelegate(IntPtr param, uint position, IntPtr buffer, uint size);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int FX_IsDataAvailDelegate(IntPtr param, UIntPtr offset, UIntPtr size);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void FX_AddSegmentDelegate(IntPtr param, UIntPtr offset, UIntPtr size);

#if !BROWSER
        private static readonly FPDF_GetBlockDelegate _getBlockDelegate = FPDF_GetBlock;

        private static readonly FX_IsDataAvailDelegate _isDataAvailDelegate = FX_IsDataAvail;

        private static readonly FX_AddSegmentDelegate _addSegmentDelegate = FX_AddSegment;
#endif

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