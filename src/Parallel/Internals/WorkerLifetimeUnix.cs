using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Threading;

namespace PDFtoImage.Parallel.Internals
{
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    internal static class WorkerLifetimeUnix
    {
        internal static void StartWatchdog(Socket lifetime)
        {
            using var ready = new ManualResetEventSlim();
            var watchdog = new Thread(() =>
            {
                ready.Set();
                try
                {
                    // This socket carries no messages. EOF, an error, or unexpected
                    // data all mean that the worker must stop immediately.
                    Span<byte> buffer = stackalloc byte[1];
                    lifetime.Receive(buffer);
                }
                catch (SocketException) { }
                catch (ObjectDisposedException) { }

                // Environment.Exit can wait for managed shutdown callbacks. Kill
                // avoids those, and does not depend on the PDFium or thread pool threads.
                using var process = Process.GetCurrentProcess();
                process.Kill();
            }) { IsBackground = true, Name = "PDFtoImage parent watchdog" };
            watchdog.Start();
            ready.Wait();
        }
    }
}