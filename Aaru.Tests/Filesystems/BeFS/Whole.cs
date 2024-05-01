// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BeFS.cs
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
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.BeFS;

[TestFixture]
public class Whole() : FilesystemTest("befs")
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "Be File System");

    public override IFilesystem Plugin     => new Aaru.Filesystems.BeFS();
    public override bool        Partitions => false;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile    = "beos_r3.1.img.lz",
            MediaType   = MediaType.DOS_35_HD,
            Sectors     = 2880,
            SectorSize  = 512,
            Clusters    = 1440,
            ClusterSize = 1024,
            VolumeName  = "volume label goes brrrr"
        },
        new FileSystemTest
        {
            TestFile    = "beos_r4.5.img.lz",
            MediaType   = MediaType.DOS_35_HD,
            Sectors     = 2880,
            SectorSize  = 512,
            Clusters    = 1440,
            ClusterSize = 1024,
            VolumeName  = "Volume label"
        },
        new FileSystemTest
        {
            TestFile    = "beos_r5.img.lz",
            MediaType   = MediaType.DOS_35_HD,
            Sectors     = 2880,
            SectorSize  = 512,
            Clusters    = 1440,
            ClusterSize = 1024,
            VolumeName  = "Volume label"
        }
    };
}