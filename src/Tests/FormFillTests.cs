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
            var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), $"hundesteuer-anmeldung_{withFormFill ?? false}.webp");

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "hundesteuer-anmeldung.pdf"));
            using var outputStream = CreateOutputStream(expectedPath);

            if (withFormFill == null)
                SaveWebp(outputStream, inputStream, options: new(Dpi: 40));
            else
                SaveWebp(outputStream, inputStream, options: new(Dpi: 40, WithFormFill: withFormFill.Value));

            CompareStreams(expectedPath, outputStream);
        }

        [TestMethod]
        [DataRow(null, DisplayName = "Default (no form fill)")]
        [DataRow(true, DisplayName = "Form fill")]
        [DataRow(false, DisplayName = "No form fill")]
        public void SavePngPageNumber(bool? withFormFill)
        {
            var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), $"hundesteuer-anmeldung_{withFormFill ?? false}.png");

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "hundesteuer-anmeldung.pdf"));
            using var outputStream = CreateOutputStream(expectedPath);

            if (withFormFill == null)
                SavePng(outputStream, inputStream, options: new(Dpi: 40));
            else
                SavePng(outputStream, inputStream, options: new(Dpi: 40, WithFormFill: withFormFill.Value));

            CompareStreams(expectedPath, outputStream);
        }

        [TestMethod]
        [DataRow(null, DisplayName = "Default (no form fill)")]
        [DataRow(true, DisplayName = "Form fill")]
        [DataRow(false, DisplayName = "No form fill")]
        public void SaveJpegPageNumber(bool? withFormFill)
        {
            var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), $"hundesteuer-anmeldung_{withFormFill ?? false}.jpg");

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "hundesteuer-anmeldung.pdf"));
            using var outputStream = CreateOutputStream(expectedPath);

            if (withFormFill == null)
                SaveJpeg(outputStream, inputStream, options: new(Dpi: 40));
            else
                SaveJpeg(outputStream, inputStream, options: new(Dpi: 40, WithFormFill: withFormFill.Value));

            CompareStreams(expectedPath, outputStream);
        }
    }
}