// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Locus.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Locus filesystem plugin
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Locus filesystem and shows information.
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
//     License aint with this library; if not, see <http://www.gnu.org/licenses/>.
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
using Aaru.Console;
using Aaru.Helpers;
using Schemas;

// Commit count
using commitcnt_t = System.Int32;

// Disk address
using daddr_t = System.Int32;

// Fstore
using fstore_t = System.Int32;

// Global File System number
using gfs_t = System.Int32;

// Inode number
using ino_t = System.Int32;
using Marshal = Aaru.Helpers.Marshal;

// Filesystem pack number
using pckno_t = System.Int16;

// Timestamp
using time_t = System.Int32;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedType.Local

namespace Aaru.Filesystems
{
    /// <inheritdoc />
    /// <summary>
    /// Implements detection of the Locus filesystem
    /// </summary>
    public sealed class Locus : IFilesystem
    {
        const int NICINOD    = 325;
        const int NICFREE    = 600;
        const int OLDNICINOD = 700;
        const int OLDNICFREE = 500;

        const uint LOCUS_MAGIC     = 0xFFEEDDCD;
        const uint LOCUS_CIGAM     = 0xCDDDEEFF;
        const uint LOCUS_MAGIC_OLD = 0xFFEEDDCC;
        const uint LOCUS_CIGAM_OLD = 0xCCDDEEFF;

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding       Encoding  { get; private set; }
        /// <inheritdoc />
        public string         Name      => "Locus Filesystem Plugin";
        /// <inheritdoc />
        public Guid           Id        => new Guid("1A70B30A-437D-479A-88E1-D0C9C1797FF4");
        /// <inheritdoc />
        public string         Author    => "Natalia Portillo";

        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(imagePlugin.Info.SectorSize < 512)
                return false;

            for(ulong location = 0; location <= 8; location++)
            {
                uint sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

                if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
                    sbSize++;

                if(partition.Start + location + sbSize >= imagePlugin.Info.Sectors)
                    break;

                byte[] sector = imagePlugin.ReadSectors(partition.Start + location, sbSize);

                if(sector.Length < Marshal.SizeOf<Superblock>())
                    return false;

                Superblock locusSb = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

                AaruConsole.DebugWriteLine("Locus plugin", "magic at {1} = 0x{0:X8}", locusSb.s_magic, location);

                if(locusSb.s_magic == LOCUS_MAGIC     ||
                   locusSb.s_magic == LOCUS_CIGAM     ||
                   locusSb.s_magic == LOCUS_MAGIC_OLD ||
                   locusSb.s_magic == LOCUS_CIGAM_OLD)
                    return true;
            }

            return false;
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";

            if(imagePlugin.Info.SectorSize < 512)
                return;

            var    locusSb = new Superblock();
            byte[] sector  = null;

            for(ulong location = 0; location <= 8; location++)
            {
                uint sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

                if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
                    sbSize++;

                sector = imagePlugin.ReadSectors(partition.Start + location, sbSize);

                if(sector.Length < Marshal.SizeOf<Superblock>())
                    return;

                locusSb = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

                if(locusSb.s_magic == LOCUS_MAGIC     ||
                   locusSb.s_magic == LOCUS_CIGAM     ||
                   locusSb.s_magic == LOCUS_MAGIC_OLD ||
                   locusSb.s_magic == LOCUS_CIGAM_OLD)
                    break;
            }

            // We don't care about old version for information
            if(locusSb.s_magic != LOCUS_MAGIC     &&
               locusSb.s_magic != LOCUS_CIGAM     &&
               locusSb.s_magic != LOCUS_MAGIC_OLD &&
               locusSb.s_magic != LOCUS_CIGAM_OLD)
                return;

            // Numerical arrays are not important for information so no need to swap them
            if(locusSb.s_magic == LOCUS_CIGAM ||
               locusSb.s_magic == LOCUS_CIGAM_OLD)
            {
                locusSb         = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);
                locusSb.s_flags = (Flags)Swapping.Swap((ushort)locusSb.s_flags);
            }

