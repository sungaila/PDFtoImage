#if NET11_0_OR_GREATER
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Parallel.Internals;
using System;
using System.IO;

namespace PDFtoImage.Tests
{
    [TestClass]
    public sealed class ParallelWorkerLaunchCommandTests
    {
        [TestMethod]
        public void FindApplicationAppHostUsesApplicationBaseDirectory()
        {
            using var applicationDirectory = new TemporaryDirectory();
            using var assemblyDirectory = new TemporaryDirectory();
            var entryAssemblyPath = Path.Combine(assemblyDirectory.Path, "Sample.Worker.dll");
            var appHostPath = Path.Combine(
                applicationDirectory.Path,
                OperatingSystem.IsWindows() ? "Sample.Worker.exe" : "Sample.Worker");
            File.WriteAllText(appHostPath, string.Empty);

            var actual = WorkerLaunchCommand.FindApplicationAppHost(entryAssemblyPath, applicationDirectory.Path);

            Assert.AreEqual(appHostPath, actual);
        }

        [TestMethod]
        public void FindApplicationAppHostReturnsNullWhenApplicationHasNoAppHost()
        {
            using var directory = new TemporaryDirectory();
            var entryAssemblyPath = Path.Combine(directory.Path, "Sample.Worker.dll");

            var actual = WorkerLaunchCommand.FindApplicationAppHost(entryAssemblyPath, directory.Path);

            Assert.IsNull(actual);
        }

        [TestMethod]
        public void ResolveApplicationExecutablePathUsesCurrentDirectoryForRelativePath()
        {
            using var baseDirectory = new TemporaryDirectory();
            using var currentDirectory = new TemporaryDirectory();
            var relativePath = Path.Combine("bin", OperatingSystem.IsWindows() ? "Sample.Worker.exe" : "Sample.Worker");
            var expected = Path.Combine(currentDirectory.Path, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(expected)!);
            File.WriteAllText(expected, string.Empty);

            var actual = WorkerLaunchCommand.ResolveApplicationExecutablePath(relativePath, baseDirectory.Path, currentDirectory.Path);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ResolveApplicationExecutablePathUsesBaseDirectoryForBareExecutableName()
        {
            using var baseDirectory = new TemporaryDirectory();
            using var currentDirectory = new TemporaryDirectory();
            var executableName = OperatingSystem.IsWindows() ? "Sample.Worker.exe" : "Sample.Worker";
            var expected = Path.Combine(baseDirectory.Path, executableName);
            File.WriteAllText(expected, string.Empty);

            var actual = WorkerLaunchCommand.ResolveApplicationExecutablePath(executableName, baseDirectory.Path, currentDirectory.Path);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ResolveApplicationExecutablePathRejectsUnknownRelativeExecutable()
        {
            using var baseDirectory = new TemporaryDirectory();
            using var currentDirectory = new TemporaryDirectory();

            Assert.ThrowsExactly<InvalidOperationException>(() =>
                WorkerLaunchCommand.ResolveApplicationExecutablePath("missing-worker", baseDirectory.Path, currentDirectory.Path));
        }

        private sealed class TemporaryDirectory : IDisposable
        {
            internal TemporaryDirectory()
            {
                Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"PDFtoImage.Tests-{Guid.NewGuid():N}");
                Directory.CreateDirectory(Path);
            }

            internal string Path { get; }

            public void Dispose() => Directory.Delete(Path, recursive: true);
        }
    }
}
#endif
