﻿// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UNICOS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : UNICOS filesystem plugin.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;

// UNICOS is ILP64 so let's think everything is 64-bit
using blkno_t = System.Int64;
using daddr_t = System.Int64;
using dev_t = System.Int64;
using extent_t = System.Int64;
using ino_t = System.Int64;
using Marshal = Aaru.Helpers.Marshal;
using time_t = System.Int64;

namespace Aaru.Filesystems
{
    /// <inheritdoc />
    /// <summary>Implements detection for the Cray UNICOS filesystem</summary>
    public sealed class UNICOS : IFilesystem
    {
        const int NC1_MAXPART = 64;
        const int NC1_MAXIREG = 4;

        const ulong UNICOS_MAGIC  = 0x6e6331667331636e;
        const ulong UNICOS_SECURE = 0xcd076d1771d670cd;

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding Encoding { get; private set; }
        /// <inheritdoc />
        public string Name => "UNICOS Filesystem Plugin";
        /// <inheritdoc />
        public Guid Id => new Guid("61712F04-066C-44D5-A2A0-1E44C66B33F0");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(imagePlugin.Info.SectorSize < 512)
                return false;

            uint sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

            if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
                sbSize++;

            if(partition.Start + sbSize >= partition.End)
                return false;

            byte[] sector = imagePlugin.ReadSectors(partition.Start, sbSize);

            if(sector.Length < Marshal.SizeOf<Superblock>())
                return false;

            Superblock unicosSb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);

            AaruConsole.DebugWriteLine("UNICOS plugin", "magic = 0x{0:X16} (expected 0x{1:X16})", unicosSb.s_magic,
                                       UNICOS_MAGIC);

            return unicosSb.s_magic == UNICOS_MAGIC;
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";

            if(imagePlugin.Info.SectorSize < 512)
                return;

            uint sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

            if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
                sbSize++;

            byte[] sector = imagePlugin.ReadSectors(partition.Start, sbSize);

            if(sector.Length < Marshal.SizeOf<Superblock>())
                return;

