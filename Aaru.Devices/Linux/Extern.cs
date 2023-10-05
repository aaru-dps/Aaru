// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Extern.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Linux direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains the P/Invoke definitions of Linux syscalls used to directly
//     interface devices.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace Aaru.Devices.Linux;

static class Extern
{
    [DllImport("libc", CharSet = CharSet.Ansi, SetLastError = true)]
    internal static extern int open(string pathname, [MarshalAs(UnmanagedType.U4)] FileFlags flags);

    [DllImport("libc")]
    internal static extern int close(int fd);

    [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
    internal static extern int ioctlSg(int fd, LinuxIoctl request, ref SgIoHdrT value);

    [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
    internal static extern int ioctlMmc(int fd, LinuxIoctl request, ref MmcIocCmd value);

    [DllImport("libc", CharSet = CharSet.Ansi, SetLastError = true)]
    internal static extern int readlink(string path, nint buf, int bufsize);

    [DllImport("libc", CharSet = CharSet.Ansi, EntryPoint = "readlink", SetLastError = true)]
    internal static extern long readlink64(string path, nint buf, long bufsize);

    [DllImport("libudev", CharSet = CharSet.Ansi, SetLastError = true)]
    internal static extern nint udev_new();

    [DllImport("libudev", CharSet = CharSet.Ansi, SetLastError = true)]
    internal static extern nint udev_device_new_from_subsystem_sysname(nint udev, string subsystem, string sysname);

    [DllImport("libudev", CharSet = CharSet.Ansi, SetLastError = true)]
    internal static extern string udev_device_get_property_value(nint udevDevice, string key);

    [DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
    internal static extern int ioctlMmcMulti(int fd, LinuxIoctl request, nint value);

    [DllImport("libc", SetLastError = true)]
    internal static extern long lseek(int fd, long offset, SeekWhence whence);

    [DllImport("libc", SetLastError = true)]
    internal static extern int read(int fd, byte[] buf, int count);

    [DllImport("libc", EntryPoint = "read", SetLastError = true)]
    internal static extern long read64(int fd, byte[] buf, long count);
}