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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.FAT12;

[TestFixture]
public class MBR : ReadOnlyFilesystemTest
{
    public MBR() : base("FAT12") {}

    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "FAT12 (MBR)");
    public override IFilesystem Plugin     => new FAT();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "darwin_6.0.2.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3009,
            ClusterSize  = 4096,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "AA180CF0"
        },
        new FileSystemTest
        {
            TestFile     = "darwin_7.0.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3057,
            ClusterSize  = 4096,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "EC241B05"
        },
        new FileSystemTest
        {
            TestFile     = "darwin_8.0.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3057,
            ClusterSize  = 4096,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "64F5191A"
        },
        new FileSystemTest
        {
            TestFile    = "compaqmsdos331.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 8192,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 995,
            ClusterSize = 4096,
            SystemId    = "IBM  3.3"
        },
        new FileSystemTest
        {
            TestFile    = "drdos_3.40.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 24576,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 2765,
            ClusterSize = 4096,
            SystemId    = "IBM  3.2",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile    = "drdos_3.41.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 24576,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 2883,
            ClusterSize = 4096,
            SystemId    = "IBM  3.2",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile    = "drdos_5.00.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 24576,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 2883,
            ClusterSize = 4096,
            SystemId    = "IBM  3.3",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile    = "drdos_6.00.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 24576,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 2883,
            ClusterSize = 4096,
            SystemId    = "IBM  3.3",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile    = "drdos_7.02.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 24576,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 2883,
            ClusterSize = 4096,
            SystemId    = "IBM  3.3",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile    = "drdos_7.03.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 24576,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 2883,
            ClusterSize = 4096,
            SystemId    = "DRDOS  7",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile     = "drdos_8.00.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2883,
            ClusterSize  = 4096,
            SystemId     = "IBM  5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "09F92756"
        },
        new FileSystemTest
        {
            TestFile     = "drdos_8.10.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2883,
            ClusterSize  = 4096,
            SystemId     = "IBM  5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "08FF1164"
        },
        new FileSystemTest
        {
            TestFile    = "msdos_3.31.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 24576,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 2883,
            ClusterSize = 4096,
            SystemId    = "IBM  3.3",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_4.00.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2883,
            ClusterSize  = 4096,
            SystemId     = "MSDOS4.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "3C2B0903"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_4.01.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 8192,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 995,
            ClusterSize  = 4096,
            SystemId     = "MSDOS4.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "407D1907"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_5.00.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2883,
            ClusterSize  = 4096,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "1B08090B"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_5.00a.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2883,
            ClusterSize  = 4096,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "3348090C"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_6.00.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2883,
            ClusterSize  = 4096,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "124008FB"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_6.20rc1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 8192,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 995,
            ClusterSize  = 4096,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "395718E9"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_6.20.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2883,
            ClusterSize  = 4096,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "31190907"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_6.21.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2883,
            ClusterSize  = 4096,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "27230917"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_6.22.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2883,
            ClusterSize  = 4096,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "156308F3"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_7.10.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 16384,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2002,
            ClusterSize  = 4096,
            SystemId     = "MSWIN4.1",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "2F781809"
        },
        new FileSystemTest
        {
            TestFile    = "novelldos_7.00.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 24576,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 2883,
            ClusterSize = 4096,
            SystemId    = "IBM  3.3",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile    = "opendos_7.01.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 24576,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 2883,
            ClusterSize = 4096,
            SystemId    = "IBM  3.3",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile     = "pcdos_2000.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2883,
            ClusterSize  = 4096,
            SystemId     = "IBM  7.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "390F090A"
        },
        new FileSystemTest
        {
            TestFile    = "pcdos_2.00.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 32768,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 4024,
            ClusterSize = 4096,
            SystemId    = "IBM  2.0"
        },
        new FileSystemTest
        {
            TestFile    = "pcdos_2.10.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 32768,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 4024,
            ClusterSize = 4096,
            SystemId    = "IBM  2.0"
        },
        new FileSystemTest
        {
            TestFile    = "pcdos_3.00.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 32768,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 4017,
            ClusterSize = 4096,
            SystemId    = "IBM  3.0"
        },
        new FileSystemTest
        {
            TestFile    = "pcdos_3.10.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 32768,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 4017,
            ClusterSize = 4096,
            SystemId    = "IBM  3.1"
        },
        new FileSystemTest
        {
            TestFile    = "pcdos_3.20.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 24576,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 2883,
            ClusterSize = 4096,
            SystemId    = "IBM  3.2",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile    = "pcdos_3.30.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 32768,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 4017,
            ClusterSize = 4096,
            SystemId    = "IBM  3.3"
        },
        new FileSystemTest
        {
            TestFile     = "pcdos_4.00.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 32768,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 4017,
            ClusterSize  = 4096,
            SystemId     = "IBM  4.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "0F340FE4"
        },
        new FileSystemTest
        {
            TestFile     = "pcdos_5.00.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2883,
            ClusterSize  = 4096,
            SystemId     = "IBM  5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "363B0904"
        },
        new FileSystemTest
        {
            TestFile     = "pcdos_5.02.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2883,
            ClusterSize  = 4096,
            SystemId     = "IBM  5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "10650902"
        },
        new FileSystemTest
        {
            TestFile     = "pcdos_6.10.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2883,
            ClusterSize  = 4096,
            SystemId     = "IBM  6.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "313208FE"
        },
        new FileSystemTest
        {
            TestFile     = "pcdos_6.30.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2883,
            ClusterSize  = 4096,
            SystemId     = "IBM  6.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "253E0901"
        },
        new FileSystemTest
        {
            TestFile     = "pcdos_7.00.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2883,
            ClusterSize  = 4096,
            SystemId     = "IBM  7.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "0E1E090D"
        },
        new FileSystemTest
        {
            TestFile    = "toshibamsdos_3.30.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 8192,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 995,
            ClusterSize = 4096,
            SystemId    = "T V3.30 ",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile     = "toshibamsdos_4.01.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 8192,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 995,
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
            Clusters     = 2002,
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
            Clusters     = 2002,
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
            Clusters    = 2002,
            ClusterSize = 4096,
            SystemId    = "IBM  3.2",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile     = "os2_1.20.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3009,
            ClusterSize  = 4096,
            SystemId     = "IBM 10.2",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "5D13D815"
        },
        new FileSystemTest
        {
            TestFile     = "os2_1.30.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3009,
            ClusterSize  = 4096,
            SystemId     = "IBM 10.2",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "5D48C815"
        },
        new FileSystemTest
        {
            TestFile     = "os2_6.307.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3009,
            ClusterSize  = 4096,
            SystemId     = "IBM 20.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "5D165815"
        },
        new FileSystemTest
        {
            TestFile     = "os2_6.514.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3009,
            ClusterSize  = 4096,
            SystemId     = "IBM 20.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "5D036415"
        },
        new FileSystemTest
        {
            TestFile     = "os2_6.617.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3009,
            ClusterSize  = 4096,
            SystemId     = "IBM 20.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "5CED2415"
        },
        new FileSystemTest
        {
            TestFile     = "os2_8.162.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3009,
            ClusterSize  = 4096,
            SystemId     = "IBM 20.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "5D470415"
        },
        new FileSystemTest
        {
            TestFile     = "os2_9.023.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3009,
            ClusterSize  = 4096,
            SystemId     = "IBM 20.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "9BA14415"
        },
        new FileSystemTest
        {
            TestFile     = "ecs.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 16384,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 1884,
            ClusterSize  = 4096,
            SystemId     = "IBM 4.50",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "E6B0B814"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.3.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3055,
            ClusterSize  = 4096,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "919408FC"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.4.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3057,
            ClusterSize  = 4096,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "BD970C0C"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.11.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 16384,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 4065,
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
            Clusters     = 3538,
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
            Clusters     = 4073,
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
            Clusters     = 2002,
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
            Clusters     = 2002,
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
            Clusters     = 2002,
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
            Clusters     = 2002,
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
            Clusters     = 2038,
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
            Clusters     = 2038,
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
            Clusters     = 2038,
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
            Clusters     = 4002,
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
            Clusters     = 2038,
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
            Clusters     = 2038,
            ClusterSize  = 4096,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "94313E7E"
        },
        new FileSystemTest
        {
            TestFile     = "winnt_4.00.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 28672,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3513,
            ClusterSize  = 4096,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "00621759"
        },
        new FileSystemTest
        {
            TestFile     = "winvista.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 16384,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3058,
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
            Clusters     = 2036,
            ClusterSize  = 4096,
            SystemId     = "BeOS    ",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "00000000"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.0.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = false,
            Clusters     = 3009,
            ClusterSize  = 4096,
            VolumeSerial = "670000"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.29.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3009,
            ClusterSize  = 4096,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609AC96F"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.34.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3009,
            ClusterSize  = 4096,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609BAA17"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.37.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3009,
            ClusterSize  = 4096,
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "609D1C96"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.38.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3009,
            ClusterSize  = 4096,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609C3CCA"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.2.17.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3009,
            ClusterSize  = 4096,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609C651A"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.2.20.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3009,
            ClusterSize  = 4096,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609C9B1B"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.4.18.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3009,
            ClusterSize  = 4096,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609CA8CE"
        },
        new FileSystemTest
        {
            TestFile     = "freebsd_6.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3009,
            ClusterSize  = 4096,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "76D11EF0"
        },
        new FileSystemTest
        {
            TestFile     = "freebsd_7.0.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3009,
            ClusterSize  = 4096,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "52491EF0"
        },
        new FileSystemTest
        {
            TestFile     = "freebsd_8.2.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3009,
            ClusterSize  = 4096,
            SystemId     = "BSD4.4  ",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "DF9D1EEF"
        },
        new FileSystemTest
        {
            TestFile     = "dflybsd_1.0.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2891,
            ClusterSize  = 4096,
            SystemId     = "BSD  4.4",
            VolumeName   = "NO NAME",
            VolumeSerial = "63C81AFB"
        },
        new FileSystemTest
        {
            TestFile     = "netbsd_1.6.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3057,
            ClusterSize  = 4096,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "84431CEB"
        },
        new FileSystemTest
        {
            TestFile     = "netbsd_6.1.5.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3057,
            ClusterSize  = 4096,
            SystemId     = "NetBSD  ",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "7F161B1D"
        },
        new FileSystemTest
        {
            TestFile     = "netbsd_7.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3057,
            ClusterSize  = 4096,
            SystemId     = "NetBSD  ",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "40961817"
        },
        new FileSystemTest
        {
            TestFile     = "openbsd_4.7.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3057,
            ClusterSize  = 4096,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "F01F07F2"
        },
        new FileSystemTest
        {
            TestFile     = "pcexchange_2.0.2.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2883,
            ClusterSize  = 4096,
            SystemId     = "PCX 2.0 ",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "241408F2"
        },
        new FileSystemTest
        {
            TestFile     = "pcexchange_2.0.5.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2883,
            ClusterSize  = 4096,
            SystemId     = "PCX 2.0 ",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "241408F2"
        },
        new FileSystemTest
        {
            TestFile     = "pcexchange_2.1.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 3055,
            ClusterSize  = 4096,
            SystemId     = "PCX 2.0 ",
            VolumeName   = "volumelabel",
            VolumeSerial = "CA6C188B"
        },
        new FileSystemTest
        {
            TestFile     = "pcexchange_2.2.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2883,
            ClusterSize  = 4096,
            SystemId     = "PCX_2.2 ",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "241408F2"
        }
    };
}