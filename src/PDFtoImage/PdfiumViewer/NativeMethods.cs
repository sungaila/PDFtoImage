namespace PDFtoImage.PdfiumViewer
{
    internal static partial class NativeMethods
    {
        static NativeMethods()
        {
#if NETFRAMEWORK
            LibraryLoader.LoadLocalLibrary<PdfDocument>("pdfium");
#else
            // .NET (Core) and Xamarin resolve the pdfium lib on their own
#endif
        }
    }
}