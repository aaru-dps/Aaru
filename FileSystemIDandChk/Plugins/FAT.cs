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
			byte[] fat32_signature = new byte[8]; // "FAT32   "
			byte[] first_fat_entry_b = new byte[4]; // No matter FAT size we read 2 bytes for checking
			ulong first_fat_entry;
			
			stream.Seek(0x15 + offset, SeekOrigin.Begin); // Media Descriptor if present is in 0x15
			media_descriptor = (byte)stream.ReadByte();
			stream.Seek(0x52 + offset, SeekOrigin.Begin); // FAT32 signature, if present, is in 0x52
			stream.Read(fat32_signature, 0, 8);
			stream.Seek(0x200 + offset, SeekOrigin.Begin); // First FAT entry is always at 0x200 in pre-FAT32
			stream.Read(first_fat_entry_b, 0, 4);
			
			first_fat_entry = BitConverter.ToUInt32(first_fat_entry_b, 0); // Easier to manage
			
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
			
			byte media_descriptor; // Not present on DOS <= 3, present on TOS but != of first FAT entry
			byte[] fat32_signature = new byte[8]; // "FAT32   "
			byte[] first_fat_entry_b = new byte[4]; // No matter FAT size we read 2 bytes for checking
			ulong first_fat_entry;
			
			stream.Seek(0x15 + offset, SeekOrigin.Begin); // Media Descriptor if present is in 0x15
			media_descriptor =(byte) stream.ReadByte();
			stream.Seek(0x52 + offset, SeekOrigin.Begin); // FAT32 signature, if present, is in 0x52
			stream.Read(fat32_signature, 0, 8);
			stream.Seek(0x200 + offset, SeekOrigin.Begin); // First FAT entry is always at 0x200 in pre-FAT32
			stream.Read(first_fat_entry_b, 0, 4);
			
			first_fat_entry = BitConverter.ToUInt32(first_fat_entry_b, 0); // Easier to manage
			
			// Let's start the fun
			if(Encoding.ASCII.GetString(fat32_signature) == "FAT32   ")
				sb.AppendLine("Microsoft FAT32"); // Seems easy, check reading
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
			
			byte[] eight_bytes = new byte[8];
			byte[] eleven_bytes = new byte[11];
			byte[] sixteen_bits = new byte[2];
			byte[] thirtytwo_bits = new byte[4];
			
			stream.Seek(3 + offset, SeekOrigin.Begin);
			stream.Read(eight_bytes, 0, 8);
			BPB.OEMName = Encoding.ASCII.GetString(eight_bytes);
			stream.Read(sixteen_bits, 0, 2);
			BPB.bps = BitConverter.ToUInt16(sixteen_bits, 0);
			BPB.spc = (byte)stream.ReadByte();
			stream.Read(sixteen_bits, 0, 2);
			BPB.rsectors = BitConverter.ToUInt16(sixteen_bits, 0);
			BPB.fats_no = (byte)stream.ReadByte();
			stream.Read(sixteen_bits, 0, 2);
			BPB.root_ent = BitConverter.ToUInt16(sixteen_bits, 0);
			stream.Read(sixteen_bits, 0, 2);
			BPB.sectors = BitConverter.ToUInt16(sixteen_bits, 0);
			BPB.media = (byte)stream.ReadByte();
			stream.Read(sixteen_bits, 0, 2);
			BPB.spfat = BitConverter.ToUInt16(sixteen_bits, 0);
			stream.Read(sixteen_bits, 0, 2);
			BPB.sptrk = BitConverter.ToUInt16(sixteen_bits, 0);
			stream.Read(sixteen_bits, 0, 2);
			BPB.heads = BitConverter.ToUInt16(sixteen_bits, 0);
			stream.Read(thirtytwo_bits, 0, 4);
			BPB.hsectors = BitConverter.ToUInt32(thirtytwo_bits, 0);
			stream.Read(thirtytwo_bits, 0, 4);
			BPB.big_sectors = BitConverter.ToUInt32(thirtytwo_bits, 0);
			
			if(Encoding.ASCII.GetString(fat32_signature) == "FAT32   ")
			{
				stream.Read(thirtytwo_bits, 0, 4);
				FAT32PB.spfat = BitConverter.ToUInt32(thirtytwo_bits, 0);
				stream.Read(sixteen_bits, 0, 2);
				FAT32PB.fat_flags = BitConverter.ToUInt16(sixteen_bits, 0);
				stream.Read(sixteen_bits, 0, 2);
				FAT32PB.version = BitConverter.ToUInt16(sixteen_bits, 0);
				stream.Read(thirtytwo_bits, 0, 4);
				FAT32PB.root_cluster = BitConverter.ToUInt32(thirtytwo_bits, 0);
				stream.Read(sixteen_bits, 0, 2);
				FAT32PB.fsinfo_sector = BitConverter.ToUInt16(sixteen_bits, 0);
				stream.Read(sixteen_bits, 0, 2);
				FAT32PB.backup_sector = BitConverter.ToUInt16(sixteen_bits, 0);
				FAT32PB.drive_no = (byte)stream.ReadByte();
				FAT32PB.nt_flags = (byte)stream.ReadByte();
				FAT32PB.signature = (byte)stream.ReadByte();
				stream.Read(thirtytwo_bits, 0, 4);
				FAT32PB.serial_no = BitConverter.ToUInt32(thirtytwo_bits, 0);
				stream.Read(eleven_bytes, 0, 11);
				FAT32PB.volume_label = Encoding.ASCII.GetString(eleven_bytes);
				stream.Read(eight_bytes, 0, 8);
				FAT32PB.fs_type = Encoding.ASCII.GetString(eight_bytes);
			}
			else
			{
				EPB.drive_no = (byte)stream.ReadByte();
				EPB.nt_flags = (byte)stream.ReadByte();
				EPB.signature = (byte)stream.ReadByte();
				stream.Read(thirtytwo_bits, 0, 4);
				EPB.serial_no = BitConverter.ToUInt32(thirtytwo_bits, 0);
				stream.Read(eleven_bytes, 0, 11);
				EPB.volume_label = Encoding.ASCII.GetString(eleven_bytes);
				stream.Read(eight_bytes, 0, 8);
				EPB.fs_type = Encoding.ASCII.GetString(eight_bytes);
			}
			
			sb.AppendFormat("OEM Name: {0}", BPB.OEMName).AppendLine();
			sb.AppendFormat("{0} bytes per sector.", BPB.bps).AppendLine();
			sb.AppendFormat("{0} sectors per cluster.", BPB.spc).AppendLine();
			sb.AppendFormat("{0} sectors reserved between BPB and FAT.", BPB.rsectors).AppendLine();
			sb.AppendFormat("{0} FATs.", BPB.fats_no).AppendLine();
			sb.AppendFormat("{0} entires on root directory.", BPB.root_ent).AppendLine();
			if(BPB.sectors==0)
				sb.AppendFormat("{0} sectors on volume.", BPB.big_sectors).AppendLine();
			else
				sb.AppendFormat("{0} sectors on volume.", BPB.sectors).AppendLine();
			if((BPB.media & 0xF0) == 0xF0)
				sb.AppendFormat("Media format: 0x{0:X2}", BPB.media).AppendLine();
			if(Encoding.ASCII.GetString(fat32_signature) == "FAT32   ")
				sb.AppendFormat("{0} sectors per FAT.", FAT32PB.spfat).AppendLine();
			else
				sb.AppendFormat("{0} sectors per FAT.", BPB.spfat).AppendLine();
			sb.AppendFormat("{0} sectors per track.", BPB.sptrk).AppendLine();
			sb.AppendFormat("{0} heads.", BPB.heads).AppendLine();
			sb.AppendFormat("{0} hidden sectors before BPB.", BPB.hsectors).AppendLine();
			
			if(Encoding.ASCII.GetString(fat32_signature) == "FAT32   ")
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
			public string OEMName;     // OEM Name, 8 bytes, space-padded
			public UInt16 bps;         // Bytes per sector
			public byte   spc;         // Sectors per cluster
			public UInt16 rsectors;    // Reserved sectors between BPB and FAT
			public byte   fats_no;     // Number of FATs
			public UInt16 root_ent;    // Number of entries on root directory
			public UInt16 sectors;     // Sectors in volume
			public byte   media;       // Media descriptor
			public UInt16 spfat;       // Sectors per FAT
			public UInt16 sptrk;       // Sectors per track
			public UInt16 heads;       // Heads
			public UInt32 hsectors;    // Hidden sectors before BPB
			public UInt32 big_sectors; // Sectors in volume if > 65535
		}
		
		public struct ExtendedParameterBlock
		{
			public byte   drive_no;     // Drive number
			public byte   nt_flags;     // Volume flags if NT (must be 0x29 signature)
			public byte   signature;    // EPB signature, 0x28 or 0x29
			public UInt32 serial_no;    // Volume serial number
			/* Present only if signature == 0x29 */
			public string volume_label; // Volume label, 11 bytes, space-padded
			public string fs_type;      // Filesystem type, 8 bytes, space-padded
		}
		
		public struct FAT32ParameterBlock
		{
			public UInt32  spfat;           // Sectors per FAT
			public UInt16 fat_flags;     // FAT flags
			public UInt16 version;       // FAT32 version
			public UInt32  root_cluster;  // Cluster of root directory
			public UInt16 fsinfo_sector; // Sector of FSINFO structure
			public UInt16 backup_sector; // Secfor of FAT32PB bacup
			       byte[] reserved;      // 12 reserved bytes
			public byte   drive_no;      // Drive number
			public byte   nt_flags;      // Volume flags
			public byte   signature;     // FAT32PB signature, should be 0x29
			public UInt32  serial_no;     // Volume serial number
			public string volume_label;  // Volume label, 11 bytes, space-padded
			public string fs_type;       // Filesystem type, 8 bytes, space-padded, must be "FAT32   "
		}
	}
}

