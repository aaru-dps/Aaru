// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Xia.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Xia filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Xia filesystem and shows information.
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
    // Information from the Linux kernel
    public class Xia : IFilesystem
    {
        const uint XIAFS_SUPER_MAGIC = 0x012FD16D;
        const uint XIAFS_ROOT_INO = 1;
        const uint XIAFS_BAD_INO = 2;
        const int XIAFS_MAX_LINK = 64000;
        const int XIAFS_DIR_SIZE = 12;
        const int XIAFS_NUM_BLOCK_POINTERS = 10;
        const int XIAFS_NAME_LEN = 248;

        Encoding currentEncoding;
        FileSystemType xmlFsType;
        public FileSystemType XmlFsType => xmlFsType;
        public Encoding Encoding => currentEncoding;
        public string Name => "Xia filesystem";
        public Guid Id => new Guid("169E1DE5-24F2-4EF6-A04D-A4B2CA66DE9D");

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            int sbSizeInBytes = Marshal.SizeOf(typeof(XiaSuperBlock));
            uint sbSizeInSectors = (uint)(sbSizeInBytes / imagePlugin.Info.SectorSize);
            if(sbSizeInBytes % imagePlugin.Info.SectorSize > 0) sbSizeInSectors++;
            if(sbSizeInSectors + partition.Start >= partition.End) return false;

            byte[] sbSector = imagePlugin.ReadSectors(partition.Start, sbSizeInSectors);
            IntPtr sbPtr = Marshal.AllocHGlobal(sbSizeInBytes);
            Marshal.Copy(sbSector, 0, sbPtr, sbSizeInBytes);
            XiaSuperBlock supblk = (XiaSuperBlock)Marshal.PtrToStructure(sbPtr, typeof(XiaSuperBlock));
            Marshal.FreeHGlobal(sbPtr);

            return supblk.s_magic == XIAFS_SUPER_MAGIC;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information, Encoding encoding)
        {
            currentEncoding = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";

            StringBuilder sb = new StringBuilder();

            int sbSizeInBytes = Marshal.SizeOf(typeof(XiaSuperBlock));
            uint sbSizeInSectors = (uint)(sbSizeInBytes / imagePlugin.Info.SectorSize);
            if(sbSizeInBytes % imagePlugin.Info.SectorSize > 0) sbSizeInSectors++;

            byte[] sbSector = imagePlugin.ReadSectors(partition.Start, sbSizeInSectors);
            IntPtr sbPtr = Marshal.AllocHGlobal(sbSizeInBytes);
            Marshal.Copy(sbSector, 0, sbPtr, sbSizeInBytes);
            XiaSuperBlock supblk = (XiaSuperBlock)Marshal.PtrToStructure(sbPtr, typeof(XiaSuperBlock));
            Marshal.FreeHGlobal(sbPtr);

            sb.AppendFormat("{0} bytes per zone", supblk.s_zone_size).AppendLine();
            sb.AppendFormat("{0} zones in volume ({1} bytes)", supblk.s_nzones, supblk.s_nzones * supblk.s_zone_size)
              .AppendLine();
            sb.AppendFormat("{0} inodes", supblk.s_ninodes).AppendLine();
            sb.AppendFormat("{0} data zones ({1} bytes)", supblk.s_ndatazones, supblk.s_ndatazones * supblk.s_zone_size)
              .AppendLine();
            sb.AppendFormat("{0} imap zones ({1} bytes)", supblk.s_imap_zones, supblk.s_imap_zones * supblk.s_zone_size)
              .AppendLine();
            sb.AppendFormat("{0} zmap zones ({1} bytes)", supblk.s_zmap_zones, supblk.s_zmap_zones * supblk.s_zone_size)
              .AppendLine();
            sb.AppendFormat("First data zone: {0}", supblk.s_firstdatazone).AppendLine();
            sb.AppendFormat("Maximum filesize is {0} bytes ({1} MiB)", supblk.s_max_size, supblk.s_max_size / 1048576)
              .AppendLine();
            sb.AppendFormat("{0} zones reserved for kernel images ({1} bytes)", supblk.s_kernzones,
                            supblk.s_kernzones * supblk.s_zone_size).AppendLine();
            sb.AppendFormat("First kernel zone: {0}", supblk.s_firstkernzone).AppendLine();

            xmlFsType = new FileSystemType
            {
                Bootable = !ArrayHelpers.ArrayIsNullOrEmpty(supblk.s_boot_segment),
                Clusters = supblk.s_nzones,
                ClusterSize = (int)supblk.s_zone_size,
                Type = "Xia filesystem"
            };

            information = sb.ToString();
        }

        /// <summary>
        ///     Xia superblock
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct XiaSuperBlock
        {
            /// <summary>1st sector reserved for boot</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)] public byte[] s_boot_segment;
            /// <summary>the name says it</summary>
            public uint s_zone_size;
            /// <summary>volume size, zone aligned</summary>
            public uint s_nzones;
            /// <summary># of inodes</summary>
            public uint s_ninodes;
            /// <summary># of data zones</summary>
            public uint s_ndatazones;
            /// <summary># of imap zones</summary>
            public uint s_imap_zones;
            /// <summary># of zmap zones</summary>
            public uint s_zmap_zones;
            /// <summary>first data zone</summary>
            public uint s_firstdatazone;
            /// <summary>z size = 1KB &lt;&lt; z shift</summary>
            public uint s_zone_shift;
            /// <summary>max size of a single file</summary>
            public uint s_max_size;
            /// <summary>reserved</summary>
            public uint s_reserved0;
            /// <summary>reserved</summary>
            public uint s_reserved1;
            /// <summary>reserved</summary>
            public uint s_reserved2;
            /// <summary>reserved</summary>
            public uint s_reserved3;
            /// <summary>first kernel zone</summary>
            public uint s_firstkernzone;
            /// <summary>kernel size in zones</summary>
            public uint s_kernzones;
            /// <summary>magic number for xiafs</summary>
            public uint s_magic;
        }

        /// <summary>
        ///     Xia directory entry
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct XiaDirect
        {
            public uint d_ino;
            public ushort d_rec_len;
            public byte d_name_len;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = XIAFS_NAME_LEN + 1)] public byte[] d_name;
        }

        /// <summary>
        ///     Xia inode
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct XiaInode
        {
            public ushort i_mode;
            public ushort i_nlinks;
            public ushort i_uid;
            public ushort i_gid;
            public uint i_size;
            public uint i_ctime;
            public uint i_atime;
            public uint i_mtime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = XIAFS_NUM_BLOCK_POINTERS)] public uint[] i_zone;
        }
    }
}