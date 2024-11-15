using SkiaSharp;
using System;
using System.IO;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace PDFtoImage.FrameworkTests.Uwp
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void CounterBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                using (var input = new FileStream("SocialPreview.pdf", FileMode.Open, FileAccess.Read))
                {
                    using (var bitmap = PDFtoImage.Conversion.ToImage(input))
                    {
                        using (var encodedImage = new MemoryStream())
                        {
                            OutputLabel.Text = $"SocialPreview.pdf size: {bitmap.Width}x{bitmap.Height}";
                            bitmap.Encode(encodedImage, SKEncodedImageFormat.Png, 100);

                            var byteArray = encodedImage.ToArray();

                            var bitmapImage = new BitmapImage
                            {
                                DecodePixelHeight = bitmap.Width,
                                DecodePixelWidth = bitmap.Height
                            };

                            encodedImage.Seek(0, SeekOrigin.Begin);
                            await bitmapImage.SetSourceAsync(encodedImage.AsRandomAccessStream());

                            imgTest.Source = bitmapImage;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OutputLabel.Text = ex.ToString();
            }
        }
    }
}