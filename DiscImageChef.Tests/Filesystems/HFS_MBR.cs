// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HFS_MBR.cs
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
using DiscImageChef.ImagePlugins;
using NUnit.Framework;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class HFS_MBR
    {
        readonly string[] testfiles =
        {
            "linux.vdi.lz", "darwin_1.3.1.vdi.lz", "darwin_1.4.1.vdi.lz", "darwin_6.0.2.vdi.lz", "darwin_8.0.1.vdi.lz",
        };

        readonly ulong[] sectors = {262144, 409600, 409600, 409600, 409600,};

        readonly uint[] sectorsize = {512, 512, 512, 512, 512,};

        readonly long[] clusters = {65018, 51145, 51145, 58452, 58502,};

        readonly int[] clustersize = {2048, 4096, 4096, 3584, 3584,};

        readonly string[] volumename =
            {"Volume label", "Volume label", "Volume label", "Volume label", "Volume label",};

        readonly string[] volumeserial = {null, null, null, null, "81FE805D61458753",};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "hfs_mbr", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new VDI();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                Filesystem fs = new DiscImageChef.Filesystems.AppleHFS();
                int part = -1;
                for(int j = 0; j < partitions.Count; j++)
                {
                    if(partitions[j].Type == "0xAF")
                    {
                        part = j;
                        break;
                    }
                }

                Assert.AreNotEqual(-1, part, string.Format("Partition not found on {0}", testfiles[i]));
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out string information);
                Assert.AreEqual(clusters[i], fs.XmlFSType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFSType.ClusterSize, testfiles[i]);
                Assert.AreEqual("HFS", fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSerial, testfiles[i]);
            }
        }
    }
}