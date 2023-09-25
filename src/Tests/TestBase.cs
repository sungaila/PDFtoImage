using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace PDFtoImage.Tests
{
	public abstract class TestBase
	{
		public TestContext? TestContext { get; set; }

		public static bool SaveOutputInGeneratedFolder { get; private set; }

		[TestInitialize]
		public void Initialize()
		{
#if NET6_0_OR_GREATER
            if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
                Assert.Inconclusive("This test must run on Windows, Linux or macOS.");
#endif

			SaveOutputInGeneratedFolder = bool.Parse(TestContext?.Properties["SaveOutputInGeneratedFolder"]?.ToString() ?? false.ToString());
		}
	}
}