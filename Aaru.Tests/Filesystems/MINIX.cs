// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MINIX.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru unit testing.
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
// Copyright © 2011-2020 Natalia Portillo
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
    public class MinixV1
    {
        readonly string[] _testFiles =
        {
            "minix_3.1.2a_dsdd.img.lz", "minix_3.1.2a_dshd.img.lz", "minix_3.1.2a_mf2dd.img.lz",
            "minix_3.1.2a_mf2hd.img.lz"
        };

        readonly MediaType[] _mediaTypes =
        {
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD
        };

        readonly ulong[] _sectors =
        {
            720, 2400, 1440, 2880
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512, 512
        };

        readonly long[] _clusters =
        {
            360, 1200, 720, 1440
        };

        readonly int[] _clusterSize =
        {
            1024, 1024, 1024, 1024
        };

        readonly string[] _types =
        {
            "Minix 3 v1", "Minix 3 v1", "Minix 3 v1", "Minix 3 v1"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "MINIX v1 filesystem",
                                               _testFiles[i]);

                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                IFilesystem fs = new MinixFS();

                var wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };

                Assert.AreEqual(true, fs.Identify(image, wholePart), _testFiles[i]);
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual(_types[i], fs.XmlFsType.Type, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class MinixV1Mbr
    {
        readonly string[] _testFiles =
        {
            "linux.aif", "minix_3.1.2a.aif", "linux_4.19_minix1_flashdrive.aif"
        };

        readonly ulong[] _sectors =
        {
            262144, 102400, 131072
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512
        };

        readonly long[] _clusters =
        {
            65535, 50399, 64512
        };

        readonly int[] _clusterSize =
        {
            1024, 1024, 1024
        };

        readonly string[] _types =
        {
            "Minix v1", "Minix 3 v1", "Minix v1"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "MINIX v1 filesystem (MBR)",
                                               _testFiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new MinixFS();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x80" ||
                       partitions[j].Type == "0x81" ||
                       partitions[j].Type == "MINIX")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual(_types[i], fs.XmlFsType.Type, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class MinixV2
    {
        readonly string[] _testFiles =
        {
            "minix_3.1.2a_dsdd.img.lz", "minix_3.1.2a_dshd.img.lz", "minix_3.1.2a_mf2dd.img.lz",
            "minix_3.1.2a_mf2hd.img.lz"
        };

        readonly MediaType[] _mediaTypes =
        {
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD
        };

        readonly ulong[] _sectors =
        {
            720, 2400, 1440, 2880
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512, 512
        };

        readonly long[] _clusters =
        {
            360, 1200, 720, 1440
        };

        readonly int[] _clusterSize =
        {
            1024, 1024, 1024, 1024
        };

        readonly string[] _types =
        {
            "Minix 3 v2", "Minix 3 v2", "Minix 3 v2", "Minix 3 v2"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "MINIX v2 filesystem",
                                               _testFiles[i]);

                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                IFilesystem fs = new MinixFS();

                var wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };

                Assert.AreEqual(true, fs.Identify(image, wholePart), _testFiles[i]);
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual(_types[i], fs.XmlFsType.Type, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class MinixV2Mbr
    {
        readonly string[] _testFiles =
        {
            "minix_3.1.2a.aif", "linux_4.19_minix2_flashdrive.aif"
        };

        readonly ulong[] _sectors =
        {
            1024000, 1024000
        };

        readonly uint[] _sectorSize =
        {
            512, 512
        };

        readonly long[] _clusters =
        {
            511055, 510976
        };

        readonly int[] _clusterSize =
        {
            1024, 1024
        };

        readonly string[] _types =
        {
            "Minix 3 v2", "Minix v2"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "MINIX v2 filesystem (MBR)",
                                               _testFiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new MinixFS();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x81" ||
                       partitions[j].Type == "MINIX")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual(_types[i], fs.XmlFsType.Type, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class MinixV3
    {
        readonly string[] _testFiles =
        {
            "minix_3.1.2a_dsdd.img.lz", "minix_3.1.2a_dshd.img.lz", "minix_3.1.2a_mf2dd.img.lz",
            "minix_3.1.2a_mf2hd.img.lz"
        };

        readonly MediaType[] _mediaTypes =
        {
            MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD
        };

        readonly ulong[] _sectors =
        {
            720, 2400, 1440, 2880
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512, 512
        };

        readonly long[] _clusters =
        {
            90, 300, 180, 360
        };

        readonly int[] _clusterSize =
        {
            4096, 4096, 4096, 4096
        };

        readonly string[] _types =
        {
            "Minix v3", "Minix v3", "Minix v3", "Minix v3"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "MINIX v3 filesystem",
                                               _testFiles[i]);

                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                IFilesystem fs = new MinixFS();

                var wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };

                Assert.AreEqual(true, fs.Identify(image, wholePart), _testFiles[i]);
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual(_types[i], fs.XmlFsType.Type, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class MinixV3Mbr
    {
        readonly string[] _testFiles =
        {
            "minix_3.1.2a.aif", "linux_4.19_minix3_flashdrive.aif"
        };

        readonly ulong[] _sectors =
        {
            4194304, 1024000
        };

        readonly uint[] _sectorSize =
        {
            512, 512
        };

        readonly long[] _clusters =
        {
            523151, 510976
        };

        readonly int[] _clusterSize =
        {
            4096, 1024
        };

        readonly string[] _types =
        {
            "Minix v3", "Minix v3"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "MINIX v3 filesystem (MBR)",
                                               _testFiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new MinixFS();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x81" ||
                       partitions[j].Type == "MINIX")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual(_types[i], fs.XmlFsType.Type, _testFiles[i]);
            }
        }
    }
}