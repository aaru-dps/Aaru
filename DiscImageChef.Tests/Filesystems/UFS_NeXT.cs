// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : UFS_MBR.cs
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

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class UFS_MBR
    {
        readonly string[] testfiles = {
            "ufs1/linux.vdi.lz", "ufs2/linux.vdi.lz", "ffs43/darwin_1.3.1.vdi.lz", "ffs43/darwin_1.4.1.vdi.lz",
            "ffs43/darwin_6.0.2.vdi.lz", "ffs43/darwin_8.0.1.vdi.lz", "ffs43/dflybsd_1.2.0.vdi.lz", "ffs43/dflybsd_3.6.1.vdi.lz",
            "ffs43/dflybsd_4.0.5.vdi.lz", "ffs43/netbsd_1.6.vdi.lz", "ffs43/netbsd_7.1.vdi.lz", "ufs1/darwin_1.3.1.vdi.lz",
            "ufs1/darwin_1.4.1.vdi.lz", "ufs1/darwin_6.0.2.vdi.lz", "ufs1/darwin_8.0.1.vdi.lz", "ufs1/dflybsd_1.2.0.vdi.lz",
            "ufs1/dflybsd_3.6.1.vdi.lz", "ufs1/dflybsd_4.0.5.vdi.lz", "ufs1/freebsd_6.1.vdi.lz", "ufs1/freebsd_7.0.vdi.lz",
            "ufs1/freebsd_8.2.vdi.lz", "ufs1/netbsd_1.6.vdi.lz", "ufs1/netbsd_7.1.vdi.lz", "ufs1/solaris_7.vdi.lz",
            "ufs1/solaris_7.vdi.lz", "ufs2/freebsd_6.1.vdi.lz", "ufs2/freebsd_7.0.vdi.lz", "ufs2/freebsd_8.2.vdi.lz",
            "ufs2/netbsd_7.1.vdi.lz",
        };

        readonly ulong[] sectors = {
            262144, 262144, 262144, 262144,
            262144, 262144, 262144, 262144,
            262144, 262144, 262144, 262144,
            262144, 262144, 262144, 262144,
            262144, 262144, 262144, 262144,
            262144, 262144, 262144, 262144,
            262144, 262144, 262144, 262144,
            262144,
        };

        readonly uint[] sectorsize = {
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
            512,
        };

        readonly long[] clusters = {
            65024, 65018, 65024, 65018,
            65024, 65018, 65024, 65018,
            65024, 65018, 65024, 65018,
            65024, 65018, 65024, 65018,
            65024, 65018, 65024, 65018,
            65024, 65018, 65024, 65018,
            65024, 65018, 65024, 65018,
            65024,
        };

        readonly int[] clustersize = {
            2048, 2048, 2048, 2048,
            2048, 2048, 2048, 2048,
            2048, 2048, 2048, 2048,
            2048, 2048, 2048, 2048,
            2048, 2048, 2048, 2048,
            2048, 2048, 2048, 2048,
            2048, 2048, 2048, 2048,
            2048,
        };

        readonly string[] volumename = {
            "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label",
        };

        readonly string[] volumeserial = {
            "59588B778E9ACDEF", "UNKNOWN", "UNKNOWN", "UNKNOWN",
            "UNKNOWN", "UNKNOWN", "UNKNOWN", "UNKNOWN",
            "UNKNOWN", "UNKNOWN", "UNKNOWN", "UNKNOWN",
            "UNKNOWN", "UNKNOWN", "UNKNOWN", "UNKNOWN",
            "UNKNOWN", "UNKNOWN", "UNKNOWN", "UNKNOWN",
            "UNKNOWN", "UNKNOWN", "UNKNOWN", "UNKNOWN",
            "UNKNOWN", "UNKNOWN", "UNKNOWN", "UNKNOWN",
            "UNKNOWN",
        };

        readonly string[] type = {
            "UFS", "UFS2", "UFS", "UFS",
            "UFS", "UFS", "UFS", "UFS",
            "UFS", "UFS", "UFS", "UFS",
            "UFS", "UFS", "UFS", "UFS",
            "UFS", "UFS", "UFS", "UFS",
            "UFS", "UFS", "UFS", "UFS",
            "UFS", "UFS2", "UFS2", "UFS2",
            "UFS2",
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "ufs_mbr", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new VDI();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                PartPlugin parts = new MBR();
                Assert.AreEqual(true, parts.GetInformation(image, out List<Partition> partitions), testfiles[i]);
                Filesystem fs = new DiscImageChef.Filesystems.FFSPlugin();
                int part = -1;
                for(int j = 0; j < partitions.Count; j++)
                {
                    if(partitions[j].PartitionType == "0x83")
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
                Assert.AreEqual(type[i], fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSerial, testfiles[i]);
            }
        }
    }
}
