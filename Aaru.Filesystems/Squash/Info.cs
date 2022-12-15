// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Squash file system plugin.
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
using System.Text;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of the squash filesystem</summary>
public sealed partial class Squash
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        uint magic = BitConverter.ToUInt32(sector, 0x00);

        return magic is SQUASH_MAGIC or SQUASH_CIGAM;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.UTF8;
        information = "";
        ErrorNumber errno = imagePlugin.ReadSector(partition.Start, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        uint magic = BitConverter.ToUInt32(sector, 0x00);

        var  sqSb         = new SuperBlock();
        bool littleEndian = true;

        switch(magic)
        {
            case SQUASH_MAGIC:
                sqSb = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sector);

                break;
            case SQUASH_CIGAM:
                sqSb         = Marshal.ByteArrayToStructureBigEndian<SuperBlock>(sector);
                littleEndian = false;

                break;
        }

        var sbInformation = new StringBuilder();

        sbInformation.AppendLine(Localization.Squash_file_system);
        sbInformation.AppendLine(littleEndian ? Localization.Little_endian : Localization.Big_endian);
        sbInformation.AppendFormat(Localization.Volume_version_0_1, sqSb.s_major, sqSb.s_minor).AppendLine();
        sbInformation.AppendFormat(Localization.Volume_has_0_bytes, sqSb.bytes_used).AppendLine();
        sbInformation.AppendFormat(Localization.Volume_has_0_bytes_per_block, sqSb.block_size).AppendLine();

        sbInformation.
            AppendFormat(Localization.Volume_created_on_0, DateHandlers.UnixUnsignedToDateTime(sqSb.mkfs_time)).
            AppendLine();

        sbInformation.AppendFormat(Localization.Volume_has_0_inodes, sqSb.inodes).AppendLine();

        switch(sqSb.compression)
        {
            case (ushort)SquashCompression.Lz4:
                sbInformation.AppendLine(Localization.Volume_is_compressed_using_LZ4);

                break;
            case (ushort)SquashCompression.Lzo:
                sbInformation.AppendLine(Localization.Volume_is_compressed_using_LZO);

                break;
            case (ushort)SquashCompression.Lzma:
                sbInformation.AppendLine(Localization.Volume_is_compressed_using_LZMA);

                break;
            case (ushort)SquashCompression.Xz:
                sbInformation.AppendLine(Localization.Volume_is_compressed_using_XZ);

                break;
            case (ushort)SquashCompression.Zlib:
                sbInformation.AppendLine(Localization.Volume_is_compressed_using_GZIP);

                break;
            case (ushort)SquashCompression.Zstd:
                sbInformation.AppendLine(Localization.Volume_is_compressed_using_Zstandard);

                break;
            default:
                sbInformation.
                    AppendFormat(Localization.Volume_is_compressed_using_unknown_algorithm_0, sqSb.compression).
                    AppendLine();

                break;
        }

        information = sbInformation.ToString();

        Metadata = new FileSystem
        {
            Type         = FS_TYPE,
            CreationDate = DateHandlers.UnixUnsignedToDateTime(sqSb.mkfs_time),
            Clusters     = (partition.End - partition.Start + 1) * imagePlugin.Info.SectorSize / sqSb.block_size,
            ClusterSize  = sqSb.block_size,
            Files        = sqSb.inodes,
            FreeClusters = 0
        };
    }
}