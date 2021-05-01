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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.FAT16
{
    [TestFixture]
    public class MBR : FilesystemTest
    {
        public MBR() : base("FAT16") {}

        public override string      DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "FAT16 (MBR)");
        public override IFilesystem Plugin     => new FAT();
        public override bool        Partitions => true;

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile    = "drdos_3.40.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 63882,
                ClusterSize = 8192,
                SystemId    = "IBM  3.2",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_3.41.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 63941,
                ClusterSize = 8192,
                SystemId    = "IBM  3.2",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_5.00.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 63941,
                ClusterSize = 8192,
                SystemId    = "IBM  3.3",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_6.00.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 63941,
                ClusterSize = 8192,
                SystemId    = "IBM  3.3",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_7.02.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 63941,
                ClusterSize = 8192,
                SystemId    = "IBM  3.3",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_7.03.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 63941,
                ClusterSize = 8192,
                SystemId    = "DRDOS  7",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_8.00.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "IBM  5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BFB0748"
            },
            new FileSystemTest
            {
                TestFile    = "msdos331.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 63941,
                ClusterSize = 8192,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile     = "msdos401.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "MSDOS4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "217B1909"
            },
            new FileSystemTest
            {
                TestFile     = "msdos500.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0C6D18FC"
            },
            new FileSystemTest
            {
                TestFile     = "msdos600.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "382B18F4"
            },
            new FileSystemTest
            {
                TestFile     = "msdos620rc1.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3E2018E9"
            },
            new FileSystemTest
            {
                TestFile     = "msdos620.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0D2418EF"
            },
            new FileSystemTest
            {
                TestFile     = "msdos621.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "195A181B"
            },
            new FileSystemTest
            {
                TestFile     = "msdos622.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "27761816"
            },
            new FileSystemTest
            {
                TestFile     = "msdos710.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "356B1809"
            },
            new FileSystemTest
            {
                TestFile    = "novelldos_7.00.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 63941,
                ClusterSize = 8192,
                SystemId    = "IBM  3.3",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "opendos_7.01.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 63941,
                ClusterSize = 8192,
                SystemId    = "IBM  3.3",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos2000.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "IBM  7.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2272100F"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos400.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "IBM  4.0",
                VolumeName   = "NO NAME",
                VolumeSerial = "07280FE1"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos500.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "IBM  5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1F630FF9"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos502.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "IBM  5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "18340FFE"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos610.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "IBM  6.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3F3F1003"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos630.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "IBM  6.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "273D1009"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.21.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "9C162C15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.30.1.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "9C1E2C15"
            },
            new FileSystemTest
            {
                TestFile    = "multiuserdos_7.22r4.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 1024000,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 63941,
                ClusterSize = 8192,
                SystemId    = "IBM  3.2",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile     = "os2_1.20.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5BE66015"
            },
            new FileSystemTest
            {
                TestFile     = "os2_1.30.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5BE43015"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.307.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5BEAC015"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.514.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6B18414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.617.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6C63414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_8.162.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1C069414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_9.023.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1C059414"
            },
            new FileSystemTest
            {
                TestFile     = "ecs.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63882,
                ClusterSize  = 8192,
                SystemId     = "IBM 4.50",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BE5B814"
            },
            new FileSystemTest
            {
                TestFile     = "macosx_10.11.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63992,
                ClusterSize  = 8192,
                SystemId     = "BSD  4.4",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3EF71EF4"
            },
            new FileSystemTest
            {
                TestFile     = "win10.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63864,
                ClusterSize  = 8192,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "DAF97911"
            },
            new FileSystemTest
            {
                TestFile     = "win2000.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63252,
                ClusterSize  = 8192,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "305637BD"
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2.1.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "275B0DE4"
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2.5.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "09650DFC"
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "38270D18"
            },
            new FileSystemTest
            {
                TestFile     = "win95.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "MSWIN4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2E620D0C"
            },
            new FileSystemTest
            {
                TestFile     = "win98se.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63998,
                ClusterSize  = 8192,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0B4F0EED"
            },
            new FileSystemTest
            {
                TestFile     = "win98.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63998,
                ClusterSize  = 8192,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0E122464"
            },
            new FileSystemTest
            {
                TestFile     = "winme.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63998,
                ClusterSize  = 8192,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3B5F0F02"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_3.10.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "C84CB6F2"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_3.50.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63998,
                ClusterSize  = 8192,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "D0E9AD4E"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_3.51.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63998,
                ClusterSize  = 8192,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "C039A2EC"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_4.00.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "501F9FA6"
            },
            new FileSystemTest
            {
                TestFile     = "winvista.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63616,
                ClusterSize  = 8192,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "9AAA4216"
            },
            new FileSystemTest
            {
                TestFile     = "beos_r4.5.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 262144,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 65268,
                ClusterSize  = 2048,
                SystemId     = "BeOS    ",
                VolumeName   = "VOLUME LABE",
                VolumeSerial = "00000000"
            },
            new FileSystemTest
            {
                TestFile     = "linux.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 262144,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 65024,
                ClusterSize  = 2048,
                SystemId     = "mkfs.fat",
                VolumeName   = "VolumeLabel",
                VolumeSerial = "A132D985"
            },
            new FileSystemTest
            {
                TestFile     = "amigaos_3.9.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024128,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "CDP  5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "374D3BD1"
            },
            new FileSystemTest
            {
                TestFile     = "aros.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Clusters     = 63882,
                ClusterSize  = 8192,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "52BEA34A"
            },
            new FileSystemTest
            {
                TestFile     = "freebsd_6.1.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63998,
                ClusterSize  = 8192,
                SystemId     = "BSD  4.4",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3CF10E0D"
            },
            new FileSystemTest
            {
                TestFile     = "freebsd_7.0.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63998,
                ClusterSize  = 8192,
                SystemId     = "BSD  4.4",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "C6C30E0D"
            },
            new FileSystemTest
            {
                TestFile     = "freebsd_8.2.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 31999,
                ClusterSize  = 16384,
                SystemId     = "BSD4.4  ",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "44770E0D"
            },
            new FileSystemTest
            {
                TestFile     = "macos_7.5.3.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "PCX 2.0 ",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "27761816"
            },
            new FileSystemTest
            {
                TestFile     = "macos_7.5.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "PCX 2.0 ",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "27761816"
            },
            new FileSystemTest
            {
                TestFile     = "macos_7.6.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "PCX 2.0 ",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "27761816"
            },
            new FileSystemTest
            {
                TestFile     = "macos_8.0.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                SystemId     = "PCX 2.0 ",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "27761816"
            },
            new FileSystemTest
            {
                TestFile     = "ecs20_fstester.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63882,
                ClusterSize  = 8192,
                SystemId     = "IBM 4.50",
                VolumeName   = "VOLUME LABE",
                VolumeSerial = "66AAF014"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.2_umsdos16_flashdrive.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63941,
                ClusterSize  = 8192,
                VolumeName   = "DICSETTER",
                VolumeSerial = "5CC78D47"
            },
            new FileSystemTest
            {
                TestFile     = "linux_4.19_fat16_msdos_flashdrive.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63872,
                ClusterSize  = 8192,
                SystemId     = "mkfs.fat",
                VolumeName   = "DICSETTER",
                VolumeSerial = "A552A493"
            },
            new FileSystemTest
            {
                TestFile     = "linux_4.19_vfat16_flashdrive.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 1024000,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 63872,
                ClusterSize  = 8192,
                SystemId     = "mkfs.fat",
                VolumeName   = "DICSETTER",
                VolumeSerial = "FCC308A7"
            },
            new FileSystemTest
            {
                TestFile     = "beos_r5.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 262144,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 65268,
                ClusterSize  = 2048,
                SystemId     = "BeOS    ",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "00000000"
            },
            new FileSystemTest
            {
                TestFile     = "dflybsd_1.0.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 262144,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 65268,
                ClusterSize  = 2048,
                SystemId     = "BSD  4.4",
                VolumeName   = "NO NAME",
                VolumeSerial = "1B3F1B00"
            }
        };
    }
}