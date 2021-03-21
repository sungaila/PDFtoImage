using System;
using System.Runtime.Serialization;

namespace PDFtoImage.PdfiumViewer
{
    internal class PdfException : Exception
    {
        public PdfError Error { get; private set; }

        public PdfException()
        {
        }

        public PdfException(PdfError error)
            : this(GetMessage(error))
        {
            Error = error;
        }

        private static string GetMessage(PdfError error)
        {
            return error switch
            {
                PdfError.Success => "No error",
                PdfError.CannotOpenFile => "File not found or could not be opened",
                PdfError.InvalidFormat => "File not in PDF format or corrupted",
                PdfError.PasswordProtected => "Password required or incorrect password",
                PdfError.UnsupportedSecurityScheme => "Unsupported security scheme",
                PdfError.PageNotFound => "Page not found or content error",
                _ => "Unknown error",
            };
        }

        public PdfException(string message)
            : base(message)
        {
        }

        public PdfException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected PdfException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}