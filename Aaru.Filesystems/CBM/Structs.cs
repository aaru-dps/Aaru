// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Commodore file system plugin.
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

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the filesystem used in 8-bit Commodore microcomputers</summary>
public sealed partial class CBM
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct BAM
    {
        /// <summary>Track where directory starts</summary>
        public readonly byte directoryTrack;
        /// <summary>Sector where directory starts</summary>
        public readonly byte directorySector;
        /// <summary>Disk DOS version, 0x41</summary>
        public readonly byte dosVersion;
        /// <summary>Set to 0x80 if 1571, 0x00 if not</summary>
        public readonly byte doubleSided;
        /// <summary>Block allocation map</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 140)]
        public readonly byte[] bam;
        /// <summary>Disk name</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] name;
        /// <summary>Filled with 0xA0</summary>
        public readonly ushort fill1;
        /// <summary>Disk ID</summary>
        public readonly ushort diskId;
        /// <summary>Filled with 0xA0</summary>
        public readonly byte fill2;
        /// <summary>DOS type</summary>
        public readonly ushort dosType;
        /// <summary>Filled with 0xA0</summary>
        public readonly uint fill3;
        /// <summary>Unused</summary>
        public readonly byte unused1;
        /// <summary>Block allocation map for Dolphin DOS extended tracks</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] dolphinBam;
        /// <summary>Block allocation map for Speed DOS extended tracks</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public readonly byte[] speedBam;
        /// <summary>Unused</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public readonly byte[] unused2;
        /// <summary>Free sector count for second side in 1571</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 9)]
        public readonly byte[] freeCount;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Header
    {
        /// <summary>Track where directory starts</summary>
        public readonly byte directoryTrack;
        /// <summary>Sector where directory starts</summary>
        public readonly byte directorySector;
        /// <summary>Disk DOS version, 0x44</summary>
        public readonly byte diskDosVersion;
        /// <summary>Unusued</summary>
        public readonly byte unused1;
        /// <summary>Disk name</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] name;
        /// <summary>Filled with 0xA0</summary>
        public readonly ushort fill1;
        /// <summary>Disk ID</summary>
        public readonly ushort diskId;
        /// <summary>Filled with 0xA0</summary>
        public readonly byte fill2;
        /// <summary>DOS version ('3')</summary>
        public readonly byte dosVersion;
        /// <summary>Disk version ('D')</summary>
        public readonly byte diskVersion;
        /// <summary>Filled with 0xA0</summary>
        public readonly short fill3;
    }
}