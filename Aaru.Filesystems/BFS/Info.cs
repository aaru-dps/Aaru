// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : BeOS filesystem plugin.
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

// Information from Practical Filesystem Design, ISBN 1-55860-497-9
/// <inheritdoc />
/// <summary>Implements detection of the Be (new) filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed partial class BeFS
{
    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(2 + partition.Start >= partition.End)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] sbSector);

        if(errno != ErrorNumber.NoError)
            return false;

        uint magic   = BitConverter.ToUInt32(sbSector, 0x20);
        uint magicBe = BigEndianBitConverter.ToUInt32(sbSector, 0x20);

        if(magic   == BEFS_MAGIC1 ||
           magicBe == BEFS_MAGIC1)
            return true;

        if(sbSector.Length >= 0x400)
        {
            magic   = BitConverter.ToUInt32(sbSector, 0x220);
            magicBe = BigEndianBitConverter.ToUInt32(sbSector, 0x220);
        }

        if(magic   == BEFS_MAGIC1 ||
           magicBe == BEFS_MAGIC1)
            return true;

        errno = imagePlugin.ReadSector(1 + partition.Start, out sbSector);

        if(errno != ErrorNumber.NoError)
            return false;

        magic   = BitConverter.ToUInt32(sbSector, 0x20);
        magicBe = BigEndianBitConverter.ToUInt32(sbSector, 0x20);

        return magic == BEFS_MAGIC1 || magicBe == BEFS_MAGIC1;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, Encoding encoding, out string information,
                               out FileSystem metadata)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";
        metadata    = new FileSystem();

        var sb = new StringBuilder();

        var besb = new SuperBlock();

        ErrorNumber errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] sbSector);

        if(errno != ErrorNumber.NoError)
            return;

        bool littleEndian;

        besb.magic1 = BigEndianBitConverter.ToUInt32(sbSector, 0x20);

        if(besb.magic1 is BEFS_MAGIC1 or BEFS_CIGAM1) // Magic is at offset
            littleEndian = besb.magic1 == BEFS_CIGAM1;
        else
        {
            errno = imagePlugin.ReadSector(1 + partition.Start, out sbSector);

            if(errno != ErrorNumber.NoError)
                return;

            besb.magic1 = BigEndianBitConverter.ToUInt32(sbSector, 0x20);

            if(besb.magic1 is BEFS_MAGIC1 or BEFS_CIGAM1) // There is a boot sector
                littleEndian = besb.magic1 == BEFS_CIGAM1;
            else if(sbSector.Length >= 0x400)
            {
                errno = imagePlugin.ReadSector(0 + partition.Start, out byte[] temp);

                if(errno != ErrorNumber.NoError)
                    return;

                besb.magic1 = BigEndianBitConverter.ToUInt32(temp, 0x220);

                if(besb.magic1 is BEFS_MAGIC1 or BEFS_CIGAM1) // There is a boot sector
                {
                    littleEndian = besb.magic1 == BEFS_CIGAM1;
                    sbSector     = new byte[0x200];
                    Array.Copy(temp, 0x200, sbSector, 0, 0x200);
                }
                else
                    return;
            }
            else
                return;
        }

        besb = littleEndian ? Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sbSector)
                   : Marshal.ByteArrayToStructureBigEndian<SuperBlock>(sbSector);

        sb.AppendLine(littleEndian ? Localization.Little_endian_BeFS : Localization.Big_endian_BeFS);

        if(besb.magic1                != BEFS_MAGIC1 ||
           besb.fs_byte_order         != BEFS_ENDIAN ||
           besb.magic2                != BEFS_MAGIC2 ||
           besb.magic3                != BEFS_MAGIC3 ||
           besb.root_dir_len          != 1           ||
           besb.indices_len           != 1           ||
           1 << (int)besb.block_shift != besb.block_size)
        {
            sb.AppendLine(Localization.Superblock_seems_corrupt_following_information_may_be_incorrect);
            sb.AppendFormat(Localization.Magic_one_0_Should_be_0x42465331, besb.magic1).AppendLine();
            sb.AppendFormat(Localization.Magic_two_0_Should_be_0xDD121031, besb.magic2).AppendLine();
            sb.AppendFormat(Localization.Magic_three_0_Should_be_0x15B6830E, besb.magic3).AppendLine();

            sb.AppendFormat(Localization.Filesystem_endianness_0_Should_be_0x42494745, besb.fs_byte_order).AppendLine();

            sb.AppendFormat(Localization.Root_folder_i_node_size_0_blocks_Should_be_one, besb.root_dir_len).
               AppendLine();

            sb.AppendFormat(Localization.Indices_i_node_size_0_blocks_Should_be_one, besb.indices_len).AppendLine();

            sb.AppendFormat(Localization.blockshift_0_1_should_be_2, besb.block_shift, 1 << (int)besb.block_shift,
                            besb.block_size).AppendLine();
        }

        switch(besb.flags)
        {
            case BEFS_CLEAN:
                sb.AppendLine(besb.log_start == besb.log_end ? Localization.Filesystem_is_clean
                                  : Localization.Filesystem_is_dirty);

                break;
            case BEFS_DIRTY:
                sb.AppendLine(Localization.Filesystem_is_dirty);

                break;
            default:
                sb.AppendFormat(Localization.Unknown_flags_0_X8, besb.flags).AppendLine();

                break;
        }

        sb.AppendFormat(Localization.Volume_name_0, StringHandlers.CToString(besb.name, Encoding)).AppendLine();
        sb.AppendFormat(Localization._0_bytes_per_block, besb.block_size).AppendLine();

        sb.AppendFormat(Localization._0_blocks_in_volume_1_bytes, besb.num_blocks, besb.num_blocks * besb.block_size).
           AppendLine();

        sb.AppendFormat(Localization._0_used_blocks_1_bytes, besb.used_blocks, besb.used_blocks * besb.block_size).
           AppendLine();

        sb.AppendFormat(Localization._0_bytes_per_i_node, besb.inode_size).AppendLine();

        sb.AppendFormat(Localization._0_blocks_per_allocation_group_1_bytes, besb.blocks_per_ag,
                        besb.blocks_per_ag * besb.block_size).AppendLine();

        sb.AppendFormat(Localization._0_allocation_groups_in_volume, besb.num_ags).AppendLine();

        sb.AppendFormat(Localization.Journal_resides_in_block_0_of_allocation_group_1_and_runs_for_2_blocks_3_bytes,
                        besb.log_blocks_start, besb.log_blocks_ag, besb.log_blocks_len,
                        besb.log_blocks_len * besb.block_size).AppendLine();

        sb.AppendFormat(Localization.Journal_starts_in_byte_0_and_ends_in_byte_1, besb.log_start, besb.log_end).
           AppendLine();

        sb.
            AppendFormat(Localization.Root_folder_s_i_node_resides_in_block_0_of_allocation_group_1_and_runs_for_2_blocks_3_bytes,
                         besb.root_dir_start, besb.root_dir_ag, besb.root_dir_len, besb.root_dir_len * besb.block_size).
            AppendLine();

        sb.
            AppendFormat(Localization.Indices_i_node_resides_in_block_0_of_allocation_group_1_and_runs_for_2_blocks_3_bytes,
                         besb.indices_start, besb.indices_ag, besb.indices_len, besb.indices_len * besb.block_size).
            AppendLine();

        information = sb.ToString();

        metadata = new FileSystem
        {
            Clusters     = (ulong)besb.num_blocks,
            ClusterSize  = besb.block_size,
            Dirty        = besb.flags == BEFS_DIRTY,
            FreeClusters = (ulong)(besb.num_blocks - besb.used_blocks),
            Type         = FS_TYPE,
            VolumeName   = StringHandlers.CToString(besb.name, Encoding)
        };
    }
}