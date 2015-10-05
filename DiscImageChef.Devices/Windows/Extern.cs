using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace DiscImageChef.Devices.Windows
{
    static class Extern
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern SafeFileHandle CreateFile(
            [MarshalAs(UnmanagedType.LPTStr)] string filename,
            //            [MarshalAs(UnmanagedType.U4)] FileAccess access,
            uint access,
            //[MarshalAs(UnmanagedType.U4)] FileShare share,
            uint share,
            IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
            //[MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            uint creationDisposition,
            //[MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
            uint flagsAndAttributes,
            IntPtr templateFile);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool DeviceIoControl(
            SafeFileHandle hDevice,
            uint IoControlCode,
            ref Structs.SCSI_PASS_THROUGH_DIRECT_AND_SENSE_BUFFER InBuffer,
            uint nInBufferSize,
            ref Structs.SCSI_PASS_THROUGH_DIRECT_AND_SENSE_BUFFER OutBuffer,
            uint nOutBufferSize,
            ref uint pBytesReturned,
            IntPtr Overlapped
        );

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(SafeFileHandle hDevice);
    }
}

