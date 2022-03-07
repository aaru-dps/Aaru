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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Tests.Images;

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

[TestFixture]
public class TeleDisk : BlockMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "TeleDisk");
    public override IMediaImage _plugin    => new DiscImages.TeleDisk();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "md2dd8.td0.lz",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            MD5        = "beef1cdb004dc69391d6b3d508988b95",
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
            TestFile   = "md2dd_fdformat_f400.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 800,
            SectorSize = 512,
            MD5        = "0aef12c906b744101b932d799ca88a78"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2dd_fdformat_f410.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 820,
            SectorSize = 512,
            MD5        = "348d12add1ed226cd712a4a6a10d1a34",
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
            TestFile   = "md2dd_fdformat_f720.td0.lz",
            MediaType  = MediaType.ECMA_78_2,
            Sectors    = 1440,
            SectorSize = 512,
            MD5        = "1c36b819cfe355c11360bc120c9216fe",
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
            TestFile   = "md2dd_fdformat_f800.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "25114403c11e337480e2afc4e6e32108",
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
            TestFile   = "md2dd_fdformat_f820.td0.lz",
            MediaType  = MediaType.FDFORMAT_525_DD,
            Sectors    = 1640,
            SectorSize = 512,
            MD5        = "3d7760ddaa55cd258057773d15106b78",
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
            TestFile   = "md2dd_freedos_800s.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 800,
            SectorSize = 512,
            MD5        = "29054ef703394ee3b35e849468a412ba"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2dd_maxiform_1640s.td0.lz",
            MediaType  = MediaType.FDFORMAT_525_DD,
            Sectors    = 1640,
            SectorSize = 512,
            MD5        = "c91e852828c2aeee2fc94a6adbeed0ae",
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
            TestFile   = "md2dd_maxiform_840s.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 840,
            SectorSize = 512,
            MD5        = "efb6cfe53a6770f0ae388cb2c7f46264",
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
            TestFile   = "md2dd_qcopy_1476s.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1476,
            SectorSize = 512,
            MD5        = "6116f7c1397cadd55ba8d79c2aadc9dd",
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
            TestFile   = "md2dd_qcopy_1600s.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "93100f8d86e5d0d0e6340f59c52a5e0d",
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
            TestFile   = "md2dd_qcopy_1640s.td0.lz",
            MediaType  = MediaType.FDFORMAT_525_DD,
            Sectors    = 1640,
            SectorSize = 512,
            MD5        = "cf7b7d43aa70863bedcc4a8432a5af67",
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
            TestFile   = "md2dd.td0.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            MD5        = "6213897b7dbf263f12abf76901d43862"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2hd_fdformat_f144.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2880,
            SectorSize = 512,
            MD5        = "073a172879a71339ef4b00ebb47b67fc",
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
            TestFile   = "md2hd_fdformat_f148.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2952,
            SectorSize = 512,
            MD5        = "d9890897130d0fc1eee3dbf4d9b0440f",
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
            TestFile   = "md2hd_maxiform_2788s.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2788,
            SectorSize = 512,
            MD5        = "09ca721aa883d5bbaa422c7943b0782c",
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
            TestFile   = "md2hd.td0.lz",
            MediaType  = MediaType.DOS_525_HD,
            Sectors    = 2400,
            SectorSize = 512,
            MD5        = "02259cd5fbcc20f8484aa6bece7a37c6",
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
            TestFile   = "md2hd_xdf.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 640,
            SectorSize = 128,
            MD5        = "b903ea7e0c9d7e4c6251df4825212db4"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_fdformat_800.td0.lz",
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
            TestFile   = "mf2dd_fdformat_820.td0.lz",
            MediaType  = MediaType.Unknown,
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
            TestFile   = "mf2dd_fdformat_f800.td0.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "26532a62985b51a2c3b877a57f6d257b",
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
            TestFile   = "mf2dd_fdformat_f820.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1640,
            SectorSize = 512,
            MD5        = "a7771acff766557cc23b8c6943b588f9",
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
            TestFile   = "mf2dd_freedos_1600s.td0.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "d07f7ffaee89742c6477aaaf94eb5715",
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
            TestFile   = "mf2dd_freedos.td0.lz",
            MediaType  = MediaType.Unknown,
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
            TestFile   = "mf2dd_maxiform_1600s.td0.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "56af87802a9852e6e01e08d544740816",
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
            TestFile   = "mf2dd_qcopy_1494s.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1494,
            SectorSize = 512,
            MD5        = "fd7fb1ba11cdfe11db54af0322abf59d",
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
            TestFile   = "mf2dd_qcopy_1600s.td0.lz",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            MD5        = "d9db52d992a76bf3bbc626ff844215a5",
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
            TestFile   = "mf2dd_qcopy_1660s.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1660,
            SectorSize = 512,
            MD5        = "5949d0be57ce8bffcda7c4be4d1348ee",
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
            TestFile   = "mf2dd.td0.lz",
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
            TestFile   = "mf2hd_2m_max.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1148,
            SectorSize = 512,
            MD5        = "4b88a3e43b57778422e8b1e851a9c902"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_2m.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1804,
            SectorSize = 512,
            MD5        = "d032d928c43b66419b7404b016ec07ff"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_alt_adv.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2880,
            SectorSize = 512,
            MD5        = "1d32a686b7675c7a4f88c15522738432"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_alt_dos_adv.td0.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            MD5        = "8aea37782c507baf6b294467249b4608"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_alt_dos.td0.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            MD5        = "8aea37782c507baf6b294467249b4608"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_alt.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2880,
            SectorSize = 512,
            MD5        = "1d32a686b7675c7a4f88c15522738432"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_dmf.td0.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            MD5        = "28764d4f69c3865e2af71a41ca3f432f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_fdformat_168.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 332,
            SectorSize = 512,
            MD5        = "62b900808c3e9f91f8361fd1716155a1"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_fdformat_172.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3444,
            SectorSize = 512,
            MD5        = "9dea1e119a73a21a38d134f36b2e5564"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_fdformat_f168.td0.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            MD5        = "7e3bf04f3660dd1052a335dc99441e44",
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
            TestFile   = "mf2hd_fdformat_f16.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3200,
            SectorSize = 512,
            MD5        = "8eb8cb310feaf03c69fffd4f6e729847",
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
            TestFile   = "mf2hd_fdformat_f172.td0.lz",
            MediaType  = MediaType.DMF_82,
            Sectors    = 3444,
            SectorSize = 512,
            MD5        = "a58fd062f024b95714f1223a8bc2232f",
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
            TestFile   = "mf2hd_freedos_3360s.td0.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            MD5        = "2bfd2e0a81bad704f8fc7758358cfcca",
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
            TestFile   = "mf2hd_freedos_3486s.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3486,
            SectorSize = 512,
            MD5        = "a79ec33c623697b4562dacaed31523b8",
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
            TestFile   = "mf2hd_freedos.td0.lz",
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
            TestFile   = "mf2hd_maxiform_3200s.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3200,
            SectorSize = 512,
            MD5        = "3c4becd695ed25866d39966a9a93c2d9",
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
            TestFile   = "mf2hd_qcopy_2460s.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2460,
            SectorSize = 512,
            MD5        = "72282e11f7d91bf9c090b550fabfe80d",
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
            TestFile   = "mf2hd_qcopy_2720s.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2720,
            SectorSize = 512,
            MD5        = "457c1126dc7f36bbbabe9e17e90372e3",
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
            TestFile   = "mf2hd_qcopy_2788s.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2788,
            SectorSize = 512,
            MD5        = "852181d5913c6f290872c66bbe992314",
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
            TestFile   = "mf2hd_qcopy_2880s.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2880,
            SectorSize = 512,
            MD5        = "2980cc32504c945598dc50f1db576994",
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
            TestFile   = "mf2hd_qcopy_2952s.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 2952,
            SectorSize = 512,
            MD5        = "c1c58d74fffb3656dd7f60f74ae8a629",
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
            TestFile   = "mf2hd_qcopy_3200s.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3200,
            SectorSize = 512,
            MD5        = "e45d41a61fbe48f328c995fcc10a5548",
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
            TestFile   = "mf2hd_qcopy_3320s.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3320,
            SectorSize = 512,
            MD5        = "c25f2a57c71db1cd4fea2263598f544a",
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
            TestFile   = "mf2hd_qcopy_3360s.td0.lz",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            MD5        = "15f71b92bd72aba5d80bf70eca4d5b1e",
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
            TestFile   = "mf2hd_qcopy_3486s.td0.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 3486,
            SectorSize = 512,
            MD5        = "d88c8d818e238c9e52b8588b5fd52efe",
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
            TestFile   = "mf2hd_xdf_adv.td0.lz",
            MediaType  = MediaType.XDF_35,
            Sectors    = 640,
            SectorSize = 512,
            MD5        = "728f9361203dc39961b1413aa050f70d"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_xdf.td0.lz",
            MediaType  = MediaType.XDF_35,
            Sectors    = 640,
            SectorSize = 512,
            MD5        = "728f9361203dc39961b1413aa050f70d"
        }
    };
}