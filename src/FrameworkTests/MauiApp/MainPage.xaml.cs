namespace PDFtoImage.FrameworkTests.MauiApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnCounterClicked(object sender, EventArgs e)
        {
            try
            {
                using var input = await FileSystem.OpenAppPackageFileAsync("SocialPreview.pdf");
                using var ms = new MemoryStream();
                input.CopyTo(ms);

#pragma warning disable CA1416 // Validate platform compatibility
                using var bitmap = PDFtoImage.Conversion.ToImage(ms);
#pragma warning restore CA1416 // Validate platform compatibility

                OutputLabel.Text = $"SocialPreview.pdf size: {bitmap.Width}x{bitmap.Height}";
            }
            catch (Exception ex)
            {
                OutputLabel.Text = ex.ToString();
            }
        }
    }
}