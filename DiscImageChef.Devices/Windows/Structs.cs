// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Windows direct device access.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains structures necessary for directly interfacing devices under
//     Windows.
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

namespace DiscImageChef.Devices.Windows
{
    [StructLayout(LayoutKind.Sequential)]
    struct ScsiPassThroughDirect
    {
        public ushort Length;
        public byte ScsiStatus;
        public byte PathId;
        public byte TargetId;
        public byte Lun;
        public byte CdbLength;
        public byte SenseInfoLength;
        [MarshalAs(UnmanagedType.U1)]
        public ScsiIoctlDirection DataIn;
        public uint DataTransferLength;
        public uint TimeOutValue;
        public IntPtr DataBuffer;
        public uint SenseInfoOffset;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] Cdb;
    };

    [StructLayout(LayoutKind.Sequential)]
    struct ScsiPassThroughDirectAndSenseBuffer
    {
        public ScsiPassThroughDirect sptd;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] SenseBuf;
    }

    struct AtaPassThroughDirect
    {
        public ushort Length;
        public AtaFlags AtaFlags;
        public byte PathId;
        public byte TargetId;
        public byte Lun;
        public byte ReservedAsUchar;
        public uint DataTransferLength;
        public uint TimeOutValue;
        public uint ReservedAsUlong;
        public IntPtr DataBuffer;
        public AtaTaskFile PreviousTaskFile;
        public AtaTaskFile CurrentTaskFile;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct AtaTaskFile
    {
        // Fields for commands sent
        [FieldOffset(0)]
        public byte Features;
        [FieldOffset(6)]
        public byte Command;

        // Fields on command return
        [FieldOffset(0)]
        public byte Error;
        [FieldOffset(6)]
        public byte Status;

        // Common fields
        [FieldOffset(1)]
        public byte SectorCount;
        [FieldOffset(2)]
        public byte SectorNumber;
        [FieldOffset(3)]
        public byte CylinderLow;
        [FieldOffset(4)]
        public byte CylinderHigh;
        [FieldOffset(5)]
        public byte DeviceHead;
        [FieldOffset(7)]
        public byte Reserved;
    }
}

