namespace PDFtoImage.FrameworkTests.AotConsole
{
    public class Program
    {
        private const int ExpectedWidth = 5333;
        private const int ExpectedHeight = 2666;

        public static void Main()
        {
            throw new NotImplementedException();
            Directory.SetCurrentDirectory(Path.GetDirectoryName(AppContext.BaseDirectory)!);
            using var input = new FileStream("SocialPreview.pdf", FileMode.Open, FileAccess.Read);
            using var bitmap = PDFtoImage.Conversion.ToImage(input);

            Console.WriteLine($"SocialPreview.pdf size: {bitmap.Width}x{bitmap.Height}");

            if (ExpectedWidth != bitmap.Width || ExpectedHeight != bitmap.Height)
                throw new InvalidOperationException("Expected PDF width and height differ.");
        }
    }
}