// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
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
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.CPM
{
    [TestFixture]
    public class SharpPersonalCPM : ReadOnlyFilesystemTest
    {
        public SharpPersonalCPM() : base("CP/M") {}

        public override string DataFolder =>
            Path.Combine(Consts.TestFilesRoot, "Filesystems", "CPM", "Sharp Personal CPM");

        public override IFilesystem Plugin     => new Aaru.Filesystems.CPM();
        public override bool        Partitions => false;

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile    = "filename.imd",
                MediaType   = MediaType.ACORN_525_DS_DD,
                Sectors     = 2560,
                SectorSize  = 256,
                Bootable    = true,
                Clusters    = 316,
                ClusterSize = 2048
            },
            new FileSystemTest
            {
                TestFile    = "files.imd",
                MediaType   = MediaType.ACORN_525_DS_DD,
                Sectors     = 2560,
                SectorSize  = 256,
                Bootable    = true,
                Clusters    = 316,
                ClusterSize = 2048
            }
        };
    }
}