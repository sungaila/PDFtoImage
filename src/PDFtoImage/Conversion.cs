using PDFtoImage.PdfiumViewer;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.Versioning;

namespace PDFtoImage
{
    /// <summary>
    /// Provides methods to render PDFs into images.
    /// </summary>
    public static class Conversion
    {
        #region SaveTiff
        /// <summary>
        /// Renders a single page of a given PDF and saves it as a TIFF.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveTiff(string imageFilename, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageFilename, ImageFormat.Tiff, pdfAsBase64String, password, page, dpi, width, height);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a TIFF.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveTiff(Stream imageStream, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageStream, ImageFormat.Tiff, pdfAsBase64String, password, page, dpi, width, height);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a TIFF.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveTiff(string imageFilename, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageFilename, ImageFormat.Tiff, pdfAsByteArray, password, page, dpi, width, height);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a TIFF.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveTiff(Stream imageStream, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageStream, ImageFormat.Tiff, pdfAsByteArray, password, page, dpi, width, height);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a TIFF.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveTiff(string imageFilename, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageFilename, ImageFormat.Tiff, pdfStream, password, page, dpi, width, height);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a TIFF.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveTiff(Stream imageStream, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageStream, ImageFormat.Tiff, pdfStream, password, page, dpi, width, height);
        }
        #endregion

        #region SaveGif
        /// <summary>
        /// Renders a single page of a given PDF and saves it as a GIF.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveGif(string imageFilename, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageFilename, ImageFormat.Gif, pdfAsBase64String, password, page, dpi, width, height);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a GIF.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsBase64String">The PDF encoded as Base64.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveGif(Stream imageStream, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageStream, ImageFormat.Gif, pdfAsBase64String, password, page, dpi, width, height);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a GIF.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveGif(string imageFilename, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageFilename, ImageFormat.Gif, pdfAsByteArray, password, page, dpi, width, height);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a GIF.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfAsByteArray">The PDF as a byte array.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveGif(Stream imageStream, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageStream, ImageFormat.Gif, pdfAsByteArray, password, page, dpi, width, height);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a GIF.
        /// </summary>
        /// <param name="imageFilename">The output image file path.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveGif(string imageFilename, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageFilename, ImageFormat.Gif, pdfStream, password, page, dpi, width, height);
        }

        /// <summary>
        /// Renders a single page of a given PDF and saves it as a GIF.
        /// </summary>
        /// <param name="imageStream">The output image stream.</param>
        /// <param name="pdfStream">The PDF as a stream.</param>
        /// <param name="password">The password for opening the PDF. Use <see langword="null"/> if no password is needed.</param>
        /// <param name="page">The specific page to be converted.</param>
        /// <param name="dpi">The DPI scaling to use for rasterization of the PDF.</param>
        /// <param name="width">The width of the desired <paramref name="page"/>. Use <see langword="null"/> if the original width should be used.</param>
        /// <param name="height">The height of the desired <paramref name="page"/>. Use <see langword="null"/> if the original height should be used.</param>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveGif(Stream imageStream, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageStream, ImageFormat.Gif, pdfStream, password, page, dpi, width, height);
        }
        #endregion

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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveJpeg(string imageFilename, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageFilename, ImageFormat.Jpeg, pdfAsBase64String, password, page, dpi, width, height);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveJpeg(Stream imageStream, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageStream, ImageFormat.Jpeg, pdfAsBase64String, password, page, dpi, width, height);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveJpeg(string imageFilename, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageFilename, ImageFormat.Jpeg, pdfAsByteArray, password, page, dpi, width, height);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveJpeg(Stream imageStream, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageStream, ImageFormat.Jpeg, pdfAsByteArray, password, page, dpi, width, height);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveJpeg(string imageFilename, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageFilename, ImageFormat.Jpeg, pdfStream, password, page, dpi, width, height);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveJpeg(Stream imageStream, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageStream, ImageFormat.Jpeg, pdfStream, password, page, dpi, width, height);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SavePng(string imageFilename, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageFilename, ImageFormat.Png, pdfAsBase64String, password, page, dpi, width, height);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SavePng(Stream imageStream, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageStream, ImageFormat.Png, pdfAsBase64String, password, page, dpi, width, height);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SavePng(string imageFilename, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageFilename, ImageFormat.Png, pdfAsByteArray, password, page, dpi, width, height);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SavePng(Stream imageStream, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageStream, ImageFormat.Png, pdfAsByteArray, password, page, dpi, width, height);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SavePng(string imageFilename, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageFilename, ImageFormat.Png, pdfStream, password, page, dpi, width, height);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SavePng(Stream imageStream, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageStream, ImageFormat.Png, pdfStream, password, page, dpi, width, height);
        }
        #endregion

        #region SaveBmp
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveBmp(string imageFilename, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageFilename, ImageFormat.Bmp, pdfAsBase64String, password, page, dpi, width, height);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveBmp(Stream imageStream, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageStream, ImageFormat.Bmp, pdfAsBase64String, password, page, dpi, width, height);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveBmp(string imageFilename, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageFilename, ImageFormat.Bmp, pdfAsByteArray, password, page, dpi, width, height);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveBmp(Stream imageStream, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageStream, ImageFormat.Bmp, pdfAsByteArray, password, page, dpi, width, height);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveBmp(string imageFilename, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageFilename, ImageFormat.Bmp, pdfStream, password, page, dpi, width, height);
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
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static void SaveBmp(Stream imageStream, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            SaveImpl(imageStream, ImageFormat.Bmp, pdfStream, password, page, dpi, width, height);
        }
        #endregion

        #region Internal save impl
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        internal static void SaveImpl(string imageFilename, ImageFormat format, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            if (imageFilename == null)
                throw new ArgumentNullException(nameof(imageFilename));

            ToImage(pdfAsBase64String, password, page, dpi, width, height).Save(imageFilename, format);
        }

