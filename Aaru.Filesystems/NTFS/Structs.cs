// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Structs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Microsoft NT File System plugin.
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

// Information from Inside Windows NT
/// <inheritdoc />
/// <summary>Implements detection of the New Technology File System (NTFS)</summary>
public sealed partial class NTFS
{
    /// <summary>NTFS $BOOT</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct BiosParameterBlock
    {
        // Start of BIOS Parameter Block
        /// <summary>0x000, Jump to boot code</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] jump;
        /// <summary>0x003, OEM Name, 8 bytes, space-padded, must be "NTFS    "</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] oem_name;
        /// <summary>0x00B, Bytes per sector</summary>
        public readonly ushort bps;
        /// <summary>0x00D, Sectors per cluster</summary>
        public readonly byte spc;
        /// <summary>0x00E, Reserved sectors, seems 0</summary>
        public readonly ushort rsectors;
        /// <summary>0x010, Number of FATs... obviously, 0</summary>
        public readonly byte fats_no;
        /// <summary>0x011, Number of entries on root directory... 0</summary>
        public readonly ushort root_ent;
        /// <summary>0x013, Sectors in volume... 0</summary>
        public readonly ushort sml_sectors;
        /// <summary>0x015, Media descriptor</summary>
        public readonly byte media;
        /// <summary>0x016, Sectors per FAT... 0</summary>
        public readonly ushort spfat;
        /// <summary>0x018, Sectors per track, required to boot</summary>
        public readonly ushort sptrk;
        /// <summary>0x01A, Heads... required to boot</summary>
        public readonly ushort heads;
        /// <summary>0x01C, Hidden sectors before BPB</summary>
        public readonly uint hsectors;
        /// <summary>0x020, Sectors in volume if &gt; 65535... 0</summary>
        public readonly uint big_sectors;
        /// <summary>0x024, Drive number</summary>
        public readonly byte drive_no;
        /// <summary>0x025, 0</summary>
        public readonly byte nt_flags;
        /// <summary>0x026, EPB signature, 0x80</summary>
        public readonly byte signature1;
        /// <summary>0x027, Alignment</summary>
        public readonly byte dummy;

        // End of BIOS Parameter Block

        // Start of NTFS real superblock
        /// <summary>0x028, Sectors on volume</summary>
        public readonly long sectors;
        /// <summary>0x030, LSN of $MFT</summary>
        public readonly long mft_lsn;
        /// <summary>0x038, LSN of $MFTMirror</summary>
        public readonly long mftmirror_lsn;
        /// <summary>0x040, Clusters per MFT record</summary>
        public readonly sbyte mft_rc_clusters;
        /// <summary>0x041, Alignment</summary>
        public readonly byte dummy2;
        /// <summary>0x042, Alignment</summary>
        public readonly ushort dummy3;
        /// <summary>0x044, Clusters per index block</summary>
        public readonly sbyte index_blk_cts;
        /// <summary>0x045, Alignment</summary>
        public readonly byte dummy4;
        /// <summary>0x046, Alignment</summary>
        public readonly ushort dummy5;
        /// <summary>0x048, Volume serial number</summary>
        public readonly ulong serial_no;
        /// <summary>Boot code.</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 430)]
        public readonly byte[] boot_code;
        /// <summary>0x1FE, 0xAA55</summary>
        public readonly ushort signature2;
    }
}