#if NET11_0_OR_GREATER
using PDFtoImage.Parallel.Internals;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Tests
{
    internal static class ParallelUnixProcessTestHook
    {
        internal const string ParentPipeVariable = "PDFTOIMAGE_TEST_UNIX_PARENT_PIPE";

        internal static void Initialize()
        {
            var pipeName = Environment.GetEnvironmentVariable(ParentPipeVariable);
            if (string.IsNullOrEmpty(pipeName))
                return;
            Environment.SetEnvironmentVariable(ParentPipeVariable, null);
            Environment.Exit(RunAsync(pipeName).GetAwaiter().GetResult());
        }

        private static async Task<int> RunAsync(string pipeName)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(45));
            using var pipe = new NamedPipeClientStream(".", pipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
            await pipe.ConnectAsync(timeout.Token);
            await using var pool = new WorkerPool(2);
            var request = new PdfRequest(SlowPdf(), null);
            await Task.WhenAll(pool.GetPageCountAsync(request, timeout.Token), pool.GetPageCountAsync(request, timeout.Token));
            var workers = pool.WorkerProcessIds.Select(Process.GetProcessById).ToArray();
            try
            {
                var renders = new[]
                {
                    pool.RenderPageAsync(request, 0, new RenderOptions(Dpi: 72), timeout.Token),
                    pool.RenderPageAsync(request, 0, new RenderOptions(Dpi: 72), timeout.Token)
                };
                await WaitForRenderingAsync(workers, renders, timeout.Token);
                using var writer = new StreamWriter(pipe, leaveOpen: true) { AutoFlush = true };
                using var reader = new StreamReader(pipe, leaveOpen: true);
                await writer.WriteLineAsync(string.Join(",", pool.WorkerProcessIds));
                await reader.ReadLineAsync(timeout.Token);
                await pool.DisposeAsync();
                try { await Task.WhenAll(renders); }
                catch (OperationCanceledException) { }
                return 0;
            }
            finally
            {
                foreach (var worker in workers)
                    worker.Dispose();
            }
        }

        internal static async Task WaitForRenderingAsync(Process[] workers, Task[] renders, CancellationToken token)
        {
            var initialCpu = workers.Select(worker => worker.TotalProcessorTime).ToArray();
            while (true)
            {
                token.ThrowIfCancellationRequested();
                if (renders.Any(render => render.IsCompleted))
                    throw new InvalidOperationException("The deliberately slow renders must still be active.");
                foreach (var worker in workers)
                    worker.Refresh();
                if (workers.Select((worker, i) => worker.TotalProcessorTime - initialCpu[i]).All(cpu => cpu.TotalMilliseconds >= 200))
                    return;
                await Task.Delay(10, token);
            }
        }

        internal static byte[] SlowPdf()
        {
            // Many overlapping fills keep PDFium busy without a huge bitmap or
            // production-only test hooks. Both processes must accrue rendering CPU
            // time before the test is allowed to kill their parent.
            var content = new StringBuilder("0.1 0.2 0.3 rg\n");
            for (var i = 0; i < 100000; i++)
                content.Append("0 0 1000 1000 re f\n");
            string[] objects =
            [
                "<< /Type /Catalog /Pages 2 0 R >>",
                "<< /Type /Pages /Kids [3 0 R] /Count 1 >>",
                "<< /Type /Page /Parent 2 0 R /MediaBox [0 0 1000 1000] /Resources << >> /Contents 4 0 R >>",
                $"<< /Length {content.Length} >>\nstream\n{content}endstream"
            ];
            var pdf = new StringBuilder("%PDF-1.4\n");
            var offsets = new int[objects.Length];
            for (var i = 0; i < objects.Length; i++)
            {
                offsets[i] = pdf.Length;
                pdf.Append($"{i + 1} 0 obj\n{objects[i]}\nendobj\n");
            }
            var xref = pdf.Length;
            pdf.Append($"xref\n0 {objects.Length + 1}\n0000000000 65535 f \n");
            foreach (var offset in offsets)
                pdf.Append($"{offset:D10} 00000 n \n");
            pdf.Append($"trailer\n<< /Size {objects.Length + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");
            return Encoding.ASCII.GetBytes(pdf.ToString());
        }
    }
}
#endif
