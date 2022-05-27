using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using static PDFtoImage.Conversion;
using static PDFtoImage.Tests.TestUtils;

namespace Tests
{
    [TestClass]
    public class FormFillTests
    {
        [TestInitialize]
        public void Initialize()
        {
#if NET6_0_OR_GREATER
            if (!OperatingSystem.IsWindows() && !OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
                Assert.Inconclusive("This test must run on Windows, Linux or macOS.");
#endif
        }

        [TestMethod]
        [DataRow(null, DisplayName = "Default (no form fill)")]
        [DataRow(true, DisplayName = "Form fill")]
        [DataRow(false, DisplayName = "No form fill")]
        public void SaveWebpPageNumber(bool? withFormFill)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "hundesteuer-anmeldung.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", $"hundesteuer-anmeldung_{withFormFill ?? false}.webp"), FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();

            if (withFormFill == null)
                SaveWebp(outputStream, inputStream, dpi: 40);
            else
                SaveWebp(outputStream, inputStream, dpi: 40, withFormFill: withFormFill.Value);

            CompareStreams(expectedStream, outputStream);
        }

        [TestMethod]
        [DataRow(null, DisplayName = "Default (no form fill)")]
        [DataRow(true, DisplayName = "Form fill")]
        [DataRow(false, DisplayName = "No form fill")]
        public void SavePngPageNumber(bool? withFormFill)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "hundesteuer-anmeldung.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", $"hundesteuer-anmeldung_{withFormFill ?? false}.png"), FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();

            if (withFormFill == null)
                SavePng(outputStream, inputStream, dpi: 40);
            else
                SavePng(outputStream, inputStream, dpi: 40, withFormFill: withFormFill.Value);

            CompareStreams(expectedStream, outputStream);
        }

        [TestMethod]
        [DataRow(null, DisplayName = "Default (no form fill)")]
        [DataRow(true, DisplayName = "Form fill")]
        [DataRow(false, DisplayName = "No form fill")]
        public void SaveJpegPageNumber(bool? withFormFill)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "hundesteuer-anmeldung.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", $"hundesteuer-anmeldung_{withFormFill ?? false}.jpg"), FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();

            if (withFormFill == null)
                SaveJpeg(outputStream, inputStream, dpi: 40);
            else
                SaveJpeg(outputStream, inputStream, dpi: 40, withFormFill: withFormFill.Value);

            CompareStreams(expectedStream, outputStream);
        }
    }
}