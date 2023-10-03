// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : F2FS filesystem plugin.
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

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the Flash-Friendly File System (F2FS)</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class F2FS
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(imagePlugin.Info.SectorSize is < F2FS_MIN_SECTOR or > F2FS_MAX_SECTOR)
            return false;

        uint sbAddr = F2FS_SUPER_OFFSET / imagePlugin.Info.SectorSize;

        if(sbAddr == 0)
            sbAddr = 1;

        var sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

        if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
            sbSize++;

        if(partition.Start + sbAddr + sbSize >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        if(sector.Length < Marshal.SizeOf<Superblock>())
            return false;

        Superblock sb = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

        return sb.magic == F2FS_MAGIC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        information = "";
        metadata    = new FileSystem();

        if(imagePlugin.Info.SectorSize is < F2FS_MIN_SECTOR or > F2FS_MAX_SECTOR)
            return;

        uint sbAddr = F2FS_SUPER_OFFSET / imagePlugin.Info.SectorSize;

        if(sbAddr == 0)
            sbAddr = 1;

        var sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

        if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
            sbSize++;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        if(sector.Length < Marshal.SizeOf<Superblock>())
            return;

        // ReSharper disable once InconsistentNaming
        Superblock f2fsSb = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

        if(f2fsSb.magic != F2FS_MAGIC)
            return;

        var sb = new StringBuilder();

        sb.AppendLine(Localization.F2FS_filesystem);
        sb.AppendFormat(Localization.Version_0_1,         f2fsSb.major_ver, f2fsSb.minor_ver).AppendLine();
        sb.AppendFormat(Localization._0_bytes_per_sector, 1 << (int)f2fsSb.log_sectorsize).AppendLine();

        sb.AppendFormat(Localization._0_sectors_1_bytes_per_block, 1 << (int)f2fsSb.log_sectors_per_block,
                        1                                            << (int)f2fsSb.log_blocksize).AppendLine();

        sb.AppendFormat(Localization._0_blocks_per_segment,             f2fsSb.log_blocks_per_seg).AppendLine();
        sb.AppendFormat(Localization._0_blocks_in_volume,               f2fsSb.block_count).AppendLine();
        sb.AppendFormat(Localization._0_segments_per_section,           f2fsSb.segs_per_sec).AppendLine();
        sb.AppendFormat(Localization._0_sections_per_zone,              f2fsSb.secs_per_zone).AppendLine();
        sb.AppendFormat(Localization._0_sections,                       f2fsSb.section_count).AppendLine();
        sb.AppendFormat(Localization._0_segments,                       f2fsSb.segment_count).AppendLine();
        sb.AppendFormat(Localization.Root_directory_resides_on_inode_0, f2fsSb.root_ino).AppendLine();
        sb.AppendFormat(Localization.Volume_UUID_0,                     f2fsSb.uuid).AppendLine();

        sb.AppendFormat(Localization.Volume_name_0,
                        StringHandlers.CToString(f2fsSb.volume_name, Encoding.Unicode, true)).AppendLine();

        sb.AppendFormat(Localization.Volume_last_mounted_on_kernel_version_0, StringHandlers.CToString(f2fsSb.version)).
           AppendLine();

        sb.AppendFormat(Localization.Volume_created_on_kernel_version_0, StringHandlers.CToString(f2fsSb.init_version)).
           AppendLine();

        information = sb.ToString();

        metadata = new FileSystem
        {
            Type                   = FS_TYPE,
            SystemIdentifier       = Encoding.ASCII.GetString(f2fsSb.version),
            Clusters               = f2fsSb.block_count,
            ClusterSize            = (uint)(1 << (int)f2fsSb.log_blocksize),
            DataPreparerIdentifier = Encoding.ASCII.GetString(f2fsSb.init_version),
            VolumeName             = StringHandlers.CToString(f2fsSb.volume_name, Encoding.Unicode, true),
            VolumeSerial           = f2fsSb.uuid.ToString()
        };
    }

#endregion
}