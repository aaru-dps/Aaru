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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.HFSX;

[TestFixture]
public class MBR() : FilesystemTest("hfsx")
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "Apple HFSX (MBR)");

    public override IFilesystem Plugin     => new AppleHFSPlus();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "macosx_10.11.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 393216,
            SectorSize   = 512,
            Clusters     = 49140,
            ClusterSize  = 4096,
            SystemId     = "10.0",
            VolumeSerial = "C2BCCCE6DE5BC98D"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.11_journal.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 409600,
            SectorSize   = 512,
            Clusters     = 51187,
            ClusterSize  = 4096,
            SystemId     = "HFSJ",
            VolumeSerial = "AC54CD78C75CC30F"
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
            TestFile     = "darwin_7.0.1_journal.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32760,
            ClusterSize  = 4096,
            SystemId     = "HFSJ",
            VolumeSerial = "49296DD5D4998317"
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
            VolumeSerial = "6086D6721C36DB41"
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
            VolumeSerial = "E6ADDA7180241419"
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
            VolumeSerial = "EA6229EBC75E4F92"
        },
        new FileSystemTest
        {
            TestFile     = "linux_4.19_hfsx_flashdrive.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Clusters     = 127744,
            ClusterSize  = 4096,
            SystemId     = "H+Lx",
            VolumeSerial = "5E4A8781D3C9286C"
        }
    };
}