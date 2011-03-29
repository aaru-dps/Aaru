using System;
using System.IO;
using System.Text;
using FileSystemIDandChk;

// Information from Inside Macintosh

namespace FileSystemIDandChk.Plugins
{
	class NTFS : Plugin
	{
		public NTFS(PluginBase Core)
        {
            base.Name = "New Technology File System (NTFS)";
			base.PluginUUID = new Guid("33513B2C-1e6d-4d21-a660-0bbc789c3871");
        }
		
		public override bool Identify(FileStream stream, long offset)
		{
			byte[] eigth_bytes = new byte[8];
			byte signature1, fats_no;
			UInt16 spfat, signature2;
			string oem_name;
			
			BinaryReader br = new BinaryReader(stream);
			
			br.BaseStream.Seek(3 + offset, SeekOrigin.Begin);
			eigth_bytes = br.ReadBytes(8);
			oem_name = StringHandlers.CToString(eigth_bytes);
			
			if(oem_name != "NTFS    ")
				return false;
			
			br.BaseStream.Seek(0x10 + offset, SeekOrigin.Begin);
			fats_no = br.ReadByte();
			
			if(fats_no != 0)
				return false;
			
			br.BaseStream.Seek(0x16 + offset, SeekOrigin.Begin);
			spfat = br.ReadUInt16();
			
			if(spfat != 0)
				return false;
			
			br.BaseStream.Seek(0x26 + offset, SeekOrigin.Begin);
			signature1 = br.ReadByte();
			
			if(signature1 != 0x80)
				return false;
			
			br.BaseStream.Seek(0x1FE + offset, SeekOrigin.Begin);
			signature2 = br.ReadUInt16();
			
			if(signature2 != 0xAA55)
				return false;
			
			return true;
		}
		
		public override void GetInformation (FileStream stream, long offset, out string information)
		{
			information = "";
			
			StringBuilder sb = new StringBuilder();
			
			BinaryReader br = new BinaryReader(stream);
			
			br.BaseStream.Seek(offset, SeekOrigin.Begin);
			
			NTFS_BootBlock ntfs_bb = new NTFS_BootBlock();
			
			byte[] oem_name = new byte[8];
			
			ntfs_bb.jmp1 = br.ReadByte();
			ntfs_bb.jmp2 = br.ReadUInt16();
			oem_name = br.ReadBytes(8);
			ntfs_bb.OEMName = StringHandlers.CToString(oem_name);
			ntfs_bb.bps = br.ReadUInt16();
			ntfs_bb.spc = br.ReadByte();
			ntfs_bb.rsectors = br.ReadUInt16();
			ntfs_bb.fats_no = br.ReadByte();
			ntfs_bb.root_ent = br.ReadUInt16();
			ntfs_bb.sml_sectors = br.ReadUInt16();
			ntfs_bb.media = br.ReadByte();
			ntfs_bb.spfat = br.ReadUInt16();
			ntfs_bb.sptrk = br.ReadUInt16();
			ntfs_bb.heads = br.ReadUInt16();
			ntfs_bb.hsectors = br.ReadUInt32();
			ntfs_bb.big_sectors = br.ReadUInt32();
			ntfs_bb.drive_no = br.ReadByte();
			ntfs_bb.nt_flags = br.ReadByte();
			ntfs_bb.signature1 = br.ReadByte();
			ntfs_bb.dummy = br.ReadByte();
			ntfs_bb.sectors = br.ReadInt64();
			ntfs_bb.mft_lsn = br.ReadInt64();
			ntfs_bb.mftmirror_lsn = br.ReadInt64();
			ntfs_bb.mft_rc_clusters = br.ReadSByte();
			ntfs_bb.dummy2 = br.ReadByte();
			ntfs_bb.dummy3 = br.ReadUInt16();
			ntfs_bb.index_blk_cts = br.ReadSByte();
			ntfs_bb.dummy4 = br.ReadByte();
			ntfs_bb.dummy5 = br.ReadUInt16();
			ntfs_bb.serial_no = br.ReadUInt64();
			br.BaseStream.Seek(430, SeekOrigin.Current);
			ntfs_bb.signature2 = br.ReadUInt16();
			
			sb.AppendFormat("{0} bytes per sector", ntfs_bb.bps).AppendLine();
			sb.AppendFormat("{0} sectors per cluster ({1} bytes)", ntfs_bb.spc, ntfs_bb.spc*ntfs_bb.bps).AppendLine();
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
			sb.AppendFormat("{0} sectors on volume ({1} bytes)", ntfs_bb.sectors, ntfs_bb.sectors*ntfs_bb.bps).AppendLine();
			sb.AppendFormat("Sectors where $MFT starts: {0}", ntfs_bb.mft_lsn).AppendLine();
			sb.AppendFormat("Sectors where $MFTMirr starts: {0}", ntfs_bb.mftmirror_lsn).AppendLine();

			if (ntfs_bb.mft_rc_clusters > 0)
				sb.AppendFormat("{0} clusters per MFT record ({1} bytes)", ntfs_bb.mft_rc_clusters,
				                ntfs_bb.mft_rc_clusters*ntfs_bb.bps*ntfs_bb.spc).AppendLine();
			else
				sb.AppendFormat("{0} bytes per MFT record", 1 << -ntfs_bb.mft_rc_clusters).AppendLine();
			if (ntfs_bb.index_blk_cts > 0)
				sb.AppendFormat("{0} clusters per Index block ({1} bytes)", ntfs_bb.index_blk_cts,
				                ntfs_bb.index_blk_cts*ntfs_bb.bps*ntfs_bb.spc).AppendLine();
			else
				sb.AppendFormat("{0} bytes per Index block", 1 << -ntfs_bb.index_blk_cts).AppendLine();

			sb.AppendFormat("Volume serial number: {0:X16}", ntfs_bb.serial_no).AppendLine();
//			sb.AppendFormat("Signature 2: 0x{0:X4}", ntfs_bb.signature2).AppendLine();
			
			information = sb.ToString();
		}
		
