using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using FileSystemIDandChk;

namespace FileSystemIDandChk.PartPlugins
{
    class NeXTDisklabel : PartPlugin
    {
        const UInt32 NEXT_MAGIC1 = 0x4E655854;
        // "NeXT"
        const UInt32 NEXT_MAGIC2 = 0x646C5632;
        // "dlV2"
        const UInt32 NEXT_MAGIC3 = 0x646C5633;
        // "dlV3"

        const UInt16 disktabStart     = 0xB4; // 180
        const UInt16 disktabEntrySize = 0x2C; // 44

        public NeXTDisklabel(PluginBase Core)
        {
            Name = "NeXT Disklabel";
            PluginUUID = new Guid("246A6D93-4F1A-1F8A-344D-50187A5513A9");
        }

        public override bool GetInformation(ImagePlugins.ImagePlugin imagePlugin, out List<Partition> partitions)
        {
            byte[] cString;
            bool magic_found;
            byte[] entry_sector;
			
            UInt32 magic;
            UInt32 sector_size;
            UInt16 front_porch;

            if (imagePlugin.GetSectorSize() == 2352 || imagePlugin.GetSectorSize() == 2448)
                sector_size = 2048;
            else
                sector_size = imagePlugin.GetSectorSize();
			
            partitions = new List<Partition>();

            entry_sector = imagePlugin.ReadSector(0); // Starts on sector 0 on NeXT machines, CDs and floppies
            magic = BigEndianBitConverter.ToUInt32(entry_sector, 0x00);

            if (magic == NEXT_MAGIC1 || magic == NEXT_MAGIC2 || magic == NEXT_MAGIC3)
                magic_found = true;
            else
            {
                entry_sector = imagePlugin.ReadSector(15); // Starts on sector 15 on MBR machines
                magic = BigEndianBitConverter.ToUInt32(entry_sector, 0x00);

                if (magic == NEXT_MAGIC1 || magic == NEXT_MAGIC2 || magic == NEXT_MAGIC3)
                    magic_found = true;
                else
                {
                    if (sector_size == 2048)
                        entry_sector = imagePlugin.ReadSector(4); // Starts on sector 4 on RISC CDs
                    else
                        entry_sector = imagePlugin.ReadSector(16); // Starts on sector 16 on RISC disks
                    magic = BigEndianBitConverter.ToUInt32(entry_sector, 0x00);
					
                    if (magic == NEXT_MAGIC1 || magic == NEXT_MAGIC2 || magic == NEXT_MAGIC3)
                        magic_found = true;
                    else
                        return false;
                }
            }
			
            front_porch = BigEndianBitConverter.ToUInt16(entry_sector, 0x6A);

            if (magic_found)
            {
                for (int i = 0; i < 8; i++)
                {
                    NeXTEntry entry = new NeXTEntry();

                    entry.start = BigEndianBitConverter.ToUInt32(entry_sector, disktabStart + disktabEntrySize * i + 0x00);
                    entry.sectors = BigEndianBitConverter.ToUInt32(entry_sector, disktabStart + disktabEntrySize * i + 0x04);
                    entry.block_size = BigEndianBitConverter.ToUInt16(entry_sector, disktabStart + disktabEntrySize * i + 0x08);
                    entry.frag_size = BigEndianBitConverter.ToUInt16(entry_sector, disktabStart + disktabEntrySize * i + 0x0A);
                    entry.optimization = entry_sector[disktabStart + disktabEntrySize * i + 0x0C];
                    entry.cpg = BigEndianBitConverter.ToUInt16(entry_sector, disktabStart + disktabEntrySize * i + 0x0D);
                    entry.bpi = BigEndianBitConverter.ToUInt16(entry_sector, disktabStart + disktabEntrySize * i + 0x0F);
                    entry.freemin = entry_sector[disktabStart + disktabEntrySize * i + 0x11];
                    entry.newfs = entry_sector[disktabStart + disktabEntrySize * i + 0x12];
                    cString = new byte[16];
                    Array.Copy(entry_sector, disktabStart + disktabEntrySize * i + 0x13, cString, 0, 16);
                    entry.mount_point = StringHandlers.CToString(cString);
                    entry.automount = entry_sector[disktabStart + disktabEntrySize * i + 0x23];
                    cString = new byte[8];
                    Array.Copy(entry_sector, disktabStart + disktabEntrySize * i + 0x24, cString, 0, 8);
                    entry.type = StringHandlers.CToString(cString);

                    if (entry.sectors > 0 && entry.sectors < 0xFFFFFFFF && entry.start < 0xFFFFFFFF)
                    {
                        Partition part = new Partition();
                        StringBuilder sb = new StringBuilder();
						
                        part.PartitionLength = (ulong)entry.sectors * sector_size;
                        part.PartitionStart = ((ulong)entry.start + front_porch) * sector_size;
                        part.PartitionType = entry.type;
                        part.PartitionSequence = (ulong)i;
                        part.PartitionName = entry.mount_point;
                        part.PartitionSectors = (ulong)entry.sectors;
                        part.PartitionStartSector = ((ulong)entry.start + front_porch);
						
                        sb.AppendFormat("{0} bytes per block", entry.block_size).AppendLine();
                        sb.AppendFormat("{0} bytes per fragment", entry.frag_size).AppendLine();
                        if (entry.optimization == 's')
                            sb.AppendLine("Space optimized");
                        else if (entry.optimization == 't')
                            sb.AppendLine("Time optimized");
                        else
                            sb.AppendFormat("Unknown optimization {0:X2}", entry.optimization).AppendLine();
                        sb.AppendFormat("{0} cylinders per group", entry.cpg).AppendLine();
                        sb.AppendFormat("{0} bytes per inode", entry.bpi).AppendLine();
                        sb.AppendFormat("{0}% of space must be free at minimum", entry.freemin).AppendLine();
                        if (entry.newfs != 1) // Seems to indicate newfs has been already run
							sb.AppendLine("Filesystem should be formatted at start");
                        if (entry.automount == 1)
                            sb.AppendLine("Filesystem should be automatically mounted");
						
                        part.PartitionDescription = sb.ToString();
						
                        partitions.Add(part);
                    }
                }
				
                return true;
            }
            return false;
        }

        struct NeXTEntry
        {
            public UInt32 start;
            // Sector of start, counting from front porch
            public UInt32 sectors;
            // Length in sectors
            public UInt16 block_size;
            // Filesystem's block size
            public UInt16 frag_size;
            // Filesystem's fragment size
            public byte optimization;
            // 's'pace or 't'ime
            public UInt16 cpg;
            // Cylinders per group
            public UInt16 bpi;
            // Bytes per inode
            public byte freemin;
            // % of minimum free space
            public byte newfs;
            // Should newfs be run on first start?
            public string mount_point;
            // Mount point or empty if mount where you want
            public byte automount;
            // Should automount
            public string type;
            // Filesystem type, always "4.3BSD"?
        }
    }
}