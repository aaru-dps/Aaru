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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using NUnit.Framework;

namespace Aaru.Tests.Images;

[TestFixture]
public class ImageDisk : BlockMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "ImageDisk");
    public override IMediaImage _plugin    => new Imd();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "CPM1_ALL.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_8,
            Sectors    = 1280,
            SectorSize = 512,
            MD5        = "b5ab1915fc3d7fceecfcd7fda82f6b0d",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1280
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0000.IMD.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            MD5        = "e8bbbd22db87181974e12ba0227ea011"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0001.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "9f5635f3df4d880a500910b0ad1ab535"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0002.IMD.lz",
            MediaType  = MediaType.ACORN_35_DS_HD,
            Sectors    = 1600,
            SectorSize = 1024,
            MD5        = "3bad4b4db8f5e2f991637fccf7a25740"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0003.IMD.lz",
            MediaType  = MediaType.ACORN_35_DS_DD,
            Sectors    = 800,
            SectorSize = 1024,
            MD5        = "85574aebeef03eb355bf8541955d06ea"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0004.IMD.lz",
            MediaType  = MediaType.ACORN_35_DS_HD,
            Sectors    = 1600,
            SectorSize = 1024,
            MD5        = "a481bd5a8281dad089edbef390c136ed"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0006.IMD.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            MD5        = "46fce47baf08c6f093f2c355a603543d"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0009.IMD.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            MD5        = "95ea232f59e44db374b994cfe7f1c07f",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2880
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0010.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            MD5        = "9e2b01f4397db2a6c76e2bc267df37b3"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0011.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1280,
            SectorSize = 1024,
            MD5        = "dbbf55398d930e14c2b0a035dd1277b9"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0012.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "656002e6e620cb3b73c27f4c21d32edb"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0013.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "1244cc2c101c66e6bb4ad5183b356b19"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0017.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3200,
            SectorSize = 512,
            MD5        = "a817a56036f591a5cff11857b7d466be"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0018.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "439b2b76e154f3ce7e86bf1377282d5f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0019.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 90,
            SectorSize = 512,
            MD5        = "3c21d11e2b4ca108de3ec8ffface814d"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0020.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "c2e64e8a388b4401719f06d6a868dd1b"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0021.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3200,
            SectorSize = 512,
            MD5        = "6fc7f2233f094af7ae0d454668976858"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0022.IMD.lz",
            MediaType  = MediaType.ACORN_525_DS_DD,
            Sectors    = 2560,
            SectorSize = 256,
            MD5        = "ad6c3e6910457a53572695401efda4ab"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0023.IMD.lz",
            MediaType  = MediaType.ACORN_35_DS_HD,
            Sectors    = 1600,
            SectorSize = 1024,
            MD5        = "5e41fe3201ab32f25873faf8d3f79a02"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0024.IMD.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            MD5        = "2302991363cb3681cffdc4388915b51e",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2880
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0025.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            MD5        = "4e4cafed1cc22ea72201169427e5e1b6"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0026.IMD.lz",
            MediaType  = MediaType.ACORN_35_DS_DD,
            Sectors    = 800,
            SectorSize = 1024,
            MD5        = "a579b349a5a24218d59a44e36bdb1333"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0027.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 960,
            SectorSize = 1024,
            MD5        = "669b2155d5e4d7849d662729717a68d8"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0028.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            MD5        = "1a4c7487382c98b7bc74623ddfb488e6"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0029.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 960,
            SectorSize = 1024,
            MD5        = "23f5700ea3bfe076c88dd399a8026a1e"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0030.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            MD5        = "af83d011608042d35021e39aa5e10b2f",
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
            TestFile   = "DSKA0031.IMD.lz",
            MediaType  = MediaType.ACORN_525_SS_DD_40,
            Sectors    = 640,
            SectorSize = 256,
            MD5        = "e640835966327f3f662e1db8e0575510"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0032.IMD.lz",
            MediaType  = MediaType.ACORN_525_SS_DD_40,
            Sectors    = 640,
            SectorSize = 256,
            MD5        = "ff3534234d1d2dd88bf6e83be23d9227"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0033.IMD.lz",
            MediaType  = MediaType.ACORN_525_SS_DD_80,
            Sectors    = 1280,
            SectorSize = 256,
            MD5        = "dfaff34a6556b515642f1e54f839b02e"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0034.IMD.lz",
            MediaType  = MediaType.ACORN_525_SS_DD_80,
            Sectors    = 1280,
            SectorSize = 256,
            MD5        = "ca8f5c7f9ed161b03ccb166eb9d62146"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0035.IMD.lz",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            MD5        = "6642c1a32d2c58e93481d664974fc202",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 320
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0036.IMD.lz",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            MD5        = "6642c1a32d2c58e93481d664974fc202",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 320
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0037.IMD.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            MD5        = "5101f89850dc28efbcfb7622086a9ddf",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 360
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0038.IMD.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            MD5        = "8e570be2ed1f00ddea82e50a2d9c446a",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 360
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0039.IMD.lz",
            MediaType  = MediaType.DOS_35_SS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            MD5        = "abba2a1ddd60a649047a9c44d94bbeae",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 640
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0040.IMD.lz",
            MediaType  = MediaType.DOS_35_SS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            MD5        = "e3bc48bec81be5b35be73d41fdffd2ab",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 640
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0041.IMD.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            MD5        = "43b5068af9d016d1432eb2e12d2b802a",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 640
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0042.IMD.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            MD5        = "5bf2ad4dc300592604b6e32f8b8e2656",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 640
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0043.IMD.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            MD5        = "cb9a832ca6a4097b8ccc30d2108e1f7d",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 720
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0044.IMD.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            MD5        = "56d181a6bb8713e6b2854fe8887faab6",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 720
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0045.IMD.lz",
            MediaType  = MediaType.NEC_35_HD_15,
            Sectors    = 2400,
            SectorSize = 512,
            MD5        = "41aef7cff26aefda1add8d49c5b962c2",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2400
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0046.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2460,
            SectorSize = 512,
            MD5        = "2437c5f089f1cba3866b36360b016f16",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2460
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0047.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_8,
            Sectors    = 1280,
            SectorSize = 512,
            MD5        = "bdaa8f17373b265830fdf3a06b794367",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1280
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0048.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            MD5        = "629932c285478d0540ff7936aa008351",
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
            TestFile   = "DSKA0049.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1476,
            SectorSize = 512,
            MD5        = "7a2abef5d4701e2e49abb05af8d4da50",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1476
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0050.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "e3507522c914264f44fb2c92c3170c09",
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
            TestFile   = "DSKA0051.IMD.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            MD5        = "824fe65dbb1a42b6b94f05405ef984f2",
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
            TestFile   = "DSKA0052.IMD.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            MD5        = "1a8c2e78e7132cf9ba5d6c2b75876be0",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2880
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0053.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2952,
            SectorSize = 512,
            MD5        = "936b20bb0966fe693b4d5e2353e24846",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2952
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0054.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3200,
            SectorSize = 512,
            MD5        = "803b01a0b440c2837d37c21308f30cd5",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3200
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0055.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3280,
            SectorSize = 512,
            MD5        = "aa0d31f914760cc4cde75479779ebed6",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3280
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0057.IMD.lz",
            MediaType  = MediaType.FDFORMAT_35_HD,
            Sectors    = 3444,
            SectorSize = 512,
            MD5        = "5e413433c54f48978d281c6e66d1106e",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3444
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0058.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3486,
            SectorSize = 512,
            MD5        = "4fc28b0128543b2eb70f6432c4c8a980",
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
            TestFile   = "DSKA0059.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3528,
            SectorSize = 512,
            MD5        = "24a7459d080cea3a60d131b8fd7dc5d1",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3528
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0060.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3570,
            SectorSize = 512,
            MD5        = "2031b1e16ee2defc0d15f732f633df33",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3570
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0061.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 5100,
            SectorSize = 512,
            MD5        = "79e5f1fbd63b87c087d85904d45964e6",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 5100
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0063.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 6604,
            SectorSize = 512,
            MD5        = "1b2495a8f2274852b6fae80ae6fbff2f",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 6604
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0064.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 9180,
            SectorSize = 512,
            MD5        = "3a70851950ad06c20e3063ad6f128eef",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 9180
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0065.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 10710,
            SectorSize = 512,
            MD5        = "98a91bbdbe8454cf64e20d0ec5c35017",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 10710
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0066.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 10710,
            SectorSize = 512,
            MD5        = "666706f299a1362cb30f34a3a7f555be",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 10710
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0067.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 13770,
            SectorSize = 512,
            MD5        = "2fa1eedb57fac492d6f6b71e2c0a079c",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 13770
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0068.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            MD5        = "3152c8e3544bbfaceff14b7522faf5af",
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
            TestFile   = "DSKA0069.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            MD5        = "5fc19ca552b6db957061e9a1750394d2",
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
            TestFile   = "DSKA0070.IMD.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            MD5        = "d1e978b679c63a218c3f77a7ca2c7206"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0073.IMD.lz",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            MD5        = "a33b46f042b78fe3d0b3c5dbb3908a93",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 320
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0074.IMD.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            MD5        = "565d3c001cbb532154aa5d3c65b2439c",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 360
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0075.IMD.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            MD5        = "e60442c3ebd72c99bdd7545fdba59613",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 640
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0076.IMD.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            MD5        = "058a33a129539285c9b64010496af52f",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 720
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0077.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 800,
            SectorSize = 512,
            MD5        = "0726ecbc38965d30a6222c3e74cd1aa3",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 800
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0078.IMD.lz",
            MediaType  = MediaType.NEC_35_HD_15,
            Sectors    = 2400,
            SectorSize = 512,
            MD5        = "c9a193837db7d8a5eb025eb41e8a76d7"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0080.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            MD5        = "c38d69ac88520f14fcc6d6ced22b065d",
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
            TestFile   = "DSKA0081.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "91d51964e1e64ef3f6f622fa19aa833c",
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
            TestFile   = "DSKA0082.IMD.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            MD5        = "db36d9651c952ff679ec33223c8db2d3",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2880
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0083.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2988,
            SectorSize = 512,
            MD5        = "5f1d98806309aee7f81de72e51e6d386",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2988
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0084.IMD.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            MD5        = "1207a1cc7ff73d4f74c8984b4e7db33f",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3360
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0085.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3486,
            SectorSize = 512,
            MD5        = "c97a3081fd25474b6b7945b8572d5ab8",
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
            TestFile   = "DSKA0086.IMD.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            MD5        = "31269ed6464302ae26d22b7c87bceb23",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3360
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0089.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 664,
            SectorSize = 512,
            MD5        = "8b31e5865611dbe01cc25b5ba2fbdf25"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0090.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 670,
            SectorSize = 2048,
            MD5        = "be278c00c3ec906756e7c8d544d8833d"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0091.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 824,
            SectorSize = 1024,
            MD5        = "8e7fb60151e0002e8bae2fb2abe13a69"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0092.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 824,
            SectorSize = 2048,
            MD5        = "45e0b2a2925a95bbdcb43a914d70f91b"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0093.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1483,
            SectorSize = 1024,
            MD5        = "082d7eda62eead1e20fd5a060997ff0f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0094.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 995,
            SectorSize = 2048,
            MD5        = "9b75a2fb671d1e7fa27434038b375e5e"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0097.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1812,
            SectorSize = 1024,
            MD5        = "97c4f895d64ba196f19a3179e68ef693"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0098.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1160,
            SectorSize = 2048,
            MD5        = "c838233a380973de386e66ee0e0cbcc2"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0099.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 164,
            SectorSize = 16384,
            MD5        = "dea88f91ca0f6d90626b4029286cb01f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0101.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 164,
            SectorSize = 16384,
            MD5        = "db82b15389e2ffa9a20f7251cc5cce5b"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0103.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 164,
            SectorSize = 16384,
            MD5        = "638b56d7061a8156ee87166c78f06111"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0105.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 400,
            SectorSize = 512,
            MD5        = "d40a99cb549fcfb26fcf9ef01b5dfca7",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 400
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0106.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 410,
            SectorSize = 512,
            MD5        = "7b41dd9ca7eb32828960eb1417a6092a",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 410
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0107.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 800,
            SectorSize = 512,
            MD5        = "126dfd25363c076727dfaab03955c931",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 800
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0108.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 820,
            SectorSize = 512,
            MD5        = "e6492aac144f5f6f593b84c64680cf64",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 820
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0109.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "7973e569ed93beb1ece2e84a5ef3a8d1",
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
            TestFile   = "DSKA0110.IMD.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            MD5        = "a793047503af08e83361427b3e2806e0",
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
            TestFile   = "DSKA0111.IMD.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            MD5        = "f01541de322c8d6d7321084d7a245e7b",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2880
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0112.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2952,
            SectorSize = 512,
            MD5        = "ba6ec1652ff41bcc687aaf9c4e32dc18",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2952
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0113.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "7973e569ed93beb1ece2e84a5ef3a8d1",
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
            TestFile   = "DSKA0114.IMD.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            MD5        = "a793047503af08e83361427b3e2806e0",
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
            TestFile   = "DSKA0115.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2952,
            SectorSize = 512,
            MD5        = "ba6ec1652ff41bcc687aaf9c4e32dc18",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2952
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0116.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3200,
            SectorSize = 512,
            MD5        = "6631b66fdfd89319323771c41334c7ba",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3200
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0117.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3240,
            SectorSize = 512,
            MD5        = "4b5e2c9599bb7861b3b52bec00d81278"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0120.IMD.lz",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            MD5        = "7d36aee5a3071ff75b979f3acb649c40",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 320
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0121.IMD.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            MD5        = "0ccb62039363ab544c69eca229a17fae"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0122.IMD.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            MD5        = "7851d31fad9302ff45d3ded4fba25387",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 640
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0123.IMD.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            MD5        = "915b08c82591e8488320e001b7303b6d",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 720
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0124.IMD.lz",
            MediaType  = MediaType.NEC_35_HD_15,
            Sectors    = 2400,
            SectorSize = 512,
            MD5        = "5e5ea6fe9adf842221fdc60e56630405",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2400
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0125.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            MD5        = "a22e254f7e3526ec30dc4915a19fcb52",
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
            TestFile   = "DSKA0126.IMD.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            MD5        = "ddc6c1200c60e9f7796280f50c2e5283",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2880
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0147.IMD.lz",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            MD5        = "6efa72a33021d5051546c3e0dd4c3c09"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0148.IMD.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            MD5        = "8b5acfd14818ff9556d3d81361ce4862"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0149.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 200,
            SectorSize = 1024,
            MD5        = "cf42d08469548a31caf2649a1d08a85f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0150.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 400,
            SectorSize = 1024,
            MD5        = "62745e10683cf2ec1dac177535459891"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0151.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 400,
            SectorSize = 512,
            MD5        = "cf42d08469548a31caf2649a1d08a85f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0153.IMD.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            MD5        = "298c377de52947c472a85d281b6d3d4d"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0154.IMD.lz",
            MediaType  = MediaType.RX50,
            Sectors    = 800,
            SectorSize = 512,
            MD5        = "387373301cf6c15d61eec9bab18d9b6a"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0155.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 848,
            SectorSize = 512,
            MD5        = "83b66a88d92cbf2715343016e4108211"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0157.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            MD5        = "20e047061b6ca4059288deed8c9dd247"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0158.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1280,
            SectorSize = 256,
            MD5        = "8b5acfd14818ff9556d3d81361ce4862"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0159.IMD.lz",
            MediaType  = MediaType.ACORN_525_SS_DD_40,
            Sectors    = 640,
            SectorSize = 256,
            MD5        = "6efa72a33021d5051546c3e0dd4c3c09"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0160.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1280,
            SectorSize = 256,
            MD5        = "8b5acfd14818ff9556d3d81361ce4862"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0162.IMD.lz",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            MD5        = "e63014a4299f52f22e6e2c9609f51979"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0163.IMD.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            MD5        = "be05d1ff10ef8b2220546c4db962ac9e",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 720
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0164.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 820,
            SectorSize = 512,
            MD5        = "32823b9009c99b6711e89336ad03ec7f",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 820
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0166.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            MD5        = "1c8b03a8550ed3e70e1c78316aa445aa",
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
            TestFile   = "DSKA0167.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 960,
            SectorSize = 1024,
            MD5        = "efbc62e2ecddc15241aa0779e078d478"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0168.IMD.lz",
            MediaType  = MediaType.NEC_35_HD_15,
            Sectors    = 2400,
            SectorSize = 512,
            MD5        = "0bdf9130c07bb5d558a4705249f949d0",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2400
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0169.IMD.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            MD5        = "2dafeddaa99e7dc0db5ef69e128f9c8e",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2880
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0170.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2952,
            SectorSize = 512,
            MD5        = "589ae671a19e78ffcba5032092c4c0d5",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2952
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0171.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2988,
            SectorSize = 512,
            MD5        = "cf0c71b65b56cb6b617d29525bd719dd",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2988
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0173.IMD.lz",
            MediaType  = MediaType.DOS_35_SS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            MD5        = "028769dc0abefab1740cc309432588b6",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 720
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0174.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            MD5        = "152023525154b45ab26687190bac94db",
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
            TestFile   = "DSKA0175.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "db38ecd93f28dd065927fed21917eed5",
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
            TestFile   = "DSKA0176.IMD.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            MD5        = "716262401bc69f2f440a9c156c21c9e9",
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
            TestFile   = "DSKA0177.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1660,
            SectorSize = 512,
            MD5        = "83213865ca6a40c289b22324a32a2608",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1660
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0180.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3200,
            SectorSize = 512,
            MD5        = "f206c0caa4e0eda37233ab6e89ab5493",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3200
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0181.IMD.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            MD5        = "554492a7b41f4cd9068a3a2b70eb0e5f",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3360
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0182.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3402,
            SectorSize = 512,
            MD5        = "865ad9072cb6c7458f7d86d7e9368622"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0183.IMD.lz",
            MediaType  = MediaType.FDFORMAT_35_HD,
            Sectors    = 3444,
            SectorSize = 512,
            MD5        = "2461e458438f0033bc5811fd6958ad02"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0184.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1760,
            SectorSize = 1024,
            MD5        = "be75996696aa70ee9338297137556d83"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0185.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1120,
            SectorSize = 2048,
            MD5        = "5a0f2bad567464288ec7ce935672870a"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0186.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 320,
            SectorSize = 4096,
            MD5        = "69f9f0b5c1fc00a8f398151df9d93ab5"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0191.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 626,
            SectorSize = 1024,
            MD5        = "fb144f79239f6f5f113b417700c2d278"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0192.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 670,
            SectorSize = 2048,
            MD5        = "6a936d2ecb771e37b856bdad16822c32"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0194.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 356,
            SectorSize = 4096,
            MD5        = "e283af9d280efaf059c816b6a2c9206b"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0196.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 960,
            SectorSize = 1024,
            MD5        = "e4625838148a4b7c6580c697cd47362c"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0197.IMD.lz",
            MediaType  = MediaType.ACORN_525_SS_DD_40,
            Sectors    = 640,
            SectorSize = 256,
            MD5        = "74f71ef3978fefce64689e8be18359ba"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0198.IMD.lz",
            MediaType  = MediaType.ACORN_525_SS_DD_80,
            Sectors    = 1280,
            SectorSize = 256,
            MD5        = "5c4e555b29a264f2a81f8a2b58bfc442"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0199.IMD.lz",
            MediaType  = MediaType.ACORN_525_DS_DD,
            Sectors    = 2560,
            SectorSize = 256,
            MD5        = "64ae73ac812bbf473a5d443de4d5dfbf"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0200.IMD.lz",
            MediaType  = MediaType.ACORN_35_DS_HD,
            Sectors    = 1600,
            SectorSize = 1024,
            MD5        = "a481bd5a8281dad089edbef390c136ed"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0201.IMD.lz",
            MediaType  = MediaType.ACORN_35_DS_DD,
            Sectors    = 800,
            SectorSize = 1024,
            MD5        = "85574aebeef03eb355bf8541955d06ea"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0202.IMD.lz",
            MediaType  = MediaType.ACORN_35_DS_HD,
            Sectors    = 1600,
            SectorSize = 1024,
            MD5        = "a481bd5a8281dad089edbef390c136ed"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0203.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1280,
            SectorSize = 1024,
            MD5        = "8a16a3008739516fc3ba4c878868d056"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0204.IMD.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            MD5        = "46fce47baf08c6f093f2c355a603543d"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0205.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1476,
            SectorSize = 512,
            MD5        = "ee73a5d5c8dfac236baf7b99811696f9"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0206.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "b3bdbc62fb96e3893dac3bccbde59ab0"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0207.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3200,
            SectorSize = 512,
            MD5        = "02942b9dc9d3b1bc9335b73c99e6da2e"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0208.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 480,
            SectorSize = 1024,
            MD5        = "dfc9e8c7bd3d50f404d6f0b6ada20b0c"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0209.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "85574aebeef03eb355bf8541955d06ea"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0210.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "85574aebeef03eb355bf8541955d06ea"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0211.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            MD5        = "647f14749f59be471aac04a71a079a64"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0212.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            MD5        = "517cdd5e42a4673f733d1aedfb46770f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0213.IMD.lz",
            MediaType  = MediaType.ACORN_35_DS_DD,
            Sectors    = 800,
            SectorSize = 1024,
            MD5        = "6ad92e9522e4ba902c01beecb5943bb1"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0214.IMD.lz",
            MediaType  = MediaType.ACORN_35_DS_HD,
            Sectors    = 1600,
            SectorSize = 1024,
            MD5        = "9a1a7d8f53fcfad7603fe585c6c7214c"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0215.IMD.lz",
            MediaType  = MediaType.ACORN_35_DS_HD,
            Sectors    = 1600,
            SectorSize = 1024,
            MD5        = "2a7a9b48551fd4d8b166bcfcbe1ca132"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0216.IMD.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            MD5        = "40199611e6e75bbc37ad6c52a5b77eae"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0218.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 5100,
            SectorSize = 512,
            MD5        = "8fa0ffd7481a94b9e7c4006599329250"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0219.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 9180,
            SectorSize = 512,
            MD5        = "3fa51592c5a65b7e4915a8e22d523ced"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0220.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 13770,
            SectorSize = 512,
            MD5        = "2153339750c119627bab75bd0bf7a193"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0221.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 5120,
            SectorSize = 256,
            MD5        = "f92b2e52259531d50bfb403dc1274ab1"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0222.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "85574aebeef03eb355bf8541955d06ea"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0223.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1600,
            SectorSize = 256,
            MD5        = "a5dc382d75ec46434b313e289c281d8c"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0224.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1152,
            SectorSize = 256,
            MD5        = "8335b175c352352e19f9008ad67d1375"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0225.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1056,
            SectorSize = 256,
            MD5        = "447efa963c19474508c503d037a3b429"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0226.IMD.lz",
            MediaType  = MediaType.ACORN_525_DS_DD,
            Sectors    = 2560,
            SectorSize = 256,
            MD5        = "b7669fa76ecf5634313675b001bb7fa2"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0227.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 5120,
            SectorSize = 256,
            MD5        = "676f1bc7764899912ab6ad8257c63a16"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0228.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1120,
            SectorSize = 256,
            MD5        = "d72e86324d4d518996f6671751614800"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0232.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 621,
            SectorSize = 512,
            MD5        = "b76bd117ce24d933cdefe09b1de2164a"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0233.IMD.lz",
            MediaType  = MediaType.ATARI_525_SD,
            Sectors    = 720,
            SectorSize = 128,
            MD5        = "a769b7642a222d97a56c46f53833fafa"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0234.IMD.lz",
            MediaType  = MediaType.ACORN_525_DS_DD,
            Sectors    = 2560,
            SectorSize = 256,
            MD5        = "dfa733d034bb1f83d694dfa217910081"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0235.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1600,
            SectorSize = 256,
            MD5        = "8260ee01a245aec2de162ee0d85f4b7f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0236.IMD.lz",
            MediaType  = MediaType.ACORN_525_SS_SD_80,
            Sectors    = 800,
            SectorSize = 256,
            MD5        = "261c7a5a4298e9f050928dd770097c77"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0238.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "a47068ff73dfbea58c25daa5b9132a9e"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0240.IMD.lz",
            MediaType  = MediaType.ATARI_525_DD,
            Sectors    = 720,
            SectorSize = 256,
            MD5        = "d1ab955f0961ab94e6cf69f78134a84b"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0241.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 714,
            SectorSize = 256,
            MD5        = "8b62738f15bcc916a668eaa67eec86e7"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0242.IMD.lz",
            MediaType  = MediaType.NEC_35_HD_8,
            Sectors    = 1232,
            SectorSize = 1024,
            MD5        = "87a432496cb23b5c2299545500df3553",
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
            TestFile   = "DSKA0243.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1280,
            SectorSize = 256,
            MD5        = "9866ab8e58fa4be25010184aec4ad3aa"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0244.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1280,
            SectorSize = 256,
            MD5        = "9dab329ae098b29889ab08278de38f95"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0245.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "0d71b4952dadbfb1061acc1f4640c787"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0246.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "af7ac6b5b9d2d57dad22dbb64ef7de38"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0247.IMD.lz",
            MediaType  = MediaType.ACORN_525_SS_DD_80,
            Sectors    = 1280,
            SectorSize = 256,
            MD5        = "f8f81f945aaad6fbfe7e2db1905302c1"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0248.IMD.lz",
            MediaType  = MediaType.ACORN_525_SS_DD_80,
            Sectors    = 1280,
            SectorSize = 256,
            MD5        = "f6f81c75b5ba45d91c1886c6dda9caee"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0250.IMD.lz",
            MediaType  = MediaType.ACORN_35_DS_DD,
            Sectors    = 800,
            SectorSize = 1024,
            MD5        = "d4809467b321991a9c772ad87fc8aa19"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0251.IMD.lz",
            MediaType  = MediaType.ACORN_525_DS_DD,
            Sectors    = 2560,
            SectorSize = 256,
            MD5        = "d075e50705f4ddca7ba4dbc981ec1176"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0252.IMD.lz",
            MediaType  = MediaType.ACORN_525_SS_DD_80,
            Sectors    = 1280,
            SectorSize = 256,
            MD5        = "9f86480c86bae33a5b444e4a7ed55048"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0253.IMD.lz",
            MediaType  = MediaType.ACORN_35_DS_DD,
            Sectors    = 800,
            SectorSize = 1024,
            MD5        = "629971775d902d1cc2658fc76f57e072"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0254.IMD.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            MD5        = "5dc0d482a773043d8683a84c8220df95",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 360
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0255.IMD.lz",
            MediaType  = MediaType.ACORN_525_DS_DD,
            Sectors    = 2544,
            SectorSize = 256,
            MD5        = "1718d8acd18fce3c5c1a7a074ed8ac29"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0258.IMD.lz",
            MediaType  = MediaType.NEC_35_HD_8,
            Sectors    = 1232,
            SectorSize = 1024,
            MD5        = "855943f9caecdcce9b06f0098d773c6b",
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
            TestFile   = "DSKA0262.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            MD5        = "5ac0a9fc7337f761098f816359b0f6f7",
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
            TestFile   = "DSKA0263.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "1ea6ec8e663218b1372048f6e25795b5",
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
            TestFile   = "DSKA0264.IMD.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            MD5        = "77a1167b1b9043496e32b8578cde0ff0"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0265.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1660,
            SectorSize = 512,
            MD5        = "4b07d760d65f3f0f8ffa5f2b81cee907",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1660
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0266.IMD.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            MD5        = "32c044c5c2b0bd13806149a759c14935",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2880
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0267.IMD.lz",
            MediaType  = MediaType.XDF_525,
            Sectors    = 3040,
            SectorSize = 512,
            MD5        = "8752095abc13dba3f3467669da333891",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3040
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0268.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3200,
            SectorSize = 512,
            MD5        = "aece7cd34bbba3e75307fa70404d9d30",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3200
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0269.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3280,
            SectorSize = 512,
            MD5        = "5289afb16a6e4a33213e3bcca56c6230",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3280
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0270.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3320,
            SectorSize = 512,
            MD5        = "1aef0a0ba233476db6567878c3c2b266"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0271.IMD.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            MD5        = "b96596711f4d2ee85dfda0fe3b9f26c3",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3360
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0272.IMD.lz",
            MediaType  = MediaType.FDFORMAT_35_HD,
            Sectors    = 3444,
            SectorSize = 512,
            MD5        = "a4f461af7fda5e93a7ab63fcbb7e7683"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0273.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3486,
            SectorSize = 512,
            MD5        = "8f7f7099d4475f6631fcf0a79b031d61",
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
            TestFile   = "DSKA0280.IMD.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            MD5        = "4feeaf4b4ee5dad85db727fbbda4b6d1",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 360
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0281.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 400,
            SectorSize = 512,
            MD5        = "3c77ca681df78e4cd7baa162aa9b0859",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 400
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0282.IMD.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            MD5        = "51da1f86c49657ffdb367bb2ddeb7990",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 640
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0283.IMD.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            MD5        = "b81a4987f89936630b8ebc62e4bbce6e"
            /* TODO: IndexOutOfRangeException
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 720
                }
            }
            */
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0284.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 800,
            SectorSize = 512,
            MD5        = "f76f92dd326c99c5efad5ee58daf72e1",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 800
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0285.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 840,
            SectorSize = 512,
            MD5        = "b6f2c10e42908e334025bc4ffd81e771",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 840
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0287.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            MD5        = "f2f409ea2a62a7866fd2777cc4fc9739",
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
            TestFile   = "DSKA0288.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1494,
            SectorSize = 512,
            MD5        = "3e441d69cec5c3169274e1379de4af4b",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1494
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0289.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "30a93f30dd4485c6fc037fe0775d3fc7",
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
            TestFile   = "DSKA0290.IMD.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            MD5        = "e0caf02cce5597c98313bcc480366ec7",
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
            TestFile   = "DSKA0291.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1660,
            SectorSize = 512,
            MD5        = "4af4904d2b3c815da7bef7049209f5eb",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1660
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0299.IMD.lz",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            MD5        = "39bf5a98bcb2185d855ac06378febcfa",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 320
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0300.IMD.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            MD5        = "dc20055b6e6fd6f8e1114d4be2effeed",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 360
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0301.IMD.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            MD5        = "56af9256cf71d5aac5fd5d363674bc49"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0302.IMD.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            MD5        = "bbba1e2d1418e05c3a4e7b4d585d160b",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 720
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0303.IMD.lz",
            MediaType  = MediaType.NEC_35_HD_15,
            Sectors    = 2400,
            SectorSize = 512,
            MD5        = "bca3a045e81617f7f5ebb5a8818eac47",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2400
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0304.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            MD5        = "a296663cb8e75e94603221352f29cfff",
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
            TestFile   = "DSKA0305.IMD.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            MD5        = "ecda36ebf0e1100233cb0ec722c18583",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2880
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0307.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 840,
            SectorSize = 512,
            MD5        = "cef2f4fe9b1a32d5c0544f814e634264",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 840
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0308.IMD.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "bbe58e26b8f8f822cd3edfd37a4e4924"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0311.IMD.lz",
            MediaType  = MediaType.FDFORMAT_35_HD,
            Sectors    = 3444,
            SectorSize = 512,
            MD5        = "b9b6ebdf711364c979de7cf70c3a438a"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0314.IMD.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            MD5        = "d37424f367f545acbb397f2bed766843"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0316.IMD.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            MD5        = "9963dd6f19ce6bd56eabeccdfbbd821a",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2880
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0317.IMD.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            MD5        = "acf6604559ae8217f7869823e2429024",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3360
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0318.IMD.lz",
            MediaType  = MediaType.FDFORMAT_35_HD,
            Sectors    = 3444,
            SectorSize = 512,
            MD5        = "23bf2139cdfdc4c16db058fd31ea6481",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3444
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0319.IMD.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            MD5        = "fa26adda0415f02057b113ad29c80c8d",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3360
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0320.IMD.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            MD5        = "4f2a8d036fefd6c6c88d99eda3aa12b7",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3360
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0322.IMD.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1386,
            SectorSize = 512,
            MD5        = "e794a3ffa4069ea999fdf7146710fa9e",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1386
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "md1dd_rx01.imd.lz",
            MediaType  = MediaType.RX01,
            Sectors    = 2002,
            SectorSize = 128,
            MD5        = "5b4e36d92b180c3845387391cb5a1c64"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md1qd_rx50.imd.lz",
            MediaType  = MediaType.RX50,
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
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2hd_nec.imd.lz",
            MediaType  = MediaType.NEC_35_HD_8,
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
            TestFile   = "mf2dd_2mgui.imd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 164,
            SectorSize = 16384,
            MD5        = "623b224f63d65ae3b6c3ddadadf3b836"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_2m.imd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 987,
            SectorSize = 1024,
            MD5        = "08b530d8c25d785b20c93a1a7a6468a0"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_fdformat_800.imd.lz",
            MediaType  = MediaType.CBM_35_DD,
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
            TestFile   = "mf2dd_fdformat_820.imd.lz",
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
            TestFile   = "mf2dd_freedos.imd.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "1ff7649b679ba22ff20d39ff717dbec8",
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
            TestFile   = "mf2dd.imd.lz",
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
            TestFile   = "mf2hd_2mgui.imd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 164,
            SectorSize = 16384,
            MD5        = "adafed1fac3d1a181380bdb590249385"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_2m.imd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1812,
            SectorSize = 1024,
            MD5        = "c741c78eecd673f8fc49e77459871940"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_fdformat_168.imd.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            MD5        = "03c2af6a8ebf4bd6f530335de34ae5dd",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3360
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_fdformat_172.imd.lz",
            MediaType  = MediaType.FDFORMAT_35_HD,
            Sectors    = 3444,
            SectorSize = 512,
            MD5        = "9dea1e119a73a21a38d134f36b2e5564"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_freedos.imd.lz",
            MediaType  = MediaType.Unknown,
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
            TestFile   = "mf2hd.imd.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            MD5        = "b4a602f67903c46eef62addb0780aa56",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2880
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_xdf.imd.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 670,
            SectorSize = 2048,
            MD5        = "71194f8dba31d29780bd0a6ecee5ab2b"
        }
    };
}