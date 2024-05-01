// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Partitions;

// Information about structures learnt from Inside Macintosh
// Constants from image testing
/// <inheritdoc />
/// <summary>Implements decoding of the Apple Partition Map</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class AppleMap : IPartition
{
    /// <summary>"ER", driver descriptor magic</summary>
    const ushort DDM_MAGIC = 0x4552;
    /// <summary>"PM", new entry magic</summary>
    const ushort APM_MAGIC = 0x504D;
    /// <summary>"TS", old map magic</summary>
    const ushort APM_MAGIC_OLD = 0x5453;
    /// <summary>Old indicator for HFS partition, "TFS1"</summary>
    const uint HFS_MAGIC_OLD = 0x54465331;
    const string MODULE_NAME = "Apple Partition Map (APM) Plugin";

#region IPartition Members

    /// <inheritdoc />
    public string Name => Localization.AppleMap_Name;

    /// <inheritdoc />
    public Guid Id => new("36405F8D-4F1A-07F5-209C-223D735D6D22");

    /// <inheritdoc />
    public string Author => Authors.NATALIA_PORTILLO;

    /// <inheritdoc />
    public bool GetInformation(IMediaImage imagePlugin, out List<Partition> partitions, ulong sectorOffset)
    {
        uint sectorSize = imagePlugin.Info.SectorSize is 2352 or 2448 ? 2048 : imagePlugin.Info.SectorSize;

        partitions = new List<Partition>();

        if(sectorOffset + 2 >= imagePlugin.Info.Sectors) return false;

        ErrorNumber errno = imagePlugin.ReadSector(sectorOffset, out byte[] ddmSector);

        if(errno != ErrorNumber.NoError) return false;

        ushort maxDrivers = 61;

        switch(sectorSize)
        {
            case 256:
            {
                var tmp = new byte[512];
                Array.Copy(ddmSector, 0, tmp, 0, 256);
                ddmSector  = tmp;
                maxDrivers = 29;

                break;
            }
            case < 256:
                return false;
        }

        AppleDriverDescriptorMap ddm = Marshal.ByteArrayToStructureBigEndian<AppleDriverDescriptorMap>(ddmSector);

        AaruConsole.DebugWriteLine(MODULE_NAME, "ddm.sbSig = 0x{0:X4}",  ddm.sbSig);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ddm.sbBlockSize = {0}", ddm.sbBlockSize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ddm.sbBlocks = {0}",    ddm.sbBlocks);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ddm.sbDevType = {0}",   ddm.sbDevType);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ddm.sbDevId = {0}",     ddm.sbDevId);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ddm.sbData = 0x{0:X8}", ddm.sbData);
        AaruConsole.DebugWriteLine(MODULE_NAME, "ddm.sbDrvrCount = {0}", ddm.sbDrvrCount);

        uint sequence = 0;

        if(ddm.sbSig == DDM_MAGIC)
        {
            if(ddm.sbDrvrCount < maxDrivers)
            {
                ddm.sbMap = new AppleDriverEntry[ddm.sbDrvrCount];

                for(var i = 0; i < ddm.sbDrvrCount; i++)
                {
                    var tmp = new byte[8];
                    Array.Copy(ddmSector, 18 + i * 8, tmp, 0, 8);
                    ddm.sbMap[i] = Marshal.ByteArrayToStructureBigEndian<AppleDriverEntry>(tmp);

                    AaruConsole.DebugWriteLine(MODULE_NAME, "ddm.sbMap[{1}].ddBlock = {0}", ddm.sbMap[i].ddBlock, i);

                    AaruConsole.DebugWriteLine(MODULE_NAME, "ddm.sbMap[{1}].ddSize = {0}", ddm.sbMap[i].ddSize, i);

                    AaruConsole.DebugWriteLine(MODULE_NAME, "ddm.sbMap[{1}].ddType = {0}", ddm.sbMap[i].ddType, i);

                    if(ddm.sbMap[i].ddSize == 0) continue;

                    var part = new Partition
                    {
                        Size     = (ulong)(ddm.sbMap[i].ddSize       * 512),
                        Length   = (ulong)(ddm.sbMap[i].ddSize * 512 / sectorSize),
                        Sequence = sequence,
                        Offset   = ddm.sbMap[i].ddBlock * sectorSize,
                        Start    = ddm.sbMap[i].ddBlock + sectorOffset,
                        Type     = "Apple_Driver"
                    };

                    if(ddm.sbMap[i].ddSize * 512 % sectorSize > 0) part.Length++;

                    partitions.Add(part);

                    sequence++;
                }
            }
        }

        errno = imagePlugin.ReadSector(1 + sectorOffset, out byte[] partSector);

        if(errno != ErrorNumber.NoError) return false;

        AppleOldDevicePartitionMap oldMap =
            Marshal.ByteArrayToStructureBigEndian<AppleOldDevicePartitionMap>(partSector);

        // This is the easy one, no sector size mixing
        if(oldMap.pdSig == APM_MAGIC_OLD)
        {
            for(var i = 2; i < partSector.Length; i += 12)
            {
                var tmp = new byte[12];
                Array.Copy(partSector, i, tmp, 0, 12);

                AppleMapOldPartitionEntry oldEntry =
                    Marshal.ByteArrayToStructureBigEndian<AppleMapOldPartitionEntry>(tmp);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "old_map.sbMap[{1}].pdStart = {0}",
                                           oldEntry.pdStart,
                                           (i - 2) / 12);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "old_map.sbMap[{1}].pdSize = {0}",
                                           oldEntry.pdSize,
                                           (i - 2) / 12);

                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           "old_map.sbMap[{1}].pdFSID = 0x{0:X8}",
                                           oldEntry.pdFSID,
                                           (i - 2) / 12);

                if(oldEntry is { pdSize: 0, pdFSID: 0 })
                {
                    if(oldEntry.pdStart == 0) break;

                    continue;
                }

                var part = new Partition
                {
                    Size     = oldEntry.pdStart                   * ddm.sbBlockSize,
                    Length   = oldEntry.pdStart * ddm.sbBlockSize / sectorSize,
                    Sequence = sequence,
                    Offset   = oldEntry.pdSize                   * ddm.sbBlockSize,
                    Start    = oldEntry.pdSize * ddm.sbBlockSize / sectorSize,
                    Scheme   = Name,
                    Type     = oldEntry.pdFSID == HFS_MAGIC_OLD ? "Apple_HFS" : $"0x{oldEntry.pdFSID:X8}"
                };

                partitions.Add(part);

                sequence++;
            }

            return partitions.Count > 0;
        }

        AppleMapPartitionEntry entry;
        uint                   entrySize;
        uint                   entryCount;
        uint                   sectorsToRead;
        uint                   skipDdm;

        // If sector is bigger than 512
        if(ddmSector.Length > 512)
        {
            var tmp = new byte[512];
            Array.Copy(ddmSector, 512, tmp, 0, 512);
            entry = Marshal.ByteArrayToStructureBigEndian<AppleMapPartitionEntry>(tmp);

            // Check for a partition entry that's 512-byte aligned
            if(entry.signature == APM_MAGIC)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_misaligned_entry);
                entrySize     = 512;
                entryCount    = entry.entries;
                skipDdm       = 512;
                sectorsToRead = (entryCount + 1) * 512 / sectorSize + 1;
            }
            else
            {
                entry = Marshal.ByteArrayToStructureBigEndian<AppleMapPartitionEntry>(partSector);

                if(entry.signature == APM_MAGIC)
                {
                    AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_aligned_entry);
                    entrySize     = sectorSize;
                    entryCount    = entry.entries;
                    skipDdm       = sectorSize;
                    sectorsToRead = entryCount + 2;
                }
                else
                    return partitions.Count > 0;
            }
        }
        else
        {
            entry = Marshal.ByteArrayToStructureBigEndian<AppleMapPartitionEntry>(partSector);

            if(entry.signature == APM_MAGIC)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_aligned_entry);
                entrySize     = sectorSize;
                entryCount    = entry.entries;
                skipDdm       = sectorSize;
                sectorsToRead = entryCount + 2;
            }
            else
                return partitions.Count > 0;
        }

        errno = imagePlugin.ReadSectors(sectorOffset, sectorsToRead, out byte[] entries);

        if(errno != ErrorNumber.NoError) return false;

        AaruConsole.DebugWriteLine(MODULE_NAME, "entry_size = {0}",      entrySize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "entry_count = {0}",     entryCount);
        AaruConsole.DebugWriteLine(MODULE_NAME, "skip_ddm = {0}",        skipDdm);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sectors_to_read = {0}", sectorsToRead);

        var copy = new byte[entries.Length - skipDdm];
        Array.Copy(entries, skipDdm, copy, 0, copy.Length);
        entries = copy;

        for(var i = 0; i < entryCount; i++)
        {
            var tmp = new byte[entrySize];
            Array.Copy(entries, i * entrySize, tmp, 0, entrySize);
            entry = Marshal.ByteArrayToStructureBigEndian<AppleMapPartitionEntry>(tmp);

            if(entry.signature != APM_MAGIC) continue;

            AaruConsole.DebugWriteLine(MODULE_NAME, "dpme[{0}].signature = 0x{1:X4}", i, entry.signature);
            AaruConsole.DebugWriteLine(MODULE_NAME, "dpme[{0}].reserved1 = 0x{1:X4}", i, entry.reserved1);
            AaruConsole.DebugWriteLine(MODULE_NAME, "dpme[{0}].entries = {1}",        i, entry.entries);
            AaruConsole.DebugWriteLine(MODULE_NAME, "dpme[{0}].start = {1}",          i, entry.start);
            AaruConsole.DebugWriteLine(MODULE_NAME, "dpme[{0}].sectors = {1}",        i, entry.sectors);

            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       "dpme[{0}].name = \"{1}\"",
                                       i,
                                       StringHandlers.CToString(entry.name));

            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       "dpme[{0}].type = \"{1}\"",
                                       i,
                                       StringHandlers.CToString(entry.type));

            AaruConsole.DebugWriteLine(MODULE_NAME, "dpme[{0}].first_data_block = {1}", i, entry.first_data_block);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dpme[{0}].data_sectors = {1}", i, entry.data_sectors);
            AaruConsole.DebugWriteLine(MODULE_NAME, "dpme[{0}].flags = {1}",        i, (AppleMapFlags)entry.flags);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dpme[{0}].first_boot_block = {1}", i, entry.first_boot_block);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dpme[{0}].boot_size = {1}", i, entry.boot_size);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dpme[{0}].load_address = 0x{1:X8}", i, entry.load_address);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dpme[{0}].load_address2 = 0x{1:X8}", i, entry.load_address2);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dpme[{0}].entry_point = 0x{1:X8}", i, entry.entry_point);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dpme[{0}].entry_point2 = 0x{1:X8}", i, entry.entry_point2);

            AaruConsole.DebugWriteLine(MODULE_NAME, "dpme[{0}].checksum = 0x{1:X8}", i, entry.checksum);

            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       "dpme[{0}].processor = \"{1}\"",
                                       i,
                                       StringHandlers.CToString(entry.processor));

            var flags = (AppleMapFlags)entry.flags;

            // BeOS doesn't mark its partitions as valid
            //if(flags.HasFlag(AppleMapFlags.Valid) &&
            if(StringHandlers.CToString(entry.type) == "Apple_partition_map" || entry.sectors <= 0) continue;

            var sb = new StringBuilder();

            var partition = new Partition
            {
                Sequence = sequence,
                Type     = StringHandlers.CToString(entry.type),
                Name     = StringHandlers.CToString(entry.name),
                Offset   = entry.start   * entrySize,
                Size     = entry.sectors * entrySize,
                Start    = entry.start * entrySize / sectorSize + sectorOffset,
                Length   = entry.sectors           * entrySize / sectorSize,
                Scheme   = Name
            };

            sb.AppendLine(Localization.Partition_flags);

            if(flags.HasFlag(AppleMapFlags.Valid)) sb.AppendLine(Localization.Partition_is_valid);

            if(flags.HasFlag(AppleMapFlags.Allocated)) sb.AppendLine(Localization.Partition_entry_is_allocated);

            if(flags.HasFlag(AppleMapFlags.InUse)) sb.AppendLine(Localization.Partition_is_in_use);

            if(flags.HasFlag(AppleMapFlags.Bootable)) sb.AppendLine(Localization.Partition_is_bootable);

            if(flags.HasFlag(AppleMapFlags.Readable)) sb.AppendLine(Localization.Partition_is_readable);

            if(flags.HasFlag(AppleMapFlags.Writable)) sb.AppendLine(Localization.Partition_is_writable);

            if(flags.HasFlag(AppleMapFlags.Bootable))
            {
                sb.AppendFormat(Localization.First_boot_sector_0, entry.first_boot_block * entrySize / sectorSize)
                  .AppendLine();

                sb.AppendFormat(Localization.Boot_is_0_bytes, entry.boot_size).AppendLine();
                sb.AppendFormat(Localization.Boot_load_address_0_X8, entry.load_address).AppendLine();
                sb.AppendFormat(Localization.Boot_entry_point_0_X8, entry.entry_point).AppendLine();
                sb.AppendFormat(Localization.Boot_code_checksum_0_X8, entry.checksum).AppendLine();
                sb.AppendFormat(Localization.Processor_0, StringHandlers.CToString(entry.processor)).AppendLine();

                if(flags.HasFlag(AppleMapFlags.PicCode))
                    sb.AppendLine(Localization.Partition_boot_code_is_position_independent);
            }

            partition.Description = sb.ToString();

            if(partition.Start < imagePlugin.Info.Sectors && partition.End < imagePlugin.Info.Sectors)
            {
                partitions.Add(partition);
                sequence++;
            }

            // Some CD and DVDs end with an Apple_Free that expands beyond the disc size...
            else if(partition.Start < imagePlugin.Info.Sectors)
            {
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.Cutting_last_partition_end_0_to_media_size_1,
                                           partition.End,
                                           imagePlugin.Info.Sectors - 1);

                partition.Length = imagePlugin.Info.Sectors - partition.Start;
                partitions.Add(partition);
                sequence++;
            }
            else
            {
                AaruConsole.DebugWriteLine(MODULE_NAME,
                                           Localization.Not_adding_partition_because_start_0_is_outside_media_size_1,
                                           partition.Start,
                                           imagePlugin.Info.Sectors - 1);
            }
        }

        return partitions.Count > 0;
    }

