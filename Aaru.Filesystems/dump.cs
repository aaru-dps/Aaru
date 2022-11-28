// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Console;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;
using ufs_daddr_t = System.Int32;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements identification of a dump(8) image (virtual filesystem on a file)</summary>
[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "UnusedMember.Local")]
public sealed class dump : IFilesystem
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

    /// <summary>Dump tape header</summary>
    const short TS_TAPE = 1;
    /// <summary>Beginning of file record</summary>
    const short TS_INODE = 2;
    /// <summary>Map of inodes on tape</summary>
    const short TS_BITS = 3;
    /// <summary>Continuation of file record</summary>
    const short TS_ADDR = 4;
    /// <summary>Map of inodes deleted since last dump</summary>
    const short TS_END = 5;
    /// <summary>Inode bitmap</summary>
    const short TS_CLRI = 6;
    const short TS_ACL = 7;
    const short TS_PCL = 8;

    const int TP_NINDIR = TP_BSIZE / 2;
    const int LBLSIZE   = 16;
    const int NAMELEN   = 64;

    const int NDADDR = 12;
    const int NIADDR = 3;

    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => Localization.dump_Name;
    /// <inheritdoc />
    public Guid Id => new("E53B4D28-C858-4800-B092-DDAE80D361B9");
    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public string Author => Authors.NataliaPortillo;

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(imagePlugin.Info.SectorSize < 512)
            return false;

        // It should be start of a tape or floppy or file
        if(partition.Start != 0)
            return false;

        uint sbSize = (uint)(Marshal.SizeOf<s_spcl>() / imagePlugin.Info.SectorSize);

        if(Marshal.SizeOf<s_spcl>() % imagePlugin.Info.SectorSize != 0)
            sbSize++;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, sbSize, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        if(sector.Length < Marshal.SizeOf<s_spcl>())
            return false;

        spcl16   oldHdr = Marshal.ByteArrayToStructureLittleEndian<spcl16>(sector);
        spcl_aix aixHdr = Marshal.ByteArrayToStructureLittleEndian<spcl_aix>(sector);
        s_spcl   newHdr = Marshal.ByteArrayToStructureLittleEndian<s_spcl>(sector);

        AaruConsole.DebugWriteLine("dump(8) plugin", "old magic = 0x{0:X8}", oldHdr.c_magic);
        AaruConsole.DebugWriteLine("dump(8) plugin", "aix magic = 0x{0:X8}", aixHdr.c_magic);
        AaruConsole.DebugWriteLine("dump(8) plugin", "new magic = 0x{0:X8}", newHdr.c_magic);

        return oldHdr.c_magic == OFS_MAGIC || aixHdr.c_magic is XIX_MAGIC or XIX_CIGAM || newHdr.c_magic == OFS_MAGIC ||
               newHdr.c_magic == NFS_MAGIC || newHdr.c_magic == OFS_CIGAM || newHdr.c_magic == NFS_CIGAM ||
               newHdr.c_magic == UFS2_MAGIC || newHdr.c_magic == UFS2_CIGAM;
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";

        if(imagePlugin.Info.SectorSize < 512)
            return;

        if(partition.Start != 0)
            return;

        uint sbSize = (uint)(Marshal.SizeOf<s_spcl>() / imagePlugin.Info.SectorSize);

        if(Marshal.SizeOf<s_spcl>() % imagePlugin.Info.SectorSize != 0)
            sbSize++;

        ErrorNumber errno = imagePlugin.ReadSectors(partition.Start, sbSize, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        if(sector.Length < Marshal.SizeOf<s_spcl>())
            return;

        spcl16   oldHdr = Marshal.ByteArrayToStructureLittleEndian<spcl16>(sector);
        spcl_aix aixHdr = Marshal.ByteArrayToStructureLittleEndian<spcl_aix>(sector);
        s_spcl   newHdr = Marshal.ByteArrayToStructureLittleEndian<s_spcl>(sector);

        bool useOld = false;
        bool useAix = false;

        if(newHdr.c_magic == OFS_MAGIC  ||
           newHdr.c_magic == NFS_MAGIC  ||
           newHdr.c_magic == OFS_CIGAM  ||
           newHdr.c_magic == NFS_CIGAM  ||
           newHdr.c_magic == UFS2_MAGIC ||
           newHdr.c_magic == UFS2_CIGAM)
        {
            if(newHdr.c_magic == OFS_CIGAM ||
               newHdr.c_magic == NFS_CIGAM ||
               newHdr.c_magic == UFS2_CIGAM)
                newHdr = Marshal.ByteArrayToStructureBigEndian<s_spcl>(sector);
        }
        else if(aixHdr.c_magic is XIX_MAGIC or XIX_CIGAM)
        {
            useAix = true;

            if(aixHdr.c_magic == XIX_CIGAM)
                aixHdr = Marshal.ByteArrayToStructureBigEndian<spcl_aix>(sector);
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
            information = Localization.Could_not_read_dump_8_header_block;

            return;
        }

        var sb = new StringBuilder();

        XmlFsType = new FileSystemType
        {
            ClusterSize = 1024,
            Clusters    = partition.Size / 1024
        };

        if(useOld)
        {
            XmlFsType.Type = Localization.Old_16_bit_dump_8;
            sb.AppendLine(XmlFsType.Type);

            if(oldHdr.c_date > 0)
            {
                XmlFsType.CreationDate          = DateHandlers.UnixToDateTime(oldHdr.c_date);
                XmlFsType.CreationDateSpecified = true;
                sb.AppendFormat(Localization.Dump_created_on_0, XmlFsType.CreationDate).AppendLine();
            }

            if(oldHdr.c_ddate > 0)
            {
                XmlFsType.BackupDate          = DateHandlers.UnixToDateTime(oldHdr.c_ddate);
                XmlFsType.BackupDateSpecified = true;
                sb.AppendFormat(Localization.Previous_dump_created_on_0, XmlFsType.BackupDate).AppendLine();
            }

            sb.AppendFormat(Localization.Dump_volume_number_0, oldHdr.c_volume).AppendLine();
        }
        else if(useAix)
        {
            XmlFsType.Type = FS_TYPE;
            sb.AppendLine(XmlFsType.Type);

            if(aixHdr.c_date > 0)
            {
                XmlFsType.CreationDate          = DateHandlers.UnixToDateTime(aixHdr.c_date);
                XmlFsType.CreationDateSpecified = true;
                sb.AppendFormat(Localization.Dump_created_on_0, XmlFsType.CreationDate).AppendLine();
            }

            if(aixHdr.c_ddate > 0)
            {
                XmlFsType.BackupDate          = DateHandlers.UnixToDateTime(aixHdr.c_ddate);
                XmlFsType.BackupDateSpecified = true;
                sb.AppendFormat(Localization.Previous_dump_created_on_0, XmlFsType.BackupDate).AppendLine();
            }

            sb.AppendFormat(Localization.Dump_volume_number_0, aixHdr.c_volume).AppendLine();
        }
        else
        {
            XmlFsType.Type = FS_TYPE;
            sb.AppendLine(XmlFsType.Type);

            if(newHdr.c_ndate > 0)
            {
                XmlFsType.CreationDate          = DateHandlers.UnixToDateTime(newHdr.c_ndate);
                XmlFsType.CreationDateSpecified = true;
                sb.AppendFormat(Localization.Dump_created_on_0, XmlFsType.CreationDate).AppendLine();
            }
            else if(newHdr.c_date > 0)
            {
                XmlFsType.CreationDate          = DateHandlers.UnixToDateTime(newHdr.c_date);
                XmlFsType.CreationDateSpecified = true;
                sb.AppendFormat(Localization.Dump_created_on_0, XmlFsType.CreationDate).AppendLine();
            }

            if(newHdr.c_nddate > 0)
            {
                XmlFsType.BackupDate          = DateHandlers.UnixToDateTime(newHdr.c_nddate);
                XmlFsType.BackupDateSpecified = true;
                sb.AppendFormat(Localization.Previous_dump_created_on_0, XmlFsType.BackupDate).AppendLine();
            }
            else if(newHdr.c_ddate > 0)
            {
                XmlFsType.BackupDate          = DateHandlers.UnixToDateTime(newHdr.c_ddate);
                XmlFsType.BackupDateSpecified = true;
                sb.AppendFormat(Localization.Previous_dump_created_on_0, XmlFsType.BackupDate).AppendLine();
            }

            sb.AppendFormat(Localization.Dump_volume_number_0, newHdr.c_volume).AppendLine();
            sb.AppendFormat(Localization.Dump_level_0, newHdr.c_level).AppendLine();
            string dumpname = StringHandlers.CToString(newHdr.c_label);

            if(!string.IsNullOrEmpty(dumpname))
            {
                XmlFsType.VolumeName = dumpname;
                sb.AppendFormat(Localization.Dump_label_0, dumpname).AppendLine();
            }

            string str = StringHandlers.CToString(newHdr.c_filesys);

            if(!string.IsNullOrEmpty(str))
                sb.AppendFormat(Localization.Dumped_filesystem_name_0, str).AppendLine();

            str = StringHandlers.CToString(newHdr.c_dev);

            if(!string.IsNullOrEmpty(str))
                sb.AppendFormat(Localization.Dumped_device_0, str).AppendLine();

            str = StringHandlers.CToString(newHdr.c_host);

            if(!string.IsNullOrEmpty(str))
                sb.AppendFormat(Localization.Dump_hostname_0, str).AppendLine();
        }

        information = sb.ToString();
    }

    const string FS_TYPE = "dump";

    // Old 16-bit format record
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct spcl16
    {
        /// <summary>Record type</summary>
        public readonly short c_type;
        /// <summary>Dump date</summary>
        public int c_date;
        /// <summary>Previous dump date</summary>
        public int c_ddate;
        /// <summary>Dump volume number</summary>
        public readonly short c_volume;
        /// <summary>Logical block of this record</summary>
        public readonly int c_tapea;
        /// <summary>Inode number</summary>
        public readonly ushort c_inumber;
        /// <summary>Magic number</summary>
        public readonly ushort c_magic;
        /// <summary>Record checksum</summary>
        public readonly int c_checksum;

        // Unneeded for now
        /*
        struct dinode  c_dinode;
        int c_count;
        char c_addr[BSIZE];
        */
    }

    // 32-bit AIX format record
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct spcl_aix
    {
        /// <summary>Record type</summary>
        public readonly int c_type;
        /// <summary>Dump date</summary>
        public readonly int c_date;
        /// <summary>Previous dump date</summary>
        public readonly int c_ddate;
        /// <summary>Dump volume number</summary>
        public readonly int c_volume;
        /// <summary>Logical block of this record</summary>
        public readonly int c_tapea;
        public readonly uint c_inumber;
        public readonly uint c_magic;
        public readonly int  c_checksum;

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
    readonly struct s_spcl
    {
        public readonly int    c_type;     /* record type (see below) */
        public readonly int    c_date;     /* date of this dump */
        public readonly int    c_ddate;    /* date of previous dump */
        public readonly int    c_volume;   /* dump volume number */
        public readonly int    c_tapea;    /* logical block of this record */
        public readonly uint   c_inumber;  /* number of inode */
        public readonly int    c_magic;    /* magic number (see above) */
        public readonly int    c_checksum; /* record checksum */
        public readonly dinode c_dinode;   /* ownership and mode of inode */
        public readonly int    c_count;    /* number of valid c_addr entries */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = TP_NINDIR)]
        public readonly byte[] c_addr; /* 1 => data; 0 => hole in inode */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = LBLSIZE)]
        public readonly byte[] c_label; /* dump label */
        public readonly int c_level;    /* level of this dump */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NAMELEN)]
        public readonly byte[] c_filesys; /* name of dumpped file system */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NAMELEN)]
        public readonly byte[] c_dev; /* name of dumpped device */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NAMELEN)]
        public readonly byte[] c_host;    /* name of dumpped host */
        public readonly int  c_flags;     /* additional information */
        public readonly int  c_firstrec;  /* first record on volume */
        public readonly long c_ndate;     /* date of this dump */
        public readonly long c_nddate;    /* date of previous dump */
        public readonly long c_ntapea;    /* logical block of this record */
        public readonly long c_nfirstrec; /* first record on volume */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
        public readonly int[] c_spare; /* reserved for future uses */
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct dinode
    {
        public readonly ushort di_mode;      /*   0: IFMT, permissions; see below. */
        public readonly short  di_nlink;     /*   2: File link count. */
        public readonly int    inumber;      /*   4: Lfs: inode number. */
        public readonly ulong  di_size;      /*   8: File byte count. */
        public readonly int    di_atime;     /*  16: Last access time. */
        public readonly int    di_atimensec; /*  20: Last access time. */
        public readonly int    di_mtime;     /*  24: Last modified time. */
        public readonly int    di_mtimensec; /*  28: Last modified time. */
        public readonly int    di_ctime;     /*  32: Last inode change time. */
        public readonly int    di_ctimensec; /*  36: Last inode change time. */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NDADDR)]
        public readonly int[] di_db; /*  40: Direct disk blocks. */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = NIADDR)]
        public readonly int[] di_ib;    /*  88: Indirect disk blocks. */
        public readonly uint di_flags;  /* 100: Status flags (chflags). */
        public readonly uint di_blocks; /* 104: Blocks actually held. */
        public readonly int  di_gen;    /* 108: Generation number. */
        public readonly uint di_uid;    /* 112: File owner. */
        public readonly uint di_gid;    /* 116: File group. */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public readonly int[] di_spare; /* 120: Reserved; currently unused */
    }
}