// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : High Performance Optical File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     On-disk structures for the High Performance Optical File System.
//
//     All structures except the boot sector are big-endian. Every field here
//     was verified both against OS/2 HPOFS20.IFS / UHPOFS.DLL code and against
//     real volumes; fields whose meaning is not known are named unknown or
//     reserved.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Aaru.CommonTypes.Attributes;

namespace Aaru.Filesystems;

[SuppressMessage("ReSharper", "UnusedType.Local")]
[SuppressMessage("ReSharper", "UnusedMember.Local")]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public sealed partial class HPOFS
{
#region Nested type: BiosParameterBlock

    /// <summary>Boot sector at LBA 0 (copy at LBA 0x7F), little-endian, 512 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct BiosParameterBlock
    {
        /// <summary>0x000, Jump to boot code (EB 00 90)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] jump;
        /// <summary>0x003, OEM name ("IBM 10.2")</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] oem_name;
        /// <summary>0x00B, Bytes per sector</summary>
        public readonly ushort bps;
        /// <summary>0x00D, Sectors per cluster</summary>
        public readonly byte spc;
        /// <summary>0x00E, Reserved sectors</summary>
        public readonly ushort rsectors;
        /// <summary>0x010, Number of FATs (0)</summary>
        public readonly byte fats_no;
        /// <summary>0x011, Root directory entries</summary>
        public readonly ushort root_ent;
        /// <summary>0x013, Total sectors, if it fits in 16 bits</summary>
        public readonly ushort sectors;
        /// <summary>0x015, Media descriptor</summary>
        public readonly byte media;
        /// <summary>0x016, Sectors per FAT (0)</summary>
        public readonly ushort spfat;
        /// <summary>0x018, Sectors per track</summary>
        public readonly ushort sptrk;
        /// <summary>0x01A, Heads</summary>
        public readonly ushort heads;
        /// <summary>0x01C, Hidden sectors</summary>
        public readonly uint hsectors;
        /// <summary>0x020, Total sectors, 32 bits</summary>
        public readonly uint big_sectors;
        /// <summary>0x024, BIOS drive number</summary>
        public readonly byte drive_no;
        /// <summary>0x025, Reserved</summary>
        public readonly byte nt_flags;
        /// <summary>0x026, Extended BPB signature (0x29)</summary>
        public readonly byte signature;
        /// <summary>0x027, Serial number (not the HPOFS volume serial)</summary>
        public readonly uint serial_no;
        /// <summary>0x02B, Volume label (not the HPOFS volume label)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public readonly byte[] volume_label;
        /// <summary>0x036, Filesystem type, "HPOFS\0\0\0"</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] fs_type;
        /// <summary>0x03E, Boot code</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 442)]
        public readonly byte[] boot_code;
        /// <summary>0x1F8, Trailer (2F F8 00 00 01 00)</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public readonly byte[] trailer;
        /// <summary>0x1FE, Boot signature (0xAA55)</summary>
        public readonly ushort signature2;
    }

#endregion

#region Nested type: MediaInformationBlock

    /// <summary>
    ///     Media information, at LBA 13 (copy at LBA 0x8C). Signature "MEDINFO " on rewritable volumes, "SMISUBCL" on
    ///     WORM volumes, which keep these same fields at the same offsets. 512 bytes.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct MediaInformationBlock
    {
        /// <summary>0x000, Signature, "MEDINFO " or "SMISUBCL"</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] blockId;
        /// <summary>0x008, Volume label, in volume codepage, space padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] volumeLabel;
        /// <summary>0x028, Volume comment, in volume codepage, space padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 160)]
        public byte[] comment;
        /// <summary>0x0C8, Volume serial number</summary>
        public uint serial;
        /// <summary>0x0CC, Creation date, DOS format</summary>
        public ushort creationDate;
        /// <summary>0x0CE, Creation time, DOS format</summary>
        public ushort creationTime;
        /// <summary>0x0D0, Codepage type, 1 = ASCII, 2 = EBCDIC</summary>
        public ushort codepageType;
        /// <summary>0x0D2, Codepage</summary>
        public ushort codepage;
        /// <summary>0x0D4, RPS level (1 and 257 WORM, 10 and 266 rewritable, 257 and 266 preserve case)</summary>
        public uint rps;
        /// <summary>0x0D8, Bytes per sector</summary>
        public ushort bps;
        /// <summary>0x0DA, Bytes per sector, again</summary>
        public ushort bps2;
        /// <summary>0x0DC, Reserved</summary>
        public uint reserved_dc;
        /// <summary>0x0E0, Total sectors on volume</summary>
        public uint sectors;
        /// <summary>0x0E4, Unknown, 0x200 in all seen volumes</summary>
        public uint unknown_e4;
        /// <summary>0x0E8, Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] reserved_e8;
        /// <summary>0x0F0, Format version major</summary>
        public ushort major;
        /// <summary>0x0F2, Format version minor</summary>
        public ushort minor;
        /// <summary>0x0F4, Reserved</summary>
        public uint reserved_f4;
        /// <summary>0x0F8, Unknown, 0x2080 on 230Mb media, 0x4080 on floppies</summary>
        public ushort unknown_f8;
        /// <summary>0x0FA, Reserved</summary>
        public ushort reserved_fa;
        /// <summary>0x0FC, Rest of the sector, holds WORM-specific fields when signature is "SMISUBCL"</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
        public byte[] rest;
    }

#endregion

#region Nested type: VolumeInformationBlock

    /// <summary>Volume information, at LBA 14 (copy at LBA 0x8D), rewritable volumes only, 512 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct VolumeInformationBlock
    {
        /// <summary>0x000, Signature, "VOLINFO "</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] blockId;
        /// <summary>0x008, Unknown</summary>
        public uint unknown_08;
        /// <summary>0x00C, Unknown</summary>
        public uint unknown_0c;
        /// <summary>0x010, Directory intent counter</summary>
        public uint dirIntentCount;
        /// <summary>0x014, Directory update counter</summary>
        public uint dirUpdateCount;
        /// <summary>0x018, Reserved, space padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
        public byte[] reserved_18;
        /// <summary>0x048, Volume owner, in volume codepage, space padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public byte[] owner;
        /// <summary>0x068, Reserved, space padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
        public byte[] reserved_68;
        /// <summary>0x07C, Maximum percentage of the volume that can be filled</summary>
        public ushort percentFull;
        /// <summary>0x07E, Reserved</summary>
        public ushort reserved_7e;
        /// <summary>0x080, Rest of the sector</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 384)]
        public byte[] rest;
    }

#endregion

#region Nested type: MasterRecord

    /// <summary>Master record ("MAST"), node 0 of a B-tree file, first 0x9C bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct MasterRecord
    {
        /// <summary>0x000, Signature, "MAST"</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] signature;
        /// <summary>0x004, Unknown</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] unknown_04;
        /// <summary>0x010, Size of a node (control interval) in bytes</summary>
        public ushort nodeSize;
        /// <summary>0x012, Unknown</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 126)]
        public byte[] unknown_12;
        /// <summary>0x090, Nodes in use</summary>
        public uint usedNodes;
        /// <summary>0x094, Root index node</summary>
        public uint rootNode;
        /// <summary>0x098, First leaf node</summary>
        public uint firstLeaf;
    }

#endregion

#region Nested type: NodeHeader

    /// <summary>Header of a B-tree node ("INDX" or "DATA"), 0x24 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct NodeHeader
    {
        /// <summary>0x000, Signature, "INDX", "DATA" or "BITM"</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] signature;
        /// <summary>0x004, Reserved</summary>
        public uint reserved_04;
        /// <summary>0x008, Parent index node</summary>
        public uint parent;
        /// <summary>0x00C, Previous leaf node</summary>
        public uint previous;
        /// <summary>0x010, Next leaf node, 0 if last</summary>
        public uint next;
        /// <summary>0x014, Level, 0 for leaves</summary>
        public ushort level;
        /// <summary>0x016, Number of entries</summary>
        public ushort entries;
        /// <summary>0x018, Bytes used</summary>
        public ushort used;
        /// <summary>0x01A, Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public byte[] reserved_1a;
    }

#endregion

#region Nested type: FileRecord

    /// <summary>
    ///     Header of a file record, as found in directory leaf entries and system file sectors, 0x40 bytes. Followed by
    ///     up to three <see cref="SubfileDescriptor" />.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct FileRecord
    {
        /// <summary>0x000, File identifier</summary>
        public uint fileId;
        /// <summary>0x004, File identifier, again</summary>
        public uint fileId2;
        /// <summary>0x008, Unknown, 1 in all seen volumes</summary>
        public uint unknown_08;
        /// <summary>0x00C, Flags, bits 13-14 are the number of subfiles</summary>
        public ushort flags;
        /// <summary>0x00E, OS/2 attributes</summary>
        public byte attributes;
        /// <summary>0x00F, Reserved</summary>
        public byte reserved_0f;
        /// <summary>0x010, Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] reserved_10;
        /// <summary>0x01C, Creation date, DOS format</summary>
        public ushort creationDate;
        /// <summary>0x01E, Creation time, DOS format</summary>
        public ushort creationTime;
        /// <summary>0x020, Last write date, DOS format</summary>
        public ushort writeDate;
        /// <summary>0x022, Last write time, DOS format</summary>
        public ushort writeTime;
        /// <summary>0x024, Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)]
        public byte[] reserved_24;
    }

#endregion

#region Nested type: SubfileDescriptor

    /// <summary>Describes the allocation of a subfile (data, extended attributes...), 0x34 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct SubfileDescriptor
    {
        /// <summary>0x000, Sectors in first extent</summary>
        public ushort firstSectors;
        /// <summary>0x002, First extent LBA, high 16 bits</summary>
        public ushort firstLbaHigh;
        /// <summary>0x004, First extent LBA, low 32 bits</summary>
        public uint firstLbaLow;
        /// <summary>0x008, Sectors in second extent, 0 if <see cref="lists" /> is not 0</summary>
        public ushort secondSectors;
        /// <summary>0x00A, Second extent, or first extent list, LBA, high 16 bits</summary>
        public ushort secondLbaHigh;
        /// <summary>0x00C, Second extent, or first extent list, LBA, low 32 bits</summary>
        public uint secondLbaLow;
        /// <summary>0x010, Number of extent list blocks</summary>
        public uint lists;
        /// <summary>0x014, Reserved</summary>
        public uint reserved_14;
        /// <summary>0x018, Size in bytes</summary>
        public uint size;
        /// <summary>0x01C, Reserved</summary>
        public uint reserved_1c;
        /// <summary>0x020, Allocated bytes</summary>
        public uint allocated;
        /// <summary>0x024, Reserved</summary>
        public uint reserved_24;
        /// <summary>0x028, Unknown</summary>
        public uint unknown_28;
        /// <summary>0x02C, Reserved</summary>
        public uint reserved_2c;
        /// <summary>0x030, Unknown</summary>
        public uint unknown_30;
    }

#endregion

#region Nested type: ExtentListHeader

    /// <summary>Header of an extent list block ("SUBF1EXT", "SUBF2EXT", "SUBF3EXT"), 0x20 bytes</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct ExtentListHeader
    {
        /// <summary>0x000, Signature</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] signature;
        /// <summary>0x008, Reserved</summary>
        public ushort reserved_08;
        /// <summary>0x00A, Next list block LBA, high 16 bits</summary>
        public ushort nextHigh;
        /// <summary>0x00C, Next list block LBA, low 32 bits</summary>
        public uint nextLow;
        /// <summary>0x010, Number of valid extents in this block</summary>
        public ushort extents;
        /// <summary>0x012, Previous list block LBA, high 16 bits</summary>
        public ushort previousHigh;
        /// <summary>0x014, Previous list block LBA, low 32 bits</summary>
        public uint previousLow;
        /// <summary>0x018, Identifier of the file owning this list</summary>
        public uint fileId;
        /// <summary>0x01C, Flags, bit 7 set on first block</summary>
        public byte flags;
        /// <summary>0x01D, Reserved</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] reserved_1d;
    }

#endregion

#region Nested type: EaDescriptor

    /// <summary>Extended attribute descriptor inside an "EALSUBCL" subfile, 0x14 bytes followed by the name</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [SwapEndian]
    partial struct EaDescriptor
    {
        /// <summary>0x000, Offset of the value from the start of the subfile, 0 if no more descriptors</summary>
        public uint valueOffset;
        /// <summary>0x004, Codepage type</summary>
        public ushort codepageType;
        /// <summary>0x006, Codepage</summary>
        public ushort codepage;
        /// <summary>0x008, Reserved</summary>
        public uint reserved_08;
        /// <summary>0x00C, Flags, 1 = critical</summary>
        public byte flags;
        /// <summary>0x00D, Reserved</summary>
        public byte reserved_0d;
        /// <summary>0x00E, Bytes used by the name, including terminator and padding</summary>
        public ushort nameLength;
        /// <summary>0x010, Value length</summary>
        public uint valueLength;
    }

#endregion
}