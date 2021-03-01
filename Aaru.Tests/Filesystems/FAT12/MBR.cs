// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FAT12.cs
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

namespace Aaru.Tests.Filesystems.FAT12
{
    [TestFixture]
    public class MBR : FilesystemTest
    {
        public MBR() : base("FAT12") {}

        public override string      _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "FAT12 (MBR)");
        public override IFilesystem _plugin     => new FAT();
        public override bool        _partitions => true;

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile    = "compaqmsdos331.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 8192,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 1000,
                ClusterSize = 4096,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_3.40.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 30720,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 3654,
                ClusterSize = 4096,
                SystemId    = "IBM  3.2",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_3.41.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 28672,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 3520,
                ClusterSize = 4096,
                SystemId    = "IBM  3.2",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_5.00.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 28672,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 3520,
                ClusterSize = 4096,
                SystemId    = "IBM  3.3",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_6.00.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 28672,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 3520,
                ClusterSize = 4096,
                SystemId    = "IBM  3.3",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_7.02.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 28672,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 3520,
                ClusterSize = 4096,
                SystemId    = "IBM  3.3",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "drdos_7.03.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 28672,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 3520,
                ClusterSize = 4096,
                SystemId    = "DRDOS  7",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile     = "drdos_8.00.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 28672,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 3520,
                ClusterSize  = 4096,
                SystemId     = "IBM  5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1BFB1273"
            },
            new FileSystemTest
            {
                TestFile    = "msdos331.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 8192,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 1000,
                ClusterSize = 4096,
                SystemId    = "IBM  3.3",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile     = "msdos401.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 8192,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 1000,
                ClusterSize  = 4096,
                SystemId     = "MSDOS4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "407D1907"
            },
            new FileSystemTest
            {
                TestFile     = "msdos500.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 8192,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 1000,
                ClusterSize  = 4096,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "345D18FB"
            },
            new FileSystemTest
            {
                TestFile     = "msdos600.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 8192,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 1000,
                ClusterSize  = 4096,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "332518F4"
            },
            new FileSystemTest
            {
                TestFile     = "msdos620rc1.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 8192,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 1000,
                ClusterSize  = 4096,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "395718E9"
            },
            new FileSystemTest
            {
                TestFile     = "msdos620.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 8192,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 1000,
                ClusterSize  = 4096,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "076718EF"
            },
            new FileSystemTest
            {
                TestFile     = "msdos621.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 8192,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 1000,
                ClusterSize  = 4096,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1371181B"
            },
            new FileSystemTest
            {
                TestFile     = "msdos622.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 8192,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 1000,
                ClusterSize  = 4096,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "23281816"
            },
            new FileSystemTest
            {
                TestFile     = "msdos710.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2008,
                ClusterSize  = 4096,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2F781809"
            },
            new FileSystemTest
            {
                TestFile    = "novelldos_7.00.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 28672,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 3520,
                ClusterSize = 4096,
                SystemId    = "IBM  3.3",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile    = "opendos_7.01.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 28672,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 3520,
                ClusterSize = 4096,
                SystemId    = "IBM  3.3",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos2000.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 32768,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 4024,
                ClusterSize  = 4096,
                SystemId     = "IBM  7.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "294F100F"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos200.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 32768,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 4031,
                ClusterSize = 4096,
                SystemId    = "IBM  2.0"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos210.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 32768,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 4031,
                ClusterSize = 4096,
                SystemId    = "IBM  2.0"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos300.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 32768,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 4024,
                ClusterSize = 4096,
                SystemId    = "IBM  3.0"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos310.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 32768,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 4024,
                ClusterSize = 4096,
                SystemId    = "IBM  3.1"
            },
            new FileSystemTest
            {
                TestFile    = "pcdos330.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 32768,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 4024,
                ClusterSize = 4096,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos400.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 32768,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 4024,
                ClusterSize  = 4096,
                SystemId     = "IBM  4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0F340FE4"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos500.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 32768,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 4024,
                ClusterSize  = 4096,
                SystemId     = "IBM  5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1A5E0FF9"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos502.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 32768,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 4024,
                ClusterSize  = 4096,
                SystemId     = "IBM  5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "1D2F0FFE"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos610.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 32768,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 4024,
                ClusterSize  = 4096,
                SystemId     = "IBM  6.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "076C1004"
            },
            new FileSystemTest
            {
                TestFile     = "pcdos630.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 32768,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 4024,
                ClusterSize  = 4096,
                SystemId     = "IBM  6.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2C481009"
            },
            new FileSystemTest
            {
                TestFile    = "toshibamsdos330.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 8192,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 1000,
                ClusterSize = 4096,
                SystemId    = "T V3.30 ",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile     = "toshibamsdos401.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 8192,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 1000,
                ClusterSize  = 4096,
                SystemId     = "T V4.00 ",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "3C2319E8"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.21.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2008,
                ClusterSize  = 4096,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "66CC3C15"
            },
            new FileSystemTest
            {
                TestFile     = "msos2_1.30.1.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2008,
                ClusterSize  = 4096,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "66A54C15"
            },
            new FileSystemTest
            {
                TestFile    = "multiuserdos_7.22r4.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 16384,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 2008,
                ClusterSize = 4096,
                SystemId    = "IBM  3.2",
                VolumeName  = "VOLUMELABEL"
            },
            new FileSystemTest
            {
                TestFile     = "os2_1.20.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2008,
                ClusterSize  = 4096,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5C578015"
            },
            new FileSystemTest
            {
                TestFile     = "os2_1.30.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2008,
                ClusterSize  = 4096,
                SystemId     = "IBM 10.2",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5B845015"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.307.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2008,
                ClusterSize  = 4096,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "5C4BF015"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.514.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2008,
                ClusterSize  = 4096,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6B5F414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_6.617.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2008,
                ClusterSize  = 4096,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6B15414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_8.162.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2008,
                ClusterSize  = 4096,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6A41414"
            },
            new FileSystemTest
            {
                TestFile     = "os2_9.023.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2008,
                ClusterSize  = 4096,
                SystemId     = "IBM 20.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6A39414"
            },
            new FileSystemTest
            {
                TestFile     = "ecs.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 1890,
                ClusterSize  = 4096,
                SystemId     = "IBM 4.50",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "E6B0B814"
            },
            new FileSystemTest
            {
                TestFile     = "macosx_10.11.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 4079,
                ClusterSize  = 2048,
                SystemId     = "BSD  4.4",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "26A21EF4"
            },
            new FileSystemTest
            {
                TestFile     = "win10.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 3552,
                ClusterSize  = 2048,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "74F4921D"
            },
            new FileSystemTest
            {
                TestFile     = "win2000.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 4088,
                ClusterSize  = 2048,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "C4B64D11"
            },
            new FileSystemTest
            {
                TestFile     = "win95.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2008,
                ClusterSize  = 4096,
                SystemId     = "MSWIN4.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "29200D0C"
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2.1.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2008,
                ClusterSize  = 4096,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "234F0DE4"
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2.5.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2008,
                ClusterSize  = 4096,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "074C0DFC"
            },
            new FileSystemTest
            {
                TestFile     = "win95osr2.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2008,
                ClusterSize  = 4096,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "33640D18"
            },
            new FileSystemTest
            {
                TestFile     = "win98.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2044,
                ClusterSize  = 4096,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "0E121460"
            },
            new FileSystemTest
            {
                TestFile     = "win98se.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2044,
                ClusterSize  = 4096,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "094C0EED"
            },
            new FileSystemTest
            {
                TestFile     = "winme.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2044,
                ClusterSize  = 4096,
                SystemId     = "MSWIN4.1",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "38310F02"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_3.10.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 4016,
                ClusterSize  = 2048,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "50489A1B"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_3.50.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2044,
                ClusterSize  = 4096,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "2CE52101"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_3.51.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2044,
                ClusterSize  = 4096,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "94313E7E"
            },
            new FileSystemTest
            {
                TestFile     = "winnt_4.00.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 4016,
                ClusterSize  = 2048,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "BC184FE6"
            },
            new FileSystemTest
            {
                TestFile     = "winvista.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 3072,
                ClusterSize  = 2048,
                SystemId     = "MSDOS5.0",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "BAD08A1E"
            },
            new FileSystemTest
            {
                TestFile     = "beos_r4.5.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2040,
                ClusterSize  = 4096,
                SystemId     = "BeOS    ",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "00000000"
            },
            new FileSystemTest
            {
                TestFile     = "linux.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 3584,
                ClusterSize  = 2048,
                SystemId     = "mkfs.fat",
                VolumeName   = "VolumeLabel",
                VolumeSerial = "8D418102"
            },
            new FileSystemTest
            {
                TestFile     = "freebsd_6.1.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2044,
                ClusterSize  = 4096,
                SystemId     = "BSD  4.4",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "8FC80E0A"
            },
            new FileSystemTest
            {
                TestFile     = "freebsd_7.0.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2044,
                ClusterSize  = 4096,
                SystemId     = "BSD  4.4",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "34FA0E0B"
            },
            new FileSystemTest
            {
                TestFile     = "freebsd_8.2.aif",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 16384,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2044,
                ClusterSize  = 4096,
                SystemId     = "BSD4.4  ",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "02140E0B"
            }
        };
    }
}