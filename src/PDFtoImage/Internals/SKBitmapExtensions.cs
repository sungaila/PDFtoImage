using SkiaSharp;
using System;
using System.IO;

namespace PDFtoImage.Internals
{
    internal static class SKBitmapExtensions
    {
        [Obsolete("This is a workaround for the missing PNG compression in SkiaSharp 3.x. Remove this once SkiaSharp is fixed. See GitHub issue: https://github.com/mono/SkiaSharp/issues/3013")]
        public static bool EncodeExt(this SKBitmap bitmap, Stream dst, SKEncodedImageFormat format, int quality)
        {
            if (format == SKEncodedImageFormat.Png)
            {
                using var pixmap = new SKPixmap(bitmap.Info, bitmap.GetPixels());
                using var data = pixmap.Encode(SKPngEncoderOptions.Default);

                if (data == null)
                    return false;

                data.SaveTo(dst);

                return true;
            }

            return bitmap.Encode(dst, format, quality);
        }
    }
}