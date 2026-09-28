namespace PDFtoImage.Parallel
{
    /// <summary>Provides the settings used when a <see cref="ParallelPdfProcessor"/> is created.</summary>
    public interface IProcessorOptions
    {
        /// <summary>
        /// Maximum number of worker processes kept by the pool. Workers start on demand.
        /// A positive value is required; <see langword="null"/> uses <see cref="System.Environment.ProcessorCount"/>.
        /// </summary>
        int? WorkerCount { get; init; }

        /// <summary>
        /// Maximum number of simultaneous operations admitted to the worker pool, including document loading
        /// and page rendering. A positive value adds memory backpressure when <see cref="WorkerCount"/>
        /// is large. <see langword="null"/> adds no limit beyond <see cref="WorkerCount"/>.
        /// Input streams in <see cref="ProcessorTransferMode.Ipc"/> mode are buffered before entering these slots.
        /// Cleanup does not wait for render slots. Returned bitmaps are owned by the caller and are not counted.
        /// </summary>
        int? SlotCount { get; init; }

        /// <summary>
        /// How PDFs and raw bitmap pixels are exchanged with workers. The default
        /// <see cref="ProcessorTransferMode.Ipc"/> buffers both through local IPC pipes;
        /// <see cref="ProcessorTransferMode.MemoryMappedFile"/> uses temporary PDF files and file-backed bitmap mappings.
        /// </summary>
        ProcessorTransferMode TransferMode { get; init; }

        /// <summary>
        /// Directory for temporary PDF and bitmap files in <see cref="ProcessorTransferMode.MemoryMappedFile"/> mode only.
        /// <see langword="null"/> uses <see cref="System.IO.Path.GetTempPath()"/>. A specified directory is resolved
        /// to an absolute path and created when the processor is constructed. Temporary files are deleted by the host
        /// when their requests finish or are cancelled; the directory itself is retained.
        /// </summary>
        string? TempDirectory { get; init; }
    }
}