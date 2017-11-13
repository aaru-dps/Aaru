// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : UNICOS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the UNICOS filesystem and shows information.
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
using DiscImageChef.Console;
// UNICOS is ILP64 so let's think everything is 64-bit
using dev_t = System.Int64;
using daddr_t = System.Int64;
using time_t = System.Int64;
using blkno_t = System.Int64;
using ino_t = System.Int64;
using extent_t = System.Int64;

namespace DiscImageChef.Filesystems
{
    public class UNICOS : Filesystem
    {
        const int NC1MAXPART = 64;
        const int NC1MAXIREG = 4;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct nc1ireg_sb
        {
            public ushort i_unused;    /* reserved */
            public ushort i_nblk;    /* number of blocks */
            public uint i_sblk;    /* start block number */
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct nc1fdev_sb
        {
            public long fd_name;    /* Physical device name */
            public uint fd_sblk;    /* Start block number */
            public uint fd_nblk;    /* Number of blocks */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NC1MAXIREG)]
            public nc1ireg_sb[] fd_ireg; /* Inode regions */
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct UNICOS_Superblock
        {
            public ulong s_magic;    /* magic number to indicate file system type */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] s_fname; /* file system name */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] s_fpack; /* file system pack name */
            public dev_t s_dev;      /* major/minor device, for verification */

