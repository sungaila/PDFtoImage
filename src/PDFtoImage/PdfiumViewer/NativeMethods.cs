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
            // Load the platform dependent Pdfium.dll if it exist.
            LoadNativeLibrary(Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().GetName(false).CodeBase!).LocalPath)!);
        }

        private static void LoadNativeLibrary(string path)
        {
            if (path == null)
                return;

#if ANDROID
            LoadNativeLibraryAndroid(path);
#elif MONOANDROID
            LoadNativeLibraryMonoAndroid(path);
#elif NETCOREAPP3_0_OR_GREATER
            LoadNativeLibraryNetCore(path);
#elif NETFRAMEWORK
            LoadNativeLibraryNetFX(path);
#else
            throw new PlatformNotSupportedException("Unkown framework and/or platform.");
#endif
        }

#if ANDROID
        private static void LoadNativeLibraryAndroid(string path)
        {
            var runtimeIdentifier = RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.X86 => "android-x86",
                Architecture.X64 => "android-x64",
                Architecture.Arm => "android-arm",
                Architecture.Arm64 => "android-arm64",
                _ => throw new PlatformNotSupportedException("Only x86, x86-64, arm and arm64 are supported on Android.")
            };
            var pdfiumLibName = "libpdfium.so";

            LoadLibrary(path, runtimeIdentifier, pdfiumLibName);
        }
#elif MONOANDROID
        private static void LoadNativeLibraryMonoAndroid(string path)
        {
            switch (RuntimeInformation.ProcessArchitecture)
            {
                case Architecture.X86:
                case Architecture.X64:
                case Architecture.Arm:
                case Architecture.Arm64:
                    break;
                default:
                    throw new PlatformNotSupportedException("Only x86, x86-64, arm and arm64 are supported on Android.");
            }

            Java.Lang.JavaSystem.LoadLibrary("pdfium");
        }
#elif NETCOREAPP3_0_OR_GREATER
        private static void LoadNativeLibraryNetCore(string path)
        {
            string runtimeIdentifier;
            string pdfiumLibName;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                runtimeIdentifier = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X86 => "win-x86",
                    Architecture.X64 => "win-x64",
                    Architecture.Arm64 => "win-arm64",
                    _ => throw new PlatformNotSupportedException("Only x86-64, x86 and arm64 are supported on Windows.")
                };
                pdfiumLibName = "pdfium.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                runtimeIdentifier = RuntimeInformation.ProcessArchitecture switch
                {
                    Architecture.X64 => "linux-x64",
                    Architecture.Arm => "linux-arm",
                    Architecture.Arm64 => "linux-arm64",
                    _ => throw new PlatformNotSupportedException("Only x86-64, arm and arm64 are supported on Linux.")
                };
                pdfiumLibName = "libpdfium.so";
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
            }
            else
            {
                throw new NotSupportedException("Only win-x86, win-x64, win-arm64, linux-x64, linux-arm, linux-arm64, osx-x64, and osx-arm64 are supported.");
            }

            LoadLibrary(path, runtimeIdentifier, pdfiumLibName);
        }
#endif

#if !MONOANDROID
        private static string? _pdfiumLibPath;
#endif

#if NETCOREAPP3_0_OR_GREATER
        private static void LoadLibrary(string path, string runtimeIdentifier, string pdfiumLibName)
        {
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

            NativeLibrary.SetDllImportResolver(typeof(NativeMethods).Assembly, ImportResolver);
            NativeLibrary.Load(_pdfiumLibPath, Assembly.GetExecutingAssembly(), default);
        }

        private static IntPtr ImportResolver(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (_pdfiumLibPath != null && libraryName == "pdfium.dll")
                return NativeLibrary.Load(_pdfiumLibPath, assembly, searchPath);

            return IntPtr.Zero;
        }
#elif NETFRAMEWORK
        private static void LoadNativeLibraryNetFX(string path)
        {
            _pdfiumLibPath = Path.Combine(path, "runtimes", Environment.Is64BitProcess ? "win-x64" : "win-x86", "native", "pdfium.dll");
            LoadLibrary(_pdfiumLibPath);
        }

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