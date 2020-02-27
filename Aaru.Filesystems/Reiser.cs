// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Reiser.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Reiser filesystem plugin
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Filesystems
{
    public class Reiser : IFilesystem
    {
        const uint REISER_SUPER_OFFSET = 0x10000;

        readonly byte[] reiser35_magic = {0x52, 0x65, 0x49, 0x73, 0x45, 0x72, 0x46, 0x73, 0x00, 0x00};
        readonly byte[] reiser36_magic = {0x52, 0x65, 0x49, 0x73, 0x45, 0x72, 0x32, 0x46, 0x73, 0x00};
        readonly byte[] reiserJr_magic = {0x52, 0x65, 0x49, 0x73, 0x45, 0x72, 0x33, 0x46, 0x73, 0x00};

        public FileSystemType XmlFsType { get; private set; }
        public Encoding       Encoding  { get; private set; }
        public string         Name      => "Reiser Filesystem Plugin";
        public Guid           Id        => new Guid("1D8CD8B8-27E6-410F-9973-D16409225FBA");
        public string         Author    => "Natalia Portillo";

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(imagePlugin.Info.SectorSize < 512) return false;

            uint sbAddr            = REISER_SUPER_OFFSET / imagePlugin.Info.SectorSize;
            if(sbAddr == 0) sbAddr = 1;

            uint sbSize = (uint)(Marshal.SizeOf<Reiser_Superblock>() / imagePlugin.Info.SectorSize);
            if(Marshal.SizeOf<Reiser_Superblock>() % imagePlugin.Info.SectorSize != 0) sbSize++;

            if(partition.Start + sbAddr + sbSize >= partition.End) return false;

            byte[] sector = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize);
            if(sector.Length < Marshal.SizeOf<Reiser_Superblock>()) return false;

            Reiser_Superblock reiserSb = Marshal.ByteArrayToStructureLittleEndian<Reiser_Superblock>(sector);

            return reiser35_magic.SequenceEqual(reiserSb.magic) || reiser36_magic.SequenceEqual(reiserSb.magic) ||
                   reiserJr_magic.SequenceEqual(reiserSb.magic);
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding    encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";
            if(imagePlugin.Info.SectorSize < 512) return;

            uint sbAddr            = REISER_SUPER_OFFSET / imagePlugin.Info.SectorSize;
            if(sbAddr == 0) sbAddr = 1;

            uint sbSize = (uint)(Marshal.SizeOf<Reiser_Superblock>() / imagePlugin.Info.SectorSize);
            if(Marshal.SizeOf<Reiser_Superblock>() % imagePlugin.Info.SectorSize != 0) sbSize++;

            byte[] sector = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize);
            if(sector.Length < Marshal.SizeOf<Reiser_Superblock>()) return;

            Reiser_Superblock reiserSb = Marshal.ByteArrayToStructureLittleEndian<Reiser_Superblock>(sector);

            if(!reiser35_magic.SequenceEqual(reiserSb.magic) && !reiser36_magic.SequenceEqual(reiserSb.magic) &&
               !reiserJr_magic.SequenceEqual(reiserSb.magic)) return;

            StringBuilder sb = new StringBuilder();

            if(reiser35_magic.SequenceEqual(reiserSb.magic)) sb.AppendLine("Reiser 3.5 filesystem");
            else if(reiser36_magic.SequenceEqual(reiserSb.magic)) sb.AppendLine("Reiser 3.6 filesystem");
            else if(reiserJr_magic.SequenceEqual(reiserSb.magic)) sb.AppendLine("Reiser Jr. filesystem");
            sb.AppendFormat("Volume has {0} blocks with {1} blocks free", reiserSb.block_count, reiserSb.free_blocks)
              .AppendLine();
            sb.AppendFormat("{0} bytes per block", reiserSb.blocksize).AppendLine();
            sb.AppendFormat("Root directory resides on block {0}", reiserSb.root_block).AppendLine();
            if(reiserSb.umount_state == 2) sb.AppendLine("Volume has not been cleanly umounted");
            sb.AppendFormat("Volume last checked on {0}", DateHandlers.UnixUnsignedToDateTime(reiserSb.last_check))
              .AppendLine();
            if(reiserSb.version >= 2)
            {
                sb.AppendFormat("Volume UUID: {0}", reiserSb.uuid).AppendLine();
                sb.AppendFormat("Volume name: {0}", Encoding.GetString(reiserSb.label)).AppendLine();
            }

            information = sb.ToString();

            XmlFsType = new FileSystemType();
            if(reiser35_magic.SequenceEqual(reiserSb.magic)) XmlFsType.Type      = "Reiser 3.5 filesystem";
            else if(reiser36_magic.SequenceEqual(reiserSb.magic)) XmlFsType.Type = "Reiser 3.6 filesystem";
            else if(reiserJr_magic.SequenceEqual(reiserSb.magic)) XmlFsType.Type = "Reiser Jr. filesystem";
            XmlFsType.ClusterSize           = reiserSb.blocksize;
            XmlFsType.Clusters              = reiserSb.block_count;
            XmlFsType.FreeClusters          = reiserSb.free_blocks;
            XmlFsType.FreeClustersSpecified = true;
            XmlFsType.Dirty                 = reiserSb.umount_state == 2;
            if(reiserSb.version < 2) return;

            XmlFsType.VolumeName   = Encoding.GetString(reiserSb.label);
            XmlFsType.VolumeSerial = reiserSb.uuid.ToString();
        }

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
            public uint                block_count;
            public uint                free_blocks;
            public uint                root_block;
            public ReiserJournalParams journal;
            public ushort              blocksize;
            public ushort              oid_maxsize;
            public ushort              oid_cursize;
            public ushort              umount_state;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public byte[] magic;
            public ushort fs_state;
            public uint   hash_function_code;
            public ushort tree_height;
            public ushort bmap_nr;
            public ushort version;
            public ushort reserved_for_journal;
            public uint   inode_generation;
            public uint   flags;
            public Guid   uuid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] label;
            public ushort mnt_count;
            public ushort max_mnt_count;
            public uint   last_check;
            public uint   check_interval;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 76)]
            public byte[] unused;
        }
    }
}