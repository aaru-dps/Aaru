// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : RayDIM.cs
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

using System.Diagnostics.CodeAnalysis;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using NUnit.Framework;

namespace Aaru.Tests.Images;

[TestFixture, SuppressMessage("ReSharper", "InconsistentNaming")]
public class RayDIM : BlockMediaImageTest
{
    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Media image formats", "Disk IMage Archiver");
    public override IMediaImage Plugin => new RayDim();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "5f1dd8.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 336,
            SectorSize = 512,
            Md5        = "c109e802e65365245dedd1737ec65c92"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5f1dd8_pass.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 336,
            SectorSize = 512,
            Md5        = "d6eb723ac53eb469f64d8df69efef3dd",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 336
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "5f1dd.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 378,
            SectorSize = 512,
            Md5        = "a327c34060570e1a917eb1d88716a11a"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5f1dd_pass.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 378,
            SectorSize = 512,
            Md5        = "b9807f1c25bf472633e7e80fa947a4d1",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 378
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "5f2dd8.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 672,
            SectorSize = 512,
            Md5        = "8b9e6662ef25a08d167f7ec4436efac8"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5f2dd8_pass.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 672,
            SectorSize = 512,
            Md5        = "532694cde41f1553587b65c528bc185b",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 672
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "5f2dd.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 756,
            SectorSize = 512,
            Md5        = "a0b2aa16acaab9f521dff74ba93485ae"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5f2dd_pass.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 756,
            SectorSize = 512,
            Md5        = "934e3a0f07410d0f4750f2beb3ce48f1",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 756
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "5f2hd.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2460,
            SectorSize = 512,
            Md5        = "78819708381987b3120fc777a5f08f2d"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5f2hd_pass.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2460,
            SectorSize = 512,
            Md5        = "37dbeabaf72384870284ccd102b85eb7",
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
            TestFile   = "DSKA0000.DIM.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "e8bbbd22db87181974e12ba0227ea011"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0001.DIM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "9f5635f3df4d880a500910b0ad1ab535"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0009.DIM.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "95ea232f59e44db374b994cfe7f1c07f",
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
            TestFile   = "DSKA0010.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "9e2b01f4397db2a6c76e2bc267df37b3"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0012.DIM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "656002e6e620cb3b73c27f4c21d32edb"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0013.DIM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "1244cc2c101c66e6bb4ad5183b356b19"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0017.DIM.lz",
            MediaType  = MediaType.XDF_525,
            Sectors    = 3040,
            SectorSize = 512,
            Md5        = "8cad624afc06ab756f9800eba22ee886"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0018.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "84cce7b4d8c8e21040163cd2d03a730c"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0020.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "d236783dfd1dc29f350c51949b1e9e68"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0021.DIM.lz",
            MediaType  = MediaType.XDF_525,
            Sectors    = 3040,
            SectorSize = 512,
            Md5        = "6915f208cdda762eea2fe64ad754e72f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0024.DIM.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "2302991363cb3681cffdc4388915b51e",
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
            TestFile   = "DSKA0025.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "4e4cafed1cc22ea72201169427e5e1b6"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0028.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "1a4c7487382c98b7bc74623ddfb488e6"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0030.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "af83d011608042d35021e39aa5e10b2f",
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
            TestFile   = "DSKA0035.DIM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            Md5        = "6642c1a32d2c58e93481d664974fc202",
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
            TestFile   = "DSKA0036.DIM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            Md5        = "846f01b8b60cb3c775bd66419e977926",
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
            TestFile   = "DSKA0037.DIM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "5101f89850dc28efbcfb7622086a9ddf",
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
            TestFile   = "DSKA0038.DIM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "8e570be2ed1f00ddea82e50a2d9c446a",
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
            TestFile   = "DSKA0039.DIM.lz",
            MediaType  = MediaType.DOS_35_SS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "abba2a1ddd60a649047a9c44d94bbeae",
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
            TestFile   = "DSKA0040.DIM.lz",
            MediaType  = MediaType.DOS_35_SS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "e3bc48bec81be5b35be73d41fdffd2ab",
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
            TestFile   = "DSKA0041.DIM.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "43b5068af9d016d1432eb2e12d2b802a",
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
            TestFile   = "DSKA0042.DIM.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "5bf2ad4dc300592604b6e32f8b8e2656",
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
            TestFile   = "DSKA0043.DIM.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "cb9a832ca6a4097b8ccc30d2108e1f7d",
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
            TestFile   = "DSKA0044.DIM.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "56d181a6bb8713e6b2854fe8887faab6",
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
            TestFile   = "DSKA0045.DIM.lz",
            MediaType  = MediaType.DOS_525_HD,
            Sectors    = 2400,
            SectorSize = 512,
            Md5        = "41aef7cff26aefda1add8d49c5b962c2",
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
            TestFile   = "DSKA0046.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2460,
            SectorSize = 512,
            Md5        = "2437c5f089f1cba3866b36360b016f16",
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
            TestFile   = "DSKA0047.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_8,
            Sectors    = 1280,
            SectorSize = 512,
            Md5        = "bdaa8f17373b265830fdf3a06b794367",
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
            TestFile   = "DSKA0048.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "629932c285478d0540ff7936aa008351",
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
            TestFile   = "DSKA0049.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1476,
            SectorSize = 512,
            Md5        = "7a2abef5d4701e2e49abb05af8d4da50",
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
            TestFile   = "DSKA0050.DIM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "e3507522c914264f44fb2c92c3170c09",
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
            TestFile   = "DSKA0051.DIM.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            Md5        = "824fe65dbb1a42b6b94f05405ef984f2",
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
            TestFile   = "DSKA0052.DIM.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "1a8c2e78e7132cf9ba5d6c2b75876be0",
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
            TestFile   = "DSKA0053.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2952,
            SectorSize = 512,
            Md5        = "936b20bb0966fe693b4d5e2353e24846",
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
            TestFile   = "DSKA0054.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3200,
            SectorSize = 512,
            Md5        = "803b01a0b440c2837d37c21308f30cd5",
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
            TestFile   = "DSKA0055.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3280,
            SectorSize = 512,
            Md5        = "aa0d31f914760cc4cde75479779ebed6",
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
            TestFile   = "DSKA0056.DIM.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            Md5        = "31269ed6464302ae26d22b7c87bceb23",
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
            TestFile   = "DSKA0057.DIM.lz",
            MediaType  = MediaType.FDFORMAT_35_HD,
            Sectors    = 3444,
            SectorSize = 512,
            Md5        = "5e413433c54f48978d281c6e66d1106e",
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
            TestFile   = "DSKA0058.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3528,
            SectorSize = 512,
            Md5        = "a7688d6be942272ce866736e6007bc46",
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
            TestFile   = "DSKA0059.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3528,
            SectorSize = 512,
            Md5        = "24a7459d080cea3a60d131b8fd7dc5d1",
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
            TestFile   = "DSKA0060.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3612,
            SectorSize = 512,
            Md5        = "ef0c3da4749da2f79d7d623d9b6f3d4d",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3612
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0061.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 5120,
            SectorSize = 512,
            Md5        = "5231d2e8a99ba5f8dfd16ca1a05f40cd",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 5120
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0068.DIM.lz",
            MediaType  = MediaType.DOS_35_SS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "8f91482c56161ecbf5d86f42b03b9636",
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
            TestFile   = "DSKA0069.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "5fc19ca552b6db957061e9a1750394d2",
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
            TestFile   = "DSKA0073.DIM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            Md5        = "a33b46f042b78fe3d0b3c5dbb3908a93",
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
            TestFile   = "DSKA0074.DIM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "565d3c001cbb532154aa5d3c65b2439c",
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
            TestFile   = "DSKA0075.DIM.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "e60442c3ebd72c99bdd7545fdba59613",
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
            TestFile   = "DSKA0076.DIM.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "058a33a129539285c9b64010496af52f",
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
            TestFile   = "DSKA0077.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 800,
            SectorSize = 512,
            Md5        = "0726ecbc38965d30a6222c3e74cd1aa3",
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
            TestFile   = "DSKA0078.DIM.lz",
            MediaType  = MediaType.DOS_525_HD,
            Sectors    = 2400,
            SectorSize = 512,
            Md5        = "c9a193837db7d8a5eb025eb41e8a76d7"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0080.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "c38d69ac88520f14fcc6d6ced22b065d",
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
            TestFile   = "DSKA0081.DIM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "91d51964e1e64ef3f6f622fa19aa833c",
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
            TestFile   = "DSKA0082.DIM.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "db36d9651c952ff679ec33223c8db2d3",
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
            TestFile   = "DSKA0083.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3024,
            SectorSize = 512,
            Md5        = "952f33314fb930c2d02ef4604585c0e6",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3024
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0084.DIM.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            Md5        = "1207a1cc7ff73d4f74c8984b4e7db33f",
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
            TestFile   = "DSKA0085.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3528,
            SectorSize = 512,
            Md5        = "53dfcaceed8203ee629fc7fe520e1217",
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
            TestFile   = "DSKA0105.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 400,
            SectorSize = 512,
            Md5        = "d40a99cb549fcfb26fcf9ef01b5dfca7",
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
            TestFile   = "DSKA0106.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 420,
            SectorSize = 512,
            Md5        = "6433f8fbf8dda1e307b15a4203c1a4e6",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 420
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0107.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 800,
            SectorSize = 512,
            Md5        = "126dfd25363c076727dfaab03955c931",
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
            TestFile   = "DSKA0108.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 840,
            SectorSize = 512,
            Md5        = "386763ae9afde1a0a19eb4a54ba462aa",
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
            TestFile   = "DSKA0109.DIM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "7973e569ed93beb1ece2e84a5ef3a8d1",
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
            TestFile   = "DSKA0110.DIM.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            Md5        = "a793047503af08e83361427b3e2806e0",
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
            TestFile   = "DSKA0111.DIM.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "f01541de322c8d6d7321084d7a245e7b",
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
            TestFile   = "DSKA0112.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2952,
            SectorSize = 512,
            Md5        = "ba6ec1652ff41bcc687aaf9c4e32dc18",
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
            TestFile   = "DSKA0113.DIM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "7973e569ed93beb1ece2e84a5ef3a8d1",
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
            TestFile   = "DSKA0114.DIM.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            Md5        = "a793047503af08e83361427b3e2806e0",
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
            TestFile   = "DSKA0115.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2952,
            SectorSize = 512,
            Md5        = "ba6ec1652ff41bcc687aaf9c4e32dc18",
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
            TestFile   = "DSKA0116.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3200,
            SectorSize = 512,
            Md5        = "6631b66fdfd89319323771c41334c7ba",
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
            TestFile   = "DSKA0117.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3280,
            SectorSize = 512,
            Md5        = "56471a253f4d6803b634e2bbff6c0931",
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
            TestFile   = "DSKA0120.DIM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            Md5        = "7d36aee5a3071ff75b979f3acb649c40",
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
            TestFile   = "DSKA0121.DIM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "0ccb62039363ab544c69eca229a17fae",
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
            TestFile   = "DSKA0122.DIM.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "7851d31fad9302ff45d3ded4fba25387",
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
            TestFile   = "DSKA0123.DIM.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "915b08c82591e8488320e001b7303b6d",
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
            TestFile   = "DSKA0124.DIM.lz",
            MediaType  = MediaType.DOS_525_HD,
            Sectors    = 2400,
            SectorSize = 512,
            Md5        = "5e5ea6fe9adf842221fdc60e56630405",
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
            TestFile   = "DSKA0125.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "a22e254f7e3526ec30dc4915a19fcb52",
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
            TestFile   = "DSKA0126.DIM.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "ddc6c1200c60e9f7796280f50c2e5283",
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
            TestFile   = "DSKA0147.DIM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            Md5        = "6efa72a33021d5051546c3e0dd4c3c09"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0148.DIM.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "8b5acfd14818ff9556d3d81361ce4862"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0151.DIM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "298c377de52947c472a85d281b6d3d4d"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0153.DIM.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "32975e1a2d10a360331de84682371277"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0154.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 800,
            SectorSize = 512,
            Md5        = "a5dc382d75ec46434b313e289c281d8c"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0157.DIM.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "3a7f25fa38019109e89051993076063a"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0162.DIM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            Md5        = "e63014a4299f52f22e6e2c9609f51979"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0163.DIM.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "be05d1ff10ef8b2220546c4db962ac9e",
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
            TestFile   = "DSKA0164.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 840,
            SectorSize = 512,
            Md5        = "e01d813dd6c3a49428520df40d63cadd",
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
            TestFile   = "DSKA0166.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "1c8b03a8550ed3e70e1c78316aa445aa",
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
            TestFile   = "DSKA0168.DIM.lz",
            MediaType  = MediaType.DOS_525_HD,
            Sectors    = 2400,
            SectorSize = 512,
            Md5        = "0bdf9130c07bb5d558a4705249f949d0",
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
            TestFile   = "DSKA0169.DIM.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "2dafeddaa99e7dc0db5ef69e128f9c8e",
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
            TestFile   = "DSKA0170.DIM.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "0c043ceba489ef80c1b7f58534af12f5"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0173.DIM.lz",
            MediaType  = MediaType.DOS_35_SS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "028769dc0abefab1740cc309432588b6",
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
            TestFile   = "DSKA0174.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "152023525154b45ab26687190bac94db",
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
            TestFile   = "DSKA0175.DIM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "db38ecd93f28dd065927fed21917eed5",
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
            TestFile   = "DSKA0176.DIM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "ca53f9cc4dcd04d06f5c4c3df09195ab"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0177.DIM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "fde94075cb3fd1c52af32062b0251af0"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0180.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3200,
            SectorSize = 512,
            Md5        = "f206c0caa4e0eda37233ab6e89ab5493",
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
            TestFile   = "DSKA0181.DIM.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            Md5        = "4375fe3d7e50a5044b4850d8542363fb",
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
            TestFile   = "DSKA0205.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1512,
            SectorSize = 512,
            Md5        = "d3106f2c989a0afcf97b63b051be8312"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0206.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "8245ddd644583bd78ac0638133c89824"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0207.DIM.lz",
            MediaType  = MediaType.XDF_525,
            Sectors    = 3040,
            SectorSize = 512,
            Md5        = "33c51a3d6f13cfedb5f08bf4c3cba7b9"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0209.DIM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "85574aebeef03eb355bf8541955d06ea"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0210.DIM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "85574aebeef03eb355bf8541955d06ea"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0211.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "647f14749f59be471aac04a71a079a64"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0212.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "517cdd5e42a4673f733d1aedfb46770f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0216.DIM.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "40199611e6e75bbc37ad6c52a5b77eae"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0218.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 5080,
            SectorSize = 512,
            Md5        = "fabacd63bd25f4c3db71523c21242bfb"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0219.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 9144,
            SectorSize = 512,
            Md5        = "0d1a1dfa4482422ff11fea76f8cef3a9"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0220.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 13716,
            SectorSize = 512,
            Md5        = "a6a67106457a20b46d05f2d9b27244f1"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0222.DIM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "85574aebeef03eb355bf8541955d06ea"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0232.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 630,
            SectorSize = 512,
            Md5        = "53a50481d90228f527b72f058de257da",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 630
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0245.DIM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "0d71b4952dadbfb1061acc1f4640c787"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0246.DIM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "af7ac6b5b9d2d57dad22dbb64ef7de38"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0262.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "5ac0a9fc7337f761098f816359b0f6f7",
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
            TestFile   = "DSKA0263.DIM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "1ea6ec8e663218b1372048f6e25795b5",
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
            TestFile   = "DSKA0264.DIM.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            Md5        = "77a1167b1b9043496e32b8578cde0ff0",
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
            TestFile   = "DSKA0265.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1680,
            SectorSize = 512,
            Md5        = "2b2c891ef5edee8518a1ae2ed3ab71a0",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1680
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0266.DIM.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "32c044c5c2b0bd13806149a759c14935",
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
            TestFile   = "DSKA0267.DIM.lz",
            MediaType  = MediaType.XDF_525,
            Sectors    = 3040,
            SectorSize = 512,
            Md5        = "8752095abc13dba3f3467669da333891",
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
            TestFile   = "DSKA0268.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3200,
            SectorSize = 512,
            Md5        = "aece7cd34bbba3e75307fa70404d9d30",
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
            TestFile   = "DSKA0269.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3280,
            SectorSize = 512,
            Md5        = "5289afb16a6e4a33213e3bcca56c6230",
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
            TestFile   = "DSKA0270.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3360,
            SectorSize = 512,
            Md5        = "092308e5df684702dd0ec393b6d3563a",
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
            TestFile   = "DSKA0271.DIM.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            Md5        = "b96596711f4d2ee85dfda0fe3b9f26c3",
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
            TestFile   = "DSKA0272.DIM.lz",
            MediaType  = MediaType.FDFORMAT_35_HD,
            Sectors    = 3444,
            SectorSize = 512,
            Md5        = "a4f461af7fda5e93a7ab63fcbb7e7683",
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
            TestFile   = "DSKA0273.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3528,
            SectorSize = 512,
            Md5        = "963f3aa8d4468d4373054f842d0e2245",
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
            TestFile   = "DSKA0280.DIM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "4feeaf4b4ee5dad85db727fbbda4b6d1",
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
            TestFile   = "DSKA0281.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 400,
            SectorSize = 512,
            Md5        = "3c77ca681df78e4cd7baa162aa9b0859",
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
            TestFile   = "DSKA0282.DIM.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "51da1f86c49657ffdb367bb2ddeb7990",
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
            TestFile   = "DSKA0283.DIM.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "b81a4987f89936630b8ebc62e4bbce6e"
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
            TestFile   = "DSKA0284.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 800,
            SectorSize = 512,
            Md5        = "f76f92dd326c99c5efad5ee58daf72e1",
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
            TestFile   = "DSKA0285.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 840,
            SectorSize = 512,
            Md5        = "b6f2c10e42908e334025bc4ffd81e771",
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
            TestFile   = "DSKA0287.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "f2f409ea2a62a7866fd2777cc4fc9739",
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
            TestFile   = "DSKA0288.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1512,
            SectorSize = 512,
            Md5        = "be89d2aab865a1217a3dda86e99bed97",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1512
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0289.DIM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "30a93f30dd4485c6fc037fe0775d3fc7",
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
            TestFile   = "DSKA0290.DIM.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            Md5        = "e0caf02cce5597c98313bcc480366ec7",
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
            TestFile   = "DSKA0299.DIM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            Md5        = "39bf5a98bcb2185d855ac06378febcfa",
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
            TestFile   = "DSKA0300.DIM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "dc20055b6e6fd6f8e1114d4be2effeed",
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
            TestFile   = "DSKA0301.DIM.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "56af9256cf71d5aac5fd5d363674bc49",
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
            TestFile   = "DSKA0302.DIM.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "bbba1e2d1418e05c3a4e7b4d585d160b",
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
            TestFile   = "DSKA0303.DIM.lz",
            MediaType  = MediaType.DOS_525_HD,
            Sectors    = 2400,
            SectorSize = 512,
            Md5        = "bca3a045e81617f7f5ebb5a8818eac47",
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
            TestFile   = "DSKA0304.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "a296663cb8e75e94603221352f29cfff",
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
            TestFile   = "DSKA0305.DIM.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "ecda36ebf0e1100233cb0ec722c18583",
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
            TestFile   = "DSKA0307.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 840,
            SectorSize = 512,
            Md5        = "cef2f4fe9b1a32d5c0544f814e634264",
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
            TestFile   = "DSKA0308.DIM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "bbe58e26b8f8f822cd3edfd37a4e4924",
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
            TestFile   = "DSKA0311.DIM.lz",
            MediaType  = MediaType.FDFORMAT_35_HD,
            Sectors    = 3444,
            SectorSize = 512,
            Md5        = "b9b6ebdf711364c979de7cf70c3a438a",
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
            TestFile   = "DSKA0314.DIM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "d37424f367f545acbb397f2bed766843",
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
            TestFile   = "DSKA0316.DIM.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "9963dd6f19ce6bd56eabeccdfbbd821a",
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
            TestFile   = "DSKA0317.DIM.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            Md5        = "acf6604559ae8217f7869823e2429024",
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
            TestFile   = "DSKA0318.DIM.lz",
            MediaType  = MediaType.FDFORMAT_35_HD,
            Sectors    = 3444,
            SectorSize = 512,
            Md5        = "23bf2139cdfdc4c16db058fd31ea6481",
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
            TestFile   = "DSKA0319.DIM.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            Md5        = "fa26adda0415f02057b113ad29c80c8d",
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
            TestFile   = "DSKA0320.DIM.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            Md5        = "4f2a8d036fefd6c6c88d99eda3aa12b7",
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
            TestFile   = "DSKA0322.DIM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1404,
            SectorSize = 512,
            Md5        = "1f6a23974b29d525706a2b0228325656",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1404
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "md1dd8.dim.lz",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            Md5        = "d81f5cb64fd0b99f138eab34110bbc3c",
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
            TestFile   = "md1dd.dim.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "a89006a75d13bee9202d1d6e52721ccb",
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
            TestFile   = "md1dd_fdformat_f200.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 400,
            SectorSize = 512,
            Md5        = "e1ad4a022778d7a0b24a93d8e68a59dc",
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
            TestFile   = "md1dd_fdformat_f205.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 420,
            SectorSize = 512,
            Md5        = "56a95fcf1d6f5c3108a17207b53ec07c",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 420
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2dd8.dim.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "beef1cdb004dc69391d6b3d508988b95",
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
            TestFile   = "md2dd.dim.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "6213897b7dbf263f12abf76901d43862"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2dd_fdformat_f400.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 800,
            SectorSize = 512,
            Md5        = "0aef12c906b744101b932d799ca88a78",
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
            TestFile   = "md2dd_fdformat_f410.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 840,
            SectorSize = 512,
            Md5        = "e7367df9998de0030a97b5131d1bed20",
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
            TestFile   = "md2dd_fdformat_f720.dim.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "1c36b819cfe355c11360bc120c9216fe",
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
            TestFile   = "md2dd_fdformat_f800.dim.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "25114403c11e337480e2afc4e6e32108",
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
            TestFile   = "md2dd_fdformat_f820.dim.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            Md5        = "3d7760ddaa55cd258057773d15106b78",
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
            TestFile   = "md2dd_freedos_800s.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 800,
            SectorSize = 512,
            Md5        = "29054ef703394ee3b35e849468a412ba",
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
            TestFile   = "md2dd_maxiform_1640s.dim.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            Md5        = "c91e852828c2aeee2fc94a6adbeed0ae",
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
            TestFile   = "md2dd_maxiform_840s.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 840,
            SectorSize = 512,
            Md5        = "efb6cfe53a6770f0ae388cb2c7f46264",
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
            TestFile   = "md2dd_qcopy_1476s.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1476,
            SectorSize = 512,
            Md5        = "6116f7c1397cadd55ba8d79c2aadc9dd",
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
            TestFile   = "md2dd_qcopy_1600s.dim.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "93100f8d86e5d0d0e6340f59c52a5e0d",
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
            TestFile   = "md2dd_qcopy_1640s.dim.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            Md5        = "cf7b7d43aa70863bedcc4a8432a5af67",
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
            TestFile   = "md2hd.dim.lz",
            MediaType  = MediaType.DOS_525_HD,
            Sectors    = 2400,
            SectorSize = 512,
            Md5        = "02259cd5fbcc20f8484aa6bece7a37c6",
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
            TestFile   = "md2hd_fdformat_f144.dim.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "073a172879a71339ef4b00ebb47b67fc",
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
            TestFile   = "md2hd_fdformat_f148.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2952,
            SectorSize = 512,
            Md5        = "d9890897130d0fc1eee3dbf4d9b0440f",
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
            TestFile   = "md2hd_maxiform_2788s.dim.lz",
            MediaType  = MediaType.FDFORMAT_525_HD,
            Sectors    = 2788,
            SectorSize = 512,
            Md5        = "09ca721aa883d5bbaa422c7943b0782c",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2788
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_alt.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1476,
            SectorSize = 512,
            Md5        = "259ff90e41e60682d948dd7d6af89735"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_alt_pass.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1476,
            SectorSize = 512,
            Md5        = "b40f8273fa7492bfe71c3d743269b97c",
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
            TestFile   = "mf2dd.dim.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "9827ba1b3e9cac41263caabd862e78f9",
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
            TestFile   = "mf2dd_fdformat_f800.dim.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "67d299c6e83f3f0fbcb8faa9ffa422c1",
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
            TestFile   = "mf2dd_fdformat_f820.dim.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            Md5        = "81d3bfec7b201f6a4503eb24c4394d4a",
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
            TestFile   = "mf2dd_freedos_1600s.dim.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "d07f7ffaee89742c6477aaaf94eb5715",
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
            TestFile   = "mf2dd_maxiform_1600s.dim.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "56af87802a9852e6e01e08d544740816",
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
            TestFile   = "mf2dd_qcopy_1494s.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1512,
            SectorSize = 512,
            Md5        = "34b7b99ef6fba2235eedbd8ae406d7d3",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1512
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_qcopy_1600s.dim.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "d9db52d992a76bf3bbc626ff844215a5",
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
            TestFile   = "mf2dd_qcopy_1660s.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1680,
            SectorSize = 512,
            Md5        = "3b74e367926181152c3499de8dd9b914",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1680
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2ed.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 5904,
            SectorSize = 512,
            Md5        = "82825116ffe6d68b4d920ad4875bd709"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2ed_pass.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 5904,
            SectorSize = 512,
            Md5        = "e7cdd1123b08eac4e9571825b1f6172f",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 5904
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_alt.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2952,
            SectorSize = 512,
            Md5        = "3b16537076c5517306dc672f8f1e376e"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_alt_pass.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2952,
            SectorSize = 512,
            Md5        = "022893d7766205894fca41bcde3c9f6c",
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
            TestFile   = "mf2hd.dim.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "1d32a686b7675c7a4f88c15522738432"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_dmf.dim.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            Md5        = "084d4d75f5e780cb9ec66a2fa784c371",
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
            TestFile   = "mf2hd_fdformat_f168.dim.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            Md5        = "1e06f21a1c11ea3347212da115bca08f",
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
            TestFile   = "mf2hd_fdformat_f16.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3200,
            SectorSize = 512,
            Md5        = "8eb8cb310feaf03c69fffd4f6e729847",
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
            TestFile   = "mf2hd_fdformat_f172.dim.lz",
            MediaType  = MediaType.FDFORMAT_35_HD,
            Sectors    = 3444,
            SectorSize = 512,
            Md5        = "3fc3a03d049416d81f81cc3b9ea8e5de",
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
            TestFile   = "mf2hd_freedos_3360s.dim.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            Md5        = "2bfd2e0a81bad704f8fc7758358cfcca",
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
            TestFile   = "mf2hd_maxiform_3200s.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3200,
            SectorSize = 512,
            Md5        = "3c4becd695ed25866d39966a9a93c2d9",
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
            TestFile   = "mf2hd_pass.dim.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "1d32a686b7675c7a4f88c15522738432"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_qcopy_2460s.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2460,
            SectorSize = 512,
            Md5        = "72282e11f7d91bf9c090b550fabfe80d",
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
            TestFile   = "mf2hd_qcopy_2720s.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2720,
            SectorSize = 512,
            Md5        = "457c1126dc7f36bbbabe9e17e90372e3",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2720
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_qcopy_2788s.dim.lz",
            MediaType  = MediaType.FDFORMAT_525_HD,
            Sectors    = 2788,
            SectorSize = 512,
            Md5        = "852181d5913c6f290872c66bbe992314",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2788
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_qcopy_2880s.dim.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "2980cc32504c945598dc50f1db576994",
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
            TestFile   = "mf2hd_qcopy_2952s.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2952,
            SectorSize = 512,
            Md5        = "c1c58d74fffb3656dd7f60f74ae8a629",
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
            TestFile   = "mf2hd_qcopy_2988s.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3024,
            SectorSize = 512,
            Md5        = "67391c3750f17a806503be3f9d514b1f",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3024
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_qcopy_3200s.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3200,
            SectorSize = 512,
            Md5        = "e45d41a61fbe48f328c995fcc10a5548",
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
            TestFile   = "mf2hd_qcopy_3320s.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3360,
            SectorSize = 512,
            Md5        = "c7764476489072dd053d5ec878171423",
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
            TestFile   = "mf2hd_qcopy_3360s.dim.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            Md5        = "15f71b92bd72aba5d80bf70eca4d5b1e",
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
            TestFile   = "mf2hd_qcopy_3486s.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3528,
            SectorSize = 512,
            Md5        = "f725bc714c3204e835e23c726ce77b89",
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
            TestFile   = "mf2hd_xdf_alt.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3772,
            SectorSize = 512,
            Md5        = "02d7c237c6ac1fbcd2fbbfb45c5fb767"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_xdf_alt_pass.dim.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3772,
            SectorSize = 512,
            Md5        = "99f83e846c5106dd4992646726e91636",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3772
                }
            }
        }
    };
}