#endregion

#region Nested type: AppleDriverDescriptorMap

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct AppleDriverDescriptorMap
    {
        /// <summary>Signature <see cref="DDM_MAGIC" /></summary>
        public readonly ushort sbSig;
        /// <summary>Bytes per sector</summary>
        public readonly ushort sbBlockSize;
        /// <summary>Sectors of the disk</summary>
        public readonly uint sbBlocks;
        /// <summary>Device type</summary>
        public readonly ushort sbDevType;
        /// <summary>Device ID</summary>
        public readonly ushort sbDevId;
        /// <summary>Reserved</summary>
        public readonly uint sbData;
        /// <summary>Number of entries of the driver descriptor</summary>
        public readonly ushort sbDrvrCount;
        /// <summary>Entries of the driver descriptor</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 61)]
        public AppleDriverEntry[] sbMap;
    }

#endregion

#region Nested type: AppleDriverEntry

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct AppleDriverEntry
    {
        /// <summary>First sector of the driver</summary>
        public readonly uint ddBlock;
        /// <summary>Size in 512bytes sectors of the driver</summary>
        public readonly ushort ddSize;
        /// <summary>Operating system (MacOS = 1)</summary>
        public readonly ushort ddType;
    }

#endregion

#region Nested type: AppleMapFlags

    [Flags]
    enum AppleMapFlags : uint
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
        Reserved = 0xBFFFFC00
    }

