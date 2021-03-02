// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : NHDr0.cs
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
using Aaru.DiscImages;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class NHDr0 : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "t98n_128.nhd.lz", "t98n_20.nhd.lz", "t98n_256.nhd.lz", "t98n_41.nhd.lz", "t98n_512.nhd.lz",
            "t98n_65.nhd.lz", "t98n_80.nhd.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // t98n_128.nhd.lz
            261120,

            // t98n_20.nhd.lz
            40800,

            // t98n_256.nhd.lz
            522240,

            // t98n_41.nhd.lz
            83640,

            // t98n_512.nhd.lz
            1044480,

            // t98n_65.nhd.lz
            132600,

            // t98n_80.nhd.lz
            163200
        };

        public override uint[] _sectorSize => new uint[]
        {
            // t98n_128.nhd.lz
            512,

            // t98n_20.nhd.lz
            512,

            // t98n_256.nhd.lz
            512,

            // t98n_41.nhd.lz
            512,

            // t98n_512.nhd.lz
            512,

            // t98n_65.nhd.lz
            512,

            // t98n_80.nhd.lz
            512
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // t98n_128.nhd.lz
            MediaType.GENERIC_HDD,

            // t98n_20.nhd.lz
            MediaType.GENERIC_HDD,

            // t98n_256.nhd.lz
            MediaType.GENERIC_HDD,

            // t98n_41.nhd.lz
            MediaType.GENERIC_HDD,

            // t98n_512.nhd.lz
            MediaType.GENERIC_HDD,

            // t98n_65.nhd.lz
            MediaType.GENERIC_HDD,

            // t98n_80.nhd.lz
            MediaType.GENERIC_HDD
        };

        public override string[] _md5S => new[]
        {
            // t98n_128.nhd.lz
            "af7c3cfa315b6661300017f865bf26d6",

            // t98n_20.nhd.lz
            "bcb390d0b4d12feac29dbadc1a623c99",

            // t98n_256.nhd.lz
            "e50e78b3742f5f89dd1a5573ba3141c4",

            // t98n_41.nhd.lz
            "007acca6fb53f90728d78f7c40c2b094",

            // t98n_512.nhd.lz
            "42d1cb6fc2a9df39ecd53002edd978d6",

            // t98n_65.nhd.lz
            "b53f5b406234663de6c2bdffac88322d",

            // t98n_80.nhd.lz
            "fe9ecc6f0b5beb9635a1595155941925"
        };

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "T-98 Next");
        public override IMediaImage _plugin => new Nhdr0();
    }
}