#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        internal static void SaveImpl(Stream imageStream, ImageFormat format, string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            if (imageStream == null)
                throw new ArgumentNullException(nameof(imageStream));

            ToImage(pdfAsBase64String, password, page, dpi, width, height).Save(imageStream, format);
        }

#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        internal static void SaveImpl(string imageFilename, ImageFormat format, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            if (imageFilename == null)
                throw new ArgumentNullException(nameof(imageFilename));

            ToImage(pdfAsByteArray, password, page, dpi, width, height).Save(imageFilename, format);
        }

#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        internal static void SaveImpl(Stream imageStream, ImageFormat format, byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            if (imageStream == null)
                throw new ArgumentNullException(nameof(imageStream));

            ToImage(pdfAsByteArray, password, page, dpi, width, height).Save(imageStream, format);
        }

#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        internal static void SaveImpl(string filename, ImageFormat format, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            ToImage(pdfStream, password, page, dpi, width, height).Save(filename, format);
        }

#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        internal static void SaveImpl(Stream stream, ImageFormat format, Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            ToImage(pdfStream, password, page, dpi, width, height).Save(stream, format);
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
        /// <returns>The converted PDF page as an image.</returns>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static Image ToImage(string pdfAsBase64String, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            if (pdfAsBase64String == null)
                throw new ArgumentNullException(nameof(pdfAsBase64String));

            return ToImage(Convert.FromBase64String(pdfAsBase64String), password, page, dpi, width, height);
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
        /// <returns>The converted PDF page as an image.</returns>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static Image ToImage(byte[] pdfAsByteArray, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
        {
            if (pdfAsByteArray == null)
                throw new ArgumentNullException(nameof(pdfAsByteArray));

            // Base64 string -> byte[] -> MemoryStream
            using var pdfStream = new MemoryStream(pdfAsByteArray, false);

            return ToImage(pdfStream, password, page, dpi, width, height);
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
        /// <returns>The rendered PDF page as an image.</returns>
#if NET5_0_OR_GREATER
        [SupportedOSPlatform("Windows")]
        [SupportedOSPlatform("Linux")]
        [SupportedOSPlatform("macOS")]
#endif
        public static Image ToImage(Stream pdfStream, string? password = null, int page = 0, int dpi = 300, int? width = null, int? height = null)
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

            // Stream -> PdfiumViewer.PdfDocument
            using var pdfDocument = PdfDocument.Load(pdfStream, password);

            // PdfiumViewer.PdfDocument -> Image
            return pdfDocument.Render(page, width ?? (int)pdfDocument.PageSizes[page].Width, height ?? (int)pdfDocument.PageSizes[page].Height, dpi, dpi, renderFlags);
        }
        #endregion
    }
}