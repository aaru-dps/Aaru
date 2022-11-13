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
public class Whole : FilesystemTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "MINIX v3 filesystem");

    public override IFilesystem Plugin     => new MinixFS();
    public override bool        Partitions => false;

    public override FileSystemTest[] Tests => new[]
    {
        new FileSystemTest
        {
            TestFile    = "minix_3.1.2a_dsdd.img.lz",
            MediaType   = MediaType.DOS_525_DS_DD_9,
            Sectors     = 720,
            SectorSize  = 512,
            Clusters    = 90,
            ClusterSize = 4096,
            Type        = "Minix v3"
        },
        new FileSystemTest
        {
            TestFile    = "minix_3.1.2a_dshd.img.lz",
            MediaType   = MediaType.DOS_525_HD,
            Sectors     = 2400,
            SectorSize  = 512,
            Clusters    = 300,
            ClusterSize = 4096,
            Type        = "Minix v3"
        },
        new FileSystemTest
        {
            TestFile    = "minix_3.1.2a_mf2dd.img.lz",
            MediaType   = MediaType.DOS_35_DS_DD_9,
            Sectors     = 1440,
            SectorSize  = 512,
            Clusters    = 180,
            ClusterSize = 4096,
            Type        = "Minix v3"
        },
        new FileSystemTest
        {
            TestFile    = "minix_3.1.2a_mf2hd.img.lz",
            MediaType   = MediaType.DOS_35_HD,
            Sectors     = 2880,
            SectorSize  = 512,
            Clusters    = 360,
            ClusterSize = 4096,
            Type        = "Minix v3"
        }
    };
}