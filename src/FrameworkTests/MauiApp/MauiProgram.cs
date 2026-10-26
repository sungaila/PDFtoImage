namespace PDFtoImage.FrameworkTests.MauiApp
{
    public static class MauiProgram
    {
        public static Microsoft.Maui.Hosting.MauiApp CreateMauiApp()
        {
            var builder = Microsoft.Maui.Hosting.MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans_Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans_Semibold.ttf", "OpenSansSemibold");
                });

            return builder.Build();
        }
    }
}