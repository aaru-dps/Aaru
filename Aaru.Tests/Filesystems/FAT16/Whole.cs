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
    public class Whole : FilesystemTest
    {
        public Whole() : base("FAT16") {}

        public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "FAT16");

        public override IFilesystem Plugin     => new FAT();
        public override bool        Partitions => false;

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile    = "msdos_3.30A_mf2ed.img.lz",
                MediaType   = MediaType.ECMA_147,
                Sectors     = 5760,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 5760,
                ClusterSize = 512,
                SystemId    = "MSDOS3.3"
            },
            new FileSystemTest
            {
                TestFile    = "msdos_3.31_mf2ed.img.lz",
                MediaType   = MediaType.ECMA_147,
                Sectors     = 5760,
                SectorSize  = 512,
                Bootable    = true,
                Clusters    = 5760,
                ClusterSize = 512,
                SystemId    = "IBM  3.3"
            },
            new FileSystemTest
            {
                TestFile     = "beos_r4.5_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2880,
                ClusterSize  = 512,
                SystemId     = "BeOS    ",
                VolumeName   = "VOLUME LABE",
                VolumeSerial = "00000000"
            },
            new FileSystemTest
            {
                TestFile     = "beos_r5_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2880,
                ClusterSize  = 512,
                SystemId     = "BeOS    ",
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "00000000"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.0.29_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2880,
                ClusterSize  = 512,
                VolumeName   = "VolumeLabel",
                VolumeSerial = "609AC308"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.0.34_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2880,
                ClusterSize  = 512,
                VolumeName   = "VolumeLabel",
                VolumeSerial = "609B8D5B"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.0.37_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2880,
                ClusterSize  = 512,
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "609D1873"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.0.38_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2880,
                ClusterSize  = 512,
                VolumeName   = "VolumeLabel",
                VolumeSerial = "609BB0D4"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.2.17_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2880,
                ClusterSize  = 512,
                VolumeName   = "VolumeLabel",
                VolumeSerial = "609C51D1"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.2.20_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2880,
                ClusterSize  = 512,
                VolumeName   = "VolumeLabel",
                VolumeSerial = "609C817B"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.4.18_mf2hd.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2880,
                ClusterSize  = 512,
                VolumeName   = "VolumeLabel",
                VolumeSerial = "609CA5B2"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.0.0_mf2hd_umsdos.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2880,
                ClusterSize  = 512,
                VolumeSerial = "670000"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.0.29_mf2hd_umsdos.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2880,
                ClusterSize  = 512,
                VolumeName   = "VolumeLabel",
                VolumeSerial = "609AC531"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.0.34_mf2hd_umsdos.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2880,
                ClusterSize  = 512,
                VolumeName   = "VolumeLabel",
                VolumeSerial = "609B8E19"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.0.37_mf2hd_umsdos.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2880,
                ClusterSize  = 512,
                VolumeName   = "VOLUMELABEL",
                VolumeSerial = "609D18ED"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.0.38_mf2hd_umsdos.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2880,
                ClusterSize  = 512,
                VolumeName   = "VolumeLabel",
                VolumeSerial = "609BB158"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.2.17_mf2hd_umsdos.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2880,
                ClusterSize  = 512,
                VolumeName   = "VolumeLabel",
                VolumeSerial = "609C545C"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.2.20_mf2hd_umsdos.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2880,
                ClusterSize  = 512,
                VolumeName   = "VolumeLabel",
                VolumeSerial = "609C87E7"
            },
            new FileSystemTest
            {
                TestFile     = "linux_2.4.18_mf2hd_umsdos.img.lz",
                MediaType    = MediaType.DOS_35_HD,
                Sectors      = 2880,
                SectorSize   = 512,
                Bootable     = true,
                Clusters     = 2880,
                ClusterSize  = 512,
                VolumeName   = "VolumeLabel",
                VolumeSerial = "609CA685"
            }
        };
    }
}