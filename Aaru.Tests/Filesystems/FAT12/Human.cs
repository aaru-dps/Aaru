// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FAT12.cs
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

namespace Aaru.Tests.Filesystems.FAT12
{
    [TestFixture]
    public class Human : ReadOnlyFilesystemTest
    {
        public Human() : base("FAT12") {}

        public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "FAT12 (Human68K)");
        public override IFilesystem Plugin => new FAT();
        public override bool Partitions => false;

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile    = "diska.aif",
                MediaType   = MediaType.SHARP_525,
                Sectors     = 1232,
                SectorSize  = 1024,
                Bootable    = true,
                Clusters    = 1232,
                ClusterSize = 1024,
                SystemId    = "Hudson soft 2.00"
            },
            new FileSystemTest
            {
                TestFile    = "diskb.aif",
                MediaType   = MediaType.SHARP_525,
                Sectors     = 1232,
                SectorSize  = 1024,
                Bootable    = true,
                Clusters    = 1232,
                ClusterSize = 1024,
                SystemId    = "Hudson soft 2.00"
            }
        };
    }
}