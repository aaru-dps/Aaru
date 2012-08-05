using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using FileSystemIDandChk;

namespace FileSystemIDandChk.PartPlugins
{
	class AppleMap : PartPlugin
	{
		private const UInt16 APM_MAGIC  = 0x4552; // "ER"
		private const UInt16 APM_ENTRY  = 0x504D; // "PM"
		private const UInt16 APM_OLDENT = 0x5453; // "TS", old entry magic

		public AppleMap (PluginBase Core)
		{
            base.Name = "Apple Partition Map";
			base.PluginUUID = new Guid("36405F8D-4F1A-07F5-209C-223D735D6D22");
		}
		
		public override bool GetInformation (FileStream stream, out List<Partition> partitions)
		{
			byte[] cString;
			
			ulong apm_entries;
			
			partitions = new List<Partition>();
			
			AppleMapBootEntry APMB = new AppleMapBootEntry();
			AppleMapPartitionEntry APMEntry = new AppleMapPartitionEntry();
			EndianAwareBinaryReader eabr = new EndianAwareBinaryReader(stream, false); // BigEndian

			eabr.BaseStream.Seek(0, SeekOrigin.Begin);
			APMB.signature = eabr.ReadUInt16();
			
			if(APMB.signature == APM_MAGIC)
			{
				APMB.sector_size = eabr.ReadUInt16();
			}
			else
				APMB.sector_size = 512; // Some disks omit the boot entry

			if(APMB.sector_size == 2048) // A CD, search if buggy (aligns in 512 bytes blocks) first
			{
				eabr.BaseStream.Seek(512, SeekOrigin.Begin); // Seek to first entry
				APMEntry.signature = eabr.ReadUInt16();
				if(APMEntry.signature != APM_ENTRY && APMEntry.signature != APM_OLDENT) // It should have partition entry signature if buggy
				{
					eabr.BaseStream.Seek(2048, SeekOrigin.Begin); // Seek to first entry considering 2048 bytes blocks. Unbuggy.
					APMEntry.signature = eabr.ReadUInt16();
					if(APMEntry.signature != APM_ENTRY && APMEntry.signature != APM_OLDENT)
						return false;
					else
						APMB.sector_size = 2048;
				}
				else
					APMB.sector_size = 512;
			}
			else
			{
				eabr.BaseStream.Seek(APMB.sector_size, SeekOrigin.Begin); // Seek to first entry
				APMEntry.signature = eabr.ReadUInt16();
				if(APMEntry.signature != APM_ENTRY && APMEntry.signature != APM_OLDENT) // It should have partition entry signature if buggy
				{
					eabr.BaseStream.Seek(512, SeekOrigin.Begin); // Seek to first entry considering 512 bytes blocks. Buggy.
					APMEntry.signature = eabr.ReadUInt16();
					if(APMEntry.signature != APM_ENTRY && APMEntry.signature != APM_OLDENT)
						return false;
					else
						APMB.sector_size = 512;
				}
			}

			eabr.BaseStream.Seek(2, SeekOrigin.Current); // Skip reserved1
			APMEntry.entries = eabr.ReadUInt32();
			if(APMEntry.entries <= 1) // It should have more than one entry
				return false;
			
//			eabr.BaseStream.Seek(4, SeekOrigin.Current); // Skip start, we don't need it
//			eabr.BaseStream.Seek(4, SeekOrigin.Current); // Skip sectors, we don't need it
//			eabr.BaseStream.Seek(32, SeekOrigin.Current); // Skip name, we don't ned it
			
//			cString = eabr.ReadBytes(32);
//			APMEntry.type = StringHandlers.CToString(cString);
//			if(APMEntry.type != "Apple_partition_map") // APM self-describes, if not, this is incorrect
//				return false;
			
			apm_entries = APMEntry.entries;
			
			for(ulong i = 1; i <= apm_entries; i++) // For each partition
			{
				APMEntry = new AppleMapPartitionEntry();
				
				eabr.BaseStream.Seek((long)(APMB.sector_size*i), SeekOrigin.Begin); // Seek to partition descriptor
				//eabr.BaseStream.Seek((long)(0x200*i), SeekOrigin.Begin); // Seek to partition descriptor
				
				APMEntry.signature = eabr.ReadUInt16();
				if(APMEntry.signature == APM_ENTRY || APMEntry.signature == APM_OLDENT) // It should have partition entry signature
				{
					Partition _partition = new Partition();
					StringBuilder sb = new StringBuilder();
					
					eabr.BaseStream.Seek(2, SeekOrigin.Current); // Skip reserved1
					eabr.BaseStream.Seek(4, SeekOrigin.Current); // Skip entries
					
					APMEntry.start = eabr.ReadUInt32();
					APMEntry.sectors = eabr.ReadUInt32();
					cString = eabr.ReadBytes(32);
					APMEntry.name = StringHandlers.CToString(cString);
					cString = eabr.ReadBytes(32);
					APMEntry.type = StringHandlers.CToString(cString);
					APMEntry.first_data_block = eabr.ReadUInt32();
					APMEntry.data_sectors = eabr.ReadUInt32();
					APMEntry.status = eabr.ReadUInt32();
					APMEntry.first_boot_block = eabr.ReadUInt32();
					APMEntry.boot_size = eabr.ReadUInt32();
					APMEntry.load_address = eabr.ReadUInt32();
					eabr.BaseStream.Seek(4, SeekOrigin.Current);
					APMEntry.entry_point = eabr.ReadUInt32();
					eabr.BaseStream.Seek(4, SeekOrigin.Current);
					APMEntry.checksum = eabr.ReadUInt32();
					cString = eabr.ReadBytes(16);
					APMEntry.processor = StringHandlers.CToString(cString);
					
					_partition.PartitionSequence = i;
					_partition.PartitionType = APMEntry.type;
					_partition.PartitionName = APMEntry.name;
//					_partition.PartitionStart = APMEntry.start * 0x200; // This seems to be hardcoded
					_partition.PartitionStart = APMEntry.start * APMB.sector_size;
//					_partition.PartitionLength = APMEntry.sectors * 0x200; // This seems to be hardcoded
					_partition.PartitionLength = APMEntry.sectors * APMB.sector_size;
					
					sb.AppendLine("Partition flags:");
					if((APMEntry.status & 0x01) == 0x01)
						sb.AppendLine("Partition is valid.");
					if((APMEntry.status & 0x02) == 0x02)
						sb.AppendLine("Partition entry is not available.");
					if((APMEntry.status & 0x04) == 0x04)
						sb.AppendLine("Partition is mounted.");
					if((APMEntry.status & 0x08) == 0x08)
						sb.AppendLine("Partition is bootable.");
					if((APMEntry.status & 0x10) == 0x10)
						sb.AppendLine("Partition is readable.");
					if((APMEntry.status & 0x20) == 0x20)
						sb.AppendLine("Partition is writable.");
					if((APMEntry.status & 0x40) == 0x40)
						sb.AppendLine("Partition's boot code is position independent.");
					
					if((APMEntry.status & 0x08) == 0x08)
					{
						sb.AppendFormat("First boot sector: {0}", APMEntry.first_boot_block).AppendLine();
						sb.AppendFormat("Boot is {0} bytes.", APMEntry.boot_size).AppendLine();
						sb.AppendFormat("Boot load address: 0x{0:X8}", APMEntry.load_address).AppendLine();
						sb.AppendFormat("Boot entry point: 0x{0:X8}", APMEntry.entry_point).AppendLine();
						sb.AppendFormat("Boot code checksum: 0x{0:X8}", APMEntry.checksum).AppendLine();
						sb.AppendFormat("Processor: {0}", APMEntry.processor).AppendLine();
					}
					
					_partition.PartitionDescription = sb.ToString();
					
					if((APMEntry.status & 0x01) == 0x01)
						if(APMEntry.type != "Apple_partition_map")
							partitions.Add(_partition);
				}
			}
			
			return true;
		}
		
