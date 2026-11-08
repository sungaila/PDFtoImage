using Microsoft.VisualStudio.TestTools.UnitTesting;
using PDFtoImage.Exceptions;
using PDFtoImage.Internals;
using System;
using System.IO;
using System.Reflection;
#if NETFRAMEWORK
using System.Diagnostics;
#endif

namespace PDFtoImage.Tests
{
    [TestClass, DoNotParallelize]
    public sealed class NativeInteropTests : TestBase
    {
        private static byte[] ReadPdf() => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "SocialPreview.pdf"));

        [TestMethod]
        [DataRow(false)]
        [DataRow(true)]
        public void FailedNativeCallDoesNotReuseDocumentError(bool createBitmap)
        {
            using var file = new PdfFile(new MemoryStream(ReadPdf()), null, disposeStream: true);
            var document = (IntPtr)typeof(PdfFile).GetField("_document", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(file)!;
            using var encrypted = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "..", "Assets", "SocialPreview with password 123456 (AES-256).pdf"));
            Assert.ThrowsExactly<PdfPasswordProtectedException>(() => Conversion.GetPageCount(encrypted, leaveOpen: true));

            var handle = createBitmap
                ? NativeMethods.Bitmap_CreateEx(0, 1, NativeMethods.FPDFBitmap.BGRA, IntPtr.Zero, 0, out NativeMethods.FPDF_ERR error)
                : NativeMethods.LoadPage(document, int.MaxValue, out error);

            Assert.AreEqual(IntPtr.Zero, handle);
            Assert.AreEqual(NativeMethods.FPDF_ERR.UNKNOWN, error);
            Assert.IsInstanceOfType<PdfUnknownException>(PdfException.CreateException(error));
        }

        [TestMethod]
        public void FailedPageLoadThrowsGenericPdfException()
        {
            using var file = new PdfFile(new MemoryStream(ReadPdf()), null, disposeStream: true);
            Assert.ThrowsExactly<PdfUnknownException>(() =>
                file.RenderPDFPageToBitmap(int.MaxValue, IntPtr.Zero, 0, 0, 1, 1, 0, default, renderFormFill: false));
        }

        [TestMethod]
        public void ExplicitLibraryDisposalAllowsReload()
        {
            var singleton = typeof(PdfLibrary).GetField("_library", BindingFlags.Static | BindingFlags.NonPublic)!;
            Assert.AreEqual(1, Conversion.GetPageCount(new MemoryStream(ReadPdf())));
            var first = (PdfLibrary)singleton.GetValue(null)!;

            first.Dispose();
            first.Dispose();
            Assert.AreEqual(1, Conversion.GetPageCount(new MemoryStream(ReadPdf())));
            Assert.AreNotSame(first, singleton.GetValue(null));

            first.Dispose();
            Assert.AreEqual(1, Conversion.GetPageCount(new MemoryStream(ReadPdf())));
        }

#if NETFRAMEWORK
        [TestMethod]
        public void AppDomainUnloadDoesNotDestroySharedLibrary()
        {
            const string childVariable = "PDFTOIMAGE_TEST_APPDOMAIN_UNLOAD";
            if (Environment.GetEnvironmentVariable(childVariable) == "1")
            {
                var pdf = ReadPdf();
                Assert.AreEqual(1, Conversion.GetPageCount(new MemoryStream(pdf)));
                var domain = AppDomain.CreateDomain("PDFium unload regression", null,
                    new AppDomainSetup { ApplicationBase = AppContext.BaseDirectory });
                try
                {
                    var reader = (DomainPdfReader)domain.CreateInstanceAndUnwrap(
                        typeof(DomainPdfReader).Assembly.FullName, typeof(DomainPdfReader).FullName);
                    Assert.AreEqual(1, reader.GetPageCount(pdf));
                }
                finally
                {
                    AppDomain.Unload(domain);
                }
                Assert.AreEqual(1, Conversion.GetPageCount(new MemoryStream(pdf)));
                return;
            }

            // A regression can crash in native code. Keep it out of the parent test process.
            var start = new ProcessStartInfo(Assembly.GetExecutingAssembly().Location)
            {
                Arguments = "--filter FullyQualifiedName=PDFtoImage.Tests.NativeInteropTests.AppDomainUnloadDoesNotDestroySharedLibrary --output Minimal --progress off",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            start.EnvironmentVariables[childVariable] = "1";
            using var process = Process.Start(start)!;
            var output = process.StandardOutput.ReadToEndAsync();
            var errorOutput = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(20000))
            {
                process.Kill();
                Assert.Fail("AppDomain unload test timed out.");
            }
            Assert.AreEqual(0, process.ExitCode, output.GetAwaiter().GetResult() + errorOutput.GetAwaiter().GetResult());
        }

        public sealed class DomainPdfReader : MarshalByRefObject
        {
            public int GetPageCount(byte[] pdf) => Conversion.GetPageCount(new MemoryStream(pdf));
        }
#endif
    }
}