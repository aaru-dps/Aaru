// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CopyQM.cs
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

namespace Aaru.Tests.Images;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

[TestFixture]
public class CopyQm : BlockMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "CopyQM");
    public override IMediaImage Plugin    => new DiscImages.CopyQm();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0000.CQM.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "e8bbbd22db87181974e12ba0227ea011"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0001.CQM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "9f5635f3df4d880a500910b0ad1ab535"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0002.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1280,
            SectorSize = 1024,
            Md5        = "9176f59e9205846b6212e084f46ed95c"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0003.CQM.lz",
            MediaType  = MediaType.ACORN_35_DS_DD,
            Sectors    = 800,
            SectorSize = 1024,
            Md5        = "85574aebeef03eb355bf8541955d06ea"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0004.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1280,
            SectorSize = 1024,
            Md5        = "1045bfd216ae1ae480dd0ef626f5ff39"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0006.CQM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "46fce47baf08c6f093f2c355a603543d"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0009.CQM.lz",
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
            TestFile   = "DSKA0010.CQM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "9e2b01f4397db2a6c76e2bc267df37b3"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0011.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1280,
            SectorSize = 1024,
            Md5        = "dbbf55398d930e14c2b0a035dd1277b9"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0012.CQM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "656002e6e620cb3b73c27f4c21d32edb"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0013.CQM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "1244cc2c101c66e6bb4ad5183b356b19"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0017.CQM.lz",
            MediaType  = MediaType.XDF_525,
            Sectors    = 3040,
            SectorSize = 512,
            Md5        = "8cad624afc06ab756f9800eba22ee886"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0018.CQM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "84cce7b4d8c8e21040163cd2d03a730c"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0019.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 640,
            SectorSize = 1024,
            Md5        = "76a1ef9485ffd5da1e9836725e375ada"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0020.CQM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "d236783dfd1dc29f350c51949b1e9e68"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0021.CQM.lz",
            MediaType  = MediaType.XDF_525,
            Sectors    = 3040,
            SectorSize = 512,
            Md5        = "6915f208cdda762eea2fe64ad754e72f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0023.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1280,
            SectorSize = 1024,
            Md5        = "b52f26c3c5b9b2cfc93a287a7fca3548"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0024.CQM.lz",
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
            TestFile   = "DSKA0025.CQM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "4e4cafed1cc22ea72201169427e5e1b6"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0026.CQM.lz",
            MediaType  = MediaType.ACORN_35_DS_DD,
            Sectors    = 800,
            SectorSize = 1024,
            Md5        = "a579b349a5a24218d59a44e36bdb1333"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0027.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 640,
            SectorSize = 1024,
            Md5        = "3135430552171a832339a8a93d44cc90"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0028.CQM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "1a4c7487382c98b7bc74623ddfb488e6"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0029.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 576,
            SectorSize = 1024,
            Md5        = "a8a9caa886a338b66181cfa21db6b620"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0030.CQM.lz",
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
            TestFile   = "DSKA0031.CQM.lz",
            MediaType  = MediaType.ACORN_525_SS_DD_40,
            Sectors    = 640,
            SectorSize = 256,
            Md5        = "e640835966327f3f662e1db8e0575510"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0032.CQM.lz",
            MediaType  = MediaType.ACORN_525_SS_DD_40,
            Sectors    = 640,
            SectorSize = 256,
            Md5        = "ff3534234d1d2dd88bf6e83be23d9227"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0033.CQM.lz",
            MediaType  = MediaType.ACORN_525_SS_DD_80,
            Sectors    = 1280,
            SectorSize = 256,
            Md5        = "dfaff34a6556b515642f1e54f839b02e"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0034.CQM.lz",
            MediaType  = MediaType.ACORN_525_SS_DD_80,
            Sectors    = 1280,
            SectorSize = 256,
            Md5        = "ca8f5c7f9ed161b03ccb166eb9d62146"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0035.CQM.lz",
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
            TestFile   = "DSKA0036.CQM.lz",
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
            TestFile   = "DSKA0037.CQM.lz",
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
            TestFile   = "DSKA0038.CQM.lz",
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
            TestFile   = "DSKA0039.CQM.lz",
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
            TestFile   = "DSKA0040.CQM.lz",
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
            TestFile   = "DSKA0041.CQM.lz",
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
            TestFile   = "DSKA0042.CQM.lz",
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
            TestFile   = "DSKA0043.CQM.lz",
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
            TestFile   = "DSKA0044.CQM.lz",
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
            TestFile   = "DSKA0045.CQM.lz",
            MediaType  = MediaType.NEC_35_HD_15,
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
            TestFile   = "DSKA0046.CQM.lz",
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
            TestFile   = "DSKA0047.CQM.lz",
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
            TestFile   = "DSKA0048.CQM.lz",
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
            TestFile   = "DSKA0049.CQM.lz",
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
            TestFile   = "DSKA0050.CQM.lz",
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
            TestFile   = "DSKA0051.CQM.lz",
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
            TestFile   = "DSKA0052.CQM.lz",
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
            TestFile   = "DSKA0053.CQM.lz",
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
            TestFile   = "DSKA0054.CQM.lz",
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
            TestFile   = "DSKA0055.CQM.lz",
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
            TestFile   = "DSKA0056.CQM.lz",
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
            TestFile   = "DSKA0057.CQM.lz",
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
            TestFile   = "DSKA0058.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3486,
            SectorSize = 512,
            Md5        = "4fc28b0128543b2eb70f6432c4c8a980",
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
            TestFile   = "DSKA0059.CQM.lz",
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
            TestFile   = "DSKA0060.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3570,
            SectorSize = 512,
            Md5        = "2031b1e16ee2defc0d15f732f633df33",
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
            TestFile   = "DSKA0069.CQM.lz",
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
            TestFile   = "DSKA0070.CQM.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            Md5        = "d1e978b679c63a218c3f77a7ca2c7206"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0073.CQM.lz",
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
            TestFile   = "DSKA0074.CQM.lz",
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
            TestFile   = "DSKA0075.CQM.lz",
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
            TestFile   = "DSKA0076.CQM.lz",
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
            TestFile   = "DSKA0077.CQM.lz",
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
            TestFile   = "DSKA0078.CQM.lz",
            MediaType  = MediaType.DOS_525_HD,
            Sectors    = 2400,
            SectorSize = 512,
            Md5        = "c9a193837db7d8a5eb025eb41e8a76d7"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0080.CQM.lz",
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
            TestFile   = "DSKA0081.CQM.lz",
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
            TestFile   = "DSKA0082.CQM.lz",
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
            TestFile   = "DSKA0083.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2988,
            SectorSize = 512,
            Md5        = "5f1d98806309aee7f81de72e51e6d386",
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
            TestFile   = "DSKA0084.CQM.lz",
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
            TestFile   = "DSKA0085.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3486,
            SectorSize = 512,
            Md5        = "c97a3081fd25474b6b7945b8572d5ab8",
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
            TestFile   = "DSKA0105.CQM.lz",
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
            TestFile   = "DSKA0106.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 410,
            SectorSize = 512,
            Md5        = "7b41dd9ca7eb32828960eb1417a6092a",
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
            TestFile   = "DSKA0107.CQM.lz",
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
            TestFile   = "DSKA0108.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 820,
            SectorSize = 512,
            Md5        = "e6492aac144f5f6f593b84c64680cf64",
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
            TestFile   = "DSKA0109.CQM.lz",
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
            TestFile   = "DSKA0110.CQM.lz",
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
            TestFile   = "DSKA0111.CQM.lz",
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
            TestFile   = "DSKA0112.CQM.lz",
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
            TestFile   = "DSKA0113.CQM.lz",
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
            TestFile   = "DSKA0114.CQM.lz",
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
            TestFile   = "DSKA0115.CQM.lz",
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
            TestFile   = "DSKA0116.CQM.lz",
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
            TestFile   = "DSKA0117.CQM.lz",
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
            TestFile   = "DSKA0120.CQM.lz",
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
            TestFile   = "DSKA0121.CQM.lz",
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
            TestFile   = "DSKA0122.CQM.lz",
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
            TestFile   = "DSKA0123.CQM.lz",
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
            TestFile   = "DSKA0124.CQM.lz",
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
            TestFile   = "DSKA0125.CQM.lz",
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
            TestFile   = "DSKA0126.CQM.lz",
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
            TestFile   = "DSKA0147.CQM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            Md5        = "6efa72a33021d5051546c3e0dd4c3c09"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0148.CQM.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "8b5acfd14818ff9556d3d81361ce4862"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0149.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 200,
            SectorSize = 1024,
            Md5        = "cf42d08469548a31caf2649a1d08a85f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0150.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 400,
            SectorSize = 1024,
            Md5        = "62745e10683cf2ec1dac177535459891"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0151.CQM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "298c377de52947c472a85d281b6d3d4d"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0153.CQM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "298c377de52947c472a85d281b6d3d4d"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0158.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1280,
            SectorSize = 256,
            Md5        = "8b5acfd14818ff9556d3d81361ce4862"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0159.CQM.lz",
            MediaType  = MediaType.ACORN_525_SS_DD_40,
            Sectors    = 640,
            SectorSize = 256,
            Md5        = "6efa72a33021d5051546c3e0dd4c3c09"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0162.CQM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            Md5        = "e63014a4299f52f22e6e2c9609f51979"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0163.CQM.lz",
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
            TestFile   = "DSKA0164.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 820,
            SectorSize = 512,
            Md5        = "32823b9009c99b6711e89336ad03ec7f",
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
            TestFile   = "DSKA0166.CQM.lz",
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
            TestFile   = "DSKA0167.CQM.lz",
            MediaType  = MediaType.ACORN_35_DS_DD,
            Sectors    = 800,
            SectorSize = 1024,
            Md5        = "185bc63e4304a2d2554615362b2d25c5"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0168.CQM.lz",
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
            TestFile   = "DSKA0169.CQM.lz",
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
            TestFile   = "DSKA0173.CQM.lz",
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
            TestFile   = "DSKA0174.CQM.lz",
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
            TestFile   = "DSKA0175.CQM.lz",
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
            TestFile   = "DSKA0180.CQM.lz",
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
            TestFile   = "DSKA0181.CQM.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            Md5        = "554492a7b41f4cd9068a3a2b70eb0e5f",
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
            TestFile   = "DSKA0182.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3402,
            SectorSize = 512,
            Md5        = "865ad9072cb6c7458f7d86d7e9368622"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0183.CQM.lz",
            MediaType  = MediaType.FDFORMAT_35_HD,
            Sectors    = 3444,
            SectorSize = 512,
            Md5        = "2461e458438f0033bc5811fd6958ad02"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0184.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1280,
            SectorSize = 1024,
            Md5        = "606d5fbf174708c7ecfbfdd2a50fec9c"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0185.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 800,
            SectorSize = 2048,
            Md5        = "6173d4c7b6a1addb14a4cbe088ede9d7"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0186.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 160,
            SectorSize = 8192,
            Md5        = "5f47876d515d9495789f5e27ed313959"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0197.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 600,
            SectorSize = 256,
            Md5        = "65531301132413a81f3994eaf0b16f50"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0198.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1200,
            SectorSize = 256,
            Md5        = "a13fbf4d230f421d1bc4d21b714dc36b"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0199.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2400,
            SectorSize = 256,
            Md5        = "de0170cd10ddd839a63370355b2ba4ed"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0200.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1280,
            SectorSize = 1024,
            Md5        = "1045bfd216ae1ae480dd0ef626f5ff39"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0201.CQM.lz",
            MediaType  = MediaType.ACORN_35_DS_DD,
            Sectors    = 800,
            SectorSize = 1024,
            Md5        = "85574aebeef03eb355bf8541955d06ea"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0202.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1280,
            SectorSize = 1024,
            Md5        = "1045bfd216ae1ae480dd0ef626f5ff39"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0203.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1280,
            SectorSize = 1024,
            Md5        = "8a16a3008739516fc3ba4c878868d056"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0204.CQM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "46fce47baf08c6f093f2c355a603543d"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0205.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1476,
            SectorSize = 512,
            Md5        = "ee73a5d5c8dfac236baf7b99811696f9"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0206.CQM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "8245ddd644583bd78ac0638133c89824"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0207.CQM.lz",
            MediaType  = MediaType.XDF_525,
            Sectors    = 3040,
            SectorSize = 512,
            Md5        = "33c51a3d6f13cfedb5f08bf4c3cba7b9"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0209.CQM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "85574aebeef03eb355bf8541955d06ea"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0210.CQM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "85574aebeef03eb355bf8541955d06ea"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0211.CQM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "647f14749f59be471aac04a71a079a64"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0212.CQM.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "517cdd5e42a4673f733d1aedfb46770f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0213.CQM.lz",
            MediaType  = MediaType.ACORN_35_DS_DD,
            Sectors    = 800,
            SectorSize = 1024,
            Md5        = "6ad92e9522e4ba902c01beecb5943bb1"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0214.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1280,
            SectorSize = 1024,
            Md5        = "8e077143864bb20e36f25a4685860a1e"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0215.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1280,
            SectorSize = 1024,
            Md5        = "9724c94417cef88b2ad2f3c1db9d8730"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0216.CQM.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "40199611e6e75bbc37ad6c52a5b77eae"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0221.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 5120,
            SectorSize = 256,
            Md5        = "f92b2e52259531d50bfb403dc1274ab1"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0222.CQM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "85574aebeef03eb355bf8541955d06ea"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0225.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 990,
            SectorSize = 256,
            Md5        = "dbcd4aa7c1c670a667c89b309bd9de42"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0228.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1050,
            SectorSize = 256,
            Md5        = "d88f521c048df99b8ef5f01a8a001455"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0232.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 621,
            SectorSize = 512,
            Md5        = "b76bd117ce24d933cdefe09b1de2164a"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0234.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2400,
            SectorSize = 256,
            Md5        = "a50f82253aa4d8dea4fb193d64a66778"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0240.CQM.lz",
            MediaType  = MediaType.ATARI_525_DD,
            Sectors    = 720,
            SectorSize = 256,
            Md5        = "d1ab955f0961ab94e6cf69f78134a84b"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0241.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 714,
            SectorSize = 256,
            Md5        = "8b62738f15bcc916a668eaa67eec86e7"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0242.CQM.lz",
            MediaType  = MediaType.NEC_35_HD_8,
            Sectors    = 1232,
            SectorSize = 1024,
            Md5        = "87a432496cb23b5c2299545500df3553",
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
            TestFile   = "DSKA0243.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1280,
            SectorSize = 256,
            Md5        = "9866ab8e58fa4be25010184aec4ad3aa"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0244.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1280,
            SectorSize = 256,
            Md5        = "9dab329ae098b29889ab08278de38f95"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0245.CQM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "0d71b4952dadbfb1061acc1f4640c787"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0246.CQM.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "af7ac6b5b9d2d57dad22dbb64ef7de38"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0247.CQM.lz",
            MediaType  = MediaType.ACORN_525_SS_DD_80,
            Sectors    = 1280,
            SectorSize = 256,
            Md5        = "f8f81f945aaad6fbfe7e2db1905302c1"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0248.CQM.lz",
            MediaType  = MediaType.ACORN_525_SS_DD_80,
            Sectors    = 1280,
            SectorSize = 256,
            Md5        = "f6f81c75b5ba45d91c1886c6dda9caee"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0250.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 640,
            SectorSize = 1024,
            Md5        = "0b9cb8107cbb94c5e36aea438a04dc98"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0251.CQM.lz",
            MediaType  = MediaType.ACORN_525_DS_DD,
            Sectors    = 2560,
            SectorSize = 256,
            Md5        = "d075e50705f4ddca7ba4dbc981ec1176"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0252.CQM.lz",
            MediaType  = MediaType.ACORN_525_SS_DD_80,
            Sectors    = 1280,
            SectorSize = 256,
            Md5        = "9f86480c86bae33a5b444e4a7ed55048"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0253.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 640,
            SectorSize = 1024,
            Md5        = "231891ccd0cc599cfe25419c669fc5f8"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0254.CQM.lz",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "5dc0d482a773043d8683a84c8220df95",
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
            TestFile   = "DSKA0258.CQM.lz",
            MediaType  = MediaType.SHARP_525,
            Sectors    = 1232,
            SectorSize = 1024,
            Md5        = "855943f9caecdcce9b06f0098d773c6b",
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
            TestFile   = "DSKA0262.CQM.lz",
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
            TestFile   = "DSKA0263.CQM.lz",
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
            TestFile   = "DSKA0264.CQM.lz",
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
            TestFile   = "DSKA0265.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1660,
            SectorSize = 512,
            Md5        = "4b07d760d65f3f0f8ffa5f2b81cee907",
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
            TestFile   = "DSKA0266.CQM.lz",
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
            TestFile   = "DSKA0267.CQM.lz",
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
            TestFile   = "DSKA0268.CQM.lz",
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
            TestFile   = "DSKA0269.CQM.lz",
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
            TestFile   = "DSKA0270.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3320,
            SectorSize = 512,
            Md5        = "1aef0a0ba233476db6567878c3c2b266",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3320
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0271.CQM.lz",
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
            TestFile   = "DSKA0272.CQM.lz",
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
            TestFile   = "DSKA0273.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3486,
            SectorSize = 512,
            Md5        = "8f7f7099d4475f6631fcf0a79b031d61",
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
            TestFile   = "DSKA0280.CQM.lz",
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
            TestFile   = "DSKA0281.CQM.lz",
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
            TestFile   = "DSKA0282.CQM.lz",
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
            TestFile   = "DSKA0283.CQM.lz",
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
            TestFile   = "DSKA0284.CQM.lz",
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
            TestFile   = "DSKA0285.CQM.lz",
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
            TestFile   = "DSKA0287.CQM.lz",
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
            TestFile   = "DSKA0288.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1494,
            SectorSize = 512,
            Md5        = "3e441d69cec5c3169274e1379de4af4b",
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
            TestFile   = "DSKA0289.CQM.lz",
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
            TestFile   = "DSKA0290.CQM.lz",
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
            TestFile   = "DSKA0291.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1660,
            SectorSize = 512,
            Md5        = "4af4904d2b3c815da7bef7049209f5eb",
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
            TestFile   = "DSKA0299.CQM.lz",
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
            TestFile   = "DSKA0300.CQM.lz",
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
            TestFile   = "DSKA0301.CQM.lz",
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
            TestFile   = "DSKA0302.CQM.lz",
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
            TestFile   = "DSKA0303.CQM.lz",
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
            TestFile   = "DSKA0304.CQM.lz",
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
            TestFile   = "DSKA0305.CQM.lz",
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
            TestFile   = "DSKA0307.CQM.lz",
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
            TestFile   = "DSKA0308.CQM.lz",
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
            TestFile   = "DSKA0311.CQM.lz",
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
            TestFile   = "DSKA0314.CQM.lz",
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
            TestFile   = "DSKA0316.CQM.lz",
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
            TestFile   = "DSKA0317.CQM.lz",
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
            TestFile   = "DSKA0318.CQM.lz",
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
            TestFile   = "DSKA0319.CQM.lz",
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
            TestFile   = "DSKA0320.CQM.lz",
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
            TestFile   = "DSKA0322.CQM.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1386,
            SectorSize = 512,
            Md5        = "e794a3ffa4069ea999fdf7146710fa9e",
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
            TestFile   = "mf2dd.cqm.lz",
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
            TestFile   = "mf2dd_fdformat_800.cqm.lz",
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
            TestFile   = "mf2dd_freedos.cqm.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "1ff7649b679ba22ff20d39ff717dbec8",
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
            TestFile   = "mf2hd_blind.cqm.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "b4a602f67903c46eef62addb0780aa56",
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
            TestFile   = "mf2hd.cqm.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "b4a602f67903c46eef62addb0780aa56",
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
            TestFile   = "mf2hd_fdformat_168.cqm.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            Md5        = "03c2af6a8ebf4bd6f530335de34ae5dd",
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
            TestFile   = "mf2hd_freedos.cqm.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            Md5        = "1a9f2eeb3cbeeb057b9a9a5c6e9b0cc6"
        }
    };
}