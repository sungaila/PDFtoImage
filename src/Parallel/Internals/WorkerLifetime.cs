using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Threading;

namespace PDFtoImage.Parallel.Internals
{
    internal static class WorkerLifetime
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "CA2022")]
        internal static void StartWatchdog(SafeFileHandle lifetime)
        {
            using var ready = new ManualResetEventSlim();
            var watchdog = new Thread(() =>
            {
                try
                {
                    using var stream = new FileStream(lifetime, FileAccess.Read, bufferSize: 1, isAsync: false);
                    ready.Set();

                    // The pipe carries no data. EOF, an error, or unexpected data all
                    // mean that the parent no longer owns this worker.
                    Span<byte> buffer = stackalloc byte[1];
                    stream.Read(buffer);
                }
                catch (IOException) { }
                catch (ObjectDisposedException) { }
                finally
                {
                    ready.Set();
                }

                try
                {
                    using var process = SafeProcessHandle.Open(Environment.ProcessId);
                    process.Kill();
                }
                catch
                {
                    Environment.FailFast("PDFtoImage worker lost its parent process.");
                }
            })
            {
                IsBackground = true,
                Name = "PDFtoImage parent watchdog"
            };

            watchdog.Start();
            ready.Wait();
        }
    }
}