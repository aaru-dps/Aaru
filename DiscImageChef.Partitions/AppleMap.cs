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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;
using System.Collections.Generic;
using DiscImageChef.Console;
using System.Runtime.InteropServices;

namespace DiscImageChef.PartPlugins
{
    // Information about structures learnt from Inside Macintosh
    // Constants from image testing
    public class AppleMap : PartPlugin
    {
        /// <summary>"ER", driver descriptor magic</summary>
        const ushort DDM_MAGIC = 0x4552;
        /// <summary>"PM", new entry magic</summary>
        const ushort APM_MAGIC = 0x504D;
        /// <summary>"TS", old map magic</summary>
        const ushort APM_MAGIC_OLD = 0x5453;
        /// <summary>Old indicator for HFS partition, "TFS1"</summary>
        const uint HFS_MAGIC_OLD = 0x54465331;

        public AppleMap()
        {
            Name = "Apple Partition Map";
            PluginUUID = new Guid("36405F8D-4F1A-07F5-209C-223D735D6D22");
        }

        public override bool GetInformation(ImagePlugins.ImagePlugin imagePlugin, out List<CommonTypes.Partition> partitions)
        {
            uint sector_size;

            if(imagePlugin.GetSectorSize() == 2352 || imagePlugin.GetSectorSize() == 2448)
                sector_size = 2048;
            else
                sector_size = imagePlugin.GetSectorSize();

            partitions = new List<CommonTypes.Partition>();

            byte[] ddm_sector = imagePlugin.ReadSector(0);
            AppleDriverDescriptorMap ddm;

            ushort max_drivers = 61;

            if(sector_size == 256)
            {
                byte[] tmp = new byte[512];
                Array.Copy(ddm_sector, 0, tmp, 0, 256);
                ddm_sector = tmp;
                max_drivers = 29;
            }
            else if(sector_size < 256)
                return false;
            
            ddm = BigEndianMarshal.ByteArrayToStructureBigEndian<AppleDriverDescriptorMap>(ddm_sector);

            DicConsole.DebugWriteLine("AppleMap Plugin", "ddm.sbSig = 0x{0:X4}", ddm.sbSig);
            DicConsole.DebugWriteLine("AppleMap Plugin", "ddm.sbBlockSize = {0}", ddm.sbBlockSize);
            DicConsole.DebugWriteLine("AppleMap Plugin", "ddm.sbBlocks = {0}", ddm.sbBlocks);
            DicConsole.DebugWriteLine("AppleMap Plugin", "ddm.sbDevType = {0}", ddm.sbDevType);
            DicConsole.DebugWriteLine("AppleMap Plugin", "ddm.sbDevId = {0}", ddm.sbDevId);
            DicConsole.DebugWriteLine("AppleMap Plugin", "ddm.sbData = 0x{0:X8}", ddm.sbData);
            DicConsole.DebugWriteLine("AppleMap Plugin", "ddm.sbDrvrCount = {0}", ddm.sbDrvrCount);

            if(ddm.sbSig != DDM_MAGIC)
                return false;

            uint sequence = 0;

            if(ddm.sbDrvrCount < max_drivers)
            {
                ddm.sbMap = new AppleDriverEntry[ddm.sbDrvrCount];
                for(int i = 0; i < ddm.sbDrvrCount; i++)
                {
                    byte[] tmp = new byte[8];
                    Array.Copy(ddm_sector, 18 + i * 8, tmp, 0, 8);
                    ddm.sbMap[i] = BigEndianMarshal.ByteArrayToStructureBigEndian<AppleDriverEntry>(tmp);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "ddm.sbMap[{1}].ddBlock = {0}", ddm.sbMap[i].ddBlock, i);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "ddm.sbMap[{1}].ddSize = {0}", ddm.sbMap[i].ddSize, i);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "ddm.sbMap[{1}].ddType = {0}", ddm.sbMap[i].ddType, i);

                    CommonTypes.Partition part = new CommonTypes.Partition()
                    {
                        PartitionLength = (ulong)(ddm.sbMap[i].ddSize * 512),
                        PartitionSectors = (ulong)((ddm.sbMap[i].ddSize * 512) / sector_size),
                        PartitionSequence = sequence,
                        PartitionStart = ddm.sbMap[i].ddBlock * sector_size,
                        PartitionStartSector = ddm.sbMap[i].ddBlock,
                        PartitionType = "Apple_Driver"
                    };

                    partitions.Add(part);

