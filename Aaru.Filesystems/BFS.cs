// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BFS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : BeOS filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the BeOS filesystem and shows information.
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

// Information from Practical Filesystem Design, ISBN 1-55860-497-9
/// <inheritdoc />
/// <summary>Implements detection of the Be (new) filesystem</summary>
[SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class BeFS : IFilesystem
{
    // Little endian constants (that is, as read by .NET :p)
    const uint BEFS_MAGIC1 = 0x42465331;
    const uint BEFS_MAGIC2 = 0xDD121031;
    const uint BEFS_MAGIC3 = 0x15B6830E;
    const uint BEFS_ENDIAN = 0x42494745;

    // Big endian constants
    const uint BEFS_CIGAM1 = 0x31534642;
    const uint BEFS_NAIDNE = 0x45474942;

    // Common constants
    const uint BEFS_CLEAN = 0x434C454E;
    const uint BEFS_DIRTY = 0x44495254;

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.BeFS_Name;
    /// <inheritdoc />
    public Guid Id => new("dc8572b3-b6ad-46e4-8de9-cbe123ff6672");
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

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
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";

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

        XmlFsType = new FileSystemType
        {
            Clusters              = (ulong)besb.num_blocks,
            ClusterSize           = besb.block_size,
            Dirty                 = besb.flags == BEFS_DIRTY,
            FreeClusters          = (ulong)(besb.num_blocks - besb.used_blocks),
            FreeClustersSpecified = true,
            Type                  = FS_TYPE,
            VolumeName            = StringHandlers.CToString(besb.name, Encoding)
        };
    }

    const string FS_TYPE = "befs";

    /// <summary>Be superblock</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct SuperBlock
    {
        /// <summary>0x000, Volume name, 32 bytes</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly byte[] name;
        /// <summary>0x020, "BFS1", 0x42465331</summary>
        public uint magic1;
        /// <summary>0x024, "BIGE", 0x42494745</summary>
        public readonly uint fs_byte_order;
        /// <summary>0x028, Bytes per block</summary>
        public readonly uint block_size;
        /// <summary>0x02C, 1 &lt;&lt; block_shift == block_size</summary>
        public readonly uint block_shift;
        /// <summary>0x030, Blocks in volume</summary>
        public readonly long num_blocks;
        /// <summary>0x038, Used blocks in volume</summary>
        public readonly long used_blocks;
        /// <summary>0x040, Bytes per inode</summary>
        public readonly int inode_size;
        /// <summary>0x044, 0xDD121031</summary>
        public readonly uint magic2;
        /// <summary>0x048, Blocks per allocation group</summary>
        public readonly int blocks_per_ag;
        /// <summary>0x04C, 1 &lt;&lt; ag_shift == blocks_per_ag</summary>
        public readonly int ag_shift;
        /// <summary>0x050, Allocation groups in volume</summary>
        public readonly int num_ags;
        /// <summary>0x054, 0x434c454e if clean, 0x44495254 if dirty</summary>
        public readonly uint flags;
        /// <summary>0x058, Allocation group of journal</summary>
        public readonly int log_blocks_ag;
        /// <summary>0x05C, Start block of journal, inside ag</summary>
        public readonly ushort log_blocks_start;
        /// <summary>0x05E, Length in blocks of journal, inside ag</summary>
        public readonly ushort log_blocks_len;
        /// <summary>0x060, Start of journal</summary>
        public readonly long log_start;
        /// <summary>0x068, End of journal</summary>
        public readonly long log_end;
        /// <summary>0x070, 0x15B6830E</summary>
        public readonly uint magic3;
        /// <summary>0x074, Allocation group where root folder's i-node resides</summary>
        public readonly int root_dir_ag;
        /// <summary>0x078, Start in ag of root folder's i-node</summary>
        public readonly ushort root_dir_start;
        /// <summary>0x07A, As this is part of inode_addr, this is 1</summary>
        public readonly ushort root_dir_len;
        /// <summary>0x07C, Allocation group where indices' i-node resides</summary>
        public readonly int indices_ag;
        /// <summary>0x080, Start in ag of indices' i-node</summary>
        public readonly ushort indices_start;
        /// <summary>0x082, As this is part of inode_addr, this is 1</summary>
        public readonly ushort indices_len;
    }
}