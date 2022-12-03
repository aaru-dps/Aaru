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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.CPM;

[TestFixture]
public class AmstradCPM : ReadOnlyFilesystemTest
{
    public AmstradCPM() : base("CP/M") {}

    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "CPM", "Amstrad CPM");

    public override IFilesystem Plugin     => new Aaru.Filesystems.CPM();
    public override bool        Partitions => false;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile    = "data_filename.imd",
            MediaType   = MediaType.DOS_525_SS_DD_9,
            Bootable    = true,
            Sectors     = 360,
            SectorSize  = 512,
            Clusters    = 171,
            ClusterSize = 1024
        },
        new FileSystemTest
        {
            TestFile    = "data_files.imd",
            MediaType   = MediaType.DOS_525_SS_DD_9,
            Bootable    = true,
            Sectors     = 360,
            SectorSize  = 512,
            Clusters    = 171,
            ClusterSize = 1024
        },
        new FileSystemTest
        {
            TestFile    = "system_filename.imd",
            MediaType   = MediaType.DOS_525_SS_DD_9,
            Sectors     = 360,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 171,
            ClusterSize = 1024
        },
        new FileSystemTest
        {
            TestFile    = "system_files.imd",
            MediaType   = MediaType.DOS_525_SS_DD_9,
            Sectors     = 360,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 171,
            ClusterSize = 1024
        }
    };
}