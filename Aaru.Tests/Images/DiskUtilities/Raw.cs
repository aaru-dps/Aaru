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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Images;
using NUnit.Framework;

namespace Aaru.Tests.Images.DiskUtilities;

[TestFixture]
public class Raw : BlockMediaImageTest
{
    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Media image formats", "disk-analyse", "raw");

    public override IMediaImage Plugin => new ZZZRawImage();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_acorn.img.lz",
            MediaType  = MediaType.AppleSonyDS,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "2626f65b49ec085253c41fa2e2a9e788"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_amiga.adf.lz",
            MediaType  = MediaType.CBM_AMIGA_35_DD,
            Sectors    = 1760,
            SectorSize = 512,
            Md5        = "7db6730656efb22695cdf0a49e2674c9"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_fdformat_820.img.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            Md5        = "9d978dff1196b456b8372d78e6b17970"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_2m.img.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 3605,
            SectorSize = 512,
            Md5        = "7ee82cecd23b30cc9aa6f0ec59877851"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_2m_max.img.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 3768,
            SectorSize = 512,
            Md5        = "c96c0be31797a0e6c9f23aad8ae38555"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_fdformat_172.img.lz",
            MediaType  = MediaType.FDFORMAT_35_HD,
            Sectors    = 3444,
            SectorSize = 512,
            Md5        = "9dea1e119a73a21a38d134f36b2e5564"
        }
        /* TODO: XDF reading is not implemented
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_xdf.img.lz",
            MediaType  = MediaType.XDF_35,
            Sectors    = 670,
            SectorSize = 8192,
            MD5        = "UNKNOWN"
        }
        */
    };
}