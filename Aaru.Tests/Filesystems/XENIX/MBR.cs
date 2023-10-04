// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : XENIX.cs
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

namespace Aaru.Tests.Filesystems.XENIX;

[TestFixture]
public class MBR() : FilesystemTest("xenixfs")
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "XENIX filesystem (MBR)");
    public override IFilesystem Plugin => new SysVfs();
    public override bool Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile    = "xenix_2.3.2d.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 40960,
            SectorSize  = 512,
            ClusterSize = 1024,
            VolumeName  = ""
        },
        new FileSystemTest
        {
            TestFile    = "xenix_2.3.4h.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 40960,
            SectorSize  = 512,
            ClusterSize = 1024,
            VolumeName  = ""
        },
        new FileSystemTest
        {
            TestFile    = "scoopenserver_5.0.7hw.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 2097152,
            SectorSize  = 512,
            ClusterSize = 1024,
            VolumeName  = ""
        }
    };
}