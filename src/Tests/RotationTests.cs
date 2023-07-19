using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage;
using System;
using System.IO;
using static PDFtoImage.Conversion;
using static PDFtoImage.Tests.TestUtils;

namespace Tests
{
    [TestClass]
    public class RotationTests
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
        [DataRow(null, DisplayName = "Default (no rotation)")]
        [DataRow(PdfRotation.Rotate0, DisplayName = "No rotation")]
        [DataRow(PdfRotation.Rotate90, DisplayName = "Rotated 90 degrees clockwise")]
        [DataRow(PdfRotation.Rotate180, DisplayName = "Rotated 180 degrees")]
        [DataRow(PdfRotation.Rotate270, DisplayName = "Rotated 90 degrees counter-clockwise")]
        public void SaveWebpPageNumber(PdfRotation? rotation)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "hundesteuer-anmeldung.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", "Rotation", $"hundesteuer-anmeldung_{Enum.GetName(typeof(PdfRotation), rotation ?? PdfRotation.Rotate0)}.webp"), FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();

            if (rotation == null)
                SaveWebp(outputStream, inputStream, dpi: 40);
            else
                SaveWebp(outputStream, inputStream, dpi: 40, rotation: rotation.Value);

            CompareStreams(expectedStream, outputStream);
        }

        [TestMethod]
        [DataRow(null, DisplayName = "Default (no rotation)")]
        [DataRow(PdfRotation.Rotate0, DisplayName = "No rotation")]
        [DataRow(PdfRotation.Rotate90, DisplayName = "Rotated 90 degrees clockwise")]
        [DataRow(PdfRotation.Rotate180, DisplayName = "Rotated 180 degrees")]
        [DataRow(PdfRotation.Rotate270, DisplayName = "Rotated 90 degrees counter-clockwise")]
        public void SavePngPageNumber(PdfRotation? rotation)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "hundesteuer-anmeldung.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", "Rotation", $"hundesteuer-anmeldung_{Enum.GetName(typeof(PdfRotation), rotation ?? PdfRotation.Rotate0)}.png"), FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();

            if (rotation == null)
                SavePng(outputStream, inputStream, dpi: 40);
            else
                SavePng(outputStream, inputStream, dpi: 40, rotation: rotation.Value);

            CompareStreams(expectedStream, outputStream);
        }

        [TestMethod]
        [DataRow(null, DisplayName = "Default (no rotation)")]
        [DataRow(PdfRotation.Rotate0, DisplayName = "No rotation")]
        [DataRow(PdfRotation.Rotate90, DisplayName = "Rotated 90 degrees clockwise")]
        [DataRow(PdfRotation.Rotate180, DisplayName = "Rotated 180 degrees")]
        [DataRow(PdfRotation.Rotate270, DisplayName = "Rotated 90 degrees counter-clockwise")]
        public void SaveJpegPageNumber(PdfRotation? rotation)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "hundesteuer-anmeldung.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", "Rotation", $"hundesteuer-anmeldung_{Enum.GetName(typeof(PdfRotation), rotation ?? PdfRotation.Rotate0)}.jpg"), FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();

            if (rotation == null)
                SaveJpeg(outputStream, inputStream, dpi: 40);
            else
                SaveJpeg(outputStream, inputStream, dpi: 40, rotation: rotation.Value);

            CompareStreams(expectedStream, outputStream);
        }

        [TestMethod]
        [DataRow(null, null, null, false)]
        [DataRow(PdfRotation.Rotate0, null, null, false)]
        [DataRow(PdfRotation.Rotate90, null, null, false)]
        [DataRow(PdfRotation.Rotate180, null, null, false)]
        [DataRow(PdfRotation.Rotate270, null, null, false)]
        [DataRow(null, null, null, true)]
        [DataRow(PdfRotation.Rotate0, null, null, true)]
        [DataRow(PdfRotation.Rotate90, null, null, true)]
        [DataRow(PdfRotation.Rotate180, null, null, true)]
        [DataRow(PdfRotation.Rotate270, null, null, true)]
        [DataRow(null, 200, null, false)]
        [DataRow(PdfRotation.Rotate0, 200, null, false)]
        [DataRow(PdfRotation.Rotate90, 200, null, false)]
        [DataRow(PdfRotation.Rotate180, 200, null, false)]
        [DataRow(PdfRotation.Rotate270, 200, null, false)]
        [DataRow(null, 200, null, true)]
        [DataRow(PdfRotation.Rotate0, 200, null, true)]
        [DataRow(PdfRotation.Rotate90, 200, null, true)]
        [DataRow(PdfRotation.Rotate180, 200, null, true)]
        [DataRow(PdfRotation.Rotate270, 200, null, true)]
        [DataRow(null, null, 200, false)]
        [DataRow(PdfRotation.Rotate0, null, 200, false)]
        [DataRow(PdfRotation.Rotate90, null, 200, false)]
        [DataRow(PdfRotation.Rotate180, null, 200, false)]
        [DataRow(PdfRotation.Rotate270, 200, null, false)]
        [DataRow(null, null, 200, true)]
        [DataRow(PdfRotation.Rotate0, null, 200, true)]
        [DataRow(PdfRotation.Rotate90, null, 200, true)]
        [DataRow(PdfRotation.Rotate180, null, 200, true)]
        [DataRow(PdfRotation.Rotate270, null, 200, true)]
        [DataRow(null, 200, 200, false)]
        [DataRow(PdfRotation.Rotate0, 200, 200, false)]
        [DataRow(PdfRotation.Rotate90, 200, 200, false)]
        [DataRow(PdfRotation.Rotate180, 200, 200, false)]
        [DataRow(PdfRotation.Rotate270, 200, 200, false)]
        [DataRow(null, 200, 200, true)]
        [DataRow(PdfRotation.Rotate0, 200, 200, true)]
        [DataRow(PdfRotation.Rotate90, 200, 200, true)]
        [DataRow(PdfRotation.Rotate180, 200, 200, true)]
        [DataRow(PdfRotation.Rotate270, 200, 200, true)]
        public void WithWidthHeightAspect(PdfRotation? rotation, int? width, int? height, bool withAspectRatio)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", "hundesteuer-anmeldung.pdf"), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", "Rotation", $"hundesteuer-anmeldung_{Enum.GetName(typeof(PdfRotation), rotation ?? PdfRotation.Rotate0)}_{width?.ToString() ?? "null"}x{height?.ToString() ?? "null"}_{withAspectRatio}.png"), FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();

            if (rotation == null)
                SavePng(outputStream, inputStream, dpi: 40, width: width, height: height, withAspectRatio: withAspectRatio);
            else
                SavePng(outputStream, inputStream, dpi: 40, width: width, height: height, withAspectRatio: withAspectRatio, rotation: rotation.Value);

            CompareStreams(expectedStream, outputStream);
        }
    }
}