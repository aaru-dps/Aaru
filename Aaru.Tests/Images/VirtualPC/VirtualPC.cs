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

namespace Aaru.Tests.Images.VirtualPC;

[TestFixture]
public class VirtualPc : BlockMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "VirtualPC");
    public override IMediaImage Plugin     => new Vhd();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "vpc40_dynamic_128mb.vhd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 261936,
            SectorSize = 512,
            Md5        = "cc634bb9bbf2dcdd88cfe251390e2049",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 17,
                    Length = 261647
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "vpc40_fixed_128mb.vhd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 261936,
            SectorSize = 512,
            Md5        = "0b6f655387e101c0249e922b1714a484",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 17,
                    Length = 261647
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "vpc50_dynamic_512mb.vhd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1052352,
            SectorSize = 512,
            Md5        = "12ebc62199ecaae97efe406ee891d68f",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 63,
                    Length = 1052289
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "vpc50_fixed_512mb.vhd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1052352,
            SectorSize = 512,
            Md5        = "4943fc799eddd6f386b2923847824ffc",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 63,
                    Length = 1052289
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "vpc50_dynamic_250mb.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 511056,
            SectorSize = 512,
            Md5        = "a6041066df8f52f5d14b8200766d6bb5"
        },
        new BlockImageTestExpected
        {
            TestFile   = "vpc50_fixed_10mb.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 20468,
            SectorSize = 512,
            Md5        = "1c843b778d48a67b78e4ca65ab602673"
        },
        new BlockImageTestExpected
        {
            TestFile   = "vpc504_dynamic_250mb.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 511056,
            SectorSize = 512,
            Md5        = "e924cd1bbb16f6a6056f81df410922ae"
        },
        new BlockImageTestExpected
        {
            TestFile   = "vpc504_fixed_10mb.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 20468,
            SectorSize = 512,
            Md5        = "b790693b1c94bed209ee1bb9d0b6a075"
        },
        new BlockImageTestExpected
        {
            TestFile   = "vpc60_differencing_parent_250mb.vhd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 511056,
            SectorSize = 512,
            Md5        = "1f9e3dc39db37a9e01fede6a12844222"
        },
        new BlockImageTestExpected
        {
            TestFile   = "vpc60_dynamic_250mb.vhd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 511056,
            SectorSize = 512,
            Md5        = "943a9da318111f50a92c3f2314fad1e0"
        },
        new BlockImageTestExpected
        {
            TestFile   = "vpc60_fixed_10mb.vhd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 20468,
            SectorSize = 512,
            Md5        = "4b4e98a5bba2469382132f9289ae1c57"
        },
        new BlockImageTestExpected
        {
            TestFile   = "vpc601_dynamic_250mb.vhd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 511056,
            SectorSize = 512,
            Md5        = "1f9e3dc39db37a9e01fede6a12844222"
        },
        new BlockImageTestExpected
        {
            TestFile   = "vpc601_fixed_10mb.vhd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 20468,
            SectorSize = 512,
            Md5        = "4b4e98a5bba2469382132f9289ae1c57"
        },
        new BlockImageTestExpected
        {
            TestFile   = "vpc702_differencing_parent_250mb.vhd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 31456656,
            SectorSize = 512,
            Md5        = "df41b76f8532fc8ef775f89212191244"
        },
        new BlockImageTestExpected
        {
            TestFile   = "vpc702_dynamic_250mb.vhd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 511056,
            SectorSize = 512,
            Md5        = "1f9e3dc39db37a9e01fede6a12844222"
        },
        new BlockImageTestExpected
        {
            TestFile   = "vpc702_fixed_10mb.vhd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 20468,
            SectorSize = 512,
            Md5        = "4b4e98a5bba2469382132f9289ae1c57"
        }
    };
}