            public daddr_t s_fsize;    /* size in blocks of entire volume */
            public long s_isize;    /* Number of total inodes */
            public long s_bigfile;  /* number of bytes at which a file is big */
            public long s_bigunit;  /* minimum number of blocks allocated for big files */
            public ulong s_secure;   /* security: secure FS label */
            public long s_maxlvl;   /* security: maximum security level */
            public long s_minlvl;   /* security: minimum security level */
            public long s_valcmp;   /* security: valid security compartments */
            public time_t s_time;     /* last super block update */
            public blkno_t s_dboff;    /* Dynamic block number */
            public ino_t s_root;     /* root inode */
            public long s_error;    /* Type of file system error detected */
            public blkno_t s_mapoff;   /* Start map block number */
            public long s_mapblks;  /* Last map block number */
            public long s_nscpys;   /* Number of copies of s.b per partition */
            public long s_npart;    /* Number of partitions */
            public long s_ifract;   /* Ratio of inodes to blocks */
            public extent_t s_sfs;     /* SFS only blocks */
            public long s_flag;     /* Flag word */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NC1MAXPART)]
            public nc1fdev_sb[] s_part;  /* Partition descriptors */
            public long s_iounit;       /* Physical block size */
            public long s_numiresblks;  /* number of inode reservation blocks */
                                        /* per region (currently 1) */
                                        /* 0 = 1*(AU) words, n = (n+1)*(AU) words */
            public long s_priparts; /* bitmap of primary partitions */
            public long s_priblock; /* block size of primary partition(s) */
                                    /* 0 = 1*512 words, n = (n+1)*512 words */
            public long s_prinblks; /* number of 512 wds blocks in primary */
            public long s_secparts; /* bitmap of secondary partitions */
            public long s_secblock; /* block size of secondary partition(s) */
                                    /* 0 = 1*512 words, n = (n+1)*512 words */
            public long s_secnblks; /* number of 512 wds blocks in secondary */
            public long s_sbdbparts;    /* bitmap of partitions with file system data */
                                        /* including super blocks, dynamic block */
                                        /* and free block bitmaps (only primary */
                                        /* partitions may contain these) */
            public long s_rootdparts;   /* bitmap of partitions with root directory */
                                        /* (only primary partitions) */
            public long s_nudparts; /* bitmap of no-user-data partitions */
                                    /* (only primary partitions) */
            public long s_nsema;    /* SFS: # fs semaphores to allocate */
            public long s_priactive;    /* bitmap of primary partitions which contain */
                                        /* active (up to date) dynamic blocks and */
                                        /* free block bitmaps. All bits set indicate */
                                        /* that all primary partitions are active, */
                                        /* and no kernel manipulation of active flag */
                                        /* is allowed. */
            public long s_sfs_arbiterid;/* SFS Arbiter ID */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 91)]
            public long[] s_fill; /* reserved */
        }

        const ulong UNICOS_Magic = 0x6e6331667331636e;
        const ulong UNICOS_Secure = 0xcd076d1771d670cd;

        public UNICOS()
        {
            Name = "UNICOS Filesystem Plugin";
            PluginUUID = new Guid("61712F04-066C-44D5-A2A0-1E44C66B33F0");
            CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
        }

        public UNICOS(Encoding encoding)
        {
            Name = "UNICOS Filesystem Plugin";
            PluginUUID = new Guid("61712F04-066C-44D5-A2A0-1E44C66B33F0");
            if(encoding == null)
                CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else
                CurrentEncoding = encoding;
        }

        public UNICOS(ImagePlugins.ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "UNICOS Filesystem Plugin";
            PluginUUID = new Guid("61712F04-066C-44D5-A2A0-1E44C66B33F0");
            if(encoding == null)
                CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
            else
                CurrentEncoding = encoding;
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, Partition partition)
        {
            if(imagePlugin.GetSectorSize() < 512)
                return false;

            UNICOS_Superblock unicosSb = new UNICOS_Superblock();

            uint sbSize = (uint)((Marshal.SizeOf(unicosSb)) / imagePlugin.GetSectorSize());
            if((Marshal.SizeOf(unicosSb)) % imagePlugin.GetSectorSize() != 0)
                sbSize++;

            byte[] sector = imagePlugin.ReadSectors(partition.Start, sbSize);
            if(sector.Length < Marshal.SizeOf(unicosSb))
                return false;

            unicosSb = BigEndianMarshal.ByteArrayToStructureBigEndian<UNICOS_Superblock>(sector);

            DicConsole.DebugWriteLine("UNICOS plugin", "magic = 0x{0:X16} (expected 0x{1:X16})", unicosSb.s_magic, UNICOS_Magic);

            return unicosSb.s_magic == UNICOS_Magic;
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, Partition partition, out string information)
        {
            information = "";
            if(imagePlugin.GetSectorSize() < 512)
                return;

            UNICOS_Superblock unicosSb = new UNICOS_Superblock();

            uint sbSize = (uint)((Marshal.SizeOf(unicosSb)) / imagePlugin.GetSectorSize());
            if((Marshal.SizeOf(unicosSb)) % imagePlugin.GetSectorSize() != 0)
                sbSize++;

            byte[] sector = imagePlugin.ReadSectors(partition.Start, sbSize);
            if(sector.Length < Marshal.SizeOf(unicosSb))
                return;

            unicosSb = BigEndianMarshal.ByteArrayToStructureBigEndian<UNICOS_Superblock>(sector);


            if(unicosSb.s_magic != UNICOS_Magic)
                return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("UNICOS filesystem");
            if(unicosSb.s_secure == UNICOS_Secure)
                sb.AppendLine("Volume is secure");
            sb.AppendFormat("Volume contains {0} partitions", unicosSb.s_npart).AppendLine();
            sb.AppendFormat("{0} bytes per sector", unicosSb.s_iounit).AppendLine();
            sb.AppendLine("4096 bytes per block");
            sb.AppendFormat("{0} data blocks in volume", unicosSb.s_fsize).AppendLine();
            sb.AppendFormat("Root resides on inode {0}", unicosSb.s_root).AppendLine();
            sb.AppendFormat("{0} inodes in volume", unicosSb.s_isize).AppendLine();
            sb.AppendFormat("Volume last updated on {0}", DateHandlers.UNIXToDateTime(unicosSb.s_time)).AppendLine();
            if(unicosSb.s_error > 0)
                sb.AppendFormat("Volume is dirty, error code = 0x{0:X16}", unicosSb.s_error).AppendLine();
            sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(unicosSb.s_fname, CurrentEncoding)).AppendLine();

            information = sb.ToString();

            xmlFSType = new Schemas.FileSystemType
            {
                Type = "UNICOS filesystem",
                ClusterSize = 4096,
                Clusters = unicosSb.s_fsize,
                VolumeName = StringHandlers.CToString(unicosSb.s_fname, CurrentEncoding),
                ModificationDate = DateHandlers.UNIXToDateTime(unicosSb.s_time),
                ModificationDateSpecified = true
            };
            xmlFSType.Dirty |= unicosSb.s_error > 0;
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