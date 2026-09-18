using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.System.JobObjects;

namespace PDFtoImage.Parallel.Internals
{
    [SupportedOSPlatform("windows10.0")]
    internal sealed class WindowsJob : IDisposable
    {
        private readonly SafeFileHandle _handle;

        private WindowsJob(SafeFileHandle handle)
        {
            _handle = handle;
        }

        internal SafeFileHandle Handle => _handle;

        internal static WindowsJob Create()
        {
            var handle = ParallelPInvoke.CreateJobObject(null, null);

            if (handle.IsInvalid)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "Could not create a Windows job object.");

            var information = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION();

            information.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;

            var bytes = MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref information, 1));

            if (!ParallelPInvoke.SetInformationJobObject(handle, JOBOBJECTINFOCLASS.JobObjectExtendedLimitInformation, bytes))
            {
                var error = Marshal.GetLastWin32Error();
                handle.Dispose();
                throw new Win32Exception(error, "Could not configure the Windows job object.");
            }

            return new WindowsJob(handle);
        }

        public void Dispose()
        {
            _handle.Dispose();
        }
    }
}