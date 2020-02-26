// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SFS_MBR.cs
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
using NUnit.Framework;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class SfsMbr
    {
        readonly string[] testfiles = {"aros.vdi.lz"};

        readonly ulong[] sectors = {409600};

        readonly uint[] sectorsize = {512};

        readonly long[] clusters = {408240};

        readonly int[] clustersize = {512};

        readonly string[] volumename = {null};

        readonly string[] volumeserial = {null};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "sfs_mbr", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new SFS();
                int             part       = -1;
                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x2F")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],       fs.XmlFsType.Clusters,     testfiles[i]);
                Assert.AreEqual(clustersize[i],    fs.XmlFsType.ClusterSize,  testfiles[i]);
                Assert.AreEqual("SmartFileSystem", fs.XmlFsType.Type,         testfiles[i]);
                Assert.AreEqual(volumename[i],     fs.XmlFsType.VolumeName,   testfiles[i]);
                Assert.AreEqual(volumeserial[i],   fs.XmlFsType.VolumeSerial, testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class SfsMbrRdb
    {
        readonly string[] testfiles = {"aros.vdi.lz"};

        readonly ulong[] sectors = {409600};

        readonly uint[] sectorsize = {512};

        readonly long[] clusters = {406224};

        readonly int[] clustersize = {512};

        readonly string[] volumename = {null};

        readonly string[] volumeserial = {null};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "sfs_mbr_rdb", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new SFS();
                int             part       = -1;
                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "\"SFS\\0\"")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],       fs.XmlFsType.Clusters,     testfiles[i]);
                Assert.AreEqual(clustersize[i],    fs.XmlFsType.ClusterSize,  testfiles[i]);
                Assert.AreEqual("SmartFileSystem", fs.XmlFsType.Type,         testfiles[i]);
                Assert.AreEqual(volumename[i],     fs.XmlFsType.VolumeName,   testfiles[i]);
                Assert.AreEqual(volumeserial[i],   fs.XmlFsType.VolumeSerial, testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class SfsRdb
    {
        readonly string[] testfiles = {"uae.vdi.lz", "aros.vdi.lz", "amigaos_4.0.vdi.lz", "amigaos_4.0_sfs2.vdi.lz"};

        readonly ulong[] sectors = {1024128, 409600, 1024128, 1024128};

        readonly uint[] sectorsize = {512, 512, 512, 512};

        readonly long[] clusters = {127000, 407232, 511040, 511040};

        readonly int[] clustersize = {2048, 512, 1024, 1024};

        readonly string[] volumename = {null, null, null, null};

        readonly string[] volumeserial = {null, null, null, null};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "sfs_rdb", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new SFS();
                int             part       = -1;
                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "\"SFS\\0\"" || partitions[j].Type == "\"SFS\\2\"")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],       fs.XmlFsType.Clusters,     testfiles[i]);
                Assert.AreEqual(clustersize[i],    fs.XmlFsType.ClusterSize,  testfiles[i]);
                Assert.AreEqual("SmartFileSystem", fs.XmlFsType.Type,         testfiles[i]);
                Assert.AreEqual(volumename[i],     fs.XmlFsType.VolumeName,   testfiles[i]);
                Assert.AreEqual(volumeserial[i],   fs.XmlFsType.VolumeSerial, testfiles[i]);
            }
        }
    }
}