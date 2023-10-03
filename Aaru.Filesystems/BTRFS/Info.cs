// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : B-tree file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the B-tree file system and shows information.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the b-tree filesystem (btrfs)</summary>
public sealed partial class BTRFS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(partition.Start >= partition.End)
            return false;

        ulong sbSectorOff  = 0x10000 / imagePlugin.Info.SectorSize;
        uint  sbSectorSize = 0x1000  / imagePlugin.Info.SectorSize;

        if(sbSectorOff + partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSectors(sbSectorOff + partition.Start, sbSectorSize, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        SuperBlock btrfsSb;

        try
        {
            btrfsSb = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sector);
        }
        catch
        {
            return false;
        }

        AaruConsole.DebugWriteLine(MODULE_NAME, "sbSectorOff = {0}", sbSectorOff);
        AaruConsole.DebugWriteLine(MODULE_NAME, "sbSectorSize = {0}", sbSectorSize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "partition.PartitionStartSector = {0}", partition.Start);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.magic = 0x{0:X16}", btrfsSb.magic);

        return btrfsSb.magic == BTRFS_MAGIC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        var sbInformation = new StringBuilder();
        metadata    = new FileSystem();
        information = "";

        ulong sbSectorOff  = 0x10000 / imagePlugin.Info.SectorSize;
        uint  sbSectorSize = 0x1000  / imagePlugin.Info.SectorSize;

        ErrorNumber errno = imagePlugin.ReadSectors(sbSectorOff + partition.Start, sbSectorSize, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        SuperBlock btrfsSb = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sector);

        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.checksum = {0}", btrfsSb.checksum);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.uuid = {0}", btrfsSb.uuid);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.pba = {0}", btrfsSb.pba);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.flags = {0}", btrfsSb.flags);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.magic = {0}", btrfsSb.magic);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.generation = {0}", btrfsSb.generation);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.root_lba = {0}", btrfsSb.root_lba);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.chunk_lba = {0}", btrfsSb.chunk_lba);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.log_lba = {0}", btrfsSb.log_lba);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.log_root_transid = {0}", btrfsSb.log_root_transid);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.total_bytes = {0}", btrfsSb.total_bytes);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.bytes_used = {0}", btrfsSb.bytes_used);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.root_dir_objectid = {0}", btrfsSb.root_dir_objectid);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.num_devices = {0}", btrfsSb.num_devices);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.sectorsize = {0}", btrfsSb.sectorsize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.nodesize = {0}", btrfsSb.nodesize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.leafsize = {0}", btrfsSb.leafsize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.stripesize = {0}", btrfsSb.stripesize);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.n = {0}", btrfsSb.n);

        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.chunk_root_generation = {0}",
                                   btrfsSb.chunk_root_generation);

        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.compat_flags = 0x{0:X16}", btrfsSb.compat_flags);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.compat_ro_flags = 0x{0:X16}", btrfsSb.compat_ro_flags);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.incompat_flags = 0x{0:X16}", btrfsSb.incompat_flags);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.csum_type = {0}", btrfsSb.csum_type);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.root_level = {0}", btrfsSb.root_level);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.chunk_root_level = {0}", btrfsSb.chunk_root_level);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.log_root_level = {0}", btrfsSb.log_root_level);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.dev_item.id = 0x{0:X16}", btrfsSb.dev_item.id);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.dev_item.bytes = {0}", btrfsSb.dev_item.bytes);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.dev_item.used = {0}", btrfsSb.dev_item.used);

        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.dev_item.optimal_align = {0}",
                                   btrfsSb.dev_item.optimal_align);

        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.dev_item.optimal_width = {0}",
                                   btrfsSb.dev_item.optimal_width);

        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.dev_item.minimal_size = {0}",
                                   btrfsSb.dev_item.minimal_size);

        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.dev_item.type = {0}", btrfsSb.dev_item.type);

        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.dev_item.generation = {0}", btrfsSb.dev_item.generation);

        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.dev_item.start_offset = {0}",
                                   btrfsSb.dev_item.start_offset);

        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.dev_item.dev_group = {0}", btrfsSb.dev_item.dev_group);

        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.dev_item.seek_speed = {0}", btrfsSb.dev_item.seek_speed);

        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.dev_item.bandwidth = {0}", btrfsSb.dev_item.bandwidth);

        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.dev_item.device_uuid = {0}", btrfsSb.dev_item.device_uuid);

        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.dev_item.uuid = {0}", btrfsSb.dev_item.uuid);
        AaruConsole.DebugWriteLine(MODULE_NAME, "btrfsSb.label = {0}", btrfsSb.label);

        sbInformation.AppendLine(Localization.B_tree_filesystem);
        sbInformation.AppendFormat(Localization.UUID_0, btrfsSb.uuid).AppendLine();
        sbInformation.AppendFormat(Localization.This_superblock_resides_on_physical_block_0, btrfsSb.pba).AppendLine();
        sbInformation.AppendFormat(Localization.Root_tree_starts_at_LBA_0, btrfsSb.root_lba).AppendLine();
        sbInformation.AppendFormat(Localization.Chunk_tree_starts_at_LBA_0, btrfsSb.chunk_lba).AppendLine();
        sbInformation.AppendFormat(Localization.Log_tree_starts_at_LBA_0, btrfsSb.log_lba).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_has_0_bytes_spanned_in_1_devices, btrfsSb.total_bytes,
                                   btrfsSb.num_devices).AppendLine();

        sbInformation.AppendFormat(Localization.Volume_has_0_bytes_used, btrfsSb.bytes_used).AppendLine();
        sbInformation.AppendFormat(Localization._0_bytes_sector, btrfsSb.sectorsize).AppendLine();
        sbInformation.AppendFormat(Localization._0_bytes_node, btrfsSb.nodesize).AppendLine();
        sbInformation.AppendFormat(Localization._0_bytes_leaf, btrfsSb.leafsize).AppendLine();
        sbInformation.AppendFormat(Localization._0_bytes_stripe, btrfsSb.stripesize).AppendLine();
        sbInformation.AppendFormat(Localization.Flags_0, btrfsSb.flags).AppendLine();
        sbInformation.AppendFormat(Localization.Compatible_flags_0, btrfsSb.compat_flags).AppendLine();
        sbInformation.AppendFormat(Localization.Read_only_compatible_flags_0, btrfsSb.compat_ro_flags).AppendLine();
        sbInformation.AppendFormat(Localization.Incompatible_flags_0, btrfsSb.incompat_flags).AppendLine();
        sbInformation.AppendFormat(Localization.Device_UUID_0, btrfsSb.dev_item.uuid).AppendLine();
        sbInformation.AppendFormat(Localization.Volume_label_0, btrfsSb.label).AppendLine();

        information = sbInformation.ToString();

        metadata = new FileSystem
        {
            Clusters            = btrfsSb.total_bytes / btrfsSb.sectorsize,
            ClusterSize         = btrfsSb.sectorsize,
            VolumeName          = btrfsSb.label,
            VolumeSerial        = $"{btrfsSb.uuid}",
            VolumeSetIdentifier = $"{btrfsSb.dev_item.device_uuid}",
            Type                = FS_TYPE
        };

        metadata.FreeClusters = metadata.Clusters - (btrfsSb.bytes_used / btrfsSb.sectorsize);
    }
}