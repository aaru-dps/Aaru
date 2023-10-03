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

namespace Aaru.Tests.Images.Lisa;

[TestFixture]
public class Raw : BlockMediaImageTest
{
    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Media image formats", "Lisa emulators", "raw");

    public override IMediaImage Plugin => new ZZZRawImage();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "profile_los202.raw.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 10108,
            SectorSize = 512,
            Md5        = "24001116ee48e6545e4514b3ea18b4e2"
        },
        new BlockImageTestExpected
        {
            TestFile   = "profile_los31.raw.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 10108,
            SectorSize = 512,
            Md5        = "2e328345fda18a97721c4a35cb2bb5bb"
        },
        new BlockImageTestExpected
        {
            TestFile   = "profile_macworksxl3.raw.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 10108,
            SectorSize = 512,
            Md5        = "78cdf7207060bf05c272cb8b22fc6449"
        },
        new BlockImageTestExpected
        {
            TestFile   = "profile_uniplus.raw.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 20216,
            SectorSize = 512,
            Md5        = "fc729677df4ba92da98137058aa1c298"
        },
        new BlockImageTestExpected
        {
            TestFile   = "profile_xenix_10Mb.raw.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 20216,
            SectorSize = 512,
            Md5        = "e98bf459bd20cfb466d92a91086cdaa7"
        },
        new BlockImageTestExpected
        {
            TestFile   = "profile_xenix.raw.lz",
            MediaType  = MediaType.GENERIC_HDD,
            Sectors    = 10108,
            SectorSize = 512,
            Md5        = "dd146bc14be87d5ad98b961dd462f469"
        }
    };
}