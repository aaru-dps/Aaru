// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AppleMap.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Partitioning scheme plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Manages Apple Partition Map.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using DiscImageChef;

// Information about structures learnt from Inside Macintosh
// Constants from image testing
using DiscImageChef.Console;


namespace DiscImageChef.PartPlugins
{
    class AppleMap : PartPlugin
    {
        // "ER"
        const UInt16 APM_MAGIC = 0x4552;
        // "PM"
        const UInt16 APM_ENTRY = 0x504D;
        // "TS", old entry magic
        const UInt16 APM_OLDENT = 0x5453;

        public AppleMap()
        {
            Name = "Apple Partition Map";
            PluginUUID = new Guid("36405F8D-4F1A-07F5-209C-223D735D6D22");
        }

        public override bool GetInformation(ImagePlugins.ImagePlugin imagePlugin, out List<CommonTypes.Partition> partitions)
        {
            ulong apm_entries;
            uint sector_size;

            if(imagePlugin.GetSectorSize() == 2352 || imagePlugin.GetSectorSize() == 2448)
                sector_size = 2048;
            else
                sector_size = imagePlugin.GetSectorSize();

            partitions = new List<CommonTypes.Partition>();

            AppleMapBootEntry APMB = new AppleMapBootEntry();
            AppleMapPartitionEntry APMEntry;

            byte[] APMB_sector = imagePlugin.ReadSector(0);

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            APMB.signature = BigEndianBitConverter.ToUInt16(APMB_sector, 0x00);
            APMB.sector_size = BigEndianBitConverter.ToUInt16(APMB_sector, 0x02);
            APMB.sectors = BigEndianBitConverter.ToUInt32(APMB_sector, 0x04);
            APMB.reserved1 = BigEndianBitConverter.ToUInt16(APMB_sector, 0x08);
            APMB.reserved2 = BigEndianBitConverter.ToUInt16(APMB_sector, 0x0A);
            APMB.reserved3 = BigEndianBitConverter.ToUInt32(APMB_sector, 0x0C);
            APMB.driver_entries = BigEndianBitConverter.ToUInt16(APMB_sector, 0x10);
            APMB.first_driver_blk = BigEndianBitConverter.ToUInt32(APMB_sector, 0x12);
            APMB.driver_size = BigEndianBitConverter.ToUInt16(APMB_sector, 0x16);
            APMB.operating_system = BigEndianBitConverter.ToUInt16(APMB_sector, 0x18);

            DicConsole.DebugWriteLine("Apple Partition Map plugin", "APMB.signature = {0:X4}", APMB.signature);
            DicConsole.DebugWriteLine("Apple Partition Map plugin", "APMB.sector_size = {0}", APMB.sector_size);
            DicConsole.DebugWriteLine("Apple Partition Map plugin", "APMB.sectors = {0}", APMB.sectors);
            DicConsole.DebugWriteLine("Apple Partition Map plugin", "APMB.reserved1 = {0:X4}", APMB.reserved1);
            DicConsole.DebugWriteLine("Apple Partition Map plugin", "APMB.reserved2 = {0:X4}", APMB.reserved2);
            DicConsole.DebugWriteLine("Apple Partition Map plugin", "APMB.reserved3 = {0:X8}", APMB.reserved3);
            DicConsole.DebugWriteLine("Apple Partition Map plugin", "APMB.driver_entries = {0}", APMB.driver_entries);
            DicConsole.DebugWriteLine("Apple Partition Map plugin", "APMB.first_driver_blk = {0}", APMB.first_driver_blk);
            DicConsole.DebugWriteLine("Apple Partition Map plugin", "APMB.driver_size = {0}", APMB.driver_size);
            DicConsole.DebugWriteLine("Apple Partition Map plugin", "APMB.operating_system = {0}", APMB.operating_system);

            ulong first_sector = 0;

            if(APMB.signature == APM_MAGIC) // APM boot block found, APM starts in next sector
                first_sector = 1;

            // Read first entry
            byte[] APMEntry_sector;
            bool APMFromHDDOnCD = false;

            if(sector_size == 2048)
            {
                APMEntry_sector = Read2048SectorAs512(imagePlugin, first_sector);
                APMEntry = DecodeAPMEntry(APMEntry_sector);

                if(APMEntry.signature == APM_ENTRY || APMEntry.signature == APM_OLDENT)
                {
                    sector_size = 512;
                    APMFromHDDOnCD = true;
                    DicConsole.DebugWriteLine("Apple Partition Map plugin", "PM sector size is 512 bytes, but device's 2048");
                }
                else
                {
                    APMEntry_sector = imagePlugin.ReadSector(first_sector);
                    APMEntry = DecodeAPMEntry(APMEntry_sector);

                    if(APMEntry.signature != APM_ENTRY && APMEntry.signature != APM_OLDENT)
                        return false;
                }
            }
            else
            {
                APMEntry_sector = imagePlugin.ReadSector(first_sector);
                APMEntry = DecodeAPMEntry(APMEntry_sector);

                if(APMEntry.signature != APM_ENTRY && APMEntry.signature != APM_OLDENT)
                    return false;
            }

            if(APMEntry.entries <= 1)
                return false;

            apm_entries = APMEntry.entries;

            for(ulong i = 0; i < apm_entries; i++) // For each partition
            {
                if(APMFromHDDOnCD)
                    APMEntry_sector = Read2048SectorAs512(imagePlugin, first_sector + i);
                else
                    APMEntry_sector = imagePlugin.ReadSector(first_sector + i);

                APMEntry = DecodeAPMEntry(APMEntry_sector);

                if(APMEntry.signature == APM_ENTRY || APMEntry.signature == APM_OLDENT) // It should have partition entry signature
                {
                    CommonTypes.Partition _partition = new CommonTypes.Partition();
                    StringBuilder sb = new StringBuilder();

                    _partition.PartitionSequence = i;
                    _partition.PartitionType = APMEntry.type;
                    _partition.PartitionName = APMEntry.name;
                    _partition.PartitionStart = APMEntry.start * sector_size;
                    _partition.PartitionLength = APMEntry.sectors * sector_size;
                    _partition.PartitionStartSector = APMEntry.start;
                    _partition.PartitionSectors = APMEntry.sectors;

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

        static byte[] Read2048SectorAs512(ImagePlugins.ImagePlugin imagePlugin, UInt64 LBA)
        {
            UInt64 LBA2k = LBA / 4;
            int Remainder = (int)(LBA % 4);

            byte[] buffer = imagePlugin.ReadSector(LBA2k);
            byte[] sector = new byte[512];

            Array.Copy(buffer, Remainder * 512, sector, 0, 512);

            return sector;
        }

        static AppleMapPartitionEntry DecodeAPMEntry(byte[] APMEntry_sector)
        {
            AppleMapPartitionEntry APMEntry = new AppleMapPartitionEntry();
            byte[] cString;

            APMEntry.signature = BigEndianBitConverter.ToUInt16(APMEntry_sector, 0x00);
            APMEntry.reserved1 = BigEndianBitConverter.ToUInt16(APMEntry_sector, 0x02);
            APMEntry.entries = BigEndianBitConverter.ToUInt32(APMEntry_sector, 0x04);
            APMEntry.start = BigEndianBitConverter.ToUInt32(APMEntry_sector, 0x08);
            APMEntry.sectors = BigEndianBitConverter.ToUInt32(APMEntry_sector, 0x0C);
            cString = new byte[32];
            Array.Copy(APMEntry_sector, 0x10, cString, 0, 32);
            APMEntry.name = StringHandlers.CToString(cString);
            cString = new byte[32];
            Array.Copy(APMEntry_sector, 0x30, cString, 0, 32);
            APMEntry.type = StringHandlers.CToString(cString);
            APMEntry.first_data_block = BigEndianBitConverter.ToUInt32(APMEntry_sector, 0x50);
            APMEntry.data_sectors = BigEndianBitConverter.ToUInt32(APMEntry_sector, 0x54);
            APMEntry.status = BigEndianBitConverter.ToUInt32(APMEntry_sector, 0x58);
            APMEntry.first_boot_block = BigEndianBitConverter.ToUInt32(APMEntry_sector, 0x5C);
            APMEntry.boot_size = BigEndianBitConverter.ToUInt32(APMEntry_sector, 0x60);
            APMEntry.load_address = BigEndianBitConverter.ToUInt32(APMEntry_sector, 0x64);
            APMEntry.reserved2 = BigEndianBitConverter.ToUInt32(APMEntry_sector, 0x68);
            APMEntry.entry_point = BigEndianBitConverter.ToUInt32(APMEntry_sector, 0x6C);
            APMEntry.reserved3 = BigEndianBitConverter.ToUInt32(APMEntry_sector, 0x70);
            APMEntry.checksum = BigEndianBitConverter.ToUInt32(APMEntry_sector, 0x74);
            cString = new byte[16];
            Array.Copy(APMEntry_sector, 0x78, cString, 0, 16);
            APMEntry.processor = StringHandlers.CToString(cString);

            return APMEntry;
        }

        public struct AppleMapBootEntry
        {
            // Signature ("ER")
            public UInt16 signature;
            // Byter per sector
            public UInt16 sector_size;
            // Sectors of the disk
            public UInt32 sectors;
            // Reserved
            public UInt16 reserved1;
            // Reserved
            public UInt16 reserved2;
            // Reserved
            public UInt32 reserved3;
            // Number of entries of the driver descriptor
            public UInt16 driver_entries;
            // First sector of the driver
            public UInt32 first_driver_blk;
            // Size in 512bytes sectors of the driver
            public UInt16 driver_size;
            // Operating system (MacOS = 1)
            public UInt16 operating_system;
        }

        public struct AppleMapPartitionEntry
        {
            // Signature ("PM" or "TS")
            public UInt16 signature;
            // Reserved
            public UInt16 reserved1;
            // Number of entries on the partition map, each one sector
            public UInt32 entries;
            // First sector of the partition
            public UInt32 start;
            // Number of sectos of the partition
            public UInt32 sectors;
            // Partition name, 32 bytes, null-padded
            public string name;
            // Partition type. 32 bytes, null-padded
            public string type;
            // First sector of the data area
            public UInt32 first_data_block;
            // Number of sectors of the data area
            public UInt32 data_sectors;
            // Partition status
            public UInt32 status;
            // First sector of the boot code
            public UInt32 first_boot_block;
            // Size in bytes of the boot code
            public UInt32 boot_size;
            // Load address of the boot code
            public UInt32 load_address;
            // Reserved
            public UInt32 reserved2;
            // Entry point of the boot code
            public UInt32 entry_point;
            // Reserved
            public UInt32 reserved3;
            // Boot code checksum
            public UInt32 checksum;
            // Processor type, 16 bytes, null-padded
            public string processor;
        }
    }
}