// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : dump.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : dump(8) file system plugin
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies backups created with dump(8) shows information.
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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Console;
using Schemas;
using ufs_daddr_t = System.Int32;

namespace DiscImageChef.Filesystems
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class dump : IFilesystem
    {
        /// <summary>Magic number for old dump</summary>
        const ushort OFS_MAGIC = 60011;
        /// <summary>Magic number for new dump</summary>
        const uint NFS_MAGIC = 60012;
        /// <summary>Magic number for AIX dump</summary>
        const uint XIX_MAGIC = 60013;
        /// <summary>Magic number for UFS2 dump</summary>
        const uint UFS2_MAGIC = 0x19540119;
        /// <summary>Magic number for old dump</summary>
        const uint OFS_CIGAM = 0x6BEA0000;
        /// <summary>Magic number for new dump</summary>
        const uint NFS_CIGAM = 0x6CEA0000;
        /// <summary>Magic number for AIX dump</summary>
        const uint XIX_CIGAM = 0x6DEA0000;
        /// <summary>Magic number for UFS2 dump</summary>
        const uint UFS2_CIGAM = 0x19015419;

        const int TP_BSIZE = 1024;

        /// <summary>
        ///     Dump tape header
        /// </summary>
        const short TS_TAPE = 1;
        /// <summary>
        ///     Beginning of file record
        /// </summary>
        const short TS_INODE = 2;
        /// <summary>
        ///     Map of inodes on tape
        /// </summary>
        const short TS_BITS = 3;
        /// <summary>
        ///     Continuation of file record
        /// </summary>
        const short TS_ADDR = 4;
        /// <summary>
        ///     Map of inodes deleted since last dump
        /// </summary>
        const short TS_END = 5;
        /// <summary>
        ///     Inode bitmap
        /// </summary>
        const short TS_CLRI = 6;
        const short TS_ACL = 7;
        const short TS_PCL = 8;

        const int TP_NINDIR = TP_BSIZE / 2;
        const int LBLSIZE   = 16;
        const int NAMELEN   = 64;

        const int NDADDR = 12;
        const int NIADDR = 3;

        public Encoding       Encoding  { get; private set; }
        public string         Name      => "dump(8) Plugin";
        public Guid           Id        => new Guid("E53B4D28-C858-4800-B092-DDAE80D361B9");
        public FileSystemType XmlFsType { get; private set; }
        public string         Author    => "Natalia Portillo";

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(imagePlugin.Info.SectorSize < 512) return false;

            // It should be start of a tape or floppy or file
            if(partition.Start != 0) return false;

            spcl16   oldHdr = new spcl16();
            spcl_aix aixHdr = new spcl_aix();
            s_spcl   newHdr = new s_spcl();

            uint sbSize = (uint)(Marshal.SizeOf(newHdr) / imagePlugin.Info.SectorSize);
            if(Marshal.SizeOf(newHdr) % imagePlugin.Info.SectorSize != 0) sbSize++;

            byte[] sector = imagePlugin.ReadSectors(partition.Start, sbSize);
            if(sector.Length < Marshal.SizeOf(newHdr)) return false;

            IntPtr oldPtr = Marshal.AllocHGlobal(Marshal.SizeOf(oldHdr));
            Marshal.Copy(sector, 0, oldPtr, Marshal.SizeOf(oldHdr));
            oldHdr = (spcl16)Marshal.PtrToStructure(oldPtr, typeof(spcl16));
            Marshal.FreeHGlobal(oldPtr);

            IntPtr aixPtr = Marshal.AllocHGlobal(Marshal.SizeOf(aixHdr));
            Marshal.Copy(sector, 0, aixPtr, Marshal.SizeOf(aixHdr));
            aixHdr = (spcl_aix)Marshal.PtrToStructure(aixPtr, typeof(spcl_aix));
            Marshal.FreeHGlobal(aixPtr);

            IntPtr newPtr = Marshal.AllocHGlobal(Marshal.SizeOf(newHdr));
            Marshal.Copy(sector, 0, newPtr, Marshal.SizeOf(newHdr));
            newHdr = (s_spcl)Marshal.PtrToStructure(newPtr, typeof(s_spcl));
            Marshal.FreeHGlobal(newPtr);

            DicConsole.DebugWriteLine("dump(8) plugin", "old magic = 0x{0:X8}", oldHdr.c_magic);
            DicConsole.DebugWriteLine("dump(8) plugin", "aix magic = 0x{0:X8}", aixHdr.c_magic);
            DicConsole.DebugWriteLine("dump(8) plugin", "new magic = 0x{0:X8}", newHdr.c_magic);

            return oldHdr.c_magic == OFS_MAGIC || aixHdr.c_magic == XIX_MAGIC  || aixHdr.c_magic == XIX_CIGAM ||
                   newHdr.c_magic == OFS_MAGIC || newHdr.c_magic == NFS_MAGIC  || newHdr.c_magic == OFS_CIGAM ||
                   newHdr.c_magic == NFS_CIGAM || newHdr.c_magic == UFS2_MAGIC || newHdr.c_magic == UFS2_CIGAM;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding    encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";
            if(imagePlugin.Info.SectorSize < 512) return;

            if(partition.Start != 0) return;

            spcl16   oldHdr = new spcl16();
            spcl_aix aixHdr = new spcl_aix();
            s_spcl   newHdr = new s_spcl();

            uint sbSize = (uint)(Marshal.SizeOf(newHdr) / imagePlugin.Info.SectorSize);
            if(Marshal.SizeOf(newHdr) % imagePlugin.Info.SectorSize != 0) sbSize++;

            byte[] sector = imagePlugin.ReadSectors(partition.Start, sbSize);
            if(sector.Length < Marshal.SizeOf(newHdr)) return;

            IntPtr oldPtr = Marshal.AllocHGlobal(Marshal.SizeOf(oldHdr));
            Marshal.Copy(sector, 0, oldPtr, Marshal.SizeOf(oldHdr));
            oldHdr = (spcl16)Marshal.PtrToStructure(oldPtr, typeof(spcl16));
            Marshal.FreeHGlobal(oldPtr);

            IntPtr aixPtr = Marshal.AllocHGlobal(Marshal.SizeOf(aixHdr));
            Marshal.Copy(sector, 0, aixPtr, Marshal.SizeOf(aixHdr));
            aixHdr = (spcl_aix)Marshal.PtrToStructure(aixPtr, typeof(spcl_aix));
            Marshal.FreeHGlobal(aixPtr);

            IntPtr newPtr = Marshal.AllocHGlobal(Marshal.SizeOf(newHdr));
            Marshal.Copy(sector, 0, newPtr, Marshal.SizeOf(newHdr));
            newHdr = (s_spcl)Marshal.PtrToStructure(newPtr, typeof(s_spcl));
            Marshal.FreeHGlobal(newPtr);

            bool useOld = false;
            bool useAix = false;

            if(newHdr.c_magic == OFS_MAGIC || newHdr.c_magic == NFS_MAGIC  || newHdr.c_magic == OFS_CIGAM ||
               newHdr.c_magic == NFS_CIGAM || newHdr.c_magic == UFS2_MAGIC || newHdr.c_magic == UFS2_CIGAM)
            {
                if(newHdr.c_magic == OFS_CIGAM || newHdr.c_magic == NFS_CIGAM || newHdr.c_magic == UFS2_CIGAM)
                    newHdr = BigEndianMarshal.ByteArrayToStructureBigEndian<s_spcl>(sector);
            }
            else if(aixHdr.c_magic == XIX_MAGIC || aixHdr.c_magic == XIX_CIGAM)
            {
                useAix = true;

                if(aixHdr.c_magic == XIX_CIGAM)
                    aixHdr = BigEndianMarshal.ByteArrayToStructureBigEndian<spcl_aix>(sector);
            }
            else if(oldHdr.c_magic == OFS_MAGIC)
            {
                useOld = true;

                // Swap PDP-11 endian
                oldHdr.c_date  = (int)Swapping.PDPFromLittleEndian((uint)oldHdr.c_date);
                oldHdr.c_ddate = (int)Swapping.PDPFromLittleEndian((uint)oldHdr.c_ddate);
            }
            else
            {
                information = "Could not read dump(8) header block";
                return;
            }

            StringBuilder sb = new StringBuilder();

            XmlFsType = new FileSystemType {ClusterSize = 1024, Clusters = (long)(partition.Size / 1024)};

            if(useOld)
            {
                XmlFsType.Type = "Old 16-bit dump(8)";
                sb.AppendLine(XmlFsType.Type);
                if(oldHdr.c_date > 0)
                {
                    XmlFsType.CreationDate          = DateHandlers.UnixToDateTime(oldHdr.c_date);
                    XmlFsType.CreationDateSpecified = true;
                    sb.AppendFormat("Dump created on {0}", XmlFsType.CreationDate).AppendLine();
                }

                if(oldHdr.c_ddate > 0)
                {
                    XmlFsType.BackupDate          = DateHandlers.UnixToDateTime(oldHdr.c_ddate);
                    XmlFsType.BackupDateSpecified = true;
                    sb.AppendFormat("Previous dump created on {0}", XmlFsType.BackupDate).AppendLine();
                }

                sb.AppendFormat("Dump volume number: {0}", oldHdr.c_volume).AppendLine();
            }
            else if(useAix)
            {
                XmlFsType.Type = "AIX dump(8)";
                sb.AppendLine(XmlFsType.Type);
                if(aixHdr.c_date > 0)
                {
                    XmlFsType.CreationDate          = DateHandlers.UnixToDateTime(aixHdr.c_date);
                    XmlFsType.CreationDateSpecified = true;
                    sb.AppendFormat("Dump created on {0}", XmlFsType.CreationDate).AppendLine();
                }

                if(aixHdr.c_ddate > 0)
                {
                    XmlFsType.BackupDate          = DateHandlers.UnixToDateTime(aixHdr.c_ddate);
                    XmlFsType.BackupDateSpecified = true;
                    sb.AppendFormat("Previous dump created on {0}", XmlFsType.BackupDate).AppendLine();
                }

                sb.AppendFormat("Dump volume number: {0}", aixHdr.c_volume).AppendLine();
            }
            else
            {
                XmlFsType.Type = "dump(8)";
                sb.AppendLine(XmlFsType.Type);
                if(newHdr.c_ndate > 0)
                {
                    XmlFsType.CreationDate          = DateHandlers.UnixToDateTime(newHdr.c_ndate);
                    XmlFsType.CreationDateSpecified = true;
                    sb.AppendFormat("Dump created on {0}", XmlFsType.CreationDate).AppendLine();
                }
                else if(newHdr.c_date > 0)
                {
                    XmlFsType.CreationDate          = DateHandlers.UnixToDateTime(newHdr.c_date);
                    XmlFsType.CreationDateSpecified = true;
                    sb.AppendFormat("Dump created on {0}", XmlFsType.CreationDate).AppendLine();
                }

                if(newHdr.c_nddate > 0)
                {
                    XmlFsType.BackupDate          = DateHandlers.UnixToDateTime(newHdr.c_nddate);
                    XmlFsType.BackupDateSpecified = true;
                    sb.AppendFormat("Previous dump created on {0}", XmlFsType.BackupDate).AppendLine();
                }
                else if(newHdr.c_ddate > 0)
                {
                    XmlFsType.BackupDate          = DateHandlers.UnixToDateTime(newHdr.c_ddate);
                    XmlFsType.BackupDateSpecified = true;
                    sb.AppendFormat("Previous dump created on {0}", XmlFsType.BackupDate).AppendLine();
                }

                sb.AppendFormat("Dump volume number: {0}", newHdr.c_volume).AppendLine();
                sb.AppendFormat("Dump level: {0}", newHdr.c_level).AppendLine();
                string dumpname = StringHandlers.CToString(newHdr.c_label);
                if(!string.IsNullOrEmpty(dumpname))
                {
                    XmlFsType.VolumeName = dumpname;
                    sb.AppendFormat("Dump label: {0}", dumpname).AppendLine();
                }

                string str = StringHandlers.CToString(newHdr.c_filesys);
                if(!string.IsNullOrEmpty(str)) sb.AppendFormat("Dumped filesystem name: {0}", str).AppendLine();
                str = StringHandlers.CToString(newHdr.c_dev);
                if(!string.IsNullOrEmpty(str)) sb.AppendFormat("Dumped device: {0}", str).AppendLine();
                str = StringHandlers.CToString(newHdr.c_host);
                if(!string.IsNullOrEmpty(str)) sb.AppendFormat("Dump hostname: {0}", str).AppendLine();
            }

            information = sb.ToString();
        }

        // Old 16-bit format record
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct spcl16
        {
            /// <summary>Record type</summary>
            public short c_type;
            /// <summary>Dump date</summary>
            public int c_date;
            /// <summary>Previous dump date</summary>
            public int c_ddate;
            /// <summary>Dump volume number</summary>
            public short c_volume;
            /// <summary>Logical block of this record</summary>
            public int c_tapea;
            /// <summary>Inode number</summary>
            public ushort c_inumber;
            /// <summary>Magic number</summary>
            public ushort c_magic;
            /// <summary>Record checksum</summary>
            public int c_checksum;
            // Unneeded for now
            /*
            struct dinode  c_dinode;
            int c_count;
            char c_addr[BSIZE];
            */
        }

        // 32-bit AIX format record
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct spcl_aix
        {
            /// <summary>Record type</summary>
            public int c_type;
            /// <summary>Dump date</summary>
            public int c_date;
            /// <summary>Previous dump date</summary>
            public int c_ddate;
            /// <summary>Dump volume number</summary>
            public int c_volume;
            /// <summary>Logical block of this record</summary>
            public int c_tapea;
            public uint c_inumber;
            public uint c_magic;
            public int  c_checksum;
            // Unneeded for now
            /*
            public bsd_dinode  bsd_c_dinode;
            public int c_count;
            public char c_addr[TP_NINDIR];
            public int xix_flag;
            public dinode xix_dinode;
            */
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct s_spcl
        {
            public int    c_type;     /* record type (see below) */
            public int    c_date;     /* date of this dump */
            public int    c_ddate;    /* date of previous dump */
            public int    c_volume;   /* dump volume number */
            public int    c_tapea;    /* logical block of this record */
            public uint   c_inumber;  /* number of inode */
            public int    c_magic;    /* magic number (see above) */
            public int    c_checksum; /* record checksum */
            public dinode c_dinode;   /* ownership and mode of inode */
            public int    c_count;    /* number of valid c_addr entries */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = TP_NINDIR)]
            public byte[] c_addr; /* 1 => data; 0 => hole in inode */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = LBLSIZE)]
            public byte[] c_label; /* dump label */
            public int c_level;    /* level of this dump */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NAMELEN)]
            public byte[] c_filesys; /* name of dumpped file system */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NAMELEN)]
            public byte[] c_dev; /* name of dumpped device */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NAMELEN)]
            public byte[] c_host;    /* name of dumpped host */
            public int  c_flags;     /* additional information */
            public int  c_firstrec;  /* first record on volume */
            public long c_ndate;     /* date of this dump */
            public long c_nddate;    /* date of previous dump */
            public long c_ntapea;    /* logical block of this record */
            public long c_nfirstrec; /* first record on volume */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public int[] c_spare; /* reserved for future uses */
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct dinode
        {
            public ushort di_mode;      /*   0: IFMT, permissions; see below. */
            public short  di_nlink;     /*   2: File link count. */
            public int    inumber;      /*   4: Lfs: inode number. */
            public ulong  di_size;      /*   8: File byte count. */
            public int    di_atime;     /*  16: Last access time. */
            public int    di_atimensec; /*  20: Last access time. */
            public int    di_mtime;     /*  24: Last modified time. */
            public int    di_mtimensec; /*  28: Last modified time. */
            public int    di_ctime;     /*  32: Last inode change time. */
            public int    di_ctimensec; /*  36: Last inode change time. */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NDADDR)]
            public int[] di_db; /*  40: Direct disk blocks. */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NIADDR)]
            public int[] di_ib;    /*  88: Indirect disk blocks. */
            public uint di_flags;  /* 100: Status flags (chflags). */
            public uint di_blocks; /* 104: Blocks actually held. */
            public int  di_gen;    /* 108: Generation number. */
            public uint di_uid;    /* 112: File owner. */
            public uint di_gid;    /* 116: File group. */
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public int[] di_spare; /* 120: Reserved; currently unused */
        }
    }
}