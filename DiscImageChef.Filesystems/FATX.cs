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
    public class FATX : IFilesystem
    {
        const uint FATX_MAGIC = 0x58544146;

        Encoding currentEncoding;
        FileSystemType xmlFsType;
        public virtual FileSystemType XmlFsType => xmlFsType;

        public virtual Encoding Encoding => currentEncoding;
        public virtual string Name => "FATX Filesystem Plugin";
        public virtual Guid Id => new Guid("ED27A721-4A17-4649-89FD-33633B46E228");

        public virtual bool Identify(IMediaImage imagePlugin, Partition partition)
        {
            if(imagePlugin.Info.SectorSize < 512) return false;

            FATX_Superblock fatxSb;
            byte[] sector = imagePlugin.ReadSector(partition.Start);

            fatxSb = BigEndianMarshal.ByteArrayToStructureBigEndian<FATX_Superblock>(sector);

            return fatxSb.magic == FATX_MAGIC;
        }

        public virtual void GetInformation(IMediaImage imagePlugin, Partition partition, out string information,
                                            Encoding encoding)
        {
            currentEncoding = Encoding.UTF8;
            information = "";
            if(imagePlugin.Info.SectorSize < 512) return;

            FATX_Superblock fatxSb;

            byte[] sector = imagePlugin.ReadSector(partition.Start);

            fatxSb = BigEndianMarshal.ByteArrayToStructureBigEndian<FATX_Superblock>(sector);

            if(fatxSb.magic != FATX_MAGIC) return;

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("FATX filesystem");
            sb.AppendFormat("Filesystem id {0}", fatxSb.id).AppendLine();
            sb.AppendFormat("{0} sectors ({1} bytes) per cluster", fatxSb.sectorsPerCluster,
                            fatxSb.sectorsPerCluster * imagePlugin.Info.SectorSize).AppendLine();
            sb.AppendFormat("Root directory starts on cluster {0}", fatxSb.rootDirectoryCluster).AppendLine();

            information = sb.ToString();

            xmlFsType = new FileSystemType
            {
                Type = "FATX filesystem",
                ClusterSize = (int)(fatxSb.sectorsPerCluster * imagePlugin.Info.SectorSize)
            };
            xmlFsType.Clusters = (long)((partition.End - partition.Start + 1) * imagePlugin.Info.SectorSize /
                                        (ulong)xmlFsType.ClusterSize);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        struct FATX_Superblock
        {
            public uint magic;
            public uint id;
            public uint sectorsPerCluster;
            public uint rootDirectoryCluster;
        }
    }
}