using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PDFtoImage.PdfiumViewer
{
#if NET8_0_OR_GREATER
#pragma warning disable CA2101 // Specify marshalling for P/Invoke string arguments
#pragma warning disable SYSLIB1054 // Use LibraryImportAttribute instead of DllImportAttribute to generate p/invoke marshalling code at compile time.
#endif
    internal static partial class NativeMethods
    {
        // Interned strings are cached over AppDomains. This means that when we
        // lock on this string, we actually lock over AppDomain's. The Pdfium
        // library is not thread safe, and this way of locking
        // guarantees that we don't access the Pdfium library from different
        // threads, even when there are multiple AppDomain's in play.
        private static readonly string LockString = string.Intern("e362349b-001d-4cb2-bf55-a71606a3e36f");

        public static void FPDF_InitLibrary()
        {
            lock (LockString)
            {
                Imports.FPDF_InitLibrary();
            }
        }

        public static void FPDF_DestroyLibrary()
        {
            lock (LockString)
            {
                Imports.FPDF_DestroyLibrary();
            }
        }

        public static void FPDF_CloseDocument(IntPtr document)
        {
            lock (LockString)
            {
                Imports.FPDF_CloseDocument(document);
            }
        }

        public static int FPDF_GetPageCount(IntPtr document)
        {
            lock (LockString)
            {
                return Imports.FPDF_GetPageCount(document);
            }
        }

        public static uint FPDF_GetDocPermissions(IntPtr document)
        {
            lock (LockString)
            {
                return Imports.FPDF_GetDocPermissions(document);
            }
        }

        public static IntPtr FPDFDOC_InitFormFillEnvironment(IntPtr document, FPDF_FORMFILLINFO formInfo)
        {
            lock (LockString)
            {
                return Imports.FPDFDOC_InitFormFillEnvironment(document, formInfo);
            }
        }

        public static void FPDF_SetFormFieldHighlightColor(IntPtr hHandle, int fieldType, uint color)
        {
            lock (LockString)
            {
                Imports.FPDF_SetFormFieldHighlightColor(hHandle, fieldType, color);
            }
        }

        public static void FPDF_SetFormFieldHighlightAlpha(IntPtr hHandle, byte alpha)
        {
            lock (LockString)
            {
                Imports.FPDF_SetFormFieldHighlightAlpha(hHandle, alpha);
            }
        }

        public static void FORM_DoDocumentJSAction(IntPtr hHandle)
        {
            lock (LockString)
            {
                Imports.FORM_DoDocumentJSAction(hHandle);
            }
        }

        public static void FORM_DoDocumentOpenAction(IntPtr hHandle)
        {
            lock (LockString)
            {
                Imports.FORM_DoDocumentOpenAction(hHandle);
            }
        }

        public static void FORM_DoDocumentAAction(IntPtr hHandle, FPDFDOC_AACTION aaType)
        {
            lock (LockString)
            {
                Imports.FORM_DoDocumentAAction(hHandle, aaType);
            }
        }

        public static IntPtr FPDF_LoadPage(IntPtr document, int page_index)
        {
            lock (LockString)
            {
                return Imports.FPDF_LoadPage(document, page_index);
            }
        }

        public static IntPtr FPDFText_LoadPage(IntPtr page)
        {
            lock (LockString)
            {
                return Imports.FPDFText_LoadPage(page);
            }
        }

        public static void FORM_OnAfterLoadPage(IntPtr page, IntPtr _form)
        {
            lock (LockString)
            {
                Imports.FORM_OnAfterLoadPage(page, _form);
            }
        }

        public static void FORM_DoPageAAction(IntPtr page, IntPtr _form, FPDFPAGE_AACTION fPDFPAGE_AACTION)
        {
            lock (LockString)
            {
                Imports.FORM_DoPageAAction(page, _form, fPDFPAGE_AACTION);
            }
        }

        public static double FPDF_GetPageWidth(IntPtr page)
        {
            lock (LockString)
            {
                return Imports.FPDF_GetPageWidth(page);
            }
        }

        public static double FPDF_GetPageHeight(IntPtr page)
        {
            lock (LockString)
            {
                return Imports.FPDF_GetPageHeight(page);
            }
        }

        public static void FORM_OnBeforeClosePage(IntPtr page, IntPtr _form)
        {
            lock (LockString)
            {
                Imports.FORM_OnBeforeClosePage(page, _form);
            }
        }

        public static void FPDFText_ClosePage(IntPtr text_page)
        {
            lock (LockString)
            {
                Imports.FPDFText_ClosePage(text_page);
            }
        }

        public static void FPDF_ClosePage(IntPtr page)
        {
            lock (LockString)
            {
                Imports.FPDF_ClosePage(page);
            }
        }

        public static void FPDF_RenderPageBitmap(IntPtr bitmapHandle, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, FPDF flags)
        {
            lock (LockString)
            {
                Imports.FPDF_RenderPageBitmap(bitmapHandle, page, start_x, start_y, size_x, size_y, rotate, flags);
            }
        }

        public static int FPDF_GetPageSizeByIndex(IntPtr document, int page_index, out double width, out double height)
        {
            lock (LockString)
            {
                return Imports.FPDF_GetPageSizeByIndex(document, page_index, out width, out height);
            }
        }

        public static IntPtr FPDFBitmap_CreateEx(int width, int height, FPDFBitmap format, IntPtr first_scan, int stride)
        {
            lock (LockString)
            {
                return Imports.FPDFBitmap_CreateEx(width, height, (int)format, first_scan, stride);
            }
        }

        public static void FPDFBitmap_FillRect(IntPtr bitmapHandle, int left, int top, int width, int height, uint color)
        {
            lock (LockString)
            {
                Imports.FPDFBitmap_FillRect(bitmapHandle, left, top, width, height, color);
            }
        }

        public static void FPDFBitmap_Destroy(IntPtr bitmapHandle)
        {
            lock (LockString)
            {
                Imports.FPDFBitmap_Destroy(bitmapHandle);
            }
        }

        public static uint FPDFDest_GetDestPageIndex(IntPtr document, IntPtr dest)
        {
            lock (LockString)
            {
                return Imports.FPDFDest_GetDestPageIndex(document, dest);
            }
        }

        public static IntPtr FPDF_BookmarkGetFirstChild(IntPtr document, IntPtr bookmark)
        {
            lock (LockString)
                return Imports.FPDFBookmark_GetFirstChild(document, bookmark);
        }

        public static IntPtr FPDF_BookmarkGetNextSibling(IntPtr document, IntPtr bookmark)
        {
            lock (LockString)
                return Imports.FPDFBookmark_GetNextSibling(document, bookmark);
        }

        public static uint FPDF_BookmarkGetTitle(IntPtr bookmark, byte[]? buffer, uint buflen)
        {
            lock (LockString)
                return Imports.FPDFBookmark_GetTitle(bookmark, buffer, buflen);
        }

        public static IntPtr FPDF_BookmarkGetDest(IntPtr document, IntPtr bookmark)
        {
            lock (LockString)
                return Imports.FPDFBookmark_GetDest(document, bookmark);
        }

        /// <summary>
        /// Opens a document using a .NET Stream. Allows opening huge
        /// PDFs without loading them into memory first.
        /// </summary>
        /// <param name="input">The input Stream. Don't dispose prior to closing the pdf.</param>
        /// <param name="password">Password, if the PDF is protected. Can be null.</param>
        /// <param name="id">Retrieves an IntPtr to the COM object for the Stream. The caller must release this with Marshal.Release prior to Disposing the Stream.</param>
        /// <returns>An IntPtr to the FPDF_DOCUMENT object.</returns>

        public static
#if NET6_0_OR_GREATER
            unsafe
#endif
            IntPtr FPDF_LoadCustomDocument(Stream input, string? password, int id)
        {
#if NET6_0_OR_GREATER
            delegate* unmanaged[Cdecl]<IntPtr, uint, IntPtr, uint, int> getBlock = &FPDF_GetBlock;
#else
			var getBlock = Marshal.GetFunctionPointerForDelegate(_getBlockDelegate);
#endif

            var access = new FPDF_FILEACCESS
            {
                m_FileLen = (uint)input.Length,
                m_GetBlock = getBlock,
                m_Param = (IntPtr)id
            };

            lock (LockString)
            {
                return Imports.FPDF_LoadCustomDocument(access, password);
            }
        }

        public static FPDF_ERR FPDF_GetLastError()
        {
            lock (LockString)
            {
                return (FPDF_ERR)Imports.FPDF_GetLastError();
            }
        }

        public static void FPDF_FFLDraw(IntPtr form, IntPtr bitmap, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, FPDF flags)
        {
            lock (LockString)
            {
                Imports.FPDF_FFLDraw(form, bitmap, page, start_x, start_y, size_x, size_y, rotate, flags);
            }
        }

        public static void FPDF_RemoveFormFieldHighlight(IntPtr form)
        {
            lock (LockString)
            {
                Imports.FPDF_RemoveFormFieldHighlight(form);
            }
        }

#if !NET6_0_OR_GREATER
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int FPDF_GetBlockDelegate(IntPtr param, uint position, IntPtr buffer, uint size);

		private static readonly FPDF_GetBlockDelegate _getBlockDelegate = FPDF_GetBlock;
#endif

#if NET6_0_OR_GREATER
        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvCdecl)])]
