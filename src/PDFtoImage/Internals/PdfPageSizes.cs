using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;

namespace PDFtoImage.Internals
{
    /// <summary>
    /// The page sizes of a document, each measured the first time it is asked for.
    /// </summary>
    internal sealed class PdfPageSizes : IReadOnlyList<SizeF>
    {
        private readonly PdfFile _file;

        private readonly SizeF?[] _measured;

#if NET9_0_OR_GREATER
        private readonly System.Threading.Lock _syncRoot = new();
#else
        private readonly object _syncRoot = new();
#endif

        public PdfPageSizes(PdfFile file, int count)
        {
            _file = file ?? throw new ArgumentNullException(nameof(file));

            if (count < 0)
                throw new ArgumentOutOfRangeException(nameof(count));

            _measured = new SizeF?[count];
        }

        public int Count => _measured.Length;

        public SizeF this[int index]
        {
            get
            {
                if (index < 0 || index >= _measured.Length)
                    throw new ArgumentOutOfRangeException(nameof(index));

                // SizeF? is a multi-field value type, so reads and writes to the cache are not
                // guaranteed to be atomic. Serialize lazy initialization to keep concurrent reads
                // deterministic; PDFium calls are serialized globally anyway.
                lock (_syncRoot)
                {
                    var measured = _measured[index];
                    if (measured.HasValue)
                        return measured.Value;

                    var size = _file.GetPDFDocInfo(index);
                    _measured[index] = size;
                    return size;
                }
            }
        }

        public IEnumerator<SizeF> GetEnumerator()
        {
            for (int i = 0; i < _measured.Length; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}