// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Xia.cs
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

namespace Aaru.Tests.Filesystems
{
    [TestFixture]
    public class Xia : FilesystemTest
    {
        public Xia() : base("Xia filesystem") {}

        public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "Xia filesystem");
        public override IFilesystem Plugin     => new Aaru.Filesystems.Xia();
        public override bool        Partitions => true;

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile    = "linux_2.0.0.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 262144,
                SectorSize  = 512,
                Clusters    = 131008,
                ClusterSize = 1024
            },
            new FileSystemTest
            {
                TestFile    = "linux_2.0.37.aif",
                MediaType   = MediaType.GENERIC_HDD,
                Sectors     = 262144,
                SectorSize  = 512,
                Clusters    = 131008,
                ClusterSize = 1024
            }
        };
    }
}