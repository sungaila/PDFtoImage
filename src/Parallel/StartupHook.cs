using PDFtoImage.Parallel.Internals;

/// <summary>
/// CoreCLR startup hook used to load PDFtoImage.Parallel before the host application's entry point.
/// Native AOT enters the same worker bootstrap through the module initializer instead.
/// </summary>
internal static class StartupHook
{
    /// <summary>
    /// Runs the worker host when this process was started by PDFtoImage.Parallel.
    /// </summary>
    public static void Initialize() => WorkerBootstrap.RunIfWorker();
}