// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FAT32.cs
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

namespace Aaru.Tests.Filesystems.FAT32;

[TestFixture]
public class MBR : ReadOnlyFilesystemTest
{
    public MBR() : base("fat32") {}

    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "FAT32 (MBR)");
    public override IFilesystem Plugin     => new FAT();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "darwin_6.0.2.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262017,
            ClusterSize  = 512,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "03A50CFD"
        },
        new FileSystemTest
        {
            TestFile     = "darwin_7.0.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262081,
            ClusterSize  = 512,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "24981B10"
        },
        new FileSystemTest
        {
            TestFile     = "darwin_8.0.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262081,
            ClusterSize  = 512,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "829119EB"
        },
        new FileSystemTest
        {
            TestFile     = "drdos_7.03.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 8388608,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 1048233,
            ClusterSize  = 4096,
            SystemId     = "DRDOS7.X",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "5955996C"
        },
        new FileSystemTest
        {
            TestFile     = "drdos_8.00.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 261009,
            ClusterSize  = 512,
            SystemId     = "IBM  7.1",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "09F93020"
        },
        new FileSystemTest
        {
            TestFile     = "drdos_8.10.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 261009,
            ClusterSize  = 512,
            SystemId     = "IBM  7.1",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "09FF2765"
        },
        new FileSystemTest
        {
            TestFile     = "msdos_7.10.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 8388608,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 1048233,
            ClusterSize  = 4096,
            SystemId     = "MSWIN4.1",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "3B331809"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.3.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262136,
            ClusterSize  = 512,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "B4E7161C"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.4.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262080,
            ClusterSize  = 512,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "F47713E9"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.11.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 4194304,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 524287,
            ClusterSize  = 4096,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "42D51EF1"
        },
        new FileSystemTest
        {
            TestFile     = "win10.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 4194304,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 524016,
            ClusterSize  = 4096,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "48073346"
        },
        new FileSystemTest
        {
            TestFile     = "win2000.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 8388608,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 1048233,
            ClusterSize  = 4096,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "EC62E6DE"
        },
        new FileSystemTest
        {
            TestFile     = "win95osr2.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 4194304,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 524152,
            ClusterSize  = 4096,
            SystemId     = "MSWIN4.1",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "2A310DE4"
        },
        new FileSystemTest
        {
            TestFile     = "win95osr2.5.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 4194304,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 524152,
            ClusterSize  = 4096,
            SystemId     = "MSWIN4.1",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "0C140DFC"
        },
        new FileSystemTest
        {
            TestFile     = "win95osr2.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 4194304,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 524152,
            ClusterSize  = 4096,
            SystemId     = "MSWIN4.1",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "3E310D18"
        },
        new FileSystemTest
        {
            TestFile     = "win98se.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 4194304,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 524112,
            ClusterSize  = 4096,
            SystemId     = "MSWIN4.1",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "0D3D0EED"
        },
        new FileSystemTest
        {
            TestFile     = "win98.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 4194304,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 524112,
            ClusterSize  = 4096,
            SystemId     = "MSWIN4.1",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "0E131162"
        },
        new FileSystemTest
        {
            TestFile     = "winme.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 4194304,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 524112,
            ClusterSize  = 4096,
            SystemId     = "MSWIN4.1",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "3F500F02"
        },
        new FileSystemTest
        {
            TestFile     = "winvista.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 4194304,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 523520,
            ClusterSize  = 4096,
            SystemId     = "MSDOS5.0",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "82EB4C04"
        },
        new FileSystemTest
        {
            TestFile     = "beos_r4.5.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 261072,
            ClusterSize  = 512,
            SystemId     = "BeOS    ",
            VolumeName   = "VOLUME LABE",
            VolumeSerial = "00000000"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.37.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 262016,
            ClusterSize  = 512,
            SystemId     = "mkdosfs",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "609D1CBD"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.2.17.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262016,
            ClusterSize  = 512,
            SystemId     = "mkdosfs",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609C673B"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.2.20.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262016,
            ClusterSize  = 512,
            SystemId     = "mkdosfs",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609C9CCA"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.4.18.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262016,
            ClusterSize  = 512,
            SystemId     = "mkdosfs",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609CA8D7"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.0.37_umsdos.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 262016,
            ClusterSize  = 512,
            SystemId     = "mkdosfs",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "609D1F50"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.2.17_umsdos.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262016,
            ClusterSize  = 512,
            SystemId     = "mkdosfs",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609C6F93"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.2.20_umsdos.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262016,
            ClusterSize  = 512,
            SystemId     = "mkdosfs",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609C9FFF"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.4.18_umsdos.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262016,
            ClusterSize  = 512,
            SystemId     = "mkdosfs",
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609CAC4E"
        },
        new FileSystemTest
        {
            TestFile     = "aros.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 4194304,
            SectorSize   = 512,
            Clusters     = 524160,
            ClusterSize  = 4096,
            SystemId     = "MSWIN4.1",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "5CAC9B4E"
        },
        new FileSystemTest
        {
            TestFile     = "freebsd_6.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 32752,
            ClusterSize  = 4096,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "40931EFC"
        },
        new FileSystemTest
        {
            TestFile     = "freebsd_7.0.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 32752,
            ClusterSize  = 4096,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "F4181EFC"
        },
        new FileSystemTest
        {
            TestFile     = "freebsd_8.2.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 4194304,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 65514,
            ClusterSize  = 32768,
            SystemId     = "BSD4.4  ",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "26E20E0F"
        },
        new FileSystemTest
        {
            TestFile     = "freedos_1.2.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 8388608,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 1048233,
            ClusterSize  = 4096,
            SystemId     = "FRDOS4.1",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "3E0C1BE8"
        },
        new FileSystemTest
        {
            TestFile     = "ecs20_fstester.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 127744,
            ClusterSize  = 4096,
            SystemId     = "mkfs.fat",
            VolumeName   = "Volume labe",
            VolumeSerial = "63084BBA"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.2_umsdos32_flashdrive.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 127882,
            ClusterSize  = 4096,
            SystemId     = "mkdosfs",
            VolumeName   = "DICSETTER",
            VolumeSerial = "5CC7908D"
        },
        new FileSystemTest
        {
            TestFile     = "linux_4.19_fat32_msdos_flashdrive.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 127744,
            ClusterSize  = 4096,
            SystemId     = "mkfs.fat",
            VolumeName   = "DICSETTER",
            VolumeSerial = "D1290612"
        },
        new FileSystemTest
        {
            TestFile     = "linux_4.19_vfat32_flashdrive.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 127744,
            ClusterSize  = 4096,
            SystemId     = "mkfs.fat",
            VolumeName   = "DICSETTER",
            VolumeSerial = "79BCA86E"
        },
        new FileSystemTest
        {
            TestFile     = "beos_r5.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 261072,
            ClusterSize  = 512,
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
            Clusters     = 32634,
            ClusterSize  = 4096,
            SystemId     = "BSD  4.4",
            VolumeName   = "NO NAME",
            VolumeSerial = "837A1B05"
        },
        new FileSystemTest
        {
            TestFile     = "netbsd_1.6.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 32760,
            ClusterSize  = 4096,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "CFAD1CF7"
        },
        new FileSystemTest
        {
            TestFile     = "netbsd_6.1.5.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 16380,
            ClusterSize  = 8192,
            SystemId     = "NetBSD  ",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "A1CC1BED"
        },
        new FileSystemTest
        {
            TestFile     = "netbsd_7.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 131040,
            ClusterSize  = 1024,
            SystemId     = "NetBSD  ",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "843A181F"
        },
        new FileSystemTest
        {
            TestFile     = "openbsd_4.7.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 32634,
            ClusterSize  = 4096,
            SystemId     = "BSD  4.4",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "C27607FB"
        },
        new FileSystemTest
        {
            TestFile     = "pcexchange_2.2.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 262080,
            ClusterSize  = 512,
            SystemId     = "PCX_2.2 ",
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "CBCE71E4"
        }
    };
}