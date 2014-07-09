/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : HPFS.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Filesystem plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Identifies OS/2 HPFS filesystems and shows information.
No pinball playing allowed.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$

using System;
using System.Text;
using DiscImageChef;

// Information from an old unnamed document
namespace DiscImageChef.Plugins
{
    class HPFS : Plugin
    {
        public HPFS(PluginBase Core)
        {
            Name = "OS/2 High Performance File System";
            PluginUUID = new Guid("33513B2C-f590-4acb-8bf2-0b1d5e19dec5");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset)
        {
            if ((2 + partitionOffset) >= imagePlugin.GetSectors())
                return false;

            UInt32 magic1, magic2;
			
            byte[] hpfs_sb_sector = imagePlugin.ReadSector(16 + partitionOffset); // Seek to superblock, on logical sector 16
            magic1 = BitConverter.ToUInt32(hpfs_sb_sector, 0x000);
            magic2 = BitConverter.ToUInt32(hpfs_sb_sector, 0x004);
			
            if (magic1 == 0xF995E849 && magic2 == 0xFA53E9C5)
                return true;
            return false;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset, out string information)
        {
            information = "";
			
            StringBuilder sb = new StringBuilder();
			
            HPFS_BIOSParameterBlock hpfs_bpb = new HPFS_BIOSParameterBlock();
            HPFS_SuperBlock hpfs_sb = new HPFS_SuperBlock();
            HPFS_SpareBlock hpfs_sp = new HPFS_SpareBlock();
			
            byte[] oem_name = new byte[8];
            byte[] volume_name = new byte[11];
			
            byte[] hpfs_bpb_sector = imagePlugin.ReadSector(0 + partitionOffset); // Seek to BIOS parameter block, on logical sector 0
            byte[] hpfs_sb_sector = imagePlugin.ReadSector(16 + partitionOffset); // Seek to superblock, on logical sector 16
            byte[] hpfs_sp_sector = imagePlugin.ReadSector(17 + partitionOffset); // Seek to spareblock, on logical sector 17

            hpfs_bpb.jmp1 = hpfs_bpb_sector[0x000];
            hpfs_bpb.jmp2 = BitConverter.ToUInt16(hpfs_bpb_sector, 0x001);
            Array.Copy(hpfs_bpb_sector, 0x003, oem_name, 0, 8);
            hpfs_bpb.OEMName = StringHandlers.CToString(oem_name);
            hpfs_bpb.bps = BitConverter.ToUInt16(hpfs_bpb_sector, 0x00B);
            hpfs_bpb.spc = hpfs_bpb_sector[0x00D];
            hpfs_bpb.rsectors = BitConverter.ToUInt16(hpfs_bpb_sector, 0x00E);
            hpfs_bpb.fats_no = hpfs_bpb_sector[0x010];
            hpfs_bpb.root_ent = BitConverter.ToUInt16(hpfs_bpb_sector, 0x011);
            hpfs_bpb.sectors = BitConverter.ToUInt16(hpfs_bpb_sector, 0x013);
            hpfs_bpb.media = hpfs_bpb_sector[0x015];
            hpfs_bpb.spfat = BitConverter.ToUInt16(hpfs_bpb_sector, 0x016);
            hpfs_bpb.sptrk = BitConverter.ToUInt16(hpfs_bpb_sector, 0x018);
            hpfs_bpb.heads = BitConverter.ToUInt16(hpfs_bpb_sector, 0x01A);
            hpfs_bpb.hsectors = BitConverter.ToUInt32(hpfs_bpb_sector, 0x01C);
            hpfs_bpb.big_sectors = BitConverter.ToUInt32(hpfs_bpb_sector, 0x024);
            hpfs_bpb.drive_no = hpfs_bpb_sector[0x028];
            hpfs_bpb.nt_flags = hpfs_bpb_sector[0x029];
            hpfs_bpb.signature = hpfs_bpb_sector[0x02A];
            hpfs_bpb.serial_no = BitConverter.ToUInt32(hpfs_bpb_sector, 0x02B);
            Array.Copy(hpfs_bpb_sector, 0x02F, volume_name, 0, 11);
            hpfs_bpb.volume_label = StringHandlers.CToString(volume_name);
            Array.Copy(hpfs_bpb_sector, 0x03A, oem_name, 0, 8);
            hpfs_bpb.fs_type = StringHandlers.CToString(oem_name);
			
            hpfs_sb.magic1 = BitConverter.ToUInt32(hpfs_sb_sector, 0x000);
            hpfs_sb.magic2 = BitConverter.ToUInt32(hpfs_sb_sector, 0x004);
            hpfs_sb.version = hpfs_sb_sector[0x008];
            hpfs_sb.func_version = hpfs_sb_sector[0x009];
            hpfs_sb.dummy = BitConverter.ToUInt16(hpfs_sb_sector, 0x00A);
            hpfs_sb.root_fnode = BitConverter.ToUInt32(hpfs_sb_sector, 0x00C);
            hpfs_sb.sectors = BitConverter.ToUInt32(hpfs_sb_sector, 0x010);
            hpfs_sb.badblocks = BitConverter.ToUInt32(hpfs_sb_sector, 0x014);
            hpfs_sb.bitmap_lsn = BitConverter.ToUInt32(hpfs_sb_sector, 0x018);
            hpfs_sb.zero1 = BitConverter.ToUInt32(hpfs_sb_sector, 0x01C);
            hpfs_sb.badblock_lsn = BitConverter.ToUInt32(hpfs_sb_sector, 0x020);
            hpfs_sb.zero2 = BitConverter.ToUInt32(hpfs_sb_sector, 0x024);
            hpfs_sb.last_chkdsk = BitConverter.ToInt32(hpfs_sb_sector, 0x028);
            hpfs_sb.last_optim = BitConverter.ToInt32(hpfs_sb_sector, 0x02C);
            hpfs_sb.dband_sectors = BitConverter.ToUInt32(hpfs_sb_sector, 0x030);
            hpfs_sb.dband_start = BitConverter.ToUInt32(hpfs_sb_sector, 0x034);
            hpfs_sb.dband_last = BitConverter.ToUInt32(hpfs_sb_sector, 0x038);
            hpfs_sb.dband_bitmap = BitConverter.ToUInt32(hpfs_sb_sector, 0x03C);
            hpfs_sb.zero3 = BitConverter.ToUInt64(hpfs_sb_sector, 0x040);
            hpfs_sb.zero4 = BitConverter.ToUInt64(hpfs_sb_sector, 0x048);
            hpfs_sb.zero5 = BitConverter.ToUInt64(hpfs_sb_sector, 0x04C);
            hpfs_sb.zero6 = BitConverter.ToUInt64(hpfs_sb_sector, 0x050);
            hpfs_sb.acl_start = BitConverter.ToUInt32(hpfs_sb_sector, 0x058);

            hpfs_sp.magic1 = BitConverter.ToUInt32(hpfs_sp_sector, 0x000);
            hpfs_sp.magic2 = BitConverter.ToUInt32(hpfs_sp_sector, 0x004);
            hpfs_sp.flags1 = hpfs_sp_sector[0x008];
            hpfs_sp.flags2 = hpfs_sp_sector[0x009];
            hpfs_sp.dummy = BitConverter.ToUInt16(hpfs_sp_sector, 0x00A);
            hpfs_sp.hotfix_start = BitConverter.ToUInt32(hpfs_sp_sector, 0x00C);
            hpfs_sp.hotfix_used = BitConverter.ToUInt32(hpfs_sp_sector, 0x010);
            hpfs_sp.hotfix_entries = BitConverter.ToUInt32(hpfs_sp_sector, 0x014);
            hpfs_sp.spare_dnodes_free = BitConverter.ToUInt32(hpfs_sp_sector, 0x018);
            hpfs_sp.spare_dnodes = BitConverter.ToUInt32(hpfs_sp_sector, 0x01C);
            hpfs_sp.codepage_lsn = BitConverter.ToUInt32(hpfs_sp_sector, 0x020);
            hpfs_sp.codepages = BitConverter.ToUInt32(hpfs_sp_sector, 0x024);
            hpfs_sp.sb_crc32 = BitConverter.ToUInt32(hpfs_sp_sector, 0x028);
            hpfs_sp.sp_crc32 = BitConverter.ToUInt32(hpfs_sp_sector, 0x02C);
			
            if (hpfs_bpb.fs_type != "HPFS    " ||
            hpfs_sb.magic1 != 0xF995E849 || hpfs_sb.magic2 != 0xFA53E9C5 ||
            hpfs_sp.magic1 != 0xF9911849 || hpfs_sp.magic2 != 0xFA5229C5)
            {
                sb.AppendLine("This may not be HPFS, following information may be not correct.");
                sb.AppendFormat("File system type: \"{0}\" (Should be \"HPFS    \")", hpfs_bpb.fs_type).AppendLine();
                sb.AppendFormat("Superblock magic1: 0x{0:X8} (Should be 0xF995E849)", hpfs_sb.magic1).AppendLine();
                sb.AppendFormat("Superblock magic2: 0x{0:X8} (Should be 0xFA53E9C5)", hpfs_sb.magic2).AppendLine();
                sb.AppendFormat("Spareblock magic1: 0x{0:X8} (Should be 0xF9911849)", hpfs_sp.magic1).AppendLine();
                sb.AppendFormat("Spareblock magic2: 0x{0:X8} (Should be 0xFA5229C5)", hpfs_sp.magic2).AppendLine();
            }
			
            sb.AppendFormat("OEM name: {0}", hpfs_bpb.OEMName).AppendLine();
            sb.AppendFormat("{0} bytes per sector", hpfs_bpb.bps).AppendLine();
            sb.AppendFormat("{0} sectors per cluster", hpfs_bpb.spc).AppendLine();
//			sb.AppendFormat("{0} reserved sectors", hpfs_bpb.rsectors).AppendLine();
//			sb.AppendFormat("{0} FATs", hpfs_bpb.fats_no).AppendLine();
//			sb.AppendFormat("{0} entries on root directory", hpfs_bpb.root_ent).AppendLine();
//			sb.AppendFormat("{0} mini sectors on volume", hpfs_bpb.sectors).AppendLine();
            sb.AppendFormat("Media descriptor: 0x{0:X2}", hpfs_bpb.media).AppendLine();
//			sb.AppendFormat("{0} sectors per FAT", hpfs_bpb.spfat).AppendLine();
//			sb.AppendFormat("{0} sectors per track", hpfs_bpb.sptrk).AppendLine();
//			sb.AppendFormat("{0} heads", hpfs_bpb.heads).AppendLine();
            sb.AppendFormat("{0} sectors hidden before BPB", hpfs_bpb.hsectors).AppendLine();
            sb.AppendFormat("{0} sectors on volume ({1} bytes)", hpfs_bpb.big_sectors, hpfs_bpb.big_sectors * hpfs_bpb.bps).AppendLine();
            sb.AppendFormat("BIOS Drive Number: 0x{0:X2}", hpfs_bpb.drive_no).AppendLine();
//			sb.AppendFormat("NT Flags: 0x{0:X2}", hpfs_bpb.nt_flags).AppendLine();
            sb.AppendFormat("Signature: 0x{0:X2}", hpfs_bpb.signature).AppendLine();
            sb.AppendFormat("Serial number: 0x{0:X8}", hpfs_bpb.serial_no).AppendLine();
            sb.AppendFormat("Volume label: {0}", hpfs_bpb.volume_label).AppendLine();
//			sb.AppendFormat("Filesystem type: \"{0}\"", hpfs_bpb.fs_type).AppendLine();
			
            DateTime last_chk = DateHandlers.UNIXToDateTime(hpfs_sb.last_chkdsk);
            DateTime last_optim = DateHandlers.UNIXToDateTime(hpfs_sb.last_optim);
			
            sb.AppendFormat("HPFS version: {0}", hpfs_sb.version).AppendLine();
            sb.AppendFormat("Functional version: {0}", hpfs_sb.func_version).AppendLine();
            sb.AppendFormat("Sector of root directory FNode: {0}", hpfs_sb.root_fnode).AppendLine();
//			sb.AppendFormat("{0} sectors on volume", hpfs_sb.sectors).AppendLine();
            sb.AppendFormat("{0} sectors are marked bad", hpfs_sb.badblocks).AppendLine();
            sb.AppendFormat("Sector of free space bitmaps: {0}", hpfs_sb.bitmap_lsn).AppendLine();
            sb.AppendFormat("Sector of bad blocks list: {0}", hpfs_sb.badblock_lsn).AppendLine();
            sb.AppendFormat("Date of last integrity check: {0}", last_chk).AppendLine();
            if (hpfs_sb.last_optim > 0)
                sb.AppendFormat("Date of last optimization {0}", last_optim).AppendLine();
            else
                sb.AppendLine("Filesystem has never been optimized");
            sb.AppendFormat("Directory band has {0} sectors", hpfs_sb.dband_sectors).AppendLine();
            sb.AppendFormat("Directory band starts at sector {0}", hpfs_sb.dband_start).AppendLine();
            sb.AppendFormat("Directory band ends at sector {0}", hpfs_sb.dband_last).AppendLine();
            sb.AppendFormat("Sector of directory band bitmap: {0}", hpfs_sb.dband_bitmap).AppendLine();
            sb.AppendFormat("Sector of ACL directory: {0}", hpfs_sb.acl_start).AppendLine();
			
            sb.AppendFormat("Sector of Hotfix directory: {0}", hpfs_sp.hotfix_start).AppendLine();
            sb.AppendFormat("{0} used Hotfix entries", hpfs_sp.hotfix_used).AppendLine();
            sb.AppendFormat("{0} total Hotfix entries", hpfs_sp.hotfix_entries).AppendLine();
            sb.AppendFormat("{0} free spare DNodes", hpfs_sp.spare_dnodes_free).AppendLine();
            sb.AppendFormat("{0} total spare DNodes", hpfs_sp.spare_dnodes).AppendLine();
            sb.AppendFormat("Sector of codepage directory: {0}", hpfs_sp.codepage_lsn).AppendLine();
            sb.AppendFormat("{0} codepages used in the volume", hpfs_sp.codepages).AppendLine();
            sb.AppendFormat("SuperBlock CRC32: {0:X8}", hpfs_sp.sb_crc32).AppendLine();
            sb.AppendFormat("SpareBlock CRC32: {0:X8}", hpfs_sp.sp_crc32).AppendLine();
			
            sb.AppendLine("Flags:");
            if ((hpfs_sp.flags1 & 0x01) == 0x01)
                sb.AppendLine("Filesystem is dirty.");
            else
                sb.AppendLine("Filesystem is clean.");
            if ((hpfs_sp.flags1 & 0x02) == 0x02)
                sb.AppendLine("Spare directory blocks are in use");
            if ((hpfs_sp.flags1 & 0x04) == 0x04)
                sb.AppendLine("Hotfixes are in use");
            if ((hpfs_sp.flags1 & 0x08) == 0x08)
                sb.AppendLine("Disk contains bad sectors");
            if ((hpfs_sp.flags1 & 0x10) == 0x10)
                sb.AppendLine("Disk has a bad bitmap");
            if ((hpfs_sp.flags1 & 0x20) == 0x20)
                sb.AppendLine("Filesystem was formatted fast");
            if ((hpfs_sp.flags1 & 0x40) == 0x40)
                sb.AppendLine("Unknown flag 0x40 on flags1 is active");
            if ((hpfs_sp.flags1 & 0x80) == 0x80)
                sb.AppendLine("Filesystem has been mounted by an old IFS");
            if ((hpfs_sp.flags2 & 0x01) == 0x01)
                sb.AppendLine("Install DASD limits");
            if ((hpfs_sp.flags2 & 0x02) == 0x02)
                sb.AppendLine("Resync DASD limits");
            if ((hpfs_sp.flags2 & 0x04) == 0x04)
                sb.AppendLine("DASD limits are operational");
            if ((hpfs_sp.flags2 & 0x08) == 0x08)
                sb.AppendLine("Multimedia is active");
            if ((hpfs_sp.flags2 & 0x10) == 0x10)
                sb.AppendLine("DCE ACLs are active");
            if ((hpfs_sp.flags2 & 0x20) == 0x20)
                sb.AppendLine("DASD limits are dirty");
            if ((hpfs_sp.flags2 & 0x40) == 0x40)
                sb.AppendLine("Unknown flag 0x40 on flags2 is active");
            if ((hpfs_sp.flags2 & 0x80) == 0x80)
                sb.AppendLine("Unknown flag 0x80 on flags2 is active");
			
            information = sb.ToString();
        }

