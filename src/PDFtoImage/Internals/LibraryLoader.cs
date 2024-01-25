#if NETFRAMEWORK
using SkiaSharp.Internals;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PDFtoImage.Internals
{
    // Taken and modified from SkiaSharp for loading native libraries in .NET Framework
    // https://github.com/mono/SkiaSharp/blob/9274aeec807fd17eec2a3266ad4c2475c37d8a0c/binding/Binding.Shared/LibraryLoader.cs
    internal static class LibraryLoader
    {
        public static string Extension { get; } = GetExtension();

        private static string GetExtension()
        {
            if (PlatformConfiguration.IsWindows)
                return ".dll";
            else if (PlatformConfiguration.IsMac)
                return ".dylib";
            else
                return ".so";
        }

        public static IntPtr LoadLocalLibrary<T>(string libraryName)
        {
            var libraryPath = GetLibraryPath(libraryName);

            var handle = LoadLibrary(libraryPath!);

            // retry with "lib" prefix when failing to load
            // e.g. pdfium.dll, libpdfium.so, libpdfium.dylib
            if (handle == IntPtr.Zero && !libraryName.StartsWith("lib"))
            {
                libraryPath = GetLibraryPath("lib" + libraryName);
                handle = LoadLibrary(libraryPath!);
            }

            if (handle == IntPtr.Zero)
                throw new DllNotFoundException($"Unable to load library '{libraryName}'.");

            return handle;

            static string? GetLibraryPath(string libraryName)
            {
                var arch = PlatformConfiguration.Is64Bit
                    ? PlatformConfiguration.IsArm ? "arm64" : "x64"
                    : PlatformConfiguration.IsArm ? "arm" : "x86";

                var libWithExt = libraryName;
                if (!libraryName.EndsWith(Extension, StringComparison.OrdinalIgnoreCase))
                    libWithExt += Extension;

                // 1. try alongside managed assembly
                var path = typeof(T).Assembly.Location;
                if (!string.IsNullOrEmpty(path))
                {
                    path = Path.GetDirectoryName(path);
                    if (CheckLibraryPath(path, arch, libWithExt, out var localLib))
                        return localLib;
                }

                // 2. try current directory
                if (CheckLibraryPath(Directory.GetCurrentDirectory(), arch, libWithExt, out var lib))
                    return lib;

                // 3. try app domain
                try
                {
                    if (AppDomain.CurrentDomain is AppDomain domain)
                    {
                        // 3.1 RelativeSearchPath
                        if (CheckLibraryPath(domain.RelativeSearchPath, arch, libWithExt, out lib))
                            return lib;

                        // 3.2 BaseDirectory
                        if (CheckLibraryPath(domain.BaseDirectory, arch, libWithExt, out lib))
                            return lib;
                    }
                }
                catch
                {
                    // no-op as there may not be any domain or path
                }

                // 4. use PATH or default loading mechanism
                return libWithExt;
            }

            static bool CheckLibraryPath(string root, string arch, string libWithExt, out string? foundPath)
            {
                if (!string.IsNullOrEmpty(root))
                {
                    // a. in specific platform sub dir
                    if (!string.IsNullOrEmpty(PlatformConfiguration.LinuxFlavor))
                    {
                        var muslLib = Path.Combine(root, PlatformConfiguration.LinuxFlavor + "-" + arch, libWithExt);
                        if (File.Exists(muslLib))
                        {
                            foundPath = muslLib;
                            return true;
                        }
                    }

                    // b. in generic platform sub dir
                    var searchLib = Path.Combine(root, arch, libWithExt);
                    if (File.Exists(searchLib))
                    {
                        foundPath = searchLib;
                        return true;
                    }

                    // c. in root
                    searchLib = Path.Combine(root, libWithExt);
                    if (File.Exists(searchLib))
                    {
                        foundPath = searchLib;
                        return true;
                    }
                }

                // d. nothing
                foundPath = null;
                return false;
            }
        }

        public static IntPtr LoadLibrary(string libraryName)
        {
            if (string.IsNullOrEmpty(libraryName))
                throw new ArgumentNullException(nameof(libraryName));

            IntPtr handle;
            if (PlatformConfiguration.IsWindows)
                handle = Win32.LoadLibrary(libraryName);
            else if (PlatformConfiguration.IsLinux)
                handle = Linux.dlopen(libraryName);
            else if (PlatformConfiguration.IsMac)
                handle = Mac.dlopen(libraryName);
            else if (RuntimeInformation.OSDescription.StartsWith("Unix"))
                handle = Linux.dlopen(libraryName);
            else
                throw new PlatformNotSupportedException($"Current platform '{RuntimeInformation.OSDescription}' is unknown, unable to load library '{libraryName}'.");

            return handle;
        }

#pragma warning disable IDE1006 // Naming Styles
        private static class Mac
        {
            private const string SystemLibrary = "/usr/lib/libSystem.dylib";

            private const int RTLD_LAZY = 1;
            private const int RTLD_NOW = 2;

            public static IntPtr dlopen(string path, bool lazy = true) =>
                dlopen(path, lazy ? RTLD_LAZY : RTLD_NOW);

            [DllImport(SystemLibrary)]
            public static extern IntPtr dlopen(string path, int mode);
        }

        private static class Linux
        {
            private const string SystemLibrary = "libdl.so";
            private const string SystemLibrary2 = "libdl.so.2"; // newer Linux distros use this

            private const int RTLD_LAZY = 1;
            private const int RTLD_NOW = 2;
            private const int RTLD_DEEPBIND = 8;

            public static IntPtr dlopen(string path, bool lazy = true)
            {
                try
                {
                    return dlopen2(path, (lazy ? RTLD_LAZY : RTLD_NOW) | RTLD_DEEPBIND);
                }
                catch (DllNotFoundException)
                {
                    return dlopen1(path, (lazy ? RTLD_LAZY : RTLD_NOW) | RTLD_DEEPBIND);
                }
            }

            [DllImport(SystemLibrary, EntryPoint = "dlopen")]
            private static extern IntPtr dlopen1(string path, int mode);

            [DllImport(SystemLibrary2, EntryPoint = "dlopen")]
            private static extern IntPtr dlopen2(string path, int mode);
        }

        private static class Win32
        {
            private const string SystemLibrary = "Kernel32.dll";

            [DllImport(SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
            public static extern IntPtr LoadLibrary(string lpFileName);
        }
#pragma warning restore IDE1006 // Naming Styles
    }
}
#endif