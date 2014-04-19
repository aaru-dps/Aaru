/***************************************************************************
FileSystem identifier and checker
----------------------------------------------------------------------------
 
Filename       : NTFS.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Filesystem plugins

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Identifies Windows NT FileSystem (aka NTFS) and shows information.
 
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
using FileSystemIDandChk;

// Information from Inside Windows NT
namespace FileSystemIDandChk.Plugins
{
    class NTFS : Plugin
    {
        public NTFS(PluginBase Core)
        {
            Name = "New Technology File System (NTFS)";
            PluginUUID = new Guid("33513B2C-1e6d-4d21-a660-0bbc789c3871");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset)
        {
            byte[] eigth_bytes = new byte[8];
            byte signature1, fats_no;
            UInt16 spfat, signature2;
            string oem_name;
			
            byte[] ntfs_bpb = imagePlugin.ReadSector(0 + partitionOffset);
			
            Array.Copy(ntfs_bpb, 0x003, eigth_bytes, 0, 8);
            oem_name = StringHandlers.CToString(eigth_bytes);
			
            if (oem_name != "NTFS    ")
                return false;
			
            fats_no = ntfs_bpb[0x010];
			
            if (fats_no != 0)
                return false;
			
            spfat = BitConverter.ToUInt16(ntfs_bpb, 0x016);
			
            if (spfat != 0)
                return false;
			
            signature1 = ntfs_bpb[0x026];
			
            if (signature1 != 0x80)
                return false;
			
            signature2 = BitConverter.ToUInt16(ntfs_bpb, 0x1FE);
			
            return signature2 == 0xAA55;
			
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionOffset, out string information)
        {
            information = "";
			
            StringBuilder sb = new StringBuilder();
			
            byte[] ntfs_bpb = imagePlugin.ReadSector(0 + partitionOffset);
			
            NTFS_BootBlock ntfs_bb = new NTFS_BootBlock();
			
            byte[] oem_name = new byte[8];
			
            ntfs_bb.jmp1 = ntfs_bpb[0x000];
            ntfs_bb.jmp2 = BitConverter.ToUInt16(ntfs_bpb, 0x001);
            Array.Copy(ntfs_bpb, 0x003, oem_name, 0, 8);
            ntfs_bb.OEMName = StringHandlers.CToString(oem_name);
            ntfs_bb.bps = BitConverter.ToUInt16(ntfs_bpb, 0x00B);
            ntfs_bb.spc = ntfs_bpb[0x00D];
            ntfs_bb.rsectors = BitConverter.ToUInt16(ntfs_bpb, 0x00E);
            ntfs_bb.fats_no = ntfs_bpb[0x010];
            ntfs_bb.root_ent = BitConverter.ToUInt16(ntfs_bpb, 0x011);
            ntfs_bb.sml_sectors = BitConverter.ToUInt16(ntfs_bpb, 0x013);
            ntfs_bb.media = ntfs_bpb[0x015];
            ntfs_bb.spfat = BitConverter.ToUInt16(ntfs_bpb, 0x016);
            ntfs_bb.sptrk = BitConverter.ToUInt16(ntfs_bpb, 0x018);
            ntfs_bb.heads = BitConverter.ToUInt16(ntfs_bpb, 0x01A);
            ntfs_bb.hsectors = BitConverter.ToUInt32(ntfs_bpb, 0x01C);
            ntfs_bb.big_sectors = BitConverter.ToUInt32(ntfs_bpb, 0x020);
            ntfs_bb.drive_no = ntfs_bpb[0x024];
            ntfs_bb.nt_flags = ntfs_bpb[0x025];
            ntfs_bb.signature1 = ntfs_bpb[0x026];
            ntfs_bb.dummy = ntfs_bpb[0x027];
            ntfs_bb.sectors = BitConverter.ToInt64(ntfs_bpb, 0x028);
            ntfs_bb.mft_lsn = BitConverter.ToInt64(ntfs_bpb, 0x030);
            ntfs_bb.mftmirror_lsn = BitConverter.ToInt64(ntfs_bpb, 0x038);
            ntfs_bb.mft_rc_clusters = (sbyte)ntfs_bpb[0x040];
            ntfs_bb.dummy2 = ntfs_bpb[0x041];
            ntfs_bb.dummy3 = BitConverter.ToUInt16(ntfs_bpb, 0x042);
            ntfs_bb.index_blk_cts = (sbyte)ntfs_bpb[0x044];
            ntfs_bb.dummy4 = ntfs_bpb[0x045];
            ntfs_bb.dummy5 = BitConverter.ToUInt16(ntfs_bpb, 0x046);
            ntfs_bb.serial_no = BitConverter.ToUInt64(ntfs_bpb, 0x048);
            ntfs_bb.signature2 = BitConverter.ToUInt16(ntfs_bpb, 0x1FE);
			
            sb.AppendFormat("{0} bytes per sector", ntfs_bb.bps).AppendLine();
            sb.AppendFormat("{0} sectors per cluster ({1} bytes)", ntfs_bb.spc, ntfs_bb.spc * ntfs_bb.bps).AppendLine();
//			sb.AppendFormat("{0} reserved sectors", ntfs_bb.rsectors).AppendLine();
//			sb.AppendFormat("{0} FATs", ntfs_bb.fats_no).AppendLine();
//			sb.AppendFormat("{0} entries in the root folder", ntfs_bb.root_ent).AppendLine();
//			sb.AppendFormat("{0} sectors on volume (small)", ntfs_bb.sml_sectors).AppendLine();
            sb.AppendFormat("Media descriptor: 0x{0:X2}", ntfs_bb.media).AppendLine();
//			sb.AppendFormat("{0} sectors per FAT", ntfs_bb.spfat).AppendLine();
            sb.AppendFormat("{0} sectors per track", ntfs_bb.sptrk).AppendLine();
            sb.AppendFormat("{0} heads", ntfs_bb.heads).AppendLine();
            sb.AppendFormat("{0} hidden sectors before filesystem", ntfs_bb.hsectors).AppendLine();
//			sb.AppendFormat("{0} sectors on volume (big)", ntfs_bb.big_sectors).AppendLine();
            sb.AppendFormat("BIOS drive number: 0x{0:X2}", ntfs_bb.drive_no).AppendLine();
//			sb.AppendFormat("NT flags: 0x{0:X2}", ntfs_bb.nt_flags).AppendLine();
//			sb.AppendFormat("Signature 1: 0x{0:X2}", ntfs_bb.signature1).AppendLine();
            sb.AppendFormat("{0} sectors on volume ({1} bytes)", ntfs_bb.sectors, ntfs_bb.sectors * ntfs_bb.bps).AppendLine();
            sb.AppendFormat("Sectors where $MFT starts: {0}", ntfs_bb.mft_lsn).AppendLine();
            sb.AppendFormat("Sectors where $MFTMirr starts: {0}", ntfs_bb.mftmirror_lsn).AppendLine();

            if (ntfs_bb.mft_rc_clusters > 0)
                sb.AppendFormat("{0} clusters per MFT record ({1} bytes)", ntfs_bb.mft_rc_clusters,
                    ntfs_bb.mft_rc_clusters * ntfs_bb.bps * ntfs_bb.spc).AppendLine();
            else
                sb.AppendFormat("{0} bytes per MFT record", 1 << -ntfs_bb.mft_rc_clusters).AppendLine();
            if (ntfs_bb.index_blk_cts > 0)
                sb.AppendFormat("{0} clusters per Index block ({1} bytes)", ntfs_bb.index_blk_cts,
                    ntfs_bb.index_blk_cts * ntfs_bb.bps * ntfs_bb.spc).AppendLine();
            else
                sb.AppendFormat("{0} bytes per Index block", 1 << -ntfs_bb.index_blk_cts).AppendLine();

            sb.AppendFormat("Volume serial number: {0:X16}", ntfs_bb.serial_no).AppendLine();
//			sb.AppendFormat("Signature 2: 0x{0:X4}", ntfs_bb.signature2).AppendLine();
			
            information = sb.ToString();
        }

        struct NTFS_BootBlock // Sector 0
        {
            // BIOS Parameter Block
            public byte jmp1;
            // 0x000, Jump to boot code
            public UInt16 jmp2;
            // 0x001, ...;
            public string OEMName;
            // 0x003, OEM Name, 8 bytes, space-padded, must be "NTFS    "
            public UInt16 bps;
            // 0x00B, Bytes per sector
            public byte spc;
            // 0x00D, Sectors per cluster
            public UInt16 rsectors;
            // 0x00E, Reserved sectors, seems 0
            public byte fats_no;
            // 0x010, Number of FATs... obviously, 0
            public UInt16 root_ent;
            // 0x011, Number of entries on root directory... 0
            public UInt16 sml_sectors;
            // 0x013, Sectors in volume... 0
            public byte media;
            // 0x015, Media descriptor
            public UInt16 spfat;
            // 0x016, Sectors per FAT... 0
            public UInt16 sptrk;
            // 0x018, Sectors per track, required to boot
            public UInt16 heads;
            // 0x01A, Heads... required to boot
            public UInt32 hsectors;
            // 0x01C, Hidden sectors before BPB
            public UInt32 big_sectors;
            // 0x020, Sectors in volume if > 65535... 0
            public byte drive_no;
            // 0x024, Drive number
            public byte nt_flags;
            // 0x025, 0
            public byte signature1;
            // 0x026, EPB signature, 0x80
            public byte dummy;
            // 0x027, Alignment
            // End of BIOS Parameter Block
            // NTFS real superblock
            public Int64 sectors;
            // 0x028, Sectors on volume
            public Int64 mft_lsn;
            // 0x030, LSN of $MFT
            public Int64 mftmirror_lsn;
            // 0x038, LSN of $MFTMirror
            public sbyte mft_rc_clusters;
            // 0x040, Clusters per MFT record
            public byte dummy2;
            // 0x041, Alignment
            public UInt16 dummy3;
            // 0x042, Alignment
            public sbyte index_blk_cts;
            // 0x044, Clusters per index block
            public byte dummy4;
            // 0x045, Alignment
            public UInt16 dummy5;
            // 0x046, Alignment
            public UInt64 serial_no;
            // 0x048, Volume serial number
            // End of NTFS superblock, followed by 430 bytes of boot code
            public UInt16 signature2;
            // 0x1FE, 0xAA55
        }
    }
}
