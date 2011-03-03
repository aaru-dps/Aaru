using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using FileSystemIDandChk;

namespace FileSystemIDandChk.PartPlugins
{
	class AppleMap : PartPlugin
	{
		public AppleMap (PluginBase Core)
		{
            base.Name = "Apple Partition Map";
            base.PluginUUID = new Guid("ffe4b4e9-82ed-4761-af49-8bade4081d10");
		}
		
		public override bool GetInformation (FileStream stream, out List<Partition> partitions)
		{
			byte[] sixteen_bits = new byte[2];
			byte[] thirtytwo_bits = new byte[4];
			byte[] sixteen_bytes = new byte[16];
			byte[] thirtytwo_bytes = new byte[32];
			
			ulong apm_entries;
			
			partitions = new List<Partition>();
			
			AppleMapBootEntry APMB = new AppleMapBootEntry();
			AppleMapPartitionEntry APMEntry = new AppleMapPartitionEntry();
			
			stream.Seek(0, SeekOrigin.Begin);
			stream.Read(sixteen_bits, 0, 2);
			sixteen_bits = Swapping.SwapTwoBytes(sixteen_bits);
			APMB.signature = BitConverter.ToUInt16(sixteen_bits, 0);
			
			if(APMB.signature != 0x4552)
				return false; // Not an Apple Partition Map
			
			stream.Read(sixteen_bits, 0, 2);
			sixteen_bits = Swapping.SwapTwoBytes(sixteen_bits);
			APMB.sector_size = BitConverter.ToUInt16(sixteen_bits, 0);
			
			stream.Seek(APMB.sector_size, SeekOrigin.Begin); // Seek to first entry
			stream.Read(sixteen_bits, 0, 2);
			sixteen_bits = Swapping.SwapTwoBytes(sixteen_bits);
			APMEntry.signature = BitConverter.ToUInt16(sixteen_bits, 0);
			if(APMEntry.signature != 0x504D && APMEntry.signature != 0x5453) // It should have partition entry signature
				return false;
			
			stream.Seek(2, SeekOrigin.Current); // Skip reserved1
			stream.Read(thirtytwo_bits, 0, 4);
			thirtytwo_bits = Swapping.SwapFourBytes(thirtytwo_bits);
			APMEntry.entries = BitConverter.ToUInt32(thirtytwo_bits, 0);
			if(APMEntry.entries <= 1) // It should have more than one entry
				return false;
			
			stream.Seek(4, SeekOrigin.Current); // Skip start, we don't need it
			stream.Seek(4, SeekOrigin.Current); // Skip sectors, we don't need it
			stream.Seek(32, SeekOrigin.Current); // Skip name, we don't ned it
			
			stream.Read(thirtytwo_bytes, 0, 32);
			APMEntry.type = StringHandlers.CToString(thirtytwo_bytes);
			if(APMEntry.type != "Apple_partition_map") // APM self-describes, if not, this is incorrect
				return false;
			
			apm_entries = APMEntry.entries;
			
			for(ulong i = 2; i <= apm_entries; i++) // For each partition
			{
				APMEntry = new AppleMapPartitionEntry();
				
				stream.Seek((long)(APMB.sector_size*i), SeekOrigin.Begin); // Seek to partition descriptor
				
				stream.Read(sixteen_bits, 0, 2);
				sixteen_bits = Swapping.SwapTwoBytes(sixteen_bits);
				APMEntry.signature = BitConverter.ToUInt16(sixteen_bits, 0);
				if(APMEntry.signature == 0x504D || APMEntry.signature == 0x5453) // It should have partition entry signature
				{
					Partition _partition = new Partition();
					StringBuilder sb = new StringBuilder();
					
					stream.Seek(2, SeekOrigin.Current); // Skip reserved1
					stream.Seek(4, SeekOrigin.Current); // Skip entries
					
					stream.Read(thirtytwo_bits, 0, 4);
					thirtytwo_bits = Swapping.SwapFourBytes(thirtytwo_bits);
					APMEntry.start = BitConverter.ToUInt32(thirtytwo_bits, 0);
					stream.Read(thirtytwo_bits, 0, 4);
					thirtytwo_bits = Swapping.SwapFourBytes(thirtytwo_bits);
					APMEntry.sectors = BitConverter.ToUInt32(thirtytwo_bits, 0);
					stream.Read(thirtytwo_bytes, 0, 32);
					APMEntry.name = StringHandlers.CToString(thirtytwo_bytes);
					stream.Read(thirtytwo_bytes, 0, 32);
					APMEntry.type = StringHandlers.CToString(thirtytwo_bytes);
					stream.Read(thirtytwo_bits, 0, 4);
					thirtytwo_bits = Swapping.SwapFourBytes(thirtytwo_bits);
					APMEntry.first_data_block = BitConverter.ToUInt32(thirtytwo_bits, 0);
					stream.Read(thirtytwo_bits, 0, 4);
					thirtytwo_bits = Swapping.SwapFourBytes(thirtytwo_bits);
					APMEntry.data_sectors = BitConverter.ToUInt32(thirtytwo_bits, 0);
					stream.Read(thirtytwo_bits, 0, 4);
					thirtytwo_bits = Swapping.SwapFourBytes(thirtytwo_bits);
					APMEntry.status = BitConverter.ToUInt32(thirtytwo_bits, 0);
					stream.Read(thirtytwo_bits, 0, 4);
					thirtytwo_bits = Swapping.SwapFourBytes(thirtytwo_bits);
					APMEntry.first_boot_block = BitConverter.ToUInt32(thirtytwo_bits, 0);
					stream.Read(thirtytwo_bits, 0, 4);
					thirtytwo_bits = Swapping.SwapFourBytes(thirtytwo_bits);
					APMEntry.boot_size = BitConverter.ToUInt32(thirtytwo_bits, 0);
					stream.Read(thirtytwo_bits, 0, 4);
					thirtytwo_bits = Swapping.SwapFourBytes(thirtytwo_bits);
					APMEntry.load_address = BitConverter.ToUInt32(thirtytwo_bits, 0);
					stream.Seek(4, SeekOrigin.Current);
					stream.Read(thirtytwo_bits, 0, 4);
					thirtytwo_bits = Swapping.SwapFourBytes(thirtytwo_bits);
					APMEntry.entry_point = BitConverter.ToUInt32(thirtytwo_bits, 0);
					stream.Seek(4, SeekOrigin.Current);
					stream.Read(thirtytwo_bits, 0, 4);
					thirtytwo_bits = Swapping.SwapFourBytes(thirtytwo_bits);
					APMEntry.checksum = BitConverter.ToUInt32(thirtytwo_bits, 0);
					stream.Read(sixteen_bytes, 0, 16);
					APMEntry.processor = StringHandlers.CToString(sixteen_bytes);
					
					_partition.PartitionSequence = i;
					_partition.PartitionType = APMEntry.type;
					_partition.PartitionName = APMEntry.name;
					_partition.PartitionStart = APMEntry.start * APMB.sector_size;
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