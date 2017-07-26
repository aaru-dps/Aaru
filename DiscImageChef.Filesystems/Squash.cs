// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Squash.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Squash file system plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Squash file system and shows information.
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;

namespace DiscImageChef.Filesystems
{
    public class Squash : Filesystem
    {
        public Squash()
        {
            Name = "Squash filesystem";
            PluginUUID = new Guid("F8F6E46F-7A2A-48E3-9C0A-46AF4DC29E09");
            CurrentEncoding = Encoding.UTF8;
        }

        public Squash(ImagePlugins.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "Squash filesystem";
            PluginUUID = new Guid("F8F6E46F-7A2A-48E3-9C0A-46AF4DC29E09");
            if(encoding == null)
                CurrentEncoding = Encoding.UTF8;
            else
                CurrentEncoding = encoding;
        }

        enum SquashCompression : ushort
        {
            Zlib = 1,
            Lzma = 2,
            Lzo = 3,
            Xz = 4,
            Lz4 = 5
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct SquashSuperBlock
        {
            public uint magic;
            public uint inodes;
            public uint mkfs_time;
            public uint block_size;
            public uint fragments;
            public ushort compression;
            public ushort block_log;
            public ushort flags;
            public ushort no_ids;
            public ushort s_major;
            public ushort s_minor;
            public ulong root_inode;
            public ulong bytes_used;
            public ulong id_table_start;
            public ulong xattr_id_table_start;
            public ulong inode_table_start;
            public ulong directory_table_start;
            public ulong fragment_table_start;
            public ulong lookup_table_start;
        }

        /// <summary>
        /// Identifier for Squash
        /// </summary>
        const uint Squash_MAGIC = 0x73717368;
        const uint Squash_CIGAM = 0x68737173;

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, Partition partition)
        {
            if(partition.Start >= partition.End)
                return false;

            byte[] sector = imagePlugin.ReadSector(partition.Start);

            uint magic = BitConverter.ToUInt32(sector, 0x00);

            return magic == Squash_MAGIC || magic == Squash_CIGAM;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, Partition partition, out string information)
        {
            byte[] sector = imagePlugin.ReadSector(partition.Start);
            uint magic = BitConverter.ToUInt32(sector, 0x00);

            SquashSuperBlock sqSb = new SquashSuperBlock();
            bool littleEndian = true;

            if(magic == Squash_MAGIC)
            {
                IntPtr sqSbPtr = Marshal.AllocHGlobal(Marshal.SizeOf(sqSb));
                Marshal.Copy(sector, 0, sqSbPtr, Marshal.SizeOf(sqSb));
                sqSb = (SquashSuperBlock)Marshal.PtrToStructure(sqSbPtr, typeof(SquashSuperBlock));
                Marshal.FreeHGlobal(sqSbPtr);
            }
            else if(magic == Squash_CIGAM)
            {
                sqSb = BigEndianMarshal.ByteArrayToStructureBigEndian<SquashSuperBlock>(sector);
                littleEndian = false;
            }

            StringBuilder sbInformation = new StringBuilder();

            sbInformation.AppendLine("Squash file system");
            if(littleEndian)
                sbInformation.AppendLine("Little-endian");
            else
                sbInformation.AppendLine("Big-endian");
            sbInformation.AppendFormat("Volume version {0}.{1}", sqSb.s_major, sqSb.s_minor).AppendLine();
            sbInformation.AppendFormat("Volume has {0} bytes", sqSb.bytes_used).AppendLine();
            sbInformation.AppendFormat("Volume has {0} bytes per block", sqSb.block_size).AppendLine();
            sbInformation.AppendFormat("Volume created on {0}", DateHandlers.UNIXUnsignedToDateTime(sqSb.mkfs_time)).AppendLine();
            sbInformation.AppendFormat("Volume has {0} inodes", sqSb.inodes).AppendLine();
            switch(sqSb.compression)
            {
                case (ushort)SquashCompression.Lz4:
                    sbInformation.AppendLine("Volume is compressed using LZ4");
                    break;
                case (ushort)SquashCompression.Lzo:
                    sbInformation.AppendLine("Volume is compressed using LZO");
                    break;
                case (ushort)SquashCompression.Lzma:
                    sbInformation.AppendLine("Volume is compressed using LZMA");
                    break;
                case (ushort)SquashCompression.Xz:
                    sbInformation.AppendLine("Volume is compressed using XZ");
                    break;
                case (ushort)SquashCompression.Zlib:
                    sbInformation.AppendLine("Volume is compressed using GZIP");
                    break;
                default:
                    sbInformation.AppendFormat("Volume is compressed using unknown algorithm {0}", sqSb.compression).AppendLine();
                    break;
            }

            information = sbInformation.ToString();

            xmlFSType = new Schemas.FileSystemType();
            xmlFSType.Type = "Squash file system";
            xmlFSType.CreationDate = DateHandlers.UNIXUnsignedToDateTime(sqSb.mkfs_time);
            xmlFSType.CreationDateSpecified = true;
            xmlFSType.Clusters = (long)(((partition.End - partition.Start + 1) * imagePlugin.ImageInfo.sectorSize) / sqSb.block_size);
            xmlFSType.ClusterSize = (int)sqSb.block_size;
            xmlFSType.Files = sqSb.inodes;
            xmlFSType.FilesSpecified = true;
            xmlFSType.FreeClusters = 0;
            xmlFSType.FreeClustersSpecified = true;
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