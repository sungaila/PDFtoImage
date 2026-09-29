using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace PDFtoImage.Parallel.Internals
{
    [SupportedOSPlatform("windows10.0")]
    [SupportedOSPlatform("linux")]
    [SupportedOSPlatform("macos")]
    internal static class WorkerBitmapRenderer
    {
        internal static void Render(Stream pipe, WorkerDocument document, int page, RenderOptions options, string? bitmapPath)
        {
            if (bitmapPath == null)
                RenderToIpc(pipe, document, page, options);
            else
                RenderToFile(pipe, document, page, options, bitmapPath);
        }

        private static void RenderToIpc(Stream pipe, WorkerDocument document, int page, RenderOptions options)
        {
            IntPtr pixels = IntPtr.Zero;
            var width = 0;
            var height = 0;
            var rowBytes = 0;

            try
            {
                document.Render(page, options, (renderWidth, renderHeight) =>
                {
                    width = renderWidth;
                    height = renderHeight;
                    rowBytes = checked(width * 4);
                    WorkerProtocol.ValidateIpcBitmapLength(checked(rowBytes * height));
                    pixels = Marshal.AllocHGlobal(rowBytes * height);
                    return (pixels, rowBytes);
                });

                WorkerProtocol.WriteBitmapResponse(pipe, pixels, width, height, rowBytes);
            }
            finally
            {
                Marshal.FreeHGlobal(pixels);
            }
        }

        private static unsafe void RenderToFile(Stream pipe, WorkerDocument document, int page, RenderOptions options, string bitmapPath)
        {
            var width = 0;
            var height = 0;
            var rowBytes = 0;
            var byteCount = 0;

            using (var file = new FileStream(bitmapPath, FileMode.Open, FileAccess.ReadWrite,
                FileShare.Read | FileShare.Delete, 4096, FileOptions.SequentialScan))
            {
                MemoryMappedFile? mapping = null;
                MemoryMappedViewAccessor? view = null;
                var pointerAcquired = false;
                try
                {
                    document.Render(page, options, (renderWidth, renderHeight) =>
                    {
                        width = renderWidth;
                        height = renderHeight;
                        rowBytes = checked(width * 4);
                        byteCount = checked(rowBytes * height);
                        if (byteCount <= 0)
                            throw new InvalidDataException("The rendered bitmap has no pixels.");

                        file.SetLength(byteCount);
                        mapping = MemoryMappedFile.CreateFromFile(file, null, byteCount,
                            MemoryMappedFileAccess.ReadWrite, HandleInheritability.None, leaveOpen: true);
                        view = mapping.CreateViewAccessor(0, byteCount, MemoryMappedFileAccess.Write);
                        byte* pointer = null;
                        view.SafeMemoryMappedViewHandle.AcquirePointer(ref pointer);
                        pointerAcquired = true;
                        return ((IntPtr)(pointer + view.PointerOffset), rowBytes);
                    });
                }
                finally
                {
                    try
                    {
                        if (pointerAcquired)
                            view!.SafeMemoryMappedViewHandle.ReleasePointer();
                    }
                    finally
                    {
                        try
                        {
                            view?.Dispose();
                        }
                        finally
                        {
                            mapping?.Dispose();
                        }
                    }
                }
            }

            WorkerProtocol.WriteMappedBitmapMetadataResponse(pipe, width, height, rowBytes, byteCount);
        }
    }
}