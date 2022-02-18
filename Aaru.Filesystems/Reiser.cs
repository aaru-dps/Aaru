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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System;
using System.Linq;
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
    /// <summary>Implements detection of the Reiser v3 filesystem</summary>
    public sealed class Reiser : IFilesystem
    {
        const uint REISER_SUPER_OFFSET = 0x10000;

        readonly byte[] _magic35 =
        {
            0x52, 0x65, 0x49, 0x73, 0x45, 0x72, 0x46, 0x73, 0x00, 0x00
        };
        readonly byte[] _magic36 =
        {
            0x52, 0x65, 0x49, 0x73, 0x45, 0x72, 0x32, 0x46, 0x73, 0x00
        };
        readonly byte[] _magicJr =
        {
            0x52, 0x65, 0x49, 0x73, 0x45, 0x72, 0x33, 0x46, 0x73, 0x00
        };

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding Encoding { get; private set; }
        /// <inheritdoc />
        public string Name => "Reiser Filesystem Plugin";
        /// <inheritdoc />
        public Guid Id => new Guid("1D8CD8B8-27E6-410F-9973-D16409225FBA");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(imagePlugin.Info.SectorSize < 512)
                return false;

            uint sbAddr = REISER_SUPER_OFFSET / imagePlugin.Info.SectorSize;

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

            Superblock reiserSb = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

            return _magic35.SequenceEqual(reiserSb.magic) || _magic36.SequenceEqual(reiserSb.magic) ||
                   _magicJr.SequenceEqual(reiserSb.magic);
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = encoding ?? Encoding.GetEncoding("iso-8859-15");
            information = "";

            if(imagePlugin.Info.SectorSize < 512)
                return;

            uint sbAddr = REISER_SUPER_OFFSET / imagePlugin.Info.SectorSize;

            if(sbAddr == 0)
                sbAddr = 1;

            uint sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

            if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
                sbSize++;

            byte[] sector = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize);

            if(sector.Length < Marshal.SizeOf<Superblock>())
                return;

            Superblock reiserSb = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

            if(!_magic35.SequenceEqual(reiserSb.magic) &&
               !_magic36.SequenceEqual(reiserSb.magic) &&
               !_magicJr.SequenceEqual(reiserSb.magic))
                return;

            var sb = new StringBuilder();

            if(_magic35.SequenceEqual(reiserSb.magic))
                sb.AppendLine("Reiser 3.5 filesystem");
            else if(_magic36.SequenceEqual(reiserSb.magic))
                sb.AppendLine("Reiser 3.6 filesystem");
            else if(_magicJr.SequenceEqual(reiserSb.magic))
                sb.AppendLine("Reiser Jr. filesystem");

            sb.AppendFormat("Volume has {0} blocks with {1} blocks free", reiserSb.block_count, reiserSb.free_blocks).
               AppendLine();

            sb.AppendFormat("{0} bytes per block", reiserSb.blocksize).AppendLine();
            sb.AppendFormat("Root directory resides on block {0}", reiserSb.root_block).AppendLine();

            if(reiserSb.umount_state == 2)
                sb.AppendLine("Volume has not been cleanly umounted");

            sb.AppendFormat("Volume last checked on {0}", DateHandlers.UnixUnsignedToDateTime(reiserSb.last_check)).
               AppendLine();

            if(reiserSb.version >= 2)
            {
                sb.AppendFormat("Volume UUID: {0}", reiserSb.uuid).AppendLine();
                sb.AppendFormat("Volume name: {0}", Encoding.GetString(reiserSb.label)).AppendLine();
            }

            information = sb.ToString();

            XmlFsType = new FileSystemType();

            if(_magic35.SequenceEqual(reiserSb.magic))
                XmlFsType.Type = "Reiser 3.5 filesystem";
            else if(_magic36.SequenceEqual(reiserSb.magic))
                XmlFsType.Type = "Reiser 3.6 filesystem";
            else if(_magicJr.SequenceEqual(reiserSb.magic))
                XmlFsType.Type = "Reiser Jr. filesystem";

            XmlFsType.ClusterSize           = reiserSb.blocksize;
            XmlFsType.Clusters              = reiserSb.block_count;
            XmlFsType.FreeClusters          = reiserSb.free_blocks;
            XmlFsType.FreeClustersSpecified = true;
            XmlFsType.Dirty                 = reiserSb.umount_state == 2;

            if(reiserSb.version < 2)
                return;

            XmlFsType.VolumeName   = StringHandlers.CToString(reiserSb.label, Encoding);
            XmlFsType.VolumeSerial = reiserSb.uuid.ToString();
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct JournalParameters
        {
            public readonly uint journal_1stblock;
            public readonly uint journal_dev;
            public readonly uint journal_size;
            public readonly uint journal_trans_max;
            public readonly uint journal_magic;
            public readonly uint journal_max_batch;
            public readonly uint journal_max_commit_age;
            public readonly uint journal_max_trans_age;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Superblock
        {
            public readonly uint              block_count;
            public readonly uint              free_blocks;
            public readonly uint              root_block;
            public readonly JournalParameters journal;
            public readonly ushort            blocksize;
            public readonly ushort            oid_maxsize;
            public readonly ushort            oid_cursize;
            public readonly ushort            umount_state;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
            public readonly byte[] magic;
            public readonly ushort fs_state;
            public readonly uint   hash_function_code;
            public readonly ushort tree_height;
            public readonly ushort bmap_nr;
            public readonly ushort version;
            public readonly ushort reserved_for_journal;
            public readonly uint   inode_generation;
            public readonly uint   flags;
            public          Guid   uuid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] label;
            public readonly ushort mnt_count;
            public readonly ushort max_mnt_count;
            public readonly uint   last_check;
            public readonly uint   check_interval;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 76)]
            public readonly byte[] unused;
        }
    }
}