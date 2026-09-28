namespace PDFtoImage.FrameworkTests.AspNetCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
#if PDFTOIMAGE_PARALLEL
#pragma warning disable CA1416
            builder.Services.AddSingleton<PDFtoImage.Parallel.IParallelPdfProcessor>(_ =>
                new PDFtoImage.Parallel.ParallelPdfProcessor(
                    new PDFtoImage.Parallel.ProcessorOptions { WorkerCount = 2 }));
#pragma warning restore CA1416
#endif
            var app = builder.Build();

            app.MapGet("/", GetOutput);

            app.Run();
        }

#if PDFTOIMAGE_PARALLEL
        private static async Task<string> GetOutput(IWebHostEnvironment hostingEnvironment, PDFtoImage.Parallel.IParallelPdfProcessor processor)
#else
        private static string GetOutput(IWebHostEnvironment hostingEnvironment)
#endif
        {
            try
            {
                using var input = new FileStream(Path.Combine(hostingEnvironment.WebRootPath, "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);

#pragma warning disable IDE0079
#pragma warning disable CA1416
#if PDFTOIMAGE_PARALLEL
                using var bitmap = await processor.ToImageAsync(input, 0);
#else
                using var bitmap = PDFtoImage.Conversion.ToImage(input, 0);
#endif
#pragma warning restore CA1416
#pragma warning restore IDE0079

                return $"SocialPreview.pdf size: {bitmap.Width}x{bitmap.Height}";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}