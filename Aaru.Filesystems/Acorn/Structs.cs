// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Acorn filesystem plugin.
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
/// <summary>Implements detection of Acorn's Advanced Data Filing System (ADFS)</summary>
public sealed partial class AcornADFS
{
#region Nested type: BootBlock

    /// <summary>Boot block, used in hard disks and ADFS-F and higher.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct BootBlock
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x1C0)]
        public readonly byte[] spare;
        public readonly DiscRecord discRecord;
        public readonly byte       flags;
        public readonly ushort     startCylinder;
        public readonly byte       checksum;
    }

#endregion

#region Nested type: DirectoryEntry

    /// <summary>Directory header, common to "old" and "new" directories</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct DirectoryEntry
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public readonly byte[] name;
        public readonly uint load;
        public readonly uint exec;
        public readonly uint length;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] address;
        public readonly byte atts;
    }

#endregion

#region Nested type: DirectoryHeader

    /// <summary>Directory header, common to "old" and "new" directories</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct DirectoryHeader
    {
        public readonly byte masterSequence;
        public readonly uint magic;
    }

#endregion

#region Nested type: DiscRecord

    /// <summary>Disc record, used in hard disks and ADFS-E and higher.</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct DiscRecord
    {
        public readonly byte   log2secsize;
        public readonly byte   spt;
        public readonly byte   heads;
        public readonly byte   density;
        public readonly byte   idlen;
        public readonly byte   log2bpmb;
        public readonly byte   skew;
        public readonly byte   bootoption;
        public readonly byte   lowsector;
        public readonly byte   nzones;
        public readonly ushort zone_spare;
        public readonly uint   root;
        public readonly uint   disc_size;
        public readonly ushort disc_id;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public readonly byte[] disc_name;
        public readonly uint disc_type;
        public readonly uint disc_size_high;
        public readonly byte flags;
        public readonly byte nzones_high;
        public readonly uint format_version;
        public readonly uint root_size;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] reserved;
    }

#endregion

#region Nested type: NewDirectory

    /// <summary>Directory, new format</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct NewDirectory
    {
        public readonly DirectoryHeader header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 77)]
        public readonly DirectoryEntry[] entries;
        public readonly NewDirectoryTail tail;
    }

#endregion

#region Nested type: NewDirectoryTail

    /// <summary>Directory tail, new format</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct NewDirectoryTail
    {
        public readonly byte   lastMark;
        public readonly ushort reserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] parent;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
        public readonly byte[] title;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public readonly byte[] name;
        public readonly byte endMasSeq;
        public readonly uint magic;
        public readonly byte checkByte;
    }

#endregion

#region Nested type: NewMap

    /// <summary>Free block map, sector 0, used in ADFS-E</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct NewMap
    {
        public readonly byte       zoneChecksum;
        public readonly ushort     freeLink;
        public readonly byte       crossChecksum;
        public readonly DiscRecord discRecord;
    }

#endregion

#region Nested type: OldDirectory

    /// <summary>Directory, old format</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct OldDirectory
    {
        public readonly DirectoryHeader header;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 47)]
        public readonly DirectoryEntry[] entries;
        public readonly OldDirectoryTail tail;
    }

#endregion

#region Nested type: OldDirectoryTail

    /// <summary>Directory tail, old format</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct OldDirectoryTail
    {
        public readonly byte lastMark;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public readonly byte[] name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] parent;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 19)]
        public readonly byte[] title;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 14)]
        public readonly byte[] reserved;
        public readonly byte endMasSeq;
        public readonly uint magic;
        public readonly byte checkByte;
    }

#endregion

#region Nested type: OldMapSector0

    /// <summary>Free block map, sector 0, used in ADFS-S, ADFS-L, ADFS-M and ADFS-D</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct OldMapSector0
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 82 * 3)]
        public readonly byte[] freeStart;
        public readonly byte reserved;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public readonly byte[] name;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] size;
        public readonly byte checksum;
    }

#endregion

#region Nested type: OldMapSector1

    /// <summary>Free block map, sector 1, used in ADFS-S, ADFS-L, ADFS-M and ADFS-D</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct OldMapSector1
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 82 * 3)]
        public readonly byte[] freeStart;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public readonly byte[] name;
        public readonly ushort discId;
        public readonly byte   boot;
        public readonly byte   freeEnd;
        public readonly byte   checksum;
    }

#endregion
}