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

namespace Aaru.Tests.Filesystems.CPM;

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filesystems;
using NUnit.Framework;

[TestFixture]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class ROMDOS : FilesystemTest
{
    public ROMDOS() : base("CP/M") {}

    public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "CPM", "ROMDOS");

    public override IFilesystem Plugin     => new CPM();
    public override bool        Partitions => false;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile    = "d1_filename.dsk.lz",
            MediaType   = MediaType.CompactFloppy,
            Sectors     = 1440,
            SectorSize  = 512,
            Clusters    = 360,
            ClusterSize = 2048
        },
        new FileSystemTest
        {
            TestFile    = "d1_files.dsk.lz",
            MediaType   = MediaType.CompactFloppy,
            Sectors     = 1440,
            SectorSize  = 512,
            Clusters    = 360,
            ClusterSize = 2048
        },
        new FileSystemTest
        {
            TestFile    = "d2_filename.dsk.lz",
            MediaType   = MediaType.CompactFloppy,
            Sectors     = 1440,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 360,
            ClusterSize = 2048
        },
        new FileSystemTest
        {
            TestFile    = "d2_files.dsk.lz",
            MediaType   = MediaType.CompactFloppy,
            Sectors     = 1440,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 360,
            ClusterSize = 2048
        },
        new FileSystemTest
        {
            TestFile    = "d10_filename.dsk.lz",
            MediaType   = MediaType.CompactFloppy,
            Sectors     = 1600,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 400,
            ClusterSize = 2048
        },
        new FileSystemTest
        {
            TestFile    = "d10_files.dsk.lz",
            MediaType   = MediaType.CompactFloppy,
            Sectors     = 1600,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 400,
            ClusterSize = 2048
        },
        new FileSystemTest
        {
            TestFile    = "d20_filename.dsk.lz",
            MediaType   = MediaType.CompactFloppy,
            Sectors     = 1600,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 400,
            ClusterSize = 2048
        },
        new FileSystemTest
        {
            TestFile    = "d20_files.dsk.lz",
            MediaType   = MediaType.CompactFloppy,
            Sectors     = 1600,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 400,
            ClusterSize = 2048
        },
        new FileSystemTest
        {
            TestFile    = "d40_filename.dsk.lz",
            MediaType   = MediaType.CompactFloppy,
            Sectors     = 1600,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 400,
            ClusterSize = 2048
        },
        new FileSystemTest
        {
            TestFile    = "d40_files.dsk.lz",
            MediaType   = MediaType.CompactFloppy,
            Sectors     = 1600,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 400,
            ClusterSize = 2048
        },
        new FileSystemTest
        {
            TestFile    = "d80_filename.dsk.lz",
            MediaType   = MediaType.CompactFloppy,
            Sectors     = 1600,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 400,
            ClusterSize = 2048
        },
        new FileSystemTest
        {
            TestFile    = "d80_files.dsk.lz",
            MediaType   = MediaType.CompactFloppy,
            Sectors     = 1600,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 400,
            ClusterSize = 2048
        }
    };
}