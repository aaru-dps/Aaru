// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AFFS_RDB.cs
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
    public class AffsRdb
    {
        readonly string[] testfiles =
        {
            "amigaos_3.9.vdi.lz", "amigaos_3.9_intl.vdi.lz", "aros.vdi.lz", "aros_intl.vdi.lz", "amigaos_4.0.vdi.lz",
            "amigaos_4.0_intl.vdi.lz", "amigaos_4.0_cache.vdi.lz"
        };

        readonly ulong[] sectors = {1024128, 1024128, 409600, 409600, 1024128, 1024128, 1024128};

        readonly uint[] sectorsize = {512, 512, 512, 512, 512, 512, 512};

        readonly long[] clusters = {510032, 510032, 407232, 407232, 511040, 511040, 511040};

        readonly int[] clustersize = {1024, 1024, 512, 512, 1024, 1024, 1024};

        readonly string[] volumename =
        {
            "Volume label", "Volume label", "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label"
        };

        readonly string[] volumeserial =
            {"A56D0F5C", "A56D049C", "A58307A9", "A58304BE", "A56CC7EE", "A56CDDC4", "A56CC133"};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "affs_rdb", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new Vdi();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                Filesystem fs = new AmigaDOSPlugin();
                int part = -1;
                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "\"DOS\\1\"" || partitions[j].Type == "\"DOS\\3\"" ||
                       partitions[j].Type == "\"DOS\\5\"")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _);
                Assert.AreEqual(clusters[i], fs.XmlFSType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFSType.ClusterSize, testfiles[i]);
                Assert.AreEqual("Amiga FFS", fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSerial, testfiles[i]);
            }
        }
    }
}