// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : VirtualPC.cs
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

namespace Aaru.Tests.Images.VirtualPC
{
    [TestFixture]
    public class Raw : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "vpc106b_fixed_150mb_fat16.lz", "vpc213_fixed_50mb_fat16.lz", "vpc303_fixed_30mb_fat16.lz",
            "vpc30_fixed_30mb_fat16.lz", "vpc4_fixed_130mb_fat16.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // vpc106b_fixed_150mb_fat16.lz
            307024,

            // vpc213_fixed_50mb_fat16.lz
            102306,

            // vpc303_fixed_30mb_fat16.lz
            62356,

            // vpc30_fixed_30mb_fat16.lz
            61404,

            // vpc4_fixed_130mb_fat16.lz
            266016
        };

        public override uint[] _sectorSize => new uint[]
        {
            // vpc106b_fixed_150mb_fat16.lz
            512,

            // vpc213_fixed_50mb_fat16.lz
            512,

            // vpc303_fixed_30mb_fat16.lz
            512,

            // vpc30_fixed_30mb_fat16.lz
            512,

            // vpc4_fixed_130mb_fat16.lz
            512
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // vpc106b_fixed_150mb_fat16.lz
            MediaType.GENERIC_HDD,

            // vpc213_fixed_50mb_fat16.lz
            MediaType.GENERIC_HDD,

            // vpc303_fixed_30mb_fat16.lz
            MediaType.GENERIC_HDD,

            // vpc30_fixed_30mb_fat16.lz
            MediaType.GENERIC_HDD,

            // vpc4_fixed_130mb_fat16.lz
            MediaType.GENERIC_HDD
        };

        public override string[] _md5S => new[]
        {
            // vpc106b_fixed_150mb_fat16.lz
            "56eb1b7a4ea849e93de35f48b8912cd1",

            // vpc213_fixed_50mb_fat16.lz
            "f05abd9ff39f6b7e39834724b52a49e1",

            // vpc303_fixed_30mb_fat16.lz
            "46d5f39b1169a2721863b71e2944e3c2",

            // vpc30_fixed_30mb_fat16.lz
            "86b522d83ab057fa76eab0941357e1f6",

            // vpc4_fixed_130mb_fat16.lz
            "5f4d4c4f268ea19c91bf4fb49f4894b6"
        };

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "VirtualPC");
        public override IMediaImage _plugin => new ZZZRawImage();
    }
}