// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Helpers;
using Schemas;
using Marshal = Aaru.Helpers.Marshal;

// ReSharper disable UnusedMember.Local

namespace Aaru.Filesystems
{
    /// <inheritdoc />
    /// <summary>Implements detection of the New Implementation of a Log-structured File System v2</summary>
    public sealed class NILFS2 : IFilesystem
    {
        const ushort NILFS2_MAGIC        = 0x3434;
        const uint   NILFS2_SUPER_OFFSET = 1024;

        /// <inheritdoc />
        public FileSystemType XmlFsType { get; private set; }
        /// <inheritdoc />
        public Encoding Encoding { get; private set; }
        /// <inheritdoc />
        public string Name => "NILFS2 Plugin";
        /// <inheritdoc />
        public Guid Id => new Guid("35224226-C5CC-48B5-8FFD-3781E91E86B6");
        /// <inheritdoc />
        public string Author => "Natalia Portillo";

        /// <inheritdoc />
        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(imagePlugin.Info.SectorSize < 512)
                return false;

            uint sbAddr = NILFS2_SUPER_OFFSET / imagePlugin.Info.SectorSize;

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

            Superblock nilfsSb = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

            return nilfsSb.magic == NILFS2_MAGIC;
        }

        /// <inheritdoc />
        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding encoding)
        {
            Encoding    = encoding ?? Encoding.UTF8;
            information = "";

            if(imagePlugin.Info.SectorSize < 512)
                return;

            uint sbAddr = NILFS2_SUPER_OFFSET / imagePlugin.Info.SectorSize;

            if(sbAddr == 0)
                sbAddr = 1;

            uint sbSize = (uint)(Marshal.SizeOf<Superblock>() / imagePlugin.Info.SectorSize);

            if(Marshal.SizeOf<Superblock>() % imagePlugin.Info.SectorSize != 0)
                sbSize++;

            byte[] sector = imagePlugin.ReadSectors(partition.Start + sbAddr, sbSize);

            if(sector.Length < Marshal.SizeOf<Superblock>())
                return;

            Superblock nilfsSb = Marshal.ByteArrayToStructureLittleEndian<Superblock>(sector);

            if(nilfsSb.magic != NILFS2_MAGIC)
                return;

            var sb = new StringBuilder();

            sb.AppendLine("NILFS2 filesystem");
            sb.AppendFormat("Version {0}.{1}", nilfsSb.rev_level, nilfsSb.minor_rev_level).AppendLine();
            sb.AppendFormat("{0} bytes per block", 1 << (int)(nilfsSb.log_block_size + 10)).AppendLine();
            sb.AppendFormat("{0} bytes in volume", nilfsSb.dev_size).AppendLine();
            sb.AppendFormat("{0} blocks per segment", nilfsSb.blocks_per_segment).AppendLine();
            sb.AppendFormat("{0} segments", nilfsSb.nsegments).AppendLine();

            if(nilfsSb.creator_os == 0)
                sb.AppendLine("Filesystem created on Linux");
            else
                sb.AppendFormat("Creator OS code: {0}", nilfsSb.creator_os).AppendLine();

            sb.AppendFormat("{0} bytes per inode", nilfsSb.inode_size).AppendLine();
            sb.AppendFormat("Volume UUID: {0}", nilfsSb.uuid).AppendLine();
            sb.AppendFormat("Volume name: {0}", StringHandlers.CToString(nilfsSb.volume_name, Encoding)).AppendLine();
            sb.AppendFormat("Volume created on {0}", DateHandlers.UnixUnsignedToDateTime(nilfsSb.ctime)).AppendLine();

            sb.AppendFormat("Volume last mounted on {0}", DateHandlers.UnixUnsignedToDateTime(nilfsSb.mtime)).
               AppendLine();

            sb.AppendFormat("Volume last written on {0}", DateHandlers.UnixUnsignedToDateTime(nilfsSb.wtime)).
               AppendLine();

            information = sb.ToString();

            XmlFsType = new FileSystemType
            {
                Type                      = "NILFS2 filesystem",
                ClusterSize               = (uint)(1 << (int)(nilfsSb.log_block_size + 10)),
                VolumeName                = StringHandlers.CToString(nilfsSb.volume_name, Encoding),
                VolumeSerial              = nilfsSb.uuid.ToString(),
                CreationDate              = DateHandlers.UnixUnsignedToDateTime(nilfsSb.ctime),
                CreationDateSpecified     = true,
                ModificationDate          = DateHandlers.UnixUnsignedToDateTime(nilfsSb.wtime),
                ModificationDateSpecified = true
            };

            if(nilfsSb.creator_os == 0)
                XmlFsType.SystemIdentifier = "Linux";

            XmlFsType.Clusters = nilfsSb.dev_size / XmlFsType.ClusterSize;
        }

        enum State : ushort
        {
            Valid = 0x0001, Error = 0x0002, Resize = 0x0004
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        readonly struct Superblock
        {
            public readonly uint   rev_level;
            public readonly ushort minor_rev_level;
            public readonly ushort magic;
            public readonly ushort bytes;
            public readonly ushort flags;
            public readonly uint   crc_seed;
            public readonly uint   sum;
            public readonly uint   log_block_size;
            public readonly ulong  nsegments;
            public readonly ulong  dev_size;
            public readonly ulong  first_data_block;
            public readonly uint   blocks_per_segment;
            public readonly uint   r_segments_percentage;
            public readonly ulong  last_cno;
            public readonly ulong  last_pseg;
            public readonly ulong  last_seq;
            public readonly ulong  free_blocks_count;
            public readonly ulong  ctime;
            public readonly ulong  mtime;
            public readonly ulong  wtime;
            public readonly ushort mnt_count;
            public readonly ushort max_mnt_count;
            public readonly State  state;
            public readonly ushort errors;
            public readonly ulong  lastcheck;
            public readonly uint   checkinterval;
            public readonly uint   creator_os;
            public readonly ushort def_resuid;
            public readonly ushort def_resgid;
            public readonly uint   first_ino;
            public readonly ushort inode_size;
            public readonly ushort dat_entry_size;
            public readonly ushort checkpoint_size;
            public readonly ushort segment_usage_size;
            public readonly Guid   uuid;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
            public readonly byte[] volume_name;
            public readonly uint  c_interval;
            public readonly uint  c_block_max;
            public readonly ulong feature_compat;
            public readonly ulong feature_compat_ro;
            public readonly ulong feature_incompat;
        }
    }
}