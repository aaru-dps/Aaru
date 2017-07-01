// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Reiser.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the Reiser filesystem and shows information.
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
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DiscImageChef.Filesystems
{
    public class Reiser : Filesystem
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct ReiserJournalParams
        {
            public uint journal_1stblock;
            public uint journal_dev;
            public uint journal_size;
            public uint journal_trans_max;
            public uint journal_magic;
            public uint journal_max_batch;
            public uint journal_max_commit_age;
            public uint journal_max_trans_age;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Reiser_Superblock
        {
            public uint block_count;
            public uint free_blocks;
            public uint root_block;
            public ReiserJournalParams journal;
            public ushort blocksize;
            public ushort oid_maxsize;
            public ushort oid_cursize;
            public ushort umount_state;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public byte[] magic;
            public ushort fs_state;
            public uint hash_function_code;
            public ushort tree_height;
            public ushort bmap_nr;
            public ushort version;
            public ushort reserved_for_journal;
            public uint inode_generation;
            public uint flags;
            public Guid uuid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] label;
            public ushort mnt_count;
            public ushort max_mnt_count;
            public uint last_check;
            public uint check_interval;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 76)]
            public byte[] unused;
        }

        readonly byte[] Reiser35_Magic = { 0x52, 0x65, 0x49, 0x73, 0x45, 0x72, 0x46, 0x73, 0x00, 0x00 };
        readonly byte[] Reiser36_Magic = { 0x52, 0x65, 0x49, 0x73, 0x45, 0x72, 0x32, 0x46, 0x73, 0x00 };
        readonly byte[] ReiserJr_Magic = { 0x52, 0x65, 0x49, 0x73, 0x45, 0x72, 0x33, 0x46, 0x73, 0x00 };
        const uint Reiser_SuperOffset = 0x10000;

        public Reiser()
        {
            Name = "Reiser Filesystem Plugin";
            PluginUUID = new Guid("1D8CD8B8-27E6-410F-9973-D16409225FBA");
            CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
        }

        public Reiser(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, Encoding encoding)
        {
            Name = "Reiser Filesystem Plugin";
            PluginUUID = new Guid("1D8CD8B8-27E6-410F-9973-D16409225FBA");
            if(encoding == null)
                CurrentEncoding = Encoding.GetEncoding("iso-8859-15");
        }

        public override bool Identify(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd)
        {
            if(imagePlugin.GetSectorSize() < 512)
                return false;

            uint sbAddr = Reiser_SuperOffset / imagePlugin.GetSectorSize();
            if(sbAddr == 0)
                sbAddr = 1;

            Reiser_Superblock reiserSb = new Reiser_Superblock();

            uint sbSize = (uint)(Marshal.SizeOf(reiserSb) / imagePlugin.GetSectorSize());
            if(Marshal.SizeOf(reiserSb) % imagePlugin.GetSectorSize() != 0)
                sbSize++;

            byte[] sector = imagePlugin.ReadSectors(partitionStart + sbAddr, sbSize);
            if(sector.Length < Marshal.SizeOf(reiserSb))
                return false;

            IntPtr sbPtr = Marshal.AllocHGlobal(Marshal.SizeOf(reiserSb));
            Marshal.Copy(sector, 0, sbPtr, Marshal.SizeOf(reiserSb));
            reiserSb = (Reiser_Superblock)Marshal.PtrToStructure(sbPtr, typeof(Reiser_Superblock));
            Marshal.FreeHGlobal(sbPtr);

            return Reiser35_Magic.SequenceEqual(reiserSb.magic) ||
                    Reiser36_Magic.SequenceEqual(reiserSb.magic) ||
                    ReiserJr_Magic.SequenceEqual(reiserSb.magic);
        }

        public override void GetInformation(ImagePlugins.ImagePlugin imagePlugin, ulong partitionStart, ulong partitionEnd, out string information)
        {
            information = "";
            if(imagePlugin.GetSectorSize() < 512)
                return;

            uint sbAddr = Reiser_SuperOffset / imagePlugin.GetSectorSize();
            if(sbAddr == 0)
                sbAddr = 1;

            Reiser_Superblock reiserSb = new Reiser_Superblock();

            uint sbSize = (uint)(Marshal.SizeOf(reiserSb) / imagePlugin.GetSectorSize());
            if(Marshal.SizeOf(reiserSb) % imagePlugin.GetSectorSize() != 0)
                sbSize++;

            byte[] sector = imagePlugin.ReadSectors(partitionStart + sbAddr, sbSize);
            if(sector.Length < Marshal.SizeOf(reiserSb))
                return;

            IntPtr sbPtr = Marshal.AllocHGlobal(Marshal.SizeOf(reiserSb));
            Marshal.Copy(sector, 0, sbPtr, Marshal.SizeOf(reiserSb));
            reiserSb = (Reiser_Superblock)Marshal.PtrToStructure(sbPtr, typeof(Reiser_Superblock));
            Marshal.FreeHGlobal(sbPtr);

            if(!Reiser35_Magic.SequenceEqual(reiserSb.magic) &&
               !Reiser36_Magic.SequenceEqual(reiserSb.magic) &&
               !ReiserJr_Magic.SequenceEqual(reiserSb.magic))
                return;

            StringBuilder sb = new StringBuilder();

            if(Reiser35_Magic.SequenceEqual(reiserSb.magic))
                sb.AppendLine("Reiser 3.5 filesystem");
            else if(Reiser36_Magic.SequenceEqual(reiserSb.magic))
                sb.AppendLine("Reiser 3.6 filesystem");
            else if(ReiserJr_Magic.SequenceEqual(reiserSb.magic))
                sb.AppendLine("Reiser Jr. filesystem");
            sb.AppendFormat("Volume has {0} blocks with {1} blocks free", reiserSb.block_count, reiserSb.free_blocks).AppendLine();
            sb.AppendFormat("{0} bytes per block", reiserSb.blocksize).AppendLine();
            sb.AppendFormat("Root directory resides on block {0}", reiserSb.root_block).AppendLine();
            if(reiserSb.umount_state == 2)
                sb.AppendLine("Volume has not been cleanly umounted");
            sb.AppendFormat("Volume last checked on {0}", DateHandlers.UNIXUnsignedToDateTime(reiserSb.last_check)).AppendLine();
            if(reiserSb.version >= 2)
            {
                sb.AppendFormat("Volume UUID: {0}", reiserSb.uuid).AppendLine();
                sb.AppendFormat("Volume name: {0}", CurrentEncoding.GetString(reiserSb.label)).AppendLine();
            }

            information = sb.ToString();

            xmlFSType = new Schemas.FileSystemType();
            if(Reiser35_Magic.SequenceEqual(reiserSb.magic))
                xmlFSType.Type = "Reiser 3.5 filesystem";
            else if(Reiser36_Magic.SequenceEqual(reiserSb.magic))
                xmlFSType.Type = "Reiser 3.6 filesystem";
            else if(ReiserJr_Magic.SequenceEqual(reiserSb.magic))
                xmlFSType.Type = "Reiser Jr. filesystem";
            xmlFSType.ClusterSize = reiserSb.blocksize;
            xmlFSType.Clusters = reiserSb.block_count;
            xmlFSType.FreeClusters = reiserSb.free_blocks;
            xmlFSType.FreeClustersSpecified = true;
            xmlFSType.Dirty = reiserSb.umount_state == 2;
            if(reiserSb.version >= 2)
            {
                xmlFSType.VolumeName = CurrentEncoding.GetString(reiserSb.label);
                xmlFSType.VolumeSerial = reiserSb.uuid.ToString();
            }
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