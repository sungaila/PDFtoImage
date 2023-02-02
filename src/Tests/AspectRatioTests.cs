using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using static PDFtoImage.Conversion;
using static PDFtoImage.Tests.TestUtils;

namespace Tests
{
    [TestClass]
    public class AspectRatioTests
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
        [DataRow("hundesteuer-anmeldung.pdf")]
        [DataRow("hundesteuer-anmeldung.pdf", 100, null, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, null, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, null, null)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 100, null)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 1000, null)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 10000, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 100, 100, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, 1000, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, 10000, null)]
        [DataRow("hundesteuer-anmeldung.pdf", null, null, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", null, null, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", null, null, 600)]
        [DataRow("hundesteuer-anmeldung.pdf", 100, null, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", 100, null, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", 100, null, 1200)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, null, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, null, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, null, 1200)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, null, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, null, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, null, 1200)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 100, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 100, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 100, 1200)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 1000, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 1000, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 1000, 1200)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 10000, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 10000, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 10000, 1200)]
        [DataRow("hundesteuer-anmeldung.pdf", 100, 100, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", 100, 100, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", 100, 100, 1200)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, 1000, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, 1000, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, 1000, 1200)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, 10000, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, 10000, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, 10000, 1200)]

        [DataRow("SocialPreview.pdf")]
        [DataRow("SocialPreview.pdf", 100, null, null)]
        [DataRow("SocialPreview.pdf", 1000, null, null)]
        [DataRow("SocialPreview.pdf", 10000, null, null)]
        [DataRow("SocialPreview.pdf", null, 100, null)]
        [DataRow("SocialPreview.pdf", null, 1000, null)]
        [DataRow("SocialPreview.pdf", null, 10000, null)]
        [DataRow("SocialPreview.pdf", 100, 100, null)]
        [DataRow("SocialPreview.pdf", 1000, 1000, null)]
        [DataRow("SocialPreview.pdf", 10000, 10000, null)]
        [DataRow("SocialPreview.pdf", null, null, 96)]
        [DataRow("SocialPreview.pdf", null, null, 300)]
        [DataRow("SocialPreview.pdf", null, null, 600)]
        [DataRow("SocialPreview.pdf", 100, null, 96)]
        [DataRow("SocialPreview.pdf", 100, null, 300)]
        [DataRow("SocialPreview.pdf", 100, null, 1200)]
        [DataRow("SocialPreview.pdf", 1000, null, 96)]
        [DataRow("SocialPreview.pdf", 1000, null, 300)]
        [DataRow("SocialPreview.pdf", 1000, null, 1200)]
        [DataRow("SocialPreview.pdf", 10000, null, 96)]
        [DataRow("SocialPreview.pdf", 10000, null, 300)]
        [DataRow("SocialPreview.pdf", 10000, null, 1200)]
        [DataRow("SocialPreview.pdf", null, 100, 96)]
        [DataRow("SocialPreview.pdf", null, 100, 300)]
        [DataRow("SocialPreview.pdf", null, 100, 1200)]
        [DataRow("SocialPreview.pdf", null, 1000, 96)]
        [DataRow("SocialPreview.pdf", null, 1000, 300)]
        [DataRow("SocialPreview.pdf", null, 1000, 1200)]
        [DataRow("SocialPreview.pdf", null, 10000, 96)]
        [DataRow("SocialPreview.pdf", null, 10000, 300)]
        [DataRow("SocialPreview.pdf", null, 10000, 1200)]
        [DataRow("SocialPreview.pdf", 100, 100, 96)]
        [DataRow("SocialPreview.pdf", 100, 100, 300)]
        [DataRow("SocialPreview.pdf", 100, 100, 1200)]
        [DataRow("SocialPreview.pdf", 1000, 1000, 96)]
        [DataRow("SocialPreview.pdf", 1000, 1000, 300)]
        [DataRow("SocialPreview.pdf", 1000, 1000, 1200)]
        [DataRow("SocialPreview.pdf", 10000, 10000, 96)]
        [DataRow("SocialPreview.pdf", 10000, 10000, 300)]
        [DataRow("SocialPreview.pdf", 10000, 10000, 1200)]

        [DataRow("Wikimedia_Commons_web.pdf")]
        [DataRow("Wikimedia_Commons_web.pdf", 100, null, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, null, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, null, null)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 100, null)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 1000, null)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 10000, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 100, 100, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, 1000, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, 10000, null)]
        [DataRow("Wikimedia_Commons_web.pdf", null, null, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", null, null, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", null, null, 600)]
        [DataRow("Wikimedia_Commons_web.pdf", 100, null, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", 100, null, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", 100, null, 1200)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, null, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, null, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, null, 1200)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, null, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, null, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, null, 1200)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 100, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 100, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 100, 1200)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 1000, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 1000, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 1000, 1200)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 10000, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 10000, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 10000, 1200)]
        [DataRow("Wikimedia_Commons_web.pdf", 100, 100, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", 100, 100, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", 100, 100, 1200)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, 1000, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, 1000, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, 1000, 1200)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, 10000, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, 10000, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, 10000, 1200)]
        public void WithoutAspectRatio(string fileName, int? width = null, int? height = null, int? dpi = null)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", fileName), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", "AspectRatio", GetExpectedFilename(fileName, "jpg", width, height, dpi, false)), FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();

            if (dpi != null)
                SaveJpeg(outputStream, inputStream, width: width, height: height, withAnnotations: true, withFormFill: true, withAspectRatio: false, dpi: dpi.Value);
            else
                SaveJpeg(outputStream, inputStream, width: width, height: height, withAnnotations: true, withFormFill: true, withAspectRatio: false);

            CompareStreams(expectedStream, outputStream);
        }

        [TestMethod]
        [DataRow("hundesteuer-anmeldung.pdf")]
        [DataRow("hundesteuer-anmeldung.pdf", 100, null, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, null, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, null, null)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 100, null)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 1000, null)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 10000, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 100, 100, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, 1000, null)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, 10000, null)]
        [DataRow("hundesteuer-anmeldung.pdf", null, null, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", null, null, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", null, null, 600)]
        [DataRow("hundesteuer-anmeldung.pdf", 100, null, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", 100, null, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", 100, null, 1200)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, null, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, null, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, null, 1200)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, null, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, null, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, null, 1200)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 100, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 100, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 100, 1200)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 1000, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 1000, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 1000, 1200)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 10000, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 10000, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 10000, 1200)]
        [DataRow("hundesteuer-anmeldung.pdf", 100, 100, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", 100, 100, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", 100, 100, 1200)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, 1000, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, 1000, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, 1000, 1200)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, 10000, 96)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, 10000, 300)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, 10000, 1200)]

        [DataRow("SocialPreview.pdf")]
        [DataRow("SocialPreview.pdf", 100, null, null)]
        [DataRow("SocialPreview.pdf", 1000, null, null)]
        [DataRow("SocialPreview.pdf", 10000, null, null)]
        [DataRow("SocialPreview.pdf", null, 100, null)]
        [DataRow("SocialPreview.pdf", null, 1000, null)]
        [DataRow("SocialPreview.pdf", null, 10000, null)]
        [DataRow("SocialPreview.pdf", 100, 100, null)]
        [DataRow("SocialPreview.pdf", 1000, 1000, null)]
        [DataRow("SocialPreview.pdf", 10000, 10000, null)]
        [DataRow("SocialPreview.pdf", null, null, 96)]
        [DataRow("SocialPreview.pdf", null, null, 300)]
        [DataRow("SocialPreview.pdf", null, null, 600)]
        [DataRow("SocialPreview.pdf", 100, null, 96)]
        [DataRow("SocialPreview.pdf", 100, null, 300)]
        [DataRow("SocialPreview.pdf", 100, null, 1200)]
        [DataRow("SocialPreview.pdf", 1000, null, 96)]
        [DataRow("SocialPreview.pdf", 1000, null, 300)]
        [DataRow("SocialPreview.pdf", 1000, null, 1200)]
        [DataRow("SocialPreview.pdf", 10000, null, 96)]
        [DataRow("SocialPreview.pdf", 10000, null, 300)]
        [DataRow("SocialPreview.pdf", 10000, null, 1200)]
        [DataRow("SocialPreview.pdf", null, 100, 96)]
        [DataRow("SocialPreview.pdf", null, 100, 300)]
        [DataRow("SocialPreview.pdf", null, 100, 1200)]
        [DataRow("SocialPreview.pdf", null, 1000, 96)]
        [DataRow("SocialPreview.pdf", null, 1000, 300)]
        [DataRow("SocialPreview.pdf", null, 1000, 1200)]
        [DataRow("SocialPreview.pdf", null, 10000, 96)]
        [DataRow("SocialPreview.pdf", null, 10000, 300)]
        [DataRow("SocialPreview.pdf", null, 10000, 1200)]
        [DataRow("SocialPreview.pdf", 100, 100, 96)]
        [DataRow("SocialPreview.pdf", 100, 100, 300)]
        [DataRow("SocialPreview.pdf", 100, 100, 1200)]
        [DataRow("SocialPreview.pdf", 1000, 1000, 96)]
        [DataRow("SocialPreview.pdf", 1000, 1000, 300)]
        [DataRow("SocialPreview.pdf", 1000, 1000, 1200)]
        [DataRow("SocialPreview.pdf", 10000, 10000, 96)]
        [DataRow("SocialPreview.pdf", 10000, 10000, 300)]
        [DataRow("SocialPreview.pdf", 10000, 10000, 1200)]

        [DataRow("Wikimedia_Commons_web.pdf")]
        [DataRow("Wikimedia_Commons_web.pdf", 100, null, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, null, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, null, null)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 100, null)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 1000, null)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 10000, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 100, 100, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, 1000, null)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, 10000, null)]
        [DataRow("Wikimedia_Commons_web.pdf", null, null, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", null, null, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", null, null, 600)]
        [DataRow("Wikimedia_Commons_web.pdf", 100, null, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", 100, null, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", 100, null, 1200)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, null, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, null, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, null, 1200)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, null, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, null, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, null, 1200)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 100, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 100, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 100, 1200)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 1000, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 1000, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 1000, 1200)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 10000, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 10000, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 10000, 1200)]
        [DataRow("Wikimedia_Commons_web.pdf", 100, 100, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", 100, 100, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", 100, 100, 1200)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, 1000, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, 1000, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, 1000, 1200)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, 10000, 96)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, 10000, 300)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, 10000, 1200)]
        public void WithAspectRatio(string fileName, int? width = null, int? height = null, int? dpi = null)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", fileName), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", "AspectRatio", GetExpectedFilename(fileName, "jpg", width, height, dpi, true)), FileMode.Open, FileAccess.Read);
            using var outputStream = new MemoryStream();

            if (dpi != null)
                SaveJpeg(outputStream, inputStream, width: width, height: height, withAnnotations: true, withFormFill: true, withAspectRatio: true, dpi: dpi.Value);
            else
                SaveJpeg(outputStream, inputStream, width: width, height: height, withAnnotations: true, withFormFill: true, withAspectRatio: true);

            CompareStreams(expectedStream, outputStream);
        }

        [TestMethod]
        [DataRow("hundesteuer-anmeldung.pdf", 100, null, false)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, null, false)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, null, false)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 100, false)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 1000, false)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 10000, false)]
        [DataRow("hundesteuer-anmeldung.pdf", 100, 100, false)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, 1000, false)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, 10000, false)]
        [DataRow("hundesteuer-anmeldung.pdf", 100, null, true)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, null, true)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, null, true)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 100, true)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 1000, true)]
        [DataRow("hundesteuer-anmeldung.pdf", null, 10000, true)]
        [DataRow("hundesteuer-anmeldung.pdf", 100, 100, true)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, 1000, true)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, 10000, true)]

        [DataRow("SocialPreview.pdf", 100, null, false)]
        [DataRow("SocialPreview.pdf", 1000, null, false)]
        [DataRow("SocialPreview.pdf", 10000, null, false)]
        [DataRow("SocialPreview.pdf", null, 100, false)]
        [DataRow("SocialPreview.pdf", null, 1000, false)]
        [DataRow("SocialPreview.pdf", null, 10000, false)]
        [DataRow("SocialPreview.pdf", 100, 100, false)]
        [DataRow("SocialPreview.pdf", 1000, 1000, false)]
        [DataRow("SocialPreview.pdf", 10000, 10000, false)]
        [DataRow("SocialPreview.pdf", 100, null, true)]
        [DataRow("SocialPreview.pdf", 1000, null, true)]
        [DataRow("SocialPreview.pdf", 10000, null, true)]
        [DataRow("SocialPreview.pdf", null, 100, true)]
        [DataRow("SocialPreview.pdf", null, 1000, true)]
        [DataRow("SocialPreview.pdf", null, 10000, true)]
        [DataRow("SocialPreview.pdf", 100, 100, true)]
        [DataRow("SocialPreview.pdf", 1000, 1000, true)]
        [DataRow("SocialPreview.pdf", 10000, 10000, true)]

        [DataRow("Wikimedia_Commons_web.pdf", 100, null, false)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, null, false)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, null, false)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 100, false)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 1000, false)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 10000, false)]
        [DataRow("Wikimedia_Commons_web.pdf", 100, 100, false)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, 1000, false)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, 10000, false)]
        [DataRow("Wikimedia_Commons_web.pdf", 100, null, true)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, null, true)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, null, true)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 100, true)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 1000, true)]
        [DataRow("Wikimedia_Commons_web.pdf", null, 10000, true)]
        [DataRow("Wikimedia_Commons_web.pdf", 100, 100, true)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, 1000, true)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, 10000, true)]
        public void IgnoreDpi(string fileName, int? width = null, int? height = null, bool withAspectRatio = false)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", fileName), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", "AspectRatio", GetExpectedFilename(fileName, "jpg", width, height, 300, withAspectRatio)), FileMode.Open, FileAccess.Read);

            for (int i = 72; i < 600; i += 100)
            {
                using var outputStream = new MemoryStream();

                ToImage(inputStream, true, dpi: i, width: width, height: height, withAnnotations: true, withFormFill: true, withAspectRatio: withAspectRatio).Encode(outputStream, SkiaSharp.SKEncodedImageFormat.Jpeg, 100);
                CompareStreams(expectedStream, outputStream);
            }
        }

        [TestMethod]
        [DataRow("hundesteuer-anmeldung.pdf", 100, 100)]
        [DataRow("hundesteuer-anmeldung.pdf", 1000, 1000)]
        [DataRow("hundesteuer-anmeldung.pdf", 10000, 10000)]
        [DataRow("SocialPreview.pdf", 100, 100)]
        [DataRow("SocialPreview.pdf", 1000, 1000)]
        [DataRow("SocialPreview.pdf", 10000, 10000)]
        [DataRow("Wikimedia_Commons_web.pdf", 100, 100)]
        [DataRow("Wikimedia_Commons_web.pdf", 1000, 1000)]
        [DataRow("Wikimedia_Commons_web.pdf", 10000, 10000)]
        public void IgnoreAspectRatio(string fileName, int width, int height)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", fileName), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", "AspectRatio", GetExpectedFilename(fileName, "jpg", width, height, 300, true)), FileMode.Open, FileAccess.Read);
            using var outputStream1 = new MemoryStream();
            using var outputStream2 = new MemoryStream();

            ToImage(inputStream, true, width: width, height: height, withAnnotations: true, withFormFill: true, withAspectRatio: false).Encode(outputStream1, SkiaSharp.SKEncodedImageFormat.Jpeg, 100);
            ToImage(inputStream, true, width: width, height: height, withAnnotations: true, withFormFill: true, withAspectRatio: true).Encode(outputStream2, SkiaSharp.SKEncodedImageFormat.Jpeg, 100);

            CompareStreams(expectedStream, outputStream1);
            CompareStreams(expectedStream, outputStream2);
        }

        [TestMethod]
        [DataRow("hundesteuer-anmeldung.pdf", 96)]
        [DataRow("hundesteuer-anmeldung.pdf", 300)]
        [DataRow("hundesteuer-anmeldung.pdf", 600)]
        [DataRow("SocialPreview.pdf", 96)]
        [DataRow("SocialPreview.pdf", 300)]
        [DataRow("SocialPreview.pdf", 600)]
        [DataRow("Wikimedia_Commons_web.pdf", 96)]
        [DataRow("Wikimedia_Commons_web.pdf", 300)]
        [DataRow("Wikimedia_Commons_web.pdf", 600)]
        public void IgnoreAspectRatioWithDpi(string fileName, int dpi)
        {
            using var inputStream = new FileStream(Path.Combine("Assets", fileName), FileMode.Open, FileAccess.Read);
            using var expectedStream = new FileStream(Path.Combine("Assets", "Expected", "AspectRatio", GetExpectedFilename(fileName, "jpg", null, null, dpi, true)), FileMode.Open, FileAccess.Read);
            using var outputStream1 = new MemoryStream();
            using var outputStream2 = new MemoryStream();

            ToImage(inputStream, true, dpi: dpi, withAnnotations: true, withFormFill: true, withAspectRatio: false).Encode(outputStream1, SkiaSharp.SKEncodedImageFormat.Jpeg, 100);
            ToImage(inputStream, true, dpi: dpi, withAnnotations: true, withFormFill: true, withAspectRatio: true).Encode(outputStream2, SkiaSharp.SKEncodedImageFormat.Jpeg, 100);

            CompareStreams(expectedStream, outputStream1);
            CompareStreams(expectedStream, outputStream2);
        }

        private static string GetExpectedFilename(string fileName, string? fileExtension, int? width, int? height, int? dpi, bool withAspectRatio)
            => $"{fileName}_w={width?.ToString() ?? "null"}_h={height?.ToString() ?? "null"}_dpi={dpi?.ToString() ?? "null"}_aspectratio={withAspectRatio}{(fileExtension != null ? $".{fileExtension}" : string.Empty)}";
    }
}