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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.FAT16;

[TestFixture]
public class MBR() : ReadOnlyFilesystemTest("fat16")
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "FAT16 (MBR)");
    public override IFilesystem Plugin     => new FAT();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests =>
    [
        new FileSystemTest
        {
            TestFile     = "darwin_6.0.2.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65368,
            ClusterSize  = 2048,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "44B70CF7"
        },
        new FileSystemTest
        {
            TestFile     = "darwin_7.0.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65384,
            ClusterSize  = 2048,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "1F2E1B0B"
        },
        new FileSystemTest
        {
            TestFile     = "darwin_8.0.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65384,
            ClusterSize  = 2048,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "936619E5"
        },
        new FileSystemTest
        {
            TestFile    = "drdos_3.40.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 32472,
            ClusterSize = 4096,
            SystemId    = "IBM  3.2",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile    = "drdos_3.41.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 32590,
            ClusterSize = 4096,
            SystemId    = "IBM  3.2",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile    = "drdos_5.00.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 32590,
            ClusterSize = 4096,
            SystemId    = "IBM  3.3",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile    = "drdos_6.00.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 65116,
            ClusterSize = 2048,
            SystemId    = "IBM  3.3",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile    = "drdos_7.02.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 32590,
            ClusterSize = 4096,
            SystemId    = "IBM  3.3",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile    = "drdos_7.03.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 32590,
            ClusterSize = 4096,
            SystemId    = "DRDOS  7",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile     = "drdos_8.00.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "IBM  5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "0A070554"
        },
        new FileSystemTest
        {
            TestFile     = "drdos_8.10.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "IBM  5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "0A1A113F"
        },
        new FileSystemTest
        {
            TestFile    = "msdos_3.30.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 16324,
            ClusterSize = 2048,
            SystemId    = "MSDOS3.3"
        },
        new FileSystemTest
        {
            TestFile    = "msdos_3.30A.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 16324,
            ClusterSize = 2048,
            SystemId    = "MSDOS3.3"
        },
        new FileSystemTest
        {
            TestFile    = "msdos_3.31.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 32590,
            ClusterSize = 4096,
            SystemId    = "IBM  3.3",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_4.00.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "MSDOS4.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "0F2E0A16"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_4.01.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 63907,
            ClusterSize  = 8192,
            SystemId     = "MSDOS4.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "217B1909"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_5.00.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "19320A14"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_5.00a.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "30400A09"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_6.00.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "39660A0C"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_6.20rc1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 63907,
            ClusterSize  = 8192,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "3E2018E9"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_6.20.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "10550A18"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_6.21.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "05580A12"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_6.22.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "054C0A08"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_7.10.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 63907,
            ClusterSize  = 8192,
            SystemId     = "MSWIN4.1",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "356B1809"
        },
        new FileSystemTest
        {
            TestFile    = "novelldos_7.00.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 65116,
            ClusterSize = 2048,
            SystemId    = "IBM  3.3",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile    = "opendos_7.01.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 65116,
            ClusterSize = 2048,
            SystemId    = "IBM  3.3",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile     = "pcdos_2000.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "IBM  7.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "30490A0F"
        },
        new FileSystemTest
        {
            TestFile     = "pcdos_4.00.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "IBM  4.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "2D670A17"
        },
        new FileSystemTest
        {
            TestFile     = "pcdos_4.01.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "IBM  4.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "23350A15"
        },
        new FileSystemTest
        {
            TestFile     = "pcdos_5.00.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "IBM  5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "1B290A0D"
        },
        new FileSystemTest
        {
            TestFile     = "pcdos_5.02.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "IBM  5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "340B0A13"
        },
        new FileSystemTest
        {
            TestFile     = "pcdos_6.10.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "IBM  6.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "052A0A0B"
        },
        new FileSystemTest
        {
            TestFile     = "pcdos_6.30.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "IBM  6.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "1E3D0A0F"
        },
        new FileSystemTest
        {
            TestFile     = "pcdos_7.00.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "IBM  7.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "2A420A06"
        },
        new FileSystemTest
        {
            TestFile     = "msos2_1.21.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 63907,
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
            Clusters     = 63907,
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
            Clusters    = 63907,
            ClusterSize = 8192,
            SystemId    = "IBM  3.2",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile     = "os2_1.20.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65368,
            ClusterSize  = 2048,
            SystemId     = "IBM 10.2",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "5D4CA815"
        },
        new FileSystemTest
        {
            TestFile     = "os2_1.30.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65368,
            ClusterSize  = 2048,
            SystemId     = "IBM 10.2",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "5D49D815"
        },
        new FileSystemTest
        {
            TestFile     = "os2_6.307.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65368,
            ClusterSize  = 2048,
            SystemId     = "IBM 20.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "5D544815"
        },
        new FileSystemTest
        {
            TestFile     = "os2_6.514.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65368,
            ClusterSize  = 2048,
            SystemId     = "IBM 20.0",
            VolumeName   = "NO NAME",
            VolumeSerial = "5CEEF415"
        },
        new FileSystemTest
        {
            TestFile     = "os2_6.617.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65368,
            ClusterSize  = 2048,
            SystemId     = "IBM 20.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "5D430415"
        },
        new FileSystemTest
        {
            TestFile     = "os2_8.162.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65368,
            ClusterSize  = 2048,
            SystemId     = "IBM 20.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "5CF5A415"
        },
        new FileSystemTest
        {
            TestFile     = "os2_9.023.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65368,
            ClusterSize  = 2048,
            SystemId     = "IBM 20.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "9BBC8415"
        },
        new FileSystemTest
        {
            TestFile     = "ecs.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 63848,
            ClusterSize  = 8192,
            SystemId     = "IBM 4.50",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "1BE5B814"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.3.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65397,
            ClusterSize  = 2048,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "84FC0912"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.4.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65383,
            ClusterSize  = 2048,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "82451318"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.11.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 63958,
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
            Clusters     = 63830,
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
            Clusters     = 63219,
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
            Clusters     = 63907,
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
            Clusters     = 63907,
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
            Clusters     = 63907,
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
            Clusters     = 63907,
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
            Clusters     = 63964,
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
            Clusters     = 63964,
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
            Clusters     = 63964,
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
            Clusters     = 63907,
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
            Clusters     = 63964,
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
            Clusters     = 63964,
            ClusterSize  = 8192,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "C039A2EC"
        },
        new FileSystemTest
        {
            TestFile     = "winnt_4.00.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65368,
            ClusterSize  = 2048,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "D840DD84"
        },
        new FileSystemTest
        {
            TestFile     = "winvista.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 63582,
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
            Clusters     = 65132,
            ClusterSize  = 2048,
            SystemId     = "BeOS    ",
            VolumeName   = "VOLUME LABE",
            VolumeSerial = "00000000"
        },
        new FileSystemTest
        {
            TestFile    = "linux_2.0.0.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65367,
            ClusterSize = 2048
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.29.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65367,
            ClusterSize  = 2048,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609ACB34"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.34.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65367,
            ClusterSize  = 2048,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609BAB2B"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.37.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65367,
            ClusterSize  = 2048,
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "609D1C9C"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.38.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65367,
            ClusterSize  = 2048,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609C3E60"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.2.17.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65367,
            ClusterSize  = 2048,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609C663E"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.2.20.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65367,
            ClusterSize  = 2048,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609C9BCB"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.4.18.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65367,
            ClusterSize  = 2048,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609CA8D4"
        },
        new FileSystemTest
        {
            TestFile    = "linux_2.0.0_umsdos.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 65367,
            ClusterSize = 2048
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.29_umsdos.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65367,
            ClusterSize  = 2048,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609ACD0D"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.34_umsdos.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65367,
            ClusterSize  = 2048,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609BAD75"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.37_umsdos.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65367,
            ClusterSize  = 2048,
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "609D1E3A"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.38_umsdos.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65367,
            ClusterSize  = 2048,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609C4C2C"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.2.17_umsdos.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65367,
            ClusterSize  = 2048,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609C6CF3"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.2.20_umsdos.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65367,
            ClusterSize  = 2048,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609C9F38"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.4.18_umsdos.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65367,
            ClusterSize  = 2048,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609CAC4B"
        },
        new FileSystemTest
        {
            TestFile     = "amigaos_3.9.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024128,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 63907,
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
            Clusters     = 63848,
            ClusterSize  = 8192,
            SystemId     = "MSWIN4.1",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "52BEA34A"
        },
        new FileSystemTest
        {
            TestFile     = "freebsd_6.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65368,
            ClusterSize  = 2048,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "437B1EF6"
        },
        new FileSystemTest
        {
            TestFile     = "freebsd_7.0.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65368,
            ClusterSize  = 2048,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "BE841EF5"
        },
        new FileSystemTest
        {
            TestFile     = "freebsd_8.2.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 16366,
            ClusterSize  = 8192,
            SystemId     = "BSD4.4  ",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "E40B1EF6"
        },
        new FileSystemTest
        {
            TestFile     = "macos_7.6.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 63907,
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
            Clusters     = 63848,
            ClusterSize  = 8192,
            SystemId     = "IBM 4.50",
            VolumeName   = "VOLUME LABE",
            VolumeSerial = "66AAF014"
        },
        new FileSystemTest
        {
            TestFile     = "linux_4.19_fat16_msdos_flashdrive.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 63837,
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
            Clusters     = 63837,
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
            Clusters     = 65132,
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
            Clusters     = 65132,
            ClusterSize  = 2048,
            SystemId     = "BSD  4.4",
            VolumeName   = "NO NAME",
            VolumeSerial = "1B3F1B00"
        },
        new FileSystemTest
        {
            TestFile     = "netbsd_1.6.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65384,
            ClusterSize  = 2048,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "80A91CF3"
        },
        new FileSystemTest
        {
            TestFile     = "netbsd_6.1.5.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 16369,
            ClusterSize  = 8192,
            SystemId     = "NetBSD  ",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "40C91B1F"
        },
        new FileSystemTest
        {
            TestFile     = "netbsd_7.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 16370,
            ClusterSize  = 8192,
            SystemId     = "NetBSD  ",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "8413181B"
        },
        new FileSystemTest
        {
            TestFile     = "openbsd_4.7.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65384,
            ClusterSize  = 2048,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "84E407F6"
        },
        new FileSystemTest
        {
            TestFile     = "pcexchange_2.0.2.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "PCX 2.0 ",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "2A6508F2"
        },
        new FileSystemTest
        {
            TestFile     = "pcexchange_2.0.5.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "PCX 2.0 ",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "2A6508F2"
        },
        new FileSystemTest
        {
            TestFile     = "pcexchange_2.1.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65383,
            ClusterSize  = 2048,
            SystemId     = "PCX 2.0 ",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "CB430C38"
        },
        new FileSystemTest
        {
            TestFile     = "pcexchange_2.2.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65116,
            ClusterSize  = 2048,
            SystemId     = "PCX_2.2 ",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "2A6508F2"
        },
        new FileSystemTest
        {
            TestFile    = "accesspc_2.0f6.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262174,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 65391,
            ClusterSize = 2048,
            SystemId    = "INSIGNIA",
            VolumeName  = "VOLUMELABEL"
        },
        new FileSystemTest
        {
            TestFile     = "morphos_3.13.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 65397,
            ClusterSize  = 2048,
            SystemId     = "MSWIN4.1",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "519F5C06"
        }
    ];
}