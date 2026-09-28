namespace PDFtoImage.Parallel
{
    /// <summary>
    /// Settings for a <see cref="ParallelPdfProcessor"/>. The <see cref="IParallelPdfProcessor"/> reads them once during construction.
    /// </summary>
    public sealed record ProcessorOptions
    {
        /// <summary>
        /// Maximum number of worker processes kept by the pool. Workers start on demand.
        /// A positive value is required; <see langword="null"/> uses <see cref="System.Environment.ProcessorCount"/>.
        /// </summary>
        public int? WorkerCount { get; init; }

        /// <summary>
        /// Maximum number of simultaneous operations admitted to the worker pool, including document loading
        /// and page rendering. A positive value adds memory backpressure when <see cref="WorkerCount"/>
        /// is large. <see langword="null"/> adds no limit beyond <see cref="WorkerCount"/>.
        /// Input streams in <see cref="ProcessorTransferMode.Ipc"/> mode are buffered before entering these slots.
        /// Cleanup does not wait for render slots. Returned bitmaps are owned by the caller and are not counted.
        /// </summary>
        public int? SlotCount { get; init; }

        /// <summary>
        /// How PDFs and raw bitmap pixels are exchanged with workers. The default
        /// <see cref="ProcessorTransferMode.Ipc"/> buffers both through local IPC pipes;
        /// <see cref="ProcessorTransferMode.MemoryMappedFile"/> uses temporary PDF files and file-backed bitmap mappings.
        /// </summary>
        public ProcessorTransferMode TransferMode { get; init; }

        /// <summary>
        /// In <see cref="ProcessorTransferMode.MemoryMappedFile"/> mode, reuse a readable, seekable
        /// <see cref="System.IO.FileStream"/> without copying its PDF to a temporary file.
        /// Its current position is ignored; the whole file is rendered from offset zero.
        /// The file must remain unchanged while it is being rendered. If it cannot be reopened for reading,
        /// the processor copies the stream instead. The default is <see langword="true"/>; this setting has
        /// no effect in <see cref="ProcessorTransferMode.Ipc"/> mode.
        /// </summary>
        public bool ReuseFileStream { get; init; } = true;

        /// <summary>
        /// Directory for temporary PDF and bitmap files in <see cref="ProcessorTransferMode.MemoryMappedFile"/> mode only.
        /// <see langword="null"/> uses <see cref="System.IO.Path.GetTempPath()"/>. A specified directory is resolved
        /// to an absolute path and created when the processor is constructed. Temporary files are deleted by the host
        /// when their requests finish or are cancelled; the directory itself is retained. On a multi-user host,
        /// choose a directory that other users cannot modify.
        /// </summary>
        public string? TempDirectory { get; init; }
    }
}