using System;

namespace PDFtoImage.Parallel.Internals
{
    // Identity belongs to a conversion operation, not the mutable input array.
    // The same request is shared by all page jobs of one async enumeration.
    internal sealed record PdfRequest(byte[] Bytes, string? Password)
    {
        internal Guid Id { get; } = Guid.NewGuid();
    }
}