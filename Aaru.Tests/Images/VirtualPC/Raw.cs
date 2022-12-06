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
// Copyright © 2011-2023 Natalia Portillo
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
        public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "VirtualPC");
        public override IMediaImage _plugin => new ZZZRawImage();

        public override BlockImageTestExpected[] Tests => new[]
        {
            new BlockImageTestExpected
            {
                TestFile   = "vpc106b_fixed_150mb_fat16.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 307024,
                SectorSize = 512,
                MD5        = "56eb1b7a4ea849e93de35f48b8912cd1"
            },
            new BlockImageTestExpected
            {
                TestFile   = "vpc213_fixed_50mb_fat16.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 102306,
                SectorSize = 512,
                MD5        = "f05abd9ff39f6b7e39834724b52a49e1"
            },
            new BlockImageTestExpected
            {
                TestFile   = "vpc303_fixed_30mb_fat16.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 62356,
                SectorSize = 512,
                MD5        = "46d5f39b1169a2721863b71e2944e3c2"
            },
            new BlockImageTestExpected
            {
                TestFile   = "vpc30_fixed_30mb_fat16.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 61404,
                SectorSize = 512,
                MD5        = "86b522d83ab057fa76eab0941357e1f6"
            },
            new BlockImageTestExpected
            {
                TestFile   = "vpc4_fixed_130mb_fat16.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 266016,
                SectorSize = 512,
                MD5        = "5f4d4c4f268ea19c91bf4fb49f4894b6",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 17,
                        Length = 265727
                    }
                }
            }
        };
    }
}