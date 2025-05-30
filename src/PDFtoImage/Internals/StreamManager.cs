using System;
using System.Collections.Generic;
using System.IO;

namespace PDFtoImage.Internals
{
#if NET8_0_OR_GREATER
#pragma warning disable CA1510 // Use ArgumentNullException throw helper
#endif
    internal static class StreamManager
    {
#if NET9_0_OR_GREATER
        private static readonly System.Threading.Lock _syncRoot = new();
#else
        private static readonly object _syncRoot = new();
#endif
        private static int _nextId = 1;
        private static readonly Dictionary<int, Stream> _files = [];

        public static int Register(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            lock (_syncRoot)
            {
                int id = _nextId++;
                _files.Add(id, stream);
                return id;
            }
        }

        public static void Unregister(int id)
        {
            lock (_syncRoot)
            {
                _files.Remove(id);
            }
        }

        public static Stream? Get(int id)
        {
            lock (_syncRoot)
            {
                _files.TryGetValue(id, out Stream? stream);
                return stream;
            }
        }
    }
}