// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FAT16.cs
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
    public class Fat16
    {
        readonly string[] _testFiles =
        {
            // MS-DOS 3.30A
            "msdos_3.30A_mf2ed.img.lz",

            // MS-DOS 3.31
            "msdos_3.31_mf2ed.img.lz"
        };

        readonly MediaType[] _mediaTypes =
        {
            // MS-DOS 3.30A
            MediaType.DOS_35_ED,

            // MS-DOS 3.31
            MediaType.DOS_35_ED
        };

        readonly ulong[] _sectors =
        {
            // MS-DOS 3.30A
            5760,

            // MS-DOS 3.31
            5760
        };

        readonly uint[] _sectorSize =
        {
            // MS-DOS 3.30A
            512,

            // MS-DOS 3.31
            512
        };

        readonly long[] _clusters =
        {
            // MS-DOS 3.30A
            5760,

            // MS-DOS 3.31
            5760
        };

        readonly int[] _clusterSize =
        {
            // MS-DOS 3.30A
            512,

            // MS-DOS 3.31
            512
        };

        readonly string[] _volumeName =
        {
            // MS-DOS 3.30A
            null,

            // MS-DOS 3.31
            null
        };

        readonly string[] _volumeSerial =
        {
            // MS-DOS 3.30A
            null,

            // MS-DOS 3.31
            null
        };

        readonly string[] _oemId =
        {
            // MS-DOS 3.30A
            "MSDOS3.3",

            // MS-DOS 3.31
            "IBM  3.3"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "FAT16", _testFiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                IFilesystem fs = new FAT();

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
                Assert.AreEqual("FAT16", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
                Assert.AreEqual(_oemId[i], fs.XmlFsType.SystemIdentifier, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class Fat16Apm
    {
        readonly string[] _testFiles =
        {
            "macosx_10.11.aif"
        };

        readonly ulong[] _sectors =
        {
            1024000
        };

        readonly uint[] _sectorSize =
        {
            512
        };

        readonly long[] _clusters =
        {
            63995
        };

        readonly int[] _clusterSize =
        {
            8192
        };

        readonly string[] _volumeName =
        {
            "VOLUMELABEL"
        };

        readonly string[] _volumeSerial =
        {
            "063D1F09"
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
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "FAT16 (APM)", _testFiles[i]);
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
                    if(partitions[j].Type == "DOS_FAT_16")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual("FAT16", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
                Assert.AreEqual(_oemId[i], fs.XmlFsType.SystemIdentifier, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class Fat16Atari
    {
        readonly string[] _testFiles =
        {
            "tos_1.04.aif", "tos_1.04_small.aif"
        };

        readonly ulong[] _sectors =
        {
            81920, 16384
        };

        readonly uint[] _sectorSize =
        {
            512, 512
        };

        readonly long[] _clusters =
        {
            10239, 8191
        };

        readonly int[] _clusterSize =
        {
            4096, 1024
        };

        readonly string[] _volumeName =
        {
            null, null
        };

        readonly string[] _volumeSerial =
        {
            "BA9831", "2019E1"
        };

        readonly string[] _oemId =
        {
            null, null
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "FAT16 (Atari)", _testFiles[i]);
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
                    if(partitions[j].Type == "GEM" ||
                       partitions[j].Type == "BGM")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual("FAT16", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
                Assert.AreEqual(_oemId[i], fs.XmlFsType.SystemIdentifier, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class Fat16Gpt
    {
        readonly string[] _testFiles =
        {
            "macosx_10.11.aif"
        };

        readonly ulong[] _sectors =
        {
            1024000
        };

        readonly uint[] _sectorSize =
        {
            512
        };

        readonly long[] _clusters =
        {
            63995
        };

        readonly int[] _clusterSize =
        {
            8192
        };

        readonly string[] _volumeName =
        {
            "VOLUMELABEL"
        };

        readonly string[] _volumeSerial =
        {
            "2E8A1F1B"
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
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "FAT16 (GPT)", _testFiles[i]);
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
                Assert.AreEqual("FAT16", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
                Assert.AreEqual(_oemId[i], fs.XmlFsType.SystemIdentifier, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class Fat16Mbr
    {
        readonly string[] _testFiles =
        {
            "drdos_3.40.aif", "drdos_3.41.aif", "drdos_5.00.aif", "drdos_6.00.aif", "drdos_7.02.aif", "drdos_7.03.aif",
            "drdos_8.00.aif", "msdos331.aif", "msdos401.aif", "msdos500.aif", "msdos600.aif", "msdos620rc1.aif",
            "msdos620.aif", "msdos621.aif", "msdos622.aif", "msdos710.aif", "novelldos_7.00.aif", "opendos_7.01.aif",
            "pcdos2000.aif", "pcdos400.aif", "pcdos500.aif", "pcdos502.aif", "pcdos610.aif", "pcdos630.aif",
            "msos2_1.21.aif", "msos2_1.30.1.aif", "multiuserdos_7.22r4.aif", "os2_1.20.aif", "os2_1.30.aif",
            "os2_6.307.aif", "os2_6.514.aif", "os2_6.617.aif", "os2_8.162.aif", "os2_9.023.aif", "ecs.aif",
            "macosx_10.11.aif", "win10.aif", "win2000.aif", "win95osr2.1.aif", "win95osr2.5.aif", "win95osr2.aif",
            "win95.aif", "win98se.aif", "win98.aif", "winme.aif", "winnt_3.10.aif", "winnt_3.50.aif", "winnt_3.51.aif",
            "winnt_4.00.aif", "winvista.aif", "beos_r4.5.aif", "linux.aif", "amigaos_3.9.aif", "aros.aif",
            "freebsd_6.1.aif", "freebsd_7.0.aif", "freebsd_8.2.aif", "macos_7.5.3.aif", "macos_7.5.aif",
            "macos_7.6.aif", "macos_8.0.aif", "ecs20_fstester.aif", "linux_2.2_umsdos16_flashdrive.aif",
            "linux_4.19_fat16_msdos_flashdrive.aif", "linux_4.19_vfat16_flashdrive.aif"
        };

        readonly ulong[] _sectors =
        {
            1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000,
            1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000,
            1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000,
            1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000,
            1024000, 1024000, 1024000, 262144, 1024128, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000,
            1024000, 1024000, 1024000, 1024000, 1024000
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512,
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512,
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512,
            512, 512
        };

        readonly long[] _clusters =
        {
            63882, 63941, 63941, 63941, 63941, 63941, 63941, 63941, 63941, 63941, 63941, 63941, 63941, 63941, 63941,
            63941, 63941, 63941, 63941, 63941, 63941, 63941, 63941, 63941, 63941, 63941, 63941, 63941, 63941, 63941,
            63941, 63941, 63941, 63941, 63882, 63992, 63864, 63252, 63941, 63941, 63941, 63941, 63998, 63998, 63998,
            63941, 63998, 63998, 63941, 63616, 63996, 65024, 63941, 63882, 63998, 63998, 31999, 63941, 63941, 63941,
            63941, 63882, 63941, 63872, 63872
        };

        readonly int[] _clusterSize =
        {
            8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192,
            8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192,
            8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 2048, 8192, 8192,
            8192, 8192, 16384, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192
        };

        readonly string[] _volumeName =
        {
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            null, "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "NO NAME", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VolumeLabel", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUME LABE", "DICSETTER",
            "DICSETTER", "DICSETTER"
        };

        readonly string[] _volumeSerial =
        {
            null, null, null, null, null, null, "1BFB0748", null, "217B1909", "0C6D18FC", "382B18F4", "3E2018E9",
            "0D2418EF", "195A181B", "27761816", "356B1809", null, null, "2272100F", "07280FE1", "1F630FF9", "18340FFE",
            "3F3F1003", "273D1009", "9C162C15", "9C1E2C15", null, "5BE66015", "5BE43015", "5BEAC015", "E6B18414",
            "E6C63414", "1C069414", "1C059414", "1BE5B814", "3EF71EF4", "DAF97911", "305637BD", "275B0DE4", "09650DFC",
            "38270D18", "2E620D0C", "0B4F0EED", "0E122464", "3B5F0F02", "C84CB6F2", "D0E9AD4E", "C039A2EC", "501F9FA6",
            "9AAA4216", "00000000", "A132D985", "374D3BD1", "52BEA34A", "3CF10E0D", "C6C30E0D", "44770E0D", "27761816",
            "27761816", "27761816", "27761816", "66AAF014", "5CC78D47", "A552A493", "FCC308A7"
        };

        readonly string[] _oemId =
        {
            "IBM  3.2", "IBM  3.2", "IBM  3.3", "IBM  3.3", "IBM  3.3", "DRDOS  7", "IBM  5.0", "IBM  3.3", "MSDOS4.0",
            "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSWIN4.1", "IBM  3.3", "IBM  3.3",
            "IBM  7.0", "IBM  4.0", "IBM  5.0", "IBM  5.0", "IBM  6.0", "IBM  6.0", "IBM 10.2", "IBM 10.2", "IBM  3.2",
            "IBM 10.2", "IBM 10.2", "IBM 20.0", "IBM 20.0", "IBM 20.0", "IBM 20.0", "IBM 20.0", "IBM 4.50", "BSD  4.4",
            "MSDOS5.0", "MSDOS5.0", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.0", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1",
            "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "MSDOS5.0", "BeOS    ", "mkfs.fat", "CDP  5.0", "MSWIN4.1",
            "BSD  4.4", "BSD  4.4", "BSD4.4  ", "PCX 2.0 ", "PCX 2.0 ", "PCX 2.0 ", "PCX 2.0 ", "IBM 4.50", null,
            "mkfs.fat", "mkfs.fat"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "FAT16 (MBR)", _testFiles[i]);
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
                Assert.AreEqual("FAT16", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
                Assert.AreEqual(_oemId[i], fs.XmlFsType.SystemIdentifier, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class Fat16Rdb
    {
        readonly string[] _testFiles =
        {
            "amigaos_3.9.aif"
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
            63689
        };

        readonly int[] _clusterSize =
        {
            8192
        };

        readonly string[] _volumeName =
        {
            "VOLUMELABEL"
        };

        readonly string[] _volumeSerial =
        {
            "374D40D1"
        };

        readonly string[] _oemId =
        {
            "CDP  5.0"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "FAT16 (RDB)", _testFiles[i]);
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
                    if(partitions[j].Type == "0x06")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual("FAT16", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
                Assert.AreEqual(_oemId[i], fs.XmlFsType.SystemIdentifier, _testFiles[i]);
            }
        }
    }

    [TestFixture]
    public class Fat16Human
    {
        readonly string[] _testFiles =
        {
            "sasidisk.aif", "scsidisk.aif"
        };

        readonly ulong[] _sectors =
        {
            162096, 204800
        };

        readonly uint[] _sectorSize =
        {
            256, 512
        };

        readonly long[] _clusters =
        {
            40510, 102367
        };

        readonly int[] _clusterSize =
        {
            1024, 1024
        };

        readonly string[] _volumeName =
        {
            null, null
        };

        readonly string[] _volumeSerial =
        {
            null, null
        };

        readonly string[] _oemId =
        {
            "Hudson soft 2.00", " Hero Soft V1.10"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "FAT16 (Human68K)",
                                               _testFiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new FAT();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "Human68k")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual("FAT16", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
                Assert.AreEqual(_oemId[i], fs.XmlFsType.SystemIdentifier, _testFiles[i]);
            }
        }
    }
}