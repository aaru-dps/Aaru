// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HFSX.cs
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
// Copyright © 2011-2021 Natalia Portillo
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
    public class HfsxApm
    {
        readonly string[] _testFiles =
        {
            "macosx_10.11.aif", "macosx_10.11_journal.aif", "darwin_8.0.1_journal.aif", "darwin_8.0.1.aif",
            "macosx_10.4_journal.aif", "macosx_10.4.aif"
        };

        readonly ulong[] _sectors =
        {
            819200, 1228800, 1638400, 1433600, 4194304, 1024000
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512, 512, 512, 512
        };

        readonly long[] _clusters =
        {
            102390, 153590, 204792, 179192, 491290, 127770
        };

        readonly int[] _clusterSize =
        {
            4096, 4096, 4096, 4096, 4096, 4096
        };

        readonly string[] _volumeName =
        {
            null, null, null, null, null, null
        };

        readonly string[] _volumeSerial =
        {
            "CC2D56884950D9AE", "7AF1175D8EA7A072", "BB4ABD7E7E2FF5AF", "E2F212D815EF77B5", "5A8C646A5D77EB16",
            "258C51A750F6A485"
        };

        readonly string[] _oemId =
        {
            "10.0", "HFSJ", "10.0", "10.0", "HFSJ", "10.0"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Apple HFSX (APM)",
                                               _testFiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AppleHFSPlus();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "Apple_HFSX")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual("HFSX", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
                Assert.AreEqual(_oemId[i], fs.XmlFsType.SystemIdentifier, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class HfsxGpt
    {
        readonly string[] _testFiles =
        {
            "macosx_10.11.aif", "macosx_10.11_journal.aif"
        };

        readonly ulong[] _sectors =
        {
            819200, 1228800
        };

        readonly uint[] _sectorSize =
        {
            512, 512
        };

        readonly long[] _clusters =
        {
            102390, 153590
        };

        readonly int[] _clusterSize =
        {
            4096, 4096
        };

        readonly string[] _volumeName =
        {
            null, null
        };

        readonly string[] _volumeSerial =
        {
            "328343989312AE9F", "FB98504073464C5C"
        };

        readonly string[] _oemId =
        {
            "10.0", "HFSJ"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Apple HFSX (GPT)",
                                               _testFiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AppleHFSPlus();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "Apple HFS")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual("HFSX", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
                Assert.AreEqual(_oemId[i], fs.XmlFsType.SystemIdentifier, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class HfsxMbr
    {
        readonly string[] _testFiles =
        {
            "macosx_10.11.aif", "macosx_10.11_journal.aif", "linux.aif", "linux_journal.aif",
            "darwin_8.0.1_journal.aif", "darwin_8.0.1.aif", "linux_4.19_hfsx_flashdrive.aif"
        };

        readonly ulong[] _sectors =
        {
            393216, 409600, 262144, 262144, 1638400, 1433600, 1024000
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] _clusters =
        {
            49140, 51187, 32512, 32512, 204792, 179192, 127744
        };

        readonly int[] _clusterSize =
        {
            4096, 4096, 4096, 4096, 4096, 4096, 4096
        };

        readonly string[] _volumeName =
        {
            null, null, null, null, null, null, null
        };

        readonly string[] _volumeSerial =
        {
            "C2BCCCE6DE5BC98D", "AC54CD78C75CC30F", null, null, "7559DD01BCFADD9A", "AEA39CFBBF14C0FF",
            "5E4A8781D3C9286C"
        };

        readonly string[] _oemId =
        {
            "10.0", "HFSJ", "10.0", "10.0", "10.0", "10.0", "H+Lx"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Apple HFSX (MBR)",
                                               _testFiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AppleHFSPlus();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0xAF")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual("HFSX", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
                Assert.AreEqual(_oemId[i], fs.XmlFsType.SystemIdentifier, _testFiles[i]);
            }
        }
    }
}