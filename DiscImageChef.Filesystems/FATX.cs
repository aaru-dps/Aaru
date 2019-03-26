// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : FATX.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FATX filesystem plugin
//
// --[ Description ] ----------------------------------------------------------
//
//     Identifies the FATX filesystem and shows information.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using Schemas;
using Marshal = DiscImageChef.Helpers.Marshal;

namespace DiscImageChef.Filesystems
{
    public class FATX : IFilesystem
    {
        const  uint           FATX_MAGIC       = 0x58544146;
        const  byte           UNUSED_DIRENTRY  = 0x00;
        const  byte           DELETED_DIRENTRY = 0xE5;
        const  byte           MAX_FILENAME     = 42;
        public FileSystemType XmlFsType { get; private set; }
        public Encoding       Encoding  { get; private set; }
        public string         Name      => "FATX Filesystem Plugin";
        public Guid           Id        => new Guid("ED27A721-4A17-4649-89FD-33633B46E228");
        public string         Author    => "Natalia Portillo";

        public bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(imagePlugin.Info.SectorSize < 512) return false;

            byte[] sector = imagePlugin.ReadSector(partition.Start);

            Superblock sb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);

            return sb.magic == FATX_MAGIC;
        }

        public void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                   Encoding    encoding)
        {
            Encoding    = Encoding.UTF8;
            information = "";
            if(imagePlugin.Info.SectorSize < 512) return;

            byte[] sector = imagePlugin.ReadSector(partition.Start);

            Superblock fatxSb = Marshal.ByteArrayToStructureBigEndian<Superblock>(sector);

            if(fatxSb.magic != FATX_MAGIC) return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("FATX filesystem");
            sb.AppendFormat("Filesystem id {0}", fatxSb.id).AppendLine();
            sb.AppendFormat("{0} sectors ({1} bytes) per cluster", fatxSb.sectorsPerCluster,
                            fatxSb.sectorsPerCluster * imagePlugin.Info.SectorSize).AppendLine();
            sb.AppendFormat("Root directory starts on cluster {0}", fatxSb.rootDirectoryCluster).AppendLine();

            information = sb.ToString();

            XmlFsType = new FileSystemType
            {
                Type        = "FATX filesystem",
                ClusterSize = (int)(fatxSb.sectorsPerCluster * imagePlugin.Info.SectorSize)
            };
            XmlFsType.Clusters = (long)((partition.End - partition.Start + 1) * imagePlugin.Info.SectorSize /
                                        (ulong)XmlFsType.ClusterSize);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct Superblock
        {
            public uint magic;
            public uint id;
            public uint sectorsPerCluster;
            public uint rootDirectoryCluster;
            public ushort unknown;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct DirectoryEntry
        {
            public byte       filenameSize;
            public Attributes attributes;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_FILENAME)]
            public byte[] filename;
            public uint   firstCluster;
            public uint   length;
            public ushort lastWrittenTime;
            public ushort lastWrittenDate;
            public ushort creationTime;
            public ushort creationDate;
            public ushort lastAccessTime;
            public ushort lastAccessDate;
        }

        [Flags]
        enum Attributes : byte
        {
            ReadOnly  = 0x01,
            Hidden    = 0x02,
            System    = 0x04,
            Directory = 0x10,
            Archive   = 0x20
        }
    }
}