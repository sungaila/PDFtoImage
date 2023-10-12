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

                using var bitmap = PDFtoImage.Conversion.ToImage(input);

                return $"SocialPreview.pdf size: {bitmap.Width}x{bitmap.Height}";
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }
    }
}