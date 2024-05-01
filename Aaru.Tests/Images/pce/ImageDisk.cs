// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ImageDisk.cs
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

namespace Aaru.Tests.Images.pce;

[TestFixture]
public class ImageDisk : BlockMediaImageTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "pce", "ImageDisk");
    public override IMediaImage Plugin => new Imd();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "md1dd_8.imd.lz",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            Md5        = "8308e749af855a3ded48d474eb7c305e"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md1dd.imd.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "b7b8a69b10ee4ec921aa8eea232fdd75"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2dd_8.imd.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "f4a77a2d2a1868dc18e8b92032d02fd2"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2dd.imd.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "099d95ac42d1a8010f914ac64ede7a70"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2hd.imd.lz",
            MediaType  = MediaType.NEC_35_HD_15,
            Sectors    = 2400,
            SectorSize = 512,
            Md5        = "3df7cd10044af75d77e8936af0dbf9ff"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2hd_nec.imd.lz",
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
            TestFile   = "mf1dd_10.imd.lz",
            MediaType  = MediaType.RX50,
            Sectors    = 800,
            SectorSize = 512,
            Md5        = "d75d3e79d9c5051922d4c2226fa4a6ff"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf1dd_11.imd.lz",
            MediaType  = MediaType.ATARI_35_SS_DD_11,
            Sectors    = 880,
            SectorSize = 512,
            Md5        = "e16ed33a1a466826562c681d8bdf3e27"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf1dd_gcr.imd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 800,
            SectorSize = 512,
            Md5        = "c5d92544c3e78b7f0a9b4baaa9a64eec"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_10.imd.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "fd48b2c12097cbc646b4a93ef4f92259"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_11.imd.lz",
            MediaType  = MediaType.CBM_AMIGA_35_DD,
            Sectors    = 1760,
            SectorSize = 512,
            Md5        = "512f7175e753e2e2ad620d448c42545d"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_fdformat_800.imd.lz",
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
            TestFile   = "mf2dd_fdformat_820.imd.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
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
            TestFile   = "mf2dd_freedos.imd.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
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
            TestFile   = "mf2dd_gcr.imd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "93e71b9ecdb39d3ec9245b4f451856d4"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd.imd.lz",
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
            TestFile   = "mf2ed.imd.lz",
            MediaType  = MediaType.ECMA_147,
            Sectors    = 5760,
            SectorSize = 512,
            Md5        = "854d0d49a522b64af698e319a24cd68e"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_2m.imd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1812,
            SectorSize = 1024,
            Md5        = "c741c78eecd673f8fc49e77459871940"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_fdformat_168.imd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3372,
            SectorSize = 512,
            Md5        = "7f9164dc43bffc895db751ba1d9b55a9",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3372
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_fdformat_172.imd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3448,
            SectorSize = 512,
            Md5        = "d6ff5df3707887a6ba4cfdc30b3deff4"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_freedos.imd.lz",
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
            TestFile   = "mf2hd.imd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2882,
            SectorSize = 2048,
            Md5        = "f5fff7704fb677ebf23d27cd937c9403"
            /* TODO: Division by zero because bytes per cluster is 0
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2882
                }
            }
            */
        },
        new BlockImageTestExpected
        {
            TestFile   = "rx01.imd.lz",
            MediaType  = MediaType.RX01,
            Sectors    = 2002,
            SectorSize = 128,
            Md5        = "5b4e36d92b180c3845387391cb5a1c64"
        },
        new BlockImageTestExpected
        {
            TestFile   = "rx50.imd.lz",
            MediaType  = MediaType.RX50,
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