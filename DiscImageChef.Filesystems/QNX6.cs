// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : QNX6.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : QNX6 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the QNX6 filesystem and shows information.
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
    public class QNX6 : Filesystem
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct QNX6_RootNode
        {
            public ulong size;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public uint[] pointers;
            public byte levels;
            public byte mode;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public byte[] spare;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct QNX6_SuperBlock
        {
            public uint magic;
            public uint checksum;
            public ulong serial;
            public uint ctime;
            public uint atime;
            public uint flags;
            public ushort version1;
            public ushort version2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] volumeid;
            public uint blockSize;
            public uint numInodes;
            public uint freeInodes;
            public uint numBlocks;
            public uint freeBlocks;
            public uint allocationGroup;
            public QNX6_RootNode inode;
            public QNX6_RootNode bitmap;
            public QNX6_RootNode longfile;
            public QNX6_RootNode unknown;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct QNX6_AudiSuperBlock
        {
            public uint magic;
            public uint checksum;
            public ulong serial;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public byte[] spare1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public byte[] id;
            public uint blockSize;
            public uint numInodes;
            public uint freeInodes;
            public uint numBlocks;
            public uint freeBlocks;
            public uint spare2;
            public QNX6_RootNode inode;
            public QNX6_RootNode bitmap;
            public QNX6_RootNode longfile;
            public QNX6_RootNode unknown;
        }

        const uint QNX6_SUPER_BLOCK_SIZE = 0x1000;
        const uint QNX6_BOOT_BLOCKS_SIZE = 0x2000;
        const uint QNX6_MAGIC = 0x68191122;

        public QNX6()
        {
            Name = "QNX6 Plugin";
            PluginUuid = new Guid("3E610EA2-4D08-4D70-8947-830CD4C74FC0");
            CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
        }

        public QNX6(Encoding encoding)
        {
            Name = "QNX6 Plugin";
            PluginUuid = new Guid("3E610EA2-4D08-4D70-8947-830CD4C74FC0");
            CurrentEncoding = encoding ?? Encoding.GetEncoding("iso-8859-15");
        }

        public QNX6(ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "QNX6 Plugin";
            PluginUuid = new Guid("3E610EA2-4D08-4D70-8947-830CD4C74FC0");
            CurrentEncoding = encoding ?? Encoding.GetEncoding("iso-8859-15");
        }

        public override bool Identify(ImagePlugin imagePlugin, Partition partition)
        {
            uint sectors = QNX6_SUPER_BLOCK_SIZE / imagePlugin.GetSectorSize();
            uint bootSectors = QNX6_BOOT_BLOCKS_SIZE / imagePlugin.GetSectorSize();

            if(partition.Start + bootSectors + sectors >= partition.End) return false;

            byte[] audiSector = imagePlugin.ReadSectors(partition.Start, sectors);
            byte[] sector = imagePlugin.ReadSectors(partition.Start + bootSectors, sectors);
            if(sector.Length < QNX6_SUPER_BLOCK_SIZE) return false;

            QNX6_AudiSuperBlock audiSb = new QNX6_AudiSuperBlock();
            IntPtr audiPtr = Marshal.AllocHGlobal(Marshal.SizeOf(audiSb));
            Marshal.Copy(audiSector, 0, audiPtr, Marshal.SizeOf(audiSb));
            audiSb = (QNX6_AudiSuperBlock)Marshal.PtrToStructure(audiPtr, typeof(QNX6_AudiSuperBlock));
            Marshal.FreeHGlobal(audiPtr);

            QNX6_SuperBlock qnxSb = new QNX6_SuperBlock();
            IntPtr sbPtr = Marshal.AllocHGlobal(Marshal.SizeOf(qnxSb));
            Marshal.Copy(sector, 0, sbPtr, Marshal.SizeOf(qnxSb));
            qnxSb = (QNX6_SuperBlock)Marshal.PtrToStructure(sbPtr, typeof(QNX6_SuperBlock));
            Marshal.FreeHGlobal(sbPtr);

            return qnxSb.magic == QNX6_MAGIC || audiSb.magic == QNX6_MAGIC;
        }

        public override void GetInformation(ImagePlugin imagePlugin, Partition partition,
                                            out string information)
        {
            information = "";
            StringBuilder sb = new StringBuilder();
            uint sectors = QNX6_SUPER_BLOCK_SIZE / imagePlugin.GetSectorSize();
            uint bootSectors = QNX6_BOOT_BLOCKS_SIZE / imagePlugin.GetSectorSize();

            byte[] audiSector = imagePlugin.ReadSectors(partition.Start, sectors);
            byte[] sector = imagePlugin.ReadSectors(partition.Start + bootSectors, sectors);
            if(sector.Length < QNX6_SUPER_BLOCK_SIZE) return;

            QNX6_AudiSuperBlock audiSb = new QNX6_AudiSuperBlock();
            IntPtr audiPtr = Marshal.AllocHGlobal(Marshal.SizeOf(audiSb));
            Marshal.Copy(audiSector, 0, audiPtr, Marshal.SizeOf(audiSb));
            audiSb = (QNX6_AudiSuperBlock)Marshal.PtrToStructure(audiPtr, typeof(QNX6_AudiSuperBlock));
            Marshal.FreeHGlobal(audiPtr);

            QNX6_SuperBlock qnxSb = new QNX6_SuperBlock();
            IntPtr sbPtr = Marshal.AllocHGlobal(Marshal.SizeOf(qnxSb));
            Marshal.Copy(sector, 0, sbPtr, Marshal.SizeOf(qnxSb));
            qnxSb = (QNX6_SuperBlock)Marshal.PtrToStructure(sbPtr, typeof(QNX6_SuperBlock));
            Marshal.FreeHGlobal(sbPtr);

            bool audi = audiSb.magic == QNX6_MAGIC;

            if(audi)
            {
                sb.AppendLine("QNX6 (Audi) filesystem");
                sb.AppendFormat("Checksum: 0x{0:X8}", audiSb.checksum).AppendLine();
                sb.AppendFormat("Serial: 0x{0:X16}", audiSb.checksum).AppendLine();
                sb.AppendFormat("{0} bytes per block", audiSb.blockSize).AppendLine();
                sb.AppendFormat("{0} inodes free of {1}", audiSb.freeInodes, audiSb.numInodes).AppendLine();
                sb.AppendFormat("{0} blocks ({1} bytes) free of {2} ({3} bytes)", audiSb.freeBlocks,
                                audiSb.freeBlocks * audiSb.blockSize, audiSb.numBlocks,
                                audiSb.numBlocks * audiSb.blockSize).AppendLine();

                XmlFsType = new FileSystemType
                {
                    Type = "QNX6 (Audi) filesystem",
                    Clusters = audiSb.numBlocks,
                    ClusterSize = (int)audiSb.blockSize,
                    Bootable = true,
                    Files = audiSb.numInodes - audiSb.freeInodes,
                    FilesSpecified = true,
                    FreeClusters = audiSb.freeBlocks,
                    FreeClustersSpecified = true,
                    VolumeSerial = $"{audiSb.serial:X16}"
                };
                //xmlFSType.VolumeName = CurrentEncoding.GetString(audiSb.id);

                information = sb.ToString();
                return;
            }

            sb.AppendLine("QNX6 filesystem");
            sb.AppendFormat("Checksum: 0x{0:X8}", qnxSb.checksum).AppendLine();
            sb.AppendFormat("Serial: 0x{0:X16}", qnxSb.checksum).AppendLine();
            sb.AppendFormat("Created on {0}", DateHandlers.UnixUnsignedToDateTime(qnxSb.ctime)).AppendLine();
            sb.AppendFormat("Last mounted on {0}", DateHandlers.UnixUnsignedToDateTime(qnxSb.atime)).AppendLine();
            sb.AppendFormat("Flags: 0x{0:X8}", qnxSb.flags).AppendLine();
            sb.AppendFormat("Version1: 0x{0:X4}", qnxSb.version1).AppendLine();
            sb.AppendFormat("Version2: 0x{0:X4}", qnxSb.version2).AppendLine();
            //sb.AppendFormat("Volume ID: \"{0}\"", CurrentEncoding.GetString(qnxSb.volumeid)).AppendLine();
            sb.AppendFormat("{0} bytes per block", qnxSb.blockSize).AppendLine();
            sb.AppendFormat("{0} inodes free of {1}", qnxSb.freeInodes, qnxSb.numInodes).AppendLine();
            sb.AppendFormat("{0} blocks ({1} bytes) free of {2} ({3} bytes)", qnxSb.freeBlocks,
                            qnxSb.freeBlocks * qnxSb.blockSize, qnxSb.numBlocks, qnxSb.numBlocks * qnxSb.blockSize)
              .AppendLine();

            XmlFsType = new FileSystemType
            {
                Type = "QNX6 filesystem",
                Clusters = qnxSb.numBlocks,
                ClusterSize = (int)qnxSb.blockSize,
                Bootable = true,
                Files = qnxSb.numInodes - qnxSb.freeInodes,
                FilesSpecified = true,
                FreeClusters = qnxSb.freeBlocks,
                FreeClustersSpecified = true,
                VolumeSerial = $"{qnxSb.serial:X16}",
                CreationDate = DateHandlers.UnixUnsignedToDateTime(qnxSb.ctime),
                CreationDateSpecified = true,
                ModificationDate = DateHandlers.UnixUnsignedToDateTime(qnxSb.atime),
                ModificationDateSpecified = true
            };
            //xmlFSType.VolumeName = CurrentEncoding.GetString(qnxSb.volumeid);

            information = sb.ToString();
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