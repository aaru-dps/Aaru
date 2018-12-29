// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : QNX4.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : QNX4 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the QNX4 filesystem and shows information.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using Schemas;

namespace DiscImageChef.Filesystems
{
    public class QNX4 : IFilesystem
    {
        readonly byte[] qnx4_rootDir_fname =
        {
            0x2F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
        };

        public FileSystemType XmlFsType { get; private set; }
        public Encoding       Encoding  { get; private set; }
        public string         Name      => "QNX4 Plugin";
        public Guid           Id        => new Guid("E73A63FA-B5B0-48BF-BF82-DA5F0A8170D2");
        public string         Author    => "Natalia Portillo";

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(partition.Start + 1 >= imagePlugin.Info.Sectors) return false;

            byte[] sector = imagePlugin.ReadSector(partition.Start + 1);
            if(sector.Length < 512) return false;

            IntPtr sbPtr = Marshal.AllocHGlobal(512);
            Marshal.Copy(sector, 0, sbPtr, 512);
            QNX4_Superblock qnxSb = (QNX4_Superblock)Marshal.PtrToStructure(sbPtr, typeof(QNX4_Superblock));
            Marshal.FreeHGlobal(sbPtr);

            // Check root directory name
            if(!qnx4_rootDir_fname.SequenceEqual(qnxSb.rootDir.di_fname)) return false;

            // Check sizes are multiple of blocks
            if(qnxSb.rootDir.di_size % 512 != 0 || qnxSb.inode.di_size % 512 != 0 || qnxSb.boot.di_size % 512 != 0 ||
               qnxSb.altBoot.di_size % 512 != 0) return false;

            // Check extents are not past device
            if(qnxSb.rootDir.di_first_xtnt.block + partition.Start >= partition.End ||
               qnxSb.inode.di_first_xtnt.block   + partition.Start >= partition.End ||
               qnxSb.boot.di_first_xtnt.block    + partition.Start >= partition.End ||
               qnxSb.altBoot.di_first_xtnt.block + partition.Start >= partition.End) return false;

            // Check inodes are in use
            if((qnxSb.rootDir.di_status & 0x01) != 0x01 || (qnxSb.inode.di_status & 0x01) != 0x01 ||
               (qnxSb.boot.di_status    & 0x01) != 0x01) return false;

            // All hail filesystems without identification marks
            return true;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding    encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";
            byte[] sector = imagePlugin.ReadSector(partition.Start + 1);
            if(sector.Length < 512) return;

            IntPtr sbPtr = Marshal.AllocHGlobal(512);
            Marshal.Copy(sector, 0, sbPtr, 512);
            QNX4_Superblock qnxSb = (QNX4_Superblock)Marshal.PtrToStructure(sbPtr, typeof(QNX4_Superblock));
            Marshal.FreeHGlobal(sbPtr);