            var sb = new StringBuilder();

            sb.AppendLine(locusSb.s_magic == LOCUS_MAGIC_OLD ? "Locus filesystem (old)" : "Locus filesystem");

            int blockSize = locusSb.s_version == Version.SB_SB4096 ? 4096 : 1024;

            string s_fsmnt = StringHandlers.CToString(locusSb.s_fsmnt, Encoding);
            string s_fpack = StringHandlers.CToString(locusSb.s_fpack, Encoding);

            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_magic = 0x{0:X8}", locusSb.s_magic);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_gfs = {0}", locusSb.s_gfs);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_fsize = {0}", locusSb.s_fsize);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_lwm = {0}", locusSb.s_lwm);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_hwm = {0}", locusSb.s_hwm);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_llst = {0}", locusSb.s_llst);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_fstore = {0}", locusSb.s_fstore);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_time = {0}", locusSb.s_time);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_tfree = {0}", locusSb.s_tfree);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_isize = {0}", locusSb.s_isize);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_nfree = {0}", locusSb.s_nfree);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_flags = {0}", locusSb.s_flags);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_tinode = {0}", locusSb.s_tinode);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_lasti = {0}", locusSb.s_lasti);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_nbehind = {0}", locusSb.s_nbehind);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_gfspack = {0}", locusSb.s_gfspack);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_ninode = {0}", locusSb.s_ninode);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_flock = {0}", locusSb.s_flock);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_ilock = {0}", locusSb.s_ilock);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_fmod = {0}", locusSb.s_fmod);
            AaruConsole.DebugWriteLine("Locus plugin", "LocusSb.s_version = {0}", locusSb.s_version);

            sb.AppendFormat("Superblock last modified on {0}", DateHandlers.UnixToDateTime(locusSb.s_time)).
               AppendLine();

            sb.AppendFormat("Volume has {0} blocks of {1} bytes each (total {2} bytes)", locusSb.s_fsize, blockSize,
                            locusSb.s_fsize * blockSize).AppendLine();

            sb.AppendFormat("{0} blocks free ({1} bytes)", locusSb.s_tfree, locusSb.s_tfree * blockSize).AppendLine();
            sb.AppendFormat("I-node list uses {0} blocks", locusSb.s_isize).AppendLine();
            sb.AppendFormat("{0} free inodes", locusSb.s_tinode).AppendLine();
            sb.AppendFormat("Next free inode search will start at inode {0}", locusSb.s_lasti).AppendLine();

            sb.AppendFormat("There are an estimate of {0} free inodes before next search start", locusSb.s_nbehind).
               AppendLine();

            if(locusSb.s_flags.HasFlag(Flags.SB_RDONLY))
                sb.AppendLine("Read-only volume");

            if(locusSb.s_flags.HasFlag(Flags.SB_CLEAN))
                sb.AppendLine("Clean volume");

            if(locusSb.s_flags.HasFlag(Flags.SB_DIRTY))
                sb.AppendLine("Dirty volume");

            if(locusSb.s_flags.HasFlag(Flags.SB_RMV))
                sb.AppendLine("Removable volume");

            if(locusSb.s_flags.HasFlag(Flags.SB_PRIMPACK))
                sb.AppendLine("This is the primary pack");

            if(locusSb.s_flags.HasFlag(Flags.SB_REPLTYPE))
                sb.AppendLine("Replicated volume");

            if(locusSb.s_flags.HasFlag(Flags.SB_USER))
                sb.AppendLine("User replicated volume");

            if(locusSb.s_flags.HasFlag(Flags.SB_BACKBONE))
                sb.AppendLine("Backbone volume");

            if(locusSb.s_flags.HasFlag(Flags.SB_NFS))
                sb.AppendLine("NFS volume");

            if(locusSb.s_flags.HasFlag(Flags.SB_BYHAND))
                sb.AppendLine("Volume inhibits automatic fsck");

            if(locusSb.s_flags.HasFlag(Flags.SB_NOSUID))
                sb.AppendLine("Set-uid/set-gid is disabled");

            if(locusSb.s_flags.HasFlag(Flags.SB_SYNCW))
                sb.AppendLine("Volume uses synchronous writes");

            sb.AppendFormat("Volume label: {0}", s_fsmnt).AppendLine();
            sb.AppendFormat("Physical volume name: {0}", s_fpack).AppendLine();
            sb.AppendFormat("Global File System number: {0}", locusSb.s_gfs).AppendLine();
            sb.AppendFormat("Global File System pack number {0}", locusSb.s_gfspack).AppendLine();

            information = sb.ToString();

            XmlFsType = new FileSystemType
            {
                Type        = "Locus filesystem",
                ClusterSize = (uint)blockSize,
                Clusters    = (ulong)locusSb.s_fsize,

                // Sometimes it uses one, or the other. Use the bigger
                VolumeName = string.IsNullOrEmpty(s_fsmnt) ? s_fpack : s_fsmnt,
                ModificationDate = DateHandlers.UnixToDateTime(locusSb.s_time),
                ModificationDateSpecified = true,
                Dirty = !locusSb.s_flags.HasFlag(Flags.SB_CLEAN) || locusSb.s_flags.HasFlag(Flags.SB_DIRTY),
                FreeClusters = (ulong)locusSb.s_tfree,
                FreeClustersSpecified = true
            };
        }

        [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "BuiltInTypeReferenceStyle"),
         StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Superblock
        {
            public readonly uint s_magic; /* identifies this as a locus filesystem */
            /* defined as a constant below */
            public readonly gfs_t   s_gfs;   /* global filesystem number */
            public readonly daddr_t s_fsize; /* size in blocks of entire volume */
            /* several ints for replicated filesystems */
            public readonly commitcnt_t s_lwm; /* all prior commits propagated */
            public readonly commitcnt_t s_hwm; /* highest commit propagated */
            /* oldest committed version in the list.
             * llst mod NCMTLST is the offset of commit #llst in the list,
             * which wraps around from there.
             */
            public readonly commitcnt_t s_llst;
            public readonly fstore_t s_fstore; /* filesystem storage bit mask; if the
                   filsys is replicated and this is not a
                   primary or backbone copy, this bit mask
                   determines which files are stored */

            public readonly time_t  s_time;  /* last super block update */
            public readonly daddr_t s_tfree; /* total free blocks*/

            public readonly ino_t   s_isize;   /* size in blocks of i-list */
            public readonly short   s_nfree;   /* number of addresses in s_free */
            public          Flags   s_flags;   /* filsys flags, defined below */
            public readonly ino_t   s_tinode;  /* total free inodes */
            public readonly ino_t   s_lasti;   /* start place for circular search */
            public readonly ino_t   s_nbehind; /* est # free inodes before s_lasti */
            public readonly pckno_t s_gfspack; /* global filesystem pack number */
            public readonly short   s_ninode;  /* number of i-nodes in s_inode */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public readonly short[] s_dinfo; /* interleave stuff */

            //#define s_m s_dinfo[0]
            //#define s_skip  s_dinfo[0]      /* AIX defines  */
            //#define s_n s_dinfo[1]
            //#define s_cyl   s_dinfo[1]      /* AIX defines  */
            public readonly byte    s_flock;   /* lock during free list manipulation */
            public readonly byte    s_ilock;   /* lock during i-list manipulation */
            public readonly byte    s_fmod;    /* super block modified flag */
            public readonly Version s_version; /* version of the data format in fs. */
            /*  defined below. */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public readonly byte[] s_fsmnt; /* name of this file system */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] s_fpack; /* name of this physical volume */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NICINOD)]
            public readonly ino_t[] s_inode; /* free i-node list */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NICFREE)]
            public readonly daddr_t[] su_free; /* free block list for non-replicated filsys */
            public readonly byte s_byteorder;  /* byte order of integers */
        }

        [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "BuiltInTypeReferenceStyle"),
         StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct OldSuperblock
        {
            public readonly uint s_magic; /* identifies this as a locus filesystem */
            /* defined as a constant below */
            public readonly gfs_t   s_gfs;   /* global filesystem number */
            public readonly daddr_t s_fsize; /* size in blocks of entire volume */
            /* several ints for replicated filsystems */
            public readonly commitcnt_t s_lwm; /* all prior commits propagated */
            public readonly commitcnt_t s_hwm; /* highest commit propagated */
            /* oldest committed version in the list.
             * llst mod NCMTLST is the offset of commit #llst in the list,
             * which wraps around from there.
             */
            public readonly commitcnt_t s_llst;
            public readonly fstore_t s_fstore; /* filesystem storage bit mask; if the
                   filsys is replicated and this is not a
                   primary or backbone copy, this bit mask
                   determines which files are stored */

            public readonly time_t  s_time;  /* last super block update */
            public readonly daddr_t s_tfree; /* total free blocks*/

            public readonly ino_t   s_isize;   /* size in blocks of i-list */
            public readonly short   s_nfree;   /* number of addresses in s_free */
            public readonly Flags   s_flags;   /* filsys flags, defined below */
            public readonly ino_t   s_tinode;  /* total free inodes */
            public readonly ino_t   s_lasti;   /* start place for circular search */
            public readonly ino_t   s_nbehind; /* est # free inodes before s_lasti */
            public readonly pckno_t s_gfspack; /* global filesystem pack number */
            public readonly short   s_ninode;  /* number of i-nodes in s_inode */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public readonly short[] s_dinfo; /* interleave stuff */

            //#define s_m s_dinfo[0]
            //#define s_skip  s_dinfo[0]      /* AIX defines  */
            //#define s_n s_dinfo[1]
            //#define s_cyl   s_dinfo[1]      /* AIX defines  */
            public readonly byte    s_flock;   /* lock during free list manipulation */
            public readonly byte    s_ilock;   /* lock during i-list manipulation */
            public readonly byte    s_fmod;    /* super block modified flag */
            public readonly Version s_version; /* version of the data format in fs. */
            /*  defined below. */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public readonly byte[] s_fsmnt; /* name of this file system */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] s_fpack; /* name of this physical volume */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = OLDNICINOD)]
            public readonly ino_t[] s_inode; /* free i-node list */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = OLDNICFREE)]
            public readonly daddr_t[] su_free; /* free block list for non-replicated filsys */
            public readonly byte s_byteorder;  /* byte order of integers */
        }

        [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "BuiltInTypeReferenceStyle"),
         Flags]
        enum Flags : ushort
        {
            SB_RDONLY   = 0x1, /* no writes on filesystem */ SB_CLEAN = 0x2, /* fs unmounted cleanly (or checks run) */
            SB_DIRTY    = 0x4, /* fs mounted without CLEAN bit set */ SB_RMV = 0x8, /* fs is a removable file system */
            SB_PRIMPACK = 0x10, /* This is the primary pack of the filesystem */
            SB_REPLTYPE = 0x20, /* This is a replicated type filesystem. */
            SB_USER     = 0x40, /* This is a "user" replicated filesystem. */
            SB_BACKBONE = 0x80, /* backbone pack ; complete copy of primary pack but not modifiable */
            SB_NFS      = 0x100, /* This is a NFS type filesystem */
            SB_BYHAND   = 0x200, /* Inhibits automatic fscks on a mangled file system */
            SB_NOSUID   = 0x400, /* Set-uid/Set-gid is disabled */ SB_SYNCW = 0x800 /* Synchronous Write */
        }

        [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "BuiltInTypeReferenceStyle"),
         Flags]
        enum Version : byte
        {
            SB_SB4096 = 1, /* smallblock filesys with 4096 byte blocks */ SB_B1024 = 2, /* 1024 byte block filesystem */
            NUMSCANDEV = 5 /* Used by scangfs(), refed in space.h */
        }
    }
}