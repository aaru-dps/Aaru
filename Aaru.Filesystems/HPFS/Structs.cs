// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : OS/2 High Performance File System plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the OS/2 High Performance File System and shows information.
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

using System.Runtime.InteropServices;

namespace Aaru.Filesystems;

// Information from an old unnamed document
/// <inheritdoc />
/// <summary>Implements detection of IBM's High Performance File System (HPFS)</summary>
public sealed partial class HPFS
{
#region Nested type: BiosParameterBlock

    /// <summary>BIOS Parameter Block, at sector 0</summary>
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
        /// <summary>0x03A, Filesystem type, 8 bytes, space-padded ("HPFS    ")</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] fs_type;
        /// <summary>Boot code.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 448)]
        public readonly byte[] boot_code;
        /// <summary>0x1FE, 0xAA55</summary>
        public readonly ushort signature2;
    }

#endregion

#region Nested type: SpareBlock

    /// <summary>HPFS spareblock at sector 17</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct SpareBlock
    {
        /// <summary>0x000, 0xF9911849</summary>
        public readonly uint magic1;
        /// <summary>0x004, 0xFA5229C5</summary>
        public readonly uint magic2;
        /// <summary>0x008, HPFS flags</summary>
        public readonly byte flags1;
        /// <summary>0x009, HPFS386 flags</summary>
        public readonly byte flags2;
        /// <summary>0x00A, Alignment</summary>
        public readonly ushort dummy;
        /// <summary>0x00C, LSN of hotfix directory</summary>
        public readonly uint hotfix_start;
        /// <summary>0x010, Used hotfixes</summary>
        public readonly uint hotfix_used;
        /// <summary>0x014, Total hotfixes available</summary>
        public readonly uint hotfix_entries;
        /// <summary>0x018, Unused spare dnodes</summary>
        public readonly uint spare_dnodes_free;
        /// <summary>0x01C, Length of spare dnodes list</summary>
        public readonly uint spare_dnodes;
        /// <summary>0x020, LSN of codepage directory</summary>
        public readonly uint codepage_lsn;
        /// <summary>0x024, Number of codepages used</summary>
        public readonly uint codepages;
        /// <summary>0x028, SuperBlock CRC32 (only HPFS386)</summary>
        public readonly uint sb_crc32;
        /// <summary>0x02C, SpareBlock CRC32 (only HPFS386)</summary>
        public readonly uint sp_crc32;
    }

#endregion

#region Nested type: SuperBlock

    /// <summary>HPFS superblock at sector 16</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct SuperBlock
    {
        /// <summary>0x000, 0xF995E849</summary>
        public readonly uint magic1;
        /// <summary>0x004, 0xFA53E9C5</summary>
        public readonly uint magic2;
        /// <summary>0x008, HPFS version</summary>
        public readonly byte version;
        /// <summary>0x009, 2 if &lt;= 4 GiB, 3 if &gt; 4 GiB</summary>
        public readonly byte func_version;
        /// <summary>0x00A, Alignment</summary>
        public readonly ushort dummy;
        /// <summary>0x00C, LSN pointer to root fnode</summary>
        public readonly uint root_fnode;
        /// <summary>0x010, Sectors on volume</summary>
        public readonly uint sectors;
        /// <summary>0x014, Bad blocks on volume</summary>
        public readonly uint badblocks;
        /// <summary>0x018, LSN pointer to volume bitmap</summary>
        public readonly uint bitmap_lsn;
        /// <summary>0x01C, 0</summary>
        public readonly uint zero1;
        /// <summary>0x020, LSN pointer to badblock directory</summary>
        public readonly uint badblock_lsn;
        /// <summary>0x024, 0</summary>
        public readonly uint zero2;
        /// <summary>0x028, Time of last CHKDSK</summary>
        public readonly int last_chkdsk;
        /// <summary>0x02C, Time of last optimization</summary>
        public readonly int last_optim;
        /// <summary>0x030, Sectors of dir band</summary>
        public readonly uint dband_sectors;
        /// <summary>0x034, Start sector of dir band</summary>
        public readonly uint dband_start;
        /// <summary>0x038, Last sector of dir band</summary>
        public readonly uint dband_last;
        /// <summary>0x03C, LSN of free space bitmap</summary>
        public readonly uint dband_bitmap;
        /// <summary>0x040, Can be used for volume name (32 bytes)</summary>
        public readonly ulong zero3;
        /// <summary>0x048, ...</summary>
        public readonly ulong zero4;
        /// <summary>0x04C, ...</summary>
        public readonly ulong zero5;
        /// <summary>0x050, ...;</summary>
        public readonly ulong zero6;
        /// <summary>0x058, LSN pointer to ACLs (only HPFS386)</summary>
        public readonly uint acl_start;
    }

#endregion
}