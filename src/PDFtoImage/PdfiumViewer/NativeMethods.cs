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
            // Load the platform dependent Pdfium.dll and libSkiaSharp.dll if they exist.
            LoadNativeLibrary(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().GetName(false).CodeBase!).LocalPath)!);
        }

        private static string? _pdfiumLibPath;
        private static string? _skiaSharpLibPath;

        private static void LoadNativeLibrary(string path)
        {
            if (path == null)
                return;

#if NETCOREAPP3_0_OR_GREATER
            string runtimeIdentifier;
            string pdfiumLibName;
            string skiaSharpLibName;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                runtimeIdentifier = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X86 => "win-x86",
                    Architecture.X64 => "win-x64",
                    Architecture.Arm64 => "win-arm64",
                    _ => throw new PlatformNotSupportedException("Only x86-64, x86 and win-arm64 are supported on Windows.")
                };
                pdfiumLibName = "pdfium.dll";
                skiaSharpLibName = "libSkiaSharp.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                runtimeIdentifier = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X64 => "linux-x64",
                    Architecture.Arm => "linux-arm",
                    Architecture.Arm64 => "linux-arm64",
                    _ => throw new PlatformNotSupportedException("Only x86-64 and arm are supported on Linux.")
                };
                pdfiumLibName = "libpdfium.so";
                skiaSharpLibName = "libSkiaSharp.so";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                runtimeIdentifier = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X64 => "osx-x64",
                    Architecture.Arm64 => "osx-arm64",
                    _ => throw new PlatformNotSupportedException("Only x86-64 and arm64 are supported on macOS.")
                };
                pdfiumLibName = "libpdfium.dylib";
                skiaSharpLibName = "libSkiaSharp.dylib";
            }
            else
            {
                throw new NotSupportedException("Only win-x86, win-x64, win-arm64, linux-x64, linux-arm, linux-arm64, osx-x64 and osx-arm64 are supported.");
            }

            if (File.Exists(Path.Combine(path, pdfiumLibName)))
            {
                // dotnet publish with a given runtime identifier (not portable) will put PDFium into the root folder
                _pdfiumLibPath = Path.Combine(path, pdfiumLibName);
            }
            else
            {
                // in any other case there should be a runtimes folder
                _pdfiumLibPath = Path.Combine(path, "runtimes", runtimeIdentifier, "native", pdfiumLibName);
            }

            if (File.Exists(Path.Combine(path, skiaSharpLibName)))
            {
                // dotnet publish with a given runtime identifier (not portable) will put PDFium into the root folder
                _skiaSharpLibPath = Path.Combine(path, skiaSharpLibName);
            }
            else
            {
                // in any other case there should be a runtimes folder
                _skiaSharpLibPath = Path.Combine(path, "runtimes", runtimeIdentifier, "native", skiaSharpLibName);
            }

            NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, ImportResolver);
            NativeLibrary.SetDllImportResolver(typeof(SkiaSharp.SKBitmap).Assembly, ImportResolver);

            NativeLibrary.Load(_pdfiumLibPath, Assembly.GetExecutingAssembly(), default);
            NativeLibrary.Load(_skiaSharpLibPath, Assembly.GetExecutingAssembly(), default);
#else
            _pdfiumLibPath = Path.Combine(path, "runtimes", Environment.Is64BitProcess ? "win-x64" : "win-x86", "native", "pdfium.dll");
            _skiaSharpLibPath = Path.Combine(path, "runtimes", Environment.Is64BitProcess ? "win-x64" : "win-x86", "native", "libSkiaSharp.dll");

            LoadLibrary(_pdfiumLibPath);
            LoadLibrary(_skiaSharpLibPath);
#endif
        }

#if NETCOREAPP3_0_OR_GREATER
        private static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (_pdfiumLibPath != null && libraryName == "pdfium.dll")
                return NativeLibrary.Load(_pdfiumLibPath, assembly, searchPath);
            else if (_skiaSharpLibPath != null && libraryName == "libSkiaSharp")
                return NativeLibrary.Load(_skiaSharpLibPath, assembly, searchPath);

            return IntPtr.Zero;
        }
#else
        /// <summary>Loads the specified module into the address space of the calling process.</summary>
        /// <param name="lpLibFileName">
        /// <para>The name of the module. This can be either a library module (a .dll file) or an executable module (an .exe file). The name specified is the file name of the module and is not related to the name stored in the library module itself, as specified by the <b>LIBRARY</b> keyword in the module-definition (.def) file. If the string specifies a full path, the function searches only that path for the module. If the string specifies a relative path or a module name without a path, the function uses a standard search strategy to find the module; for more information, see the Remarks. If the function cannot find the  module, the function fails. When specifying a path, be sure to use backslashes (\\), not forward slashes (/). For more information about paths, see <a href="https://docs.microsoft.com/windows/desktop/FileIO/naming-a-file">Naming a File or Directory</a>. If the string specifies a module name without a path and the file name extension is omitted, the function appends the default library extension .dll to the module name. To prevent the function from appending .dll to the module name, include a trailing point character (.) in the module name string.</para>
        /// <para><see href="https://docs.microsoft.com/windows/win32/api//libloaderapi/nf-libloaderapi-loadlibraryw#parameters">Read more on docs.microsoft.com</see>.</para>
        /// </param>
        /// <returns>
        /// <para>If the function succeeds, the return value is a handle to the module. If the function fails, the return value is NULL. To get extended error information, call <a href="/windows/desktop/api/errhandlingapi/nf-errhandlingapi-getlasterror">GetLastError</a>.</para>
        /// </returns>
        /// <remarks>
        /// <para><see href="https://docs.microsoft.com/windows/win32/api//libloaderapi/nf-libloaderapi-loadlibraryw">Learn more about this API from docs.microsoft.com.</see></para>
        /// </remarks>
        [DllImport("Kernel32", ExactSpelling = true, EntryPoint = "LoadLibraryW", SetLastError = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPWStr)] string lpLibFileName);
#endif
    }
}