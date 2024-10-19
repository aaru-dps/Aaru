// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MINIX filesystem plugin.
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
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

// Information from the Linux kernel
/// <inheritdoc />
/// <summary>Implements detection of the MINIX filesystem</summary>
public sealed partial class MinixFS
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        uint sector = 2;
        uint offset = 0;

        if(imagePlugin.Info.MetadataMediaType == MetadataMediaType.OpticalDisc)
        {
            sector = 0;
            offset = 0x400;
        }

        if(sector + partition.Start >= partition.End) return false;

        ErrorNumber errno = imagePlugin.ReadSector(sector + partition.Start, out byte[] minixSbSector);

        if(errno != ErrorNumber.NoError) return false;

        // Optical media
        if(offset > 0)
        {
            var tmp = new byte[0x200];
            Array.Copy(minixSbSector, offset, tmp, 0, 0x200);
            minixSbSector = tmp;
        }

        var magic = BitConverter.ToUInt16(minixSbSector, 0x010);

        if(magic is MINIX_MAGIC
                 or MINIX_MAGIC2
                 or MINIX2_MAGIC
                 or MINIX2_MAGIC2
                 or MINIX_CIGAM
                 or MINIX_CIGAM2
                 or MINIX2_CIGAM
                 or MINIX2_CIGAM2)
            return true;

        magic = BitConverter.ToUInt16(minixSbSector, 0x018); // Here should reside magic number on Minix v3

        return magic is MINIX_MAGIC or MINIX2_MAGIC or MINIX3_MAGIC or MINIX_CIGAM or MINIX2_CIGAM or MINIX3_CIGAM;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        information = "";
        metadata    = new FileSystem();

        var sb = new StringBuilder();

        uint sector = 2;
        uint offset = 0;

        if(imagePlugin.Info.MetadataMediaType == MetadataMediaType.OpticalDisc)
        {
            sector = 0;
            offset = 0x400;
        }

        var         minix3 = false;
        int         filenamesize;
        string      minixVersion;
        ErrorNumber errno = imagePlugin.ReadSector(sector + partition.Start, out byte[] minixSbSector);

        if(errno != ErrorNumber.NoError) return;

        // Optical media
        if(offset > 0)
        {
            var tmp = new byte[0x200];
            Array.Copy(minixSbSector, offset, tmp, 0, 0x200);
            minixSbSector = tmp;
        }

        var magic = BitConverter.ToUInt16(minixSbSector, 0x018);

        metadata = new FileSystem();

        bool littleEndian;

        if(magic is MINIX3_MAGIC or MINIX3_CIGAM or MINIX2_MAGIC or MINIX2_CIGAM or MINIX_MAGIC or MINIX_CIGAM)
        {
            filenamesize = 60;
            littleEndian = magic is not (MINIX3_CIGAM or MINIX2_CIGAM or MINIX_CIGAM);

            switch(magic)
            {
                case MINIX3_MAGIC:
                case MINIX3_CIGAM:
                    minixVersion  = Localization.Minix_v3_filesystem;
                    metadata.Type = FS_TYPE_V3;

                    break;
                case MINIX2_MAGIC:
                case MINIX2_CIGAM:
                    minixVersion  = Localization.Minix_3_v2_filesystem;
                    metadata.Type = FS_TYPE_V3;

                    break;
                default:
                    minixVersion  = Localization.Minix_3_v1_filesystem;
                    metadata.Type = FS_TYPE_V3;

                    break;
            }

            minix3 = true;
        }
        else
        {
            magic = BitConverter.ToUInt16(minixSbSector, 0x010);

            switch(magic)
            {
                case MINIX_MAGIC:
                    filenamesize  = 14;
                    minixVersion  = Localization.Minix_v1_filesystem;
                    littleEndian  = true;
                    metadata.Type = FS_TYPE_V1;

                    break;
                case MINIX_MAGIC2:
                    filenamesize  = 30;
                    minixVersion  = Localization.Minix_v1_filesystem;
                    littleEndian  = true;
                    metadata.Type = FS_TYPE_V1;

                    break;
                case MINIX2_MAGIC:
                    filenamesize  = 14;
                    minixVersion  = Localization.Minix_v2_filesystem;
                    littleEndian  = true;
                    metadata.Type = FS_TYPE_V2;

                    break;
                case MINIX2_MAGIC2:
                    filenamesize  = 30;
                    minixVersion  = Localization.Minix_v2_filesystem;
                    littleEndian  = true;
                    metadata.Type = FS_TYPE_V2;

                    break;
                case MINIX_CIGAM:
                    filenamesize  = 14;
                    minixVersion  = Localization.Minix_v1_filesystem;
                    littleEndian  = false;
                    metadata.Type = FS_TYPE_V1;

                    break;
                case MINIX_CIGAM2:
                    filenamesize  = 30;
                    minixVersion  = Localization.Minix_v1_filesystem;
                    littleEndian  = false;
                    metadata.Type = FS_TYPE_V1;

                    break;
                case MINIX2_CIGAM:
                    filenamesize  = 14;
                    minixVersion  = Localization.Minix_v2_filesystem;
                    littleEndian  = false;
                    metadata.Type = FS_TYPE_V2;

                    break;
                case MINIX2_CIGAM2:
                    filenamesize  = 30;
                    minixVersion  = Localization.Minix_v2_filesystem;
                    littleEndian  = false;
                    metadata.Type = FS_TYPE_V2;

                    break;
                default:
                    return;
            }
        }

        if(minix3)
        {
            SuperBlock3 mnxSb = littleEndian
                                    ? Marshal.ByteArrayToStructureLittleEndian<SuperBlock3>(minixSbSector)
                                    : Marshal.ByteArrayToStructureBigEndian<SuperBlock3>(minixSbSector);

            if(magic != MINIX3_MAGIC && magic != MINIX3_CIGAM) mnxSb.s_blocksize = 1024;

            sb.AppendLine(minixVersion);
            sb.AppendFormat(Localization._0_chars_in_filename, filenamesize).AppendLine();

            if(mnxSb.s_zones > 0) // On V2
            {
                sb.AppendFormat(Localization._0_zones_in_volume_1_bytes, mnxSb.s_zones, mnxSb.s_zones * 1024)
                  .AppendLine();
            }
            else
            {
                sb.AppendFormat(Localization._0_zones_in_volume_1_bytes, mnxSb.s_nzones, mnxSb.s_nzones * 1024)
                  .AppendLine();
            }

            sb.AppendFormat(Localization._0_bytes_block,      mnxSb.s_blocksize).AppendLine();
            sb.AppendFormat(Localization._0_inodes_in_volume, mnxSb.s_ninodes).AppendLine();

            sb.AppendFormat(Localization._0_blocks_on_inode_map_1_bytes,
                            mnxSb.s_imap_blocks,
                            mnxSb.s_imap_blocks * mnxSb.s_blocksize)
              .AppendLine();

            sb.AppendFormat(Localization._0_blocks_on_zone_map_1_bytes,
                            mnxSb.s_zmap_blocks,
                            mnxSb.s_zmap_blocks * mnxSb.s_blocksize)
              .AppendLine();

            sb.AppendFormat(Localization.First_data_zone_0, mnxSb.s_firstdatazone).AppendLine();

            //sb.AppendFormat("log2 of blocks/zone: {0}", mnx_sb.s_log_zone_size).AppendLine(); // Apparently 0
            sb.AppendFormat(Localization._0_bytes_maximum_per_file,    mnxSb.s_max_size).AppendLine();
            sb.AppendFormat(Localization.On_disk_filesystem_version_0, mnxSb.s_disk_version).AppendLine();

            metadata.ClusterSize = mnxSb.s_blocksize;
            metadata.Clusters    = mnxSb.s_zones > 0 ? mnxSb.s_zones : mnxSb.s_nzones;
        }
        else
        {
            SuperBlock mnxSb = littleEndian
                                   ? Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(minixSbSector)
                                   : Marshal.ByteArrayToStructureBigEndian<SuperBlock>(minixSbSector);

            sb.AppendLine(minixVersion);
            sb.AppendFormat(Localization._0_chars_in_filename, filenamesize).AppendLine();

            if(mnxSb.s_zones > 0) // On V2
            {
                sb.AppendFormat(Localization._0_zones_in_volume_1_bytes, mnxSb.s_zones, mnxSb.s_zones * 1024)
                  .AppendLine();
            }
            else
            {
                sb.AppendFormat(Localization._0_zones_in_volume_1_bytes, mnxSb.s_nzones, mnxSb.s_nzones * 1024)
                  .AppendLine();
            }

            sb.AppendFormat(Localization._0_inodes_in_volume, mnxSb.s_ninodes).AppendLine();

            sb.AppendFormat(Localization._0_blocks_on_inode_map_1_bytes,
                            mnxSb.s_imap_blocks,
                            mnxSb.s_imap_blocks * 1024)
              .AppendLine();

            sb.AppendFormat(Localization._0_blocks_on_zone_map_1_bytes, mnxSb.s_zmap_blocks, mnxSb.s_zmap_blocks * 1024)
              .AppendLine();

            sb.AppendFormat(Localization.First_data_zone_0, mnxSb.s_firstdatazone).AppendLine();

            //sb.AppendFormat("log2 of blocks/zone: {0}", mnx_sb.s_log_zone_size).AppendLine(); // Apparently 0
            sb.AppendFormat(Localization._0_bytes_maximum_per_file, mnxSb.s_max_size).AppendLine();
            sb.AppendFormat(Localization.Filesystem_state_0,        mnxSb.s_state).AppendLine();
            metadata.ClusterSize = 1024;
            metadata.Clusters    = mnxSb.s_zones > 0 ? mnxSb.s_zones : mnxSb.s_nzones;
        }

        information = sb.ToString();
    }

#endregion
}