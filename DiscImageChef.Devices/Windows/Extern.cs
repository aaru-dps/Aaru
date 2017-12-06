// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Extern.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Windows direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains the P/Invoke definitions of Windows syscalls used to directly
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/


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

        [DllImport("Kernel32.dll", SetLastError = true, EntryPoint = "DeviceIoControl", CharSet = CharSet.Auto)]
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

        [DllImport("Kernel32.dll", SetLastError = true, EntryPoint = "DeviceIoControl", CharSet = CharSet.Auto)]
        internal static extern bool DeviceIoControlAta(
            SafeFileHandle hDevice,
            WindowsIoctl IoControlCode,
            ref AtaPassThroughDirectWithBuffer InBuffer,
            uint nInBufferSize,
            ref AtaPassThroughDirectWithBuffer OutBuffer,
            uint nOutBufferSize,
            ref uint pBytesReturned,
            IntPtr Overlapped
        );

        [DllImport("Kernel32.dll", SetLastError = true, EntryPoint = "DeviceIoControl", CharSet = CharSet.Auto)]
        internal static extern bool DeviceIoControlStorageQuery(
           SafeFileHandle hDevice,
           WindowsIoctl IoControlCode,
           ref StoragePropertyQuery InBuffer,
           uint nInBufferSize,
           IntPtr OutBuffer,
           uint nOutBufferSize,
           ref uint pBytesReturned,
           IntPtr Overlapped
        );

        [DllImport("Kernel32.dll", SetLastError = true, EntryPoint = "DeviceIoControl", CharSet = CharSet.Auto)]
        internal static extern bool DeviceIoControlIde(
            SafeFileHandle hDevice,
            WindowsIoctl IoControlCode,
            ref IdePassThroughDirect InBuffer,
            uint nInBufferSize,
            ref IdePassThroughDirect OutBuffer,
            uint nOutBufferSize,
            ref uint pBytesReturned,
            IntPtr Overlapped
        );


        [DllImport("Kernel32.dll", SetLastError = true, EntryPoint = "DeviceIoControl", CharSet = CharSet.Auto)]
        internal static extern bool DeviceIoControlGetDeviceNumber(
            SafeFileHandle hDevice,
            WindowsIoctl IoControlCode,
            IntPtr InBuffer,
            uint nInBufferSize,
            ref StorageDeviceNumber OutBuffer,
            uint nOutBufferSize,
            ref uint pBytesReturned,
            IntPtr Overlapped
        );

        [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
        internal static extern SafeFileHandle SetupDiGetClassDevs(
            ref Guid ClassGuid,
            IntPtr Enumerator,
            IntPtr hwndParent,
            DeviceGetClassFlags Flags
        );
        
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiEnumDeviceInterfaces(
            SafeFileHandle hDevInfo,
            IntPtr devInfo,
            ref Guid interfaceClassGuid,
            uint memberIndex,
            ref DeviceInterfaceData deviceInterfaceData
        );
        
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiGetDeviceInterfaceDetail(
            SafeFileHandle hDevInfo,
            ref DeviceInterfaceData deviceInterfaceData,
            IntPtr deviceInterfaceDetailData,
            UInt32 deviceInterfaceDetailDataSize,
            ref UInt32 requiredSize,
            IntPtr deviceInfoData
        );
        
        [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetupDiDestroyDeviceInfoList(SafeFileHandle hDevInfo);

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        internal static extern bool CloseHandle(SafeFileHandle hDevice);
    }
}

