using Microsoft.VisualBasic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Tests;
using System.IO;
using static PDFtoImage.Conversion;
using static PDFtoImage.Tests.TestUtils;

namespace Tests
{
	[TestClass]
	public class BatchingTests : TestBase
	{
		[TestMethod]
        [DataRow("hundesteuer-anmeldung.pdf")]
        public void WithoutAspectRatio(string fileName)
		{
			using var inputStream = GetInputStream(Path.Combine("..", "Assets", fileName));

			var test = ToImages(inputStream);
		}
	}
}