namespace PDFtoImage.PdfiumViewer
{
    public enum PdfError
    {
        Success = (int)NativeMethods.FPDF_ERR.SUCCESS,
        Unknown = (int)NativeMethods.FPDF_ERR.UNKNOWN,
        CannotOpenFile = (int)NativeMethods.FPDF_ERR.FILE,
        InvalidFormat = (int)NativeMethods.FPDF_ERR.FORMAT,
        PasswordProtected = (int)NativeMethods.FPDF_ERR.PASSWORD,
        UnsupportedSecurityScheme = (int)NativeMethods.FPDF_ERR.SECURITY,
        PageNotFound = (int)NativeMethods.FPDF_ERR.PAGE
    }
}