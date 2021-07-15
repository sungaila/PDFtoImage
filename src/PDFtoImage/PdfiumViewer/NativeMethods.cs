using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace PDFtoImage.PdfiumViewer
{
    internal static partial class NativeMethods
    {
        static NativeMethods()
        {
            // Load the platform dependent Pdfium.dll if it exists.
            LoadNativeLibrary(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!);
        }

        private static string? _pdfiumLibPath;

        private static bool LoadNativeLibrary(string path)
        {
            if (path == null)
                return false;

            _pdfiumLibPath = Path.Combine(path, "runtimes");

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _pdfiumLibPath = Path.Combine(_pdfiumLibPath, (RuntimeInformation.ProcessArchitecture) switch
                {
                    Architecture.X86 => "win-x86",
                    Architecture.X64 => "win-x64",
                    _ => throw new PlatformNotSupportedException("Only x86-64 and x86 are supported on Windows.")
                });
                _pdfiumLibPath = Path.Combine(_pdfiumLibPath, "native");
                _pdfiumLibPath = Path.Combine(_pdfiumLibPath, "pdfium.dll");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _pdfiumLibPath = Path.Combine(_pdfiumLibPath, (RuntimeInformation.ProcessArchitecture) switch
                {
                    Architecture.X64 => "linux-x64",
                    _ => throw new PlatformNotSupportedException("Only x86-64 is supported on Linux.")
                });
                _pdfiumLibPath = Path.Combine(_pdfiumLibPath, "native");
                _pdfiumLibPath = Path.Combine(_pdfiumLibPath, "libpdfium.so");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _pdfiumLibPath = Path.Combine(_pdfiumLibPath, (RuntimeInformation.ProcessArchitecture) switch
                {
                    Architecture.X64 => "osx-x64",
                    Architecture.Arm64 => "osx-arm64",
                    _ => throw new PlatformNotSupportedException("Only x86-64 and arm64 are supported on macOS.")
                });
                _pdfiumLibPath = Path.Combine(_pdfiumLibPath, "native");
                _pdfiumLibPath = Path.Combine(_pdfiumLibPath, "libpdfium.dylib");
            }
            else
            {
                throw new NotSupportedException("Only win-x86, win-x64, linux-x64 and osx-x64 are supported.");
            } 

            NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, ImportResolver);

            return File.Exists(_pdfiumLibPath) && NativeLibrary.Load(_pdfiumLibPath) != IntPtr.Zero;
        }

        private static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (_pdfiumLibPath == null || libraryName != "pdfium.dll")
                return IntPtr.Zero;

            return NativeLibrary.Load(_pdfiumLibPath);
        }
    }
}