                    sequence++;
                }
            }

            byte[] part_sector = imagePlugin.ReadSector(1);
            AppleOldDevicePartitionMap old_map = BigEndianMarshal.ByteArrayToStructureBigEndian<AppleOldDevicePartitionMap>(part_sector);

            // This is the easy one, no sector size mixing
            if(old_map.pdSig == APM_MAGIC_OLD)
            {
                for(int i = 2; i < part_sector.Length; i+=12)
                {
                    byte[] tmp = new byte[12];
                    Array.Copy(part_sector, i, tmp, 0, 12);
                    AppleMapOldPartitionEntry old_entry = BigEndianMarshal.ByteArrayToStructureBigEndian<AppleMapOldPartitionEntry>(tmp);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "old_map.sbMap[{1}].pdStart = {0}", old_entry.pdStart, (i - 2) / 12);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "old_map.sbMap[{1}].pdSize = {0}", old_entry.pdSize, (i - 2) / 12);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "old_map.sbMap[{1}].pdFSID = 0x{0:X8}", old_entry.pdFSID, (i - 2) / 12);

                    if(old_entry.pdSize == 0 && old_entry.pdFSID == 0)
                    {
                        if(old_entry.pdStart == 0)
                            break;
                        continue;
                    }

                    CommonTypes.Partition part = new CommonTypes.Partition
                    {
                        PartitionLength = old_entry.pdStart * ddm.sbBlockSize,
                        PartitionSectors = (old_entry.pdStart * ddm.sbBlockSize) / sector_size,
                        PartitionSequence = sequence,
                        PartitionStart = old_entry.pdSize * ddm.sbBlockSize,
                        PartitionStartSector = (old_entry.pdSize * ddm.sbBlockSize) / sector_size,
                    };

                    if(old_entry.pdFSID == HFS_MAGIC_OLD)
                        part.PartitionType = "Apple_HFS";
                    else
                        part.PartitionType = string.Format("0x{0:X8}", old_entry.pdFSID);

                    partitions.Add(part);

                    sequence++;
                }

                return true;
            }

            AppleMapPartitionEntry entry;
            uint entry_size;
            uint entry_count;
            uint sectors_to_read;
            uint skip_ddm;

            // If sector is bigger than 512
            if(sector_size > 512)
            {
                byte[] tmp = new byte[512];
                Array.Copy(ddm_sector, 512, tmp, 0, 512); 
                entry = BigEndianMarshal.ByteArrayToStructureBigEndian<AppleMapPartitionEntry>(tmp);
                // Check for a partition entry that's 512-byte aligned
                if(entry.signature == APM_MAGIC)
                {
                    DicConsole.DebugWriteLine("AppleMap Plugin", "Found misaligned entry.");
                    entry_size = 512;
                    entry_count = entry.entries;
                    skip_ddm = 512;
                    sectors_to_read = (((entry_count + 1) * 512) / sector_size) + 1;
                }
                else
                {
                    entry = BigEndianMarshal.ByteArrayToStructureBigEndian<AppleMapPartitionEntry>(part_sector);
                    if(entry.signature == APM_MAGIC)
                    {
                        DicConsole.DebugWriteLine("AppleMap Plugin", "Found aligned entry.");
                        entry_size = sector_size;
                        entry_count = entry.entries;
                        skip_ddm = sector_size;
                        sectors_to_read = entry_count + 2;
                    }
                    else
                        return true;
                }
            }
            else
            {
                entry = BigEndianMarshal.ByteArrayToStructureBigEndian<AppleMapPartitionEntry>(part_sector);
                if(entry.signature == APM_MAGIC)
                {
                    DicConsole.DebugWriteLine("AppleMap Plugin", "Found aligned entry.");
                    entry_size = sector_size;
                    entry_count = entry.entries;
                    skip_ddm = sector_size;
                    sectors_to_read = entry_count + 2;
                }
                else
                    return true;
            }

            byte[] entries = imagePlugin.ReadSectors(0, sectors_to_read);
            DicConsole.DebugWriteLine("AppleMap Plugin", "entry_size = {0}", entry_size);
            DicConsole.DebugWriteLine("AppleMap Plugin", "entry_count = {0}", entry_count);
            DicConsole.DebugWriteLine("AppleMap Plugin", "skip_ddm = {0}", skip_ddm);
            DicConsole.DebugWriteLine("AppleMap Plugin", "sectors_to_read = {0}", sectors_to_read);

            byte[] copy = new byte[entries.Length - skip_ddm];
            Array.Copy(entries, skip_ddm, copy, 0, copy.Length);
            entries = copy;

            for(int i = 0; i < entry_count; i++)
            {
                byte[] tmp = new byte[entry_size];
                Array.Copy(entries, i * entry_size, tmp, 0, entry_size);
                entry = BigEndianMarshal.ByteArrayToStructureBigEndian<AppleMapPartitionEntry>(tmp);
                if(entry.signature == APM_MAGIC)
                {
                    DicConsole.DebugWriteLine("AppleMap Plugin", "dpme[{0}].signature = 0x{1:X4}", i, entry.signature);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "dpme[{0}].reserved1 = 0x{1:X4}", i, entry.reserved1);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "dpme[{0}].entries = {1}", i, entry.entries);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "dpme[{0}].start = {1}", i, entry.start);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "dpme[{0}].sectors = {1}", i, entry.sectors);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "dpme[{0}].name = \"{1}\"", i, StringHandlers.CToString(entry.name));
                    DicConsole.DebugWriteLine("AppleMap Plugin", "dpme[{0}].type = \"{1}\"", i, StringHandlers.CToString(entry.type));
                    DicConsole.DebugWriteLine("AppleMap Plugin", "dpme[{0}].first_data_block = {1}", i, entry.first_data_block);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "dpme[{0}].data_sectors = {1}", i, entry.data_sectors);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "dpme[{0}].flags = {1}", i, (AppleMapFlags)entry.flags);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "dpme[{0}].first_boot_block = {1}", i, entry.first_boot_block);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "dpme[{0}].boot_size = {1}", i, entry.boot_size);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "dpme[{0}].load_address = 0x{1:X8}", i, entry.load_address);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "dpme[{0}].load_address2 = 0x{1:X8}", i, entry.load_address2);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "dpme[{0}].entry_point = 0x{1:X8}", i, entry.entry_point);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "dpme[{0}].entry_point2 = 0x{1:X8}", i, entry.entry_point2);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "dpme[{0}].checksum = 0x{1:X8}", i, entry.checksum);
                    DicConsole.DebugWriteLine("AppleMap Plugin", "dpme[{0}].processor = \"{1}\"", i, StringHandlers.CToString(entry.processor));

                    AppleMapFlags flags = (AppleMapFlags)entry.flags;

                    // BeOS doesn't mark its partitions as valid
                    //if(flags.HasFlag(AppleMapFlags.Valid) &&
                    if(StringHandlers.CToString(entry.type) != "Apple_partition_map" && entry.sectors > 0)
                    {
                        StringBuilder sb = new StringBuilder();

                        CommonTypes.Partition _partition = new CommonTypes.Partition
                        {
                            PartitionSequence = sequence,
                            PartitionType = StringHandlers.CToString(entry.type),
                            PartitionName = StringHandlers.CToString(entry.name),
                            PartitionStart = entry.start * entry_size,
                            PartitionLength = entry.sectors * entry_size,
                            PartitionStartSector = (entry.start * entry_size) / sector_size,
                            PartitionSectors = (entry.sectors * entry_size) / sector_size
                        };
                        sb.AppendLine("Partition flags:");
                        if(flags.HasFlag(AppleMapFlags.Valid))
                            sb.AppendLine("Partition is valid.");
                        if(flags.HasFlag(AppleMapFlags.Allocated))
                            sb.AppendLine("Partition entry is allocated.");
                        if(flags.HasFlag(AppleMapFlags.InUse))
                            sb.AppendLine("Partition is in use.");
                        if(flags.HasFlag(AppleMapFlags.Bootable))
                            sb.AppendLine("Partition is bootable.");
                        if(flags.HasFlag(AppleMapFlags.Readable))
                            sb.AppendLine("Partition is readable.");
                        if(flags.HasFlag(AppleMapFlags.Writable))
                            sb.AppendLine("Partition is writable.");

                        if(flags.HasFlag(AppleMapFlags.Bootable))
                        {
                            sb.AppendFormat("First boot sector: {0}", (entry.first_boot_block * entry_size) / sector_size).AppendLine();
                            sb.AppendFormat("Boot is {0} bytes.", entry.boot_size).AppendLine();
                            sb.AppendFormat("Boot load address: 0x{0:X8}", entry.load_address).AppendLine();
                            sb.AppendFormat("Boot entry point: 0x{0:X8}", entry.entry_point).AppendLine();
                            sb.AppendFormat("Boot code checksum: 0x{0:X8}", entry.checksum).AppendLine();
                            sb.AppendFormat("Processor: {0}", StringHandlers.CToString(entry.processor)).AppendLine();

                            if(flags.HasFlag(AppleMapFlags.PicCode))
                                sb.AppendLine("Partition's boot code is position independent.");
                        }

                        _partition.PartitionDescription = sb.ToString();
                        partitions.Add(_partition);
                    }
                }
            }

            return true;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AppleDriverDescriptorMap
        {
            /// <summary>Signature <see cref="DDM_MAGIC"/></summary>
            public ushort sbSig;
            /// <summary>Byter per sector</summary>
            public ushort sbBlockSize;
            /// <summary>Sectors of the disk</summary>
            public uint sbBlocks;
            /// <summary>Device type</summary>
            public ushort sbDevType;
            /// <summary>Device ID</summary>
            public ushort sbDevId;
            /// <summary>Reserved</summary>
            public uint sbData;
            /// <summary>Number of entries of the driver descriptor</summary>
            public ushort sbDrvrCount;
            /// <summary>Entries of the driver descriptor</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 61)]
            public AppleDriverEntry[] sbMap;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AppleDriverEntry
        {
            /// <summary>First sector of the driver</summary>
            public uint ddBlock;
            /// <summary>Size in 512bytes sectors of the driver</summary>
            public ushort ddSize;
            /// <summary>Operating system (MacOS = 1)</summary>
            public ushort ddType;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AppleOldDevicePartitionMap
        {
            /// <summary>Signature <see cref="APM_MAGIC_OLD"/></summary>
            public ushort pdSig;
            /// <summary>Entries of the driver descriptor</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
            public AppleMapOldPartitionEntry[] pdMap;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AppleMapOldPartitionEntry
        {
            /// <summary>First sector of the partition</summary>
            public uint pdStart;
            /// <summary>Number of sectors of the partition</summary>
            public uint pdSize;
            /// <summary>Partition type</summary>
            public uint pdFSID;
        }

        [Flags]
        public enum AppleMapFlags : uint
        {
            /// <summary>Partition is valid</summary>
            Valid = 0x01,
            /// <summary>Partition is allocated</summary>
            Allocated = 0x02,
            /// <summary>Partition is in use</summary>
            InUse = 0x04,
            /// <summary>Partition is bootable</summary>
            Bootable = 0x08,
            /// <summary>Partition is readable</summary>
            Readable = 0x10,
            /// <summary>Partition is writable</summary>
            Writable = 0x20,
            /// <summary>Partition boot code is position independent</summary>
            PicCode = 0x40,
            /// <summary>OS specific flag</summary>
            Specific1 = 0x80,
            /// <summary>OS specific flag</summary>
            Specific2 = 0x100,
            /// <summary>Unknown, seen in the wild</summary>
            Unknown = 0x200,
            /// <summary>Unknown, seen in the wild</summary>
            Unknown2 = 0x40000000,
            /// <summary>Reserved, not seen in the wild</summary>
            Reserved = 0xBFFFFC00,
        }


        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct AppleMapPartitionEntry
        {
            /// <summary>Signature <see cref="APM_MAGIC"/></summary>
            public ushort signature;
            /// <summary>Reserved</summary>
            public ushort reserved1;
            /// <summary>Number of entries on the partition map, each one sector</summary>
            public uint entries;
            /// <summary>First sector of the partition</summary>
            public uint start;
            /// <summary>Number of sectos of the partition</summary>
            public uint sectors;
            /// <summary>Partition name, 32 bytes, null-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] name;
            /// <summary>Partition type. 32 bytes, null-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] type;
            /// <summary>First sector of the data area</summary>
            public uint first_data_block;
            /// <summary>Number of sectors of the data area</summary>
            public uint data_sectors;
            /// <summary>Partition flags</summary>
            public uint flags;
            /// <summary>First sector of the boot code</summary>
            public uint first_boot_block;
            /// <summary>Size in bytes of the boot code</summary>
            public uint boot_size;
            /// <summary>Load address of the boot code</summary>
            public uint load_address;
            /// <summary>Load address of the boot code</summary>
            public uint load_address2;
            /// <summary>Entry point of the boot code</summary>
            public uint entry_point;
            /// <summary>Entry point of the boot code</summary>
            public uint entry_point2;
            /// <summary>Boot code checksum</summary>
            public uint checksum;
            /// <summary>Processor type, 16 bytes, null-padded</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] processor;
            /// <summary>Boot arguments</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public uint[] boot_arguments;
        }
    }
}