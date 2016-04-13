// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Extern.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Contains the P/Invoke definitions of FreeBSD syscalls used to directly interface devices
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using System.Runtime.InteropServices;

namespace DiscImageChef.Devices.FreeBSD
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

        [DllImport("libcam", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern IntPtr cam_open_device(
            string path,
            [MarshalAs(UnmanagedType.U4)]
            FileFlags flags);

        [DllImport("libcam", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern void cam_close_device(IntPtr dev);

        [DllImport("libcam", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern IntPtr cam_getccb(IntPtr dev);

        [DllImport("libcam", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern void cam_freeccb(IntPtr ccb);

        [DllImport("libcam", CharSet = CharSet.Ansi, SetLastError = true)]
        internal static extern int cam_send_ccb(IntPtr dev, IntPtr ccb);
    }
}

