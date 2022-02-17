using PDFtoImage.PdfiumViewer;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage
{
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveJpeg(string imageFilename, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Jpeg, pdfAsBase64String, password, page, dpi, width, height, withAnnotations, withFormFill);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveJpeg(Stream imageStream, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Jpeg, pdfAsBase64String, password, page, dpi, width, height, withAnnotations, withFormFill);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveJpeg(string imageFilename, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Jpeg, pdfAsByteArray, password, page, dpi, width, height, withAnnotations, withFormFill);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveJpeg(Stream imageStream, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Jpeg, pdfAsByteArray, password, page, dpi, width, height, withAnnotations, withFormFill);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveJpeg(string imageFilename, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Jpeg, pdfStream, password, page, dpi, width, height, withAnnotations, withFormFill);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveJpeg(Stream imageStream, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Jpeg, pdfStream, password, page, dpi, width, height, withAnnotations, withFormFill);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SavePng(string imageFilename, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Png, pdfAsBase64String, password, page, dpi, width, height, withAnnotations, withFormFill);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SavePng(Stream imageStream, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Png, pdfAsBase64String, password, page, dpi, width, height, withAnnotations, withFormFill);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SavePng(string imageFilename, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Png, pdfAsByteArray, password, page, dpi, width, height, withAnnotations, withFormFill);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SavePng(Stream imageStream, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Png, pdfAsByteArray, password, page, dpi, width, height, withAnnotations, withFormFill);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SavePng(string imageFilename, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Png, pdfStream, password, page, dpi, width, height, withAnnotations, withFormFill);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SavePng(Stream imageStream, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Png, pdfStream, password, page, dpi, width, height, withAnnotations, withFormFill);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveWebp(string imageFilename, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Webp, pdfAsBase64String, password, page, dpi, width, height, withAnnotations, withFormFill);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveWebp(Stream imageStream, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Webp, pdfAsBase64String, password, page, dpi, width, height, withAnnotations, withFormFill);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveWebp(string imageFilename, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Webp, pdfAsByteArray, password, page, dpi, width, height, withAnnotations, withFormFill);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveWebp(Stream imageStream, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Webp, pdfAsByteArray, password, page, dpi, width, height, withAnnotations, withFormFill);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveWebp(string imageFilename, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            SaveImpl(imageFilename, SKEncodedImageFormat.Webp, pdfStream, password, page, dpi, width, height, withAnnotations, withFormFill);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static void SaveWebp(Stream imageStream, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            SaveImpl(imageStream, SKEncodedImageFormat.Webp, pdfStream, password, page, dpi, width, height, withAnnotations, withFormFill);
        }
        #endregion

        #region Internal save impl
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        internal static void SaveImpl(string imageFilename, SKEncodedImageFormat format, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            if (imageFilename == null)
                throw new ArgumentNullException(nameof(imageFilename));

            using var fileStream = new FileStream(imageFilename, FileMode.Create, FileAccess.Write);
            SaveImpl(fileStream, format, pdfAsBase64String, password, page, dpi, width, height, withAnnotations, withFormFill);
        }

#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        internal static void SaveImpl(Stream imageStream, SKEncodedImageFormat format, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            if (imageStream == null)
                throw new ArgumentNullException(nameof(imageStream));

            ToImage(pdfAsBase64String, password, page, dpi, width, height, withAnnotations, withFormFill).Encode(imageStream, format, 100);
        }

#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        internal static void SaveImpl(string imageFilename, SKEncodedImageFormat format, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            if (imageFilename == null)
                throw new ArgumentNullException(nameof(imageFilename));

            using var fileStream = new FileStream(imageFilename, FileMode.Create, FileAccess.Write);
            SaveImpl(fileStream, format, pdfAsByteArray, password, page, dpi, width, height, withAnnotations, withFormFill);
        }

#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        internal static void SaveImpl(Stream imageStream, SKEncodedImageFormat format, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            if (imageStream == null)
                throw new ArgumentNullException(nameof(imageStream));

            ToImage(pdfAsByteArray, password, page, dpi, width, height, withAnnotations, withFormFill).Encode(imageStream, format, 100);
        }

#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        internal static void SaveImpl(string filename, SKEncodedImageFormat format, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            using var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            SaveImpl(fileStream, format, pdfStream, password, page, dpi, width, height, withAnnotations, withFormFill);
        }

