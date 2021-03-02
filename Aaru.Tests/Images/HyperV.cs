// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HyperV.cs
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
    public class HyperV : BlockMediaImageTest
    {
        public override string[] _testFiles => new[]
        {
            "dynamic_exfat.vhdx.lz", "dynamic_fat32.vhdx.lz", "dynamic_ntfs.vhdx.lz", "dynamic_udf.vhdx.lz",
            "fixed_exfat.vhdx.lz", "fixed_fat32.vhdx.lz", "fixed_ntfs.vhdx.lz", "fixed_udf.vhdx.lz"
        };

        public override ulong[] _sectors => new ulong[]
        {
            // dynamic_exfat.vhdx.lz
            409600,

            // dynamic_fat32.vhdx.lz
            409600,

            // dynamic_ntfs.vhdx.lz
            409600,

            // dynamic_udf.vhdx.lz
            409600,

            // fixed_exfat.vhdx.lz
            409600,

            // fixed_fat32.vhdx.lz
            409600,

            // fixed_ntfs.vhdx.lz
            409600,

            // fixed_udf.vhdx.lz
            409600
        };

        public override uint[] _sectorSize => new uint[]
        {
            // dynamic_exfat.vhdx.lz
            512,

            // dynamic_fat32.vhdx.lz
            512,

            // dynamic_ntfs.vhdx.lz
            512,

            // dynamic_udf.vhdx.lz
            512,

            // fixed_exfat.vhdx.lz
            512,

            // fixed_fat32.vhdx.lz
            512,

            // fixed_ntfs.vhdx.lz
            512,

            // fixed_udf.vhdx.lz
            512
        };

        public override MediaType[] _mediaTypes => new[]
        {
            // dynamic_exfat.vhdx.lz
            MediaType.GENERIC_HDD,

            // dynamic_fat32.vhdx.lz
            MediaType.GENERIC_HDD,

            // dynamic_ntfs.vhdx.lz
            MediaType.GENERIC_HDD,

            // dynamic_udf.vhdx.lz
            MediaType.GENERIC_HDD,

            // fixed_exfat.vhdx.lz
            MediaType.GENERIC_HDD,

            // fixed_fat32.vhdx.lz
            MediaType.GENERIC_HDD,

            // fixed_ntfs.vhdx.lz
            MediaType.GENERIC_HDD,

            // fixed_udf.vhdx.lz
            MediaType.GENERIC_HDD
        };

        public override string[] _md5S => new[]
        {
            // dynamic_exfat.vhdx.lz
            "b3b3e6b89763ef45f6863d7fd1195778",

            // dynamic_fat32.vhdx.lz
            "f2a720176adb4cf70c04c56b58339024",

            // dynamic_ntfs.vhdx.lz
            "bc6be23bbb139bd6fcd928f212205ce1",

            // dynamic_udf.vhdx.lz
            "cfc501f3bcc12a00aa08db30e80c25ae",

            // fixed_exfat.vhdx.lz
            "06e97867ff89301fef7e9451ad7aa4ed",

            // fixed_fat32.vhdx.lz
            "d544a96ac1bd4431b884e244717d3dca",

            // fixed_ntfs.vhdx.lz
            "b10ed3ac22d882f7080b6f9859d1e646",

            // fixed_udf.vhdx.lz
            "338ba2043d7f9cb2693c35e3194e6c9c"
        };

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Hyper-V");
        public override IMediaImage _plugin => new Vhdx();
    }
}