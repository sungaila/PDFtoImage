using System.Runtime.InteropServices;

namespace PDFtoImage.FrameworkTests.AotConsole;

public static class Program
{
    private const int ExpectedWidth = 5333;
    private const int ExpectedHeight = 2666;

    public static int Main()
    {
        try
        {
            Console.WriteLine($"Framework: {RuntimeInformation.FrameworkDescription}");
            Console.WriteLine($"OS: {RuntimeInformation.OSDescription}");
            Console.WriteLine($"Process architecture: {RuntimeInformation.ProcessArchitecture}");

            Directory.SetCurrentDirectory(AppContext.BaseDirectory);

            using var input = new FileStream(
                "SocialPreview.pdf",
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

            using var bitmap = PDFtoImage.Conversion.ToImage(input, 0);

            Console.WriteLine($"SocialPreview.pdf size: {bitmap.Width}x{bitmap.Height}");

            if (ExpectedWidth != bitmap.Width || ExpectedHeight != bitmap.Height)
            {
                throw new InvalidOperationException(
                    $"Expected {ExpectedWidth}x{ExpectedHeight}, " +
                    $"but received {bitmap.Width}x{bitmap.Height}.");
            }

            Console.WriteLine("PDFtoImage smoke test passed.");
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }
}
