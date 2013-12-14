using System;
using System.IO;
using System.Text;
using FileSystemIDandChk;

// Information from Inside Macintosh

namespace FileSystemIDandChk.Plugins
{
	class FAT : Plugin
	{
		public FAT(PluginBase Core)
        {
            base.Name = "Microsoft File Allocation Table";
			base.PluginUUID = new Guid("33513B2C-0D26-0D2D-32C3-79D8611158E0");
        }
		
		public override bool Identify(FileStream stream, long offset)
		{
			byte media_descriptor; // Not present on DOS <= 3, present on TOS but != of first FAT entry
			byte fats_no; // Must be 1 or 2. Dunno if it can be 0 in the wild, but it CANNOT BE bigger than 2
			byte[] fat32_signature = new byte[8]; // "FAT32   "
			UInt32 first_fat_entry; // No matter FAT size we read 4 bytes for checking
			UInt16 bps, rsectors;

			BinaryReader br = new BinaryReader(stream);

			br.BaseStream.Seek(0x10 + offset, SeekOrigin.Begin); // FATs, 1 or 2, maybe 0, never bigger
			fats_no = br.ReadByte();
			br.BaseStream.Seek(0x15 + offset, SeekOrigin.Begin); // Media Descriptor if present is in 0x15
			media_descriptor = br.ReadByte();
			br.BaseStream.Seek(0x52 + offset, SeekOrigin.Begin); // FAT32 signature, if present, is in 0x52
			fat32_signature = br.ReadBytes(8);
			br.BaseStream.Seek(0x0B + offset, SeekOrigin.Begin); // Bytes per sector
			bps = br.ReadUInt16();
			if(bps==0)
				bps=0x200;
			br.BaseStream.Seek(0x0E + offset, SeekOrigin.Begin); // Sectors between BPB and FAT, including the BPB sector => [BPB,FAT) 
			rsectors = br.ReadUInt16();
			if(rsectors==0)
				rsectors=1;
			if((ulong)br.BaseStream.Length > (ulong)(bps*rsectors + offset))
				br.BaseStream.Seek(bps*rsectors + offset, SeekOrigin.Begin); // First FAT entry
			else
				return false;
			first_fat_entry = br.ReadUInt32(); // Easier to manage

			if(MainClass.isDebug)
			{
				Console.WriteLine("FAT: fats_no = {0}", fats_no);
				Console.WriteLine("FAT: media_descriptor = 0x{0:X2}", media_descriptor);
				Console.WriteLine("FAT: fat32_signature = {0}", StringHandlers.CToString(fat32_signature));
				Console.WriteLine("FAT: bps = {0}", bps);
				Console.WriteLine("FAT: first_fat_entry = 0x{0:X8}", first_fat_entry);
			}

			if(fats_no > 2) // Must be 1 or 2, but as TOS makes strange things and I have not checked if it puts this to 0, ignore if 0. MUST NOT BE BIGGER THAN 2!
				return false;

			// Let's start the fun
			if(Encoding.ASCII.GetString(fat32_signature) == "FAT32   ")
				return true; // Seems easy, check reading
			
			if((first_fat_entry & 0xFFFFFFF0) == 0xFFFFFFF0) // Seems to be FAT16
			{
				if((first_fat_entry & 0xFF) == media_descriptor)
					return true; // It MUST be FAT16, or... maybe not :S
			}
			else if((first_fat_entry & 0x00FFFFF0) == 0x00FFFFF0)
			{
				//if((first_fat_entry & 0xFF) == media_descriptor) // Pre DOS<4 does not implement this, TOS does and is !=
					return true; // It MUST be FAT12, or... maybe not :S
			}

				return false;
		}
		
