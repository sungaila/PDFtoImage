using System.Runtime.InteropServices;

namespace PDFtoImage.FrameworkTests.AotConsole;

public static class Program
{
    private const int ExpectedWidth = 5333;
    private const int ExpectedHeight = 2666;

    public static async Task Main()
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

#if PDFTOIMAGE_PARALLEL
        Console.WriteLine("Renderer: PDFtoImage.Parallel");
        await using var processor = new PDFtoImage.Parallel.ParallelPdfProcessor(workerCount: 2);
        using var bitmap = await processor.ToImageAsync(input, 0);
#else
        Console.WriteLine("Renderer: PDFtoImage");
        using var bitmap = PDFtoImage.Conversion.ToImage(input, 0);
#endif

        Console.WriteLine($"SocialPreview.pdf size: {bitmap.Width}x{bitmap.Height}");
        Console.WriteLine();

        if (ExpectedWidth != bitmap.Width || ExpectedHeight != bitmap.Height)
        {
            throw new InvalidOperationException($"Expected {ExpectedWidth}x{ExpectedHeight}, but received {bitmap.Width}x{bitmap.Height}.");
        }

        Console.WriteLine("PDFtoImage smoke test passed.");
    }
}