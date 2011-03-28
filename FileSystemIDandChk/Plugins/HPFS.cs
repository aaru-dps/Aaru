using System;
using System.IO;
using System.Text;
using FileSystemIDandChk;

// Information from Inside Macintosh

namespace FileSystemIDandChk.Plugins
{
	class HPFS : Plugin
	{
		public HPFS(PluginBase Core)
        {
            base.Name = "OS/2 High Performance File System";
			base.PluginUUID = new Guid("33513B2C-f590-4acb-8bf2-0b1d5e19dec5");
        }
		
		public override bool Identify(FileStream stream, long offset)
		{
			UInt16 bps;
			UInt32 magic1, magic2;
			
			BinaryReader br = new BinaryReader(stream);
			
			br.BaseStream.Seek(offset + 3 + 8, SeekOrigin.Begin); // Seek to bps
			bps = br.ReadUInt16();
			
			br.BaseStream.Seek(offset + (16 * bps), SeekOrigin.Begin); // Seek to superblock, on logical sector 16
			magic1 = br.ReadUInt32();
			magic2 = br.ReadUInt32();
			
			if(magic1 == 0xF995E849 && magic2 == 0xFA53E9C5)
				return true;
			else
				return false;
		}
		
		public override void GetInformation (FileStream stream, long offset, out string information)
		{
			information = "";
			
			StringBuilder sb = new StringBuilder();
			
			BinaryReader br = new BinaryReader(stream);
			
			HPFS_BIOSParameterBlock hpfs_bpb = new HPFS_BIOSParameterBlock();
			HPFS_SuperBlock hpfs_sb = new HPFS_SuperBlock();
			HPFS_SpareBlock hpfs_sp = new HPFS_SpareBlock();
			
			byte[] oem_name = new byte[8];
			byte[] volume_name = new byte[11];
			
			br.BaseStream.Seek(offset, SeekOrigin.Begin); // Seek to BPB
			hpfs_bpb.jmp1 = br.ReadByte();
			hpfs_bpb.jmp2 = br.ReadUInt16();
			oem_name = br.ReadBytes(8);
			hpfs_bpb.OEMName = StringHandlers.CToString(oem_name);
			hpfs_bpb.bps = br.ReadUInt16();
			hpfs_bpb.spc = br.ReadByte();
			hpfs_bpb.rsectors = br.ReadUInt16();
			hpfs_bpb.fats_no = br.ReadByte();
			hpfs_bpb.root_ent = br.ReadUInt16();
			hpfs_bpb.sectors = br.ReadUInt16();
			hpfs_bpb.media = br.ReadByte();
			hpfs_bpb.spfat = br.ReadUInt16();
			hpfs_bpb.sptrk = br.ReadUInt16();
			hpfs_bpb.heads = br.ReadUInt16();
			hpfs_bpb.hsectors = br.ReadUInt32();
			hpfs_bpb.big_sectors = br.ReadUInt32();
			hpfs_bpb.drive_no = br.ReadByte();
			hpfs_bpb.nt_flags = br.ReadByte();
			hpfs_bpb.signature = br.ReadByte();
			hpfs_bpb.serial_no = br.ReadUInt32();
			volume_name = br.ReadBytes(11);
			hpfs_bpb.volume_label = StringHandlers.CToString(volume_name);
			oem_name = br.ReadBytes(8);
			hpfs_bpb.fs_type = StringHandlers.CToString(oem_name);
			
			br.BaseStream.Seek((16*hpfs_bpb.bps) + offset, SeekOrigin.Begin); // Seek to SuperBlock
			
			hpfs_sb.magic1 = br.ReadUInt32();
			hpfs_sb.magic2 = br.ReadUInt32();
			hpfs_sb.version = br.ReadByte();
			hpfs_sb.func_version = br.ReadByte();
			hpfs_sb.dummy = br.ReadUInt16();
			hpfs_sb.root_fnode = br.ReadUInt32();
			hpfs_sb.sectors = br.ReadUInt32();
			hpfs_sb.badblocks = br.ReadUInt32();
			hpfs_sb.bitmap_lsn = br.ReadUInt32();
			hpfs_sb.zero1 = br.ReadUInt32();
			hpfs_sb.badblock_lsn = br.ReadUInt32();
			hpfs_sb.zero2 = br.ReadUInt32();
			hpfs_sb.last_chkdsk = br.ReadInt32();
			hpfs_sb.last_optim = br.ReadInt32();
			hpfs_sb.dband_sectors = br.ReadUInt32();
			hpfs_sb.dband_start = br.ReadUInt32();
			hpfs_sb.dband_last = br.ReadUInt32();
			hpfs_sb.dband_bitmap = br.ReadUInt32();
			hpfs_sb.zero3 = br.ReadUInt64();
			hpfs_sb.zero4 = br.ReadUInt64();
			hpfs_sb.zero5 = br.ReadUInt64();
			hpfs_sb.zero6 = br.ReadUInt64();

			br.BaseStream.Seek((17*hpfs_bpb.bps) + offset, SeekOrigin.Begin); // Seek to SuperBlock
			
			hpfs_sp.magic1 = br.ReadUInt32();
			hpfs_sp.magic2 = br.ReadUInt32();
			hpfs_sp.flags1 = br.ReadByte();
			hpfs_sp.flags2 = br.ReadByte();
			hpfs_sp.dummy = br.ReadUInt16();
			hpfs_sp.hotfix_start = br.ReadUInt32();
			hpfs_sp.hotfix_used = br.ReadUInt32();
			hpfs_sp.hotfix_entries = br.ReadUInt32();
			hpfs_sp.spare_dnodes_free = br.ReadUInt32();
			hpfs_sp.spare_dnodes = br.ReadUInt32();
			hpfs_sp.codepage_lsn = br.ReadUInt32();
			hpfs_sp.codepages = br.ReadUInt32();
			hpfs_sp.sb_crc32 = br.ReadUInt32();
			hpfs_sp.sp_crc32 = br.ReadUInt32();
			
			if(hpfs_bpb.fs_type != "HPFS    " ||
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
			sb.AppendFormat("{0} sectors on volume ({1} bytes)", hpfs_bpb.big_sectors, hpfs_bpb.big_sectors*hpfs_bpb.bps).AppendLine();
			sb.AppendFormat("BIOS Drive Number: 0x{0:X2}", hpfs_bpb.drive_no).AppendLine();
//			sb.AppendFormat("NT Flags: 0x{0:X2}", hpfs_bpb.nt_flags).AppendLine();
			sb.AppendFormat("Signature: 0x{0:X2}", hpfs_bpb.signature).AppendLine();
			sb.AppendFormat("Serial number: 0x{0:X8}", hpfs_bpb.serial_no).AppendLine();
			sb.AppendFormat("Volume label: {0}", hpfs_bpb.volume_label).AppendLine();
//			sb.AppendFormat("Filesystem type: \"{0}\"", hpfs_bpb.fs_type).AppendLine();
			
			DateTime last_chk = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(hpfs_sb.last_chkdsk);
			DateTime last_optim = new DateTime(1970, 1, 1, 0, 0, 0).AddSeconds(hpfs_sb.last_optim);
			
			sb.AppendFormat("HPFS version: {0}", hpfs_sb.version).AppendLine();
			sb.AppendFormat("Functional version: {0}", hpfs_sb.func_version).AppendLine();
			sb.AppendFormat("Sector of root directory FNode: {0}", hpfs_sb.root_fnode).AppendLine();
//			sb.AppendFormat("{0} sectors on volume", hpfs_sb.sectors).AppendLine();
			sb.AppendFormat("{0} sectors are marked bad", hpfs_sb.badblocks).AppendLine();
			sb.AppendFormat("Sector of free space bitmaps: {0}", hpfs_sb.bitmap_lsn).AppendLine();
			sb.AppendFormat("Sector of bad blocks list: {0}", hpfs_sb.badblock_lsn).AppendLine();
			sb.AppendFormat("Date of last integrity check: {0}", last_chk).AppendLine();
			if(hpfs_sb.last_optim>0)
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
			if((hpfs_sp.flags1 & 0x01) == 0x01)
				sb.AppendLine("Filesystem is dirty.");
			else
				sb.AppendLine("Filesystem is clean.");
			if((hpfs_sp.flags1 & 0x02) == 0x02)
				sb.AppendLine("Spare directory blocks are in use");
			if((hpfs_sp.flags1 & 0x04) == 0x04)
				sb.AppendLine("Hotfixes are in use");
			if((hpfs_sp.flags1 & 0x08) == 0x08)
				sb.AppendLine("Disk contains bad sectors");
			if((hpfs_sp.flags1 & 0x10) == 0x10)
				sb.AppendLine("Disk has a bad bitmap");
			if((hpfs_sp.flags1 & 0x20) == 0x20)
				sb.AppendLine("Filesystem was formatted fast");
			if((hpfs_sp.flags1 & 0x40) == 0x40)
				sb.AppendLine("Unknown flag 0x40 on flags1 is active");
			if((hpfs_sp.flags1 & 0x80) == 0x80)
				sb.AppendLine("Filesystem has been mounted by an old IFS");
			if((hpfs_sp.flags2 & 0x01) == 0x01)
				sb.AppendLine("Install DASD limits");
			if((hpfs_sp.flags2 & 0x02) == 0x02)
				sb.AppendLine("Resync DASD limits");
			if((hpfs_sp.flags2 & 0x04) == 0x04)
				sb.AppendLine("DASD limits are operational");
			if((hpfs_sp.flags2 & 0x08) == 0x08)
				sb.AppendLine("Multimedia is active");
			if((hpfs_sp.flags2 & 0x10) == 0x10)
				sb.AppendLine("DCE ACLs are active");
			if((hpfs_sp.flags2 & 0x20) == 0x20)
				sb.AppendLine("DASD limits are dirty");
			if((hpfs_sp.flags2 & 0x40) == 0x40)
				sb.AppendLine("Unknown flag 0x40 on flags2 is active");
			if((hpfs_sp.flags2 & 0x80) == 0x80)
				sb.AppendLine("Unknown flag 0x80 on flags2 is active");
			
			information = sb.ToString();
		}
		
		private struct HPFS_BIOSParameterBlock // Sector 0
		{
			public byte   jmp1;        // Jump to boot code
			public UInt16 jmp2;        // ...;
			public string OEMName;     // OEM Name, 8 bytes, space-padded
			public UInt16 bps;         // Bytes per sector
			public byte   spc;         // Sectors per cluster
			public UInt16 rsectors;    // Reserved sectors between BPB and... does it have sense in HPFS?
			public byte   fats_no;     // Number of FATs... seriously?
			public UInt16 root_ent;    // Number of entries on root directory... ok
			public UInt16 sectors;     // Sectors in volume... doubt it
			public byte   media;       // Media descriptor
			public UInt16 spfat;       // Sectors per FAT... again
			public UInt16 sptrk;       // Sectors per track... you're kidding
			public UInt16 heads;       // Heads... stop!
			public UInt32 hsectors;    // Hidden sectors before BPB
			public UInt32 big_sectors; // Sectors in volume if > 65535...
			public byte   drive_no;     // Drive number
			public byte   nt_flags;     // Volume flags?
			public byte   signature;    // EPB signature, 0x29
			public UInt32 serial_no;    // Volume serial number
			public string volume_label; // Volume label, 11 bytes, space-padded
			public string fs_type;      // Filesystem type, 8 bytes, space-padded ("HPFS    ")
		}
		
		private struct HPFS_SuperBlock // Sector 16
		{
			public UInt32 magic1;        // 0xF995E849
			public UInt32 magic2;        // 0xFA53E9C5
			public byte   version;       // HPFS version
			public byte   func_version;  // 2 if <= 4 GiB, 3 if > 4 GiB
			public UInt16 dummy;         // Alignment
			public UInt32 root_fnode;    // LSN pointer to root fnode
			public UInt32 sectors;       // Sectors on volume
			public UInt32 badblocks;     // Bad blocks on volume
			public UInt32 bitmap_lsn;    // LSN pointer to volume bitmap
			public UInt32 zero1;         // 0
			public UInt32 badblock_lsn;  // LSN pointer to badblock directory
			public UInt32 zero2;         // 0
			public Int32  last_chkdsk;   // Time of last CHKDSK
			public Int32  last_optim;    // Time of last optimization
			public UInt32 dband_sectors; // Sectors of dir band
			public UInt32 dband_start;   // Start sector of dir band
			public UInt32 dband_last;    // Last sector of dir band
			public UInt32 dband_bitmap;  // LSN of free space bitmap
			public UInt64 zero3;         // Can be used for volume name (32 bytes)
			public UInt64 zero4;         // ...
			public UInt64 zero5;         // ...
			public UInt64 zero6;         // ...;
			public UInt32 acl_start;     // LSN pointer to ACLs (only HPFS386)
		}
		
		private struct HPFS_SpareBlock // Sector 17
		{
			public UInt32 magic1;            // 0xF9911849
			public UInt32 magic2;            // 0xFA5229C5
			public byte   flags1;            // HPFS flags
			public byte   flags2;            // HPFS386 flags
			public UInt16 dummy;             // Alignment
			public UInt32 hotfix_start;      // LSN of hotfix directory
			public UInt32 hotfix_used;       // Used hotfixes
			public UInt32 hotfix_entries;    // Total hotfixes available
			public UInt32 spare_dnodes_free; // Unused spare dnodes
			public UInt32 spare_dnodes;      // Length of spare dnodes list
			public UInt32 codepage_lsn;      // LSN of codepage directory
			public UInt32 codepages;         // Number of codepages used
			public UInt32 sb_crc32;          // SuperBlock CRC32 (only HPFS386)
			public UInt32 sp_crc32;          // SpareBlock CRC32 (only HPFS386)
		}
	}
}

