// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AOFS.cs
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
    public class Aofs
    {
        readonly string[] _testFiles =
        {
            "amigaos_3.9.adf.lz", "amigaos_3.9_intl.adf.lz"
        };

        readonly MediaType[] _mediaTypes =
        {
            MediaType.CBM_AMIGA_35_DD, MediaType.CBM_AMIGA_35_DD
        };

        readonly ulong[] _sectors =
        {
            1760, 1760
        };

        readonly uint[] _sectorSize =
        {
            512, 512
        };

        readonly long[] _clusters =
        {
            1760, 1760
        };

        readonly int[] _clusterSize =
        {
            512, 512
        };

        readonly string[] _volumeName =
        {
            "Volume label", "Volume label"
        };

        readonly string[] _volumeSerial =
        {
            "A5D9FE71", "A5D9F14F"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Amiga Old File System",
                                               _testFiles[i]);

                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                IFilesystem fs = new AmigaDOSPlugin();

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
                Assert.AreEqual("Amiga OFS", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class AofsMbr
    {
        readonly string[] _testFiles =
        {
            "aros.aif", "aros_intl.aif"
        };

        readonly ulong[] _sectors =
        {
            409600, 409600
        };

        readonly uint[] _sectorSize =
        {
            512, 512
        };

        readonly long[] _clusters =
        {
            408240, 408240
        };

        readonly int[] _clusterSize =
        {
            512, 512
        };

        readonly string[] _volumeName =
        {
            "Volume label", "Volume label"
        };

        readonly string[] _volumeSerial =
        {
            "A582C90C", "A582CE0D"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Amiga Old File System (MBR)",
                                               _testFiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AmigaDOSPlugin();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x2C")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual("Amiga OFS", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class AofsMbrRdb
    {
        readonly string[] _testFiles =
        {
            "aros.aif", "aros_intl.aif"
        };

        readonly ulong[] _sectors =
        {
            409600, 409600
        };

        readonly uint[] _sectorSize =
        {
            512, 512
        };

        readonly long[] _clusters =
        {
            406224, 406224
        };

        readonly int[] _clusterSize =
        {
            512, 512
        };

        readonly string[] _volumeName =
        {
            "Volume label", "Volume label"
        };

        readonly string[] _volumeSerial =
        {
            "A5833C5B", "A5833085"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Amiga Old File System (MBR+RDB)",
                                               _testFiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AmigaDOSPlugin();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "\"DOS\\0\"")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual("Amiga OFS", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class AofsRdb
    {
        readonly string[] _testFiles =
        {
            "amigaos_3.9.aif", "amigaos_3.9_intl.aif", "aros.aif", "aros_intl.aif"
        };

        readonly ulong[] _sectors =
        {
            1024128, 1024128, 409600, 409600
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512, 512
        };

        readonly long[] _clusters =
        {
            510032, 510032, 407232, 407232
        };

        readonly int[] _clusterSize =
        {
            1024, 1024, 512, 512
        };

        readonly string[] _volumeName =
        {
            "Volume label", "Volume label", "Volume label", "Volume label"
        };

        readonly string[] _volumeSerial =
        {
            "A56D13BB", "A56D0415", "A582F3A0", "A5830B06"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Amiga Old File System (RDB)",
                                               _testFiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AmigaDOSPlugin();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "\"DOS\\0\"" ||
                       partitions[j].Type == "\"DOS\\2\"" ||
                       partitions[j].Type == "\"DOS\\4\"")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual("Amiga OFS", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
            }
        }
    }
}