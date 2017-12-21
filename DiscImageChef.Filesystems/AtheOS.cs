// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using Schemas;

namespace DiscImageChef.Filesystems
{
    public class AtheOS : Filesystem
    {
        // Little endian constants (that is, as read by .NET :p)
        const uint AFS_MAGIC1 = 0x41465331;
        const uint AFS_MAGIC2 = 0xDD121031;
        const uint AFS_MAGIC3 = 0x15B6830E;
        // Common constants
        const uint AFS_SUPERBLOCK_SIZE = 1024;
        const uint AFS_BOOTBLOCK_SIZE = AFS_SUPERBLOCK_SIZE;

        public AtheOS()
        {
            Name = "AtheOS Filesystem";
            PluginUUID = new Guid("AAB2C4F1-DC07-49EE-A948-576CC51B58C5");
            CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
        }

        public AtheOS(Encoding encoding)
        {
            Name = "AtheOS Filesystem";
            PluginUUID = new Guid("AAB2C4F1-DC07-49EE-A948-576CC51B58C5");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else CurrentEncoding = encoding;
        }

        public AtheOS(ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "AtheOS Filesystem";
            PluginUUID = new Guid("AAB2C4F1-DC07-49EE-A948-576CC51B58C5");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else CurrentEncoding = encoding;
        }

        public override bool Identify(ImagePlugin imagePlugin, Partition partition)
        {
            ulong sector = AFS_BOOTBLOCK_SIZE / imagePlugin.GetSectorSize();
            uint offset = AFS_BOOTBLOCK_SIZE % imagePlugin.GetSectorSize();
            uint run = 1;

            if(imagePlugin.GetSectorSize() < AFS_SUPERBLOCK_SIZE)
                run = AFS_SUPERBLOCK_SIZE / imagePlugin.GetSectorSize();

            if(sector + partition.Start >= partition.End) return false;

            uint magic;

            byte[] tmp = imagePlugin.ReadSectors(sector + partition.Start, run);
            byte[] sb_sector = new byte[AFS_SUPERBLOCK_SIZE];
            Array.Copy(tmp, offset, sb_sector, 0, AFS_SUPERBLOCK_SIZE);

            magic = BitConverter.ToUInt32(sb_sector, 0x20);

            return magic == AFS_MAGIC1;
        }

        public override void GetInformation(ImagePlugin imagePlugin, Partition partition,
                                            out string information)
        {
            information = "";

            StringBuilder sb = new StringBuilder();

            AtheosSuperBlock afs_sb = new AtheosSuperBlock();

            ulong sector = AFS_BOOTBLOCK_SIZE / imagePlugin.GetSectorSize();
            uint offset = AFS_BOOTBLOCK_SIZE % imagePlugin.GetSectorSize();
            uint run = 1;

            if(imagePlugin.GetSectorSize() < AFS_SUPERBLOCK_SIZE)
                run = AFS_SUPERBLOCK_SIZE / imagePlugin.GetSectorSize();

            byte[] tmp = imagePlugin.ReadSectors(sector + partition.Start, run);
            byte[] sb_sector = new byte[AFS_SUPERBLOCK_SIZE];
            Array.Copy(tmp, offset, sb_sector, 0, AFS_SUPERBLOCK_SIZE);

            GCHandle handle = GCHandle.Alloc(sb_sector, GCHandleType.Pinned);
            afs_sb = (AtheosSuperBlock)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(AtheosSuperBlock));
            handle.Free();

            sb.AppendLine("Atheos filesystem");

            if(afs_sb.flags == 1) sb.AppendLine("Filesystem is read-only");

            sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(afs_sb.name, CurrentEncoding)).AppendLine();
            sb.AppendFormat("{0} bytes per block", afs_sb.block_size).AppendLine();
            sb.AppendFormat("{0} blocks in volume ({1} bytes)", afs_sb.num_blocks,
                            afs_sb.num_blocks * afs_sb.block_size).AppendLine();
            sb.AppendFormat("{0} used blocks ({1} bytes)", afs_sb.used_blocks, afs_sb.used_blocks * afs_sb.block_size)
              .AppendLine();
            sb.AppendFormat("{0} bytes per i-node", afs_sb.inode_size).AppendLine();
            sb.AppendFormat("{0} blocks per allocation group ({1} bytes)", afs_sb.blocks_per_ag,
                            afs_sb.blocks_per_ag * afs_sb.block_size).AppendLine();
            sb.AppendFormat("{0} allocation groups in volume", afs_sb.num_ags).AppendLine();
            sb.AppendFormat("Journal resides in block {0} of allocation group {1} and runs for {2} blocks ({3} bytes)",
                            afs_sb.log_blocks_start, afs_sb.log_blocks_ag, afs_sb.log_blocks_len,
                            afs_sb.log_blocks_len * afs_sb.block_size).AppendLine();
            sb.AppendFormat("Journal starts in byte {0} and has {1} bytes in {2} blocks", afs_sb.log_start,
                            afs_sb.log_size, afs_sb.log_valid_blocks).AppendLine();
            sb
                .AppendFormat("Root folder's i-node resides in block {0} of allocation group {1} and runs for {2} blocks ({3} bytes)",
                              afs_sb.root_dir_start, afs_sb.root_dir_ag, afs_sb.root_dir_len,
                              afs_sb.root_dir_len * afs_sb.block_size).AppendLine();
            sb
                .AppendFormat("Directory containing files scheduled for deletion's i-node resides in block {0} of allocation group {1} and runs for {2} blocks ({3} bytes)",
                              afs_sb.deleted_start, afs_sb.deleted_ag, afs_sb.deleted_len,
                              afs_sb.deleted_len * afs_sb.block_size).AppendLine();
            sb
                .AppendFormat("Indices' i-node resides in block {0} of allocation group {1} and runs for {2} blocks ({3} bytes)",
                              afs_sb.indices_start, afs_sb.indices_ag, afs_sb.indices_len,
                              afs_sb.indices_len * afs_sb.block_size).AppendLine();
            sb.AppendFormat("{0} blocks for bootloader ({1} bytes)", afs_sb.boot_size,
                            afs_sb.boot_size * afs_sb.block_size).AppendLine();

