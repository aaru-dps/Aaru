// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Virtual98.cs
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

namespace Aaru.Tests.Images;

[TestFixture]
public class Virtual98 : BlockMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Virtual98");
    public override IMediaImage _plugin    => new DiscImages.Virtual98();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "v98_128.hdd.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 524288,
            SectorSize = 256,
            MD5        = "be3693b92a5242101e80087611b33092"
        },
        new BlockImageTestExpected
        {
            TestFile   = "v98_20.hdd.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 81920,
            SectorSize = 256,
            MD5        = "811b2a9d08abbecf4cb75531d5e51808"
        },
        new BlockImageTestExpected
        {
            TestFile   = "v98_256.hdd.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 1048576,
            SectorSize = 256,
            MD5        = "cf4375422f50d62e163d697a18542eca"
        },
        new BlockImageTestExpected
        {
            TestFile   = "v98_41.hdd.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 167936,
            SectorSize = 256,
            MD5        = "fe4fc08015f1e3a4562e8e867107b561"
        },
        new BlockImageTestExpected
        {
            TestFile   = "v98_512.hdd.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 2097152,
            SectorSize = 256,
            MD5        = "afb49485f0ef2b39e8377c1fe880e77b"
        },
        new BlockImageTestExpected
        {
            TestFile   = "v98_65.hdd.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 266240,
            SectorSize = 256,
            MD5        = "9e4c0bc8bc955b1a21a94df0f7bec3ab"
        },
        new BlockImageTestExpected
        {
            TestFile   = "v98_80.hdd.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 327680,
            SectorSize = 256,
            MD5        = "f5906261c390ea5c5a0e46864fb066cd"
        }
    };
}