// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Parallels.cs
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
using NUnit.Framework;

namespace Aaru.Tests.Images.QEMU;

[TestFixture]
public class Parallels : BlockMediaImageTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "QEMU", "Parallels");
    public override IMediaImage Plugin => new Aaru.Images.Parallels();

    public override BlockImageTestExpected[] Tests =>
    [
        new BlockImageTestExpected
        {
            TestFile   = "parallels.hdd.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 251904,
            SectorSize = 512,
            Md5        = "4bfc9e9e2dd86aa52ef709e77d2617ed",
            Partitions =
            [
                new BlockPartitionVolumes
                {
                    Start  = 63,
                    Length = 251841
                }
            ]
        }
    ];
}