using System;

namespace PDFtoImage.Parallel
{
    /// <summary>
    /// Represents an error reported by, or communicating with, a PDF conversion worker process.
    /// </summary>
    public sealed class ParallelConversionException : Exception
    {
        /// <summary>
        /// Gets the remote exception type name or worker failure category.
        /// </summary>
        public string RemoteExceptionType { get; }

        internal ParallelConversionException(
            string remoteExceptionType,
            string message,
            string? remoteStackTrace,
            Exception? innerException = null)
            : base(
                message + (string.IsNullOrWhiteSpace(remoteStackTrace) ? string.Empty : Environment.NewLine + remoteStackTrace),
                innerException)
        {
            RemoteExceptionType = remoteExceptionType;
        }
    }
}
