// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HFSPlus.cs
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
    public class HfsPlusApm
    {
        // Missing Darwin 1.4.1
        readonly string[] _testfiles =
        {
            "macosx_10.11.aif", "macosx_10.11_journal.aif", "darwin_1.3.1.aif", "darwin_1.3.1_wrapped.aif",
            "darwin_1.4.1_wrapped.aif", "darwin_6.0.2.aif", "darwin_6.0.2_wrapped.aif", "darwin_8.0.1_journal.aif",
            "darwin_8.0.1.aif", "darwin_8.0.1_wrapped.aif", "macos_8.1.aif", "macos_9.0.4.aif", "macos_9.1.aif",
            "macos_9.2.1.aif", "macos_9.2.2.aif", "macosx_10.2.aif", "macosx_10.3_journal.aif", "macosx_10.3.aif",
            "macosx_10.4_journal.aif", "macosx_10.4.aif"
        };

        readonly ulong[] _sectors =
        {
            409600, 614400, 819200, 614400, 614400, 819200, 614400, 1228800, 819200, 614400, 4194304, 4194304, 4194304,
            4194304, 4194304, 4194304, 2097152, 4194304, 2097152, 4194304
        };

        readonly uint[] _sectorsize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] _clusters =
        {
            51190, 76790, 102392, 76774, 76774, 102392, 76774, 153592, 102392, 76774, 524152, 524088, 524088, 524088,
            524088, 524008, 261884, 491240, 261884, 491240
        };

        readonly int[] _clustersize =
        {
            4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096,
            4096, 4096
        };

        readonly string[] _volumename =
        {
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null
        };

        readonly string[] _volumeserial =
        {
            "FA94762D086A18A9", "33D4A309C8E7BD10", null, null, null, null, null, "4D5140EB8F14A385",
            "0D592249833E2DC4", "AA616146576BD9BC", null, null, null, null, null, "EFA132FFFAC1ADA6",
            "009D570FFCF8F20B", "17F6F33AB313EE32", "AD5690C093F66FCF", "A7D63854DF76DDE6"
        };

        readonly string[] _oemid =
        {
            "10.0", "HFSJ", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "8.10", "8.10", "8.10",
            "8.10", "8.10", "10.0", "HFSJ", "10.0", "HFSJ", "10.0"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Apple HFS+ (APM)",
                                               _testfiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testfiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testfiles[i]);
                Assert.AreEqual(_sectorsize[i], image.Info.SectorSize, _testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AppleHFSPlus();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "Apple_HFS")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testfiles[i]);
                Assert.AreEqual(_clustersize[i], fs.XmlFsType.ClusterSize, _testfiles[i]);
                Assert.AreEqual("HFS+", fs.XmlFsType.Type, _testfiles[i]);
                Assert.AreEqual(_volumename[i], fs.XmlFsType.VolumeName, _testfiles[i]);
                Assert.AreEqual(_volumeserial[i], fs.XmlFsType.VolumeSerial, _testfiles[i]);
                Assert.AreEqual(_oemid[i], fs.XmlFsType.SystemIdentifier, _testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class HfsPlusGpt
    {
        readonly string[] _testfiles =
        {
            "macosx_10.11.aif", "macosx_10.11_journal.aif"
        };

        readonly ulong[] _sectors =
        {
            409600, 614400
        };

        readonly uint[] _sectorsize =
        {
            512, 512
        };

        readonly long[] _clusters =
        {
            51190, 76790
        };

        readonly int[] _clustersize =
        {
            4096, 4096
        };

        readonly string[] _volumename =
        {
            null, null
        };

        readonly string[] _volumeserial =
        {
            "D8C68470046E67BE", "FD3CB598F3C6294A"
        };

        readonly string[] _oemid =
        {
            "10.0", "HFSJ"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Apple HFS+ (GPT)",
                                               _testfiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testfiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testfiles[i]);
                Assert.AreEqual(_sectorsize[i], image.Info.SectorSize, _testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AppleHFSPlus();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "Apple HFS")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testfiles[i]);
                Assert.AreEqual(_clustersize[i], fs.XmlFsType.ClusterSize, _testfiles[i]);
                Assert.AreEqual("HFS+", fs.XmlFsType.Type, _testfiles[i]);
                Assert.AreEqual(_volumename[i], fs.XmlFsType.VolumeName, _testfiles[i]);
                Assert.AreEqual(_volumeserial[i], fs.XmlFsType.VolumeSerial, _testfiles[i]);
                Assert.AreEqual(_oemid[i], fs.XmlFsType.SystemIdentifier, _testfiles[i]);
            }
        }
    }

    // Mising Darwin 6.0.2 wrapped
    [TestFixture]
    public class HfsPlusMbr
    {
        readonly string[] _testfiles =
        {
            "macosx_10.11.aif", "macosx_10.11_journal.aif", "linux.aif", "linux_journal.aif", "darwin_1.3.1.aif",
            "darwin_1.3.1_wrapped.aif", "darwin_1.4.1.aif", "darwin_1.4.1_wrapped.aif", "darwin_6.0.2.aif",
            "darwin_8.0.1_journal.aif", "darwin_8.0.1.aif", "darwin_8.0.1_wrapped.aif", "linux_4.19_hfs+_flashdrive.aif"
        };

        readonly ulong[] _sectors =
        {
            303104, 352256, 262144, 262144, 819200, 614400, 819200, 614400, 819200, 1228800, 819200, 614400, 1024000
        };

        readonly uint[] _sectorsize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] _clusters =
        {
            37878, 44021, 32512, 32512, 102178, 76708, 102178, 76708, 102178, 153592, 102392, 76774, 127744
        };

        readonly int[] _clustersize =
        {
            4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096
        };

        readonly string[] _volumename =
        {
            null, null, null, null, null, null, null, null, null, null, null, null, null
        };

        readonly string[] _volumeserial =
        {
            "C84F550907D13F50", "016599F88029F73D", null, null, null, null, null, null, null, "F92964F9B3F64ABB",
            "A8FAC484A0A2B177", "D5D5BF1346AD2B8D", "B9BAC6856878A404"
        };

        readonly string[] _oemid =
        {
            "10.0", "HFSJ", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "H+Lx"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Apple HFS+ (MBR)",
                                               _testfiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testfiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testfiles[i]);
                Assert.AreEqual(_sectorsize[i], image.Info.SectorSize, _testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AppleHFSPlus();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0xAF")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testfiles[i]);
                Assert.AreEqual(_clustersize[i], fs.XmlFsType.ClusterSize, _testfiles[i]);
                Assert.AreEqual("HFS+", fs.XmlFsType.Type, _testfiles[i]);
                Assert.AreEqual(_volumename[i], fs.XmlFsType.VolumeName, _testfiles[i]);
                Assert.AreEqual(_volumeserial[i], fs.XmlFsType.VolumeSerial, _testfiles[i]);
                Assert.AreEqual(_oemid[i], fs.XmlFsType.SystemIdentifier, _testfiles[i]);
            }
        }
    }
}