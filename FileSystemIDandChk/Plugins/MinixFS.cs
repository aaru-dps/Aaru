using System;
using System.IO;
using System.Text;
using FileSystemIDandChk;

// Information from Inside Macintosh

namespace FileSystemIDandChk.Plugins
{
	class MinixFS : Plugin
	{
		private const UInt16 MINIX_MAGIC   = 0x137F; // Minix v1, 14 char filenames
		private const UInt16 MINIX_MAGIC2  = 0x138F; // Minix v1, 30 char filenames
		private const UInt16 MINIX2_MAGIC  = 0x2468; // Minix v2, 14 char filenames
		private const UInt16 MINIX2_MAGIC2 = 0x2478; // Minix v2, 30 char filenames
		private const UInt16 MINIX3_MAGIC  = 0x4D5A; // Minix v3, 60 char filenames

		// Byteswapped
		private const UInt16 MINIX_CIGAM   = 0x7F13; // Minix v1, 14 char filenames
		private const UInt16 MINIX_CIGAM2  = 0x8F13; // Minix v1, 30 char filenames
		private const UInt16 MINIX2_CIGAM  = 0x6824; // Minix v2, 14 char filenames
		private const UInt16 MINIX2_CIGAM2 = 0x7824; // Minix v2, 30 char filenames
		private const UInt16 MINIX3_CIGAM  = 0x5A4D; // Minix v3, 60 char filenames

		public MinixFS(PluginBase Core)
        {
            base.Name = "Minix Filesystem";
			base.PluginUUID = new Guid("FE248C3B-B727-4AE5-A39F-79EA9A07D4B3");
        }
		
		public override bool Identify(FileStream stream, long offset)
		{
			UInt16 magic;
			BinaryReader br = new BinaryReader(stream);

			br.BaseStream.Seek(0x400 + 0x10 + offset, SeekOrigin.Begin); // Here should reside magic number on Minix V1 & V2
			magic = br.ReadUInt16();
			
			if(magic == MINIX_MAGIC || magic == MINIX_MAGIC2 || magic == MINIX2_MAGIC || magic == MINIX2_MAGIC2 ||
			   magic == MINIX_CIGAM || magic == MINIX_CIGAM2 || magic == MINIX2_CIGAM || magic == MINIX2_CIGAM2)
				return true;
			else
			{
				br.BaseStream.Seek(0x400 + 0x18 + offset, SeekOrigin.Begin); // Here should reside magic number on Minix V1 & V2
				magic = br.ReadUInt16();

				if(magic == MINIX3_MAGIC || magic == MINIX3_CIGAM)
					return true;
				else
					return false;
			}
		}
		
