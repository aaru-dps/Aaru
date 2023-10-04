// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Atheos filesystem plugin.
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection for the AtheOS filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class AtheOS
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        ulong sector = AFS_BOOTBLOCK_SIZE / imagePlugin.Info.SectorSize;
        uint  offset = AFS_BOOTBLOCK_SIZE % imagePlugin.Info.SectorSize;
        uint  run    = 1;

        if(imagePlugin.Info.SectorSize < AFS_SUPERBLOCK_SIZE)
            run = AFS_SUPERBLOCK_SIZE / imagePlugin.Info.SectorSize;

        if(sector + partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSectors(sector + partition.Start, run, out byte[] tmp);

        if(errno != ErrorNumber.NoError)
            return false;

        var sbSector = new byte[AFS_SUPERBLOCK_SIZE];

        if(offset + AFS_SUPERBLOCK_SIZE > tmp.Length)
            return false;

        Array.Copy(tmp, offset, sbSector, 0, AFS_SUPERBLOCK_SIZE);

        var magic = BitConverter.ToUInt32(sbSector, 0x20);

        return magic == AFS_MAGIC1;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        encoding    ??= Encoding.GetEncoding("iso-8859-15");
        information =   "";
        metadata    =   new FileSystem();

        var sb = new StringBuilder();

        ulong sector = AFS_BOOTBLOCK_SIZE / imagePlugin.Info.SectorSize;
        uint  offset = AFS_BOOTBLOCK_SIZE % imagePlugin.Info.SectorSize;
        uint  run    = 1;

        if(imagePlugin.Info.SectorSize < AFS_SUPERBLOCK_SIZE)
            run = AFS_SUPERBLOCK_SIZE / imagePlugin.Info.SectorSize;

        ErrorNumber errno = imagePlugin.ReadSectors(sector + partition.Start, run, out byte[] tmp);

        if(errno != ErrorNumber.NoError)
            return;

        var sbSector = new byte[AFS_SUPERBLOCK_SIZE];
        Array.Copy(tmp, offset, sbSector, 0, AFS_SUPERBLOCK_SIZE);

        SuperBlock afsSb = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sbSector);

        sb.AppendLine(Localization.Atheos_filesystem);

        if(afsSb.flags == 1)
            sb.AppendLine(Localization.Filesystem_is_read_only);

        sb.AppendFormat(Localization.Volume_name_0,      StringHandlers.CToString(afsSb.name, encoding)).AppendLine();
        sb.AppendFormat(Localization._0_bytes_per_block, afsSb.block_size).AppendLine();

        sb.AppendFormat(Localization._0_blocks_in_volume_1_bytes, afsSb.num_blocks,
                        afsSb.num_blocks * afsSb.block_size).
           AppendLine();

        sb.AppendFormat(Localization._0_used_blocks_1_bytes, afsSb.used_blocks, afsSb.used_blocks * afsSb.block_size).
           AppendLine();

        sb.AppendFormat(Localization._0_bytes_per_i_node, afsSb.inode_size).AppendLine();

        sb.AppendFormat(Localization._0_blocks_per_allocation_group_1_bytes, afsSb.blocks_per_ag,
                        afsSb.blocks_per_ag * afsSb.block_size).
           AppendLine();

        sb.AppendFormat(Localization._0_allocation_groups_in_volume, afsSb.num_ags).AppendLine();

        sb.AppendFormat(Localization.Journal_resides_in_block_0_of_allocation_group_1_and_runs_for_2_blocks_3_bytes,
                        afsSb.log_blocks_start, afsSb.log_blocks_ag, afsSb.log_blocks_len,
                        afsSb.log_blocks_len * afsSb.block_size).
           AppendLine();

        sb.AppendFormat(Localization.Journal_starts_in_byte_0_and_has_1_bytes_in_2_blocks, afsSb.log_start,
                        afsSb.log_size, afsSb.log_valid_blocks).
           AppendLine();

        sb.
            AppendFormat(Localization.Root_folder_s_i_node_resides_in_block_0_of_allocation_group_1_and_runs_for_2_blocks_3_bytes,
                         afsSb.root_dir_start, afsSb.root_dir_ag, afsSb.root_dir_len,
                         afsSb.root_dir_len * afsSb.block_size).
            AppendLine();

        sb.
            AppendFormat(Localization.Directory_containing_files_scheduled_for_deletion_i_node_resides_in_block_0_of_allocation_group_1_and_runs_for_2_blocks_3_bytes,
                         afsSb.deleted_start, afsSb.deleted_ag, afsSb.deleted_len,
                         afsSb.deleted_len * afsSb.block_size).
            AppendLine();

        sb.
            AppendFormat(Localization.Indices_i_node_resides_in_block_0_of_allocation_group_1_and_runs_for_2_blocks_3_bytes,
                         afsSb.indices_start, afsSb.indices_ag, afsSb.indices_len,
                         afsSb.indices_len * afsSb.block_size).
            AppendLine();

        sb.AppendFormat(Localization._0_blocks_for_bootloader_1_bytes, afsSb.boot_size,
                        afsSb.boot_size * afsSb.block_size).
           AppendLine();

        information = sb.ToString();

        metadata = new FileSystem
        {
            Clusters     = (ulong)afsSb.num_blocks,
            ClusterSize  = afsSb.block_size,
            Dirty        = false,
            FreeClusters = (ulong)(afsSb.num_blocks - afsSb.used_blocks),
            Type         = FS_TYPE,
            VolumeName   = StringHandlers.CToString(afsSb.name, encoding)
        };
    }

#endregion
}