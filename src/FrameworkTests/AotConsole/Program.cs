using System.Runtime.InteropServices;

namespace PDFtoImage.FrameworkTests.AotConsole;

public static class Program
{
    private const int ExpectedWidth = 5333;
    private const int ExpectedHeight = 2666;

    public static void Main()
    {
        Console.WriteLine($"Framework: {RuntimeInformation.FrameworkDescription}");
        Console.WriteLine($"OS: {RuntimeInformation.OSDescription}");
        Console.WriteLine($"Process architecture: {RuntimeInformation.ProcessArchitecture}");
        Console.WriteLine();

        Directory.SetCurrentDirectory(AppContext.BaseDirectory);

        using var input = new FileStream(
            "SocialPreview.pdf",
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        using var bitmap = PDFtoImage.Conversion.ToImage(input, 0);

        Console.WriteLine($"SocialPreview.pdf size: {bitmap.Width}x{bitmap.Height}");
        Console.WriteLine();

        if (ExpectedWidth != bitmap.Width || ExpectedHeight != bitmap.Height)
        {
            throw new InvalidOperationException($"Expected {ExpectedWidth}x{ExpectedHeight}, but received {bitmap.Width}x{bitmap.Height}.");
        }

        Console.WriteLine("PDFtoImage smoke test passed.");
    }
}