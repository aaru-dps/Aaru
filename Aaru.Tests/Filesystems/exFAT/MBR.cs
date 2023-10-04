// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : exFAT.cs
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
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.exFAT;

[TestFixture]
public class MBR() : FilesystemTest("exfat")
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "exFAT (MBR)");

    public override IFilesystem Plugin     => new Aaru.Filesystems.exFAT();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "linux.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32464,
            ClusterSize  = 4096,
            VolumeSerial = "603565AC"
        },
        new FileSystemTest
        {
            TestFile     = "macosx_10.11.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32712,
            ClusterSize  = 4096,
            VolumeSerial = "595AC21E"
        },
        new FileSystemTest
        {
            TestFile     = "win10.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32448,
            ClusterSize  = 4096,
            VolumeSerial = "20126663"
        },
        new FileSystemTest
        {
            TestFile     = "winvista.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 262144,
            SectorSize   = 512,
            Clusters     = 32208,
            ClusterSize  = 4096,
            VolumeSerial = "0AC5CA52"
        },
        new FileSystemTest
        {
            TestFile     = "linux_4.19_exfat_flashdrive.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024000,
            SectorSize   = 512,
            Clusters     = 15964,
            ClusterSize  = 32768,
            VolumeSerial = "636E083B"
        }
    };
}