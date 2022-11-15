// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems;

/// <inheritdoc />
/// <summary>Implements detection of QNX 4 filesystem</summary>
[SuppressMessage("ReSharper", "UnusedType.Local")]
public sealed class QNX4 : IFilesystem
{
    readonly byte[] _rootDirFname =
    {
        0x2F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00
    };

    /// <inheritdoc />
    public FileSystemType XmlFsType { get; private set; }
    /// <inheritdoc />
    public Encoding Encoding { get; private set; }
    /// <inheritdoc />
    public string Name => "QNX4 Plugin";
    /// <inheritdoc />
    public Guid Id => new("E73A63FA-B5B0-48BF-BF82-DA5F0A8170D2");
    /// <inheritdoc />
    public string Author => "Natalia Portillo";

    /// <inheritdoc />
    public bool Identify(IMediaImage imagePlugin, Partition partition)
    {
        if(partition.Start + 1 >= imagePlugin.Info.Sectors)
            return false;

        ErrorNumber errno = imagePlugin.ReadSector(partition.Start + 1, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return false;

        if(sector.Length < 512)
            return false;

        Superblock qnxSb = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

        // Check root directory name
        if(!_rootDirFname.SequenceEqual(qnxSb.rootDir.di_fname))
            return false;

        // Check sizes are multiple of blocks
        if(qnxSb.rootDir.di_size % 512 != 0 ||
           qnxSb.inode.di_size   % 512 != 0 ||
           qnxSb.boot.di_size    % 512 != 0 ||
           qnxSb.altBoot.di_size % 512 != 0)
            return false;

        // Check extents are not past device
        if(qnxSb.rootDir.di_first_xtnt.Block + partition.Start >= partition.End ||
           qnxSb.inode.di_first_xtnt.Block   + partition.Start >= partition.End ||
           qnxSb.boot.di_first_xtnt.Block    + partition.Start >= partition.End ||
           qnxSb.altBoot.di_first_xtnt.Block + partition.Start >= partition.End)
            return false;

        // Check inodes are in use
        return (qnxSb.rootDir.di_status & 0x01) == 0x01 && (qnxSb.inode.di_status & 0x01) == 0x01 &&
               (qnxSb.boot.di_status    & 0x01) == 0x01;

        // All hail filesystems without identification marks
    }

    /// <inheritdoc />
    public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
    {
        Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
        information = "";
        ErrorNumber errno = imagePlugin.ReadSector(partition.Start + 1, out byte[] sector);

        if(errno != ErrorNumber.NoError)
            return;

        if(sector.Length < 512)
            return;

        Superblock qnxSb = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

        // Too much useless information
        /*
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_fname = {0}", CurrentEncoding.GetString(qnxSb.rootDir.di_fname));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_size = {0}", qnxSb.rootDir.di_size);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_first_xtnt.block = {0}", qnxSb.rootDir.di_first_xtnt.block);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_first_xtnt.length = {0}", qnxSb.rootDir.di_first_xtnt.length);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_xblk = {0}", qnxSb.rootDir.di_xblk);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_ftime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.rootDir.di_ftime));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_mtime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.rootDir.di_mtime));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_atime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.rootDir.di_atime));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_ctime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.rootDir.di_ctime));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_num_xtnts = {0}", qnxSb.rootDir.di_num_xtnts);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_mode = {0}", Convert.ToString(qnxSb.rootDir.di_mode, 8));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_uid = {0}", qnxSb.rootDir.di_uid);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_gid = {0}", qnxSb.rootDir.di_gid);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_nlink = {0}", qnxSb.rootDir.di_nlink);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_zero = {0}", qnxSb.rootDir.di_zero);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_type = {0}", qnxSb.rootDir.di_type);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.rootDir.di_status = {0}", qnxSb.rootDir.di_status);

        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_fname = {0}", CurrentEncoding.GetString(qnxSb.inode.di_fname));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_size = {0}", qnxSb.inode.di_size);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_first_xtnt.block = {0}", qnxSb.inode.di_first_xtnt.block);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_first_xtnt.length = {0}", qnxSb.inode.di_first_xtnt.length);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_xblk = {0}", qnxSb.inode.di_xblk);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_ftime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.inode.di_ftime));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_mtime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.inode.di_mtime));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_atime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.inode.di_atime));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_ctime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.inode.di_ctime));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_num_xtnts = {0}", qnxSb.inode.di_num_xtnts);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_mode = {0}", Convert.ToString(qnxSb.inode.di_mode, 8));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_uid = {0}", qnxSb.inode.di_uid);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_gid = {0}", qnxSb.inode.di_gid);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_nlink = {0}", qnxSb.inode.di_nlink);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_zero = {0}", qnxSb.inode.di_zero);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_type = {0}", qnxSb.inode.di_type);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.inode.di_status = {0}", qnxSb.inode.di_status);

        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_fname = {0}", CurrentEncoding.GetString(qnxSb.boot.di_fname));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_size = {0}", qnxSb.boot.di_size);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_first_xtnt.block = {0}", qnxSb.boot.di_first_xtnt.block);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_first_xtnt.length = {0}", qnxSb.boot.di_first_xtnt.length);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_xblk = {0}", qnxSb.boot.di_xblk);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_ftime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.boot.di_ftime));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_mtime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.boot.di_mtime));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_atime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.boot.di_atime));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_ctime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.boot.di_ctime));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_num_xtnts = {0}", qnxSb.boot.di_num_xtnts);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_mode = {0}", Convert.ToString(qnxSb.boot.di_mode, 8));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_uid = {0}", qnxSb.boot.di_uid);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_gid = {0}", qnxSb.boot.di_gid);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_nlink = {0}", qnxSb.boot.di_nlink);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_zero = {0}", qnxSb.boot.di_zero);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_type = {0}", qnxSb.boot.di_type);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.boot.di_status = {0}", qnxSb.boot.di_status);

        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_fname = {0}", CurrentEncoding.GetString(qnxSb.altBoot.di_fname));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_size = {0}", qnxSb.altBoot.di_size);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_first_xtnt.block = {0}", qnxSb.altBoot.di_first_xtnt.block);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_first_xtnt.length = {0}", qnxSb.altBoot.di_first_xtnt.length);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_xblk = {0}", qnxSb.altBoot.di_xblk);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_ftime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.altBoot.di_ftime));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_mtime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.altBoot.di_mtime));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_atime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.altBoot.di_atime));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_ctime = {0}", DateHandlers.UNIXUnsignedToDateTime(qnxSb.altBoot.di_ctime));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_num_xtnts = {0}", qnxSb.altBoot.di_num_xtnts);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_mode = {0}", Convert.ToString(qnxSb.altBoot.di_mode, 8));
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_uid = {0}", qnxSb.altBoot.di_uid);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_gid = {0}", qnxSb.altBoot.di_gid);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_nlink = {0}", qnxSb.altBoot.di_nlink);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_zero = {0}", qnxSb.altBoot.di_zero);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_type = {0}", qnxSb.altBoot.di_type);
        AaruConsole.DebugWriteLine("QNX4 plugin", "qnxSb.altBoot.di_status = {0}", qnxSb.altBoot.di_status);
        */

        information = $"QNX4 filesystem\nCreated on {DateHandlers.UnixUnsignedToDateTime(qnxSb.rootDir.di_ftime)}\n";

        XmlFsType = new FileSystemType
        {
            Type                      = "QNX4 filesystem",
            Clusters                  = partition.Length,
            ClusterSize               = 512,
            CreationDate              = DateHandlers.UnixUnsignedToDateTime(qnxSb.rootDir.di_ftime),
            CreationDateSpecified     = true,
            ModificationDate          = DateHandlers.UnixUnsignedToDateTime(qnxSb.rootDir.di_mtime),
            ModificationDateSpecified = true
        };

        XmlFsType.Bootable |= qnxSb.boot.di_size != 0 || qnxSb.altBoot.di_size != 0;
    }

    struct Extent
    {
        public uint Block;
        public uint Length;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Inode
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public readonly byte[] di_fname;
        public readonly uint   di_size;
        public readonly Extent di_first_xtnt;
        public readonly uint   di_xblk;
        public readonly uint   di_ftime;
        public readonly uint   di_mtime;
        public readonly uint   di_atime;
        public readonly uint   di_ctime;
        public readonly ushort di_num_xtnts;
        public readonly ushort di_mode;
        public readonly ushort di_uid;
        public readonly ushort di_gid;
        public readonly ushort di_nlink;
        public readonly uint   di_zero;
        public readonly byte   di_type;
        public readonly byte   di_status;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct LinkInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
        public readonly byte[] dl_fname;
        public readonly uint dl_inode_blk;
        public readonly byte dl_inode_ndx;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public readonly byte[] dl_spare;
        public readonly byte dl_status;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct ExtentBlock
    {
        public readonly uint next_xblk;
        public readonly uint prev_xblk;
        public readonly byte num_xtnts;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public readonly byte[] spare;
        public readonly uint num_blocks;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 60)]
        public readonly Extent[] xtnts;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public readonly byte[] signature;
        public readonly Extent first_xtnt;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    readonly struct Superblock
    {
        public readonly Inode rootDir;
        public readonly Inode inode;
        public readonly Inode boot;
        public readonly Inode altBoot;
    }
}