		public override void GetInformation (FileStream stream, long offset, out string information)
		{
			information = "";
			
			StringBuilder sb = new StringBuilder();
			BinaryReader br = new BinaryReader(stream);

			byte[] dosString; // Space-padded
			bool isFAT32 = false;
			UInt32 first_fat_entry;
			byte media_descriptor, fats_no;
			string fat32_signature;
			UInt16 bps, rsectors;

			br.BaseStream.Seek(0x10 + offset, SeekOrigin.Begin); // Media Descriptor if present is in 0x15
			fats_no = br.ReadByte();
			br.BaseStream.Seek(0x15 + offset, SeekOrigin.Begin); // Media Descriptor if present is in 0x15
			media_descriptor =(byte) stream.ReadByte();
			br.BaseStream.Seek(0x52 + offset, SeekOrigin.Begin); // FAT32 signature, if present, is in 0x52
			dosString = br.ReadBytes(8);
			fat32_signature = Encoding.ASCII.GetString(dosString);
			br.BaseStream.Seek(0x0B + offset, SeekOrigin.Begin); // Bytes per sector
			bps = br.ReadUInt16();
			if(bps==0)
				bps=0x200;
			br.BaseStream.Seek(0x0E + offset, SeekOrigin.Begin); // Sectors between BPB and FAT, including the BPB sector => [BPB,FAT) 
			rsectors = br.ReadUInt16();
			if(rsectors==0)
				rsectors=1;
			br.BaseStream.Seek(bps*rsectors + offset, SeekOrigin.Begin); // First FAT entry
			first_fat_entry = br.ReadUInt32(); // Easier to manage

			if(fats_no > 2) // Must be 1 or 2, but as TOS makes strange things and I have not checked if it puts this to 0, ignore if 0. MUST NOT BE BIGGER THAN 2!
				return;

			// Let's start the fun
			if(fat32_signature == "FAT32   ")
			{
				sb.AppendLine("Microsoft FAT32"); // Seems easy, check reading
				isFAT32 = true;
			}
			else if((first_fat_entry & 0xFFFFFFF0) == 0xFFFFFFF0) // Seems to be FAT16
			{
				if((first_fat_entry & 0xFF) == media_descriptor)
					sb.AppendLine("Microsoft FAT16"); // It MUST be FAT16, or... maybe not :S
			}
			else if((first_fat_entry & 0x00FFFFF0) == 0x00FFFFF0)
			{
				//if((first_fat_entry & 0xFF) == media_descriptor) // Pre DOS<4 does not implement this, TOS does and is !=
					sb.AppendLine("Microsoft FAT12"); // It MUST be FAT12, or... maybe not :S
			}
			else
				return;
			
			BIOSParameterBlock BPB = new BIOSParameterBlock();
			ExtendedParameterBlock EPB = new ExtendedParameterBlock();
			FAT32ParameterBlock FAT32PB = new FAT32ParameterBlock();
			

			br.BaseStream.Seek(3 + offset, SeekOrigin.Begin);
			dosString = br.ReadBytes(8);
			BPB.OEMName = Encoding.ASCII.GetString(dosString);
			BPB.bps = br.ReadUInt16();
			BPB.spc = br.ReadByte();
			BPB.rsectors = br.ReadUInt16();
			BPB.fats_no = br.ReadByte();
			BPB.root_ent = br.ReadUInt16();
			BPB.sectors = br.ReadUInt16();
			BPB.media = br.ReadByte();
			BPB.spfat = br.ReadUInt16();
			BPB.sptrk = br.ReadUInt16();
			BPB.heads = br.ReadUInt16();
			BPB.hsectors = br.ReadUInt32();
			BPB.big_sectors = br.ReadUInt32();
			
			if(isFAT32)
			{
				FAT32PB.spfat = br.ReadUInt32();
				FAT32PB.fat_flags = br.ReadUInt16();
				FAT32PB.version = br.ReadUInt16();
				FAT32PB.root_cluster = br.ReadUInt32();
				FAT32PB.fsinfo_sector = br.ReadUInt16();
				FAT32PB.backup_sector = br.ReadUInt16();
				FAT32PB.drive_no = br.ReadByte();
				FAT32PB.nt_flags = br.ReadByte();
				FAT32PB.signature = br.ReadByte();
				FAT32PB.serial_no = br.ReadUInt32();
				dosString = br.ReadBytes(11);
				FAT32PB.volume_label = Encoding.ASCII.GetString(dosString);
				dosString = br.ReadBytes(8);
				FAT32PB.fs_type = Encoding.ASCII.GetString(dosString);
			}
			else
			{
				EPB.drive_no = br.ReadByte();
				EPB.nt_flags = br.ReadByte();
				EPB.signature = br.ReadByte();
				EPB.serial_no = br.ReadUInt32();
				dosString = br.ReadBytes(11);
				EPB.volume_label = Encoding.ASCII.GetString(dosString);
				dosString = br.ReadBytes(8);
				EPB.fs_type = Encoding.ASCII.GetString(dosString);
			}
			
			sb.AppendFormat("OEM Name: {0}", BPB.OEMName).AppendLine();
			sb.AppendFormat("{0} bytes per sector.", BPB.bps).AppendLine();
			sb.AppendFormat("{0} sectors per cluster.", BPB.spc).AppendLine();
			sb.AppendFormat("{0} sectors reserved between BPB and FAT.", BPB.rsectors).AppendLine();
			sb.AppendFormat("{0} FATs.", BPB.fats_no).AppendLine();
			sb.AppendFormat("{0} entires on root directory.", BPB.root_ent).AppendLine();
			if(BPB.sectors==0)
				sb.AppendFormat("{0} sectors on volume ({1} bytes).", BPB.big_sectors, BPB.big_sectors*BPB.bps).AppendLine();
			else
				sb.AppendFormat("{0} sectors on volume ({1} bytes).", BPB.sectors, BPB.sectors*BPB.bps).AppendLine();
			if((BPB.media & 0xF0) == 0xF0)
				sb.AppendFormat("Media format: 0x{0:X2}", BPB.media).AppendLine();
			if(fat32_signature == "FAT32   ")
				sb.AppendFormat("{0} sectors per FAT.", FAT32PB.spfat).AppendLine();
			else
				sb.AppendFormat("{0} sectors per FAT.", BPB.spfat).AppendLine();
			sb.AppendFormat("{0} sectors per track.", BPB.sptrk).AppendLine();
			sb.AppendFormat("{0} heads.", BPB.heads).AppendLine();
			sb.AppendFormat("{0} hidden sectors before BPB.", BPB.hsectors).AppendLine();
			
			if(isFAT32)
			{
				sb.AppendFormat("Cluster of root directory: {0}", FAT32PB.root_cluster).AppendLine();
				sb.AppendFormat("Sector of FSINFO structure: {0}", FAT32PB.fsinfo_sector).AppendLine();
				sb.AppendFormat("Sector of backup FAT32 parameter block: {0}", FAT32PB.backup_sector).AppendLine();
				sb.AppendFormat("Drive number: 0x{0:X2}", FAT32PB.drive_no).AppendLine();
				sb.AppendFormat("Volume Serial Number: 0x{0:X8}", FAT32PB.serial_no).AppendLine();
				if((FAT32PB.nt_flags & 0x01) == 0x01)
				{
					sb.AppendLine("Volume should be checked on next mount.");	
					if((EPB.nt_flags & 0x02) == 0x02)
						sb.AppendLine("Disk surface should be checked also.");	
				}
					
				sb.AppendFormat("Volume label: {0}", EPB.volume_label).AppendLine();
				sb.AppendFormat("Filesystem type: {0}", EPB.fs_type).AppendLine();
			}
			else if(EPB.signature == 0x28 || EPB.signature == 0x29)
			{
				sb.AppendFormat("Drive number: 0x{0:X2}", EPB.drive_no).AppendLine();
				sb.AppendFormat("Volume Serial Number: 0x{0:X8}", EPB.serial_no).AppendLine();
				if(EPB.signature==0x29)
				{
					if((EPB.nt_flags & 0x01) == 0x01)
					{
						sb.AppendLine("Volume should be checked on next mount.");	
						if((EPB.nt_flags & 0x02) == 0x02)
						sb.AppendLine("Disk surface should be checked also.");	
					}
					
					sb.AppendFormat("Volume label: {0}", EPB.volume_label).AppendLine();
					sb.AppendFormat("Filesystem type: {0}", EPB.fs_type).AppendLine();
				}
			}
			
			information = sb.ToString();
		}
		
