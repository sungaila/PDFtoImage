namespace PDFtoImage.Parallel
{
    /// <summary>Controls how PDF input and raw bitmap pixels cross process boundaries.</summary>
    public enum ProcessorTransferMode
    {
        /// <summary>
        /// Buffer PDF streams in memory and copy PDFs and bitmap pixels through local IPC pipes.
        /// This is the default and is subject to the IPC message size limit.
        /// </summary>
        Ipc = 0,

        /// <summary>
        /// Buffer each PDF in a temporary file that workers open read-only. Workers write raw bitmap pixels
        /// to temporary file-backed memory maps; the host copies them into returned bitmaps and deletes the files.
        /// Use <see cref="IProcessorOptions.TempDirectory"/> to select the directory.
        /// </summary>
        MemoryMappedFile = 1
    }
}