            // Too much useless information
            /*
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_fname = {0}", CurrentEncoding.GetString(qnxSb.rootDir.di_fname));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_size = {0}", qnxSb.rootDir.di_size);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_first_xtnt.block = {0}", qnxSb.rootDir.di_first_xtnt.block);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_first_xtnt.length = {0}", qnxSb.rootDir.di_first_xtnt.length);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_xblk = {0}", qnxSb.rootDir.di_xblk);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_ftime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.rootDir.di_ftime));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_mtime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.rootDir.di_mtime));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_atime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.rootDir.di_atime));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_ctime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.rootDir.di_ctime));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_num_xtnts = {0}", qnxSb.rootDir.di_num_xtnts);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_mode = {0}", Convert.ToString(qnxSb.rootDir.di_mode, 8));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_uid = {0}", qnxSb.rootDir.di_uid);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_gid = {0}", qnxSb.rootDir.di_gid);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_nlink = {0}", qnxSb.rootDir.di_nlink);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_zero = {0}", qnxSb.rootDir.di_zero);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_type = {0}", qnxSb.rootDir.di_type);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_status = {0}", qnxSb.rootDir.di_status);

            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_fname = {0}", CurrentEncoding.GetString(qnxSb.inode.di_fname));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_size = {0}", qnxSb.inode.di_size);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_first_xtnt.block = {0}", qnxSb.inode.di_first_xtnt.block);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_first_xtnt.length = {0}", qnxSb.inode.di_first_xtnt.length);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_xblk = {0}", qnxSb.inode.di_xblk);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_ftime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.inode.di_ftime));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_mtime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.inode.di_mtime));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_atime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.inode.di_atime));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_ctime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.inode.di_ctime));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_num_xtnts = {0}", qnxSb.inode.di_num_xtnts);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_mode = {0}", Convert.ToString(qnxSb.inode.di_mode, 8));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_uid = {0}", qnxSb.inode.di_uid);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_gid = {0}", qnxSb.inode.di_gid);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_nlink = {0}", qnxSb.inode.di_nlink);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_zero = {0}", qnxSb.inode.di_zero);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_type = {0}", qnxSb.inode.di_type);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_status = {0}", qnxSb.inode.di_status);

            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_fname = {0}", CurrentEncoding.GetString(qnxSb.boot.di_fname));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_size = {0}", qnxSb.boot.di_size);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_first_xtnt.block = {0}", qnxSb.boot.di_first_xtnt.block);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_first_xtnt.length = {0}", qnxSb.boot.di_first_xtnt.length);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_xblk = {0}", qnxSb.boot.di_xblk);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_ftime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.boot.di_ftime));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_mtime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.boot.di_mtime));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_atime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.boot.di_atime));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_ctime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.boot.di_ctime));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_num_xtnts = {0}", qnxSb.boot.di_num_xtnts);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_mode = {0}", Convert.ToString(qnxSb.boot.di_mode, 8));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_uid = {0}", qnxSb.boot.di_uid);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_gid = {0}", qnxSb.boot.di_gid);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_nlink = {0}", qnxSb.boot.di_nlink);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_zero = {0}", qnxSb.boot.di_zero);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_type = {0}", qnxSb.boot.di_type);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_status = {0}", qnxSb.boot.di_status);

            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_fname = {0}", CurrentEncoding.GetString(qnxSb.altBoot.di_fname));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_size = {0}", qnxSb.altBoot.di_size);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_first_xtnt.block = {0}", qnxSb.altBoot.di_first_xtnt.block);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_first_xtnt.length = {0}", qnxSb.altBoot.di_first_xtnt.length);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_xblk = {0}", qnxSb.altBoot.di_xblk);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_ftime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.altBoot.di_ftime));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_mtime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.altBoot.di_mtime));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_atime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.altBoot.di_atime));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_ctime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.altBoot.di_ctime));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_num_xtnts = {0}", qnxSb.altBoot.di_num_xtnts);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_mode = {0}", Convert.ToString(qnxSb.altBoot.di_mode, 8));
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_uid = {0}", qnxSb.altBoot.di_uid);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_gid = {0}", qnxSb.altBoot.di_gid);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_nlink = {0}", qnxSb.altBoot.di_nlink);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_zero = {0}", qnxSb.altBoot.di_zero);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_type = {0}", qnxSb.altBoot.di_type);
            DicConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_status = {0}", qnxSb.altBoot.di_status);
            */

            information =
                $"QNX4 filesystem\nCreated on {DateHandlers.UnixUnsignedToDateTime(qnxSb.rootDir.di_ftime)}\n";

            XmlFsType = new FileSystemType
            {
                Type                      = "QNX4 filesystem",
                Clusters                  = (long)partition.Length,
                ClusterSize               = 512,
                CreationDate              = DateHandlers.UnixUnsignedToDateTime(qnxSb.rootDir.di_ftime),
                CreationDateSpecified     = true,
                ModificationDate          = DateHandlers.UnixUnsignedToDateTime(qnxSb.rootDir.di_mtime),
                ModificationDateSpecified = true
            };
            XmlFsType.Bootable |= qnxSb.boot.di_size != 0 || qnxSb.altBoot.di_size != 0;
        }

        struct QNX4_Extent
        {
            public uint block;
            public uint length;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct QNX4_Inode
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] di_fname;
            public uint        di_size;
            public QNX4_Extent di_first_xtnt;
            public uint        di_xblk;
            public uint        di_ftime;
            public uint        di_mtime;
            public uint        di_atime;
            public uint        di_ctime;
            public ushort      di_num_xtnts;
            public ushort      di_mode;
            public ushort      di_uid;
            public ushort      di_gid;
            public ushort      di_nlink;
            public uint        di_zero;
            public byte        di_type;
            public byte        di_status;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct QNX4_LinkInfo
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] dl_fname;
            public uint dl_inode_blk;
            public byte dl_inode_ndx;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public byte[] dl_spare;
            public byte dl_status;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct QNX4_ExtentBlock
        {
            public uint next_xblk;
            public uint prev_xblk;
            public byte num_xtnts;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] spare;
            public uint num_blocks;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
            public QNX4_Extent[] xtnts;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] signature;
            public QNX4_Extent first_xtnt;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct QNX4_Superblock
        {
            public QNX4_Inode rootDir;
            public QNX4_Inode inode;
            public QNX4_Inode boot;
            public QNX4_Inode altBoot;
        }
    }
}