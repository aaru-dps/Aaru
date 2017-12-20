// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;

namespace DiscImageChef.Filesystems
{
    // Information from Practical Filesystem Design, ISBN 1-55860-497-9
    public class BeFS : Filesystem
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

        public BeFS()
        {
            Name = "Be Filesystem";
            PluginUUID = new Guid("dc8572b3-b6ad-46e4-8de9-cbe123ff6672");
            CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
        }

        public BeFS(Encoding encoding)
        {
            Name = "Be Filesystem";
            PluginUUID = new Guid("dc8572b3-b6ad-46e4-8de9-cbe123ff6672");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else CurrentEncoding = encoding;
        }

        public BeFS(DiscImages.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "Be Filesystem";
            PluginUUID = new Guid("dc8572b3-b6ad-46e4-8de9-cbe123ff6672");
            if(encoding == null) CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else CurrentEncoding = encoding;
        }

        public override bool Identify(DiscImages.ImagePlugin imagePlugin, Partition partition)
        {
            if(2 + partition.Start >= partition.End) return false;

            uint magic;
            uint magic_be;

            byte[] sb_sector = imagePlugin.ReadSector(0 + partition.Start);

            magic = BitConverter.ToUInt32(sb_sector, 0x20);
            magic_be = BigEndianBitConverter.ToUInt32(sb_sector, 0x20);

            if(magic == BEFS_MAGIC1 || magic_be == BEFS_MAGIC1) return true;

            if(sb_sector.Length >= 0x400)
            {
                magic = BitConverter.ToUInt32(sb_sector, 0x220);
                magic_be = BigEndianBitConverter.ToUInt32(sb_sector, 0x220);
            }

            if(magic == BEFS_MAGIC1 || magic_be == BEFS_MAGIC1) return true;

            sb_sector = imagePlugin.ReadSector(1 + partition.Start);

            magic = BitConverter.ToUInt32(sb_sector, 0x20);
            magic_be = BigEndianBitConverter.ToUInt32(sb_sector, 0x20);

            if(magic == BEFS_MAGIC1 || magic_be == BEFS_MAGIC1) return true;

            return false;
        }

        public override void GetInformation(DiscImages.ImagePlugin imagePlugin, Partition partition,
                                            out string information)
        {
            information = "";
            byte[] name_bytes = new byte[32];

            StringBuilder sb = new StringBuilder();

            BeSuperBlock besb = new BeSuperBlock();

            byte[] sb_sector = imagePlugin.ReadSector(0 + partition.Start);

            bool littleEndian = true;

            besb.magic1 = BigEndianBitConverter.ToUInt32(sb_sector, 0x20);
            if(besb.magic1 == BEFS_MAGIC1 || besb.magic1 == BEFS_CIGAM1) // Magic is at offset
            {
                littleEndian = besb.magic1 == BEFS_CIGAM1;
            }
            else
            {
                sb_sector = imagePlugin.ReadSector(1 + partition.Start);
                besb.magic1 = BigEndianBitConverter.ToUInt32(sb_sector, 0x20);

                if(besb.magic1 == BEFS_MAGIC1 || besb.magic1 == BEFS_CIGAM1) // There is a boot sector
                {
                    littleEndian = besb.magic1 == BEFS_CIGAM1;
                }
                else if(sb_sector.Length >= 0x400)
                {
                    byte[] temp = imagePlugin.ReadSector(0 + partition.Start);
                    besb.magic1 = BigEndianBitConverter.ToUInt32(temp, 0x220);

                    if(besb.magic1 == BEFS_MAGIC1 || besb.magic1 == BEFS_CIGAM1) // There is a boot sector
                    {
                        littleEndian = besb.magic1 == BEFS_CIGAM1;
                        sb_sector = new byte[0x200];
                        Array.Copy(temp, 0x200, sb_sector, 0, 0x200);
                    }
                    else return;
                }
                else return;
            }

            if(littleEndian)
            {
                GCHandle handle = GCHandle.Alloc(sb_sector, GCHandleType.Pinned);
                besb = (BeSuperBlock)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(BeSuperBlock));
                handle.Free();
            }
            else besb = BigEndianMarshal.ByteArrayToStructureBigEndian<BeSuperBlock>(sb_sector);

            if(littleEndian) // Big-endian filesystem
                sb.AppendLine("Little-endian BeFS");
            else sb.AppendLine("Big-endian BeFS");

