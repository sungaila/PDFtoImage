using System;

namespace PDFtoImage.PdfiumViewer
{
    internal class PdfiumResolveEventArgs : EventArgs
    {
        public string? PdfiumFileName { get; set; }
    }

    internal delegate void PdfiumResolveEventHandler(object? sender, PdfiumResolveEventArgs e);
}