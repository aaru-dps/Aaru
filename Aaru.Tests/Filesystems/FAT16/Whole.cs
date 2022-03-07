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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Tests.Filesystems.FAT16;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

[TestFixture]
public class Whole : ReadOnlyFilesystemTest
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
            Clusters    = 5711,
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
            Clusters    = 5711,
            ClusterSize = 512,
            SystemId    = "IBM  3.3"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.2.17_mf2hd.img.lz",
            MediaType    = MediaType.DOS_35_HD,
            Sectors      = 2880,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2841,
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
            Clusters     = 2841,
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
            Clusters     = 2841,
            ClusterSize  = 512,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609CA5B2"
        },
        new FileSystemTest
        {
            TestFile     = "linux_2.2.17_mf2hd_umsdos.img.lz",
            MediaType    = MediaType.DOS_35_HD,
            Sectors      = 2880,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 2841,
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
            Clusters     = 2841,
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
            Clusters     = 2841,
            ClusterSize  = 512,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "609CA685"
        },
        new FileSystemTest
        {
            TestFile     = "macos_8.5.img.lz",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 204800,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 51091,
            ClusterSize  = 2048,
            VolumeName   = "FAT",
            VolumeSerial = "34050000"
        },
        new FileSystemTest
        {
            TestFile     = "macos_8.6.img.lz",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 204800,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 51091,
            ClusterSize  = 2048,
            VolumeName   = "FAT",
            VolumeSerial = "A6040000"
        },
        new FileSystemTest
        {
            TestFile     = "pcexchange_3.0.3.img.lz",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 32731,
            ClusterSize  = 4096,
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "A1360000"
        },
        new FileSystemTest
        {
            TestFile     = "pcexchange_3.0.4.img.lz",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 32731,
            ClusterSize  = 4096,
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "4B320000"
        },
        new FileSystemTest
        {
            TestFile     = "pcexchange_3.0.5.img.lz",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Bootable     = true,
            Clusters     = 32731,
            ClusterSize  = 4096,
            VolumeName   = "VOLUMELABEL",
            VolumeSerial = "E3230000"
        }
    };
}