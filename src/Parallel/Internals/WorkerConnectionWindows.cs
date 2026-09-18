using System;
using System.ComponentModel;
using System.IO;
using System.IO.Pipes;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Parallel.Internals
{
    [SupportedOSPlatform("windows10.0")]
    internal sealed class WorkerConnectionWindows : WorkerConnection
    {
        private WorkerConnectionWindows(NamedPipeServerStream pipe) : base(pipe) { }

        internal static async Task<WorkerConnectionWindows> StartAsync(WindowsJob job, CancellationToken cancellationToken)
        {
            var pipeName = "PDFtoImage.Parallel." + Guid.NewGuid().ToString("N");

            var pipe = new NamedPipeServerStream(
                pipeName,
                PipeDirection.InOut,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);

            var worker = new WorkerConnectionWindows(pipe);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                worker._process = WorkerProcessLauncherWindows.StartSuspendedAndAssign(job, pipeName);

                using var startupTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                using var startupCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, startupTimeout.Token);
                worker._process.EnableRaisingEvents = true;

                void onExit(object? _1, EventArgs _2)
                {
                    // Unsubscribing cannot retract an already queued Exited callback.
                    try
                    {
                        startupCancellation.Cancel();
                    }
                    catch (ObjectDisposedException) { }
                }
                worker._process.Exited += onExit;

                try
                {
                    if (worker._process.HasExited)
                        throw new EndOfStreamException("The PDF conversion worker exited before connecting.");

                    await pipe.WaitForConnectionAsync(startupCancellation.Token).ConfigureAwait(false);

                    await ReadHelloAsync(pipe, cancellationToken, startupTimeout.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (TimeoutException) when (worker._process.HasExited)
                {
                    throw new EndOfStreamException("The PDF conversion worker exited during startup.");
                }
                catch (OperationCanceledException) when (worker._process.HasExited)
                {
                    throw new EndOfStreamException("The PDF conversion worker exited during startup.");
                }
                catch (OperationCanceledException) when (startupTimeout.IsCancellationRequested)
                {
                    throw new TimeoutException("The PDF conversion worker did not complete startup within 30 seconds.");
                }
                finally
                {
                    worker._process.Exited -= onExit;
                }

                return worker;
            }
            catch
            {
                worker.Dispose();
                throw;
            }
        }

        public override void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            _stream.Dispose();

            var process = Interlocked.Exchange(ref _process, null);

            if (process == null)
                return;

            try
            {
                if (!process.HasExited)
                    process.Kill();

                process.WaitForExit();
            }
            catch (InvalidOperationException) { /* The process already exited. */ }
            catch (Win32Exception) when (process.HasExited) { /* The job terminated it concurrently. */ }
            finally
            {
                process.Dispose();
            }
        }

    }
}