            Superblock unicosSb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);

            if(unicosSb.s_magic != UNICOS_MAGIC)
                return;

            var sb = new StringBuilder();

            sb.AppendLine("UNICOS filesystem");

            if(unicosSb.s_secure == UNICOS_SECURE)
                sb.AppendLine("Volume is secure");

            sb.AppendFormat("Volume contains {0} partitions", unicosSb.s_npart).AppendLine();
            sb.AppendFormat("{0} bytes per sector", unicosSb.s_iounit).AppendLine();
            sb.AppendLine("4096 bytes per block");
            sb.AppendFormat("{0} data blocks in volume", unicosSb.s_fsize).AppendLine();
            sb.AppendFormat("Root resides on inode {0}", unicosSb.s_root).AppendLine();
            sb.AppendFormat("{0} inodes in volume", unicosSb.s_isize).AppendLine();
            sb.AppendFormat("Volume last updated on {0}", DateHandlers.UnixToDateTime(unicosSb.s_time)).AppendLine();

            if(unicosSb.s_error > 0)
                sb.AppendFormat("Volume is dirty, error code = 0x{0:X16}", unicosSb.s_error).AppendLine();

            sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(unicosSb.s_fname, Encoding)).AppendLine();

            information = sb.ToString();

            XmlFsType = new FileSystemType
            {
                Type                      = "UNICOS filesystem",
                ClusterSize               = 4096,
                Clusters                  = (ulong)unicosSb.s_fsize,
                VolumeName                = StringHandlers.CToString(unicosSb.s_fname, Encoding),
                ModificationDate          = DateHandlers.UnixToDateTime(unicosSb.s_time),
                ModificationDateSpecified = true
            };

            XmlFsType.Dirty |= unicosSb.s_error > 0;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1), SuppressMessage("ReSharper", "InconsistentNaming")]
        readonly struct nc1ireg_sb
        {
            public readonly ushort i_unused; /* reserved */
            public readonly ushort i_nblk;   /* number of blocks */
            public readonly uint   i_sblk;   /* start block number */
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1), SuppressMessage("ReSharper", "InconsistentNaming")]
        readonly struct nc1fdev_sb
        {
            public readonly long fd_name; /* Physical device name */
            public readonly uint fd_sblk; /* Start block number */
            public readonly uint fd_nblk; /* Number of blocks */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NC1_MAXIREG)]
            public readonly nc1ireg_sb[] fd_ireg; /* Inode regions */
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1), SuppressMessage("ReSharper", "InconsistentNaming"),
         SuppressMessage("ReSharper", "BuiltInTypeReferenceStyle")]
        readonly struct Superblock
        {
            public readonly ulong s_magic; /* magic number to indicate file system type */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] s_fname; /* file system name */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] s_fpack; /* file system pack name */
            public readonly dev_t s_dev;    /* major/minor device, for verification */

            public readonly daddr_t  s_fsize;   /* size in blocks of entire volume */
            public readonly long     s_isize;   /* Number of total inodes */
            public readonly long     s_bigfile; /* number of bytes at which a file is big */
            public readonly long     s_bigunit; /* minimum number of blocks allocated for big files */
            public readonly ulong    s_secure;  /* security: secure FS label */
            public readonly long     s_maxlvl;  /* security: maximum security level */
            public readonly long     s_minlvl;  /* security: minimum security level */
            public readonly long     s_valcmp;  /* security: valid security compartments */
            public readonly time_t   s_time;    /* last super block update */
            public readonly blkno_t  s_dboff;   /* Dynamic block number */
            public readonly ino_t    s_root;    /* root inode */
            public readonly long     s_error;   /* Type of file system error detected */
            public readonly blkno_t  s_mapoff;  /* Start map block number */
            public readonly long     s_mapblks; /* Last map block number */
            public readonly long     s_nscpys;  /* Number of copies of s.b per partition */
            public readonly long     s_npart;   /* Number of partitions */
            public readonly long     s_ifract;  /* Ratio of inodes to blocks */
            public readonly extent_t s_sfs;     /* SFS only blocks */
            public readonly long     s_flag;    /* Flag word */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NC1_MAXPART)]
            public readonly nc1fdev_sb[] s_part; /* Partition descriptors */
            public readonly long s_iounit;       /* Physical block size */
            public readonly long s_numiresblks;  /* number of inode reservation blocks */
            /* per region (currently 1) */
            /* 0 = 1*(AU) words, n = (n+1)*(AU) words */
            public readonly long s_priparts; /* bitmap of primary partitions */
            public readonly long s_priblock; /* block size of primary partition(s) */
            /* 0 = 1*512 words, n = (n+1)*512 words */
            public readonly long s_prinblks; /* number of 512 wds blocks in primary */
            public readonly long s_secparts; /* bitmap of secondary partitions */
            public readonly long s_secblock; /* block size of secondary partition(s) */
            /* 0 = 1*512 words, n = (n+1)*512 words */
            public readonly long s_secnblks;  /* number of 512 wds blocks in secondary */
            public readonly long s_sbdbparts; /* bitmap of partitions with file system data */
            /* including super blocks, dynamic block */
            /* and free block bitmaps (only primary */
            /* partitions may contain these) */
            public readonly long s_rootdparts; /* bitmap of partitions with root directory */
            /* (only primary partitions) */
            public readonly long s_nudparts; /* bitmap of no-user-data partitions */
            /* (only primary partitions) */
            public readonly long s_nsema;     /* SFS: # fs semaphores to allocate */
            public readonly long s_priactive; /* bitmap of primary partitions which contain */
            /* active (up to date) dynamic blocks and */
            /* free block bitmaps. All bits set indicate */
            /* that all primary partitions are active, */
            /* and no kernel manipulation of active flag */
            /* is allowed. */
            public readonly long s_sfs_arbiterid; /* SFS Arbiter ID */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 91)]
            public readonly long[] s_fill; /* reserved */
        }
    }
}