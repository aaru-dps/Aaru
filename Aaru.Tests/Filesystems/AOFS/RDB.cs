// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AOFS.cs
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

namespace Aaru.Tests.Filesystems.AOFS;

[TestFixture]
public class RDB() : FilesystemTest("aofs")
{
    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Filesystems", "Amiga Old File System (RDB)");

    public override IFilesystem Plugin     => new AmigaDOSPlugin();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile     = "amigaos_3.9.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024128,
            SectorSize   = 512,
            Clusters     = 510032,
            ClusterSize  = 1024,
            VolumeName   = "Volume label",
            VolumeSerial = "A56D13BB"
        },
        new FileSystemTest
        {
            TestFile     = "amigaos_3.9_intl.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 1024128,
            SectorSize   = 512,
            Clusters     = 510032,
            ClusterSize  = 1024,
            VolumeName   = "Volume label",
            VolumeSerial = "A56D0415"
        },
        new FileSystemTest
        {
            TestFile     = "aros.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 409600,
            SectorSize   = 512,
            Clusters     = 407232,
            ClusterSize  = 512,
            VolumeName   = "Volume label",
            VolumeSerial = "A582F3A0"
        },
        new FileSystemTest
        {
            TestFile     = "aros_intl.aif",
            MediaType    = MediaType.GENERIC_HDD,
            Sectors      = 409600,
            SectorSize   = 512,
            Clusters     = 407232,
            ClusterSize  = 512,
            VolumeName   = "Volume label",
            VolumeSerial = "A5830B06"
        }
    };
}