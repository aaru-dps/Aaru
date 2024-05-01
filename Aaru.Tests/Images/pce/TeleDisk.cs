// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : TeleDisk.cs
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
using NUnit.Framework;

namespace Aaru.Tests.Images.pce;

[TestFixture]
public class TeleDisk : BlockMediaImageTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "pce", "TeleDisk");
    public override IMediaImage Plugin => new Aaru.Images.TeleDisk();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "md1dd_8.td0.lz",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            Md5        = "8308e749af855a3ded48d474eb7c305e"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md1dd.td0.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "b7b8a69b10ee4ec921aa8eea232fdd75"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2dd_8.td0.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "f4a77a2d2a1868dc18e8b92032d02fd2"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2dd.td0.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "099d95ac42d1a8010f914ac64ede7a70"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2hd_nec.td0.lz",
            MediaType  = MediaType.NEC_35_HD_8,
            Sectors    = 1232,
            SectorSize = 1024,
            Md5        = "fd54916f713d01b670c1a5df5e74a97f",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1232
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2hd.td0.lz",
            MediaType  = MediaType.DOS_525_HD,
            Sectors    = 2400,
            SectorSize = 512,
            Md5        = "3df7cd10044af75d77e8936af0dbf9ff"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf1dd_10.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 800,
            SectorSize = 512,
            Md5        = "d75d3e79d9c5051922d4c2226fa4a6ff"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf1dd_11.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 880,
            SectorSize = 512,
            Md5        = "e16ed33a1a466826562c681d8bdf3e27"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_10.td0.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "fd48b2c12097cbc646b4a93ef4f92259"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_11.td0.lz",
            MediaType  = MediaType.CBM_AMIGA_35_DD,
            Sectors    = 1760,
            SectorSize = 512,
            Md5        = "512f7175e753e2e2ad620d448c42545d"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_fdformat_800.td0.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "c533488a21098a62c85f1649abda2803",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1600
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_fdformat_820.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1640,
            SectorSize = 512,
            Md5        = "db9cfb6eea18820b7a7e0b5b45594471",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1640
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_freedos.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1640,
            SectorSize = 512,
            Md5        = "456390a9c6ab05cb458a03c47296de08",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1640
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd.td0.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "de3f85896f771b7e5bc4c9e3926d64e4",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1440
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2ed.td0.lz",
            MediaType  = MediaType.ECMA_147,
            Sectors    = 5760,
            SectorSize = 512,
            Md5        = "854d0d49a522b64af698e319a24cd68e"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_2m_max.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1148,
            SectorSize = 512,
            Md5        = "4b88a3e43b57778422e8b1e851a9c902"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_2m.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1804,
            SectorSize = 512,
            Md5        = "d032d928c43b66419b7404b016ec07ff"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_fdformat_168.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 332,
            SectorSize = 512,
            Md5        = "323ea79c83432663669b9bc29f13785c",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 332
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_fdformat_172.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3444,
            SectorSize = 512,
            Md5        = "9dea1e119a73a21a38d134f36b2e5564"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_freedos.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3486,
            SectorSize = 512,
            Md5        = "dbd52e9e684f97d9e2292811242bb24e",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3486
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "b4a602f67903c46eef62addb0780aa56"
            /* TODO: Division by zero because bytes per cluster is 0
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2880
                }
            }
            */
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_xdf.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "95164877cb156ff596f5e948a0a5b83f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_xdf_teledisk.td0.lz",
            MediaType  = MediaType.XDF_35,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "a8142f403d972f96787eb76655f5d42c"
        },
        /* TODO: Not opening
        new BlockImageTestExpected
        {
            TestFile   = "rx01.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2002,
            SectorSize = 0,
            MD5        = "UNKNOWN"
        },
        */ new BlockImageTestExpected
        {
            TestFile   = "rx50.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 800,
            SectorSize = 512,
            Md5        = "ccd4431139755c58f340681f63510642",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 800
                }
            }
        }
    };
}