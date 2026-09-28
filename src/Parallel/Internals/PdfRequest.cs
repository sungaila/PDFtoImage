using System;
using System.IO;
using System.Threading;

namespace PDFtoImage.Parallel.Internals
{
    // Identity belongs to a conversion operation, not the mutable input array.
    // The same request is shared by all page jobs of one async enumeration.
    internal sealed class PdfRequest : IDisposable
    {
        private readonly FileStream? _lifetime;

        private readonly Action<PdfRequest>? _onDispose;

        private int _disposed;

        internal PdfRequest(byte[] bytes, string? password)
        {
            Bytes = bytes;
            Password = password;
        }

        internal PdfRequest(string filePath, FileStream lifetime, string? password, Action<PdfRequest>? onDispose = null)
        {
            FilePath = filePath;
            Password = password;
            _lifetime = lifetime;
            _onDispose = onDispose;
        }

        internal Guid Id { get; } = Guid.NewGuid();

        internal byte[]? Bytes { get; }

        internal string? FilePath { get; }

        internal string? Password { get; }

        public void Dispose()
        {
            if (FilePath != null && Interlocked.Exchange(ref _disposed, 1) == 0)
            {
                try
                {
                    _lifetime?.Dispose();
                    File.Delete(FilePath);
                }
                finally
                {
                    _onDispose?.Invoke(this);
                }
            }
        }
    }
}