#endregion

#region Nested type: AppleMapOldPartitionEntry

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct AppleMapOldPartitionEntry
    {
        /// <summary>First sector of the partition</summary>
        public readonly uint pdStart;
        /// <summary>Number of sectors of the partition</summary>
        public readonly uint pdSize;
        /// <summary>Partition type</summary>
        public readonly uint pdFSID;
    }

#endregion

#region Nested type: AppleMapPartitionEntry

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct AppleMapPartitionEntry
    {
        /// <summary>Signature <see cref="APM_MAGIC" /></summary>
        public readonly ushort signature;
        /// <summary>Reserved</summary>
        public readonly ushort reserved1;
        /// <summary>Number of entries on the partition map, each one sector</summary>
        public readonly uint entries;
        /// <summary>First sector of the partition</summary>
        public readonly uint start;
        /// <summary>Number of sectos of the partition</summary>
        public readonly uint sectors;
        /// <summary>Partition name, 32 bytes, null-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] name;
        /// <summary>Partition type. 32 bytes, null-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] type;
        /// <summary>First sector of the data area</summary>
        public readonly uint first_data_block;
        /// <summary>Number of sectors of the data area</summary>
        public readonly uint data_sectors;
        /// <summary>Partition flags</summary>
        public readonly uint flags;
        /// <summary>First sector of the boot code</summary>
        public readonly uint first_boot_block;
        /// <summary>Size in bytes of the boot code</summary>
        public readonly uint boot_size;
        /// <summary>Load address of the boot code</summary>
        public readonly uint load_address;
        /// <summary>Load address of the boot code</summary>
        public readonly uint load_address2;
        /// <summary>Entry point of the boot code</summary>
        public readonly uint entry_point;
        /// <summary>Entry point of the boot code</summary>
        public readonly uint entry_point2;
        /// <summary>Boot code checksum</summary>
        public readonly uint checksum;
        /// <summary>Processor type, 16 bytes, null-padded</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] processor;
        /// <summary>Boot arguments</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly uint[] boot_arguments;
    }

#endregion

#region Nested type: AppleOldDevicePartitionMap

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct AppleOldDevicePartitionMap
    {
        /// <summary>Signature <see cref="APM_MAGIC_OLD" /></summary>
        public readonly ushort pdSig;
        /// <summary>Entries of the driver descriptor</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 42)]
        public readonly AppleMapOldPartitionEntry[] pdMap;
    }

#endregion
}