using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage;
using System;
using System.IO;
using System.Text;
using static PDFtoImage.Conversion;
using static PDFtoImage.Tests.TestUtils;

namespace Tests
{
    [TestClass]
    public class AntiAliasingTests
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
        [DataRow(null, DisplayName = "Default (None)")]
        [DataRow(PdfAntiAliasing.None, DisplayName = "None")]
        [DataRow(PdfAntiAliasing.Text, DisplayName = "Text")]
        [DataRow(PdfAntiAliasing.Images, DisplayName = "Images")]
        [DataRow(PdfAntiAliasing.Paths, DisplayName = "Paths")]
        [DataRow(PdfAntiAliasing.Text | PdfAntiAliasing.Images, DisplayName = "Text | Images")]
        [DataRow(PdfAntiAliasing.Text | PdfAntiAliasing.Paths, DisplayName = "Text | Paths")]
        [DataRow(PdfAntiAliasing.Images | PdfAntiAliasing.Paths, DisplayName = "Images | Paths")]
        [DataRow(PdfAntiAliasing.Text | PdfAntiAliasing.Images | PdfAntiAliasing.Paths, DisplayName = "Text | Images | Paths")]
        [DataRow(PdfAntiAliasing.All, DisplayName = "All")]
        public void SaveJpegWithAntiAliasing(PdfAntiAliasing? antiAliasing)
        {
            var expectedPath = Path.Combine("Assets", "Expected", GetPlatformAsString(), "AntiAliasing", $"hundesteuer-anmeldung_{GetFileName(antiAliasing ?? PdfAntiAliasing.All)}.jpg");

            using var inputStream = GetInputStream(Path.Combine("Assets", "hundesteuer-anmeldung.pdf"));
            using var outputStream = CreateOutputStream(expectedPath);

            if (antiAliasing == null)
                SaveJpeg(outputStream, inputStream, dpi: 40);
            else
                SaveJpeg(outputStream, inputStream, dpi: 40, antiAliasing: antiAliasing.Value);

            CompareStreams(expectedPath, outputStream);
        }

        [TestMethod]
        [DataRow(null, DisplayName = "Default (None)")]
        [DataRow(PdfAntiAliasing.None, DisplayName = "None")]
        [DataRow(PdfAntiAliasing.Text, DisplayName = "Text")]
        [DataRow(PdfAntiAliasing.Images, DisplayName = "Images")]
        [DataRow(PdfAntiAliasing.Paths, DisplayName = "Paths")]
        [DataRow(PdfAntiAliasing.Text | PdfAntiAliasing.Images, DisplayName = "Text | Images")]
        [DataRow(PdfAntiAliasing.Text | PdfAntiAliasing.Paths, DisplayName = "Text | Paths")]
        [DataRow(PdfAntiAliasing.Images | PdfAntiAliasing.Paths, DisplayName = "Images | Paths")]
        [DataRow(PdfAntiAliasing.Text | PdfAntiAliasing.Images | PdfAntiAliasing.Paths, DisplayName = "Text | Images | Paths")]
        [DataRow(PdfAntiAliasing.All, DisplayName = "All")]
        public void SavePngWithAntiAliasing(PdfAntiAliasing? antiAliasing)
        {
            var expectedPath = Path.Combine("Assets", "Expected", GetPlatformAsString(), "AntiAliasing", $"hundesteuer-anmeldung_{GetFileName(antiAliasing ?? PdfAntiAliasing.All)}.png");

            using var inputStream = GetInputStream(Path.Combine("Assets", "hundesteuer-anmeldung.pdf"));
            using var outputStream = CreateOutputStream(expectedPath);

            if (antiAliasing == null)
                SavePng(outputStream, inputStream, dpi: 40);
            else
                SavePng(outputStream, inputStream, dpi: 40, antiAliasing: antiAliasing.Value);

            CompareStreams(expectedPath, outputStream);
        }

        [TestMethod]
        [DataRow(null, DisplayName = "Default (None)")]
        [DataRow(PdfAntiAliasing.None, DisplayName = "None")]
        [DataRow(PdfAntiAliasing.Text, DisplayName = "Text")]
        [DataRow(PdfAntiAliasing.Images, DisplayName = "Images")]
        [DataRow(PdfAntiAliasing.Paths, DisplayName = "Paths")]
        [DataRow(PdfAntiAliasing.Text | PdfAntiAliasing.Images, DisplayName = "Text | Images")]
        [DataRow(PdfAntiAliasing.Text | PdfAntiAliasing.Paths, DisplayName = "Text | Paths")]
        [DataRow(PdfAntiAliasing.Images | PdfAntiAliasing.Paths, DisplayName = "Images | Paths")]
        [DataRow(PdfAntiAliasing.Text | PdfAntiAliasing.Images | PdfAntiAliasing.Paths, DisplayName = "Text | Images | Paths")]
        [DataRow(PdfAntiAliasing.All, DisplayName = "All")]
        public void SaveWebpWithAntiAliasing(PdfAntiAliasing? antiAliasing)
        {
            var expectedPath = Path.Combine("Assets", "Expected", GetPlatformAsString(), "AntiAliasing", $"hundesteuer-anmeldung_{GetFileName(antiAliasing ?? PdfAntiAliasing.All)}.webp");

            using var inputStream = GetInputStream(Path.Combine("Assets", "hundesteuer-anmeldung.pdf"));
            using var outputStream = CreateOutputStream(expectedPath);

            if (antiAliasing == null)
                SaveWebp(outputStream, inputStream, dpi: 40);
            else
                SaveWebp(outputStream, inputStream, dpi: 40, antiAliasing: antiAliasing.Value);

            CompareStreams(expectedPath, outputStream);
        }

        private static string GetFileName(PdfAntiAliasing antiAliasing)
        {
            if (antiAliasing == PdfAntiAliasing.None)
                return "aliasing_none";

            var sb = new StringBuilder("aliasing");

            if (antiAliasing.HasFlag(PdfAntiAliasing.Text))
                sb.Append("_text");

            if (antiAliasing.HasFlag(PdfAntiAliasing.Images))
                sb.Append("_images");

            if (antiAliasing.HasFlag(PdfAntiAliasing.Paths))
                sb.Append("_paths");

            return sb.ToString();
        }
    }
}