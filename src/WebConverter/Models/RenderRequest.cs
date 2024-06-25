using Microsoft.AspNetCore.Components.Forms;
using SkiaSharp;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace PDFtoImage.WebConverter.Models
{
    public class RenderRequest : IDisposable
    {
        public static readonly SKEncodedImageFormat[] FormatWhitelist =
        [
            SKEncodedImageFormat.Png,
            SKEncodedImageFormat.Jpeg,
            SKEncodedImageFormat.Webp
        ];

        private bool disposedValue;

        [Required(ErrorMessage = "Select a PDF file to convert.")]
        public IBrowserFile? File { get; set; }

        public string? Password { get; set; }

        [Required]
        public SKEncodedImageFormat Format { get; set; } = SKEncodedImageFormat.Png;

        [Required]
        [Range(0, 100, ErrorMessage = "Quality invalid (1-100).")]
        public int Quality { get; set; } = 100;

        [Range(1, int.MaxValue, ErrorMessage = "Width invalid (≥0).")]
        public int? Width { get; set; } = null;

        [Range(1, int.MaxValue, ErrorMessage = "Height invalid (≥0).")]
        public int? Height { get; set; } = null;

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "DPI invalid (≥0).")]
        public int Dpi { get; set; } = 300;

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Page number invalid (≥0).")]
        public int Page { get; set; } = 0;

        [Required]
        public bool WithAnnotations { get; set; } = true;

        [Required]
        public bool WithFormFill { get; set; } = true;

        [Required]
        public bool WithAspectRatio { get; set; } = true;

        [Required]
        public bool AntiAliasingText { get; set; } = true;

        [Required]
        public bool AntiAliasingImages { get; set; } = true;

        [Required]
        public bool AntiAliasingPaths { get; set; } = true;

        [Required]
        public string BackgroundColor { get; set; } = "#FFFFFF";

        [Range(byte.MinValue, byte.MaxValue, ErrorMessage = "Opacity invalid (0-255).")]
        public int Opacity { get; set; } = 255;

        [Required]
        public PdfRotation Rotation { get; set; } = PdfRotation.Rotate0;

        [Required]
        public bool UseBounds { get; set; } = false;

        [Required]
        [Range(float.MinValue, float.MaxValue)]
        public float BoundsX { get; set; } = 0f;

        [Required]
        [Range(float.MinValue, float.MaxValue)]
        public float BoundsY { get; set; } = 0f;

        [Required]
        [Range(float.MinValue, float.MaxValue)]
        public float BoundsWidth { get; set; } = 1000f;

        [Required]
        [Range(float.MinValue, float.MaxValue)]
        public float BoundsHeight { get; set; } = 1000f;

        public bool UseTiling { get; set; } = true;

        public bool DpiRelativeToBounds { get; set; } = false;

        public static string GetRotationLocalized(PdfRotation rotation) => rotation switch
        {
            PdfRotation.Rotate0 => "0°",
            PdfRotation.Rotate90 => "90°",
            PdfRotation.Rotate180 => "180°",
            PdfRotation.Rotate270 => "270°",
            _ => throw new ArgumentOutOfRangeException(nameof(rotation))
        };

        public static string GetMimeType(SKEncodedImageFormat format) => format switch
        {
            SKEncodedImageFormat.Png => "image/png",
            SKEncodedImageFormat.Jpeg => "image/jpeg",
            SKEncodedImageFormat.Webp => "image/webp",
            _ => throw new ArgumentOutOfRangeException(nameof(format))
        };

        public static string GetOutputFileName(RenderRequest model) => $"{model.File!.Name}.{model.Format.ToString().ToLowerInvariant()}";

        public Stream? Input { get; set; }

        public Stream? Output { get; set; }

        public override string ToString()
        {
            return $"{nameof(RenderRequest)} {nameof(File)}={File?.Name ?? "<null>"}, {nameof(Password)}={(!string.IsNullOrEmpty(Password) ? "<password>" : "<null>")}, {nameof(Page)}={Page}, {nameof(Format)}={Format}, {nameof(Quality)}={Quality}, {nameof(Width)}={(Width != null ? Width.Value : "<null>")}, {nameof(Height)}={(Height != null ? Height.Value : "<null>")}, {nameof(Dpi)}={Dpi}, {nameof(Rotation)}={Rotation}, {nameof(WithAspectRatio)}={WithAspectRatio}, {nameof(WithAnnotations)}={WithAnnotations}, {nameof(WithFormFill)}={WithFormFill}";
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Input?.Dispose();
                    Input = null;
                    Output?.Dispose();
                    Output = null;
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
