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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace Aaru.Devices.Windows;

[SuppressMessage("ReSharper", "UnusedMember.Global")]
static partial class Extern
{
    [LibraryImport("kernel32.dll",
                   EntryPoint = "CreateFileW",
                   SetLastError = true,
                   StringMarshalling = StringMarshalling.Utf16)]
    internal static partial SafeFileHandle CreateFile([MarshalAs(UnmanagedType.LPTStr)] string filename,
                                                      [MarshalAs(UnmanagedType.U4)]     FileAccess access,
                                                      [MarshalAs(UnmanagedType.U4)]     FileShare share,
                                                      nint securityAttributes, // optional SECURITY_ATTRIBUTES struct or IntPtr.Zero
                                                      [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
                                                      [MarshalAs(UnmanagedType.U4)] FileAttributes flagsAndAttributes,
                                                      nint templateFile);

    [LibraryImport("Kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DeviceIoControlScsi(SafeFileHandle hDevice, WindowsIoctl ioControlCode,
                                                     ref ScsiPassThroughDirectAndSenseBuffer inBuffer,
                                                     uint nInBufferSize,
                                                     ref ScsiPassThroughDirectAndSenseBuffer outBuffer,
                                                     uint nOutBufferSize, ref uint pBytesReturned, nint overlapped);

    [LibraryImport("Kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DeviceIoControlAta(SafeFileHandle           hDevice,        WindowsIoctl ioControlCode,
                                                    ref AtaPassThroughDirect inBuffer,       uint nInBufferSize,
                                                    ref AtaPassThroughDirect outBuffer,      uint nOutBufferSize,
                                                    ref uint                 pBytesReturned, nint overlapped);

    [LibraryImport("Kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DeviceIoControlStorageQuery(SafeFileHandle hDevice, WindowsIoctl ioControlCode,
                                                             ref StoragePropertyQuery inBuffer, uint nInBufferSize,
                                                             nint outBuffer, uint nOutBufferSize,
                                                             ref uint pBytesReturned, nint overlapped);

    [LibraryImport("Kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DeviceIoControlIde(SafeFileHandle           hDevice,        WindowsIoctl ioControlCode,
                                                    ref IdePassThroughDirect inBuffer,       uint nInBufferSize,
                                                    ref IdePassThroughDirect outBuffer,      uint nOutBufferSize,
                                                    ref uint                 pBytesReturned, nint overlapped);

    [LibraryImport("Kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DeviceIoControlGetDeviceNumber(SafeFileHandle hDevice, WindowsIoctl ioControlCode,
                                                                nint inBuffer, uint nInBufferSize,
                                                                ref StorageDeviceNumber outBuffer, uint nOutBufferSize,
                                                                ref uint pBytesReturned, nint overlapped);

    [LibraryImport("Kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DeviceIoControl(SafeFileHandle hDevice, WindowsIoctl ioControlCode, nint inBuffer,
                                                 uint nInBufferSize, ref SffdiskQueryDeviceProtocolData outBuffer,
                                                 uint nOutBufferSize, out uint pBytesReturned, nint overlapped);

    [LibraryImport("Kernel32.dll", EntryPoint = "DeviceIoControl", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool DeviceIoControl(SafeFileHandle hDevice, WindowsIoctl ioControlCode, byte[] inBuffer,
                                                 uint           nInBufferSize, byte[] outBuffer, uint nOutBufferSize,
                                                 out uint       pBytesReturned, nint overlapped);

    [LibraryImport("setupapi.dll", EntryPoint = "SetupDiGetClassDevsW")]
    internal static partial SafeFileHandle SetupDiGetClassDevs(ref Guid classGuid, nint enumerator, nint hwndParent,
                                                               DeviceGetClassFlags flags);

    [LibraryImport("setupapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetupDiEnumDeviceInterfaces(SafeFileHandle          hDevInfo,           nint devInfo,
                                                           ref Guid                interfaceClassGuid, uint memberIndex,
                                                           ref DeviceInterfaceData deviceInterfaceData);

    [LibraryImport("setupapi.dll", EntryPoint = "SetupDiGetDeviceInterfaceDetailW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetupDiGetDeviceInterfaceDetail(SafeFileHandle hDevInfo,
                                                               ref DeviceInterfaceData deviceInterfaceData,
                                                               nint deviceInterfaceDetailData,
                                                               uint deviceInterfaceDetailDataSize,
                                                               ref uint requiredSize, nint deviceInfoData);

    [LibraryImport("setupapi.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetupDiDestroyDeviceInfoList(SafeFileHandle hDevInfo);

    [LibraryImport("Kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static partial bool CloseHandle(SafeFileHandle hDevice);

    [LibraryImport("Kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool SetFilePointerEx(SafeFileHandle hFile, long liDistanceToMove, out long lpNewFilePointer,
                                                MoveMethod     dwMoveMethod);

    [LibraryImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    public static partial bool ReadFile(SafeFileHandle hFile,               byte[] lpBuffer, uint nNumberOfBytesToRead,
                                        out uint       lpNumberOfBytesRead, nint   lpOverlapped);
}