using System;
using System.ComponentModel;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Parallel.Internals
{
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    internal sealed class WorkerConnectionUnix : WorkerConnection
    {
        private Socket? _lifetime;

        private WorkerConnectionUnix() : base(Stream.Null) { }

        internal static async Task<WorkerConnectionUnix> StartAsync(string commandPath, string lifetimePath, CancellationToken cancellationToken)
        {
            var worker = new WorkerConnectionUnix();
            using var commandListener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            using var lifetimeListener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            var commandBound = false;
            var lifetimeBound = false;

            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                commandListener.Bind(new UnixDomainSocketEndPoint(commandPath));
                commandBound = true;
                commandListener.Listen(1);
                lifetimeListener.Bind(new UnixDomainSocketEndPoint(lifetimePath));
                lifetimeBound = true;
                lifetimeListener.Listen(1);

                using var startupTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                using var startupCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, startupTimeout.Token);
                worker._process = WorkerProcessLauncherUnix.Start(commandPath, lifetimePath);
                worker._process.EnableRaisingEvents = true;

                void onExit(object? sender, EventArgs args)
                {
                    try { startupCancellation.Cancel(); }
                    catch (ObjectDisposedException) { /* An Exited callback was already queued. */ }
                }
                worker._process.Exited += onExit;

                try
                {
                    if (worker._process.HasExited)
                        throw new EndOfStreamException("The PDF conversion worker exited before connecting.");

                    worker._lifetime = await lifetimeListener.AcceptAsync(startupCancellation.Token).ConfigureAwait(false);
                    var command = await commandListener.AcceptAsync(startupCancellation.Token).ConfigureAwait(false);
                    worker._stream = new NetworkStream(command, ownsSocket: true);
                    await ReadHelloAsync(worker._stream, startupCancellation.Token, startupTimeout.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
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
            finally
            {
                // Connected sockets stay usable after unlinking. No pathname is
                // needed once startup has completed, or after a failed attempt.
                try
                {
                    commandListener.Dispose();
                    lifetimeListener.Dispose();
                    if (commandBound)
                        File.Delete(commandPath);
                    if (lifetimeBound)
                        File.Delete(lifetimePath);
                }
                catch
                {
                    worker.Dispose();
                    throw;
                }
            }
        }

        public override void Dispose()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            _stream.Dispose();
            _lifetime?.Dispose();
            var process = Interlocked.Exchange(ref _process, null);
            if (process == null)
                return;

            try
            {
                // The watchdog also terminates a worker stuck in native rendering.
                // Kill is the fallback for a worker that never reached its hook.
                if (!process.WaitForExit(1000))
                    process.Kill();
                process.WaitForExit();
            }
            catch (InvalidOperationException) { /* The process already exited. */ }
            catch (Win32Exception) when (process.HasExited) { /* The watchdog won the race. */ }
            finally
            {
                process.Dispose();
            }
        }
    }
}