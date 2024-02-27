using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;

namespace PDFtoImage.Compatibility
{
    /// <summary>
    /// Provides methods to render PDFs into images. Used for backward compatibility.
    /// </summary>
#if NET6_0_OR_GREATER
    [SupportedOSPlatform("Windows")]
    [SupportedOSPlatform("Linux")]
    [SupportedOSPlatform("macOS")]
    [SupportedOSPlatform("Android31.0")]
#endif
    [Obsolete("This class is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class Conversion
    {
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SaveJpeg(
            string imageFilename,
            string pdfAsBase64String,
            string? password = null,
            int page = 0,
            int dpi = 300,
            int? width = null,
            int? height = null,
            bool withAnnotations = false,
            bool withFormFill = false,
            bool withAspectRatio = false,
            PdfRotation rotation = PdfRotation.Rotate0,
            PdfAntiAliasing antiAliasing = PdfAntiAliasing.All,
            SKColor? backgroundColor = null)
        {
            PDFtoImage.Conversion.SaveImpl(imageFilename, SKEncodedImageFormat.Jpeg, pdfAsBase64String, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SaveJpeg(
            Stream imageStream,
            string pdfAsBase64String,
            string? password = null,
            int page = 0,
            int dpi = 300,
            int? width = null,
            int? height = null,
            bool withAnnotations = false,
            bool withFormFill = false,
            bool withAspectRatio = false,
            PdfRotation rotation = PdfRotation.Rotate0,
            PdfAntiAliasing antiAliasing = PdfAntiAliasing.All,
            SKColor? backgroundColor = null)
        {
            PDFtoImage.Conversion.SaveImpl(imageStream, SKEncodedImageFormat.Jpeg, pdfAsBase64String, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SaveJpeg(
            string imageFilename,
            byte[] pdfAsByteArray,
            string? password = null,
            int page = 0,
            int dpi = 300,
            int? width = null,
            int? height = null,
            bool withAnnotations = false,
            bool withFormFill = false,
            bool withAspectRatio = false,
            PdfRotation rotation = PdfRotation.Rotate0,
            PdfAntiAliasing antiAliasing = PdfAntiAliasing.All,
            SKColor? backgroundColor = null)
        {
            PDFtoImage.Conversion.SaveImpl(imageFilename, SKEncodedImageFormat.Jpeg, pdfAsByteArray, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SaveJpeg(
            Stream imageStream,
            byte[] pdfAsByteArray,
            string? password = null,
            int page = 0,
            int dpi = 300,
            int? width = null,
            int? height = null,
            bool withAnnotations = false,
            bool withFormFill = false,
            bool withAspectRatio = false,
            PdfRotation rotation = PdfRotation.Rotate0,
            PdfAntiAliasing antiAliasing = PdfAntiAliasing.All,
            SKColor? backgroundColor = null)
        {
            PDFtoImage.Conversion.SaveImpl(imageStream, SKEncodedImageFormat.Jpeg, pdfAsByteArray, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SaveJpeg(
            string imageFilename,
            Stream pdfStream,
            string? password = null,
            int page = 0,
            int dpi = 300,
            int? width = null,
            int? height = null,
            bool withAnnotations = false,
            bool withFormFill = false,
            bool withAspectRatio = false,
            PdfRotation rotation = PdfRotation.Rotate0,
            PdfAntiAliasing antiAliasing = PdfAntiAliasing.All,
            SKColor? backgroundColor = null)
        {
            PDFtoImage.Conversion.SaveImpl(imageFilename, SKEncodedImageFormat.Jpeg, pdfStream, false, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SaveJpeg(
            Stream imageStream,
            Stream pdfStream,
            string? password = null,
            int page = 0,
            int dpi = 300,
            int? width = null,
            int? height = null,
            bool withAnnotations = false,
            bool withFormFill = false,
            bool withAspectRatio = false,
            PdfRotation rotation = PdfRotation.Rotate0,
            PdfAntiAliasing antiAliasing = PdfAntiAliasing.All,
            SKColor? backgroundColor = null)
        {
            PDFtoImage.Conversion.SaveImpl(imageStream, SKEncodedImageFormat.Jpeg, pdfStream, false, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
        }

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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SavePng(string imageFilename, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            PDFtoImage.Conversion.SaveImpl(imageFilename, SKEncodedImageFormat.Png, pdfAsBase64String, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SavePng(Stream imageStream, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            PDFtoImage.Conversion.SaveImpl(imageStream, SKEncodedImageFormat.Png, pdfAsBase64String, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SavePng(string imageFilename, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            PDFtoImage.Conversion.SaveImpl(imageFilename, SKEncodedImageFormat.Png, pdfAsByteArray, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SavePng(Stream imageStream, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            PDFtoImage.Conversion.SaveImpl(imageStream, SKEncodedImageFormat.Png, pdfAsByteArray, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SavePng(string imageFilename, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            PDFtoImage.Conversion.SaveImpl(imageFilename, SKEncodedImageFormat.Png, pdfStream, false, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SavePng(Stream imageStream, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            PDFtoImage.Conversion.SaveImpl(imageStream, SKEncodedImageFormat.Png, pdfStream, false, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
        }

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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SaveWebp(string imageFilename, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            PDFtoImage.Conversion.SaveImpl(imageFilename, SKEncodedImageFormat.Webp, pdfAsBase64String, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SaveWebp(Stream imageStream, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            PDFtoImage.Conversion.SaveImpl(imageStream, SKEncodedImageFormat.Webp, pdfAsBase64String, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SaveWebp(string imageFilename, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            PDFtoImage.Conversion.SaveImpl(imageFilename, SKEncodedImageFormat.Webp, pdfAsByteArray, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SaveWebp(Stream imageStream, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            PDFtoImage.Conversion.SaveImpl(imageStream, SKEncodedImageFormat.Webp, pdfAsByteArray, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SaveWebp(string imageFilename, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            PDFtoImage.Conversion.SaveImpl(imageFilename, SKEncodedImageFormat.Webp, pdfStream, false, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void SaveWebp(Stream imageStream, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            PDFtoImage.Conversion.SaveImpl(imageStream, SKEncodedImageFormat.Webp, pdfStream, false, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
        }

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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static SKBitmap ToImage(string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            return PDFtoImage.Conversion.ToImage(pdfAsBase64String, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static SKBitmap ToImage(byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            return PDFtoImage.Conversion.ToImage(pdfAsByteArray, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static SKBitmap ToImage(Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            return PDFtoImage.Conversion.ToImage(pdfStream, false, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static SKBitmap ToImage(Stream pdfStream, bool leaveOpen, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            return PDFtoImage.Conversion.ToImage(pdfStream, leaveOpen, password, page, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
        }

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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEnumerable<SKBitmap> ToImages(string pdfAsBase64String, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            return PDFtoImage.Conversion.ToImages(pdfAsBase64String, password, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEnumerable<SKBitmap> ToImages(byte[] pdfAsByteArray, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            return PDFtoImage.Conversion.ToImages(pdfAsByteArray, password, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEnumerable<SKBitmap> ToImages(Stream pdfStream, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            return PDFtoImage.Conversion.ToImages(pdfStream, false, password, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IEnumerable<SKBitmap> ToImages(Stream pdfStream, bool leaveOpen, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null)
        {
            return PDFtoImage.Conversion.ToImages(pdfStream, leaveOpen, password, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor));
        }

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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(string pdfAsBase64String, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var image in PDFtoImage.Conversion.ToImagesAsync(pdfAsBase64String, password, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor), cancellationToken))
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
#endif
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(byte[] pdfAsByteArray, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var image in PDFtoImage.Conversion.ToImagesAsync(pdfAsByteArray, password, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor), cancellationToken))
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
#endif
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(Stream pdfStream, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var image in PDFtoImage.Conversion.ToImagesAsync(pdfStream, false, password, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor), cancellationToken))
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
        [Obsolete("This method is for backward compatibility. Use PDFtoImage.Conversion instead.", false)]
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(Stream pdfStream, bool leaveOpen, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, bool withAspectRatio = false, PdfRotation rotation = PdfRotation.Rotate0, PdfAntiAliasing antiAliasing = PdfAntiAliasing.All, SKColor? backgroundColor = null, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var image in PDFtoImage.Conversion.ToImagesAsync(pdfStream, leaveOpen, password, new(dpi, width, height, withAnnotations, withFormFill, withAspectRatio, rotation, antiAliasing, backgroundColor), cancellationToken))
            {
                yield return image;
            }
        }
#endif
    }
}