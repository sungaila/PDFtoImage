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

                // Racing callers measure the same page twice and agree, so no lock is needed.
                return _measured[index] ??= _file.GetPDFDocInfo(index);
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
