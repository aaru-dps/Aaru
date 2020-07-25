// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SysV.cs
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
    public class SysV
    {
        readonly string[] _testFiles =
        {
            "amix.adf.lz", "att_unix_svr4v2.1_dsdd.img.lz", "att_unix_svr4v2.1_mf2dd.img.lz",
            "att_unix_svr4v2.1_mf2hd.img.lz", "scoopenserver_5.0.7hw_dmf.img.lz", "scoopenserver_5.0.7hw_dshd.img.lz",
            "scoopenserver_5.0.7hw_mf2dd.img.lz", "scoopenserver_5.0.7hw_mf2ed.img.lz",
            "scoopenserver_5.0.7hw_mf2hd.img.lz"
        };

        readonly MediaType[] _mediaTypes =
        {
            MediaType.CBM_AMIGA_35_DD, MediaType.DOS_525_DS_DD_9, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD,
            MediaType.DMF, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED, MediaType.DOS_35_HD
        };

        readonly ulong[] _sectors =
        {
            1760, 720, 1440, 2880, 3360, 2400, 1440, 5760, 2880
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] _clusters =
        {
            880, 360, 720, 1440, 1680, 1200, 720, 2880, 1440
        };

        readonly int[] _clusterSize =
        {
            1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024
        };

        readonly string[] _volumeName =
        {
            "", "", "", "", "", "", "", "", ""
        };

        readonly string[] _volumeSerial =
        {
            null, null, null, null, null, null, null, null, null
        };

        readonly string[] _type =
        {
            "SVR4 fs", "SVR4 fs", "SVR4 fs", "SVR4 fs", "SVR4 fs", "SVR4 fs", "SVR4 fs", "SVR4 fs", "SVR4 fs"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "System V filesystem",
                                               _testFiles[i]);

                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                IFilesystem fs = new SysVfs();

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
                Assert.AreEqual(_type[i], fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class SysVMbr
    {
        readonly string[] _testFiles =
        {
            "att_unix_svr4v2.1.aif", "att_unix_svr4v2.1_2k.aif", "scoopenserver_5.0.7hw.aif"
        };

        readonly ulong[] _sectors =
        {
            1024000, 1024000, 2097152
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512
        };

        readonly long[] _clusters =
        {
            511056, 255528, 1020096
        };

        readonly int[] _clusterSize =
        {
            1024, 2048, 1024
        };

        readonly string[] _volumeName =
        {
            "/usr3", "/usr3", "Volume label"
        };

        readonly string[] _volumeSerial =
        {
            null, null, null
        };

        readonly string[] _type =
        {
            "SVR4 fs", "SVR4 fs", "SVR4 fs"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "System V filesystem (MBR)",
                                               _testFiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new SysVfs();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "UNIX: /usr" ||
                       partitions[j].Type == "XENIX")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual(_type[i], fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class SysVRdb
    {
        readonly string[] _testFiles =
        {
            "amix.aif"
        };

        readonly ulong[] _sectors =
        {
            1024128
        };

        readonly uint[] _sectorSize =
        {
            512
        };

        readonly long[] _clusters =
        {
            511424
        };

        readonly int[] _clusterSize =
        {
            1024
        };

        readonly string[] _volumeName =
        {
            ""
        };

        readonly string[] _volumeSerial =
        {
            null
        };

        readonly string[] _type =
        {
            "SVR4 fs"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "System V filesystem (RDB)",
                                               _testFiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new SysVfs();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "\"UNI\\1\"")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual(_type[i], fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
            }
        }
    }
}