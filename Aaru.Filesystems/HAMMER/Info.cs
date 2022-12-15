// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : HAMMER filesystem plugin.
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
using hammer_crc_t = System.UInt32;
using hammer_off_t = System.UInt64;
using hammer_tid_t = System.UInt64;
using Partition = Aaru.CommonTypes.Partition;

#pragma warning disable 169

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection for the HAMMER filesystem</summary>
public sealed partial class HAMMER
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        uint run = HAMMER_VOLHDR_SIZE / imagePlugin.Info.SectorSize;

        if(HAMMER_VOLHDR_SIZE % imagePlugin.Info.SectorSize > 0)
            run++;

        if(run + partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, run, out byte[] sbSector);

        if(errno != ErrorNumber.NoError)
            return false;

        ulong magic = BitConverter.ToUInt64(sbSector, 0);

        return magic is HAMMER_FSBUF_VOLUME or HAMMER_FSBUF_VOLUME_REV;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";

        var sb = new StringBuilder();

        uint run = HAMMER_VOLHDR_SIZE / imagePlugin.Info.SectorSize;

        if(HAMMER_VOLHDR_SIZE % imagePlugin.Info.SectorSize > 0)
            run++;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, run, out byte[] sbSector);

        if(errno != ErrorNumber.NoError)
            return;

        ulong magic = BitConverter.ToUInt64(sbSector, 0);

        SuperBlock superBlock = magic == HAMMER_FSBUF_VOLUME
                                    ? Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sbSector)
                                    : Marshal.ByteArrayToStructureBigEndian<SuperBlock>(sbSector);

        sb.AppendLine(Localization.HAMMER_filesystem);

        sb.AppendFormat(Localization.Volume_version_0, superBlock.vol_version).AppendLine();

        sb.AppendFormat(Localization.Volume_0_of_1_on_this_filesystem, superBlock.vol_no + 1, superBlock.vol_count).
           AppendLine();

        sb.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(superBlock.vol_label, Encoding)).
           AppendLine();

        sb.AppendFormat(Localization.Volume_serial_0, superBlock.vol_fsid).AppendLine();
        sb.AppendFormat(Localization.Filesystem_type_0, superBlock.vol_fstype).AppendLine();
        sb.AppendFormat(Localization.Boot_area_starts_at_0, superBlock.vol_bot_beg).AppendLine();
        sb.AppendFormat(Localization.Memory_log_starts_at_0, superBlock.vol_mem_beg).AppendLine();
        sb.AppendFormat(Localization.First_volume_buffer_starts_at_0, superBlock.vol_buf_beg).AppendLine();
        sb.AppendFormat(Localization.Volume_ends_at_0, superBlock.vol_buf_end).AppendLine();

        Metadata = new FileSystem
        {
            Clusters     = partition.Size / HAMMER_BIGBLOCK_SIZE,
            ClusterSize  = HAMMER_BIGBLOCK_SIZE,
            Dirty        = false,
            Type         = FS_TYPE,
            VolumeName   = StringHandlers.CToString(superBlock.vol_label, Encoding),
            VolumeSerial = superBlock.vol_fsid.ToString()
        };

        if(superBlock.vol_no == superBlock.vol_rootvol)
        {
            sb.AppendFormat(Localization.Filesystem_contains_0_big_blocks_1_bytes, superBlock.vol0_stat_bigblocks,
                            superBlock.vol0_stat_bigblocks * HAMMER_BIGBLOCK_SIZE).AppendLine();

            sb.AppendFormat(Localization.Filesystem_has_0_big_blocks_free_1_bytes, superBlock.vol0_stat_freebigblocks,
                            superBlock.vol0_stat_freebigblocks * HAMMER_BIGBLOCK_SIZE).AppendLine();

            sb.AppendFormat(Localization.Filesystem_has_0_inodes_used, superBlock.vol0_stat_inodes).AppendLine();

            Metadata.Clusters     = (ulong)superBlock.vol0_stat_bigblocks;
            Metadata.FreeClusters = (ulong)superBlock.vol0_stat_freebigblocks;
            Metadata.Files        = (ulong)superBlock.vol0_stat_inodes;
        }

        // 0 ?
        //sb.AppendFormat("Volume header CRC: 0x{0:X8}", afs_sb.vol_crc).AppendLine();

        information = sb.ToString();
    }
}