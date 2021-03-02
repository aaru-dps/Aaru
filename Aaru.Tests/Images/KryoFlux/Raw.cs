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
// Copyright © 2011-2021 Natalia Portillo
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
        public override string[] _testFiles => new[]
        {
            "mf1dd_gcr_s0.img.lz", "mf2dd_acorn.img.lz", "mf2dd_amiga.adf.lz", "mf2dd_fdformat_820.img.lz",
            "mf2dd_gcr.img.lz", "mf2hd_fdformat_172.img.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // mf1dd_gcr_s0.img.lz
            800,

            // mf2dd_acorn.img.lz
            1600,

            // mf2dd_amiga.adf.lz
            1760,

            // mf2dd_fdformat_820.img.lz
            1640,

            // mf2dd_gcr.img.lz
            1600,

            // mf2hd_fdformat_172.img.lz
            3444
        };

        public override uint[] _sectorSize => new uint[]
        {
            // mf1dd_gcr_s0.img.lz
            512,

            // mf2dd_acorn.img.lz
            512,

            // mf2dd_amiga.adf.lz
            512,

            // mf2dd_fdformat_820.img.lz
            512,

            // mf2dd_gcr.img.lz
            512,

            // mf2hd_fdformat_172.img.lz
            512
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // mf1dd_gcr_s0.img.lz
            MediaType.AppleSonySS,

            // mf2dd_acorn.img.lz
            MediaType.AppleSonyDS,

            // mf2dd_amiga.adf.lz
            MediaType.CBM_AMIGA_35_DD,

            // mf2dd_fdformat_820.img.lz
            MediaType.FDFORMAT_35_DD,

            // mf2dd_gcr.img.lz
            MediaType.AppleSonyDS,

            // mf2hd_fdformat_172.img.lz
            MediaType.FDFORMAT_35_HD
        };

        public override string[] _md5S => new[]
        {
            // mf1dd_gcr_s0.img.lz
            "c1b868482a064686d2a592f3246c2958",

            // mf2dd_acorn.img.lz
            "2626f65b49ec085253c41fa2e2a9e788",

            // mf2dd_amiga.adf.lz
            "7db6730656efb22695cdf0a49e2674c9",

            // mf2dd_fdformat_820.img.lz
            "9d978dff1196b456b8372d78e6b17970",

            // mf2dd_gcr.img.lz
            "ee038347920d088c14f79e6c5fc241c9",

            // mf2hd_fdformat_172.img.lz
            "9dea1e119a73a21a38d134f36b2e5564"
        };

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "KryoFlux", "raw");
        public override IMediaImage _plugin => new ZZZRawImage();
    }
}