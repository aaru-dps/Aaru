// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : F2FS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : F2FS filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the F2FS filesystem and shows information.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems
{
    /// <inheritdoc />
    /// <summary>Implements detection of the Flash-Friendly File System (F2FS)</summary>
    [SuppressMessage("ReSharper", "UnusedMember.Local")]
    public sealed class F2FS : IFilesystem
    {
        // ReSharper disable InconsistentNaming
        const uint F2FS_MAGIC        = 0xF2F52010;
        const uint F2FS_SUPER_OFFSET = 1024;
        const uint F2FS_MIN_SECTOR   = 512;
        const uint F2FS_MAX_SECTOR   = 4096;
        const uint F2FS_BLOCK_SIZE   = 4096;

        // ReSharper restore InconsistentNaming

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding Encoding { get; private set; }
        /// <inheritdoc />
        public string Name => "F2FS Plugin";
        /// <inheritdoc />
        public Guid Id => new Guid("82B0920F-5F0D-4063-9F57-ADE0AE02ECE5");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(imagePlugin.Info.SectorSize < F2FS_MIN_SECTOR ||
               imagePlugin.Info.SectorSize > F2FS_MAX_SECTOR)
                return false;

            uint sbAddr = F2FS_SUPER_OFFSET / imagePlugin.Info.SectorSize;

            if(sbAddr == 0)
                sbAddr = 1;

            uint sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

            if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
                sbSize++;

            if(partition.Start + sbAddr + sbSize >= partition.End)
                return false;

            byte[] sector = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize);

            if(sector.Length < Marshal.SizeOf<Superblock>())
                return false;

            Superblock sb = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

            return sb.magic == F2FS_MAGIC;
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = Encoding.Unicode;
            information = "";

            if(imagePlugin.Info.SectorSize < F2FS_MIN_SECTOR ||
               imagePlugin.Info.SectorSize > F2FS_MAX_SECTOR)
                return;

            uint sbAddr = F2FS_SUPER_OFFSET / imagePlugin.Info.SectorSize;

            if(sbAddr == 0)
                sbAddr = 1;

            uint sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

            if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
                sbSize++;

            byte[] sector = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize);

            if(sector.Length < Marshal.SizeOf<Superblock>())
                return;

            // ReSharper disable once InconsistentNaming
            Superblock f2fsSb = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

            if(f2fsSb.magic != F2FS_MAGIC)
                return;

            var sb = new StringBuilder();

            sb.AppendLine("F2FS filesystem");
            sb.AppendFormat("Version {0}.{1}", f2fsSb.major_ver, f2fsSb.minor_ver).AppendLine();
            sb.AppendFormat("{0} bytes per sector", 1 << (int)f2fsSb.log_sectorsize).AppendLine();

            sb.AppendFormat("{0} sectors ({1} bytes) per block", 1 << (int)f2fsSb.log_sectors_per_block,
                            1                                      << (int)f2fsSb.log_blocksize).AppendLine();

            sb.AppendFormat("{0} blocks per segment", f2fsSb.log_blocks_per_seg).AppendLine();
            sb.AppendFormat("{0} blocks in volume", f2fsSb.block_count).AppendLine();
            sb.AppendFormat("{0} segments per section", f2fsSb.segs_per_sec).AppendLine();
            sb.AppendFormat("{0} sections per zone", f2fsSb.secs_per_zone).AppendLine();
            sb.AppendFormat("{0} sections", f2fsSb.section_count).AppendLine();
            sb.AppendFormat("{0} segments", f2fsSb.segment_count).AppendLine();
            sb.AppendFormat("Root directory resides on inode {0}", f2fsSb.root_ino).AppendLine();
            sb.AppendFormat("Volume UUID: {0}", f2fsSb.uuid).AppendLine();

            sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(f2fsSb.volume_name, Encoding.Unicode, true)).
               AppendLine();

            sb.AppendFormat("Volume last mounted on kernel version: {0}", StringHandlers.CToString(f2fsSb.version)).
               AppendLine();

            sb.AppendFormat("Volume created on kernel version: {0}", StringHandlers.CToString(f2fsSb.init_version)).
               AppendLine();

            information = sb.ToString();

            XmlFsType = new FileSystemType
            {
                Type                   = "F2FS filesystem",
                SystemIdentifier       = Encoding.ASCII.GetString(f2fsSb.version),
                Clusters               = f2fsSb.block_count,
                ClusterSize            = (uint)(1 << (int)f2fsSb.log_blocksize),
                DataPreparerIdentifier = Encoding.ASCII.GetString(f2fsSb.init_version),
                VolumeName             = StringHandlers.CToString(f2fsSb.volume_name, Encoding.Unicode, true),
                VolumeSerial           = f2fsSb.uuid.ToString()
            };
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1), SuppressMessage("ReSharper", "InconsistentNaming")]
        readonly struct Superblock
        {
            public readonly uint   magic;
            public readonly ushort major_ver;
            public readonly ushort minor_ver;
            public readonly uint   log_sectorsize;
            public readonly uint   log_sectors_per_block;
            public readonly uint   log_blocksize;
            public readonly uint   log_blocks_per_seg;
            public readonly uint   segs_per_sec;
            public readonly uint   secs_per_zone;
            public readonly uint   checksum_offset;
            public readonly ulong  block_count;
            public readonly uint   section_count;
            public readonly uint   segment_count;
            public readonly uint   segment_count_ckpt;
            public readonly uint   segment_count_sit;
            public readonly uint   segment_count_nat;
            public readonly uint   segment_count_ssa;
            public readonly uint   segment_count_main;
            public readonly uint   segment0_blkaddr;
            public readonly uint   cp_blkaddr;
            public readonly uint   sit_blkaddr;
            public readonly uint   nat_blkaddr;
            public readonly uint   ssa_blkaddr;
            public readonly uint   main_blkaddr;
            public readonly uint   root_ino;
            public readonly uint   node_ino;
            public readonly uint   meta_ino;
            public readonly Guid   uuid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
            public readonly byte[] volume_name;
            public readonly uint extension_count;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public readonly byte[] extension_list1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public readonly byte[] extension_list2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public readonly byte[] extension_list3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public readonly byte[] extension_list4;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public readonly byte[] extension_list5;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public readonly byte[] extension_list6;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public readonly byte[] extension_list7;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public readonly byte[] extension_list8;
            public readonly uint cp_payload;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public readonly byte[] version;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public readonly byte[] init_version;
            public readonly uint feature;
            public readonly byte encryption_level;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] encrypt_pw_salt;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 871)]
            public readonly byte[] reserved;
        }
    }
}