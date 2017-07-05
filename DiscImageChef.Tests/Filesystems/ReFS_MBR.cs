// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : NTFS_MBR.cs
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
    public class NTFS_MBR
    {
        readonly string[] testfiles = {
            "win10.vdi.lz", "win2000.vdi.lz", "winnt_3.10.vdi.lz", "winnt_3.50.vdi.lz",
            "winnt_3.51.vdi.lz", "winnt_4.00.vdi.lz", "winvista.vdi.lz",

        };

        readonly ulong[] sectors = {
            524288, 2097152, 1024000, 524288,
            524288, 524288, 524288,
        };

        readonly uint[] sectorsize = {
            512, 512, 512, 512,
            512, 512, 512
        };

        readonly long[] clusters = {
            65263, 1046511, 1023057, 524256, 
            524256, 524096, 64767,
        };

        readonly int[] clustersize = {
            4096, 1024, 512, 512,
            512, 512, 4096,
        };

        readonly string[] volumename = {
            null, null, null, null,
            null, null, null,
        };

        readonly string[] volumeserial = {
            "C46C1B3C6C1B28A6","8070C8EC70C8E9CC","10CC6AC6CC6AA5A6","7A14F50014F4BFE5",
            "24884447884419A6","822C288D2C287E73","E20AF54B0AF51D6B",
        };

        readonly string[] oemid = {
            null, null, null, null,
            null, null, null,
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "ntfs_mbr", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new VDI();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                PartPlugin parts = new MBR();
                Assert.AreEqual(true, parts.GetInformation(image, out List<Partition> partitions), testfiles[i]);
                Filesystem fs = new DiscImageChef.Filesystems.NTFS();
                int part = -1;
                for(int j = 0; j < partitions.Count; j++)
                {
                    if(partitions[j].PartitionType == "0x07")
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
                Assert.AreEqual("NTFS", fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSerial, testfiles[i]);
                Assert.AreEqual(oemid[i], fs.XmlFSType.SystemIdentifier, testfiles[i]);
            }
        }
    }
}
