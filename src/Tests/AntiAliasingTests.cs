using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using static PDFtoImage.Conversion;
using static PDFtoImage.Tests.TestUtils;

namespace PDFtoImage.Tests
{
#pragma warning disable IDE0079
#pragma warning disable CA2263
#pragma warning restore IDE0079
    [TestClass]
    public class AntiAliasingTests : TestBase
    {
#pragma warning disable IDE0079
#pragma warning disable MSTEST0042
#pragma warning restore IDE0079
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
            var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), "AntiAliasing", $"hundesteuer-anmeldung_{GetFileName(antiAliasing ?? PdfAntiAliasing.All)}.jpg");

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "hundesteuer-anmeldung.pdf"));
            using var outputStream = CreateOutputStream(expectedPath);

            if (antiAliasing == null)
                SaveJpeg(outputStream, inputStream, options: new(Dpi: 40));
            else
                SaveJpeg(outputStream, inputStream, options: new(Dpi: 40, AntiAliasing: antiAliasing.Value));

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
            var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), "AntiAliasing", $"hundesteuer-anmeldung_{GetFileName(antiAliasing ?? PdfAntiAliasing.All)}.png");

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "hundesteuer-anmeldung.pdf"));
            using var outputStream = CreateOutputStream(expectedPath);

            if (antiAliasing == null)
                SavePng(outputStream, inputStream, options: new(Dpi: 40));
            else
                SavePng(outputStream, inputStream, options: new(Dpi: 40, AntiAliasing: antiAliasing.Value));

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
            var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), "AntiAliasing", $"hundesteuer-anmeldung_{GetFileName(antiAliasing ?? PdfAntiAliasing.All)}.webp");

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "hundesteuer-anmeldung.pdf"));
            using var outputStream = CreateOutputStream(expectedPath);

            if (antiAliasing == null)
                SaveWebp(outputStream, inputStream, options: new(Dpi: 40));
            else
                SaveWebp(outputStream, inputStream, options: new(Dpi: 40, AntiAliasing: antiAliasing.Value));

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