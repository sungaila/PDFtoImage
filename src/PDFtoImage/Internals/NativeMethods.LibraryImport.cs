#if NET6_0_OR_GREATER && !BROWSER
using System;
using System.Buffers;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PDFtoImage.Internals
{
    internal static partial class NativeMethods
    {
        public static bool Bitmap_FillRect(IntPtr bitmapHandle, int left, int top, int width, int height, uint color)
        {
            lock (LockString)
            {
                return Imports.FPDFBitmap_FillRect(bitmapHandle, left, top, width, height, new CULong(color)) != 0;
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
                return (FPDF_ERR)checked((uint)Imports.FPDF_GetLastError().Value.ToUInt64());
            }
        }

        private unsafe static IntPtr CreateAvailFileAccessState(Stream input, int id)
        {
            delegate* unmanaged[Cdecl]<IntPtr, CULong, IntPtr, CULong, int> getBlock = &FPDF_GetBlock;
            var access = new FPDF_FILEACCESS(new CULong((uint)input.Length), getBlock, id);

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

        private static IntPtr Avail_GetDocumentCore(IntPtr avail, string? password)
            => Imports.FPDFAvail_GetDocument(avail, password);

        private unsafe static IntPtr GetIsDataAvailCallbackPointer()
        {
            delegate* unmanaged[Cdecl]<IntPtr, nuint, nuint, int> callback = &FX_IsDataAvail;
            return (IntPtr)callback;
        }

        private unsafe static IntPtr GetAddSegmentCallbackPointer()
        {
            delegate* unmanaged[Cdecl]<IntPtr, nuint, nuint, void> callback = &FX_AddSegment;
            return (IntPtr)callback;
        }

        // PDFtoImage gives PDFium a complete seekable stream, matching pdfium_test's local-file
        // availability provider: all requested byte ranges are reported as present and download
        // hints are intentionally ignored.
        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        private static int FX_IsDataAvail(IntPtr param, nuint offset, nuint size) => 1;

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        private static void FX_AddSegment(IntPtr param, nuint offset, nuint size) { }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
        private static int FPDF_GetBlock(IntPtr param, CULong position, IntPtr buffer, CULong size)
        {
            byte[]? rentedBuffer = null;

            try
            {
                var streamId = checked((int)param.ToInt64());
                var nativePosition = position.Value.ToUInt64();
                var nativeSize = size.Value.ToUInt64();

                if (nativePosition > long.MaxValue)
                    return 0;

                if (nativeSize > int.MaxValue)
                    return 0;

                var positionConverted = (long)nativePosition;
                var sizeConverted = (int)nativeSize;

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
            public static partial IntPtr FPDF_LoadPage(IntPtr document, int page_index);

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
            public static partial void FPDF_ClosePage(IntPtr page);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDF_RenderPageBitmap(IntPtr bitmap, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, int flags);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial int FPDF_GetPageSizeByIndex(IntPtr document, int page_index, out double width, out double height);

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

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial IntPtr FPDFAvail_Create(IntPtr file_avail, IntPtr file);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial void FPDFAvail_Destroy(IntPtr avail);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial int FPDFAvail_IsDocAvail(IntPtr avail, IntPtr hints);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial int FPDFAvail_IsPageAvail(IntPtr avail, int page_index, IntPtr hints);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial int FPDFAvail_IsFormAvail(IntPtr avail, IntPtr hints);

            [LibraryImport("pdfium", StringMarshalling = StringMarshalling.Utf8)]
            [UnmanagedCallConv(CallConvs = [typeof(CallConvCdecl)])]
            public static partial IntPtr FPDFAvail_GetDocument(IntPtr avail, string? password);

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