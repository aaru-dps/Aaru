// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : COHERENT.cs
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
using Aaru.Filesystems;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.COHERENT;

[TestFixture]
public class Whole() : FilesystemTest("coherent")
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "COHERENT filesystem");

    public override IFilesystem Plugin     => new SysVfs();
    public override bool        Partitions => false;

    public override FileSystemTest[] Tests =>
    [
        new FileSystemTest
        {
            TestFile    = "coherentunix_4.2.10_dsdd.img.lz",
            MediaType   = MediaType.DOS_525_DS_DD_9,
            Sectors     = 720,
            SectorSize  = 512,
            Clusters    = 720,
            ClusterSize = 512,
            VolumeName  = "noname"
        },
        new FileSystemTest
        {
            TestFile    = "coherentunix_4.2.10_dshd.img.lz",
            MediaType   = MediaType.DOS_525_HD,
            Sectors     = 2400,
            SectorSize  = 512,
            Clusters    = 2400,
            ClusterSize = 512,
            VolumeName  = "noname"
        },
        new FileSystemTest
        {
            TestFile    = "coherentunix_4.2.10_mf2dd.img.lz",
            MediaType   = MediaType.DOS_35_DS_DD_9,
            Sectors     = 1440,
            SectorSize  = 512,
            Clusters    = 1440,
            ClusterSize = 512,
            VolumeName  = "noname"
        },
        new FileSystemTest
        {
            TestFile    = "coherentunix_4.2.10_mf2hd.img.lz",
            MediaType   = MediaType.DOS_35_HD,
            Sectors     = 2880,
            SectorSize  = 512,
            Clusters    = 2880,
            ClusterSize = 512,
            VolumeName  = "noname"
        }
    ];
}