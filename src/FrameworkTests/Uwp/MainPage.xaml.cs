using System;
using System.IO;
using Windows.UI.Xaml.Controls;

namespace PDFtoImage.FrameworkTests.Uwp
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void CounterBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            try
            {
                using (var input = new FileStream("SocialPreview.pdf", FileMode.Open, FileAccess.Read))
                {
                    using (var bitmap = PDFtoImage.Conversion.ToImage(input))
                    {
                        OutputLabel.Text = $"SocialPreview.pdf size: {bitmap.Width}x{bitmap.Height}";
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