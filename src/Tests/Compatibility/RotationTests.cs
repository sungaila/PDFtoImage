#pragma warning disable CS0618
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage;
using PDFtoImage.Tests;
using System;
using System.IO;
using static PDFtoImage.Compatibility.Conversion;
using static PDFtoImage.Tests.TestUtils;

namespace Tests.Compatibility
{
	[TestClass]
	public class RotationTests : TestBase
	{
		[TestMethod]
		[DataRow(null, DisplayName = "Default (no rotation)")]
		[DataRow(PdfRotation.Rotate0, DisplayName = "No rotation")]
		[DataRow(PdfRotation.Rotate90, DisplayName = "Rotated 90 degrees clockwise")]
		[DataRow(PdfRotation.Rotate180, DisplayName = "Rotated 180 degrees")]
		[DataRow(PdfRotation.Rotate270, DisplayName = "Rotated 90 degrees counter-clockwise")]
		public void SaveWebpPageNumber(PdfRotation? rotation)
		{
			var expectedPath = Path.Combine("Assets", "Expected", GetPlatformAsString(), "Rotation", $"hundesteuer-anmeldung_{Enum.GetName(typeof(PdfRotation), rotation ?? PdfRotation.Rotate0)}.webp");

			using var inputStream = GetInputStream(Path.Combine("Assets", "hundesteuer-anmeldung.pdf"));
			using var outputStream = CreateOutputStream(expectedPath);

			if (rotation == null)
				SaveWebp(outputStream, inputStream, dpi: 40);
			else
				SaveWebp(outputStream, inputStream, dpi: 40, rotation: rotation.Value);

			CompareStreams(expectedPath, outputStream);
		}

		[TestMethod]
		[DataRow(null, DisplayName = "Default (no rotation)")]
		[DataRow(PdfRotation.Rotate0, DisplayName = "No rotation")]
		[DataRow(PdfRotation.Rotate90, DisplayName = "Rotated 90 degrees clockwise")]
		[DataRow(PdfRotation.Rotate180, DisplayName = "Rotated 180 degrees")]
		[DataRow(PdfRotation.Rotate270, DisplayName = "Rotated 90 degrees counter-clockwise")]
		public void SavePngPageNumber(PdfRotation? rotation)
		{
			var expectedPath = Path.Combine("Assets", "Expected", GetPlatformAsString(), "Rotation", $"hundesteuer-anmeldung_{Enum.GetName(typeof(PdfRotation), rotation ?? PdfRotation.Rotate0)}.png");

			using var inputStream = GetInputStream(Path.Combine("Assets", "hundesteuer-anmeldung.pdf"));
			using var outputStream = CreateOutputStream(expectedPath);

			if (rotation == null)
				SavePng(outputStream, inputStream, dpi: 40);
			else
				SavePng(outputStream, inputStream, dpi: 40, rotation: rotation.Value);

			CompareStreams(expectedPath, outputStream);
		}

		[TestMethod]
		[DataRow(null, DisplayName = "Default (no rotation)")]
		[DataRow(PdfRotation.Rotate0, DisplayName = "No rotation")]
		[DataRow(PdfRotation.Rotate90, DisplayName = "Rotated 90 degrees clockwise")]
		[DataRow(PdfRotation.Rotate180, DisplayName = "Rotated 180 degrees")]
		[DataRow(PdfRotation.Rotate270, DisplayName = "Rotated 90 degrees counter-clockwise")]
		public void SaveJpegPageNumber(PdfRotation? rotation)
		{
			var expectedPath = Path.Combine("Assets", "Expected", GetPlatformAsString(), "Rotation", $"hundesteuer-anmeldung_{Enum.GetName(typeof(PdfRotation), rotation ?? PdfRotation.Rotate0)}.jpg");

			using var inputStream = GetInputStream(Path.Combine("Assets", "hundesteuer-anmeldung.pdf"));
			using var outputStream = CreateOutputStream(expectedPath);

			if (rotation == null)
				SaveJpeg(outputStream, inputStream, dpi: 40);
			else
				SaveJpeg(outputStream, inputStream, dpi: 40, rotation: rotation.Value);

			CompareStreams(expectedPath, outputStream);
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
			var expectedPath = Path.Combine("Assets", "Expected", GetPlatformAsString(), "Rotation", $"hundesteuer-anmeldung_{Enum.GetName(typeof(PdfRotation), rotation ?? PdfRotation.Rotate0)}_{width?.ToString() ?? "null"}x{height?.ToString() ?? "null"}_{withAspectRatio}.png");

			using var inputStream = GetInputStream(Path.Combine("Assets", "hundesteuer-anmeldung.pdf"));
			using var outputStream = CreateOutputStream(expectedPath);

			if (rotation == null)
				SavePng(outputStream, inputStream, dpi: 40, width: width, height: height, withAspectRatio: withAspectRatio);
			else
				SavePng(outputStream, inputStream, dpi: 40, width: width, height: height, withAspectRatio: withAspectRatio, rotation: rotation.Value);

			CompareStreams(expectedPath, outputStream);
		}
	}
}