            information = sb.ToString();

            xmlFSType = new FileSystemType
            {
                Clusters = afs_sb.num_blocks,
                ClusterSize = (int)afs_sb.block_size,
                Dirty = false,
                FreeClusters = afs_sb.num_blocks - afs_sb.used_blocks,
                FreeClustersSpecified = true,
                Type = "AtheOS filesystem",
                VolumeName = StringHandlers.CToString(afs_sb.name, CurrentEncoding)
            };
        }

        /// <summary>
        /// Be superblock
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct AtheosSuperBlock
        {
            /// <summary>0x000, Volume name, 32 bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] name;
            /// <summary>0x020, "AFS1", 0x41465331</summary>
            public uint magic1;
            /// <summary>0x024, "BIGE", 0x42494745</summary>
            public uint fs_byte_order;
            /// <summary>0x028, Bytes per block</summary>
            public uint block_size;
            /// <summary>0x02C, 1 &lt;&lt; block_shift == block_size</summary>
            public uint block_shift;
            /// <summary>0x030, Blocks in volume</summary>
            public long num_blocks;
            /// <summary>0x038, Used blocks in volume</summary>
            public long used_blocks;
            /// <summary>0x040, Bytes per inode</summary>
            public int inode_size;
            /// <summary>0x044, 0xDD121031</summary>
            public uint magic2;
            /// <summary>0x048, Blocks per allocation group</summary>
            public int blocks_per_ag;
            /// <summary>0x04C, 1 &lt;&lt; ag_shift == blocks_per_ag</summary>
            public int ag_shift;
            /// <summary>0x050, Allocation groups in volume</summary>
            public int num_ags;
            /// <summary>0x054, 0x434c454e if clean, 0x44495254 if dirty</summary>
            public uint flags;
            /// <summary>0x058, Allocation group of journal</summary>
            public int log_blocks_ag;
            /// <summary>0x05C, Start block of journal, inside ag</summary>
            public ushort log_blocks_start;
            /// <summary>0x05E, Length in blocks of journal, inside ag</summary>
            public ushort log_blocks_len;
            /// <summary>0x060, Start of journal</summary>
            public long log_start;
            /// <summary>0x068, Valid block logs</summary>
            public int log_valid_blocks;
            /// <summary>0x06C, Log size</summary>
            public int log_size;
            /// <summary>0x070, 0x15B6830E</summary>
            public uint magic3;
            /// <summary>0x074, Allocation group where root folder's i-node resides</summary>
            public int root_dir_ag;
            /// <summary>0x078, Start in ag of root folder's i-node</summary>
            public ushort root_dir_start;
            /// <summary>0x07A, As this is part of inode_addr, this is 1</summary>
            public ushort root_dir_len;
            /// <summary>0x07C, Allocation group where pending-delete-files' i-node resides</summary>
            public int deleted_ag;
            /// <summary>0x080, Start in ag of pending-delete-files' i-node</summary>
            public ushort deleted_start;
            /// <summary>0x082, As this is part of inode_addr, this is 1</summary>
            public ushort deleted_len;
            /// <summary>0x084, Allocation group where indices' i-node resides</summary>
            public int indices_ag;
            /// <summary>0x088, Start in ag of indices' i-node</summary>
            public ushort indices_start;
            /// <summary>0x08A, As this is part of inode_addr, this is 1</summary>
            public ushort indices_len;
            /// <summary>0x08C, Size of bootloader</summary>
            public int boot_size;
        }

        public override Errno Mount()
        {
            return Errno.NotImplemented;
        }

        public override Errno Mount(bool debug)
        {
            return Errno.NotImplemented;
        }

        public override Errno Unmount()
        {
            return Errno.NotImplemented;
        }

        public override Errno MapBlock(string path, long fileBlock, ref long deviceBlock)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetAttributes(string path, ref FileAttributes attributes)
        {
            return Errno.NotImplemented;
        }

        public override Errno ListXAttr(string path, ref List<string> xattrs)
        {
            return Errno.NotImplemented;
        }

        public override Errno GetXattr(string path, string xattr, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno Read(string path, long offset, long size, ref byte[] buf)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadDir(string path, ref List<string> contents)
        {
            return Errno.NotImplemented;
        }

        public override Errno StatFs(ref FileSystemInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno Stat(string path, ref FileEntryInfo stat)
        {
            return Errno.NotImplemented;
        }

        public override Errno ReadLink(string path, ref string dest)
        {
            return Errno.NotImplemented;
        }
    }
}