// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : UNIXBFS.cs
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
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using Aaru.Filesystems;
using Aaru.Filters;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems
{
    [TestFixture]
    public class Unixbfs
    {
        readonly string[] _testfiles =
        {
            "amix_mf2dd.adf.lz", "att_unix_svr4v2.1_dsdd.img.lz", "att_unix_svr4v2.1_dshd.img.lz",
            "att_unix_svr4v2.1_mf2dd.img.lz", "att_unix_svr4v2.1_mf2hd.img.lz"
        };

        readonly MediaType[] _mediatypes =
        {
            MediaType.CBM_AMIGA_35_DD, MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD
        };

        readonly ulong[] _sectors =
        {
            1760, 720, 2400, 1440, 2880
        };

        readonly uint[] _sectorsize =
        {
            512, 512, 512, 512, 512
        };

        readonly long[] _clusters =
        {
            1760, 720, 2400, 1440, 2880
        };

        readonly int[] _clustersize =
        {
            512, 512, 512, 512, 512
        };

        readonly string[] _volumename =
        {
            "Label", null, null, null, null
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Boot File System",
                                               _testfiles[i]);

                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true, image.Open(filter), _testfiles[i]);
                Assert.AreEqual(_mediatypes[i], image.Info.MediaType, _testfiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testfiles[i]);
                Assert.AreEqual(_sectorsize[i], image.Info.SectorSize, _testfiles[i]);
                IFilesystem fs = new BFS();

                var wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };

                Assert.AreEqual(true, fs.Identify(image, wholePart), _testfiles[i]);
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testfiles[i]);
                Assert.AreEqual(_clustersize[i], fs.XmlFsType.ClusterSize, _testfiles[i]);
                Assert.AreEqual("BFS", fs.XmlFsType.Type, _testfiles[i]);
                Assert.AreEqual(_volumename[i], fs.XmlFsType.VolumeName, _testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class UnixbfsMbr
    {
        readonly string[] _testfiles =
        {
            "linux.aif"
        };

        readonly ulong[] _sectors =
        {
            262144
        };

        readonly uint[] _sectorsize =
        {
            512
        };

        readonly long[] _clusters =
        {
            260096
        };

        readonly int[] _clustersize =
        {
            512
        };

        readonly string[] _volumename =
        {
            "Label"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Boot File System (MBR)",
                                               _testfiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testfiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testfiles[i]);
                Assert.AreEqual(_sectorsize[i], image.Info.SectorSize, _testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new BFS();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x83")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testfiles[i]);
                Assert.AreEqual(_clustersize[i], fs.XmlFsType.ClusterSize, _testfiles[i]);
                Assert.AreEqual("BFS", fs.XmlFsType.Type, _testfiles[i]);
                Assert.AreEqual(_volumename[i], fs.XmlFsType.VolumeName, _testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class UnixbfsRdb
    {
        readonly string[] _testfiles =
        {
            "amix.aif"
        };

        readonly ulong[] _sectors =
        {
            1024128
        };

        readonly uint[] _sectorsize =
        {
            512
        };

        readonly long[] _clusters =
        {
            65024
        };

        readonly int[] _clustersize =
        {
            2048
        };

        readonly string[] _volumename =
        {
            null
        };

        readonly string[] _volumeserial =
        {
            "UNKNOWN"
        };

        readonly string[] _type =
        {
            "UFS"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Boot File System (RDB)",
                                               _testfiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testfiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testfiles[i]);
                Assert.AreEqual(_sectorsize[i], image.Info.SectorSize, _testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new BFS();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "\"UNI\\0\"")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testfiles[i]);
                Assert.AreEqual(_clustersize[i], fs.XmlFsType.ClusterSize, _testfiles[i]);
                Assert.AreEqual(_type[i], fs.XmlFsType.Type, _testfiles[i]);
                Assert.AreEqual(_volumename[i], fs.XmlFsType.VolumeName, _testfiles[i]);
                Assert.AreEqual(_volumeserial[i], fs.XmlFsType.VolumeSerial, _testfiles[i]);
            }
        }
    }
}