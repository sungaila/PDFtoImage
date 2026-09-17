using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace PDFtoImage.Parallel.Internals
{
    internal static class OrderedScheduler
    {
#if NETCOREAPP
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Maintainability", "CA1512")]
#endif
        internal static async IAsyncEnumerable<T> RunAsync<T>(
            IEnumerable<int> pages, int capacity, Func<int, CancellationToken, Task<T>> render,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));
            using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            using var input = pages.GetEnumerator();
            var pending = new Queue<Task<T>>();
            try
            {
                for (var i = 0; i < capacity && input.MoveNext(); i++)
                    pending.Enqueue(render(input.Current, cancellation.Token));
                while (pending.Count > 0)
                {
                    cancellation.Token.ThrowIfCancellationRequested();
                    var result = await pending.Dequeue().ConfigureAwait(false);
                    cancellation.Token.ThrowIfCancellationRequested();
                    yield return result;
                    if (input.MoveNext())
                        pending.Enqueue(render(input.Current, cancellation.Token));
                }
            }
            finally
            {
                cancellation.Cancel();
                foreach (var task in pending)
                {
                    try { await task.ConfigureAwait(false); }
                    catch { /* Observe failures during cancellation/early enumeration exit. */ }
                }
            }
        }
    }
}