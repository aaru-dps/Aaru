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
    public class VirtualPc : BlockMediaImageTest
    {
        public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "VirtualPC");
        public override IMediaImage _plugin => new Vhd();

        public override BlockImageTestExpected[] Tests => new[]
        {
            new BlockImageTestExpected
            {
                TestFile   = "vpc504_dynamic_250mb.lz",
                MediaType  = MediaType.Unknown,
                Sectors    = 511056,
                SectorSize = 512,
                MD5        = "cbcee980986d980f6add1f9622a5f917"
            },
            new BlockImageTestExpected
            {
                TestFile   = "vpc504_fixed_10mb.lz",
                MediaType  = MediaType.Unknown,
                Sectors    = 20468,
                SectorSize = 512,
                MD5        = "b790693b1c94bed209ee1bb9d0b6a075"
            },
            new BlockImageTestExpected
            {
                TestFile   = "vpc50_dynamic_250mb.lz",
                MediaType  = MediaType.Unknown,
                Sectors    = 511056,
                SectorSize = 512,
                MD5        = "c0955193d302f5eae2138a3669c89669"
            },
            new BlockImageTestExpected
            {
                TestFile   = "vpc50_fixed_10mb.lz",
                MediaType  = MediaType.Unknown,
                Sectors    = 20468,
                SectorSize = 512,
                MD5        = "1c843b778d48a67b78e4ca65ab602673"
            },
            new BlockImageTestExpected
            {
                TestFile   = "vpc601_dynamic_250mb.vhd.lz",
                MediaType  = MediaType.Unknown,
                Sectors    = 511056,
                SectorSize = 512,
                MD5        = "3e3675037a8ec4b78ebafdc2b25e5ceb"
            },
            new BlockImageTestExpected
            {
                TestFile   = "vpc601_fixed_10mb.vhd.lz",
                MediaType  = MediaType.Unknown,
                Sectors    = 20468,
                SectorSize = 512,
                MD5        = "4b4e98a5bba2469382132f9289ae1c57"
            },
            new BlockImageTestExpected
            {
                TestFile   = "vpc60_differencing_parent_250mb.vhd.lz",
                MediaType  = MediaType.Unknown,
                Sectors    = 511056,
                SectorSize = 512,
                MD5        = "3e3675037a8ec4b78ebafdc2b25e5ceb"
            },
            new BlockImageTestExpected
            {
                TestFile   = "vpc60_dynamic_250mb.vhd.lz",
                MediaType  = MediaType.Unknown,
                Sectors    = 511056,
                SectorSize = 512,
                MD5        = "723b2ed575e0e87f253f672f39b3a49f"
            },
            new BlockImageTestExpected
            {
                TestFile   = "vpc60_fixed_10mb.vhd.lz",
                MediaType  = MediaType.Unknown,
                Sectors    = 20468,
                SectorSize = 512,
                MD5        = "4b4e98a5bba2469382132f9289ae1c57"
            },
            new BlockImageTestExpected
            {
                TestFile   = "vpc702_differencing_parent_250mb.vhd.lz",
                MediaType  = MediaType.Unknown,
                Sectors    = 511056,
                SectorSize = 512,
                MD5        = "0f6b4f4bb22f02e88e442638f803e4f4"
            },
            new BlockImageTestExpected
            {
                TestFile   = "vpc702_dynamic_250mb.vhd.lz",
                MediaType  = MediaType.Unknown,
                Sectors    = 511056,
                SectorSize = 512,
                MD5        = "3e3675037a8ec4b78ebafdc2b25e5ceb"
            },
            new BlockImageTestExpected
            {
                TestFile   = "vpc702_fixed_10mb.vhd.lz",
                MediaType  = MediaType.Unknown,
                Sectors    = 20468,
                SectorSize = 512,
                MD5        = "4b4e98a5bba2469382132f9289ae1c57"
            }
        };
    }
}