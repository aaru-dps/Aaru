using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using FileSystemIDandChk;

namespace FileSystemIDandChk.PartPlugins
{
	class NeXTDisklabel : PartPlugin
	{
		public NeXTDisklabel (PluginBase Core)
		{
            base.Name = "NeXT Disklabel";
            base.PluginUUID = new Guid("460840f4-23f7-1fbe-6f28-50815e871853");
		}
		
		public override bool GetInformation (FileStream stream, out List<Partition> partitions)
		{
			byte[] sixteen_bits = new byte[2];
			byte[] thirtytwo_bits = new byte[4];
			byte[] eight_bytes = new byte[8];
			byte[] sixteen_bytes = new byte[16];
			bool magic_found = false;
			
			UInt32 magic;
			UInt32 sector_size;
			UInt16 front_porch;
			
			partitions = new List<Partition>();
			
			stream.Seek(0, SeekOrigin.Begin); // Starts on sector 0 on NeXT machines, CDs and floppies
			stream.Read(thirtytwo_bits, 0, 4);
			thirtytwo_bits = Swapping.SwapFourBytes(thirtytwo_bits);
			magic = BitConverter.ToUInt32(thirtytwo_bits, 0);
			
			if(magic == 0x4E655854 || magic == 0x646C5632 || magic == 0x646C5633)
				magic_found = true;
			else
			{
				stream.Seek(0x1E00, SeekOrigin.Begin); // Starts on sector 15 on MBR machines
				stream.Read(thirtytwo_bits, 0, 4);
				thirtytwo_bits = Swapping.SwapFourBytes(thirtytwo_bits);
				magic = BitConverter.ToUInt32(thirtytwo_bits, 0);
				
				if(magic == 0x4E655854 || magic == 0x646C5632 || magic == 0x646C5633)
					magic_found = true;
				else
				{
					stream.Seek(0x2000, SeekOrigin.Begin); // Starts on sector 16 (4 on CD) on RISC disks
					stream.Read(thirtytwo_bits, 0, 4);
					thirtytwo_bits = Swapping.SwapFourBytes(thirtytwo_bits);
					magic = BitConverter.ToUInt32(thirtytwo_bits, 0);
					
					if(magic == 0x4E655854 || magic == 0x646C5632 || magic == 0x646C5633)
						magic_found = true;
					else
						return false;
				}
			}
			
			if(magic_found)
			{
				stream.Seek(88, SeekOrigin.Current); // Seek to sector size
				stream.Read(thirtytwo_bits, 0, 4);
				thirtytwo_bits = Swapping.SwapFourBytes(thirtytwo_bits);
				sector_size = BitConverter.ToUInt32(thirtytwo_bits, 0);
				stream.Seek(16, SeekOrigin.Current); // Seek to front porch
				stream.Read(sixteen_bits, 0, 2);
				sixteen_bits = Swapping.SwapTwoBytes(sixteen_bits);
				front_porch = BitConverter.ToUInt16(sixteen_bits, 0);
				
				stream.Seek(76, SeekOrigin.Current); // Seek to first partition entry
				
				NeXTEntry entry = new NeXTEntry();
				
				for(int i = 0; i < 8; i ++)
				{
					stream.Read(thirtytwo_bits, 0, 4);
					thirtytwo_bits = Swapping.SwapFourBytes(thirtytwo_bits);
					entry.start = BitConverter.ToUInt32(thirtytwo_bits, 0);
					stream.Read(thirtytwo_bits, 0, 4);
					thirtytwo_bits = Swapping.SwapFourBytes(thirtytwo_bits);
					entry.sectors = BitConverter.ToUInt32(thirtytwo_bits, 0);
					stream.Read(sixteen_bits, 0, 2);
					sixteen_bits = Swapping.SwapTwoBytes(sixteen_bits);
					entry.block_size = BitConverter.ToUInt16(sixteen_bits, 0);
					stream.Read(sixteen_bits, 0, 2);
					sixteen_bits = Swapping.SwapTwoBytes(sixteen_bits);
					entry.frag_size = BitConverter.ToUInt16(sixteen_bits, 0);
					entry.optimization = (byte)stream.ReadByte();
					stream.Read(sixteen_bits, 0, 2);
					sixteen_bits = Swapping.SwapTwoBytes(sixteen_bits);
					entry.cpg = BitConverter.ToUInt16(sixteen_bits, 0);
					stream.Read(sixteen_bits, 0, 2);
					sixteen_bits = Swapping.SwapTwoBytes(sixteen_bits);
					entry.bpi = BitConverter.ToUInt16(sixteen_bits, 0);
					entry.freemin = (byte)stream.ReadByte();
					entry.unknown = (byte)stream.ReadByte();
					entry.newfs = (byte)stream.ReadByte();
					stream.Read(sixteen_bytes, 0, 16);
					entry.mount_point = StringHandlers.CToString(sixteen_bytes);
					entry.automount = (byte)stream.ReadByte();
					stream.Read(eight_bytes, 0, 8);
					entry.type = StringHandlers.CToString(eight_bytes);
					entry.unknown2 = (byte)stream.ReadByte();
					
					if(entry.sectors > 0 && entry.sectors < 0xFFFFFFFF && entry.start < 0xFFFFFFFF)
					{
						Partition part = new Partition();
						StringBuilder sb = new StringBuilder();
						
						part.PartitionLength = (long)entry.sectors * sector_size;
						part.PartitionStart = ((long)entry.start + front_porch) * sector_size;
						part.PartitionType = entry.type;
						part.PartitionSequence = (ulong)i;
						part.PartitionName = entry.mount_point;
						
						sb.AppendFormat("{0} bytes per block", entry.block_size).AppendLine();
						sb.AppendFormat("{0} bytes per fragment", entry.frag_size).AppendLine();
						if(entry.optimization == 's')
							sb.AppendLine("Space optimized");
						else if(entry.optimization == 't')
							sb.AppendLine("Time optimized");
						else
							sb.AppendFormat("Unknown optimization {0:X2}", entry.optimization).AppendLine();
						sb.AppendFormat("{0} cylinders per group", entry.cpg).AppendLine();
						sb.AppendFormat("{0} bytes per inode", entry.bpi).AppendLine();
						sb.AppendFormat("{0}% of space must be free at minimum", entry.freemin).AppendLine();
						if(entry.newfs != 1) // Seems to indicate news has been already run
							sb.AppendLine("Filesystem should be formatted at start");
						if(entry.automount == 1)
							sb.AppendLine("Filesystem should be automatically mounted");
						
						part.PartitionDescription = sb.ToString();
						
						partitions.Add(part);
					}
				}
				
				return true;
			}
			else
				return false;
		}
		
		private struct NeXTEntry
		{
			public UInt32 start;        // Sector of start, counting from front porch
			public UInt32 sectors;      // Length in sectors
			public UInt16 block_size;   // Filesystem's block size
			public UInt16 frag_size;    // Filesystem's fragment size
			public byte   optimization; // 's'pace or 't'ime
			public UInt16 cpg;          // Cylinders per group
			public UInt16 bpi;          // Bytes per inode
			public byte freemin;        // % of minimum free space
			public byte unknown;        // Unknown
			public byte newfs;          // Should newfs be run on first start?	
			public string mount_point;  // Mount point or empty if mount where you want
			public byte automount;      // Should automount
			public string type;         // Filesystem type, always "4.3BSD"?
			public byte unknown2;       // Unknown
		}
	}
}