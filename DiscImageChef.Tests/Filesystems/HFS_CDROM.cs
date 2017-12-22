// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HFS_CDROM.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef unit testing.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using NUnit.Framework;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class HfsCdrom
    {
        readonly string[] testfiles =
        {
            "toast_3.5.7_hfs_from_volume.iso.lz", "toast_3.5.7_iso9660_hfs.iso.lz",
            "toast_4.1.3_hfs_from_volume.iso.lz", "toast_4.1.3_iso9660_hfs.iso.lz", "toast_3.5.7_hfs_from_files.iso.lz",
            "toast_4.1.3_hfs_from_files.iso.lz"
        };

        readonly ulong[] sectors = {942, 1880, 943, 1882, 1509, 1529};

        readonly uint[] sectorsize = {2048, 2048, 2048, 2048, 2048, 2048};

        readonly long[] clusters = {3724, 931, 931, 931, 249, 249};

        readonly int[] clustersize = {512, 2048, 2048, 2048, 12288, 12288};

        readonly string[] volumename =
            {"Disk utils", "Disk utils", "Disk utils", "Disk utils", "Disk utils", "Disk utils"};

        readonly string[] volumeserial = {null, null, null, null, null, null};

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
                Assert.AreEqual(sectors[i], image.ImageInfo.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                Filesystem fs = new AppleHFS();
                int part = -1;
                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "Apple_HFS")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _);
                Assert.AreEqual(clusters[i], fs.XmlFSType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFSType.ClusterSize, testfiles[i]);
                Assert.AreEqual("HFS", fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSerial, testfiles[i]);
            }
        }
    }
}