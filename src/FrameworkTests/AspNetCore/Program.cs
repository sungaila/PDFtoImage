namespace PDFtoImage.FrameworkTests.AspNetCore
{
    public class Program
    {
        private static IWebHostEnvironment? _hostingEnvironment;

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var app = builder.Build();

            _hostingEnvironment = app.Services.GetService<IWebHostEnvironment>();

            app.MapGet("/", GetOutput);

            app.Run();
        }

        private static string GetOutput()
        {
            try
            {
                using var input = new FileStream(Path.Combine(_hostingEnvironment!.WebRootPath, "SocialPreview.pdf"), FileMode.Open, FileAccess.Read);

#pragma warning disable IDE0079
#pragma warning disable CA1416
                using var bitmap = PDFtoImage.Conversion.ToImage(input, 0);
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