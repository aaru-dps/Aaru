// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HPFS.cs
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems
{
    // Information from an old unnamed document
    public class HPFS : IFilesystem
    {
        public FileSystemType XmlFsType { get; private set; }
        public Encoding       Encoding  { get; private set; }
        public string         Name      => "OS/2 High Performance File System";
        public Guid           Id        => new Guid("33513B2C-f590-4acb-8bf2-0b1d5e19dec5");
        public string         Author    => "Natalia Portillo";

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(16 + partition.Start >= partition.End) return false;

            byte[] hpfsSbSector =
                imagePlugin.ReadSector(16 + partition.Start); // Seek to superblock, on logical sector 16
            uint magic1 = BitConverter.ToUInt32(hpfsSbSector, 0x000);
            uint magic2 = BitConverter.ToUInt32(hpfsSbSector, 0x004);

            return magic1 == 0xF995E849 && magic2 == 0xFA53E9C5;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding    encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("ibm850");
            information = "";

            StringBuilder sb = new StringBuilder();

            byte[] hpfsBpbSector =
                imagePlugin.ReadSector(0 + partition.Start); // Seek to BIOS parameter block, on logical sector 0
            byte[] hpfsSbSector =
                imagePlugin.ReadSector(16 + partition.Start); // Seek to superblock, on logical sector 16
            byte[] hpfsSpSector =
                imagePlugin.ReadSector(17 + partition.Start); // Seek to spareblock, on logical sector 17

            HpfsBiosParameterBlock hpfsBpb =
                Marshal.ByteArrayToStructureLittleEndian<HpfsBiosParameterBlock>(hpfsBpbSector);

            HpfsSuperBlock hpfsSb = Marshal.ByteArrayToStructureLittleEndian<HpfsSuperBlock>(hpfsSbSector);

            HpfsSpareBlock hpfsSp = Marshal.ByteArrayToStructureLittleEndian<HpfsSpareBlock>(hpfsSpSector);

            if(StringHandlers.CToString(hpfsBpb.fs_type) != "HPFS    " || hpfsSb.magic1 != 0xF995E849 ||
               hpfsSb.magic2                             != 0xFA53E9C5 || hpfsSp.magic1 != 0xF9911849 ||
               hpfsSp.magic2                             != 0xFA5229C5)
            {
                sb.AppendLine("This may not be HPFS, following information may be not correct.");
                sb.AppendFormat("File system type: \"{0}\" (Should be \"HPFS    \")", hpfsBpb.fs_type).AppendLine();
                sb.AppendFormat("Superblock magic1: 0x{0:X8} (Should be 0xF995E849)", hpfsSb.magic1).AppendLine();
                sb.AppendFormat("Superblock magic2: 0x{0:X8} (Should be 0xFA53E9C5)", hpfsSb.magic2).AppendLine();
                sb.AppendFormat("Spareblock magic1: 0x{0:X8} (Should be 0xF9911849)", hpfsSp.magic1).AppendLine();
                sb.AppendFormat("Spareblock magic2: 0x{0:X8} (Should be 0xFA5229C5)", hpfsSp.magic2).AppendLine();
            }

            sb.AppendFormat("OEM name: {0}", StringHandlers.CToString(hpfsBpb.oem_name)).AppendLine();
            sb.AppendFormat("{0} bytes per sector", hpfsBpb.bps).AppendLine();
            //          sb.AppendFormat("{0} sectors per cluster", hpfs_bpb.spc).AppendLine();
            //          sb.AppendFormat("{0} reserved sectors", hpfs_bpb.rsectors).AppendLine();
            //          sb.AppendFormat("{0} FATs", hpfs_bpb.fats_no).AppendLine();
            //          sb.AppendFormat("{0} entries on root directory", hpfs_bpb.root_ent).AppendLine();
            //          sb.AppendFormat("{0} mini sectors on volume", hpfs_bpb.sectors).AppendLine();
            sb.AppendFormat("Media descriptor: 0x{0:X2}", hpfsBpb.media).AppendLine();
            //          sb.AppendFormat("{0} sectors per FAT", hpfs_bpb.spfat).AppendLine();
            //          sb.AppendFormat("{0} sectors per track", hpfs_bpb.sptrk).AppendLine();
            //          sb.AppendFormat("{0} heads", hpfs_bpb.heads).AppendLine();
            sb.AppendFormat("{0} sectors hidden before BPB", hpfsBpb.hsectors).AppendLine();
            sb.AppendFormat("{0} sectors on volume ({1} bytes)", hpfsSb.sectors, hpfsSb.sectors * hpfsBpb.bps)
              .AppendLine();
            //          sb.AppendFormat("{0} sectors on volume ({1} bytes)", hpfs_bpb.big_sectors, hpfs_bpb.big_sectors * hpfs_bpb.bps).AppendLine();
            sb.AppendFormat("BIOS Drive Number: 0x{0:X2}", hpfsBpb.drive_no).AppendLine();
            sb.AppendFormat("NT Flags: 0x{0:X2}", hpfsBpb.nt_flags).AppendLine();
            sb.AppendFormat("Signature: 0x{0:X2}", hpfsBpb.signature).AppendLine();
            sb.AppendFormat("Serial number: 0x{0:X8}", hpfsBpb.serial_no).AppendLine();
            sb.AppendFormat("Volume label: {0}", StringHandlers.CToString(hpfsBpb.volume_label, Encoding)).AppendLine();
            //          sb.AppendFormat("Filesystem type: \"{0}\"", hpfs_bpb.fs_type).AppendLine();

            DateTime lastChk   = DateHandlers.UnixToDateTime(hpfsSb.last_chkdsk);
            DateTime lastOptim = DateHandlers.UnixToDateTime(hpfsSb.last_optim);

            sb.AppendFormat("HPFS version: {0}", hpfsSb.version).AppendLine();
            sb.AppendFormat("Functional version: {0}", hpfsSb.func_version).AppendLine();
            sb.AppendFormat("Sector of root directory FNode: {0}", hpfsSb.root_fnode).AppendLine();
            sb.AppendFormat("{0} sectors are marked bad", hpfsSb.badblocks).AppendLine();
            sb.AppendFormat("Sector of free space bitmaps: {0}", hpfsSb.bitmap_lsn).AppendLine();
            sb.AppendFormat("Sector of bad blocks list: {0}", hpfsSb.badblock_lsn).AppendLine();
            if(hpfsSb.last_chkdsk > 0) sb.AppendFormat("Date of last integrity check: {0}", lastChk).AppendLine();
            else sb.AppendLine("Filesystem integrity has never been checked");
            if(hpfsSb.last_optim > 0) sb.AppendFormat("Date of last optimization {0}", lastOptim).AppendLine();
            else sb.AppendLine("Filesystem has never been optimized");
            sb.AppendFormat("Directory band has {0} sectors", hpfsSb.dband_sectors).AppendLine();
            sb.AppendFormat("Directory band starts at sector {0}", hpfsSb.dband_start).AppendLine();
            sb.AppendFormat("Directory band ends at sector {0}", hpfsSb.dband_last).AppendLine();
            sb.AppendFormat("Sector of directory band bitmap: {0}", hpfsSb.dband_bitmap).AppendLine();
            sb.AppendFormat("Sector of ACL directory: {0}", hpfsSb.acl_start).AppendLine();

            sb.AppendFormat("Sector of Hotfix directory: {0}", hpfsSp.hotfix_start).AppendLine();
            sb.AppendFormat("{0} used Hotfix entries", hpfsSp.hotfix_used).AppendLine();
            sb.AppendFormat("{0} total Hotfix entries", hpfsSp.hotfix_entries).AppendLine();
            sb.AppendFormat("{0} free spare DNodes", hpfsSp.spare_dnodes_free).AppendLine();
            sb.AppendFormat("{0} total spare DNodes", hpfsSp.spare_dnodes).AppendLine();
            sb.AppendFormat("Sector of codepage directory: {0}", hpfsSp.codepage_lsn).AppendLine();
            sb.AppendFormat("{0} codepages used in the volume", hpfsSp.codepages).AppendLine();
            sb.AppendFormat("SuperBlock CRC32: {0:X8}", hpfsSp.sb_crc32).AppendLine();
            sb.AppendFormat("SpareBlock CRC32: {0:X8}", hpfsSp.sp_crc32).AppendLine();

            sb.AppendLine("Flags:");
            sb.AppendLine((hpfsSp.flags1 & 0x01) == 0x01 ? "Filesystem is dirty." : "Filesystem is clean.");
            if((hpfsSp.flags1 & 0x02) == 0x02) sb.AppendLine("Spare directory blocks are in use");
            if((hpfsSp.flags1 & 0x04) == 0x04) sb.AppendLine("Hotfixes are in use");
            if((hpfsSp.flags1 & 0x08) == 0x08) sb.AppendLine("Disk contains bad sectors");
            if((hpfsSp.flags1 & 0x10) == 0x10) sb.AppendLine("Disk has a bad bitmap");
            if((hpfsSp.flags1 & 0x20) == 0x20) sb.AppendLine("Filesystem was formatted fast");
            if((hpfsSp.flags1 & 0x40) == 0x40) sb.AppendLine("Unknown flag 0x40 on flags1 is active");
            if((hpfsSp.flags1 & 0x80) == 0x80) sb.AppendLine("Filesystem has been mounted by an old IFS");
            if((hpfsSp.flags2 & 0x01) == 0x01) sb.AppendLine("Install DASD limits");
            if((hpfsSp.flags2 & 0x02) == 0x02) sb.AppendLine("Resync DASD limits");
            if((hpfsSp.flags2 & 0x04) == 0x04) sb.AppendLine("DASD limits are operational");
            if((hpfsSp.flags2 & 0x08) == 0x08) sb.AppendLine("Multimedia is active");
            if((hpfsSp.flags2 & 0x10) == 0x10) sb.AppendLine("DCE ACLs are active");
            if((hpfsSp.flags2 & 0x20) == 0x20) sb.AppendLine("DASD limits are dirty");
            if((hpfsSp.flags2 & 0x40) == 0x40) sb.AppendLine("Unknown flag 0x40 on flags2 is active");
            if((hpfsSp.flags2 & 0x80) == 0x80) sb.AppendLine("Unknown flag 0x80 on flags2 is active");

            XmlFsType = new FileSystemType();

            // Theoretically everything from BPB to SB is boot code, should I hash everything or only the sector loaded by BIOS itself?
            if(hpfsBpb.jump[0]    == 0xEB && hpfsBpb.jump[1] > 0x3C && hpfsBpb.jump[1] < 0x80 &&
               hpfsBpb.signature2 == 0xAA55)
            {
                XmlFsType.Bootable = true;
                string bootChk = Sha1Context.Data(hpfsBpb.boot_code, out byte[] _);
                sb.AppendLine("Volume is bootable");
                sb.AppendFormat("Boot code's SHA1: {0}", bootChk).AppendLine();
            }

            XmlFsType.Dirty            |= (hpfsSp.flags1 & 0x01) == 0x01;
            XmlFsType.Clusters         =  hpfsSb.sectors;
            XmlFsType.ClusterSize      =  hpfsBpb.bps;
            XmlFsType.Type             =  "HPFS";
            XmlFsType.VolumeName       =  StringHandlers.CToString(hpfsBpb.volume_label, Encoding);
            XmlFsType.VolumeSerial     =  $"{hpfsBpb.serial_no:X8}";
            XmlFsType.SystemIdentifier =  StringHandlers.CToString(hpfsBpb.oem_name);

            information = sb.ToString();
        }

        /// <summary>
        ///     BIOS Parameter Block, at sector 0
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HpfsBiosParameterBlock
        {
            /// <summary>0x000, Jump to boot code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] jump;
            /// <summary>0x003, OEM Name, 8 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] oem_name;
            /// <summary>0x00B, Bytes per sector</summary>
            public ushort bps;
            /// <summary>0x00D, Sectors per cluster</summary>
            public byte spc;
            /// <summary>0x00E, Reserved sectors between BPB and... does it have sense in HPFS?</summary>
            public ushort rsectors;
            /// <summary>0x010, Number of FATs... seriously?</summary>
            public byte fats_no;
            /// <summary>0x011, Number of entries on root directory... ok</summary>
            public ushort root_ent;
            /// <summary>0x013, Sectors in volume... doubt it</summary>
            public ushort sectors;
            /// <summary>0x015, Media descriptor</summary>
            public byte media;
            /// <summary>0x016, Sectors per FAT... again</summary>
            public ushort spfat;
            /// <summary>0x018, Sectors per track... you're kidding</summary>
            public ushort sptrk;
            /// <summary>0x01A, Heads... stop!</summary>
            public ushort heads;
            /// <summary>0x01C, Hidden sectors before BPB</summary>
            public uint hsectors;
            /// <summary>0x024, Sectors in volume if &gt; 65535...</summary>
            public uint big_sectors;
            /// <summary>0x028, Drive number</summary>
            public byte drive_no;
            /// <summary>0x029, Volume flags?</summary>
            public byte nt_flags;
            /// <summary>0x02A, EPB signature, 0x29</summary>
            public byte signature;
            /// <summary>0x02B, Volume serial number</summary>
            public uint serial_no;
            /// <summary>0x02F, Volume label, 11 bytes, space-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public byte[] volume_label;
            /// <summary>0x03A, Filesystem type, 8 bytes, space-padded ("HPFS    ")</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] fs_type;
            /// <summary>Boot code.</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 448)]
            public byte[] boot_code;
            /// <summary>0x1FE, 0xAA55</summary>
            public ushort signature2;
        }

        /// <summary>
        ///     HPFS superblock at sector 16
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HpfsSuperBlock
        {
            /// <summary>0x000, 0xF995E849</summary>
            public uint magic1;
            /// <summary>0x004, 0xFA53E9C5</summary>
            public uint magic2;
            /// <summary>0x008, HPFS version</summary>
            public byte version;
            /// <summary>0x009, 2 if &lt;= 4 GiB, 3 if &gt; 4 GiB</summary>
            public byte func_version;
            /// <summary>0x00A, Alignment</summary>
            public ushort dummy;
            /// <summary>0x00C, LSN pointer to root fnode</summary>
            public uint root_fnode;
            /// <summary>0x010, Sectors on volume</summary>
            public uint sectors;
            /// <summary>0x014, Bad blocks on volume</summary>
            public uint badblocks;
            /// <summary>0x018, LSN pointer to volume bitmap</summary>
            public uint bitmap_lsn;
            /// <summary>0x01C, 0</summary>
            public uint zero1;
            /// <summary>0x020, LSN pointer to badblock directory</summary>
            public uint badblock_lsn;
            /// <summary>0x024, 0</summary>
            public uint zero2;
            /// <summary>0x028, Time of last CHKDSK</summary>
            public int last_chkdsk;
            /// <summary>0x02C, Time of last optimization</summary>
            public int last_optim;
            /// <summary>0x030, Sectors of dir band</summary>
            public uint dband_sectors;
            /// <summary>0x034, Start sector of dir band</summary>
            public uint dband_start;
            /// <summary>0x038, Last sector of dir band</summary>
            public uint dband_last;
            /// <summary>0x03C, LSN of free space bitmap</summary>
            public uint dband_bitmap;
            /// <summary>0x040, Can be used for volume name (32 bytes)</summary>
            public ulong zero3;
            /// <summary>0x048, ...</summary>
            public ulong zero4;
            /// <summary>0x04C, ...</summary>
            public ulong zero5;
            /// <summary>0x050, ...;</summary>
            public ulong zero6;
            /// <summary>0x058, LSN pointer to ACLs (only HPFS386)</summary>
            public uint acl_start;
        }

        /// <summary>
        ///     HPFS spareblock at sector 17
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct HpfsSpareBlock
        {
            /// <summary>0x000, 0xF9911849</summary>
            public uint magic1;
            /// <summary>0x004, 0xFA5229C5</summary>
            public uint magic2;
            /// <summary>0x008, HPFS flags</summary>
            public byte flags1;
            /// <summary>0x009, HPFS386 flags</summary>
            public byte flags2;
            /// <summary>0x00A, Alignment</summary>
            public ushort dummy;
            /// <summary>0x00C, LSN of hotfix directory</summary>
            public uint hotfix_start;
            /// <summary>0x010, Used hotfixes</summary>
            public uint hotfix_used;
            /// <summary>0x014, Total hotfixes available</summary>
            public uint hotfix_entries;
            /// <summary>0x018, Unused spare dnodes</summary>
            public uint spare_dnodes_free;
            /// <summary>0x01C, Length of spare dnodes list</summary>
            public uint spare_dnodes;
            /// <summary>0x020, LSN of codepage directory</summary>
            public uint codepage_lsn;
            /// <summary>0x024, Number of codepages used</summary>
            public uint codepages;
            /// <summary>0x028, SuperBlock CRC32 (only HPFS386)</summary>
            public uint sb_crc32;
            /// <summary>0x02C, SpareBlock CRC32 (only HPFS386)</summary>
            public uint sp_crc32;
        }
    }
}