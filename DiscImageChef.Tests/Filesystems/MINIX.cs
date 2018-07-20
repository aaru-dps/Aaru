// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MINIX.cs
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
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.DiscImages;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using NUnit.Framework;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class MinixV1
    {
        readonly string[] testfiles =
        {
            "minix_3.1.2a_dsdd.img.lz", "minix_3.1.2a_dshd.img.lz", "minix_3.1.2a_mf2dd.img.lz",
            "minix_3.1.2a_mf2hd.img.lz"
        };

        readonly MediaType[] mediatypes =
            {MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD};

        readonly ulong[] sectors = {720, 2400, 1440, 2880};

        readonly uint[] sectorsize = {512, 512, 512, 512};

        readonly long[] clusters = {360, 1200, 720, 1440};

        readonly int[] clustersize = {1024, 1024, 1024, 1024};

        readonly string[] types = {"Minix 3 v1", "Minix 3 v1", "Minix 3 v1", "Minix 3 v1"};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "minixv1", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.Info.MediaType,  testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                IFilesystem fs = new MinixFS();
                Partition wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };
                Assert.AreEqual(true, fs.Identify(image, wholePart), testfiles[i]);
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(clusters[i],    fs.XmlFsType.Clusters,    testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFsType.ClusterSize, testfiles[i]);
                Assert.AreEqual(types[i],       fs.XmlFsType.Type,        testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class MinixV1Mbr
    {
        readonly string[] testfiles = {"linux.vdi.lz", "minix_3.1.2a.vdi.lz"};

        readonly ulong[] sectors = {262144, 102400};

        readonly uint[] sectorsize = {512, 512};

        readonly long[] clusters = {65535, 50399};

        readonly int[] clustersize = {1024, 1024};

        readonly string[] types = {"Minix v1", "Minix 3 v1"};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "minixv1_mbr", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new MinixFS();
                int             part       = -1;
                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x80" || partitions[j].Type == "0x81" || partitions[j].Type == "MINIX")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],    fs.XmlFsType.Clusters,    testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFsType.ClusterSize, testfiles[i]);
                Assert.AreEqual(types[i],       fs.XmlFsType.Type,        testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class MinixV2
    {
        readonly string[] testfiles =
        {
            "minix_3.1.2a_dsdd.img.lz", "minix_3.1.2a_dshd.img.lz", "minix_3.1.2a_mf2dd.img.lz",
            "minix_3.1.2a_mf2hd.img.lz"
        };

        readonly MediaType[] mediatypes =
            {MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD};

        readonly ulong[] sectors = {720, 2400, 1440, 2880};

        readonly uint[] sectorsize = {512, 512, 512, 512};

        readonly long[] clusters = {360, 1200, 720, 1440};

        readonly int[] clustersize = {1024, 1024, 1024, 1024};

        readonly string[] types = {"Minix 3 v2", "Minix 3 v2", "Minix 3 v2", "Minix 3 v2"};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "minixv2", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.Info.MediaType,  testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                IFilesystem fs = new MinixFS();
                Partition wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };
                Assert.AreEqual(true, fs.Identify(image, wholePart), testfiles[i]);
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(clusters[i],    fs.XmlFsType.Clusters,    testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFsType.ClusterSize, testfiles[i]);
                Assert.AreEqual(types[i],       fs.XmlFsType.Type,        testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class MinixV2Mbr
    {
        readonly string[] testfiles = {"minix_3.1.2a.vdi.lz"};

        readonly ulong[] sectors = {1024000};

        readonly uint[] sectorsize = {512};

        readonly long[] clusters = {511055};

        readonly int[] clustersize = {1024};

        readonly string[] types = {"Minix 3 v2"};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "minixv2_mbr", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new MinixFS();
                int             part       = -1;
                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x81" || partitions[j].Type == "MINIX")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],    fs.XmlFsType.Clusters,    testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFsType.ClusterSize, testfiles[i]);
                Assert.AreEqual(types[i],       fs.XmlFsType.Type,        testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class MinixV3
    {
        readonly string[] testfiles =
        {
            "minix_3.1.2a_dsdd.img.lz", "minix_3.1.2a_dshd.img.lz", "minix_3.1.2a_mf2dd.img.lz",
            "minix_3.1.2a_mf2hd.img.lz"
        };

        readonly MediaType[] mediatypes =
            {MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD};

        readonly ulong[] sectors = {720, 2400, 1440, 2880};

        readonly uint[] sectorsize = {512, 512, 512, 512};

        readonly long[] clusters = {90, 300, 180, 360};

        readonly int[] clustersize = {4096, 4096, 4096, 4096};

        readonly string[] types = {"Minix v3", "Minix v3", "Minix v3", "Minix v3"};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "minixv3", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.Info.MediaType,  testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                IFilesystem fs = new MinixFS();
                Partition wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };
                Assert.AreEqual(true, fs.Identify(image, wholePart), testfiles[i]);
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(clusters[i],    fs.XmlFsType.Clusters,    testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFsType.ClusterSize, testfiles[i]);
                Assert.AreEqual(types[i],       fs.XmlFsType.Type,        testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class MinixV3Mbr
    {
        readonly string[] testfiles = {"minix_3.1.2a.vdi.lz"};

        readonly ulong[] sectors = {4194304};

        readonly uint[] sectorsize = {512};

        readonly long[] clusters = {523151};

        readonly int[] clustersize = {4096};

        readonly string[] types = {"Minix v3"};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "minixv3_mbr", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new MinixFS();
                int             part       = -1;
                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x81" || partitions[j].Type == "MINIX")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],    fs.XmlFsType.Clusters,    testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFsType.ClusterSize, testfiles[i]);
                Assert.AreEqual(types[i],       fs.XmlFsType.Type,        testfiles[i]);
            }
        }
    }
}