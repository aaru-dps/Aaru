// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : raw.cs
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

namespace Aaru.Tests.Images.pce
{
    [TestFixture]
    public class Raw : BlockMediaImageTest
    {
        public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "pce", "raw");
        public override IMediaImage _plugin => new ZZZRawImage();

        public override BlockImageTestExpected[] Tests => new[]
        {
            new BlockImageTestExpected
            {
                TestFile   = "md1dd_8.img.lz",
                MediaType  = MediaType.DOS_525_SS_DD_8,
                Sectors    = 320,
                SectorSize = 512,
                MD5        = "8308e749af855a3ded48d474eb7c305e"
            },
            new BlockImageTestExpected
            {
                TestFile   = "md1dd.img.lz",
                MediaType  = MediaType.DOS_525_SS_DD_9,
                Sectors    = 360,
                SectorSize = 512,
                MD5        = "b7b8a69b10ee4ec921aa8eea232fdd75"
            },
            new BlockImageTestExpected
            {
                TestFile   = "md2dd_8.img.lz",
                MediaType  = MediaType.DOS_525_DS_DD_8,
                Sectors    = 640,
                SectorSize = 512,
                MD5        = "f4a77a2d2a1868dc18e8b92032d02fd2"
            },
            new BlockImageTestExpected
            {
                TestFile   = "md2dd.img.lz",
                MediaType  = MediaType.DOS_525_DS_DD_9,
                Sectors    = 720,
                SectorSize = 512,
                MD5        = "099d95ac42d1a8010f914ac64ede7a70"
            },
            new BlockImageTestExpected
            {
                TestFile   = "md2hd.img.lz",
                MediaType  = MediaType.DOS_525_HD,
                Sectors    = 2400,
                SectorSize = 512,
                MD5        = "3df7cd10044af75d77e8936af0dbf9ff"
            },
            new BlockImageTestExpected
            {
                TestFile   = "md2hd_nec.img.lz",
                MediaType  = MediaType.SHARP_525,
                Sectors    = 1232,
                SectorSize = 1024,
                MD5        = "fd54916f713d01b670c1a5df5e74a97f",
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
                TestFile   = "mf1dd_10.img.lz",
                MediaType  = MediaType.AppleSonySS,
                Sectors    = 800,
                SectorSize = 512,
                MD5        = "d75d3e79d9c5051922d4c2226fa4a6ff"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf1dd_11.img.lz",
                MediaType  = MediaType.ATARI_35_SS_DD_11,
                Sectors    = 880,
                SectorSize = 512,
                MD5        = "e16ed33a1a466826562c681d8bdf3e27"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf1dd_gcr.img.lz",
                MediaType  = MediaType.AppleSonySS,
                Sectors    = 800,
                SectorSize = 512,
                MD5        = "c5d92544c3e78b7f0a9b4baaa9a64eec"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf2dd_10.img.lz",
                MediaType  = MediaType.AppleSonyDS,
                Sectors    = 1600,
                SectorSize = 512,
                MD5        = "fd48b2c12097cbc646b4a93ef4f92259"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf2dd_11.img.lz",
                MediaType  = MediaType.CBM_AMIGA_35_DD,
                Sectors    = 1760,
                SectorSize = 512,
                MD5        = "512f7175e753e2e2ad620d448c42545d"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf2dd_fdformat_800.img.lz",
                MediaType  = MediaType.AppleSonyDS,
                Sectors    = 1600,
                SectorSize = 512,
                MD5        = "c533488a21098a62c85f1649abda2803",
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
                TestFile   = "mf2dd_fdformat_820.img.lz",
                MediaType  = MediaType.FDFORMAT_35_DD,
                Sectors    = 1640,
                SectorSize = 512,
                MD5        = "db9cfb6eea18820b7a7e0b5b45594471",
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
                TestFile   = "mf2dd_freedos.img.lz",
                MediaType  = MediaType.FDFORMAT_35_DD,
                Sectors    = 1640,
                SectorSize = 512,
                MD5        = "456390a9c6ab05cb458a03c47296de08",
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
                TestFile   = "mf2dd_gcr.img.lz",
                MediaType  = MediaType.AppleSonyDS,
                Sectors    = 1600,
                SectorSize = 512,
                MD5        = "93e71b9ecdb39d3ec9245b4f451856d4"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf2dd.img.lz",
                MediaType  = MediaType.DOS_35_DS_DD_9,
                Sectors    = 1440,
                SectorSize = 512,
                MD5        = "de3f85896f771b7e5bc4c9e3926d64e4",
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
                TestFile   = "mf2ed.img.lz",
                MediaType  = MediaType.ECMA_147,
                Sectors    = 5760,
                SectorSize = 512,
                MD5        = "854d0d49a522b64af698e319a24cd68e"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf2hd_2m.img.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 3605,
                SectorSize = 512,
                MD5        = "c741c78eecd673f8fc49e77459871940"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf2hd_2m_max.img.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 3768,
                SectorSize = 512,
                MD5        = "0393fbfee10e47c71e0fb7b39237be49"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf2hd_fdformat_168.img.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 3372,
                SectorSize = 512,
                MD5        = "7f9164dc43bffc895db751ba1d9b55a9",
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
                TestFile   = "mf2hd_fdformat_172.img.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 3448,
                SectorSize = 512,
                MD5        = "d6ff5df3707887a6ba4cfdc30b3deff4"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf2hd_freedos.img.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 3486,
                SectorSize = 512,
                MD5        = "dbd52e9e684f97d9e2292811242bb24e",
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
                TestFile   = "mf2hd.img.lz",
                MediaType  = MediaType.GENERIC_HDD,
                Sectors    = 2888,
                SectorSize = 512,
                MD5        = "f5fff7704fb677ebf23d27cd937c9403"
                /* TODO: Division by zero because bytes per cluster is 0
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 2888
                    }
                }
                */
            },
            /* TODO: XDF not implemented
            new BlockImageTestExpected
            {
                TestFile   = "mf2hd_xdf.img.lz",
                MediaType  = MediaType.XDF_35,
                Sectors    = 670,
                SectorSize = 8192,
                MD5        = "UNKNOWN"
            },
            new BlockImageTestExpected
            {
                TestFile   = "mf2hd_xdf_teledisk.img.lz",
                MediaType  = MediaType.XDF_35,
                Sectors    = 3680,
                SectorSize = 512,
                MD5        = "UNKNOWN"
            },
            */ new BlockImageTestExpected
            {
                TestFile   = "rx01.img.lz",
                MediaType  = MediaType.ECMA_54,
                Sectors    = 2002,
                SectorSize = 128,
                MD5        = "5b4e36d92b180c3845387391cb5a1c64"
            },
            new BlockImageTestExpected
            {
                TestFile   = "rx50.img.lz",
                MediaType  = MediaType.AppleSonySS,
                Sectors    = 800,
                SectorSize = 512,
                MD5        = "ccd4431139755c58f340681f63510642",
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
}