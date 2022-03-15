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
public class AMSDOS : FilesystemTest
{
    public AMSDOS() : base("CP/M") {}

    public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "CPM", "AMSDOS");

    public override IFilesystem Plugin     => new CPM();
    public override bool        Partitions => false;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile    = "data_filename.dsk.lz",
            MediaType   = MediaType.CompactFloppy,
            Sectors     = 360,
            SectorSize  = 512,
            Clusters    = 180,
            ClusterSize = 1024
        },
        new FileSystemTest
        {
            TestFile    = "data_files.dsk.lz",
            MediaType   = MediaType.CompactFloppy,
            Sectors     = 360,
            SectorSize  = 512,
            Clusters    = 180,
            ClusterSize = 1024
        },
        new FileSystemTest
        {
            TestFile    = "ibm_filename.dsk.lz",
            MediaType   = MediaType.CompactFloppy,
            Sectors     = 320,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 155,
            ClusterSize = 1024
        },
        new FileSystemTest
        {
            TestFile    = "ibm_files.dsk.lz",
            MediaType   = MediaType.CompactFloppy,
            Sectors     = 360,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 155,
            ClusterSize = 1024
        },
        new FileSystemTest
        {
            TestFile    = "system_filename.dsk.lz",
            MediaType   = MediaType.CompactFloppy,
            Sectors     = 360,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 170,
            ClusterSize = 1024
        },
        new FileSystemTest
        {
            TestFile    = "system_files.dsk.lz",
            MediaType   = MediaType.CompactFloppy,
            Sectors     = 360,
            SectorSize  = 512,
            Bootable    = true,
            Clusters    = 170,
            ClusterSize = 1024
        }
    };
}