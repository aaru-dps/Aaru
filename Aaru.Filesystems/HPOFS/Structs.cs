// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : High Performance Optical File System plugin.
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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

[SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class HPOFS
{
#region Nested type: BiosParameterBlock

    /// <summary>BIOS Parameter Block, at sector 0, little-endian</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct BiosParameterBlock
    {
        /// <summary>0x000, Jump to boot code</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] jump;
        /// <summary>0x003, OEM Name, 8 bytes, space-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] oem_name;
        /// <summary>0x00B, Bytes per sector</summary>
        public readonly ushort bps;
        /// <summary>0x00D, Sectors per cluster</summary>
        public readonly byte spc;
        /// <summary>0x00E, Reserved sectors between BPB and... does it have sense in HPFS?</summary>
        public readonly ushort rsectors;
        /// <summary>0x010, Number of FATs... seriously?</summary>
        public readonly byte fats_no;
        /// <summary>0x011, Number of entries on root directory... ok</summary>
        public readonly ushort root_ent;
        /// <summary>0x013, Sectors in volume... doubt it</summary>
        public readonly ushort sectors;
        /// <summary>0x015, Media descriptor</summary>
        public readonly byte media;
        /// <summary>0x016, Sectors per FAT... again</summary>
        public readonly ushort spfat;
        /// <summary>0x018, Sectors per track... you're kidding</summary>
        public readonly ushort sptrk;
        /// <summary>0x01A, Heads... stop!</summary>
        public readonly ushort heads;
        /// <summary>0x01C, Hidden sectors before BPB</summary>
        public readonly uint hsectors;
        /// <summary>0x024, Sectors in volume if &gt; 65535...</summary>
        public readonly uint big_sectors;
        /// <summary>0x028, Drive number</summary>
        public readonly byte drive_no;
        /// <summary>0x029, Volume flags?</summary>
        public readonly byte nt_flags;
        /// <summary>0x02A, EPB signature, 0x29</summary>
        public readonly byte signature;
        /// <summary>0x02B, Volume serial number</summary>
        public readonly uint serial_no;
        /// <summary>0x02F, Volume label, 11 bytes, space-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public readonly byte[] volume_label;
        /// <summary>0x03A, Filesystem type, 8 bytes, space-padded ("HPOFS   ")</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] fs_type;
        /// <summary>Boot code.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 442)]
        public readonly byte[] boot_code;
        /// <summary>0x1F8, Unknown</summary>
        public readonly uint unknown;
        /// <summary>0x1FC, Unknown</summary>
        public readonly ushort unknown2;
        /// <summary>0x1FE, 0xAA55</summary>
        public readonly ushort signature2;
    }

#endregion

#region Nested type: Dci

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Dci
    {
        /// <summary>"DATA"</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] blockId;
        /// <summary>Unknown</summary>
        public readonly uint unknown;
        /// <summary>Unknown</summary>
        public readonly uint unknown2;
        /// <summary>Unknown</summary>
        public readonly uint unknown3;
        /// <summary>Unknown</summary>
        public readonly uint unknown4;
        /// <summary>Unknown</summary>
        public readonly uint unknown5;
        /// <summary>Unknown</summary>
        public readonly ushort unknown6;
        /// <summary>Unknown</summary>
        public readonly ushort unknown7;
        /// <summary>Unknown</summary>
        public readonly uint unknown8;
        /// <summary>Unknown</summary>
        public readonly uint unknown9;
        /// <summary>Entries, size unknown</summary>
        public readonly DciEntry[] entries;
    }

#endregion

#region Nested type: DciEntry

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct DciEntry
    {
        /// <summary>Key length</summary>
        public readonly ushort key_len;
        /// <summary>Record length</summary>
        public readonly ushort record_len;
        /// <summary>dci key</summary>
        public readonly DciKey key;
        /// <summary>Padding? Size is key_len - size of DciKey</summary>
        public readonly byte[] padding;
        /// <summary>Direct</summary>
        public readonly Direct dir;
        /// <summary>Padding? Size is record_len - size of Direct</summary>
        public readonly byte[] unknown;
    }

#endregion

#region Nested type: DciKey

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct DciKey
    {
        /// <summary>Unknown</summary>
        public readonly byte unknown;
        /// <summary>Name size + 2</summary>
        public readonly byte size;
        /// <summary>Unknown</summary>
        public readonly byte unknown2;
        /// <summary>Unknown</summary>
        public readonly byte unknown3;
        /// <summary>Unknown</summary>
        public readonly byte unknown4;
        /// <summary>Unknown</summary>
        public readonly byte unknown5;
        /// <summary>Name, length = size - 2</summary>
        public readonly byte[] name;
    }

#endregion

#region Nested type: Direct

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Direct
    {
        /// <summary>Unknown</summary>
        public readonly uint unknown;
        /// <summary>Unknown</summary>
        public readonly uint unknown2;
        /// <summary>Unknown</summary>
        public readonly uint unknown3;
        /// <summary>Mask 0x6000</summary>
        public readonly ushort subfiles_no;
        /// <summary>Unknown</summary>
        public readonly ushort unknown4;
        /// <summary>Unknown</summary>
        public readonly uint unknown5;
        /// <summary>Unknown</summary>
        public readonly uint unknown6;
        /// <summary>Unknown</summary>
        public readonly uint unknown7;
        /// <summary>Some date</summary>
        public readonly ushort date1;
        /// <summary>Some time</summary>
        public readonly ushort time1;
        /// <summary>Some date</summary>
        public readonly ushort date2;
        /// <summary>Some time</summary>
        public readonly ushort time2;
        /// <summary>Unknown</summary>
        public readonly uint unknown8;
        /// <summary>Unknown</summary>
        public readonly uint unknown9;
        /// <summary>Unknown</summary>
        public readonly uint unknown10;
        /// <summary>Unknown</summary>
        public readonly uint unknown11;
        /// <summary>Unknown</summary>
        public readonly uint unknown12;
        /// <summary>Unknown</summary>
        public readonly uint unknown13;
        /// <summary>Unknown</summary>
        public readonly uint unknown14;
        /// <summary>Subfiles, length unknown</summary>
        public readonly SubFile[] subfiles;
    }

#endregion

#region Nested type: Extent

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Extent
    {
        /// <summary>Extent length in sectors</summary>
        public readonly ushort length;
        /// <summary>Unknown</summary>
        public readonly short unknown;
        /// <summary>Extent starting sector</summary>
        public readonly int start;
    }

#endregion

#region Nested type: MasterRecord

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct MasterRecord
    {
        /// <summary>"MAST"</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public readonly byte[] blockId;
        /// <summary>Unknown</summary>
        public readonly uint unknown;
        /// <summary>Unknown</summary>
        public readonly ushort unknown2;
        /// <summary>Unknown</summary>
        public readonly ushort unknown3;
        /// <summary>Unknown</summary>
        public readonly uint unknown4;
        /// <summary>Unknown</summary>
        public readonly ushort unknown5;
        /// <summary>Unknown</summary>
        public readonly ushort unknown6;
        /// <summary>Unknown</summary>
        public readonly ushort unknown7;
        /// <summary>Unknown</summary>
        public readonly ushort unknown8;
        /// <summary>Unknown</summary>
        public readonly uint unknown9;
    }

#endregion

#region Nested type: MediaInformationBlock

    /// <summary>Media Information Block, at sector 13, big-endian</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct MediaInformationBlock
    {
        /// <summary>Block identifier "MEDINFO "</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] blockId;
        /// <summary>Volume label</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] volumeLabel;
        /// <summary>Volume comment</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 160)]
        public readonly byte[] comment;
        /// <summary>Volume serial number</summary>
        public readonly uint serial;
        /// <summary>Volume creation date, DOS format</summary>
        public readonly ushort creationDate;
        /// <summary>Volume creation time, DOS format</summary>
        public readonly ushort creationTime;
        /// <summary>Codepage type: 1 ASCII, 2 EBCDIC</summary>
        public readonly ushort codepageType;
        /// <summary>Codepage</summary>
        public readonly ushort codepage;
        /// <summary>RPS level</summary>
        public readonly uint rps;
        /// <summary>Coincides with bytes per sector, and bytes per cluster, need more media</summary>
        public readonly ushort bps;
        /// <summary>Coincides with bytes per sector, and bytes per cluster, need more media</summary>
        public readonly ushort bpc;
        /// <summary>Unknown, empty</summary>
        public readonly uint unknown2;
        /// <summary>Sectors (or clusters)</summary>
        public readonly uint sectors;
        /// <summary>Unknown, coincides with bps but changing it makes nothing</summary>
        public readonly uint unknown3;
        /// <summary>Empty?</summary>
        public readonly ulong unknown4;
        /// <summary>Format major version</summary>
        public readonly ushort major;
        /// <summary>Format minor version</summary>
        public readonly ushort minor;
        /// <summary>Empty?</summary>
        public readonly uint unknown5;
        /// <summary>Unknown, non-empty</summary>
        public readonly uint unknown6;
        /// <summary>Empty</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 260)]
        public readonly byte[] filler;
    }

#endregion

#region Nested type: SubFile

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct SubFile
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly Extent[] extents;
        /// <summary>Unknown</summary>
        public readonly uint unknown;
        /// <summary>Unknown</summary>
        public readonly uint unknown2;
        /// <summary>Logical size in bytes</summary>
        public readonly uint logical_size;
        /// <summary>Unknown</summary>
        public readonly uint unknown3;
        /// <summary>Physical size in bytes</summary>
        public readonly uint physical_size;
        /// <summary>Unknown</summary>
        public readonly uint unknown4;
        /// <summary>Physical size in bytes</summary>
        public readonly uint physical_size2;
        /// <summary>Unknown</summary>
        public readonly uint unknown5;
        /// <summary>Unknown</summary>
        public readonly uint unknown6;
    }

#endregion

#region Nested type: VolumeInformationBlock

    /// <summary>Volume Information Block, at sector 14, big-endian</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct VolumeInformationBlock
    {
        /// <summary>Block identifier "VOLINFO "</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] blockId;
        /// <summary>Unknown</summary>
        public readonly uint unknown;
        /// <summary>Unknown</summary>
        public readonly uint unknown2;
        /// <summary>Some kind of counter</summary>
        public readonly uint dir_intent_cnt;
        /// <summary>Some kind of counter, another</summary>
        public readonly uint dir_update_cnt;
        /// <summary>Unknown</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
        public readonly byte[] unknown3;
        /// <summary>Unknown, space-padded string</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] unknown4;
        /// <summary>Owner, space-padded string</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] owner;
        /// <summary>Unknown, space-padded string</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] unknown5;
        /// <summary>Unknown, empty?</summary>
        public readonly uint unknown6;
        /// <summary>Maximum percent full</summary>
        public readonly ushort percentFull;
        /// <summary>Unknown, empty?</summary>
        public readonly ushort unknown7;
        /// <summary>Empty</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 384)]
        public readonly byte[] filler;
    }

#endregion
}