using PDFtoImage.Internals;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage
{
#if NET8_0_OR_GREATER
#pragma warning disable CA1510 // Use ArgumentNullException throw helper
#endif
    /// <summary>
    /// Provides methods to render PDFs into images.
    /// </summary>
    public static class Conversion
    {
        #region SaveJpeg
        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveJpeg(string imageFilename, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Jpeg, pdfAsBase64String, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveJpeg(Stream imageStream, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Jpeg, pdfAsBase64String, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveJpeg(string imageFilename, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Jpeg, pdfAsByteArray, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveJpeg(Stream imageStream, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Jpeg, pdfAsByteArray, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveJpeg(string imageFilename, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Jpeg, pdfStream, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a JPEG.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveJpeg(Stream imageStream, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Jpeg, pdfStream, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }
        #endregion

        #region SavePng
        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SavePng(string imageFilename, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Png, pdfAsBase64String, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SavePng(Stream imageStream, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Png, pdfAsBase64String, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SavePng(string imageFilename, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Png, pdfAsByteArray, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SavePng(Stream imageStream, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Png, pdfAsByteArray, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SavePng(string imageFilename, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Png, pdfStream, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a PNG.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SavePng(Stream imageStream, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Png, pdfStream, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }
        #endregion

        #region SaveWebp
        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveWebp(string imageFilename, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Webp, pdfAsBase64String, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveWebp(Stream imageStream, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Webp, pdfAsBase64String, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveWebp(string imageFilename, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Webp, pdfAsByteArray, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveWebp(Stream imageStream, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Webp, pdfAsByteArray, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveWebp(string imageFilename, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Webp, pdfStream, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a bitmap.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveWebp(Stream imageStream, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Webp, pdfStream, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }
        #endregion

        #region Internal save impl
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        internal static void SaveImpl(string imageFilename, SKEncodedImageFormat format, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            if (imageFilename == null)
                throw new ArgumentNullException(nameof(imageFilename));

            using var fileStream = new FileStream(imageFilename, FileMode.Create, FileAccess.Write);
            SaveImpl(fileStream, format, pdfAsBase64String, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        internal static void SaveImpl(Stream imageStream, SKEncodedImageFormat format, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            if (imageStream == null)
                throw new ArgumentNullException(nameof(imageStream));

            using var bitmap = ToImage(pdfAsBase64String, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
            bitmap.Encode(imageStream, format, 100);
        }

#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        internal static void SaveImpl(string imageFilename, SKEncodedImageFormat format, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            if (imageFilename == null)
                throw new ArgumentNullException(nameof(imageFilename));

            using var fileStream = new FileStream(imageFilename, FileMode.Create, FileAccess.Write);
            SaveImpl(fileStream, format, pdfAsByteArray, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        internal static void SaveImpl(Stream imageStream, SKEncodedImageFormat format, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            if (imageStream == null)
                throw new ArgumentNullException(nameof(imageStream));

            using var bitmap = ToImage(pdfAsByteArray, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
            bitmap.Encode(imageStream, format, 100);
        }

#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        internal static void SaveImpl(string filename, SKEncodedImageFormat format, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            using var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            SaveImpl(fileStream, format, pdfStream, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        internal static void SaveImpl(Stream stream, SKEncodedImageFormat format, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            using var bitmap = ToImage(pdfStream, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
            bitmap.Encode(stream, format, 100);
        }
        #endregion

        #region ToImage
        /// <summary>
        /// Renders a single page of a given PDF into an image.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
        /// <returns>The converted PDF page as an image.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static SKBitmap ToImage(string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            return ToImage(Convert.FromBase64String(pdfAsBase64String), password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

        /// <summary>
        /// Renders a single page of a given PDF into an image.
        /// </summary>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
        /// <returns>The converted PDF page as an image.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static SKBitmap ToImage(byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            return ToImage(pdfStream, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

        /// <summary>
        /// Renders a single page of a given PDF into an image.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
        /// <returns>The rendered PDF page as an image.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static SKBitmap ToImage(Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            return ToImage(pdfStream, false, password, page, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

        /// <summary>
        /// Renders a single page of a given PDF into an image.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
        /// <returns>The rendered PDF page as an image.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static SKBitmap ToImage(Stream pdfStream, bool leaveOpen, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            if (page < 0)
                throw new ArgumentOutOfRangeException(nameof(page), "The page number must be 0 or greater.");

            // correct the width and height for the given dpi
            // but only if both width and height are not specified (so the original sizes are corrected)
            var correctFromDpi = width == null && height == null;

            NativeMethods.FPDF renderFlags = default;

            if (withAnnotations)
                renderFlags |= NativeMethods.FPDF.ANNOT;

            if (!antiAliasing.HasFlag(PdfAntiAliasing.Text))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHTEXT;
            if (!antiAliasing.HasFlag(PdfAntiAliasing.Images))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHIMAGE;
            if (!antiAliasing.HasFlag(PdfAntiAliasing.Paths))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHPATH;

            // Stream -> Internals.PdfDocument
            using var pdfDocument = PdfDocument.Load(pdfStream, password, !leaveOpen);

            if (page >= pdfDocument.PageSizes.Count)
                throw new ArgumentOutOfRangeException(nameof(page), $"The page number {page} does not exist. Highest page number available is {pdfDocument.PageSizes.Count - 1}.");

            var pageSize = pdfDocument.PageSizes[page];

            // correct aspect ratio if requested
            if (withAspectRatio)
                AdjustForAspectRatio(ref width, ref height, pageSize);

            // Internals.PdfDocument -> Image
            return pdfDocument.Render(page, width ?? (int)pageSize.Width, height ?? (int)pageSize.Height, dpi, dpi, rotation, renderFlags, withFormFill, correctFromDpi, backgroundColor ?? SKColors.White);
        }
        #endregion

        #region ToImages
        /// <summary>
        /// Renders all pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the all pages. Use <see langword="null"/> if the original width (per page) should be used.</param>
        /// <param name="height">The height of all pages. Use <see langword="null"/> if the original height (per page) should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
        /// <returns>The rendered PDF pages as images.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static IEnumerable<SKBitmap> ToImages(string pdfAsBase64String, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            foreach (var image in ToImages(Convert.FromBase64String(pdfAsBase64String), password, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor))
            {
                yield return image;
            }
        }

        /// <summary>
        /// Renders all pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the all pages. Use <see langword="null"/> if the original width (per page) should be used.</param>
        /// <param name="height">The height of all pages. Use <see langword="null"/> if the original height (per page) should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
        /// <returns>The rendered PDF pages as images.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static IEnumerable<SKBitmap> ToImages(byte[] pdfAsByteArray, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            foreach (var image in ToImages(pdfStream, password, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor))
            {
                yield return image;
            }
        }

        /// <summary>
        /// Renders all pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the all pages. Use <see langword="null"/> if the original width (per page) should be used.</param>
        /// <param name="height">The height of all pages. Use <see langword="null"/> if the original height (per page) should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
        /// <returns>The rendered PDF pages as images.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static IEnumerable<SKBitmap> ToImages(Stream pdfStream, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            return ToImages(pdfStream, false, password, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor);
        }

        /// <summary>
        /// Renders all pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the all pages. Use <see langword="null"/> if the original width (per page) should be used.</param>
        /// <param name="height">The height of all pages. Use <see langword="null"/> if the original height (per page) should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
        /// <returns>The rendered PDF pages as images.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static IEnumerable<SKBitmap> ToImages(Stream pdfStream, bool leaveOpen, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            // correct the width and height for the given dpi
            // but only if both width and height are not specified (so the original sizes are corrected)
            var correctFromDpi = width == null && height == null;

            NativeMethods.FPDF renderFlags = default;

            if (withAnnotations)
                renderFlags |= NativeMethods.FPDF.ANNOT;

            if (!antiAliasing.HasFlag(PdfAntiAliasing.Text))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHTEXT;
            if (!antiAliasing.HasFlag(PdfAntiAliasing.Images))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHIMAGE;
            if (!antiAliasing.HasFlag(PdfAntiAliasing.Paths))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHPATH;

            // Stream -> Internals.PdfDocument
            using var pdfDocument = PdfDocument.Load(pdfStream, password, !leaveOpen);

            for (int i = 0; i < pdfDocument.PageSizes.Count; i++)
            {
                var currentWidth = width;
                var currentHeight = height;
                var pageSize = pdfDocument.PageSizes[i];

                // correct aspect ratio if requested
                if (withAspectRatio)
                    AdjustForAspectRatio(ref currentWidth, ref currentHeight, pageSize);

                // Internals.PdfDocument -> Image
                yield return pdfDocument.Render(i, currentWidth ?? (int)pageSize.Width, currentHeight ?? (int)pageSize.Height, dpi, dpi, rotation, renderFlags, withFormFill, correctFromDpi, backgroundColor ?? SKColors.White);
            }
        }
        #endregion

        #region ToImagesAsnyc
#if NET6_0_OR_GREATER
        /// <summary>
        /// Renders all pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the conversion. Please note that an ongoing rendering cannot be cancelled (the next page will not be rendered though).</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the all pages. Use <see langword="null"/> if the original width (per page) should be used.</param>
        /// <param name="height">The height of all pages. Use <see langword="null"/> if the original height (per page) should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
        /// <returns>The rendered PDF pages as images.</returns>
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(string pdfAsBase64String, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            await foreach (var image in ToImagesAsync(Convert.FromBase64String(pdfAsBase64String), password, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor, cancellationToken))
            {
                yield return image;
            }
        }

        /// <summary>
        /// Renders all pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the conversion. Please note that an ongoing rendering cannot be cancelled (the next page will not be rendered though).</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the all pages. Use <see langword="null"/> if the original width (per page) should be used.</param>
        /// <param name="height">The height of all pages. Use <see langword="null"/> if the original height (per page) should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
        /// <returns>The rendered PDF pages as images.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(byte[] pdfAsByteArray, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            await foreach (var image in ToImagesAsync(pdfStream, password, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor, cancellationToken))
            {
                yield return image;
            }
        }

        /// <summary>
        /// Renders all pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the conversion. Please note that an ongoing rendering cannot be cancelled (the next page will not be rendered though).</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the all pages. Use <see langword="null"/> if the original width (per page) should be used.</param>
        /// <param name="height">The height of all pages. Use <see langword="null"/> if the original height (per page) should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
        /// <returns>The rendered PDF pages as images.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(Stream pdfStream, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var image in ToImagesAsync(pdfStream, false, password, dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor, cancellationToken))
            {
                yield return image;
            }
        }

        /// <summary>
        /// Renders all pages of a given PDF into images.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="cancellationToken">The cancellation token to cancel the conversion. Please note that an ongoing rendering cannot be cancelled (the next page will not be rendered though).</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the all pages. Use <see langword="null"/> if the original width (per page) should be used.</param>
        /// <param name="height">The height of all pages. Use <see langword="null"/> if the original height (per page) should be used.</param>
        /// <param name="withAnnotations">Specifies whether annotations be rendered.</param>
        /// <param name="withFormFill">Specifies whether form filling will be rendered.</param>
        /// <param name="withAspectRatio">Specifies that <paramref name="width"/> or <paramref name="height"/> should be adjusted for aspect ratio (either one must be <see langword="null"/>).</param>
        /// <param name="rotation">Specifies the rotation at 90 degree intervals.</param>
        /// <param name="antiAliasing">Specifies which parts of the PDF should be anti-aliasing for rendering.</param>
        /// <param name="backgroundColor">Specifies the background color. Defaults to <see cref="SKColors.White"/>.</param>
        /// <returns>The rendered PDF pages as images.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(Stream pdfStream, bool leaveOpen, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            // correct the width and height for the given dpi
            // but only if both width and height are not specified (so the original sizes are corrected)
            var correctFromDpi = width == null && height == null;

            NativeMethods.FPDF renderFlags = default;

            if (withAnnotations)
                renderFlags |= NativeMethods.FPDF.ANNOT;

            if (!antiAliasing.HasFlag(PdfAntiAliasing.Text))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHTEXT;
            if (!antiAliasing.HasFlag(PdfAntiAliasing.Images))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHIMAGE;
            if (!antiAliasing.HasFlag(PdfAntiAliasing.Paths))
                renderFlags |= NativeMethods.FPDF.RENDER_NO_SMOOTHPATH;

            // Stream -> Internals.PdfDocument
            using var pdfDocument = await Task.Run(() => PdfDocument.Load(pdfStream, password, !leaveOpen), cancellationToken);

            for (int i = 0; i < pdfDocument.PageSizes.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var currentWidth = width;
                var currentHeight = height;
                var pageSize = pdfDocument.PageSizes[i];

                // correct aspect ratio if requested
                if (withAspectRatio)
                    AdjustForAspectRatio(ref currentWidth, ref currentHeight, pageSize);

                // Internals.PdfDocument -> Image
                yield return await Task.Run(() => pdfDocument.Render(i, currentWidth ?? (int)pageSize.Width, currentHeight ?? (int)pageSize.Height, dpi, dpi, rotation, renderFlags, withFormFill, correctFromDpi, backgroundColor ?? SKColors.White), cancellationToken);
            }
        }
#endif
        #endregion

        #region GetPageCount
        /// <summary>
        /// Returns the page count of a given PDF.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page count of the given PDF.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static int GetPageCount(string pdfAsBase64String, string? password = null)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            return GetPageCount(Convert.FromBase64String(pdfAsBase64String), password);
        }

        /// <summary>
        /// Returns the page count of a given PDF.
        /// </summary>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page count of the given PDF.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static int GetPageCount(byte[] pdfAsByteArray, string? password = null)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            return GetPageCount(pdfStream, password);
        }

        /// <summary>
        /// Returns the page count of a given PDF.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page count of the given PDF.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static int GetPageCount(Stream pdfStream, string? password = null)
        {
            return GetPageCount(pdfStream, false, password);
        }

        /// <summary>
        /// Returns the page count of a given PDF.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page count of the given PDF.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static int GetPageCount(Stream pdfStream, bool leaveOpen, string? password = null)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            using var pdfDocument = PdfDocument.Load(pdfStream, password, !leaveOpen);

            return pdfDocument.PageSizes.Count;
        }
        #endregion

        #region GetPageSize
        /// <summary>
        /// Returns the PDF page size for a given page number.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="page">The specific page to query the size for.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page size containing width and height.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static SizeF GetPageSize(string pdfAsBase64String, int page, string? password = null)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            return GetPageSize(Convert.FromBase64String(pdfAsBase64String), page, password);
        }

        /// <summary>
        /// Returns the PDF page size for a given page number.
        /// </summary>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="page">The specific page to query the size for.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page size containing width and height.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static SizeF GetPageSize(byte[] pdfAsByteArray, int page, string? password = null)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            return GetPageSize(pdfStream, page, password);
        }

        /// <summary>
        /// Returns the PDF page size for a given page number.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="page">The specific page to query the size for.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page size containing width and height.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static SizeF GetPageSize(Stream pdfStream, int page, string? password = null)
        {
            return GetPageSize(pdfStream, false, page, password);
        }

        /// <summary>
        /// Returns the PDF page size for a given page number.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="page">The specific page to query the size for.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page size containing width and height.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static SizeF GetPageSize(Stream pdfStream, bool leaveOpen, int page, string? password = null)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            if (page < 0)
                throw new ArgumentOutOfRangeException(nameof(page), "The page number must be 0 or greater.");

            using var pdfDocument = PdfDocument.Load(pdfStream, password, !leaveOpen);

            return pdfDocument.PageSizes[page];
        }
        #endregion

        #region GetPageSizes
        /// <summary>
        /// Returns the sizes of all PDF pages.
        /// </summary>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page sizes containing width and height.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static IList<SizeF> GetPageSizes(string pdfAsBase64String, string? password = null)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            return GetPageSizes(Convert.FromBase64String(pdfAsBase64String), password);
        }

        /// <summary>
        /// Returns the sizes of all PDF pages.
        /// </summary>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page sizes containing width and height.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static IList<SizeF> GetPageSizes(byte[] pdfAsByteArray, string? password = null)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            return GetPageSizes(pdfStream, password);
        }

        /// <summary>
        /// Returns the sizes of all PDF pages.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page sizes containing width and height.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static IList<SizeF> GetPageSizes(Stream pdfStream, string? password = null)
        {
            return GetPageSizes(pdfStream, false, password);
        }

        /// <summary>
        /// Returns the sizes of all PDF pages.
        /// </summary>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="leaveOpen"><see langword="true"/> to leave the <paramref name="pdfStream"/> open after the PDF document is loaded; otherwise, <see langword="false"/>.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <returns>The page sizes containing width and height.</returns>
#if NET6_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static IList<SizeF> GetPageSizes(Stream pdfStream, bool leaveOpen, string? password = null)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            using var pdfDocument = PdfDocument.Load(pdfStream, password, !leaveOpen);

            return pdfDocument.PageSizes.ToList().AsReadOnly();
        }
        #endregion

        private static void AdjustForAspectRatio(ref int? width, ref int? height, SizeF pageSize)
        {
            if (width == null && height != null)
            {
                width = (int)Math.Round((pageSize.Width / pageSize.Height) * height.Value);
            }
            else if (width != null && height == null)
            {
                height = (int)Math.Round((pageSize.Height / pageSize.Width) * width.Value);
            }
        }
    }
}