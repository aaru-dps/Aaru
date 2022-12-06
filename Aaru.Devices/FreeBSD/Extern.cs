// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Extern.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FreeBSD direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains the P/Invoke definitions of FreeBSD syscalls used to directly
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

using System;
using System.Runtime.InteropServices;

namespace Aaru.Devices.FreeBSD
{
    [Obsolete]
    internal static class Extern
    {
        [DllImport("libc", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern int open(string pathname, [MarshalAs(UnmanagedType.U4)] FileFlags flags);

        [DllImport("libc")]
        internal static extern int close(int fd);

        [DllImport("libcam", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern IntPtr cam_open_device(string path, [MarshalAs(UnmanagedType.U4)] FileFlags flags);

        [DllImport("libcam", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern void cam_close_device(IntPtr dev);

        [DllImport("libcam", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern IntPtr cam_getccb(IntPtr dev);

        [DllImport("libcam", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern void cam_freeccb(IntPtr ccb);

        [DllImport("libcam", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern int cam_send_ccb(IntPtr dev, IntPtr ccb);

        [DllImport("libc")]
        internal static extern int ioctl(int fd, FreebsdIoctl request, IntPtr argp);
    }
}