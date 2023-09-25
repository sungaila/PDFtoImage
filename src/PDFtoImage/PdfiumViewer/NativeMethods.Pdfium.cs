using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PDFtoImage.PdfiumViewer
{
#pragma warning disable SYSLIB1054 // LibraryImportAttribute cannot substitute most of the following DllImportAttributes
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

		public static void FPDFDOC_ExitFormFillEnvironment(IntPtr hHandle)
		{
			lock (LockString)
			{
				Imports.FPDFDOC_ExitFormFillEnvironment(hHandle);
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

		public static IntPtr FPDFBitmap_CreateEx(int width, int height, int format, IntPtr first_scan, int stride)
		{
			lock (LockString)
			{
				return Imports.FPDFBitmap_CreateEx(width, height, format, first_scan, stride);
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

		#region Save / Edit Methods
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
		#endregion

		#region Custom Load/Save Logic
#if !NET6_0_OR_GREATER
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate int FPDF_GetBlockDelegate(IntPtr param, uint position, IntPtr buffer, uint size);

		private static readonly FPDF_GetBlockDelegate _getBlockDelegate = FPDF_GetBlock;
#endif

#if NET6_0_OR_GREATER
		[UnmanagedCallersOnly(CallConvs = new[] { typeof(CallConvCdecl) })]
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
		#endregion

		private const string _dllName =
#if NET6_0_OR_GREATER
			"libpdfium"
#else
			"pdfium"
#endif
			;

		private static class Imports
		{
			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern void FPDF_InitLibrary();

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern void FPDF_DestroyLibrary();

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
			public static extern IntPtr FPDF_LoadCustomDocument([MarshalAs(UnmanagedType.LPStruct)] FPDF_FILEACCESS access, string? password);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern void FPDF_CloseDocument(IntPtr document);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern int FPDF_GetPageCount(IntPtr document);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern uint FPDF_GetDocPermissions(IntPtr document);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern IntPtr FPDFDOC_InitFormFillEnvironment(IntPtr document, FPDF_FORMFILLINFO formInfo);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern void FPDF_SetFormFieldHighlightColor(IntPtr hHandle, int fieldType, uint color);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern void FPDF_SetFormFieldHighlightAlpha(IntPtr hHandle, byte alpha);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern void FORM_DoDocumentJSAction(IntPtr hHandle);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern void FORM_DoDocumentOpenAction(IntPtr hHandle);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern void FPDFDOC_ExitFormFillEnvironment(IntPtr hHandle);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern void FORM_DoDocumentAAction(IntPtr hHandle, FPDFDOC_AACTION aaType);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern IntPtr FPDF_LoadPage(IntPtr document, int page_index);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern IntPtr FPDFText_LoadPage(IntPtr page);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern void FORM_OnAfterLoadPage(IntPtr page, IntPtr _form);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern void FORM_DoPageAAction(IntPtr page, IntPtr _form, FPDFPAGE_AACTION fPDFPAGE_AACTION);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern double FPDF_GetPageWidth(IntPtr page);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern double FPDF_GetPageHeight(IntPtr page);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern void FORM_OnBeforeClosePage(IntPtr page, IntPtr _form);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern void FPDFText_ClosePage(IntPtr text_page);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern void FPDF_ClosePage(IntPtr page);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern void FPDF_RenderPageBitmap(IntPtr bitmapHandle, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, FPDF flags);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern int FPDF_GetPageSizeByIndex(IntPtr document, int page_index, out double width, out double height);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern IntPtr FPDFBitmap_CreateEx(int width, int height, int format, IntPtr first_scan, int stride);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern void FPDFBitmap_FillRect(IntPtr bitmapHandle, int left, int top, int width, int height, uint color);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern void FPDFBitmap_Destroy(IntPtr bitmapHandle);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern uint FPDFDest_GetDestPageIndex(IntPtr document, IntPtr dest);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern IntPtr FPDFBookmark_GetFirstChild(IntPtr document, IntPtr bookmark);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern IntPtr FPDFBookmark_GetNextSibling(IntPtr document, IntPtr bookmark);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern uint FPDFBookmark_GetTitle(IntPtr bookmark, byte[]? buffer, uint buflen);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern IntPtr FPDFBookmark_GetDest(IntPtr document, IntPtr bookmark);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern uint FPDF_GetLastError();

			#region Save/Edit APIs
			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern void FPDF_FFLDraw(IntPtr form, IntPtr bitmap, IntPtr page, int start_x, int start_y, int size_x, int size_y, int rotate, FPDF flags);

			[DllImport(_dllName, CallingConvention = CallingConvention.Cdecl)]
			public static extern void FPDF_RemoveFormFieldHighlight(IntPtr form);
			#endregion
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

			// XFA support i.e. version 2

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

		[Flags]
		public enum FPDF
		{
			ANNOT = 0x01,
			LCD_TEXT = 0x02,
			NO_NATIVETEXT = 0x04,
			GRAYSCALE = 0x08,
			DEBUG_INFO = 0x80,
			NO_CATCH = 0x100,
			RENDER_LIMITEDIMAGECACHE = 0x200,
			RENDER_FORCEHALFTONE = 0x400,
			PRINTING = 0x800,
			REVERSE_BYTE_ORDER = 0x10
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

		#region Save/Edit Structs and Flags
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
		#endregion
	}
}