		public override void GetInformation (FileStream stream, long offset, out string information)
		{
			information = "";
			
			StringBuilder sb = new StringBuilder();

			bool littleendian = true;
			bool minix3 = false;
			int filenamesize = 0;
			string minixVersion;
			UInt16 magic;
			EndianAwareBinaryReader eabr = new EndianAwareBinaryReader(stream, littleendian);

			eabr.BaseStream.Seek(0x400 + 0x18 + offset, SeekOrigin.Begin);
			magic = eabr.ReadUInt16();

			if(magic == MINIX3_MAGIC || magic == MINIX3_CIGAM)
			{
				filenamesize = 60;
				minixVersion = "Minix V3 filesystem";
				if(magic == MINIX3_CIGAM)
					littleendian = false;
				else
					littleendian = true;

				minix3 = true;
			}
			else
			{
				eabr.BaseStream.Seek(0x400 + 0x10 + offset, SeekOrigin.Begin);
				magic = eabr.ReadUInt16();

				switch(magic)
				{
				case MINIX_MAGIC:
					filenamesize = 14;
					minixVersion = "Minix V1 filesystem";
					littleendian = true;
					break;
				case MINIX_MAGIC2:
					filenamesize = 30;
					minixVersion = "Minix V1 filesystem";
					littleendian = true;
					break;
				case MINIX2_MAGIC:
					filenamesize = 14;
					minixVersion = "Minix V2 filesystem";
					littleendian = true;
					break;
				case MINIX2_MAGIC2:
					filenamesize = 30;
					minixVersion = "Minix V2 filesystem";
					littleendian = true;
					break;
				case MINIX_CIGAM:
					filenamesize = 14;
					minixVersion = "Minix V1 filesystem";
					littleendian = false;
					break;
				case MINIX_CIGAM2:
					filenamesize = 30;
					minixVersion = "Minix V1 filesystem";
					littleendian = false;
					break;
				case MINIX2_CIGAM:
					filenamesize = 14;
					minixVersion = "Minix V2 filesystem";
					littleendian = false;
					break;
				case MINIX2_CIGAM2:
					filenamesize = 30;
					minixVersion = "Minix V2 filesystem";
					littleendian = false;
					break;
				default:
					return;
					break;
				}
			}

			eabr = new EndianAwareBinaryReader(stream, littleendian);
			eabr.BaseStream.Seek(0x400 + offset, SeekOrigin.Begin);

			if(minix3)
			{
				Minix3SuperBlock mnx_sb = new Minix3SuperBlock();

				mnx_sb.s_ninodes = eabr.ReadUInt32();
				mnx_sb.s_pad0 = eabr.ReadUInt16();
				mnx_sb.s_imap_blocks = eabr.ReadUInt16();
				mnx_sb.s_zmap_blocks = eabr.ReadUInt16();
				mnx_sb.s_firstdatazone = eabr.ReadUInt16();
				mnx_sb.s_log_zone_size = eabr.ReadUInt16();
				mnx_sb.s_pad1 = eabr.ReadUInt16();
				mnx_sb.s_max_size = eabr.ReadUInt32();
				mnx_sb.s_zones = eabr.ReadUInt32();
				mnx_sb.s_magic = eabr.ReadUInt16();
				mnx_sb.s_pad2 = eabr.ReadUInt16();
				mnx_sb.s_blocksize = eabr.ReadUInt16();
				mnx_sb.s_disk_version = eabr.ReadByte();

				sb.AppendLine(minixVersion);
				sb.AppendFormat("{0} chars in filename", filenamesize).AppendLine();
				sb.AppendFormat("{0} zones on volume ({1} bytes)", mnx_sb.s_zones, mnx_sb.s_zones*mnx_sb.s_blocksize).AppendLine();
				sb.AppendFormat("{0} bytes/block", mnx_sb.s_blocksize).AppendLine();
				sb.AppendFormat("{0} inodes on volume", mnx_sb.s_ninodes).AppendLine();
				sb.AppendFormat("{0} blocks on inode map ({1} bytes)", mnx_sb.s_imap_blocks, mnx_sb.s_imap_blocks*mnx_sb.s_blocksize).AppendLine();
				sb.AppendFormat("{0} blocks on zone map ({1} bytes)", mnx_sb.s_zmap_blocks, mnx_sb.s_zmap_blocks*mnx_sb.s_blocksize).AppendLine();
				sb.AppendFormat("First data zone: {0}", mnx_sb.s_firstdatazone).AppendLine();
				//sb.AppendFormat("log2 of blocks/zone: {0}", mnx_sb.s_log_zone_size).AppendLine(); // Apparently 0
				sb.AppendFormat("{0} bytes maximum per file", mnx_sb.s_max_size).AppendLine();
				sb.AppendFormat("On-disk filesystem version: {0}", mnx_sb.s_disk_version).AppendLine();
			}
			else
			{
				MinixSuperBlock mnx_sb = new MinixSuperBlock();
				
				mnx_sb.s_ninodes = eabr.ReadUInt16();
				mnx_sb.s_nzones = eabr.ReadUInt16();
				mnx_sb.s_imap_blocks = eabr.ReadUInt16();
				mnx_sb.s_zmap_blocks = eabr.ReadUInt16();
				mnx_sb.s_firstdatazone = eabr.ReadUInt16();
				mnx_sb.s_log_zone_size = eabr.ReadUInt16();
				mnx_sb.s_max_size = eabr.ReadUInt32();
				mnx_sb.s_magic = eabr.ReadUInt16();
				mnx_sb.s_state = eabr.ReadUInt16();
				mnx_sb.s_zones = eabr.ReadUInt32();

				sb.AppendLine(minixVersion);
				sb.AppendFormat("{0} chars in filename", filenamesize).AppendLine();
				if(mnx_sb.s_zones > 0) // On V2
					sb.AppendFormat("{0} zones on volume ({1} bytes)", mnx_sb.s_zones, mnx_sb.s_zones*1024).AppendLine();
				else
					sb.AppendFormat("{0} zones on volume ({1} bytes)", mnx_sb.s_nzones, mnx_sb.s_nzones*1024).AppendLine();
				sb.AppendFormat("{0} inodes on volume", mnx_sb.s_ninodes).AppendLine();
				sb.AppendFormat("{0} blocks on inode map ({1} bytes)", mnx_sb.s_imap_blocks, mnx_sb.s_imap_blocks*1024).AppendLine();
				sb.AppendFormat("{0} blocks on zone map ({1} bytes)", mnx_sb.s_zmap_blocks, mnx_sb.s_zmap_blocks*1024).AppendLine();
				sb.AppendFormat("First data zone: {0}", mnx_sb.s_firstdatazone).AppendLine();
				//sb.AppendFormat("log2 of blocks/zone: {0}", mnx_sb.s_log_zone_size).AppendLine(); // Apparently 0
				sb.AppendFormat("{0} bytes maximum per file", mnx_sb.s_max_size).AppendLine();
				sb.AppendFormat("Filesystem state: {0:X4}", mnx_sb.s_state).AppendLine();
			}
			information = sb.ToString();
		}

		public struct MinixSuperBlock
		{
			public UInt16 s_ninodes;       // 0x00, inodes on volume
			public UInt16 s_nzones;        // 0x02, zones on volume
			public UInt16 s_imap_blocks;   // 0x04, blocks on inode map
			public UInt16 s_zmap_blocks;   // 0x06, blocks on zone map
			public UInt16 s_firstdatazone; // 0x08, first data zone
			public UInt16 s_log_zone_size; // 0x0A, log2 of blocks/zone
			public UInt32 s_max_size;      // 0x0C, max file size
			public UInt16 s_magic;         // 0x10, magic
			public UInt16 s_state;         // 0x12, filesystem state
			public UInt32 s_zones;         // 0x14, number of zones
		}

		public struct Minix3SuperBlock
		{
			public UInt32 s_ninodes;       // 0x00, inodes on volume
			public UInt16 s_pad0;          // 0x04, padding
			public UInt16 s_imap_blocks;   // 0x06, blocks on inode map
			public UInt16 s_zmap_blocks;   // 0x08, blocks on zone map
			public UInt16 s_firstdatazone; // 0x0A, first data zone
			public UInt16 s_log_zone_size; // 0x0C, log2 of blocks/zone
			public UInt16 s_pad1;          // 0x0E, padding
			public UInt32 s_max_size;      // 0x10, max file size
			public UInt32 s_zones;         // 0x14, number of zones
			public UInt16 s_magic;         // 0x18, magic
			public UInt16 s_pad2;          // 0x1A, padding
			public UInt16 s_blocksize;     // 0x1C, bytes in a block
			public byte   s_disk_version;  // 0x1E, on-disk structures version
		}
	}
}

