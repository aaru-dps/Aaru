// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HFSPlus.cs
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

// ReSharper disable CheckNamespace

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.HFSPlus;

[TestFixture]
public class APM : FilesystemTest
{
    public APM() : base("hfsplus") {}

    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "Apple HFS+ (APM)");
    public override IFilesystem Plugin     => new AppleHFSPlus();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "macosx_10.11.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 409600,
            SectorSize   = 512,
            Clusters     = 51190,
            ClusterSize  = 4096,
            SystemId     = "10.0",
            VolumeSerial = "FA94762D086A18A9"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.11_journal.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 614400,
            SectorSize   = 512,
            Clusters     = 76790,
            ClusterSize  = 4096,
            SystemId     = "HFSJ",
            VolumeSerial = "33D4A309C8E7BD10"
        },
        new FileSystemTest
        {
            TestFile    = "darwin_1.0.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 32760,
            ClusterSize = 4096,
            SystemId    = "10.0"
        },
        new FileSystemTest
        {
            TestFile    = "darwin_1.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 32760,
            ClusterSize = 4096,
            SystemId    = "8.10"
        },
        new FileSystemTest
        {
            TestFile    = "darwin_1.3.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 32760,
            ClusterSize = 4096,
            SystemId    = "10.0"
        },
        new FileSystemTest
        {
            TestFile    = "darwin_1.4.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 32760,
            ClusterSize = 4096,
            SystemId    = "10.0"
        },
        new FileSystemTest
        {
            TestFile    = "darwin_6.0.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 32760,
            ClusterSize = 4096,
            SystemId    = "10.0"
        },
        new FileSystemTest
        {
            TestFile     = "darwin_7.0.1_journal.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32760,
            ClusterSize  = 4096,
            SystemId     = "HFSJ",
            VolumeSerial = "DF8853FD178AE8BE"
        },
        new FileSystemTest
        {
            TestFile     = "darwin_7.0.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32760,
            ClusterSize  = 4096,
            SystemId     = "10.0",
            VolumeSerial = "6FE2BC81D9725A6B"
        },
        new FileSystemTest
        {
            TestFile     = "darwin_8.0.1_journal.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32760,
            ClusterSize  = 4096,
            SystemId     = "HFSJ",
            VolumeSerial = "06309CEDD929D53A"
        },
        new FileSystemTest
        {
            TestFile     = "darwin_8.0.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32760,
            ClusterSize  = 4096,
            SystemId     = "10.0",
            VolumeSerial = "4A3AE13A1F410E25"
        },
        new FileSystemTest
        {
            TestFile    = "macos_8.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 261340,
            ClusterSize = 512,
            SystemId    = "8.10"
        },
        new FileSystemTest
        {
            TestFile    = "macos_9.0.4.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 260824,
            ClusterSize = 512,
            SystemId    = "8.10"
        },
        new FileSystemTest
        {
            TestFile    = "macos_9.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 260828,
            ClusterSize = 512,
            SystemId    = "8.10"
        },
        new FileSystemTest
        {
            TestFile    = "macos_9.2.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 260828,
            ClusterSize = 512,
            SystemId    = "8.10"
        },
        new FileSystemTest
        {
            TestFile    = "macos_9.2.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 260824,
            ClusterSize = 512,
            SystemId    = "8.10"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.0.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32526,
            ClusterSize  = 4096,
            SystemId     = "10.0",
            VolumeSerial = "ED2DC1BFDBB80AFB"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.1.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32526,
            ClusterSize  = 4096,
            SystemId     = "10.0",
            VolumeSerial = "6CEAA69B7E8E154A"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.2.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32526,
            ClusterSize  = 4096,
            SystemId     = "10.0",
            VolumeSerial = "BDCB2D31BDBD60CD"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.3_journal.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32525,
            ClusterSize  = 4096,
            SystemId     = "HFSJ",
            VolumeSerial = "7FDF37D35B3C220A"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.3.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32525,
            ClusterSize  = 4096,
            SystemId     = "10.0",
            VolumeSerial = "EA42A9AC5FAC2CE3"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.4_journal.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32525,
            ClusterSize  = 4096,
            SystemId     = "HFSJ",
            VolumeSerial = "05A25B5EF0D99402"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.4.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32525,
            ClusterSize  = 4096,
            SystemId     = "10.0",
            VolumeSerial = "2812682CF7B8EB3B"
        }
    };
}