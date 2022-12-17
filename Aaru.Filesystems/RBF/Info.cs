// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Random Block File filesystem plugin
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
/// <summary>Implements detection of the Locus filesystem</summary>
public sealed partial class RBF : IFilesystem
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(imagePlugin.Info.SectorSize < 256)
            return false;

        // Documentation says ID should be sector 0
        // I've found that OS-9/X68000 has it on sector 4
        // I've read OS-9/Apple2 has it on sector 15
        foreach(int i in new[]
                {
                    0, 4, 15
                })
        {
            ulong location = (ulong)i;

            uint sbSize = (uint)(Marshal.SizeOf<IdSector>() / imagePlugin.Info.SectorSize);

            if(Marshal.SizeOf<IdSector>() % imagePlugin.Info.SectorSize != 0)
                sbSize++;

            if(partition.Start + location + sbSize >= imagePlugin.Info.Sectors)
                break;

            ErrorNumber errno = imagePlugin.ReadSectors(partition.Start + location, sbSize, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                return false;

            if(sector.Length < Marshal.SizeOf<IdSector>())
                return false;

            IdSector    rbfSb     = Marshal.ByteArrayToStructureBigEndian<IdSector>(sector);
            NewIdSector rbf9000Sb = Marshal.ByteArrayToStructureBigEndian<NewIdSector>(sector);

            AaruConsole.DebugWriteLine("RBF plugin", Localization.magic_at_0_equals_1_or_2_expected_3_or_4, location,
                                       rbfSb.dd_sync, rbf9000Sb.rid_sync, RBF_SYNC, RBF_CNYS);

            if(rbfSb.dd_sync == RBF_SYNC ||
               rbf9000Sb.rid_sync is RBF_SYNC or RBF_CNYS)
                return true;
        }

        return false;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";
        metadata    = new FileSystem();

        if(imagePlugin.Info.SectorSize < 256)
            return;

        var rbfSb     = new IdSector();
        var rbf9000Sb = new NewIdSector();

        foreach(int i in new[]
                {
                    0, 4, 15
                })
        {
            ulong location = (ulong)i;
            uint  sbSize   = (uint)(Marshal.SizeOf<IdSector>() / imagePlugin.Info.SectorSize);

            if(Marshal.SizeOf<IdSector>() % imagePlugin.Info.SectorSize != 0)
                sbSize++;

            ErrorNumber errno = imagePlugin.ReadSectors(partition.Start + location, sbSize, out byte[] sector);

            if(errno != ErrorNumber.NoError)
                return;

            if(sector.Length < Marshal.SizeOf<IdSector>())
                return;

            rbfSb     = Marshal.ByteArrayToStructureBigEndian<IdSector>(sector);
            rbf9000Sb = Marshal.ByteArrayToStructureBigEndian<NewIdSector>(sector);

            AaruConsole.DebugWriteLine("RBF plugin", Localization.magic_at_0_equals_1_or_2_expected_3_or_4, location,
                                       rbfSb.dd_sync, rbf9000Sb.rid_sync, RBF_SYNC, RBF_CNYS);

            if(rbfSb.dd_sync == RBF_SYNC ||
               rbf9000Sb.rid_sync is RBF_SYNC or RBF_CNYS)
                break;
        }

        if(rbfSb.dd_sync      != RBF_SYNC &&
           rbf9000Sb.rid_sync != RBF_SYNC &&
           rbf9000Sb.rid_sync != RBF_CNYS)
            return;

        if(rbf9000Sb.rid_sync == RBF_CNYS)
            rbf9000Sb = (NewIdSector)Marshal.SwapStructureMembersEndian(rbf9000Sb);

        var sb = new StringBuilder();

        sb.AppendLine(Localization.OS_9_Random_Block_File);

        if(rbf9000Sb.rid_sync == RBF_SYNC)
        {
            sb.AppendFormat(Localization.Volume_ID_0_X8, rbf9000Sb.rid_diskid).AppendLine();
            sb.AppendFormat(Localization._0_blocks_in_volume, rbf9000Sb.rid_totblocks).AppendLine();
            sb.AppendFormat(Localization._0_cylinders, rbf9000Sb.rid_cylinders).AppendLine();
            sb.AppendFormat(Localization._0_blocks_in_cylinder_zero, rbf9000Sb.rid_cyl0size).AppendLine();
            sb.AppendFormat(Localization._0_blocks_per_cylinder, rbf9000Sb.rid_cylsize).AppendLine();
            sb.AppendFormat(Localization._0_heads, rbf9000Sb.rid_heads).AppendLine();
            sb.AppendFormat(Localization._0_bytes_per_block, rbf9000Sb.rid_blocksize).AppendLine();

            // TODO: Convert to flags?
            sb.AppendLine((rbf9000Sb.rid_format & 0x01) == 0x01 ? Localization.Disk_is_double_sided
                              : Localization.Disk_is_single_sided);

            sb.AppendLine((rbf9000Sb.rid_format & 0x02) == 0x02 ? Localization.Disk_is_double_density
                              : Localization.Disk_is_single_density);

            if((rbf9000Sb.rid_format & 0x10) == 0x10)
                sb.AppendLine(Localization.Disk_is_384_TPI);
            else if((rbf9000Sb.rid_format & 0x08) == 0x08)
                sb.AppendLine(Localization.Disk_is_192_TPI);
            else if((rbf9000Sb.rid_format & 0x04) == 0x04)
                sb.AppendLine(Localization.Disk_is_96_TPI_or_135_TPI);
            else
                sb.AppendLine(Localization.Disk_is_48_TPI);

            sb.AppendFormat(Localization.Allocation_bitmap_descriptor_starts_at_block_0,
                            rbf9000Sb.rid_bitmap == 0 ? 1 : rbf9000Sb.rid_bitmap).AppendLine();

            if(rbf9000Sb.rid_firstboot > 0)
                sb.AppendFormat(Localization.Debugger_descriptor_starts_at_block_0, rbf9000Sb.rid_firstboot).
                   AppendLine();

            if(rbf9000Sb.rid_bootfile > 0)
                sb.AppendFormat(Localization.Boot_file_descriptor_starts_at_block_0, rbf9000Sb.rid_bootfile).
                   AppendLine();

            sb.AppendFormat(Localization.Root_directory_descriptor_starts_at_block_0, rbf9000Sb.rid_rootdir).
               AppendLine();

            sb.AppendFormat(Localization.Disk_is_owned_by_group_0_user_1, rbf9000Sb.rid_group, rbf9000Sb.rid_owner).
               AppendLine();

            sb.AppendFormat(Localization.Volume_was_created_on_0, DateHandlers.UnixToDateTime(rbf9000Sb.rid_ctime)).
               AppendLine();

            sb.AppendFormat(Localization.Volume_identification_block_was_last_written_on_0,
                            DateHandlers.UnixToDateTime(rbf9000Sb.rid_mtime)).AppendLine();

            sb.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(rbf9000Sb.rid_name, Encoding)).
               AppendLine();

            metadata = new FileSystem
            {
                Type             = FS_TYPE,
                Bootable         = rbf9000Sb.rid_bootfile > 0,
                ClusterSize      = rbf9000Sb.rid_blocksize,
                Clusters         = rbf9000Sb.rid_totblocks,
                CreationDate     = DateHandlers.UnixToDateTime(rbf9000Sb.rid_ctime),
                ModificationDate = DateHandlers.UnixToDateTime(rbf9000Sb.rid_mtime),
                VolumeName       = StringHandlers.CToString(rbf9000Sb.rid_name, Encoding),
                VolumeSerial     = $"{rbf9000Sb.rid_diskid:X8}"
            };
        }
        else
        {
            sb.AppendFormat(Localization.Volume_ID_0_X4, rbfSb.dd_dsk).AppendLine();
            sb.AppendFormat(Localization._0_blocks_in_volume, LSNToUInt32(rbfSb.dd_tot)).AppendLine();
            sb.AppendFormat(Localization._0_tracks, rbfSb.dd_tks).AppendLine();
            sb.AppendFormat(Localization._0_sectors_per_track, rbfSb.dd_spt).AppendLine();
            sb.AppendFormat(Localization._0_bytes_per_sector, 256 << rbfSb.dd_lsnsize).AppendLine();

            sb.AppendFormat(Localization._0_sectors_per_cluster_1_bytes, rbfSb.dd_bit,
                            rbfSb.dd_bit * (256 << rbfSb.dd_lsnsize)).AppendLine();

            // TODO: Convert to flags?
            sb.AppendLine((rbfSb.dd_fmt & 0x01) == 0x01 ? Localization.Disk_is_double_sided
                              : Localization.Disk_is_single_sided);

            sb.AppendLine((rbfSb.dd_fmt & 0x02) == 0x02 ? Localization.Disk_is_double_density
                              : Localization.Disk_is_single_density);

            if((rbfSb.dd_fmt & 0x10) == 0x10)
                sb.AppendLine(Localization.Disk_is_384_TPI);
            else if((rbfSb.dd_fmt & 0x08) == 0x08)
                sb.AppendLine(Localization.Disk_is_192_TPI);
            else if((rbfSb.dd_fmt & 0x04) == 0x04)
                sb.AppendLine(Localization.Disk_is_96_TPI_or_135_TPI);
            else
                sb.AppendLine(Localization.Disk_is_48_TPI);

            sb.AppendFormat(Localization.Allocation_bitmap_descriptor_starts_at_block_0,
                            rbfSb.dd_maplsn == 0 ? 1 : rbfSb.dd_maplsn).AppendLine();

            sb.AppendFormat(Localization._0_bytes_in_allocation_bitmap, rbfSb.dd_map).AppendLine();

            if(LSNToUInt32(rbfSb.dd_bt) > 0 &&
               rbfSb.dd_bsz             > 0)
                sb.AppendFormat(Localization.Boot_file_starts_at_block_0_and_has_1_bytes, LSNToUInt32(rbfSb.dd_bt),
                                rbfSb.dd_bsz).AppendLine();

            sb.AppendFormat(Localization.Root_directory_descriptor_starts_at_block_0, LSNToUInt32(rbfSb.dd_dir)).
               AppendLine();

            sb.AppendFormat(Localization.Disk_is_owned_by_user_0, rbfSb.dd_own).AppendLine();

            sb.AppendFormat(Localization.Volume_was_created_on_0, DateHandlers.Os9ToDateTime(rbfSb.dd_dat)).
               AppendLine();

            sb.AppendFormat(Localization.Volume_attributes_0, rbfSb.dd_att).AppendLine();
            sb.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(rbfSb.dd_nam, Encoding)).AppendLine();

            sb.AppendFormat(Localization.Path_descriptor_options_0, StringHandlers.CToString(rbfSb.dd_opt, Encoding)).
               AppendLine();

            metadata = new FileSystem
            {
                Type         = FS_TYPE,
                Bootable     = LSNToUInt32(rbfSb.dd_bt) > 0 && rbfSb.dd_bsz > 0,
                ClusterSize  = (uint)(rbfSb.dd_bit * (256 << rbfSb.dd_lsnsize)),
                Clusters     = LSNToUInt32(rbfSb.dd_tot),
                CreationDate = DateHandlers.Os9ToDateTime(rbfSb.dd_dat),
                VolumeName   = StringHandlers.CToString(rbfSb.dd_nam, Encoding),
                VolumeSerial = $"{rbfSb.dd_dsk:X4}"
            };
        }

        information = sb.ToString();
    }
}