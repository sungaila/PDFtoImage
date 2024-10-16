using SkiaSharp;

namespace PDFtoImage.FrameworkTests.MauiApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private SKBitmap _bitmap;
        private async void OnCounterClicked(object sender, EventArgs e)
        {
            try
            {
                using var input = await FileSystem.OpenAppPackageFileAsync("SocialPreview.pdf");
                using var ms = new MemoryStream();
                input.CopyTo(ms);             

                _bitmap = PDFtoImage.Conversion.ToImage(ms, 0);
                OutputLabel.Text = $"SocialPreview.pdf size: {_bitmap.Width}x{_bitmap.Height}";
                
                SKImage image = SKImage.FromBitmap(_bitmap);
                SKData encodedData = image.Encode(SKEncodedImageFormat.Png, 100);
                string imagePath = Path.Combine(FileSystem.CacheDirectory, "image.png");
                var bitmapImageStream = File.Open(imagePath, 
                    FileMode.Create, 
                    FileAccess.Write, 
                    FileShare.None);
                encodedData.SaveTo(bitmapImageStream);
                bitmapImageStream.Flush(true);
                bitmapImageStream.Dispose();

                imgTest.Source = ImageSource.FromFile(imagePath);


            }
            catch (Exception ex)
            {
                OutputLabel.Text = ex.ToString();
            }
        }
    }
}