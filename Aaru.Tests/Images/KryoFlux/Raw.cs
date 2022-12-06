// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Raw.cs
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

namespace Aaru.Tests.Images.KryoFlux
{
    [TestFixture]
    public class Raw : BlockMediaImageTest
    {
        public override string DataFolder =>
            Path.Combine(Consts.TestFilesRoot, "Media image formats", "KryoFlux", "raw");
        public override IMediaImage _plugin => new ZZZRawImage();

        public override BlockImageTestExpected[] Tests => new[]
        {
            new BlockImageTestExpected
            {
                TestFile   = "mf1dd_gcr_s0.img.lz",
                MediaType  = MediaType.AppleSonySS,
                Sectors    = 800,
                SectorSize = 512,
                MD5        = "c1b868482a064686d2a592f3246c2958"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf2dd_acorn.img.lz",
                MediaType  = MediaType.AppleSonyDS,
                Sectors    = 1600,
                SectorSize = 512,
                MD5        = "2626f65b49ec085253c41fa2e2a9e788"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf2dd_amiga.adf.lz",
                MediaType  = MediaType.CBM_AMIGA_35_DD,
                Sectors    = 1760,
                SectorSize = 512,
                MD5        = "7db6730656efb22695cdf0a49e2674c9"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf2dd_fdformat_820.img.lz",
                MediaType  = MediaType.FDFORMAT_35_DD,
                Sectors    = 1640,
                SectorSize = 512,
                MD5        = "9d978dff1196b456b8372d78e6b17970"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf2dd_gcr.img.lz",
                MediaType  = MediaType.AppleSonyDS,
                Sectors    = 1600,
                SectorSize = 512,
                MD5        = "ee038347920d088c14f79e6c5fc241c9"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf2hd_fdformat_172.img.lz",
                MediaType  = MediaType.FDFORMAT_35_HD,
                Sectors    = 3444,
                SectorSize = 512,
                MD5        = "9dea1e119a73a21a38d134f36b2e5564"
            }
        };
    }
}