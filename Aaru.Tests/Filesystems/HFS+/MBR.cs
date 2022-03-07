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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/



// ReSharper disable CheckNamespace

namespace Aaru.Tests.Filesystems.HFSPlus;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

[TestFixture]
public class MBR : FilesystemTest
{
    public MBR() : base("HFS+") {}

    public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Apple HFS+ (MBR)");

    public override IFilesystem Plugin     => new AppleHFSPlus();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "macosx_10.3.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32767,
            ClusterSize  = 4096,
            SystemId     = "10.0",
            VolumeSerial = "97B4DC7AC201699A"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.3_journal.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32767,
            ClusterSize  = 4096,
            SystemId     = "HFSJ",
            VolumeSerial = "9BF2CE504121C20F"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.4.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32760,
            ClusterSize  = 4096,
            SystemId     = "10.0",
            VolumeSerial = "092FD8AF262EAE9C"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.4_journal.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32760,
            ClusterSize  = 4096,
            SystemId     = "HFSJ",
            VolumeSerial = "326CC53BB867CCDC"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.11.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 303104,
            SectorSize   = 512,
            Clusters     = 37878,
            ClusterSize  = 4096,
            SystemId     = "10.0",
            VolumeSerial = "C84F550907D13F50"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.11_journal.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 352256,
            SectorSize   = 512,
            Clusters     = 44021,
            ClusterSize  = 4096,
            SystemId     = "HFSJ",
            VolumeSerial = "016599F88029F73D"
        },
        new FileSystemTest
        {
            TestFile    = "linux.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 32512,
            ClusterSize = 4096,
            SystemId    = "10.0"
        },
        new FileSystemTest
        {
            TestFile    = "linux_journal.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 32512,
            ClusterSize = 4096,
            SystemId    = "10.0"
        },
        new FileSystemTest
        {
            TestFile    = "darwin_1.3.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 32752,
            ClusterSize = 4096,
            SystemId    = "10.0"
        },
        new FileSystemTest
        {
            TestFile    = "darwin_1.4.1.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 32752,
            ClusterSize = 4096,
            SystemId    = "10.0"
        },
        new FileSystemTest
        {
            TestFile    = "darwin_6.0.2.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 262144,
            SectorSize  = 512,
            Clusters    = 32752,
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
            VolumeSerial = "B2B3DCFC3EBF92F9"
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
            VolumeSerial = "EF9142272A79F2C7"
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
            VolumeSerial = "191CACE470B64449"
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
            VolumeSerial = "27E25570C58F3CDB"
        },
        new FileSystemTest
        {
            TestFile     = "linux_4.19_hfs+_flashdrive.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Clusters     = 127744,
            ClusterSize  = 4096,
            SystemId     = "H+Lx",
            VolumeSerial = "B9BAC6856878A404"
        }
    };
}