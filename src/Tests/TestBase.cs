using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Runtime.InteropServices;

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

            SaveOutputInGeneratedFolder = TestContext!.Properties.TryGetValue("SaveOutputInGeneratedFolder", out var value) && value != null && bool.TryParse(value.ToString(), out var parsed) && parsed;

            if (HandleTestQuirks(TestContext) is string message)
                Assert.Inconclusive(message);
        }

        private static string? HandleTestQuirks(TestContext testContext)
        {
            if (testContext.FullyQualifiedTestClassName == typeof(TilingTests).FullName && testContext.TestName == nameof(TilingTests.WithRotation))
            {
                if (testContext.TestData?.Length == 3 &&
                    testContext.TestData[0]?.Equals("SocialPreview.pdf") == true &&
                    testContext.TestData[1]?.Equals(PdfRotation.Rotate90) == true &&
                    testContext.TestData[2]?.Equals(true) == true)
                {
                    if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        return "This test run with these parameters is not supported due to differences in output under Windows on ARM.";
                    }
                }
            }

            if (testContext.FullyQualifiedTestClassName == typeof(TilingTests).FullName && testContext.TestName == nameof(TilingTests.WithRotation))
            {
                if (testContext.TestData?.Length == 3 &&
                    testContext.TestData[0]?.Equals("SocialPreview.pdf") == true &&
                    testContext.TestData[1]?.Equals(PdfRotation.Rotate270) == true &&
                    testContext.TestData[2]?.Equals(true) == true)
                {
                    if (RuntimeInformation.ProcessArchitecture == Architecture.Arm64 && RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    {
                        return "This test run with these parameters is not supported due to differences in output under Windows on ARM.";
                    }
                }
            }

            return null;
        }
    }
}