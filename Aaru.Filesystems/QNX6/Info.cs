// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : QNX6 filesystem plugin.
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
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of QNX 6 filesystem</summary>
public sealed partial class QNX6
{
#region IFilesystem Members

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        uint sectors     = QNX6_SUPER_BLOCK_SIZE / imagePlugin.Info.SectorSize;
        uint bootSectors = QNX6_BOOT_BLOCKS_SIZE / imagePlugin.Info.SectorSize;

        if(partition.Start + bootSectors + sectors >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, sectors, out byte[] audiSector);

        if(errno != ErrorNumber.NoError)
            return false;

        errno = imagePlugin.ReadSectors(partition.Start + bootSectors, sectors, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        if(sector.Length < QNX6_SUPER_BLOCK_SIZE)
            return false;

        AudiSuperBlock audiSb = Marshal.ByteArrayToStructureLittleEndian<AudiSuperBlock>(audiSector);

        SuperBlock qnxSb = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sector);

        return qnxSb.magic == QNX6_MAGIC || audiSb.magic == QNX6_MAGIC;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        information = "";
        metadata    = new FileSystem();
        var  sb          = new StringBuilder();
        uint sectors     = QNX6_SUPER_BLOCK_SIZE / imagePlugin.Info.SectorSize;
        uint bootSectors = QNX6_BOOT_BLOCKS_SIZE / imagePlugin.Info.SectorSize;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, sectors, out byte[] audiSector);

        if(errno != ErrorNumber.NoError)
            return;

        errno = imagePlugin.ReadSectors(partition.Start + bootSectors, sectors, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        if(sector.Length < QNX6_SUPER_BLOCK_SIZE)
            return;

        AudiSuperBlock audiSb = Marshal.ByteArrayToStructureLittleEndian<AudiSuperBlock>(audiSector);

        SuperBlock qnxSb = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sector);

        bool audi = audiSb.magic == QNX6_MAGIC;

        if(audi)
        {
            sb.AppendLine(Localization.QNX6_Audi_filesystem);
            sb.AppendFormat(Localization.Checksum_0_X8,       audiSb.checksum).AppendLine();
            sb.AppendFormat(Localization.Serial_0_X16,        audiSb.checksum).AppendLine();
            sb.AppendFormat(Localization._0_bytes_per_block,  audiSb.blockSize).AppendLine();
            sb.AppendFormat(Localization._0_inodes_free_of_1, audiSb.freeInodes, audiSb.numInodes).AppendLine();

            sb.AppendFormat(Localization._0_blocks_1_bytes_free_of_2_3_bytes, audiSb.freeBlocks,
                            audiSb.freeBlocks * audiSb.blockSize, audiSb.numBlocks,
                            audiSb.numBlocks  * audiSb.blockSize).AppendLine();

            metadata = new FileSystem
            {
                Type         = FS_TYPE,
                Clusters     = audiSb.numBlocks,
                ClusterSize  = audiSb.blockSize,
                Bootable     = true,
                Files        = audiSb.numInodes - audiSb.freeInodes,
                FreeClusters = audiSb.freeBlocks,
                VolumeSerial = $"{audiSb.serial:X16}"
            };

            //xmlFSType.VolumeName = CurrentEncoding.GetString(audiSb.id);

            information = sb.ToString();

            return;
        }

        sb.AppendLine(Localization.QNX6_filesystem);
        sb.AppendFormat(Localization.Checksum_0_X8,     qnxSb.checksum).AppendLine();
        sb.AppendFormat(Localization.Serial_0_X16,      qnxSb.checksum).AppendLine();
        sb.AppendFormat(Localization.Created_on_0,      DateHandlers.UnixUnsignedToDateTime(qnxSb.ctime)).AppendLine();
        sb.AppendFormat(Localization.Last_mounted_on_0, DateHandlers.UnixUnsignedToDateTime(qnxSb.atime)).AppendLine();
        sb.AppendFormat(Localization.Flags_0_X8,        qnxSb.flags).AppendLine();
        sb.AppendFormat(Localization.Version1_0_X4,     qnxSb.version1).AppendLine();
        sb.AppendFormat(Localization.Version2_0_X4,     qnxSb.version2).AppendLine();

        //sb.AppendFormat("Volume ID: \"{0}\"", CurrentEncoding.GetString(qnxSb.volumeid)).AppendLine();
        sb.AppendFormat(Localization._0_bytes_per_block,  qnxSb.blockSize).AppendLine();
        sb.AppendFormat(Localization._0_inodes_free_of_1, qnxSb.freeInodes, qnxSb.numInodes).AppendLine();

        sb.AppendFormat(Localization._0_blocks_1_bytes_free_of_2_3_bytes, qnxSb.freeBlocks,
                        qnxSb.freeBlocks * qnxSb.blockSize, qnxSb.numBlocks, qnxSb.numBlocks * qnxSb.blockSize).
           AppendLine();

        metadata = new FileSystem
        {
            Type             = FS_TYPE,
            Clusters         = qnxSb.numBlocks,
            ClusterSize      = qnxSb.blockSize,
            Bootable         = true,
            Files            = qnxSb.numInodes - qnxSb.freeInodes,
            FreeClusters     = qnxSb.freeBlocks,
            VolumeSerial     = $"{qnxSb.serial:X16}",
            CreationDate     = DateHandlers.UnixUnsignedToDateTime(qnxSb.ctime),
            ModificationDate = DateHandlers.UnixUnsignedToDateTime(qnxSb.atime)
        };

        //xmlFSType.VolumeName = CurrentEncoding.GetString(qnxSb.volumeid);

        information = sb.ToString();
    }

#endregion
}