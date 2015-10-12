// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Extern.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Windows direct device access
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Contains the P/Invoke definitions of Windows syscalls used to directly interface devices
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

