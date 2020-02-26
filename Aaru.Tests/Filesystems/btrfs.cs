// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : btrfs.cs
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.DiscImages;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.Partitions;
using NUnit.Framework;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class Btrfs
    {
        readonly string[] testfiles = {"linux.vdi.lz", "linux_4.19_btrfs_flashdrive.vdi.lz"};

        readonly ulong[] sectors = {262144, 1024000};

        readonly uint[] sectorsize = {512, 512};

        readonly long[] clusters = {32512, 127744};

        readonly int[] clustersize = {4096, 4096};

        readonly string[] volumename = {"VolumeLabel", "btrfs"};

        readonly string[] volumeserial =
        {
            "a4fc5201-85cc-6840-8a68-998cab9ae897", "5af44541-0605-f541-af6d-c229576707ab"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "btrfs", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                IPartition parts = new MBR();
                Assert.AreEqual(true, parts.GetInformation(image, out List<Partition> partitions, 0), testfiles[i]);
                IFilesystem fs   = new BTRFS();
                int         part = -1;
                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x83")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],          fs.XmlFsType.Clusters,     testfiles[i]);
                Assert.AreEqual(clustersize[i],       fs.XmlFsType.ClusterSize,  testfiles[i]);
                Assert.AreEqual("B-tree file system", fs.XmlFsType.Type,         testfiles[i]);
                Assert.AreEqual(volumename[i],        fs.XmlFsType.VolumeName,   testfiles[i]);
                Assert.AreEqual(volumeserial[i],      fs.XmlFsType.VolumeSerial, testfiles[i]);
            }
        }
    }
}