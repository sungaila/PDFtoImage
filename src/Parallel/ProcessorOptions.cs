namespace PDFtoImage.Parallel
{
    /// <summary>
    /// Settings for a <see cref="ParallelPdfProcessor"/>. Properties can be assigned with an object initializer;
    /// the processor reads them once during construction.
    /// </summary>
    public sealed class ProcessorOptions : IProcessorOptions
    {
        /// <summary>
        /// Maximum number of worker processes. Workers start on demand; <see langword="null"/> uses
        /// <see cref="System.Environment.ProcessorCount"/>. A specified value must be positive.
        /// </summary>
        public int? WorkerCount { get; init; }

        /// <summary>
        /// Maximum number of concurrent worker-pool operations for loading and rendering.
        /// Use a positive value for memory backpressure with a large <see cref="WorkerCount"/>;
        /// <see langword="null"/> adds no limit. Cleanup is not queued behind render slots.
        /// IPC input buffering happens before this limit; returned bitmaps are owned by the caller.
        /// </summary>
        public int? SlotCount { get; init; }

        /// <summary>
        /// Transfer method. <see cref="ProcessorTransferMode.Ipc"/> is the default;
        /// <see cref="ProcessorTransferMode.MemoryMappedFile"/> uses temporary PDF files and mapped bitmap files.
        /// </summary>
        public ProcessorTransferMode TransferMode { get; init; }

        /// <summary>
        /// In <see cref="ProcessorTransferMode.MemoryMappedFile"/> mode, reuse a readable, seekable
        /// <see cref="System.IO.FileStream"/> from position zero without copying its PDF to a temporary file.
        /// The file must remain unchanged while it is being rendered. If it cannot be reopened for reading,
        /// the processor copies the stream instead. Defaults to <see langword="true"/> and has no effect
        /// in <see cref="ProcessorTransferMode.Ipc"/> mode.
        /// </summary>
        public bool ReuseFileStream { get; init; } = true;

        /// <summary>
        /// Directory for temporary PDFs and bitmaps in <see cref="ProcessorTransferMode.MemoryMappedFile"/> mode only.
        /// <see langword="null"/> uses <see cref="System.IO.Path.GetTempPath()"/>. A specified directory is created
        /// if needed and is retained after temporary files are deleted. On a multi-user host, choose a directory
        /// that other users cannot modify.
        /// </summary>
        public string? TempDirectory { get; init; }
    }
}