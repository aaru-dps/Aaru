// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MINIX.cs
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

namespace Aaru.Tests.Filesystems.MINIX.V3;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

[TestFixture]
public class MBR : FilesystemTest
{
    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Filesystems", "MINIX v3 filesystem (MBR)");
    public override IFilesystem Plugin     => new MinixFS();
    public override bool        Partitions => true;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile    = "minix_3.1.2a.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 4194304,
            SectorSize  = 512,
            Clusters    = 523151,
            ClusterSize = 4096,
            Type        = "Minix v3"
        },
        new FileSystemTest
        {
            TestFile    = "linux_4.19_minix3_flashdrive.aif",
            MediaType   = MediaType.GENERIC_HDD,
            Sectors     = 1024000,
            SectorSize  = 512,
            Clusters    = 510976,
            ClusterSize = 1024,
            Type        = "Minix v3"
        }
    };
}