		public struct BIOSParameterBlock
		{
			public string OEMName;     // 0x03, OEM Name, 8 bytes, space-padded
			public UInt16 bps;         // 0x0B, Bytes per sector
			public byte   spc;         // 0x0D, Sectors per cluster
			public UInt16 rsectors;    // 0x0E, Reserved sectors between BPB and FAT
			public byte   fats_no;     // 0x10, Number of FATs
			public UInt16 root_ent;    // 0x11, Number of entries on root directory
			public UInt16 sectors;     // 0x13, Sectors in volume
			public byte   media;       // 0x15, Media descriptor
			public UInt16 spfat;       // 0x16, Sectors per FAT
			public UInt16 sptrk;       // 0x18, Sectors per track
			public UInt16 heads;       // 0x1A, Heads
			public UInt32 hsectors;    // 0x1C, Hidden sectors before BPB
			public UInt32 big_sectors; // 0x20, Sectors in volume if > 65535
		}

		// This only applies for bootable disks
		// From http://info-coach.fr/atari/software/FD-Soft.php
		public struct AtariBootBlock
		{
			public UInt16 hsectors;    // 0x01C, Atari ST use 16 bit for hidden sectors, probably so did old DOS
			public UInt16 xflag;       // 0x01E, indicates if COMMAND.PRG must be executed after OS load
			public UInt16 ldmode;      // 0x020, load mode for, or 0 if fname indicates boot file
			public UInt16 bsect;       // 0x022, sector from which to boot
			public UInt16 bsects_no;   // 0x024, how many sectors to boot
			public UInt32 ldaddr;      // 0x026, RAM address where boot should be located
			public UInt32 fatbuf;      // 0x02A, RAM address to copy the FAT and root directory
			public string fname;       // 0x02E, 11 bytes, name of boot file
			public UInt16 reserved;    // 0x039, unused
			public byte[] boot_code;   // 0x03B, 451 bytes boot code
			public UInt16 checksum;    // 0x1FE, the sum of all the BPB+ABB must be 0x1234, so this bigendian value works as adjustment
		}
		
