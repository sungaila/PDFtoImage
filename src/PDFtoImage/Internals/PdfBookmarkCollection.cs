using System.Collections.ObjectModel;

namespace PDFtoImage.Internals
{
	internal sealed class PdfBookmark
	{
		public string? Title { get; set; }
		public int PageIndex { get; set; }

		public PdfBookmarkCollection Children { get; }

		public PdfBookmark()
		{
            Children = [];
		}
	}

	internal class PdfBookmarkCollection : Collection<PdfBookmark>
	{
	}
}