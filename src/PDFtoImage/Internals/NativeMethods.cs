using System;
using System.Runtime.InteropServices;

namespace PDFtoImage.Internals
{
    internal static partial class NativeMethods
    {
        // Interned strings are cached over AppDomains. This means that when we
        // lock on this string, we actually lock over AppDomain's. The Pdfium
        // library is not thread safe, and this way of locking
        // guarantees that we don't access the Pdfium library from different
        // threads, even when there are multiple AppDomain's in play.
        private static readonly string LockString = string.Intern("e362349b-001d-4cb2-bf55-a71606a3e36f");

        public static void InitLibrary()
        {
            lock (LockString)
            {
                Imports.FPDF_InitLibrary();
            }
        }

        public static void DestroyLibrary()
        {
            lock (LockString)
            {
                Imports.FPDF_DestroyLibrary();
            }
        }

        public static void CloseDocument(IntPtr document)
        {
            lock (LockString)
            {
                Imports.FPDF_CloseDocument(document);
            }
        }

        public static int GetPageCount(IntPtr document)
        {
            lock (LockString)
            {
                return Imports.FPDF_GetPageCount(document);
            }
        }

        public static IntPtr Doc_InitFormFillEnvironment(IntPtr document, IntPtr formInfo)
        {
            lock (LockString)
            {
                return Imports.FPDFDOC_InitFormFillEnvironment(document, formInfo);
            }
        }

        public static void Doc_ExitFormFillEnvironment(IntPtr handle)
        {
            lock (LockString)
            {
                Imports.FPDFDOC_ExitFormFillEnvironment(handle);
            }
        }

        public static void SetFormFieldHighlightAlpha(IntPtr hHandle, byte alpha)
        {
            lock (LockString)
            {
                Imports.FPDF_SetFormFieldHighlightAlpha(hHandle, alpha);
            }
        }

        public static IntPtr LoadPage(IntPtr document, int page_index)
        {
            lock (LockString)
            {
                return Imports.FPDF_LoadPage(document, page_index);
            }
        }

        public static IntPtr Text_LoadPage(IntPtr page)
        {
            lock (LockString)
            {
                return Imports.FPDFText_LoadPage(page);
            }
        }

        public static void OnAfterLoadPage(IntPtr page, IntPtr _form)
        {
            lock (LockString)
            {
                Imports.FORM_OnAfterLoadPage(page, _form);
            }
        }

        public static double GetPageWidth(IntPtr page)
        {
            lock (LockString)
            {
                return Imports.FPDF_GetPageWidth(page);
            }
        }

        public static double GetPageHeight(IntPtr page)
        {
            lock (LockString)
            {
                return Imports.FPDF_GetPageHeight(page);
            }
        }

        public static void Form_OnBeforeClosePage(IntPtr page, IntPtr _form)
        {
            lock (LockString)
            {
                Imports.FORM_OnBeforeClosePage(page, _form);
            }
        }

        public static void Text_ClosePage(IntPtr text_page)
        {
            lock (LockString)
            {
                Imports.FPDFText_ClosePage(text_page);
            }
        }

        public static void ClosePage(IntPtr page)
        {
            lock (LockString)
            {
                Imports.FPDF_ClosePage(page);
            }
        }

        public static void RenderPageBitmap(IntPtr bitmap, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, FPDFRenderFlags flags)
        {
            lock (LockString)
            {
                Imports.FPDF_RenderPageBitmap(bitmap, page, start_x, start_y, size_x, size_y, rotate, (int)flags);
            }
        }

        public static IntPtr Bitmap_CreateEx(int width, int height, FPDFBitmap format, IntPtr first_scan, int stride)
        {
            lock (LockString)
            {
                return Imports.FPDFBitmap_CreateEx(width, height, (int)format, first_scan, stride);
            }
        }

        public static void Bitmap_Destroy(IntPtr bitmapHandle)
        {
            lock (LockString)
            {
                Imports.FPDFBitmap_Destroy(bitmapHandle);
            }
        }

        public static void FFLDraw(IntPtr form, IntPtr bitmap, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, FPDFRenderFlags flags)
        {
            lock (LockString)
            {
                Imports.FPDF_FFLDraw(form, bitmap, page, start_x, start_y, size_x, size_y, rotate, (int)flags);
            }
        }

        public static void RemoveFormFieldHighlight(IntPtr form)
        {
            lock (LockString)
            {
                Imports.FPDF_RemoveFormFieldHighlight(form);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public readonly struct FPDF_FORMFILLINFO(int version)
        {
            private readonly int version = version;

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

            private readonly int xfa_disabled;

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

            private readonly IntPtr FFI_OnFocusChange;

            private readonly IntPtr FFI_DoURIActionWithKeyboardModifier;
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
        public enum FPDFRenderFlags : int
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
    }
}