		private struct NTFS_BootBlock // Sector 0
		{
			// BIOS Parameter Block
			public byte   jmp1;            // Jump to boot code
			public UInt16 jmp2;            // ...;
			public string OEMName;         // OEM Name, 8 bytes, space-padded, must be "NTFS    "
			public UInt16 bps;             // Bytes per sector
			public byte   spc;             // Sectors per cluster
			public UInt16 rsectors;        // Reserved sectors, seems 0
			public byte   fats_no;         // Number of FATs... obviously, 0
			public UInt16 root_ent;        // Number of entries on root directory... 0
			public UInt16 sml_sectors;     // Sectors in volume... 0
			public byte   media;           // Media descriptor
			public UInt16 spfat;           // Sectors per FAT... 0
			public UInt16 sptrk;           // Sectors per track, required to boot
			public UInt16 heads;           // Heads... required to boot
			public UInt32 hsectors;        // Hidden sectors before BPB
			public UInt32 big_sectors;     // Sectors in volume if > 65535... 0
			public byte   drive_no;        // Drive number
			public byte   nt_flags;        // 0
			public byte   signature1;       // EPB signature, 0x80
			public byte   dummy;           // Alignment
			// End of BIOS Parameter Block
			// NTFS real superblock
			public Int64  sectors;         // Sectors on volume
			public Int64  mft_lsn;         // LSN of $MFT
			public Int64  mftmirror_lsn;   // LSN of $MFTMirror
			public sbyte  mft_rc_clusters; // Clusters per MFT record
			public byte   dummy2;          // Alignment
			public UInt16 dummy3;          // Alignment
			public sbyte  index_blk_cts;   // Clusters per index block
			public byte   dummy4;          // Alignment
			public UInt16 dummy5;          // Alignment
			public UInt64 serial_no;       // Volume serial number
			// End of NTFS superblock, followed by 426 bytes of boot code
			public UInt16 signature2;      // 0xAA55
		}
	}
}