        struct HPFS_BIOSParameterBlock // Sector 0
        {
            public byte jmp1;
            // 0x000, Jump to boot code
            public UInt16 jmp2;
            // 0x001, ...;
            public string OEMName;
            // 0x003, OEM Name, 8 bytes, space-padded
            public UInt16 bps;
            // 0x00B, Bytes per sector
            public byte spc;
            // 0x00D, Sectors per cluster
            public UInt16 rsectors;
            // 0x00E, Reserved sectors between BPB and... does it have sense in HPFS?
            public byte fats_no;
            // 0x010, Number of FATs... seriously?
            public UInt16 root_ent;
            // 0x011, Number of entries on root directory... ok
            public UInt16 sectors;
            // 0x013, Sectors in volume... doubt it
            public byte media;
            // 0x015, Media descriptor
            public UInt16 spfat;
            // 0x016, Sectors per FAT... again
            public UInt16 sptrk;
            // 0x018, Sectors per track... you're kidding
            public UInt16 heads;
            // 0x01A, Heads... stop!
            public UInt32 hsectors;
            // 0x01C, Hidden sectors before BPB
            public UInt32 big_sectors;
            // 0x024, Sectors in volume if > 65535...
            public byte drive_no;
            // 0x028, Drive number
            public byte nt_flags;
            // 0x029, Volume flags?
            public byte signature;
            // 0x02A, EPB signature, 0x29
            public UInt32 serial_no;
            // 0x02B, Volume serial number
            public string volume_label;
            // 0x02F, Volume label, 11 bytes, space-padded
            public string fs_type;
            // 0x03A, Filesystem type, 8 bytes, space-padded ("HPFS    ")
        }

