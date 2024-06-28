using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage;
using PDFtoImage.Tests;
using System;
using System.Drawing;
using System.IO;
using static PDFtoImage.Conversion;
using static PDFtoImage.Tests.TestUtils;

namespace Tests
{
    [TestClass]
    public class TilingTests : TestBase
    {
        [TestMethod]
        [DataRow("hundesteuer-anmeldung.pdf", 300, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 300, false)]
        [DataRow("hundesteuer-anmeldung.pdf", 300, true)]
        [DataRow("SocialPreview.pdf", 300, null)]
        [DataRow("SocialPreview.pdf", 300, false)]
        [DataRow("SocialPreview.pdf", 300, true)]
        [DataRow("Wikimedia_Commons_web.pdf", 300, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 300, false)]
        [DataRow("Wikimedia_Commons_web.pdf", 300, true)]

        [DataRow("hundesteuer-anmeldung.pdf", 600, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 600, false)]
        [DataRow("hundesteuer-anmeldung.pdf", 600, true)]
        [DataRow("SocialPreview.pdf", 600, null)]
        [DataRow("SocialPreview.pdf", 600, false)]
        [DataRow("SocialPreview.pdf", 600, true)]
        [DataRow("Wikimedia_Commons_web.pdf", 600, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 600, false)]
        [DataRow("Wikimedia_Commons_web.pdf", 600, true)]

        [DataRow("hundesteuer-anmeldung.pdf", 1200, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 1200, false)]
        [DataRow("hundesteuer-anmeldung.pdf", 1200, true)]
        [DataRow("SocialPreview.pdf", 1200, null)]
        [DataRow("SocialPreview.pdf", 1200, false)]
        [DataRow("SocialPreview.pdf", 1200, true)]
        [DataRow("Wikimedia_Commons_web.pdf", 1200, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 1200, false)]
        [DataRow("Wikimedia_Commons_web.pdf", 1200, true)]
        public void WithDpi(string fileName, int dpi, bool? useTiling = null)
        {
            var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), "Tiling", useTiling == true ? "tiled" : "normal", GetExpectedFilename(fileName, "png", null, null, dpi, false, default, default));

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", fileName));
            using var outputStream = CreateOutputStream(expectedPath);

            if (useTiling != null)
                SavePng(outputStream, inputStream, options: new(Dpi: dpi, UseTiling: useTiling.Value, AntiAliasing: PdfAntiAliasing.None));
            else
                SavePng(outputStream, inputStream, options: new(Dpi: dpi, AntiAliasing: PdfAntiAliasing.None));

