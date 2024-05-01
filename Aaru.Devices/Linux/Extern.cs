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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Aaru.Devices.Linux;

static partial class Extern
{
    [LibraryImport("libc",
                   SetLastError = true,
                   StringMarshalling = StringMarshalling.Custom,
                   StringMarshallingCustomType = typeof(AnsiStringMarshaller))]
    internal static partial int open(string pathname, [MarshalAs(UnmanagedType.U4)] FileFlags flags);

    [LibraryImport("libc")]
    internal static partial int close(int fd);

    [LibraryImport("libc", EntryPoint = "ioctl", SetLastError = true)]
    internal static partial int ioctlSg(int fd, LinuxIoctl request, ref SgIoHdrT value);

    [LibraryImport("libc", EntryPoint = "ioctl", SetLastError = true)]
    internal static partial int ioctlMmc(int fd, LinuxIoctl request, ref MmcIocCmd value);

    [LibraryImport("libc",
                   SetLastError = true,
                   StringMarshalling = StringMarshalling.Custom,
                   StringMarshallingCustomType = typeof(AnsiStringMarshaller))]
    internal static partial int readlink(string path, nint buf, int bufsize);

    [LibraryImport("libc",
                   EntryPoint = "readlink",
                   SetLastError = true,
                   StringMarshalling = StringMarshalling.Custom,
                   StringMarshallingCustomType = typeof(AnsiStringMarshaller))]
    internal static partial long readlink64(string path, nint buf, long bufsize);

    [LibraryImport("libudev", SetLastError = true)]
    internal static partial nint udev_new();

    [LibraryImport("libudev",
                   SetLastError = true,
                   StringMarshalling = StringMarshalling.Custom,
                   StringMarshallingCustomType = typeof(AnsiStringMarshaller))]
    internal static partial nint udev_device_new_from_subsystem_sysname(nint udev, string subsystem, string sysname);

    [LibraryImport("libudev",
                   SetLastError = true,
                   StringMarshalling = StringMarshalling.Custom,
                   StringMarshallingCustomType = typeof(AnsiStringMarshaller))]
    internal static partial string udev_device_get_property_value(nint udevDevice, string key);

    [LibraryImport("libc", EntryPoint = "ioctl", SetLastError = true)]
    internal static partial int ioctlMmcMulti(int fd, LinuxIoctl request, nint value);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial long lseek(int fd, long offset, SeekWhence whence);

    [LibraryImport("libc", SetLastError = true)]
    internal static partial int read(int fd, byte[] buf, int count);

    [LibraryImport("libc", EntryPoint = "read", SetLastError = true)]
    internal static partial long read64(int fd, byte[] buf, long count);
}