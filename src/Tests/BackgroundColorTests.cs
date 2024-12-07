using Microsoft.VisualStudio.TestTools.UnitTesting;
using SkiaSharp;
using System.IO;
using static PDFtoImage.Conversion;
using static PDFtoImage.Tests.TestUtils;

namespace PDFtoImage.Tests
{
    [TestClass]
    public class BackgroundColorTests : TestBase
    {
        [TestMethod]
        [DataRow(null, DisplayName = "Default (White)")]
        [DataRow(0xFFFFFFFF, DisplayName = "White")]
        [DataRow((uint)0x64FFFFFF, DisplayName = "White (100 alpha)")]
        [DataRow(0xFFFF0000, DisplayName = "Red")]
        [DataRow((uint)0x64FF0000, DisplayName = "Red (100 alpha)")]
        [DataRow(0xFF00FF00, DisplayName = "Green")]
        [DataRow((uint)0x6400FF00, DisplayName = "Green (100 alpha)")]
        [DataRow(0xFF0000FF, DisplayName = "Blue")]
        [DataRow((uint)0x640000FF, DisplayName = "Blue (100 alpha)")]
        [DataRow(0xFFFFFF00, DisplayName = "Yellow")]
        [DataRow((uint)0x64FFFF00, DisplayName = "Yellow (100 alpha)")]
        [DataRow(0xFFFF00FF, DisplayName = "Magenta")]
        [DataRow((uint)0x64FF00FF, DisplayName = "Magenta (100 alpha)")]
        [DataRow(0xFF00FFFF, DisplayName = "Cyan")]
        [DataRow((uint)0x6400FFFF, DisplayName = "Cyan (100 alpha)")]
        [DataRow(0xFF000000, DisplayName = "Black")]
        [DataRow((uint)0x64000000, DisplayName = "Black (100 alpha)")]
        [DataRow((uint)0x00FFFFFF, DisplayName = "Transparent")]
        public void SaveJpegWithBackgroundColor(uint? backgroundColor)
        {
            var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), "BackgroundColor", $"hundesteuer-anmeldung_{GetFileName(backgroundColor ?? SKColors.White)}.jpg");

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "hundesteuer-anmeldung.pdf"));
            using var outputStream = CreateOutputStream(expectedPath);

            if (backgroundColor == null)
                SaveJpeg(outputStream, inputStream, options: new(Dpi: 40));
            else
                SaveJpeg(outputStream, inputStream, options: new(Dpi: 40, BackgroundColor: backgroundColor.Value));

            CompareStreams(expectedPath, outputStream);
        }

        [TestMethod]
        [DataRow(null, DisplayName = "Default (White)")]
        [DataRow(0xFFFFFFFF, DisplayName = "White")]
        [DataRow((uint)0x64FFFFFF, DisplayName = "White (100 alpha)")]
        [DataRow(0xFFFF0000, DisplayName = "Red")]
        [DataRow((uint)0x64FF0000, DisplayName = "Red (100 alpha)")]
        [DataRow(0xFF00FF00, DisplayName = "Green")]
        [DataRow((uint)0x6400FF00, DisplayName = "Green (100 alpha)")]
        [DataRow(0xFF0000FF, DisplayName = "Blue")]
        [DataRow((uint)0x640000FF, DisplayName = "Blue (100 alpha)")]
        [DataRow(0xFFFFFF00, DisplayName = "Yellow")]
        [DataRow((uint)0x64FFFF00, DisplayName = "Yellow (100 alpha)")]
        [DataRow(0xFFFF00FF, DisplayName = "Magenta")]
        [DataRow((uint)0x64FF00FF, DisplayName = "Magenta (100 alpha)")]
        [DataRow(0xFF00FFFF, DisplayName = "Cyan")]
        [DataRow((uint)0x6400FFFF, DisplayName = "Cyan (100 alpha)")]
        [DataRow(0xFF000000, DisplayName = "Black")]
        [DataRow((uint)0x64000000, DisplayName = "Black (100 alpha)")]
        [DataRow((uint)0x00FFFFFF, DisplayName = "Transparent")]
        public void SavePngWithBackgroundColor(uint? backgroundColor)
        {
            var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), "BackgroundColor", $"hundesteuer-anmeldung_{GetFileName(backgroundColor ?? SKColors.White)}.png");

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "hundesteuer-anmeldung.pdf"));
            using var outputStream = CreateOutputStream(expectedPath);

            if (backgroundColor == null)
                SavePng(outputStream, inputStream, options: new(Dpi: 40));
            else
                SavePng(outputStream, inputStream, options: new(Dpi: 40, BackgroundColor: backgroundColor.Value));

            CompareStreams(expectedPath, outputStream);
        }

        [TestMethod]
        [DataRow(null, DisplayName = "Default (White)")]
        [DataRow(0xFFFFFFFF, DisplayName = "White")]
        [DataRow((uint)0x64FFFFFF, DisplayName = "White (100 alpha)")]
        [DataRow(0xFFFF0000, DisplayName = "Red")]
        [DataRow((uint)0x64FF0000, DisplayName = "Red (100 alpha)")]
        [DataRow(0xFF00FF00, DisplayName = "Green")]
        [DataRow((uint)0x6400FF00, DisplayName = "Green (100 alpha)")]
        [DataRow(0xFF0000FF, DisplayName = "Blue")]
        [DataRow((uint)0x640000FF, DisplayName = "Blue (100 alpha)")]
        [DataRow(0xFFFFFF00, DisplayName = "Yellow")]
        [DataRow((uint)0x64FFFF00, DisplayName = "Yellow (100 alpha)")]
        [DataRow(0xFFFF00FF, DisplayName = "Magenta")]
        [DataRow((uint)0x64FF00FF, DisplayName = "Magenta (100 alpha)")]
        [DataRow(0xFF00FFFF, DisplayName = "Cyan")]
        [DataRow((uint)0x6400FFFF, DisplayName = "Cyan (100 alpha)")]
        [DataRow(0xFF000000, DisplayName = "Black")]
        [DataRow((uint)0x64000000, DisplayName = "Black (100 alpha)")]
        [DataRow((uint)0x00FFFFFF, DisplayName = "Transparent")]
        public void SaveWebpWithBackgroundColor(uint? backgroundColor)
        {
            var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), "BackgroundColor", $"hundesteuer-anmeldung_{GetFileName(backgroundColor ?? SKColors.White)}.webp");

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", "hundesteuer-anmeldung.pdf"));
            using var outputStream = CreateOutputStream(expectedPath);

            if (backgroundColor == null)
                SaveWebp(outputStream, inputStream, options: new(Dpi: 40));
            else
                SaveWebp(outputStream, inputStream, options: new(Dpi: 40, BackgroundColor: backgroundColor.Value));

            CompareStreams(expectedPath, outputStream);
        }

        private static string GetFileName(SKColor backgroundColor) => backgroundColor.ToString();
    }
}