        struct HPFS_SuperBlock // Sector 16
        {
            public UInt32 magic1;
            // 0x000, 0xF995E849
            public UInt32 magic2;
            // 0x004, 0xFA53E9C5
            public byte version;
            // 0x008, HPFS version
            public byte func_version;
            // 0x009, 2 if <= 4 GiB, 3 if > 4 GiB
            public UInt16 dummy;
            // 0x00A, Alignment
            public UInt32 root_fnode;
            // 0x00C, LSN pointer to root fnode
            public UInt32 sectors;
            // 0x010, Sectors on volume
            public UInt32 badblocks;
            // 0x014, Bad blocks on volume
            public UInt32 bitmap_lsn;
            // 0x018, LSN pointer to volume bitmap
            public UInt32 zero1;
            // 0x01C, 0
            public UInt32 badblock_lsn;
            // 0x020, LSN pointer to badblock directory
            public UInt32 zero2;
            // 0x024, 0
            public Int32 last_chkdsk;
            // 0x028, Time of last CHKDSK
            public Int32 last_optim;
            // 0x02C, Time of last optimization
            public UInt32 dband_sectors;
            // 0x030, Sectors of dir band
            public UInt32 dband_start;
            // 0x034, Start sector of dir band
            public UInt32 dband_last;
            // 0x038, Last sector of dir band
            public UInt32 dband_bitmap;
            // 0x03C, LSN of free space bitmap
            public UInt64 zero3;
            // 0x040, Can be used for volume name (32 bytes)
            public UInt64 zero4;
            // 0x048, ...
            public UInt64 zero5;
            // 0x04C, ...
            public UInt64 zero6;
            // 0x050, ...;
            public UInt32 acl_start;
            // 0x058, LSN pointer to ACLs (only HPFS386)
        }

        struct HPFS_SpareBlock // Sector 17
        {
            public UInt32 magic1;
            // 0x000, 0xF9911849
            public UInt32 magic2;
            // 0x004, 0xFA5229C5
            public byte flags1;
            // 0x008, HPFS flags
            public byte flags2;
            // 0x009, HPFS386 flags
            public UInt16 dummy;
            // 0x00A, Alignment
            public UInt32 hotfix_start;
            // 0x00C, LSN of hotfix directory
            public UInt32 hotfix_used;
            // 0x010, Used hotfixes
            public UInt32 hotfix_entries;
            // 0x014, Total hotfixes available
            public UInt32 spare_dnodes_free;
            // 0x018, Unused spare dnodes
            public UInt32 spare_dnodes;
            // 0x01C, Length of spare dnodes list
            public UInt32 codepage_lsn;
            // 0x020, LSN of codepage directory
            public UInt32 codepages;
            // 0x024, Number of codepages used
            public UInt32 sb_crc32;
            // 0x028, SuperBlock CRC32 (only HPFS386)
            public UInt32 sp_crc32;
            // 0x02C, SpareBlock CRC32 (only HPFS386)
        }
    }
}
