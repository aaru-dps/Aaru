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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class Virtual98 : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "v98_128.hdd.lz", "v98_20.hdd.lz", "v98_256.hdd.lz", "v98_41.hdd.lz", "v98_512.hdd.lz", "v98_65.hdd.lz",
            "v98_80.hdd.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // v98_128.hdd.lz
            524288,

            // v98_20.hdd.lz
            81920,

            // v98_256.hdd.lz
            1048576,

            // v98_41.hdd.lz
            167936,

            // v98_512.hdd.lz
            2097152,

            // v98_65.hdd.lz
            266240,

            // v98_80.hdd.lz
            327680
        };

        public override uint[] _sectorSize => new uint[]
        {
            // v98_128.hdd.lz
            256,

            // v98_20.hdd.lz
            256,

            // v98_256.hdd.lz
            256,

            // v98_41.hdd.lz
            256,

            // v98_512.hdd.lz
            256,

            // v98_65.hdd.lz
            256,

            // v98_80.hdd.lz
            256
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // v98_128.hdd.lz
            MediaType.GENERIC_HDD,

            // v98_20.hdd.lz
            MediaType.GENERIC_HDD,

            // v98_256.hdd.lz
            MediaType.GENERIC_HDD,

            // v98_41.hdd.lz
            MediaType.GENERIC_HDD,

            // v98_512.hdd.lz
            MediaType.GENERIC_HDD,

            // v98_65.hdd.lz
            MediaType.GENERIC_HDD,

            // v98_80.hdd.lz
            MediaType.GENERIC_HDD
        };

        public override string[] _md5S => new[]
        {
            // v98_128.hdd.lz
            "be3693b92a5242101e80087611b33092",

            // v98_20.hdd.lz
            "811b2a9d08abbecf4cb75531d5e51808",

            // v98_256.hdd.lz
            "cf4375422f50d62e163d697a18542eca",

            // v98_41.hdd.lz
            "fe4fc08015f1e3a4562e8e867107b561",

            // v98_512.hdd.lz
            "afb49485f0ef2b39e8377c1fe880e77b",

            // v98_65.hdd.lz
            "9e4c0bc8bc955b1a21a94df0f7bec3ab",

            // v98_80.hdd.lz
            "f5906261c390ea5c5a0e46864fb066cd"
        };

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Virtual98");
        public override IMediaImage _plugin => new DiscImages.Virtual98();
    }
}