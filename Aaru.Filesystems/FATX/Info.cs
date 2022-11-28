// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FATX filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the FATX filesystem and shows information.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;

namespace Aaru.Filesystems;

public sealed partial class XboxFatPlugin
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(imagePlugin.Info.SectorSize < 512)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        Superblock sb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);

        return sb.magic is FATX_MAGIC or FATX_CIGAM;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = Encoding.UTF8;
        information = "";

        if(imagePlugin.Info.SectorSize < 512)
            return;

        bool bigEndian = true;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        Superblock fatxSb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);

        if(fatxSb.magic == FATX_CIGAM)
        {
            fatxSb    = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);
            bigEndian = false;
        }

        if(fatxSb.magic != FATX_MAGIC)
            return;

        int logicalSectorsPerPhysicalSectors = partition.Offset == 0 && !bigEndian ? 8 : 1;

        var sb = new StringBuilder();

        sb.AppendLine(Localization.FATX_filesystem);

        sb.AppendFormat(Localization._0_logical_sectors_1_bytes_per_physical_sector, logicalSectorsPerPhysicalSectors,
                        logicalSectorsPerPhysicalSectors * imagePlugin.Info.SectorSize).AppendLine();

        sb.AppendFormat(Localization._0_sectors_1_bytes_per_cluster, fatxSb.sectorsPerCluster,
                        fatxSb.sectorsPerCluster * logicalSectorsPerPhysicalSectors * imagePlugin.Info.SectorSize).
           AppendLine();

        sb.AppendFormat(Localization.Root_directory_starts_on_cluster_0, fatxSb.rootDirectoryCluster).AppendLine();

        string volumeLabel = StringHandlers.CToString(fatxSb.volumeLabel,
                                                      bigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode, true);

        sb.AppendFormat(Localization.Volume_label_0, volumeLabel).AppendLine();
        sb.AppendFormat(Localization.Volume_serial_0_X8, fatxSb.id).AppendLine();

        information = sb.ToString();

        XmlFsType = new FileSystemType
        {
            Type = FS_TYPE,
            ClusterSize = (uint)(fatxSb.sectorsPerCluster * logicalSectorsPerPhysicalSectors *
                                 imagePlugin.Info.SectorSize),
            VolumeName   = volumeLabel,
            VolumeSerial = $"{fatxSb.id:X8}"
        };

        XmlFsType.Clusters = (partition.End - partition.Start + 1) * imagePlugin.Info.SectorSize /
                             XmlFsType.ClusterSize;
    }
}