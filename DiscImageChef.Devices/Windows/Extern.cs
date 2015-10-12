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
            [MarshalAs(UnmanagedType.U4)] FileAccess access,
            [MarshalAs(UnmanagedType.U4)] FileShare share,
            IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
            IntPtr templateFile);

        [DllImport("Kernel32.dll", SetLastError = true, EntryPoint="ioctl", CharSet = CharSet.Auto)]
        internal static extern bool DeviceIoControlScsi(
            SafeFileHandle hDevice,
            WindowsIoctl IoControlCode,
            ref ScsiPassThroughDirectAndSenseBuffer InBuffer,
            uint nInBufferSize,
            ref ScsiPassThroughDirectAndSenseBuffer OutBuffer,
            uint nOutBufferSize,
            ref uint pBytesReturned,
            IntPtr Overlapped
        );

        [DllImport("Kernel32.dll", SetLastError = true, EntryPoint="ioctl", CharSet = CharSet.Auto)]
        internal static extern bool DeviceIoControlAta(
            SafeFileHandle hDevice,
            WindowsIoctl IoControlCode,
            ref AtaPassThroughDirect InBuffer,
            uint nInBufferSize,
            ref AtaPassThroughDirect OutBuffer,
            uint nOutBufferSize,
            ref uint pBytesReturned,
            IntPtr Overlapped
        );

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool CloseHandle(SafeFileHandle hDevice);
    }
}

