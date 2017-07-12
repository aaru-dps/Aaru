// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HFS_CDROM.cs
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
    public class HFS_CDROM
    {
        readonly string[] testfiles = {
            "toast_3.5.7_hfs_from_volume.iso.lz","toast_3.5.7_iso9660_hfs.iso.lz",
            "toast_4.1.3_hfs_from_volume.iso.lz","toast_4.1.3_iso9660_hfs.iso.lz",
            // TODO: These two expect the CD-ROM to return 512 bytes sectors, that is something DIC doesn't if the extension is .iso
            "toast_3.5.7_hfs_from_files.bin.lz","toast_4.1.3_hfs_from_files.bin.lz",
        };

        readonly ulong[] sectors = {
            942,1880,
            943,1882,
            6036,6116,
        };

        readonly uint[] sectorsize = {
            2048,2048,
            2048,2048,
            512,512,
        };

        readonly long[] clusters = {
            3724,931,
            931,931,
            249,249,
        };

        readonly int[] clustersize = {
            512,2048,
            2048,2048,
            12288,12288,
        };

        readonly string[] volumename = {
            "Disk utils","Disk utils","Disk utils",
            "Disk utils","Disk utils","Disk utils",
        };

        readonly string[] volumeserial = {
            null,null,null,
            null,null,null,
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "hfs_cdrom", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new ZZZRawImage();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                PartPlugin parts = new AppleMap();
                Assert.AreEqual(true, parts.GetInformation(image, out List<Partition> partitions), testfiles[i]);
                Filesystem fs = new DiscImageChef.Filesystems.AppleHFS();
                int part = -1;
                for(int j = 0; j < partitions.Count; j++)
                {
                    if(partitions[j].PartitionType == "Apple_HFS")
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
                Assert.AreEqual("HFS", fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSerial, testfiles[i]);
            }
        }
    }
}
