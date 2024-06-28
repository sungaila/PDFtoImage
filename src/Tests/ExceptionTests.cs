using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Exceptions;
using PDFtoImage.Tests;
using System;
using System.IO;
using static PDFtoImage.Conversion;
using static PDFtoImage.Tests.TestUtils;

namespace Tests
{
	[TestClass]
	public class ExceptionTests : TestBase
	{
		[TestMethod]
		public void ThrowsInvalidFormat()
		{
			using var inputStream = GetInputStream(Path.Combine("..", "Assets", "DummyImage.png"));
			Assert.ThrowsException<PdfInvalidFormatException>(() => GetPageCount(inputStream));
		}

		[TestMethod]
		[DataRow("hundesteuer-anmeldung.pdf")]
		[DataRow("SocialPreview.pdf")]
		[DataRow("Wikimedia_Commons_web.pdf")]
		public void ThrowsPageNotFound(string inputFile)
		{
			using var inputStream = GetInputStream(Path.Combine("..", "Assets", inputFile));
			Assert.ThrowsException<ArgumentOutOfRangeException>(() => ToImage(inputStream, page: 80085));
		}
	}
}