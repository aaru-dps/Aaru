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

namespace Aaru.Tests.Images.Lisa
{
    [TestFixture]
    public class Raw : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "profile_los202.raw.lz", "profile_los31.raw.lz", "profile_macworksxl3.raw.lz", "profile_uniplus.raw.lz",
            "profile_xenix_10Mb.raw.lz", "profile_xenix.raw.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // profile_los202.raw.lz
            10108,

            // profile_los31.raw.lz
            10108,

            // profile_macworksxl3.raw.lz
            10108,

            // profile_uniplus.raw.lz
            20216,

            // profile_xenix_10Mb.raw.lz
            20216,

            // profile_xenix.raw.lz
            10108
        };

        public override uint[] _sectorSize => new uint[]
        {
            // profile_los202.raw.lz
            512,

            // profile_los31.raw.lz
            512,

            // profile_macworksxl3.raw.lz
            512,

            // profile_uniplus.raw.lz
            512,

            // profile_xenix_10Mb.raw.lz
            512,

            // profile_xenix.raw.lz
            512
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // profile_los202.raw.lz
            MediaType.GENERIC_HDD,

            // profile_los31.raw.lz
            MediaType.GENERIC_HDD,

            // profile_macworksxl3.raw.lz
            MediaType.GENERIC_HDD,

            // profile_uniplus.raw.lz
            MediaType.GENERIC_HDD,

            // profile_xenix_10Mb.raw.lz
            MediaType.GENERIC_HDD,

            // profile_xenix.raw.lz
            MediaType.GENERIC_HDD
        };

        public override string[] _md5S => new[]
        {
            // profile_los202.raw.lz
            "24001116ee48e6545e4514b3ea18b4e2",

            // profile_los31.raw.lz
            "2e328345fda18a97721c4a35cb2bb5bb",

            // profile_macworksxl3.raw.lz
            "78cdf7207060bf05c272cb8b22fc6449",

            // profile_uniplus.raw.lz
            "fc729677df4ba92da98137058aa1c298",

            // profile_xenix_10Mb.raw.lz
            "e98bf459bd20cfb466d92a91086cdaa7",

            // profile_xenix.raw.lz
            "dd146bc14be87d5ad98b961dd462f469"
        };

        public override string _dataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Lisa emulators", "raw");
        public override IMediaImage _plugin => new ZZZRawImage();
    }
}