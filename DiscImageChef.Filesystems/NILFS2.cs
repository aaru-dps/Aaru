// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : NILFS2.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : NILFS2 filesystem plugin.
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the NILFS2 filesystem and shows information.
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
    public class NILFS2 : Filesystem
    {
        enum NILFS2_State : ushort
        {
            Valid = 0x0001,
            Error = 0x0002,
            Resize = 0x0004
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct NILFS2_Superblock
        {
            public uint rev_level;
            public ushort minor_rev_level;
            public ushort magic;
            public ushort bytes;
            public ushort flags;
            public uint crc_seed;
            public uint sum;
            public uint log_block_size;
            public ulong nsegments;
            public ulong dev_size;
            public ulong first_data_block;
            public uint blocks_per_segment;
            public uint r_segments_percentage;
            public ulong last_cno;
            public ulong last_pseg;
            public ulong last_seq;
            public ulong free_blocks_count;
            public ulong ctime;
            public ulong mtime;
            public ulong wtime;
            public ushort mnt_count;
            public ushort max_mnt_count;
            public NILFS2_State state;
            public ushort errors;
            public ulong lastcheck;
            public uint checkinterval;
            public uint creator_os;
            public ushort def_resuid;
            public ushort def_resgid;
            public uint first_ino;
            public ushort inode_size;
            public ushort dat_entry_size;
            public ushort checkpoint_size;
            public ushort segment_usage_size;
            public Guid uuid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)] public byte[] volume_name;
            public uint c_interval;
            public uint c_block_max;
            public ulong feature_compat;
            public ulong feature_compat_ro;
            public ulong feature_incompat;
        }

        const ushort NILFS2_MAGIC = 0x3434;
        const uint NILFS2_SUPER_OFFSET = 1024;

        public NILFS2()
        {
            Name = "NILFS2 Plugin";
            PluginUuid = new Guid("35224226-C5CC-48B5-8FFD-3781E91E86B6");
            CurrentEncoding = Encoding.UTF8;
        }

        public NILFS2(Encoding encoding)
        {
            Name = "NILFS2 Plugin";
            PluginUuid = new Guid("35224226-C5CC-48B5-8FFD-3781E91E86B6");
            CurrentEncoding = encoding ?? Encoding.UTF8;
        }

        public NILFS2(ImagePlugin imagePlugin, Partition partition, Encoding encoding)
        {
            Name = "NILFS2 Plugin";
            PluginUuid = new Guid("35224226-C5CC-48B5-8FFD-3781E91E86B6");
            CurrentEncoding = encoding ?? Encoding.UTF8;
        }

        public override bool Identify(ImagePlugin imagePlugin, Partition partition)
        {
            if(imagePlugin.GetSectorSize() < 512) return false;

            uint sbAddr = NILFS2_SUPER_OFFSET / imagePlugin.GetSectorSize();
            if(sbAddr == 0) sbAddr = 1;

            NILFS2_Superblock nilfsSb = new NILFS2_Superblock();

            uint sbSize = (uint)(Marshal.SizeOf(nilfsSb) / imagePlugin.GetSectorSize());
            if(Marshal.SizeOf(nilfsSb) % imagePlugin.GetSectorSize() != 0) sbSize++;

            if(partition.Start + sbAddr + sbSize >= partition.End) return false;

            byte[] sector = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize);
            if(sector.Length < Marshal.SizeOf(nilfsSb)) return false;

            IntPtr sbPtr = Marshal.AllocHGlobal(Marshal.SizeOf(nilfsSb));
            Marshal.Copy(sector, 0, sbPtr, Marshal.SizeOf(nilfsSb));
            nilfsSb = (NILFS2_Superblock)Marshal.PtrToStructure(sbPtr, typeof(NILFS2_Superblock));
            Marshal.FreeHGlobal(sbPtr);

            return nilfsSb.magic == NILFS2_MAGIC;
        }

        public override void GetInformation(ImagePlugin imagePlugin, Partition partition,
                                            out string information)
        {
            information = "";
            if(imagePlugin.GetSectorSize() < 512) return;

            uint sbAddr = NILFS2_SUPER_OFFSET / imagePlugin.GetSectorSize();
            if(sbAddr == 0) sbAddr = 1;

            NILFS2_Superblock nilfsSb = new NILFS2_Superblock();

            uint sbSize = (uint)(Marshal.SizeOf(nilfsSb) / imagePlugin.GetSectorSize());
            if(Marshal.SizeOf(nilfsSb) % imagePlugin.GetSectorSize() != 0) sbSize++;

            byte[] sector = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize);
            if(sector.Length < Marshal.SizeOf(nilfsSb)) return;

            IntPtr sbPtr = Marshal.AllocHGlobal(Marshal.SizeOf(nilfsSb));
            Marshal.Copy(sector, 0, sbPtr, Marshal.SizeOf(nilfsSb));
            nilfsSb = (NILFS2_Superblock)Marshal.PtrToStructure(sbPtr, typeof(NILFS2_Superblock));
            Marshal.FreeHGlobal(sbPtr);

            if(nilfsSb.magic != NILFS2_MAGIC) return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("NILFS2 filesystem");
            sb.AppendFormat("Version {0}.{1}", nilfsSb.rev_level, nilfsSb.minor_rev_level).AppendLine();
            sb.AppendFormat("{0} bytes per block", 1 << (int)(nilfsSb.log_block_size + 10)).AppendLine();
            sb.AppendFormat("{0} bytes in volume", nilfsSb.dev_size).AppendLine();
            sb.AppendFormat("{0} blocks per segment", nilfsSb.blocks_per_segment).AppendLine();
            sb.AppendFormat("{0} segments", nilfsSb.nsegments).AppendLine();
            if(nilfsSb.creator_os == 0) sb.AppendLine("Filesystem created on Linux");
            else sb.AppendFormat("Creator OS code: {0}", nilfsSb.creator_os).AppendLine();
            sb.AppendFormat("{0} bytes per inode", nilfsSb.inode_size).AppendLine();
            sb.AppendFormat("Volume UUID: {0}", nilfsSb.uuid).AppendLine();
            sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(nilfsSb.volume_name, CurrentEncoding))
              .AppendLine();
            sb.AppendFormat("Volume created on {0}", DateHandlers.UnixUnsignedToDateTime(nilfsSb.ctime)).AppendLine();
            sb.AppendFormat("Volume last mounted on {0}", DateHandlers.UnixUnsignedToDateTime(nilfsSb.mtime))
              .AppendLine();
            sb.AppendFormat("Volume last written on {0}", DateHandlers.UnixUnsignedToDateTime(nilfsSb.wtime))
              .AppendLine();

            information = sb.ToString();

            XmlFsType = new FileSystemType
            {
                Type = "NILFS2 filesystem",
                ClusterSize = 1 << (int)(nilfsSb.log_block_size + 10),
                VolumeName = StringHandlers.CToString(nilfsSb.volume_name, CurrentEncoding),
                VolumeSerial = nilfsSb.uuid.ToString(),
                CreationDate = DateHandlers.UnixUnsignedToDateTime(nilfsSb.ctime),
                CreationDateSpecified = true,
                ModificationDate = DateHandlers.UnixUnsignedToDateTime(nilfsSb.wtime),
                ModificationDateSpecified = true
            };
            if(nilfsSb.creator_os == 0) XmlFsType.SystemIdentifier = "Linux";
            XmlFsType.Clusters = (long)nilfsSb.dev_size / XmlFsType.ClusterSize;
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