            CompareStreams(expectedPath, outputStream);
        }

        [TestMethod]
        [DataRow("hundesteuer-anmeldung.pdf", 1200, 1200, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 1200, 1200, false)]
        [DataRow("hundesteuer-anmeldung.pdf", 1200, 1200, true)]
        [DataRow("SocialPreview.pdf", 1200, 1200, null)]
        [DataRow("SocialPreview.pdf", 1200, 1200, false)]
        [DataRow("SocialPreview.pdf", 1200, 1200, true)]
        [DataRow("Wikimedia_Commons_web.pdf", 1200, 1200, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 1200, 1200, false)]
        [DataRow("Wikimedia_Commons_web.pdf", 1200, 1200, true)]

        [DataRow("hundesteuer-anmeldung.pdf", 4000, 4000, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 4000, 4000, false)]
        [DataRow("hundesteuer-anmeldung.pdf", 4000, 4000, true)]
        [DataRow("SocialPreview.pdf", 4000, 4000, null)]
        [DataRow("SocialPreview.pdf", 4000, 4000, false)]
        [DataRow("SocialPreview.pdf", 4000, 4000, true)]
        [DataRow("Wikimedia_Commons_web.pdf", 4000, 4000, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 4000, 4000, false)]
        [DataRow("Wikimedia_Commons_web.pdf", 4000, 4000, true)]

        [DataRow("hundesteuer-anmeldung.pdf", 6000, 6000, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 6000, 6000, false)]
        [DataRow("hundesteuer-anmeldung.pdf", 6000, 6000, true)]
        [DataRow("SocialPreview.pdf", 6000, 6000, null)]
        [DataRow("SocialPreview.pdf", 6000, 6000, false)]
        [DataRow("SocialPreview.pdf", 6000, 6000, true)]
        [DataRow("Wikimedia_Commons_web.pdf", 6000, 6000, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 6000, 6000, false)]
        [DataRow("Wikimedia_Commons_web.pdf", 6000, 6000, true)]
        public void WithWidthAndHeight(string fileName, int? width = null, int? height = null, bool? useTiling = null)
        {
            var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), "Tiling", useTiling == true ? "tiled" : "normal", GetExpectedFilename(fileName, "png", width, height, null, false, default, default));

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", fileName));
            using var outputStream = CreateOutputStream(expectedPath);

            if (useTiling != null)
                SavePng(outputStream, inputStream, options: new(Width: width, Height: height, UseTiling: useTiling.Value, AntiAliasing: PdfAntiAliasing.None));
            else
                SavePng(outputStream, inputStream, options: new(Width: width, Height: height, AntiAliasing: PdfAntiAliasing.None));

            CompareStreams(expectedPath, outputStream);
        }

        [TestMethod]
        [DataRow("hundesteuer-anmeldung.pdf", null, null)]
        [DataRow("hundesteuer-anmeldung.pdf", null, false)]
        [DataRow("hundesteuer-anmeldung.pdf", null, true)]
        [DataRow("SocialPreview.pdf", null, null)]
        [DataRow("SocialPreview.pdf", null, false)]
        [DataRow("SocialPreview.pdf", null, true)]
        [DataRow("Wikimedia_Commons_web.pdf", null, null)]
        [DataRow("Wikimedia_Commons_web.pdf", null, false)]
        [DataRow("Wikimedia_Commons_web.pdf", null, true)]

        [DataRow("hundesteuer-anmeldung.pdf", PdfRotation.Rotate0, null)]
        [DataRow("hundesteuer-anmeldung.pdf", PdfRotation.Rotate0, false)]
        [DataRow("hundesteuer-anmeldung.pdf", PdfRotation.Rotate0, true)]
        [DataRow("SocialPreview.pdf", PdfRotation.Rotate0, null)]
        [DataRow("SocialPreview.pdf", PdfRotation.Rotate0, false)]
        [DataRow("SocialPreview.pdf", PdfRotation.Rotate0, true)]
        [DataRow("Wikimedia_Commons_web.pdf", PdfRotation.Rotate0, null)]
        [DataRow("Wikimedia_Commons_web.pdf", PdfRotation.Rotate0, false)]
        [DataRow("Wikimedia_Commons_web.pdf", PdfRotation.Rotate0, true)]

        [DataRow("hundesteuer-anmeldung.pdf", PdfRotation.Rotate90, null)]
        [DataRow("hundesteuer-anmeldung.pdf", PdfRotation.Rotate90, false)]
        [DataRow("hundesteuer-anmeldung.pdf", PdfRotation.Rotate90, true)]
        [DataRow("SocialPreview.pdf", PdfRotation.Rotate90, null)]
        [DataRow("SocialPreview.pdf", PdfRotation.Rotate90, false)]
        [DataRow("SocialPreview.pdf", PdfRotation.Rotate90, true)]
        [DataRow("Wikimedia_Commons_web.pdf", PdfRotation.Rotate90, null)]
        [DataRow("Wikimedia_Commons_web.pdf", PdfRotation.Rotate90, false)]
        [DataRow("Wikimedia_Commons_web.pdf", PdfRotation.Rotate90, true)]

        [DataRow("hundesteuer-anmeldung.pdf", PdfRotation.Rotate180, null)]
        [DataRow("hundesteuer-anmeldung.pdf", PdfRotation.Rotate180, false)]
        [DataRow("hundesteuer-anmeldung.pdf", PdfRotation.Rotate180, true)]
        [DataRow("SocialPreview.pdf", PdfRotation.Rotate180, null)]
        [DataRow("SocialPreview.pdf", PdfRotation.Rotate180, false)]
        [DataRow("SocialPreview.pdf", PdfRotation.Rotate180, true)]
        [DataRow("Wikimedia_Commons_web.pdf", PdfRotation.Rotate180, null)]
        [DataRow("Wikimedia_Commons_web.pdf", PdfRotation.Rotate180, false)]
        [DataRow("Wikimedia_Commons_web.pdf", PdfRotation.Rotate180, true)]

        [DataRow("hundesteuer-anmeldung.pdf", PdfRotation.Rotate270, null)]
        [DataRow("hundesteuer-anmeldung.pdf", PdfRotation.Rotate270, false)]
        [DataRow("hundesteuer-anmeldung.pdf", PdfRotation.Rotate270, true)]
        [DataRow("SocialPreview.pdf", PdfRotation.Rotate270, null)]
        [DataRow("SocialPreview.pdf", PdfRotation.Rotate270, false)]
        [DataRow("SocialPreview.pdf", PdfRotation.Rotate270, true)]
        [DataRow("Wikimedia_Commons_web.pdf", PdfRotation.Rotate270, null)]
        [DataRow("Wikimedia_Commons_web.pdf", PdfRotation.Rotate270, false)]
        [DataRow("Wikimedia_Commons_web.pdf", PdfRotation.Rotate270, true)]
        public void WithRotation(string fileName, PdfRotation? rotation = null, bool? useTiling = null)
        {
            var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), "Tiling", useTiling == true ? "tiled" : "normal", GetExpectedFilename(fileName, "png", null, null, 600, false, rotation ?? default, default));

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", fileName));
            using var outputStream = CreateOutputStream(expectedPath);

            if (rotation != null)
                SavePng(outputStream, inputStream, options: new(Dpi: 600, Rotation: rotation.Value, UseTiling: useTiling == true, AntiAliasing: PdfAntiAliasing.None));
            else
                SavePng(outputStream, inputStream, options: new(Dpi: 600, UseTiling: useTiling == true, AntiAliasing: PdfAntiAliasing.None));

            CompareStreams(expectedPath, outputStream);
        }

        [TestMethod]
        [DataRow("hundesteuer-anmeldung.pdf", 0, 0, 500, 500, null, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 0, 0, 500, 500, null, false)]
        [DataRow("hundesteuer-anmeldung.pdf", 0, 0, 500, 500, null, true)]
        [DataRow("SocialPreview.pdf", 0, 0, 500, 500, null, null)]
        [DataRow("SocialPreview.pdf", 0, 0, 500, 500, null, false)]
        [DataRow("SocialPreview.pdf", 0, 0, 500, 500, null, true)]
        [DataRow("Wikimedia_Commons_web.pdf", 0, 0, 500, 500, null, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 0, 0, 500, 500, null, false)]
        [DataRow("Wikimedia_Commons_web.pdf", 0, 0, 500, 500, null, true)]

        [DataRow("hundesteuer-anmeldung.pdf", 0, 0, 500, 500, PdfRotation.Rotate0, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 0, 0, 500, 500, PdfRotation.Rotate0, false)]
        [DataRow("hundesteuer-anmeldung.pdf", 0, 0, 500, 500, PdfRotation.Rotate0, true)]
        [DataRow("SocialPreview.pdf", 0, 0, 500, 500, PdfRotation.Rotate0, null)]
        [DataRow("SocialPreview.pdf", 0, 0, 500, 500, PdfRotation.Rotate0, false)]
        [DataRow("SocialPreview.pdf", 0, 0, 500, 500, PdfRotation.Rotate0, true)]
        [DataRow("Wikimedia_Commons_web.pdf", 0, 0, 500, 500, PdfRotation.Rotate0, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 0, 0, 500, 500, PdfRotation.Rotate0, false)]
        [DataRow("Wikimedia_Commons_web.pdf", 0, 0, 500, 500, PdfRotation.Rotate0, true)]

        [DataRow("hundesteuer-anmeldung.pdf", 0, 0, 500, 500, PdfRotation.Rotate90, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 0, 0, 500, 500, PdfRotation.Rotate90, false)]
        [DataRow("hundesteuer-anmeldung.pdf", 0, 0, 500, 500, PdfRotation.Rotate90, true)]
        [DataRow("SocialPreview.pdf", 0, 0, 500, 500, PdfRotation.Rotate90, null)]
        [DataRow("SocialPreview.pdf", 0, 0, 500, 500, PdfRotation.Rotate90, false)]
        [DataRow("SocialPreview.pdf", 0, 0, 500, 500, PdfRotation.Rotate90, true)]
        [DataRow("Wikimedia_Commons_web.pdf", 0, 0, 500, 500, PdfRotation.Rotate90, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 0, 0, 500, 500, PdfRotation.Rotate90, false)]
        [DataRow("Wikimedia_Commons_web.pdf", 0, 0, 500, 500, PdfRotation.Rotate90, true)]

        [DataRow("hundesteuer-anmeldung.pdf", 0, 0, 500, 500, PdfRotation.Rotate180, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 0, 0, 500, 500, PdfRotation.Rotate180, false)]
        [DataRow("hundesteuer-anmeldung.pdf", 0, 0, 500, 500, PdfRotation.Rotate180, true)]
        [DataRow("SocialPreview.pdf", 0, 0, 500, 500, PdfRotation.Rotate180, null)]
        [DataRow("SocialPreview.pdf", 0, 0, 500, 500, PdfRotation.Rotate180, false)]
        [DataRow("SocialPreview.pdf", 0, 0, 500, 500, PdfRotation.Rotate180, true)]
        [DataRow("Wikimedia_Commons_web.pdf", 0, 0, 500, 500, PdfRotation.Rotate180, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 0, 0, 500, 500, PdfRotation.Rotate180, false)]
        [DataRow("Wikimedia_Commons_web.pdf", 0, 0, 500, 500, PdfRotation.Rotate180, true)]

        [DataRow("hundesteuer-anmeldung.pdf", 0, 0, 500, 500, PdfRotation.Rotate270, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 0, 0, 500, 500, PdfRotation.Rotate270, false)]
        [DataRow("hundesteuer-anmeldung.pdf", 0, 0, 500, 500, PdfRotation.Rotate270, true)]
        [DataRow("SocialPreview.pdf", 0, 0, 500, 500, PdfRotation.Rotate270, null)]
        [DataRow("SocialPreview.pdf", 0, 0, 500, 500, PdfRotation.Rotate270, false)]
        [DataRow("SocialPreview.pdf", 0, 0, 500, 500, PdfRotation.Rotate270, true)]
        [DataRow("Wikimedia_Commons_web.pdf", 0, 0, 500, 500, PdfRotation.Rotate270, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 0, 0, 500, 500, PdfRotation.Rotate270, false)]
        [DataRow("Wikimedia_Commons_web.pdf", 0, 0, 500, 500, PdfRotation.Rotate270, true)]

        [DataRow("hundesteuer-anmeldung.pdf", 200, 200, 700, 700, null, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 200, 200, 700, 700, null, false)]
        [DataRow("hundesteuer-anmeldung.pdf", 200, 200, 700, 700, null, true)]
        [DataRow("SocialPreview.pdf", 200, 200, 700, 700, null, null)]
        [DataRow("SocialPreview.pdf", 200, 200, 700, 700, null, false)]
        [DataRow("SocialPreview.pdf", 200, 200, 700, 700, null, true)]
        [DataRow("Wikimedia_Commons_web.pdf", 200, 200, 700, 700, null, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 200, 200, 700, 700, null, false)]
        [DataRow("Wikimedia_Commons_web.pdf", 200, 200, 700, 700, null, true)]

        [DataRow("hundesteuer-anmeldung.pdf", -200, -200, 700, 700, null, null)]
        [DataRow("hundesteuer-anmeldung.pdf", -200, -200, 700, 700, null, false)]
        [DataRow("hundesteuer-anmeldung.pdf", -200, -200, 700, 700, null, true)]
        [DataRow("SocialPreview.pdf", -200, -200, 700, 700, null, null)]
        [DataRow("SocialPreview.pdf", -200, -200, 700, 700, null, false)]
        [DataRow("SocialPreview.pdf", -200, -200, 700, 700, null, true)]
        [DataRow("Wikimedia_Commons_web.pdf", -200, -200, 700, 700, null, null)]
        [DataRow("Wikimedia_Commons_web.pdf", -200, -200, 700, 700, null, false)]
        [DataRow("Wikimedia_Commons_web.pdf", -200, -200, 700, 700, null, true)]
        public void WithBounds(string fileName, float x, float y, float width, float height, PdfRotation? rotation = null, bool? useTiling = null)
        {
            var bounds = new RectangleF(x, y, width, height);
            var expectedPath = Path.Combine("..", "Assets", "Expected", GetPlatformAsString(), "Tiling", useTiling == true ? "tiled" : "normal", GetExpectedFilename(fileName, "png", null, null, 600, false, rotation ?? default, bounds));

            using var inputStream = GetInputStream(Path.Combine("..", "Assets", fileName));
            using var outputStream = CreateOutputStream(expectedPath);

            if (rotation != null)
                SavePng(outputStream, inputStream, options: new(Dpi: 600, Bounds: bounds, Rotation: rotation.Value, UseTiling: useTiling == true, AntiAliasing: PdfAntiAliasing.None));
            else
                SavePng(outputStream, inputStream, options: new(Dpi: 600, Bounds: bounds, UseTiling: useTiling == true, AntiAliasing: PdfAntiAliasing.None));

            CompareStreams(expectedPath, outputStream);
        }

        private static string GetExpectedFilename(string fileName, string? fileExtension, int? width, int? height, int? dpi, bool withAspectRatio, PdfRotation rotation, RectangleF? bounds)
            => $"{fileName}_w={width?.ToString() ?? "null"}_h={height?.ToString() ?? "null"}_dpi={dpi?.ToString() ?? "null"}_aspectratio={withAspectRatio}_{Enum.GetName(typeof(PdfRotation), rotation)}{(bounds != null ? $"_{bounds.Value.X}-{bounds.Value.Y}-{bounds.Value.Width}-{bounds.Value.Height}" : string.Empty)}{(fileExtension != null ? $".{fileExtension}" : string.Empty)}";
    }
}