		public struct AppleMapBootEntry
		{
			public UInt16 signature;        // Signature ("ER")
			public UInt16 sector_size;      // Byter per sector
			public UInt32 sectors;          // Sectors of the disk
			public UInt16 reserved1;        // Reserved
			public UInt16 reserved2;        // Reserved
			public UInt32 reserved3;        // Reserved
			public UInt16 driver_entries;   // Number of entries of the driver descriptor
			public UInt32 first_driver_blk; // First sector of the driver
			public UInt16 driver_size;      // Size in 512bytes sectors of the driver
			public UInt16 operating_system; // Operating system (MacOS = 1)	
		}
		
		public struct AppleMapPartitionEntry
		{
			public UInt16 signature;        // Signature ("PM" or "TS")
			public UInt16 reserved1;        // Reserved
			public UInt32 entries;          // Number of entries on the partition map, each one sector
			public UInt32 start;            // First sector of the partition
			public UInt32 sectors;          // Number of sectos of the partition
			public string name;             // Partition name, 32 bytes, null-padded
			public string type;             // Partition type. 32 bytes, null-padded
			public UInt32 first_data_block; // First sector of the data area
			public UInt32 data_sectors;     // Number of sectors of the data area
			public UInt32 status;           // Partition status
			public UInt32 first_boot_block; // First sector of the boot code
			public UInt32 boot_size;        // Size in bytes of the boot code
			public UInt32 load_address;     // Load address of the boot code
			public UInt32 reserved2;        // Reserved
			public UInt32 entry_point;      // Entry point of the boot code
			public UInt32 reserved3;        // Reserved
			public UInt32 checksum;         // Boot code checksum
			public string processor;        // Processor type, 16 bytes, null-padded
		}
	}
}