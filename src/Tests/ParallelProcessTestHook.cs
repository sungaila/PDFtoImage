#if NET11_0_OR_GREATER
using PDFtoImage.Parallel;
using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

// Loaded explicitly via DOTNET_STARTUP_HOOKS only by the process-lifetime tests.
// Ordinary test runs return immediately. Parallel workers prepend their own hook
// and exit before executing this hook or the application's Main method.
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1050")]
public static class StartupHook
{
    public static void Initialize()
    {
        PDFtoImage.Tests.ParallelUnixProcessTestHook.Initialize();
        var pipeName = Environment.GetEnvironmentVariable("PDFTOIMAGE_TEST_PARENT_PIPE");
        if (string.IsNullOrEmpty(pipeName))
            return;
        Environment.SetEnvironmentVariable("PDFTOIMAGE_TEST_PARENT_PIPE", null);
        Environment.Exit(RunAsync(pipeName).GetAwaiter().GetResult());
    }

    private static async Task<int> RunAsync(string pipeName)
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

        await pipe.ConnectAsync(timeout.Token);

        var pdf = await File.ReadAllBytesAsync(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "Wikimedia_Commons_web.pdf"), timeout.Token);

        await using var pool = new ParallelPdfProcessor(2);

        var warmup = await Task.WhenAll(
            pool.ToImageAsync(new MemoryStream(pdf, writable: false), options: new PDFtoImage.RenderOptions(Dpi: 40), cancellationToken: timeout.Token),
            pool.ToImageAsync(new MemoryStream(pdf, writable: false), options: new PDFtoImage.RenderOptions(Dpi: 40), cancellationToken: timeout.Token));

        foreach (var bitmap in warmup)
            bitmap.Dispose();

        using var writer = new StreamWriter(pipe, leaveOpen: true) { AutoFlush = true };
        using var reader = new StreamReader(pipe, leaveOpen: true);

        await writer.WriteLineAsync(string.Join(",", pool.WorkerProcessIds));
        await reader.ReadLineAsync(timeout.Token);

        return 0;
    }
}
#endif