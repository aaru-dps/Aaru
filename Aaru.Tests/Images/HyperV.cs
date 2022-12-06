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
// Copyright © 2011-2023 Natalia Portillo
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
        public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "Hyper-V");
        public override IMediaImage _plugin => new Vhdx();

        public override BlockImageTestExpected[] Tests => new[]
        {
            new BlockImageTestExpected
            {
                TestFile   = "dynamic_exfat.vhdx.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 409600,
                SectorSize = 512,
                MD5        = "b3b3e6b89763ef45f6863d7fd1195778"
            },
            new BlockImageTestExpected
            {
                TestFile   = "dynamic_fat32.vhdx.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 409600,
                SectorSize = 512,
                MD5        = "f2a720176adb4cf70c04c56b58339024",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 34,
                        Length = 65536
                    },
                    new BlockPartitionVolumes
                    {
                        Start  = 65664,
                        Length = 339968
                    }
                }
            },
            new BlockImageTestExpected
            {
                TestFile   = "dynamic_ntfs.vhdx.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 409600,
                SectorSize = 512,
                MD5        = "bc6be23bbb139bd6fcd928f212205ce1"
            },
            new BlockImageTestExpected
            {
                TestFile   = "dynamic_udf.vhdx.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 409600,
                SectorSize = 512,
                MD5        = "cfc501f3bcc12a00aa08db30e80c25ae"
            },
            new BlockImageTestExpected
            {
                TestFile   = "fixed_exfat.vhdx.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 409600,
                SectorSize = 512,
                MD5        = "06e97867ff89301fef7e9451ad7aa4ed"
            },
            new BlockImageTestExpected
            {
                TestFile   = "fixed_fat32.vhdx.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 409600,
                SectorSize = 512,
                MD5        = "d544a96ac1bd4431b884e244717d3dca"
            },
            new BlockImageTestExpected
            {
                TestFile   = "fixed_ntfs.vhdx.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 409600,
                SectorSize = 512,
                MD5        = "b10ed3ac22d882f7080b6f9859d1e646"
            },
            new BlockImageTestExpected
            {
                TestFile   = "fixed_udf.vhdx.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 409600,
                SectorSize = 512,
                MD5        = "338ba2043d7f9cb2693c35e3194e6c9c"
            }
        };
    }
}