#endif
        private static int FPDF_GetBlock(IntPtr param, uint position, IntPtr buffer, uint size)
        {
            var stream = StreamManager.Get(checked((int)param));
            if (stream == null)
                return 0;
            byte[] managedBuffer = new byte[size];

            stream.Position = position;
            int read = stream.Read(managedBuffer, 0, (int)size);
            if (read != size)
                return 0;

            Marshal.Copy(managedBuffer, 0, buffer, (int)size);
            return 1;
        }

        private static partial class Imports
        {
            // TODO: LibraryImportAttribute is not supported by Blazor WebAssembly right now
#if NET7_0_OR_GREATER && FALSE
            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial void FPDF_InitLibrary();

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial void FPDF_DestroyLibrary();

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial void FPDF_CloseDocument(IntPtr document);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial int FPDF_GetPageCount(IntPtr document);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial uint FPDF_GetDocPermissions(IntPtr document);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial void FPDF_SetFormFieldHighlightColor(IntPtr hHandle, int fieldType, uint color);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial void FPDF_SetFormFieldHighlightAlpha(IntPtr hHandle, byte alpha);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial void FORM_DoDocumentJSAction(IntPtr hHandle);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial void FORM_DoDocumentOpenAction(IntPtr hHandle);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial void FORM_DoDocumentAAction(IntPtr hHandle, FPDFDOC_AACTION aaType);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial IntPtr FPDF_LoadPage(IntPtr document, int page_index);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial IntPtr FPDFText_LoadPage(IntPtr page);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial void FORM_OnAfterLoadPage(IntPtr page, IntPtr _form);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial void FORM_DoPageAAction(IntPtr page, IntPtr _form, FPDFPAGE_AACTION fPDFPAGE_AACTION);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial double FPDF_GetPageWidth(IntPtr page);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial double FPDF_GetPageHeight(IntPtr page);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial void FORM_OnBeforeClosePage(IntPtr page, IntPtr _form);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial void FPDFText_ClosePage(IntPtr text_page);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial void FPDF_ClosePage(IntPtr page);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial void FPDF_RenderPageBitmap(IntPtr bitmapHandle, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, FPDF flags);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial int FPDF_GetPageSizeByIndex(IntPtr document, int page_index, out double width, out double height);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial IntPtr FPDFBitmap_CreateEx(int width, int height, int format, IntPtr first_scan, int stride);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial void FPDFBitmap_FillRect(IntPtr bitmapHandle, int left, int top, int width, int height, uint color);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial void FPDFBitmap_Destroy(IntPtr bitmapHandle);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial uint FPDFDest_GetDestPageIndex(IntPtr document, IntPtr dest);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial IntPtr FPDFBookmark_GetFirstChild(IntPtr document, IntPtr bookmark);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial IntPtr FPDFBookmark_GetNextSibling(IntPtr document, IntPtr bookmark);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial uint FPDFBookmark_GetTitle(IntPtr bookmark, byte[]? buffer, uint buflen);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial IntPtr FPDFBookmark_GetDest(IntPtr document, IntPtr bookmark);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial uint FPDF_GetLastError();

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial void FPDF_FFLDraw(IntPtr form, IntPtr bitmap, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, FPDF flags);

            [LibraryImport("pdfium")]
            [UnmanagedCallConv(CallConvs = new Type[] { typeof(CallConvCdecl) })]
            public static partial void FPDF_RemoveFormFieldHighlight(IntPtr form);
#else
            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDF_InitLibrary();

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDF_DestroyLibrary();

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDF_CloseDocument(IntPtr document);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern int FPDF_GetPageCount(IntPtr document);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern uint FPDF_GetDocPermissions(IntPtr document);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDF_SetFormFieldHighlightColor(IntPtr hHandle, int fieldType, uint color);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDF_SetFormFieldHighlightAlpha(IntPtr hHandle, byte alpha);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FORM_DoDocumentJSAction(IntPtr hHandle);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FORM_DoDocumentOpenAction(IntPtr hHandle);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FORM_DoDocumentAAction(IntPtr hHandle, FPDFDOC_AACTION aaType);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr FPDF_LoadPage(IntPtr document, int page_index);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr FPDFText_LoadPage(IntPtr page);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FORM_OnAfterLoadPage(IntPtr page, IntPtr _form);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FORM_DoPageAAction(IntPtr page, IntPtr _form, FPDFPAGE_AACTION fPDFPAGE_AACTION);

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
            public static extern void FPDF_RenderPageBitmap(IntPtr bitmapHandle, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, FPDF flags);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern int FPDF_GetPageSizeByIndex(IntPtr document, int page_index, out double width, out double height);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr FPDFBitmap_CreateEx(int width, int height, int format, IntPtr first_scan, int stride);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDFBitmap_FillRect(IntPtr bitmapHandle, int left, int top, int width, int height, uint color);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDFBitmap_Destroy(IntPtr bitmapHandle);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern uint FPDFDest_GetDestPageIndex(IntPtr document, IntPtr dest);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr FPDFBookmark_GetFirstChild(IntPtr document, IntPtr bookmark);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr FPDFBookmark_GetNextSibling(IntPtr document, IntPtr bookmark);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern uint FPDFBookmark_GetTitle(IntPtr bookmark, byte[]? buffer, uint buflen);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr FPDFBookmark_GetDest(IntPtr document, IntPtr bookmark);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern uint FPDF_GetLastError();

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDF_FFLDraw(IntPtr form, IntPtr bitmap, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, FPDF flags);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern void FPDF_RemoveFormFieldHighlight(IntPtr form);
#endif

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
            public static extern IntPtr FPDF_LoadCustomDocument(FPDF_FILEACCESS access, string? password);

            [DllImport("pdfium", CallingConvention = CallingConvention.Cdecl)]
            public static extern IntPtr FPDFDOC_InitFormFillEnvironment(IntPtr document, FPDF_FORMFILLINFO formInfo);
        }

        [StructLayout(LayoutKind.Sequential)]
        public class FPDF_FORMFILLINFO
        {
            public int version;

            private readonly IntPtr Release;

            private readonly IntPtr FFI_Invalidate;

            private readonly IntPtr FFI_OutputSelectedRect;

            private readonly IntPtr FFI_SetCursor;

            private readonly IntPtr FFI_SetTimer;

            private readonly IntPtr FFI_KillTimer;

            private readonly IntPtr FFI_GetLocalTime;

            private readonly IntPtr FFI_OnChange;

            private readonly IntPtr FFI_GetPage;

            private readonly IntPtr FFI_GetCurrentPage;

            private readonly IntPtr FFI_GetRotation;

            private readonly IntPtr FFI_ExecuteNamedAction;

            private readonly IntPtr FFI_SetTextFieldFocus;

            private readonly IntPtr FFI_DoURIAction;

            private readonly IntPtr FFI_DoGoToAction;

            private readonly IntPtr m_pJsPlatform;

            private readonly IntPtr FFI_DisplayCaret;

            private readonly IntPtr FFI_GetCurrentPageIndex;

            private readonly IntPtr FFI_SetCurrentPage;

            private readonly IntPtr FFI_GotoURL;

            private readonly IntPtr FFI_GetPageViewRect;

            private readonly IntPtr FFI_PageEvent;

            private readonly IntPtr FFI_PopupMenu;

            private readonly IntPtr FFI_OpenFile;

            private readonly IntPtr FFI_EmailTo;

            private readonly IntPtr FFI_UploadTo;

            private readonly IntPtr FFI_GetPlatform;

            private readonly IntPtr FFI_GetLanguage;

            private readonly IntPtr FFI_DownloadFromURL;

            private readonly IntPtr FFI_PostRequestURL;

            private readonly IntPtr FFI_PutRequestURL;
        }

        public enum FPDFDOC_AACTION
        {
            WC = 0x10,
            WS = 0x11,
            DS = 0x12,
            WP = 0x13,
            DP = 0x14
        }

        public enum FPDFPAGE_AACTION
        {
            OPEN = 0,
            CLOSE = 1
        }

        /// <summary>
        /// A number indicating for bitmap format.
        /// </summary>
        public enum FPDFBitmap : int
        {
            /// <summary>
            /// Gray scale bitmap, one byte per pixel.
            /// </summary>
            Gray = 1,

            /// <summary>
            /// 3 bytes per pixel, byte order: blue, green, red.
            /// </summary>
            BGR = 2,

            /// <summary>
            /// 4 bytes per pixel, byte order: blue, green, red, unused.
            /// </summary>
            BGRx = 3,

            /// <summary>
            /// 4 bytes per pixel, byte order: blue, green, red, alpha.
            /// </summary>
            BGRA = 4
        }

        [Flags]
        public enum FPDF
        {
            /// <summary>
            /// Set if annotations are to be rendered.
            /// </summary>
            ANNOT = 0x01,

            /// <summary>
            /// Set if using text rendering optimized for LCD display. This flag will only take effect if anti-aliasing is enabled for text.
            /// </summary>
            LCD_TEXT = 0x02,

            /// <summary>
            /// Don't use the native text output available on some platforms.
            /// </summary>
            NO_NATIVETEXT = 0x04,

            /// <summary>
            /// Grayscale output.
            /// </summary>
            GRAYSCALE = 0x08,

            /// <summary>
            /// Obsolete, has no effect, retained for compatibility.
            /// </summary>
            [Obsolete("Obsolete, has no effect, retained for compatibility.")]
            DEBUG_INFO = 0x80,

            /// <summary>
            /// Obsolete, has no effect, retained for compatibility.
            /// </summary>
            [Obsolete("Obsolete, has no effect, retained for compatibility.")]
            NO_CATCH = 0x100,

            /// <summary>
            /// Limit image cache size.
            /// </summary>
            RENDER_LIMITEDIMAGECACHE = 0x200,

            /// <summary>
            /// Always use halftone for image stretching.
            /// </summary>
            RENDER_FORCEHALFTONE = 0x400,

            /// <summary>
            /// Render for printing.
            /// </summary>
            PRINTING = 0x800,

            /// <summary>
            /// Set to disable anti-aliasing on text. This flag will also disable LCD optimization for text rendering.
            /// </summary>
            RENDER_NO_SMOOTHTEXT = 0x1000,

            /// <summary>
            /// Set to disable anti-aliasing on images.
            /// </summary>
            RENDER_NO_SMOOTHIMAGE = 0x2000,

            /// <summary>
            /// Set to disable anti-aliasing on paths.
            /// </summary>
            RENDER_NO_SMOOTHPATH = 0x4000,

            /// <summary>
            /// Set whether to render in a reverse Byte order, this flag is only used when rendering to a bitmap.
            /// </summary>
            REVERSE_BYTE_ORDER = 0x10,

            /// <summary>
            /// Set whether fill paths need to be stroked. This flag is only used when FPDF_COLORSCHEME is passed in, since with a single fill color for paths the boundaries of adjacent fill paths are less visible.
            /// </summary>
            CONVERT_FILL_TO_STROKE = 0x20
        }

        public enum FPDF_ERR : uint
        {
            /// <summary>
            /// No error.
            /// </summary>
            SUCCESS = 0,

            /// <summary>
            /// Unknown error.
            /// </summary>
            UNKNOWN = 1,

            /// <summary>
            /// File not found or could not be opened.
            /// </summary>
            FILE = 2,

            /// <summary>
            /// File not in PDF format or corrupted.
            /// </summary>
            FORMAT = 3,

            /// <summary>
            /// Password required or incorrect password.
            /// </summary>
            PASSWORD = 4,

            /// <summary>
            /// Unsupported security scheme.
            /// </summary>
            SECURITY = 5,

            /// <summary>
            /// Page not found or content error.
            /// </summary>
            PAGE = 6
        }

        [StructLayout(LayoutKind.Sequential)]
        public
#if NET6_0_OR_GREATER
            unsafe
#endif
            class FPDF_FILEACCESS
        {
            public uint m_FileLen;
#if NET6_0_OR_GREATER
            public delegate* unmanaged[Cdecl]<IntPtr, uint, IntPtr, uint, int> m_GetBlock;
#else
			public IntPtr m_GetBlock;
#endif
            public IntPtr m_Param;
        }
    }
}