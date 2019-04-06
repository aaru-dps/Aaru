// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Info.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : FATX filesystem plugin.
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

using System.Text;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.Helpers;
using Schemas;

namespace DiscImageChef.Filesystems.FATX
{
    public partial class XboxFatPlugin
    {
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
    }
}