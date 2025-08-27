using Android.App;
using Android.Runtime;

namespace PDFtoImage.FrameworkTests.MauiApp.Platforms.Android
{
    [Application]
    public class MainApplication : MauiApplication
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0290")]
        public MainApplication(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override Microsoft.Maui.Hosting.MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}