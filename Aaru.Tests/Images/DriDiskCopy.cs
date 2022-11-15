// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DriDiskCopy.cs
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
using NUnit.Framework;

namespace Aaru.Tests.Images;

[TestFixture]
public class DriDiskCopy : BlockMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "DRI DISKCOPY");
    public override IMediaImage Plugin     => new DiscImages.DriDiskCopy();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0000.IMG.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "e8bbbd22db87181974e12ba0227ea011"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0001.IMG.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "9f5635f3df4d880a500910b0ad1ab535"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0009.IMG.lz",
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
            TestFile   = "DSKA0010.IMG.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "9e2b01f4397db2a6c76e2bc267df37b3"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0024.IMG.lz",
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
            TestFile   = "DSKA0025.IMG.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "f7dd138edcab7bd328d7396d48aac395"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0030.IMG.lz",
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
            TestFile   = "DSKA0035.IMG.lz",
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
            TestFile   = "DSKA0036.IMG.lz",
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
            TestFile   = "DSKA0037.IMG.lz",
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
            TestFile   = "DSKA0038.IMG.lz",
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
            TestFile   = "DSKA0039.IMG.lz",
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
            TestFile   = "DSKA0040.IMG.lz",
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
            TestFile   = "DSKA0041.IMG.lz",
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
            TestFile   = "DSKA0042.IMG.lz",
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
            TestFile   = "DSKA0043.IMG.lz",
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
            TestFile   = "DSKA0044.IMG.lz",
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
            TestFile   = "DSKA0045.IMG.lz",
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
            TestFile   = "DSKA0046.IMG.lz",
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
            TestFile   = "DSKA0047.IMG.lz",
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
            TestFile   = "DSKA0048.IMG.lz",
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
            TestFile   = "DSKA0049.IMG.lz",
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
            TestFile   = "DSKA0050.IMG.lz",
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
            TestFile   = "DSKA0051.IMG.lz",
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
            TestFile   = "DSKA0052.IMG.lz",
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
            TestFile   = "DSKA0053.IMG.lz",
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
            TestFile   = "DSKA0054.IMG.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3200,
            SectorSize = 512,
            Md5        = "803b01a0b440c2837d37c21308f30cd5"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0055.IMG.lz",
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
            TestFile   = "DSKA0056.IMG.lz",
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
            TestFile   = "DSKA0057.IMG.lz",
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
            TestFile   = "DSKA0058.IMG.lz",
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
            TestFile   = "DSKA0059.IMG.lz",
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
            TestFile   = "DSKA0060.IMG.lz",
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
            TestFile   = "DSKA0069.IMG.lz",
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
            TestFile   = "DSKA0073.IMG.lz",
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
            TestFile   = "DSKA0074.IMG.lz",
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
            TestFile   = "DSKA0075.IMG.lz",
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
            TestFile   = "DSKA0076.IMG.lz",
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
            TestFile   = "DSKA0077.IMG.lz",
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
            TestFile   = "DSKA0078.IMG.lz",
            MediaType  = MediaType.DOS_525_HD,
            Sectors    = 2400,
            SectorSize = 512,
            Md5        = "c9a193837db7d8a5eb025eb41e8a76d7"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0080.IMG.lz",
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
            TestFile   = "DSKA0081.IMG.lz",
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
            TestFile   = "DSKA0082.IMG.lz",
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
            TestFile   = "DSKA0083.IMG.lz",
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
            TestFile   = "DSKA0084.IMG.lz",
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
            TestFile   = "DSKA0085.IMG.lz",
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
            TestFile   = "DSKA0089.IMG.lz",
            MediaType  = MediaType.XDF_525,
            Sectors    = 3040,
            SectorSize = 512,
            Md5        = "0abf995856080e5292e63c63f7c97a45"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0090.IMG.lz",
            MediaType  = MediaType.XDF_35,
            Sectors    = 3680,
            SectorSize = 512,
            Md5        = "8be2aaf6ecea213aee9fc82c8a85061e"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0091.IMG.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            Md5        = "0a432572a28d3b53a0cf2b5c211fe777",
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
            TestFile   = "DSKA0092.IMG.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1804,
            SectorSize = 512,
            Md5        = "cd84fa2d62ac7c36783224c3ba0be664",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1804
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0093.IMG.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2952,
            SectorSize = 512,
            Md5        = "63d29a9d867d924421c10793a0f22965",
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
            TestFile   = "DSKA0094.IMG.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3116,
            SectorSize = 512,
            Md5        = "21778906886c0314f0f33c4b0040ba16",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3116
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0097.IMG.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3608,
            SectorSize = 512,
            Md5        = "6b8e89b1d5117ba19c3e52544ffe041e",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3608
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0098.IMG.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3772,
            SectorSize = 512,
            Md5        = "543fc539902eb66b5c312d7908ecf97a",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3772
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0099.IMG.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1952,
            SectorSize = 512,
            Md5        = "b30709f798bfb8469d02a82c882f780c",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1952
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0101.IMG.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3280,
            SectorSize = 512,
            Md5        = "0f3e923010b50b550591a89ea2dee62b",
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
            TestFile   = "DSKA0103.IMG.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3944,
            SectorSize = 512,
            Md5        = "d5b927503abcd1978496bc679bb9c2f7",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3944
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0105.IMG.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 400,
            SectorSize = 512,
            Md5        = "d40a99cb549fcfb26fcf9ef01b5dfca7"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0106.IMG.lz",
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
            TestFile   = "DSKA0107.IMG.lz",
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
            TestFile   = "DSKA0108.IMG.lz",
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
            TestFile   = "DSKA0109.IMG.lz",
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
            TestFile   = "DSKA0110.IMG.lz",
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
            TestFile   = "DSKA0111.IMG.lz",
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
            TestFile   = "DSKA0112.IMG.lz",
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
            TestFile   = "DSKA0113.IMG.lz",
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
            TestFile   = "DSKA0114.IMG.lz",
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
            TestFile   = "DSKA0115.IMG.lz",
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
            TestFile   = "DSKA0116.IMG.lz",
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
            TestFile   = "DSKA0117.IMG.lz",
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
            TestFile   = "DSKA0120.IMG.lz",
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
            TestFile   = "DSKA0121.IMG.lz",
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
            TestFile   = "DSKA0122.IMG.lz",
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
            TestFile   = "DSKA0123.IMG.lz",
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
            TestFile   = "DSKA0124.IMG.lz",
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
            TestFile   = "DSKA0125.IMG.lz",
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
            TestFile   = "DSKA0126.IMG.lz",
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
            TestFile   = "DSKA0163.IMG.lz",
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
            TestFile   = "DSKA0164.IMG.lz",
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
            TestFile   = "DSKA0166.IMG.lz",
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
            TestFile   = "DSKA0168.IMG.lz",
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
            TestFile   = "DSKA0169.IMG.lz",
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
            TestFile   = "DSKA0173.IMG.lz",
            MediaType  = MediaType.DOS_35_SS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "028769dc0abefab1740cc309432588b6"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0174.IMG.lz",
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
            TestFile   = "DSKA0175.IMG.lz",
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
            TestFile   = "DSKA0180.IMG.lz",
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
            TestFile   = "DSKA0181.IMG.lz",
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
            TestFile   = "DSKA0182.IMG.lz",
            MediaType  = MediaType.FDFORMAT_35_HD,
            Sectors    = 3444,
            SectorSize = 512,
            Md5        = "36dd03967a2a3369538cad29b8b74b71",
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
            TestFile   = "DSKA0183.IMG.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3486,
            SectorSize = 512,
            Md5        = "4f5c02448e75bbc086e051c728414513",
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
            TestFile   = "DSKA0262.IMG.lz",
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
            TestFile   = "DSKA0263.IMG.lz",
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
            TestFile   = "DSKA0264.IMG.lz",
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
            TestFile   = "DSKA0265.IMG.lz",
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
            TestFile   = "DSKA0266.IMG.lz",
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
            TestFile   = "DSKA0267.IMG.lz",
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
            TestFile   = "DSKA0268.IMG.lz",
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
            TestFile   = "DSKA0269.IMG.lz",
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
            TestFile   = "DSKA0270.IMG.lz",
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
            TestFile   = "DSKA0271.IMG.lz",
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
            TestFile   = "DSKA0272.IMG.lz",
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
            TestFile   = "DSKA0273.IMG.lz",
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
            TestFile   = "DSKA0280.IMG.lz",
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
            TestFile   = "DSKA0281.IMG.lz",
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
            TestFile   = "DSKA0282.IMG.lz",
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
            TestFile   = "DSKA0283.IMG.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "b81a4987f89936630b8ebc62e4bbce6e"
            /* TODO IndexOutOfRangeException
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
            TestFile   = "DSKA0284.IMG.lz",
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
            TestFile   = "DSKA0285.IMG.lz",
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
            TestFile   = "DSKA0287.IMG.lz",
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
            TestFile   = "DSKA0288.IMG.lz",
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
            TestFile   = "DSKA0289.IMG.lz",
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
            TestFile   = "DSKA0290.IMG.lz",
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
            TestFile   = "DSKA0291.IMG.lz",
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
            TestFile   = "DSKA0299.IMG.lz",
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
            TestFile   = "DSKA0300.IMG.lz",
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
            TestFile   = "DSKA0301.IMG.lz",
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
            TestFile   = "DSKA0302.IMG.lz",
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
            TestFile   = "DSKA0303.IMG.lz",
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
            TestFile   = "DSKA0304.IMG.lz",
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
            TestFile   = "DSKA0305.IMG.lz",
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
            TestFile   = "DSKA0308.IMG.lz",
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
            TestFile   = "DSKA0311.IMG.lz",
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
            TestFile   = "DSKA0314.IMG.lz",
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
            TestFile   = "DSKA0316.IMG.lz",
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
            TestFile   = "DSKA0317.IMG.lz",
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
            TestFile   = "DSKA0318.IMG.lz",
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
            TestFile   = "DSKA0319.IMG.lz",
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
            TestFile   = "DSKA0320.IMG.lz",
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
            TestFile   = "DSKA0322.IMG.lz",
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
            TestFile   = "md1dd8.img.lz",
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
            TestFile   = "md1dd.img.lz",
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
            TestFile   = "md2dd_2m_fast.img.lz",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            Md5        = "319fa8bef964c2a63e34bdb48e77cc4e",
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
            TestFile   = "md2dd_2m_max.img.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1804,
            SectorSize = 512,
            Md5        = "306a61469b4c3c83f3e5f9ae409d83cd",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1804
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2dd8.img.lz",
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
            TestFile   = "md2dd_freedos_800s.img.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 800,
            SectorSize = 512,
            Md5        = "29054ef703394ee3b35e849468a412ba"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2dd.img.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "6213897b7dbf263f12abf76901d43862"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2dd_maxiform_1640s.img.lz",
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
            TestFile   = "md2dd_maxiform_840s.img.lz",
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
            TestFile   = "md2dd_qcopy_1476s.img.lz",
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
            TestFile   = "md2dd_qcopy_1600s.img.lz",
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
            TestFile   = "md2dd_qcopy_1640s.img.lz",
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
            TestFile   = "md2hd_2m_fast.img.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2952,
            SectorSize = 512,
            Md5        = "215198cf2a336e718208fc207bb62c6d",
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
            TestFile   = "md2hd_2m_max.img.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3116,
            SectorSize = 512,
            Md5        = "2c96964b5d91444302e21721c25ea120",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3116
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2hd.img.lz",
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
            TestFile   = "md2hd_maxiform_2788s.img.lz",
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
            TestFile   = "md2hd_nec.img.lz",
            MediaType  = MediaType.SHARP_525,
            Sectors    = 2464,
            SectorSize = 1024,
            Md5        = "84812b791fd2113b4aa00894f6894339"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2hd_xdf.img.lz",
            MediaType  = MediaType.XDF_525,
            Sectors    = 3040,
            SectorSize = 512,
            Md5        = "d78dc81491edeec99aa202d02f3daf00"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_2m.dsk.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1968,
            SectorSize = 512,
            Md5        = "9a8670fbaf6307b8d5f32aa10e1be435",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1968
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_2m_fast.img.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1968,
            SectorSize = 512,
            Md5        = "05d29642cdcddafa0dcaff91682f8fe0",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 1968
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_2mgui.dsk.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 9408,
            SectorSize = 128,
            Md5        = "beb782f6bc970e32ceef79cd112e2e48"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_2m_max.dsk.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2132,
            SectorSize = 512,
            Md5        = "a99603cd3219aab1299e66b2999f0e57",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2132
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_2m_max.img.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2132,
            SectorSize = 512,
            Md5        = "3da419125f45e1fe3b46f6fad3acc1c2",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2132
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd.dsk.lz",
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
            TestFile   = "mf2dd_fdformat_800.dsk.lz",
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
            TestFile   = "mf2dd_fdformat_820.dsk.lz",
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
            TestFile   = "mf2dd_freedos_1600s.img.lz",
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
            TestFile   = "mf2dd_freedos.dsk.lz",
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
            TestFile   = "mf2dd.img.lz",
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
            TestFile   = "mf2dd_maxiform_1600s.img.lz",
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
            TestFile   = "mf2dd_qcopy_1494s.img.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1494,
            SectorSize = 512,
            Md5        = "fd7fb1ba11cdfe11db54af0322abf59d",
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
            TestFile   = "mf2dd_qcopy_1600s.img.lz",
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
            TestFile   = "mf2dd_qcopy_1660s.img.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1660,
            SectorSize = 512,
            Md5        = "5949d0be57ce8bffcda7c4be4d1348ee",
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
            TestFile   = "mf2ed.img.lz",
            MediaType  = MediaType.ECMA_147,
            Sectors    = 5760,
            SectorSize = 512,
            Md5        = "4aeafaf2a088d6a7406856dce8118567",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 5760
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_2m.dsk.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3608,
            SectorSize = 512,
            Md5        = "2f6964d410b275c8e9f60fe2f24b361a"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_2m_fast.img.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3608,
            SectorSize = 512,
            Md5        = "967726aede85c68f66887672078f8856",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3608
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_2mgui.dsk.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 15776,
            SectorSize = 128,
            Md5        = "0037b5497d5cb0c7721085f61e223b6a"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_2m_max.dsk.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3772,
            SectorSize = 512,
            Md5        = "3fa4f87d7058ba940b88e0d80f0d7ded",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3772
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_2m_max.img.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3772,
            SectorSize = 512,
            Md5        = "5a6d961ed5f089364f2816692bcbe685",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3772
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_dmf.img.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            Md5        = "b042310181410227d0072fef1e98a989",
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
            TestFile   = "mf2hd.dsk.lz",
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
            TestFile   = "mf2hd_fdformat_168.dsk.lz",
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
            TestFile   = "mf2hd_fdformat_172.dsk.lz",
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
            TestFile   = "mf2hd_freedos_3360s.img.lz",
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
            TestFile   = "mf2hd_freedos_3486s.img.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3486,
            SectorSize = 512,
            Md5        = "a79ec33c623697b4562dacaed31523b8",
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
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "00e61c06bf29f0c04a7eabe2dbd7efb6",
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
            TestFile   = "mf2hd_maxiform_3200s.img.lz",
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
            TestFile   = "mf2hd_nec.img.lz",
            MediaType  = MediaType.SHARP_525,
            Sectors    = 2464,
            SectorSize = 1024,
            Md5        = "626ec389d4f8968170401b3775181a2b"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_qcopy_2460s.img.lz",
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
            TestFile   = "mf2hd_qcopy_2720s.img.lz",
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
            TestFile   = "mf2hd_qcopy_2788s.img.lz",
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
            TestFile   = "mf2hd_qcopy_2880s.img.lz",
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
            TestFile   = "mf2hd_qcopy_2952s.img.lz",
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
            TestFile   = "mf2hd_qcopy_2988s.img.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2988,
            SectorSize = 512,
            Md5        = "097bb2fd34cee5ebde7b5641975ffd60",
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
            TestFile   = "mf2hd_qcopy_3200s.img.lz",
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
            TestFile   = "mf2hd_qcopy_3320s.img.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3320,
            SectorSize = 512,
            Md5        = "c25f2a57c71db1cd4fea2263598f544a",
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
            TestFile   = "mf2hd_qcopy_3360s.img.lz",
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
            TestFile   = "mf2hd_qcopy_3486s.img.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3486,
            SectorSize = 512,
            Md5        = "d88c8d818e238c9e52b8588b5fd52efe",
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
            TestFile   = "mf2hd_xdf.dsk.lz",
            MediaType  = MediaType.XDF_35,
            Sectors    = 3680,
            SectorSize = 512,
            Md5        = "3d5fcdaf627257ae9f50a06bdba26965",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 3680
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_xdf.img.lz",
            MediaType  = MediaType.XDF_35,
            Sectors    = 3680,
            SectorSize = 512,
            Md5        = "4cb9398cf02ed9e08d0972c1ccba804b"
        }
    };
}