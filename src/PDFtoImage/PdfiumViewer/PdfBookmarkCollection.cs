using System.Collections.ObjectModel;

namespace PDFtoImage.PdfiumViewer
{
    internal class PdfBookmark
    {
        public string? Title { get; set; }
        public int PageIndex { get; set; }

        public PdfBookmarkCollection Children { get; }

        public PdfBookmark()
        {
            Children = new PdfBookmarkCollection();
        }
    }

    internal class PdfBookmarkCollection : Collection<PdfBookmark>
    {
    }
}