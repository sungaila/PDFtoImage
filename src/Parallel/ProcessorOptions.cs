namespace PDFtoImage.Parallel
{
    /// <summary>
    /// Settings for a <see cref="ParallelPdfProcessor"/>. Properties can be assigned with an object initializer;
    /// the processor reads them once during construction.
    /// </summary>
    public sealed class ProcessorOptions : IProcessorOptions
    {
        /// <inheritdoc />
        public int? WorkerCount { get; init; }

        /// <inheritdoc />
        public int? SlotCount { get; init; }

        /// <inheritdoc />
        public ProcessorTransferMode TransferMode { get; init; }

        /// <inheritdoc />
        public bool ReuseFileStream { get; init; } = true;

        /// <inheritdoc />
        public string? TempDirectory { get; init; }
    }
}