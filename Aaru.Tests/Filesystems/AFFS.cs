// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AFFS.cs
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
    public class Affs
    {
        readonly string[] _testfiles =
        {
            "amigaos_3.9.adf.lz", "amigaos_3.9_intl.adf.lz"
        };

        readonly MediaType[] _mediatypes =
        {
            MediaType.CBM_AMIGA_35_DD, MediaType.CBM_AMIGA_35_DD
        };

        readonly ulong[] _sectors =
        {
            1760, 1760
        };

        readonly uint[] _sectorsize =
        {
            512, 512
        };

        readonly long[] _clusters =
        {
            1760, 1760
        };

        readonly int[] _clustersize =
        {
            512, 512
        };

        readonly string[] _volumename =
        {
            "Volume label", "Volume label"
        };

        readonly string[] _volumeserial =
        {
            "A5D9FAE2", "A5DA0CC9"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Amiga Fast File System",
                                               _testfiles[i]);

                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true, image.Open(filter), _testfiles[i]);
                Assert.AreEqual(_mediatypes[i], image.Info.MediaType, _testfiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testfiles[i]);
                Assert.AreEqual(_sectorsize[i], image.Info.SectorSize, _testfiles[i]);
                IFilesystem fs = new AmigaDOSPlugin();

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
                Assert.AreEqual("Amiga FFS", fs.XmlFsType.Type, _testfiles[i]);
                Assert.AreEqual(_volumename[i], fs.XmlFsType.VolumeName, _testfiles[i]);
                Assert.AreEqual(_volumeserial[i], fs.XmlFsType.VolumeSerial, _testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class AffsMbr
    {
        readonly string[] _testfiles =
        {
            "aros.aif", "aros_intl.aif"
        };

        readonly ulong[] _sectors =
        {
            409600, 409600
        };

        readonly uint[] _sectorsize =
        {
            512, 512
        };

        readonly long[] _clusters =
        {
            408240, 408240
        };

        readonly int[] _clustersize =
        {
            512, 512
        };

        readonly string[] _volumename =
        {
            "Volume label", "Volume label"
        };

        readonly string[] _volumeserial =
        {
            "A582DCA4", "A582BC91"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Amiga Fast File System (MBR)",
                                               _testfiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testfiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testfiles[i]);
                Assert.AreEqual(_sectorsize[i], image.Info.SectorSize, _testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AmigaDOSPlugin();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x2D" ||
                       partitions[j].Type == "0x2E")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testfiles[i]);
                Assert.AreEqual(_clustersize[i], fs.XmlFsType.ClusterSize, _testfiles[i]);
                Assert.AreEqual("Amiga FFS", fs.XmlFsType.Type, _testfiles[i]);
                Assert.AreEqual(_volumename[i], fs.XmlFsType.VolumeName, _testfiles[i]);
                Assert.AreEqual(_volumeserial[i], fs.XmlFsType.VolumeSerial, _testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class AffsMbrRdb
    {
        readonly string[] _testfiles =
        {
            "aros.aif", "aros_intl.aif"
        };

        readonly ulong[] _sectors =
        {
            409600, 409600
        };

        readonly uint[] _sectorsize =
        {
            512, 512
        };

        readonly long[] _clusters =
        {
            406224, 406224
        };

        readonly int[] _clustersize =
        {
            512, 512
        };

        readonly string[] _volumename =
        {
            "Volume label", "Volume label"
        };

        readonly string[] _volumeserial =
        {
            "A58348CE", "A5833CD0"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems",
                                               "Amiga Fast File System (MBR+RDB)", _testfiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testfiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testfiles[i]);
                Assert.AreEqual(_sectorsize[i], image.Info.SectorSize, _testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AmigaDOSPlugin();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "\"DOS\\1\"" ||
                       partitions[j].Type == "\"DOS\\3\"")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testfiles[i]);
                Assert.AreEqual(_clustersize[i], fs.XmlFsType.ClusterSize, _testfiles[i]);
                Assert.AreEqual("Amiga FFS", fs.XmlFsType.Type, _testfiles[i]);
                Assert.AreEqual(_volumename[i], fs.XmlFsType.VolumeName, _testfiles[i]);
                Assert.AreEqual(_volumeserial[i], fs.XmlFsType.VolumeSerial, _testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class AffsRdb
    {
        readonly string[] _testfiles =
        {
            "amigaos_3.9.aif", "amigaos_3.9_intl.aif", "aros.aif", "aros_intl.aif", "amigaos_4.0.aif",
            "amigaos_4.0_intl.aif", "amigaos_4.0_cache.aif"
        };

        readonly ulong[] _sectors =
        {
            1024128, 1024128, 409600, 409600, 1024128, 1024128, 1024128
        };

        readonly uint[] _sectorsize =
        {
            512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] _clusters =
        {
            510032, 510032, 407232, 407232, 511040, 511040, 511040
        };

        readonly int[] _clustersize =
        {
            1024, 1024, 512, 512, 1024, 1024, 1024
        };

        readonly string[] _volumename =
        {
            "Volume label", "Volume label", "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label"
        };

        readonly string[] _volumeserial =
        {
            "A56D0F5C", "A56D049C", "A58307A9", "A58304BE", "A56CC7EE", "A56CDDC4", "A56CC133"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Amiga Fast File System (RDB)",
                                               _testfiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testfiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testfiles[i]);
                Assert.AreEqual(_sectorsize[i], image.Info.SectorSize, _testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AmigaDOSPlugin();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "\"DOS\\1\"" ||
                       partitions[j].Type == "\"DOS\\3\"" ||
                       partitions[j].Type == "\"DOS\\5\"")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testfiles[i]);
                Assert.AreEqual(_clustersize[i], fs.XmlFsType.ClusterSize, _testfiles[i]);
                Assert.AreEqual("Amiga FFS", fs.XmlFsType.Type, _testfiles[i]);
                Assert.AreEqual(_volumename[i], fs.XmlFsType.VolumeName, _testfiles[i]);
                Assert.AreEqual(_volumeserial[i], fs.XmlFsType.VolumeSerial, _testfiles[i]);
            }
        }
    }
}