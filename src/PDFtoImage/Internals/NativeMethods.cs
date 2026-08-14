using System;
using System.IO;
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

                if (_downloadHintsPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_downloadHintsPtr);
                    _downloadHintsPtr = IntPtr.Zero;
                }

                if (_fileAvailPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_fileAvailPtr);
                    _fileAvailPtr = IntPtr.Zero;
                }
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

        // These callback structs carry no per-document state. Keep one unmanaged instance of
        // each for the PDFium library lifetime so every FPDF_AVAIL receives a stable callback-
        // structure address. Lazy initialization and cleanup both run while LockString is held.
        private static IntPtr _fileAvailPtr;

        private static IntPtr _downloadHintsPtr;

        /// <summary>
        /// Creates an availability provider over a .NET Stream.
        /// </summary>
        /// <param name="input">The input Stream. Don't dispose prior to destroying the provider.</param>
        /// <param name="id">The id the stream is registered under.</param>
        /// <param name="fileAccessState">Native FPDF_FILEACCESS storage retained for the provider lifetime. Pass to <see cref="Avail_Destroy"/> after closing the document.</param>
        /// <returns>An IntPtr to the FPDF_AVAIL object.</returns>
        public static IntPtr Avail_Create(Stream input, int id, out IntPtr fileAccessState)
        {
            fileAccessState = CreateAvailFileAccessState(input, id);

            try
            {
                IntPtr avail;

                lock (LockString)
                {
                    avail = Imports.FPDFAvail_Create(GetFileAvailPointer(), fileAccessState);
                }

                if (avail == IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(fileAccessState);
                    fileAccessState = IntPtr.Zero;
                }

                return avail;
            }
            catch
            {
                Marshal.FreeHGlobal(fileAccessState);
                fileAccessState = IntPtr.Zero;
                throw;
            }
        }

        public static IntPtr Avail_GetDocument(IntPtr avail, string? password)
        {
            lock (LockString)
            {
                return Avail_GetDocumentCore(avail, password);
            }
        }

        private static IntPtr GetFileAvailPointer()
        {
            if (_fileAvailPtr == IntPtr.Zero)
            {
                var fileAvail = new FX_FILEAVAIL(1, GetIsDataAvailCallbackPointer());
                _fileAvailPtr = AllocateAvailabilityStruct(fileAvail);
            }

            return _fileAvailPtr;
        }

        private static IntPtr GetDownloadHintsPointer()
        {
            if (_downloadHintsPtr == IntPtr.Zero)
            {
                var hints = new FX_DOWNLOADHINTS(1, GetAddSegmentCallbackPointer());
                _downloadHintsPtr = AllocateAvailabilityStruct(hints);
            }

            return _downloadHintsPtr;
        }

        private static IntPtr AllocateAvailabilityStruct<T>(T value) where T : struct
        {
            var ptr = Marshal.AllocHGlobal(Marshal.SizeOf<T>());

            try
            {
                Marshal.StructureToPtr(value, ptr, false);
                return ptr;
            }
            catch
            {
                Marshal.FreeHGlobal(ptr);
                throw;
            }
        }

        // Call only after the FPDF_DOCUMENT created from this provider has been closed. Destroy
        // the provider before releasing the FPDF_FILEACCESS storage passed to FPDFAvail_Create().
        // Both arguments tolerate IntPtr.Zero.
        public static void Avail_Destroy(IntPtr avail, IntPtr fileAccessState)
        {
            try
            {
                if (avail != IntPtr.Zero)
                {
                    lock (LockString)
                    {
                        Imports.FPDFAvail_Destroy(avail);
                    }
                }
            }
            finally
            {
                if (fileAccessState != IntPtr.Zero)
                    Marshal.FreeHGlobal(fileAccessState);
            }
        }

        public static FPDF_AVAILABILITY Avail_IsDocAvail(IntPtr avail)
        {
            lock (LockString)
            {
                return (FPDF_AVAILABILITY)Imports.FPDFAvail_IsDocAvail(avail, GetDownloadHintsPointer());
            }
        }

        public static FPDF_AVAILABILITY Avail_IsPageAvail(IntPtr avail, int page_index)
        {
            lock (LockString)
            {
                return (FPDF_AVAILABILITY)Imports.FPDFAvail_IsPageAvail(avail, page_index, GetDownloadHintsPointer());
            }
        }

        public static FPDF_FORM_AVAILABILITY Avail_IsFormAvail(IntPtr avail)
        {
            lock (LockString)
            {
                return (FPDF_FORM_AVAILABILITY)Imports.FPDFAvail_IsFormAvail(avail, GetDownloadHintsPointer());
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

        public static IntPtr LoadPage(IntPtr document, int page_index)
        {
            lock (LockString)
            {
                return Imports.FPDF_LoadPage(document, page_index);
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
        private readonly struct FX_FILEAVAIL(int version, IntPtr m_IsDataAvail)
        {
            private readonly int version = version;
            private readonly IntPtr m_IsDataAvail = m_IsDataAvail;
        }

        [StructLayout(LayoutKind.Sequential)]
        private readonly struct FX_DOWNLOADHINTS(int version, IntPtr m_AddSegment)
        {
            private readonly int version = version;
            private readonly IntPtr m_AddSegment = m_AddSegment;
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

        public enum FPDF_AVAILABILITY : int
        {
            /// <summary>
            /// A common error is returned. Data availability unknown.
            /// </summary>
            PDF_DATA_ERROR = -1,

            /// <summary>
            /// Data not yet available.
            /// </summary>
            PDF_DATA_NOTAVAIL = 0,

            /// <summary>
            /// Data available.
            /// </summary>
            PDF_DATA_AVAIL = 1
        }

        public enum FPDF_FORM_AVAILABILITY : int
        {
            /// <summary>
            /// A common eror, in general incorrect parameters.
            /// </summary>
            PDF_FORM_ERROR = -1,

            /// <summary>
            /// Data not available.
            /// </summary>
            PDF_FORM_NOTAVAIL = 0,

            /// <summary>
            /// Data available.
            /// </summary>
            PDF_FORM_AVAIL = 1,

            /// <summary>
            /// No form data.
            /// </summary>
            PDF_FORM_NOTEXIST = 2
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