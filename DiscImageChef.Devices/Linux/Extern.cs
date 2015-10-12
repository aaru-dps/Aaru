using System.Runtime.InteropServices;

namespace DiscImageChef.Devices.Linux
{
    static class Extern
    {
        [DllImport("libc", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern int open(
            string pathname,
            [MarshalAs(UnmanagedType.U4)]
            FileFlags flags);

        [DllImport("libc")]
        internal static extern int close(int fd);

        [DllImport("libc", EntryPoint="ioctl", SetLastError = true)]
        internal static extern int ioctlInt(int fd, LinuxIoctl request, out int value);

        [DllImport("libc", EntryPoint="ioctl", SetLastError = true)]
        internal static extern int ioctlSg(int fd, LinuxIoctl request, ref sg_io_hdr_t value);

        [DllImport("libc", EntryPoint="ioctl", SetLastError = true)]
        internal static extern int ioctlHdTaskfile(int fd, LinuxIoctl request, ref hd_drive_task_hdr value);

        [DllImport("libc", EntryPoint="ioctl", SetLastError = true)]
        internal static extern int ioctlHdTask(int fd, LinuxIoctl request, ref byte[] value);
    }
}