		public struct ExtendedParameterBlock
		{
			public byte   drive_no;     // 0x24, Drive number
			public byte   nt_flags;     // 0x25, Volume flags if NT (must be 0x29 signature)
			public byte   signature;    // 0x26, EPB signature, 0x28 or 0x29
			public UInt32 serial_no;    // 0x27, Volume serial number
			/* Present only if signature == 0x29 */
			public string volume_label; // 0x2B, Volume label, 11 bytes, space-padded
			public string fs_type;      // 0x36, Filesystem type, 8 bytes, space-padded
		}
		
		public struct FAT32ParameterBlock
		{
			public UInt32 spfat;         // 0x24, Sectors per FAT
			public UInt16 fat_flags;     // 0x28, FAT flags
			public UInt16 version;       // 0x2A, FAT32 version
			public UInt32 root_cluster;  // 0x2C, Cluster of root directory
			public UInt16 fsinfo_sector; // 0x30, Sector of FSINFO structure
			public UInt16 backup_sector; // 0x32, Sector of FAT32PB bacup
			       byte[] reserved;      // 0x34, 12 reserved bytes
			public byte   drive_no;      // 0x40, Drive number
			public byte   nt_flags;      // 0x41, Volume flags
			public byte   signature;     // 0x42, FAT32PB signature, should be 0x29
			public UInt32 serial_no;     // 0x43, Volume serial number
			public string volume_label;  // 0x47, Volume label, 11 bytes, space-padded
			public string fs_type;       // 0x52, Filesystem type, 8 bytes, space-padded, must be "FAT32   "
		}
	}
}

