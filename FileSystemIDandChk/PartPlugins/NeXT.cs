using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using FileSystemIDandChk;

namespace FileSystemIDandChk.PartPlugins
{
	class NeXTDisklabel : PartPlugin
	{
		public const UInt32 NEXT_MAGIC1 = 0x4E655854; // "NeXT"
		public const UInt32 NEXT_MAGIC2 = 0x646C5632; // "dlV2"
		public const UInt32 NEXT_MAGIC3 = 0x646C5633; // "dlV3"

		public NeXTDisklabel (PluginBase Core)
		{
            base.Name = "NeXT Disklabel";
			base.PluginUUID = new Guid("246A6D93-4F1A-1F8A-344D-50187A5513A9");
		}
		
		public override bool GetInformation (FileStream stream, out List<Partition> partitions)
		{
			byte[] cString;
			bool magic_found = false;
			
			UInt32 magic;
			UInt32 sector_size;
			UInt16 front_porch;
			
			partitions = new List<Partition>();

			EndianAwareBinaryReader eabr = new EndianAwareBinaryReader(stream, false); // BigEndian
			
			eabr.BaseStream.Seek(0, SeekOrigin.Begin); // Starts on sector 0 on NeXT machines, CDs and floppies
			magic = eabr.ReadUInt32();
			
			if(magic == NEXT_MAGIC1 || magic == NEXT_MAGIC2 || magic == NEXT_MAGIC3)
				magic_found = true;
			else
			{
				eabr.BaseStream.Seek(0x1E00, SeekOrigin.Begin); // Starts on sector 15 on MBR machines
				magic = eabr.ReadUInt32();
				
				if(magic == NEXT_MAGIC1 || magic == NEXT_MAGIC2 || magic == NEXT_MAGIC3)
					magic_found = true;
				else
				{
					eabr.BaseStream.Seek(0x2000, SeekOrigin.Begin); // Starts on sector 16 (4 on CD) on RISC disks
					magic = eabr.ReadUInt32();
					
					if(magic == NEXT_MAGIC1 || magic == NEXT_MAGIC2 || magic == NEXT_MAGIC3)
						magic_found = true;
					else
						return false;
				}
			}
			
			if(magic_found)
			{
				eabr.BaseStream.Seek(88, SeekOrigin.Current); // Seek to sector size
				sector_size = eabr.ReadUInt32();
				eabr.BaseStream.Seek(16, SeekOrigin.Current); // Seek to front porch
				front_porch = eabr.ReadUInt16();
				
				eabr.BaseStream.Seek(76, SeekOrigin.Current); // Seek to first partition entry
				
				for(int i = 0; i < 8; i ++)
				{
					NeXTEntry entry = new NeXTEntry();

					entry.start = eabr.ReadUInt32();
					entry.sectors = eabr.ReadUInt32();
					entry.block_size = eabr.ReadUInt16();
					entry.frag_size = eabr.ReadUInt16();
					entry.optimization = eabr.ReadByte();
					entry.cpg = eabr.ReadUInt16();
					entry.bpi = eabr.ReadUInt16();
					entry.freemin = eabr.ReadByte();
					entry.unknown = eabr.ReadByte();
					entry.newfs = eabr.ReadByte();
					cString = eabr.ReadBytes(16);
					entry.mount_point = StringHandlers.CToString(cString);
					entry.automount = eabr.ReadByte();
					cString = eabr.ReadBytes(8);
					entry.type = StringHandlers.CToString(cString);
					entry.unknown2 = eabr.ReadByte();
					
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