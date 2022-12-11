﻿// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Xia filesystem plugin.
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
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;

namespace Aaru.Filesystems;

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements detection for the Xia filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local"), SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed partial class Xia
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        int  sbSizeInBytes   = Marshal.SizeOf<SuperBlock>();
        uint sbSizeInSectors = (uint)(sbSizeInBytes / imagePlugin.Info.SectorSize);

        if(sbSizeInBytes % imagePlugin.Info.SectorSize > 0)
            sbSizeInSectors++;

        if(sbSizeInSectors + partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, sbSizeInSectors, out byte[] sbSector);

        if(errno != ErrorNumber.NoError)
            return false;

        SuperBlock supblk = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sbSector);

        return supblk.s_magic == XIAFS_SUPER_MAGIC;
    }

    const string FS_TYPE = "xia";

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";

        var sb = new StringBuilder();

        int  sbSizeInBytes   = Marshal.SizeOf<SuperBlock>();
        uint sbSizeInSectors = (uint)(sbSizeInBytes / imagePlugin.Info.SectorSize);

        if(sbSizeInBytes % imagePlugin.Info.SectorSize > 0)
            sbSizeInSectors++;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, sbSizeInSectors, out byte[] sbSector);

        if(errno != ErrorNumber.NoError)
            return;

        SuperBlock supblk = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sbSector);

        sb.AppendFormat(Localization._0_bytes_per_zone, supblk.s_zone_size).AppendLine();

        sb.AppendFormat(Localization._0_zones_in_volume_1_bytes, supblk.s_nzones, supblk.s_nzones * supblk.s_zone_size).
           AppendLine();

        sb.AppendFormat(Localization._0_inodes, supblk.s_ninodes).AppendLine();

        sb.AppendFormat(Localization._0_data_zones_1_bytes, supblk.s_ndatazones,
                        supblk.s_ndatazones * supblk.s_zone_size).AppendLine();

        sb.AppendFormat(Localization._0_imap_zones_1_bytes, supblk.s_imap_zones,
                        supblk.s_imap_zones * supblk.s_zone_size).AppendLine();

        sb.AppendFormat(Localization._0_zmap_zones_1_bytes, supblk.s_zmap_zones,
                        supblk.s_zmap_zones * supblk.s_zone_size).AppendLine();

        sb.AppendFormat(Localization.First_data_zone_0, supblk.s_firstdatazone).AppendLine();

        sb.AppendFormat(Localization.Maximum_filesize_is_0_bytes_1_MiB, supblk.s_max_size, supblk.s_max_size / 1048576).
           AppendLine();

        sb.AppendFormat(Localization._0_zones_reserved_for_kernel_images_1_bytes, supblk.s_kernzones,
                        supblk.s_kernzones * supblk.s_zone_size).AppendLine();

        sb.AppendFormat(Localization.First_kernel_zone_0, supblk.s_firstkernzone).AppendLine();

        XmlFsType = new FileSystemType
        {
            Bootable    = !ArrayHelpers.ArrayIsNullOrEmpty(supblk.s_boot_segment),
            Clusters    = supblk.s_nzones,
            ClusterSize = supblk.s_zone_size,
            Type        = FS_TYPE
        };

        information = sb.ToString();
    }
}