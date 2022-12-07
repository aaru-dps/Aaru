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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.FAT16;

[TestFixture]
public class Atari : ReadOnlyFilesystemTest
{
    public Atari() : base("fat16") {}

    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "FAT16 (Atari)");
    public override IFilesystem Plugin     => new FAT();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "tos_1.00_gem.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Clusters     = 12230,
            ClusterSize  = 1024,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "DA6664"
        },
        new FileSystemTest
        {
            TestFile     = "tos_1.00_bgm.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 8188,
            ClusterSize  = 16384,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "D0EFA1"
        },
        new FileSystemTest
        {
            TestFile     = "tos_1.02_gem.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Clusters     = 12230,
            ClusterSize  = 1024,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "1079CC"
        },
        new FileSystemTest
        {
            TestFile     = "tos_1.02_bgm.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 8188,
            ClusterSize  = 16384,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "9C65B3"
        },
        new FileSystemTest
        {
            TestFile     = "tos_1.04_gem.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Clusters     = 12230,
            ClusterSize  = 1024,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "DD5AA6"
        },
        new FileSystemTest
        {
            TestFile     = "tos_1.04_bgm.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 16374,
            ClusterSize  = 8192,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "D430E2"
        },
        new FileSystemTest
        {
            TestFile     = "tos_1.06_gem.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Clusters     = 12230,
            ClusterSize  = 1024,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "D0599C"
        },
        new FileSystemTest
        {
            TestFile     = "tos_1.06_bgm.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 16374,
            ClusterSize  = 8192,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "895043"
        },
        new FileSystemTest
        {
            TestFile     = "tos_1.62_gem.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Clusters     = 12230,
            ClusterSize  = 1024,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "D22E19"
        },
        new FileSystemTest
        {
            TestFile     = "tos_1.62_bgm.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 16374,
            ClusterSize  = 8192,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "6566D8"
        },
        new FileSystemTest
        {
            TestFile     = "tos_2.06_gem.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 24576,
            SectorSize   = 512,
            Clusters     = 12230,
            ClusterSize  = 1024,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "700332"
        },
        new FileSystemTest
        {
            TestFile     = "tos_2.06_bgm.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 16374,
            ClusterSize  = 8192,
            VolumeName   = "VolumeLabel",
            VolumeSerial = "086A33"
        }
    };
}