            if(besb.magic1 != BEFS_MAGIC1 || besb.fs_byte_order != BEFS_ENDIAN || besb.magic2 != BEFS_MAGIC2 ||
               besb.magic3 != BEFS_MAGIC3 || besb.root_dir_len != 1 || besb.indices_len != 1 ||
               1 << (int)besb.block_shift != besb.block_size)
            {
                sb.AppendLine("Superblock seems corrupt, following information may be incorrect");
                sb.AppendFormat("Magic 1: 0x{0:X8} (Should be 0x42465331)", besb.magic1).AppendLine();
                sb.AppendFormat("Magic 2: 0x{0:X8} (Should be 0xDD121031)", besb.magic2).AppendLine();
                sb.AppendFormat("Magic 3: 0x{0:X8} (Should be 0x15B6830E)", besb.magic3).AppendLine();
                sb.AppendFormat("Filesystem endianness: 0x{0:X8} (Should be 0x42494745)", besb.fs_byte_order)
                  .AppendLine();
                sb.AppendFormat("Root folder's i-node size: {0} blocks (Should be 1)", besb.root_dir_len).AppendLine();
                sb.AppendFormat("Indices' i-node size: {0} blocks (Should be 1)", besb.indices_len).AppendLine();
                sb.AppendFormat("1 << block_shift == block_size => 1 << {0} == {1} (Should be {2})", besb.block_shift,
                                1 << (int)besb.block_shift, besb.block_size).AppendLine();
            }

            if(besb.flags == BEFS_CLEAN)
            {
                if(besb.log_start == besb.log_end) sb.AppendLine("Filesystem is clean");
                else sb.AppendLine("Filesystem is dirty");
            }
            else if(besb.flags == BEFS_DIRTY) sb.AppendLine("Filesystem is dirty");
            else sb.AppendFormat("Unknown flags: {0:X8}", besb.flags).AppendLine();

            sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(besb.name, CurrentEncoding)).AppendLine();
            sb.AppendFormat("{0} bytes per block", besb.block_size).AppendLine();
            sb.AppendFormat("{0} blocks in volume ({1} bytes)", besb.num_blocks, besb.num_blocks * besb.block_size)
              .AppendLine();
            sb.AppendFormat("{0} used blocks ({1} bytes)", besb.used_blocks, besb.used_blocks * besb.block_size)
              .AppendLine();
            sb.AppendFormat("{0} bytes per i-node", besb.inode_size).AppendLine();
            sb.AppendFormat("{0} blocks per allocation group ({1} bytes)", besb.blocks_per_ag,
                            besb.blocks_per_ag * besb.block_size).AppendLine();
            sb.AppendFormat("{0} allocation groups in volume", besb.num_ags).AppendLine();
            sb.AppendFormat("Journal resides in block {0} of allocation group {1} and runs for {2} blocks ({3} bytes)",
                            besb.log_blocks_start, besb.log_blocks_ag, besb.log_blocks_len,
                            besb.log_blocks_len * besb.block_size).AppendLine();
            sb.AppendFormat("Journal starts in byte {0} and ends in byte {1}", besb.log_start, besb.log_end)
              .AppendLine();
            sb
                .AppendFormat("Root folder's i-node resides in block {0} of allocation group {1} and runs for {2} blocks ({3} bytes)",
                              besb.root_dir_start, besb.root_dir_ag, besb.root_dir_len,
                              besb.root_dir_len * besb.block_size).AppendLine();
            sb
                .AppendFormat("Indices' i-node resides in block {0} of allocation group {1} and runs for {2} blocks ({3} bytes)",
                              besb.indices_start, besb.indices_ag, besb.indices_len, besb.indices_len * besb.block_size)
                .AppendLine();

            information = sb.ToString();

            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.Clusters = besb.num_blocks;
            xmlFSType.ClusterSize = (int)besb.block_size;
            xmlFSType.Dirty = besb.flags == BEFS_DIRTY;
            xmlFSType.FreeClusters = besb.num_blocks - besb.used_blocks;
            xmlFSType.FreeClustersSpecified = true;
            xmlFSType.Type = "BeFS";
            xmlFSType.VolumeName = StringHandlers.CToString(besb.name, CurrentEncoding);
        }

        /// <summary>
        /// Be superblock
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct BeSuperBlock
        {
            /// <summary>0x000, Volume name, 32 bytes</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)] public byte[] name;
            /// <summary>0x020, "BFS1", 0x42465331</summary>
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
            /// <summary>0x068, End of journal</summary>
            public long log_end;
            /// <summary>0x070, 0x15B6830E</summary>
            public uint magic3;
            /// <summary>0x074, Allocation group where root folder's i-node resides</summary>
            public int root_dir_ag;
            /// <summary>0x078, Start in ag of root folder's i-node</summary>
            public ushort root_dir_start;
            /// <summary>0x07A, As this is part of inode_addr, this is 1</summary>
            public ushort root_dir_len;
            /// <summary>0x07C, Allocation group where indices' i-node resides</summary>
            public int indices_ag;
            /// <summary>0x080, Start in ag of indices' i-node</summary>
            public ushort indices_start;
            /// <summary>0x082, As this is part of inode_addr, this is 1</summary>
            public ushort indices_len;
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