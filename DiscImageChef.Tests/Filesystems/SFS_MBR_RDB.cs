// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AFFS_MBR_RDB.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System.Collections.Generic;
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;
using DiscImageChef.PartPlugins;
using NUnit.Framework;
using System;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class AFFS_MBR_RDB
    {
        readonly string[] testfiles = {
            "aros.vdi.lz","aros_intl.vdi.lz",
        };

        readonly ulong[] sectors = {
            409600,409600,
        };

        readonly uint[] sectorsize = {
            512,512,
        };

        readonly long[] clusters = {
            408240,408240,
        };

        readonly int[] clustersize = {
            512,512,
        };

        readonly string[] volumename = {
            "Volume label","Volume label",
        };

        readonly string[] volumeserial = {
            null,null,
        };

        [Test]
        public void Test()
        {
            throw new NotImplementedException("Partition schemes inside partitions are not yet implemented, and should be tested here.");
/*            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "affs_mbr_rdb", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new VDI();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                PartPlugin parts = new MBR();
                Assert.AreEqual(true, parts.GetInformation(image, out List<Partition> partitions), testfiles[i]);
                Filesystem fs = new DiscImageChef.Filesystems.AmigaDOSPlugin();
                int part = -1;
                for(int j = 0; j < partitions.Count; j++)
                {
                    if(partitions[j].PartitionType == "0x2D")
                    {
                        part = j;
                        break;
                    }
                }
                Assert.AreNotEqual(-1, part, "Partition not found");
                Assert.AreEqual(true, fs.Identify(image, partitions[part].PartitionStartSector, partitions[part].PartitionStartSector + partitions[part].PartitionSectors - 1), testfiles[i]);
                fs.GetInformation(image, partitions[part].PartitionStartSector, partitions[part].PartitionStartSector + partitions[part].PartitionSectors - 1, out string information);
                Assert.AreEqual(clusters[i], fs.XmlFSType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFSType.ClusterSize, testfiles[i]);
                Assert.AreEqual("Amiga FFS", fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSerial, testfiles[i]);
            }*/
        }
    }
}
