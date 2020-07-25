// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : FAT32.cs
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
    public class Fat32Apm
    {
        readonly string[] _testFiles =
        {
            "macosx_10.11.aif"
        };

        readonly ulong[] _sectors =
        {
            4194304
        };

        readonly uint[] _sectorSize =
        {
            512
        };

        readonly long[] _clusters =
        {
            524278
        };

        readonly int[] _clusterSize =
        {
            4096
        };

        readonly string[] _volumeName =
        {
            "VOLUMELABEL"
        };

        readonly string[] _volumeSerial =
        {
            "35BD1F0A"
        };

        readonly string[] _oemId =
        {
            "BSD  4.4"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "FAT32 (APM)", _testFiles[i]);
                IFilter filter   = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new FAT();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "DOS_FAT_32")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual("FAT32", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
                Assert.AreEqual(_oemId[i], fs.XmlFsType.SystemIdentifier, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class Fat32Gpt
    {
        readonly string[] _testFiles =
        {
            "macosx_10.11.aif"
        };

        readonly ulong[] _sectors =
        {
            4194304
        };

        readonly uint[] _sectorSize =
        {
            512
        };

        readonly long[] _clusters =
        {
            523775
        };

        readonly int[] _clusterSize =
        {
            4096
        };

        readonly string[] _volumeName =
        {
            "VOLUMELABEL"
        };

        readonly string[] _volumeSerial =
        {
            "7ABE1F1B"
        };

        readonly string[] _oemId =
        {
            "BSD  4.4"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "FAT32 (GPT)", _testFiles[i]);
                IFilter filter   = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new FAT();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "Microsoft Basic data")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual("FAT32", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
                Assert.AreEqual(_oemId[i], fs.XmlFsType.SystemIdentifier, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class Fat32Mbr
    {
        readonly string[] _testFiles =
        {
            "drdos_7.03.aif", "drdos_8.00.aif", "msdos_7.10.aif", "macosx_10.11.aif", "win10.aif", "win2000.aif",
            "win95osr2.1.aif", "win95osr2.5.aif", "win95osr2.aif", "win98se.aif", "win98.aif", "winme.aif",
            "winvista.aif", "beos_r4.5.aif", "linux.aif", "aros.aif", "freebsd_6.1.aif", "freebsd_7.0.aif",
            "freebsd_8.2.aif", "freedos_1.2.aif", "ecs20_fstester.aif", "linux_2.2_umsdos32_flashdrive.aif",
            "linux_4.19_fat32_msdos_flashdrive.aif", "linux_4.19_vfat32_flashdrive.aif"
        };

        readonly ulong[] _sectors =
        {
            8388608, 8388608, 8388608, 4194304, 4194304, 8388608, 4194304, 4194304, 4194304, 4194304, 4194304, 4194304,
            4194304, 4194304, 262144, 4194304, 4194304, 4194304, 4194304, 8388608, 1024000, 1024000, 1024000, 1024000
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512,
            512, 512, 512
        };

        readonly long[] _clusters =
        {
            1048233, 1048233, 1048233, 524287, 524016, 1048233, 524152, 524152, 524152, 524112, 524112, 524112, 523520,
            1048560, 260096, 524160, 524112, 524112, 65514, 1048233, 127744, 127882, 127744, 127744
        };

        readonly int[] _clusterSize =
        {
            4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 2048, 512, 4096, 4096, 4096,
            32768, 4096, 4096, 4096, 4096, 4096
        };

        readonly string[] _volumeName =
        {
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VolumeLabel", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "Volume labe",
            "DICSETTER", "DICSETTER", "DICSETTER"
        };

        readonly string[] _volumeSerial =
        {
            "5955996C", "1BFB1A43", "3B331809", "42D51EF1", "48073346", "EC62E6DE", "2A310DE4", "0C140DFC", "3E310D18",
            "0D3D0EED", "0E131162", "3F500F02", "82EB4C04", "00000000", "B488C502", "5CAC9B4E", "41540E0E", "4E600E0F",
            "26E20E0F", "3E0C1BE8", "63084BBA", "5CC7908D", "D1290612", "79BCA86E"
        };

        readonly string[] _oemId =
        {
            "DRDOS7.X", "IBM  7.1", "MSWIN4.1", "BSD  4.4", "MSDOS5.0", "MSDOS5.0", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1",
            "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSDOS5.0", "BeOS    ", "mkfs.fat", "MSWIN4.1", "BSD  4.4", "BSD  4.4",
            "BSD4.4  ", "FRDOS4.1", "mkfs.fat", "mkdosfs", "mkfs.fat", "mkfs.fat"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "FAT32 (MBR)", _testFiles[i]);
                IFilter filter   = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new FAT();
                Assert.AreEqual(true, fs.Identify(image, partitions[0]), _testFiles[i]);
                fs.GetInformation(image, partitions[0], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual("FAT32", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
                Assert.AreEqual(_oemId[i], fs.XmlFsType.SystemIdentifier, _testFiles[i]);
            }
        }
    }
}