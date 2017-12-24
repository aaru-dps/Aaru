// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using Schemas;

namespace DiscImageChef.Filesystems
{
    public class F2FS : Filesystem
    {
        const uint F2FS_MAGIC = 0xF2F52010;
        const uint F2FS_SUPER_OFFSET = 1024;
        const uint F2FS_MIN_SECTOR = 512;
        const uint F2FS_MAX_SECTOR = 4096;
        const uint F2FS_BLOCK_SIZE = 4096;

        public F2FS()
        {
            Name = "F2FS Plugin";
            PluginUuid = new Guid("82B0920F-5F0D-4063-9F57-ADE0AE02ECE5");
            CurrentEncoding = Encoding.Unicode;
        }

        public F2FS(Encoding encoding)
        {
            Name = "F2FS Plugin";
            PluginUuid = new Guid("82B0920F-5F0D-4063-9F57-ADE0AE02ECE5");
            CurrentEncoding = Encoding.Unicode;
        }

        public F2FS(ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "F2FS Plugin";
            PluginUuid = new Guid("82B0920F-5F0D-4063-9F57-ADE0AE02ECE5");
            CurrentEncoding = Encoding.Unicode;
        }

        public override bool Identify(ImagePlugin imagePlugin, Partition partition)
        {
            if(imagePlugin.GetSectorSize() < F2FS_MIN_SECTOR || imagePlugin.GetSectorSize() > F2FS_MAX_SECTOR)
                return false;

            uint sbAddr = F2FS_SUPER_OFFSET / imagePlugin.GetSectorSize();
            if(sbAddr == 0) sbAddr = 1;

            F2FS_Superblock f2fsSb = new F2FS_Superblock();

            uint sbSize = (uint)(Marshal.SizeOf(f2fsSb) / imagePlugin.GetSectorSize());
            if(Marshal.SizeOf(f2fsSb) % imagePlugin.GetSectorSize() != 0) sbSize++;

            if(partition.Start + sbAddr >= partition.End) return false;

            byte[] sector = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize);
            if(sector.Length < Marshal.SizeOf(f2fsSb)) return false;

            IntPtr sbPtr = Marshal.AllocHGlobal(Marshal.SizeOf(f2fsSb));
            Marshal.Copy(sector, 0, sbPtr, Marshal.SizeOf(f2fsSb));
            f2fsSb = (F2FS_Superblock)Marshal.PtrToStructure(sbPtr, typeof(F2FS_Superblock));
            Marshal.FreeHGlobal(sbPtr);

            return f2fsSb.magic == F2FS_MAGIC;
        }

        public override void GetInformation(ImagePlugin imagePlugin, Partition partition, out string information)
        {
            information = "";
            if(imagePlugin.GetSectorSize() < F2FS_MIN_SECTOR || imagePlugin.GetSectorSize() > F2FS_MAX_SECTOR) return;

            uint sbAddr = F2FS_SUPER_OFFSET / imagePlugin.GetSectorSize();
            if(sbAddr == 0) sbAddr = 1;

            F2FS_Superblock f2fsSb = new F2FS_Superblock();

            uint sbSize = (uint)(Marshal.SizeOf(f2fsSb) / imagePlugin.GetSectorSize());
            if(Marshal.SizeOf(f2fsSb) % imagePlugin.GetSectorSize() != 0) sbSize++;

            byte[] sector = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize);
            if(sector.Length < Marshal.SizeOf(f2fsSb)) return;

            IntPtr sbPtr = Marshal.AllocHGlobal(Marshal.SizeOf(f2fsSb));
            Marshal.Copy(sector, 0, sbPtr, Marshal.SizeOf(f2fsSb));
            f2fsSb = (F2FS_Superblock)Marshal.PtrToStructure(sbPtr, typeof(F2FS_Superblock));
            Marshal.FreeHGlobal(sbPtr);

            if(f2fsSb.magic != F2FS_MAGIC) return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("F2FS filesystem");
            sb.AppendFormat("Version {0}.{1}", f2fsSb.major_ver, f2fsSb.minor_ver).AppendLine();
            sb.AppendFormat("{0} bytes per sector", 1 << (int)f2fsSb.log_sectorsize).AppendLine();
            sb.AppendFormat("{0} sectors ({1} bytes) per block", 1 << (int)f2fsSb.log_sectors_per_block,
                            1 << (int)f2fsSb.log_blocksize).AppendLine();
            sb.AppendFormat("{0} blocks per segment", f2fsSb.log_blocks_per_seg).AppendLine();
            sb.AppendFormat("{0} blocks in volume", f2fsSb.block_count).AppendLine();
            sb.AppendFormat("{0} segments per section", f2fsSb.segs_per_sec).AppendLine();
            sb.AppendFormat("{0} sections per zone", f2fsSb.secs_per_zone).AppendLine();
            sb.AppendFormat("{0} sections", f2fsSb.section_count).AppendLine();
            sb.AppendFormat("{0} segments", f2fsSb.segment_count).AppendLine();
            sb.AppendFormat("Root directory resides on inode {0}", f2fsSb.root_ino).AppendLine();
            sb.AppendFormat("Volume UUID: {0}", f2fsSb.uuid).AppendLine();
            sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(f2fsSb.volume_name, Encoding.Unicode, true))
              .AppendLine();
            sb.AppendFormat("Volume last mounted on kernel version: {0}", StringHandlers.CToString(f2fsSb.version))
              .AppendLine();
            sb.AppendFormat("Volume created on kernel version: {0}", StringHandlers.CToString(f2fsSb.init_version))
              .AppendLine();

            information = sb.ToString();

            XmlFsType = new FileSystemType
            {
                Type = "F2FS filesystem",
                SystemIdentifier = Encoding.ASCII.GetString(f2fsSb.version),
                Clusters = (long)f2fsSb.block_count,
                ClusterSize = 1 << (int)f2fsSb.log_blocksize,
                DataPreparerIdentifier = Encoding.ASCII.GetString(f2fsSb.init_version),
                VolumeName = StringHandlers.CToString(f2fsSb.volume_name, Encoding.Unicode, true),
                VolumeSerial = f2fsSb.uuid.ToString()
            };
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

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        struct F2FS_Superblock
        {
            public uint magic;
            public ushort major_ver;
            public ushort minor_ver;
            public uint log_sectorsize;
            public uint log_sectors_per_block;
            public uint log_blocksize;
            public uint log_blocks_per_seg;
            public uint segs_per_sec;
            public uint secs_per_zone;
            public uint checksum_offset;
            public ulong block_count;
            public uint section_count;
            public uint segment_count;
            public uint segment_count_ckpt;
            public uint segment_count_sit;
            public uint segment_count_nat;
            public uint segment_count_ssa;
            public uint segment_count_main;
            public uint segment0_blkaddr;
            public uint cp_blkaddr;
            public uint sit_blkaddr;
            public uint nat_blkaddr;
            public uint ssa_blkaddr;
            public uint main_blkaddr;
            public uint root_ino;
            public uint node_ino;
            public uint meta_ino;
            public Guid uuid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)] public byte[] volume_name;
            public uint extension_count;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] public byte[] extension_list1;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] public byte[] extension_list2;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] public byte[] extension_list3;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] public byte[] extension_list4;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] public byte[] extension_list5;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] public byte[] extension_list6;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] public byte[] extension_list7;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)] public byte[] extension_list8;
            public uint cp_payload;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] public byte[] version;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] public byte[] init_version;
            public uint feature;
            public byte encryption_level;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] encrypt_pw_salt;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 871)] public byte[] reserved;
        }
    }
}