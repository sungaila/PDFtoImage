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

        private readonly bool _deleteOnClose;

        private readonly Lock _gate = new();

        private bool _disposed;

        internal PdfRequest(byte[] bytes, string? password)
        {
            Bytes = bytes;
            Password = password;
        }

        internal PdfRequest(string filePath, FileStream lifetime, string? password, Action<PdfRequest>? onDispose = null, bool deleteOnClose = false)
        {
            FilePath = filePath;
            Password = password;
            _lifetime = lifetime;
            _onDispose = onDispose;
            _deleteOnClose = deleteOnClose;
        }

        internal Guid Id { get; } = Guid.NewGuid();

        internal byte[]? Bytes { get; }

        internal string? FilePath { get; }

        internal string? Password { get; }

        public void Dispose()
        {
            lock (_gate)
            {
                if (FilePath == null || _disposed)
                    return;

                try
                {
                    _lifetime?.Dispose();
                }
                finally
                {
                    // Windows may still have a worker handle on a delete-pending file.
                    // DeleteOnClose already owns deletion; a second DeleteFile can fail.
                    if (!_deleteOnClose)
                        File.Delete(FilePath);
                }

                // Keep failed deletions registered for a later cleanup attempt.
                _disposed = true;
                _onDispose?.Invoke(this);
            }
        }
    }
}