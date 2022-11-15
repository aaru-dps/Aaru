// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HFSX.cs
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

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.HFSX;

[TestFixture]
public class APM : FilesystemTest
{
    public APM() : base("HFSX") {}

    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "Apple HFSX (APM)");
    public override IFilesystem Plugin     => new AppleHFSPlus();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "macosx_10.4_journal.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32758,
            ClusterSize  = 4096,
            SystemId     = "HFSJ",
            VolumeSerial = "62F6BD837D62D1DB"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.4.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32758,
            ClusterSize  = 4096,
            SystemId     = "10.0",
            VolumeSerial = "32517120509F8539"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.11.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 819200,
            SectorSize   = 512,
            Clusters     = 102390,
            ClusterSize  = 4096,
            SystemId     = "10.0",
            VolumeSerial = "CC2D56884950D9AE"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.11_journal.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1228800,
            SectorSize   = 512,
            Clusters     = 153590,
            ClusterSize  = 4096,
            SystemId     = "HFSJ",
            VolumeSerial = "7AF1175D8EA7A072"
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
            VolumeSerial = "675A390EBFDFC534"
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
            VolumeSerial = "EA0C27012D10135B"
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
            VolumeSerial = "50059A9AA0119AD3"
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
            VolumeSerial = "BCA9EBC858957259"
        }
    };
}