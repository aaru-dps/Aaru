// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Atheos.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Atheos filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Atheos filesystem and shows information.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems
{
    /// <summary>
    /// Implements detection for the AtheOS filesystem
    /// </summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public sealed class AtheOS : IFilesystem
    {
        // Little endian constants (that is, as read by .NET :p)
        const uint AFS_MAGIC1 = 0x41465331;
        const uint AFS_MAGIC2 = 0xDD121031;
        const uint AFS_MAGIC3 = 0x15B6830E;

        // Common constants
        const uint AFS_SUPERBLOCK_SIZE = 1024;
        const uint AFS_BOOTBLOCK_SIZE  = AFS_SUPERBLOCK_SIZE;

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding       Encoding  { get; private set; }
        /// <inheritdoc />
        public string         Name      => "AtheOS Filesystem";
        /// <inheritdoc />
        public Guid           Id        => new Guid("AAB2C4F1-DC07-49EE-A948-576CC51B58C5");
        /// <inheritdoc />
        public string         Author    => "Natalia Portillo";

        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            ulong sector = AFS_BOOTBLOCK_SIZE / imagePlugin.Info.SectorSize;
            uint  offset = AFS_BOOTBLOCK_SIZE % imagePlugin.Info.SectorSize;
            uint  run    = 1;

            if(imagePlugin.Info.SectorSize < AFS_SUPERBLOCK_SIZE)
                run = AFS_SUPERBLOCK_SIZE / imagePlugin.Info.SectorSize;

            if(sector + partition.Start >= partition.End)
                return false;

            byte[] tmp      = imagePlugin.ReadSectors(sector + partition.Start, run);
            byte[] sbSector = new byte[AFS_SUPERBLOCK_SIZE];

            if(offset + AFS_SUPERBLOCK_SIZE > tmp.Length)
                return false;

            Array.Copy(tmp, offset, sbSector, 0, AFS_SUPERBLOCK_SIZE);

            uint magic = BitConverter.ToUInt32(sbSector, 0x20);

            return magic == AFS_MAGIC1;
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";

            var sb = new StringBuilder();

            ulong sector = AFS_BOOTBLOCK_SIZE / imagePlugin.Info.SectorSize;
            uint  offset = AFS_BOOTBLOCK_SIZE % imagePlugin.Info.SectorSize;
            uint  run    = 1;

            if(imagePlugin.Info.SectorSize < AFS_SUPERBLOCK_SIZE)
                run = AFS_SUPERBLOCK_SIZE / imagePlugin.Info.SectorSize;

            byte[] tmp      = imagePlugin.ReadSectors(sector + partition.Start, run);
            byte[] sbSector = new byte[AFS_SUPERBLOCK_SIZE];
            Array.Copy(tmp, offset, sbSector, 0, AFS_SUPERBLOCK_SIZE);

            SuperBlock afsSb = Marshal.ByteArrayToStructureLittleEndian<SuperBlock>(sbSector);

            sb.AppendLine("Atheos filesystem");

            if(afsSb.flags == 1)
                sb.AppendLine("Filesystem is read-only");

            sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(afsSb.name, Encoding)).AppendLine();
            sb.AppendFormat("{0} bytes per block", afsSb.block_size).AppendLine();

            sb.AppendFormat("{0} blocks in volume ({1} bytes)", afsSb.num_blocks, afsSb.num_blocks * afsSb.block_size).
               AppendLine();

            sb.AppendFormat("{0} used blocks ({1} bytes)", afsSb.used_blocks, afsSb.used_blocks * afsSb.block_size).
               AppendLine();

            sb.AppendFormat("{0} bytes per i-node", afsSb.inode_size).AppendLine();

            sb.AppendFormat("{0} blocks per allocation group ({1} bytes)", afsSb.blocks_per_ag,
                            afsSb.blocks_per_ag * afsSb.block_size).AppendLine();

            sb.AppendFormat("{0} allocation groups in volume", afsSb.num_ags).AppendLine();

            sb.AppendFormat("Journal resides in block {0} of allocation group {1} and runs for {2} blocks ({3} bytes)",
                            afsSb.log_blocks_start, afsSb.log_blocks_ag, afsSb.log_blocks_len,
                            afsSb.log_blocks_len * afsSb.block_size).AppendLine();

            sb.AppendFormat("Journal starts in byte {0} and has {1} bytes in {2} blocks", afsSb.log_start,
                            afsSb.log_size, afsSb.log_valid_blocks).AppendLine();

            sb.
                AppendFormat("Root folder's i-node resides in block {0} of allocation group {1} and runs for {2} blocks ({3} bytes)",
                             afsSb.root_dir_start, afsSb.root_dir_ag, afsSb.root_dir_len,
                             afsSb.root_dir_len * afsSb.block_size).AppendLine();

            sb.
                AppendFormat("Directory containing files scheduled for deletion's i-node resides in block {0} of allocation group {1} and runs for {2} blocks ({3} bytes)",
                             afsSb.deleted_start, afsSb.deleted_ag, afsSb.deleted_len,
                             afsSb.deleted_len * afsSb.block_size).AppendLine();

            sb.
                AppendFormat("Indices' i-node resides in block {0} of allocation group {1} and runs for {2} blocks ({3} bytes)",
                             afsSb.indices_start, afsSb.indices_ag, afsSb.indices_len,
                             afsSb.indices_len * afsSb.block_size).AppendLine();

            sb.AppendFormat("{0} blocks for bootloader ({1} bytes)", afsSb.boot_size,
                            afsSb.boot_size * afsSb.block_size).AppendLine();

            information = sb.ToString();

            XmlFsType = new FileSystemType
            {
                Clusters              = (ulong)afsSb.num_blocks,
                ClusterSize           = afsSb.block_size,
                Dirty                 = false,
                FreeClusters          = (ulong)(afsSb.num_blocks - afsSb.used_blocks),
                FreeClustersSpecified = true,
                Type                  = "AtheOS filesystem",
                VolumeName            = StringHandlers.CToString(afsSb.name, Encoding)
            };
        }

        /// <summary>Be superblock</summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct SuperBlock
        {
            /// <summary>0x000, Volume name, 32 bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public readonly byte[] name;
            /// <summary>0x020, "AFS1", 0x41465331</summary>
            public readonly uint magic1;
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
            /// <summary>0x068, Valid block logs</summary>
            public readonly int log_valid_blocks;
            /// <summary>0x06C, Log size</summary>
            public readonly int log_size;
            /// <summary>0x070, 0x15B6830E</summary>
            public readonly uint magic3;
            /// <summary>0x074, Allocation group where root folder's i-node resides</summary>
            public readonly int root_dir_ag;
            /// <summary>0x078, Start in ag of root folder's i-node</summary>
            public readonly ushort root_dir_start;
            /// <summary>0x07A, As this is part of inode_addr, this is 1</summary>
            public readonly ushort root_dir_len;
            /// <summary>0x07C, Allocation group where pending-delete-files' i-node resides</summary>
            public readonly int deleted_ag;
            /// <summary>0x080, Start in ag of pending-delete-files' i-node</summary>
            public readonly ushort deleted_start;
            /// <summary>0x082, As this is part of inode_addr, this is 1</summary>
            public readonly ushort deleted_len;
            /// <summary>0x084, Allocation group where indices' i-node resides</summary>
            public readonly int indices_ag;
            /// <summary>0x088, Start in ag of indices' i-node</summary>
            public readonly ushort indices_start;
            /// <summary>0x08A, As this is part of inode_addr, this is 1</summary>
            public readonly ushort indices_len;
            /// <summary>0x08C, Size of bootloader</summary>
            public readonly int boot_size;
        }
    }
}