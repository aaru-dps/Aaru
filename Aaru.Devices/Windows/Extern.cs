// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Aaru.Devices.Windows;

internal static class Extern
{
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern SafeFileHandle CreateFile([MarshalAs(UnmanagedType.LPTStr)] string filename,
                                                     [MarshalAs(UnmanagedType.U4)] FileAccess access,
                                                     [MarshalAs(UnmanagedType.U4)] FileShare share,
                                                     IntPtr securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
                                                     [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
                                                     [MarshalAs(UnmanagedType.U4)]
                                                     FileAttributes flagsAndAttributes, IntPtr templateFile);

    [DllImport("Kernel32.dll", SetLastError = true, EntryPoint = "DeviceIoControl", CharSet = CharSet.Auto)]
    internal static extern bool DeviceIoControlScsi(SafeFileHandle hDevice, WindowsIoctl ioControlCode,
                                                    ref ScsiPassThroughDirectAndSenseBuffer inBuffer,
                                                    uint nInBufferSize,
                                                    ref ScsiPassThroughDirectAndSenseBuffer outBuffer,
                                                    uint nOutBufferSize, ref uint pBytesReturned,
                                                    IntPtr overlapped);

    [DllImport("Kernel32.dll", SetLastError = true, EntryPoint = "DeviceIoControl", CharSet = CharSet.Auto)]
    internal static extern bool DeviceIoControlAta(SafeFileHandle hDevice, WindowsIoctl ioControlCode,
                                                   ref AtaPassThroughDirect inBuffer, uint nInBufferSize,
                                                   ref AtaPassThroughDirect outBuffer, uint nOutBufferSize,
                                                   ref uint pBytesReturned, IntPtr overlapped);

    [DllImport("Kernel32.dll", SetLastError = true, EntryPoint = "DeviceIoControl", CharSet = CharSet.Auto)]
    internal static extern bool DeviceIoControlStorageQuery(SafeFileHandle hDevice, WindowsIoctl ioControlCode,
                                                            ref StoragePropertyQuery inBuffer, uint nInBufferSize,
                                                            IntPtr outBuffer, uint nOutBufferSize,
                                                            ref uint pBytesReturned, IntPtr overlapped);

    [DllImport("Kernel32.dll", SetLastError = true, EntryPoint = "DeviceIoControl", CharSet = CharSet.Auto)]
    internal static extern bool DeviceIoControlIde(SafeFileHandle hDevice, WindowsIoctl ioControlCode,
                                                   ref IdePassThroughDirect inBuffer, uint nInBufferSize,
                                                   ref IdePassThroughDirect outBuffer, uint nOutBufferSize,
                                                   ref uint pBytesReturned, IntPtr overlapped);

    [DllImport("Kernel32.dll", SetLastError = true, EntryPoint = "DeviceIoControl", CharSet = CharSet.Auto)]
    internal static extern bool DeviceIoControlGetDeviceNumber(SafeFileHandle hDevice, WindowsIoctl ioControlCode,
                                                               IntPtr inBuffer, uint nInBufferSize,
                                                               ref StorageDeviceNumber outBuffer,
                                                               uint nOutBufferSize, ref uint pBytesReturned,
                                                               IntPtr overlapped);

    [DllImport("Kernel32.dll", SetLastError = true, EntryPoint = "DeviceIoControl", CharSet = CharSet.Auto)]
    internal static extern bool DeviceIoControl(SafeFileHandle hDevice, WindowsIoctl ioControlCode, IntPtr inBuffer,
                                                uint nInBufferSize, ref SffdiskQueryDeviceProtocolData outBuffer,
                                                uint nOutBufferSize, out uint pBytesReturned, IntPtr overlapped);

    [DllImport("Kernel32.dll", SetLastError = true, EntryPoint = "DeviceIoControl", CharSet = CharSet.Auto)]
    internal static extern bool DeviceIoControl(SafeFileHandle hDevice, WindowsIoctl ioControlCode, byte[] inBuffer,
                                                uint nInBufferSize, byte[] outBuffer, uint nOutBufferSize,
                                                out uint pBytesReturned, IntPtr overlapped);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto)]
    internal static extern SafeFileHandle SetupDiGetClassDevs(ref Guid classGuid, IntPtr enumerator,
                                                              IntPtr hwndParent, DeviceGetClassFlags flags);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool SetupDiEnumDeviceInterfaces(SafeFileHandle hDevInfo, IntPtr devInfo,
                                                          ref Guid interfaceClassGuid, uint memberIndex,
                                                          ref DeviceInterfaceData deviceInterfaceData);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool SetupDiGetDeviceInterfaceDetail(SafeFileHandle hDevInfo,
                                                              ref DeviceInterfaceData deviceInterfaceData,
                                                              IntPtr deviceInterfaceDetailData,
                                                              uint deviceInterfaceDetailDataSize,
                                                              ref uint requiredSize, IntPtr deviceInfoData);

    [DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool SetupDiDestroyDeviceInfoList(SafeFileHandle hDevInfo);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    internal static extern bool CloseHandle(SafeFileHandle hDevice);

    [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool SetFilePointerEx(SafeFileHandle hFile, long liDistanceToMove,
                                               out long lpNewFilePointer, MoveMethod dwMoveMethod);

    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern bool ReadFile(SafeFileHandle hFile, byte[] lpBuffer, uint nNumberOfBytesToRead,
                                       out uint lpNumberOfBytesRead, IntPtr lpOverlapped);
}