#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        internal static void SaveImpl(Stream stream, SKEncodedImageFormat format, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            ToImage(pdfStream, password, page, dpi, width, height, withAnnotations, withFormFill).Encode(stream, format, 100);
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
        /// <returns>The converted PDF page as an image.</returns>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static SKBitmap ToImage(string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            return ToImage(Convert.FromBase64String(pdfAsBase64String), password, page, dpi, width, height, withAnnotations, withFormFill);
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
        /// <returns>The converted PDF page as an image.</returns>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static SKBitmap ToImage(byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            return ToImage(pdfStream, password, page, dpi, width, height, withAnnotations, withFormFill);
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
        /// <returns>The rendered PDF page as an image.</returns>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static SKBitmap ToImage(Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            if (page < 0)
                throw new ArgumentOutOfRangeException(nameof(page), "The page number must be 0 or greater.");

            // correct the width and height for the given dpi
            // but only if both width and height are not specified (so the original sizes are corrected)
            var renderFlags = PdfRenderFlags.ForPrinting;
            if (width == null && height == null)
                renderFlags |= PdfRenderFlags.CorrectFromDpi;

            if (withAnnotations)
                renderFlags |= PdfRenderFlags.Annotations;

            // Stream -> PdfiumViewer.PdfDocument
            using var pdfDocument = PdfDocument.Load(pdfStream, password);

            if (page >= pdfDocument.PageSizes.Count)
                throw new ArgumentOutOfRangeException(nameof(page), $"The page number {page} does not exist. Highest page number available is {pdfDocument.PageSizes.Count - 1}.");

            // PdfiumViewer.PdfDocument -> Image
            return pdfDocument.Render(page, width ?? (int)pdfDocument.PageSizes[page].Width, height ?? (int)pdfDocument.PageSizes[page].Height, dpi, dpi, renderFlags, withFormFill);
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
        /// <returns>The rendered PDF pages as images.</returns>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static IEnumerable<SKBitmap> ToImages(string pdfAsBase64String, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            foreach (var image in ToImages(Convert.FromBase64String(pdfAsBase64String), password, dpi, width, height, withAnnotations, withFormFill))
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
        /// <returns>The rendered PDF pages as images.</returns>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static IEnumerable<SKBitmap> ToImages(byte[] pdfAsByteArray, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            foreach (var image in ToImages(pdfStream, password, dpi, width, height, withAnnotations, withFormFill))
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
        /// <returns>The rendered PDF pages as images.</returns>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static IEnumerable<SKBitmap> ToImages(Stream pdfStream, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            // correct the width and height for the given dpi
            // but only if both width and height are not specified (so the original sizes are corrected)
            var renderFlags = PdfRenderFlags.ForPrinting;
            if (width == null && height == null)
                renderFlags |= PdfRenderFlags.CorrectFromDpi;

            if (withAnnotations)
                renderFlags |= PdfRenderFlags.Annotations;

            // Stream -> PdfiumViewer.PdfDocument
            using var pdfDocument = PdfDocument.Load(pdfStream, password);

            for (int i = 0; i < pdfDocument.PageSizes.Count; i++)
            {
                // PdfiumViewer.PdfDocument -> Image
                yield return pdfDocument.Render(i, width ?? (int)pdfDocument.PageSizes[i].Width, height ?? (int)pdfDocument.PageSizes[i].Height, dpi, dpi, renderFlags, withFormFill);
            }
        }
        #endregion

        #region ToImagesAsnyc
#if NETCOREAPP3_0_OR_GREATER
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
        /// <returns>The rendered PDF pages as images.</returns>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(string pdfAsBase64String, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            await foreach (var image in ToImagesAsync(Convert.FromBase64String(pdfAsBase64String), password, dpi, width, height, withAnnotations, withFormFill, cancellationToken))
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
        /// <returns>The rendered PDF pages as images.</returns>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(byte[] pdfAsByteArray, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            await foreach (var image in ToImagesAsync(pdfStream, password, dpi, width, height, withAnnotations, withFormFill, cancellationToken))
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
        /// <returns>The rendered PDF pages as images.</returns>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static async IAsyncEnumerable<SKBitmap> ToImagesAsync(Stream pdfStream, string? password = null, int dpi = 300, int? width = null, int? height = null, bool withAnnotations = false, bool withFormFill = false, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            // correct the width and height for the given dpi
            // but only if both width and height are not specified (so the original sizes are corrected)
            var renderFlags = PdfRenderFlags.ForPrinting;
            if (width == null && height == null)
                renderFlags |= PdfRenderFlags.CorrectFromDpi;

            if (withAnnotations)
                renderFlags |= PdfRenderFlags.Annotations;

            // Stream -> PdfiumViewer.PdfDocument
            using var pdfDocument = await Task.Run(() => PdfDocument.Load(pdfStream, password), cancellationToken);

            for (int i = 0; i < pdfDocument.PageSizes.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                // PdfiumViewer.PdfDocument -> Image
                yield return await Task.Run(() => pdfDocument.Render(i, width ?? (int)pdfDocument.PageSizes[i].Width, height ?? (int)pdfDocument.PageSizes[i].Height, dpi, dpi, renderFlags, withFormFill), cancellationToken);
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
#if NET5_0_OR_GREATER
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
#if NET5_0_OR_GREATER
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Android31.0")]
#endif
        public static int GetPageCount(Stream pdfStream, string? password = null)
        {
            if (pdfStream == null)
                throw new ArgumentNullException(nameof(pdfStream));

            using var pdfDocument = PdfDocument.Load(pdfStream, password);

            return pdfDocument.PageSizes.Count;
        }
        #endregion
    }
}