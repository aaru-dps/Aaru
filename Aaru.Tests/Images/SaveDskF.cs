// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SaveDskF.cs
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
using NUnit.Framework;

namespace Aaru.Tests.Images;

[TestFixture]
public class SaveDskF : BlockMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "SaveDskF");
    public override IMediaImage Plugin     => new Aaru.Images.SaveDskF();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "5dd8_c.dsk",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "5a1e0a75d31d88c1ce7429fd333c268f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5dd8_ck.dsk",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "5a1e0a75d31d88c1ce7429fd333c268f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5dd8_na.dsk",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "4989762c82f173f9b52e0bdb8cf5becb"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5dd8_nak.dsk",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "4989762c82f173f9b52e0bdb8cf5becb"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5dd8_n.dsk",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "5a1e0a75d31d88c1ce7429fd333c268f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5dd8_nk.dsk",
            MediaType  = MediaType.DOS_525_DS_DD_8,
            Sectors    = 640,
            SectorSize = 512,
            Md5        = "5a1e0a75d31d88c1ce7429fd333c268f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5dd_c.dsk",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "c1a67b27bc76b64d0845965501b24120"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5dd_ck.dsk",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "c1a67b27bc76b64d0845965501b24120"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5dd_na.dsk",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "8a4d35dd0d97e6bca8b000170a43a56f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5dd_nak.dsk",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "8a4d35dd0d97e6bca8b000170a43a56f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5dd_n.dsk",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "c1a67b27bc76b64d0845965501b24120"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5dd_nk.dsk",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "c1a67b27bc76b64d0845965501b24120"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5hd_c.dsk",
            MediaType  = MediaType.DOS_525_HD,
            Sectors    = 2400,
            SectorSize = 512,
            Md5        = "1c28b4c3cdc1dbf19c24a5eca3891a87"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5hd_ck.dsk",
            MediaType  = MediaType.DOS_525_HD,
            Sectors    = 2400,
            SectorSize = 512,
            Md5        = "1c28b4c3cdc1dbf19c24a5eca3891a87"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5hd_na.dsk",
            MediaType  = MediaType.DOS_525_HD,
            Sectors    = 2400,
            SectorSize = 512,
            Md5        = "2ce745ac23712d3eb03d7a11ba933b12"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5hd_nak.dsk",
            MediaType  = MediaType.DOS_525_HD,
            Sectors    = 2400,
            SectorSize = 512,
            Md5        = "2ce745ac23712d3eb03d7a11ba933b12"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5hd_n.dsk",
            MediaType  = MediaType.DOS_525_HD,
            Sectors    = 2400,
            SectorSize = 512,
            Md5        = "1c28b4c3cdc1dbf19c24a5eca3891a87"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5hd_nk.dsk",
            MediaType  = MediaType.DOS_525_HD,
            Sectors    = 2400,
            SectorSize = 512,
            Md5        = "1c28b4c3cdc1dbf19c24a5eca3891a87"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5sd8_c.dsk",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            Md5        = "65ce0cd08d90c882df12637c9c72c1ba"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5sd8_ck.dsk",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            Md5        = "65ce0cd08d90c882df12637c9c72c1ba"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5sd8_na.dsk",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            Md5        = "6f5d09c13a7b481bad9ea78042e61e00"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5sd8_nak.dsk",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            Md5        = "6f5d09c13a7b481bad9ea78042e61e00"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5sd8_n.dsk",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            Md5        = "65ce0cd08d90c882df12637c9c72c1ba"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5sd8_nk.dsk",
            MediaType  = MediaType.DOS_525_SS_DD_8,
            Sectors    = 320,
            SectorSize = 512,
            Md5        = "65ce0cd08d90c882df12637c9c72c1ba"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5sd_c.dsk",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "412fdc582506c0d7e76735d403b30759"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5sd_ck.dsk",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "412fdc582506c0d7e76735d403b30759"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5sd_na.dsk",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "fd81fceb26bda5b02053c5c729a6f67f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5sd_nak.dsk",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "fd81fceb26bda5b02053c5c729a6f67f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5sd_n.dsk",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "412fdc582506c0d7e76735d403b30759"
        },
        new BlockImageTestExpected
        {
            TestFile   = "5sd_nk.dsk",
            MediaType  = MediaType.DOS_525_SS_DD_9,
            Sectors    = 360,
            SectorSize = 512,
            Md5        = "412fdc582506c0d7e76735d403b30759"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md1dd8.dsk",
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
            TestFile   = "md1dd.dsk",
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
            TestFile   = "md1dd_fdformat_f200.dsk",
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
            TestFile   = "md1dd_fdformat_f205.dsk",
            MediaType  = MediaType.Unknown,
            Sectors    = 410,
            SectorSize = 512,
            Md5        = "353f3c2125ab6f74e3a271b60ad34840",
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
            TestFile   = "md2dd_2m_fast.dsk",
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
            TestFile   = "md2dd_2m_max.dsk",
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
            TestFile   = "md2dd8.dsk",
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
            TestFile   = "md2dd.dsk",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "6213897b7dbf263f12abf76901d43862"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2dd_fdformat_f400.dsk",
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
            TestFile   = "md2dd_fdformat_f410.dsk",
            MediaType  = MediaType.Unknown,
            Sectors    = 820,
            SectorSize = 512,
            Md5        = "348d12add1ed226cd712a4a6a10d1a34",
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
            TestFile   = "md2dd_fdformat_f720.dsk",
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
            TestFile   = "md2dd_fdformat_f800.dsk",
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
            TestFile   = "md2dd_fdformat_f820.dsk",
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
            TestFile   = "md2dd_freedos_800s.dsk",
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
            TestFile   = "md2dd_maxiform_1640s.dsk",
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
            TestFile   = "md2dd_maxiform_840s.dsk",
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
            TestFile   = "md2dd_qcopy_1476s.dsk",
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
            TestFile   = "md2dd_qcopy_1600s.dsk",
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
            TestFile   = "md2dd_qcopy_1640s.dsk",
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
            TestFile   = "md2hd_2m_fast.dsk",
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
            TestFile   = "md2hd_2m_max.dsk",
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
            TestFile   = "md2hd.dsk",
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
            TestFile   = "md2hd_fdformat_f144.dsk",
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
            TestFile   = "md2hd_fdformat_f148.dsk",
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
            TestFile   = "md2hd_maxiform_2788s.dsk",
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
            TestFile   = "md2hd_xdf.dsk",
            MediaType  = MediaType.XDF_525,
            Sectors    = 3040,
            SectorSize = 512,
            Md5        = "d78dc81491edeec99aa202d02f3daf00"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_2m.dsk",
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
            TestFile   = "mf2dd_2m_fast.dsk",
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
            TestFile   = "mf2dd_2mgui.dsk",
            MediaType  = MediaType.Unknown,
            Sectors    = 9408,
            SectorSize = 128,
            Md5        = "beb782f6bc970e32ceef79cd112e2e48"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_2m_max.dsk",
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
            TestFile   = "mf2dd_c.dsk",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "2aefc1e97f29bf9982e0fd7091dfb9f5"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_ck.dsk",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "2aefc1e97f29bf9982e0fd7091dfb9f5"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd.dsk",
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
            TestFile   = "mf2dd_fdformat_800.dsk",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "2e69bbd591ab736e471834ae03dde9a6",
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
            TestFile   = "mf2dd_fdformat_820.dsk",
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
            TestFile   = "mf2dd_fdformat_f800.dsk",
            MediaType  = MediaType.CBM_35_DD,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "26532a62985b51a2c3b877a57f6d257b",
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
            TestFile   = "mf2dd_fdformat_f820.dsk",
            MediaType  = MediaType.FDFORMAT_35_DD,
            Sectors    = 1640,
            SectorSize = 512,
            Md5        = "a7771acff766557cc23b8c6943b588f9",
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
            TestFile   = "mf2dd_freedos_1600s.dsk",
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
            TestFile   = "mf2dd_maxiform_1600s.dsk",
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
            TestFile   = "mf2dd_na.dsk",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "e574be0d057f2ef775dfb685561d27cf"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_nak.dsk",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "e574be0d057f2ef775dfb685561d27cf"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_n.dsk",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "2aefc1e97f29bf9982e0fd7091dfb9f5"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_nk.dsk",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "2aefc1e97f29bf9982e0fd7091dfb9f5"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2dd_qcopy_1494s.dsk",
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
            TestFile   = "mf2dd_qcopy_1600s.dsk",
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
            TestFile   = "mf2dd_qcopy_1660s.dsk",
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
            TestFile   = "mf2ed_c.dsk",
            MediaType  = MediaType.ECMA_147,
            Sectors    = 5760,
            SectorSize = 512,
            Md5        = "e4746aa9629a2325c520db1c8a641ac6"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2ed_ck.dsk",
            MediaType  = MediaType.ECMA_147,
            Sectors    = 5760,
            SectorSize = 512,
            Md5        = "e4746aa9629a2325c520db1c8a641ac6"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2ed.dsk",
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
            TestFile   = "mf2ed_na.dsk",
            MediaType  = MediaType.ECMA_147,
            Sectors    = 5760,
            SectorSize = 512,
            Md5        = "42e73287b23ac985c9825466cae26859"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2ed_nak.dsk",
            MediaType  = MediaType.ECMA_147,
            Sectors    = 5760,
            SectorSize = 512,
            Md5        = "42e73287b23ac985c9825466cae26859"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2ed_n.dsk",
            MediaType  = MediaType.ECMA_147,
            Sectors    = 5760,
            SectorSize = 512,
            Md5        = "e4746aa9629a2325c520db1c8a641ac6"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2ed_nk.dsk",
            MediaType  = MediaType.ECMA_147,
            Sectors    = 5760,
            SectorSize = 512,
            Md5        = "e4746aa9629a2325c520db1c8a641ac6"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_2m.dsk",
            MediaType  = MediaType.Unknown,
            Sectors    = 3608,
            SectorSize = 512,
            Md5        = "2f6964d410b275c8e9f60fe2f24b361a"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_2m_fast.dsk",
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
            TestFile   = "mf2hd_2mgui.dsk",
            MediaType  = MediaType.Unknown,
            Sectors    = 15776,
            SectorSize = 128,
            Md5        = "786e45bbfcb369913968aa31365f00bb"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_2m_max.dsk",
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
            TestFile   = "mf2hd_c.dsk",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "003e9130d83a23018f488f9fa89cae5e"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_ck.dsk",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "003e9130d83a23018f488f9fa89cae5e"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_dmf.dsk",
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
            TestFile   = "mf2hd.dsk",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "00e61c06bf29f0c04a7eabe2dbd7efb6"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_fdformat_168.dsk",
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
            TestFile   = "mf2hd_fdformat_172.dsk",
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
            TestFile   = "mf2hd_fdformat_f168.dsk",
            MediaType  = MediaType.DMF,
            Sectors    = 3360,
            SectorSize = 512,
            Md5        = "7e3bf04f3660dd1052a335dc99441e44",
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
            TestFile   = "mf2hd_fdformat_f16.dsk",
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
            TestFile   = "mf2hd_fdformat_f172.dsk",
            MediaType  = MediaType.FDFORMAT_35_HD,
            Sectors    = 3444,
            SectorSize = 512,
            Md5        = "a58fd062f024b95714f1223a8bc2232f",
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
            TestFile   = "mf2hd_freedos_3360s.dsk",
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
            TestFile   = "mf2hd_freedos_3486s.dsk",
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
            TestFile   = "mf2hd_maxiform_3200s.dsk",
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
            TestFile   = "mf2hd_na.dsk",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "009cc68e28b2b13814d3afbec9d9e59f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_nak.dsk",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "009cc68e28b2b13814d3afbec9d9e59f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_n.dsk",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "003e9130d83a23018f488f9fa89cae5e"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_nk.dsk",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "003e9130d83a23018f488f9fa89cae5e"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_qcopy_2460s.dsk",
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
            TestFile   = "mf2hd_qcopy_2720s.dsk",
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
            TestFile   = "mf2hd_qcopy_2788s.dsk",
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
            TestFile   = "mf2hd_qcopy_2880s.dsk",
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
            TestFile   = "mf2hd_qcopy_2952s.dsk",
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
            TestFile   = "mf2hd_qcopy_2988s.dsk",
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
            TestFile   = "mf2hd_qcopy_3200s.dsk",
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
            TestFile   = "mf2hd_qcopy_3320s.dsk",
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
            TestFile   = "mf2hd_qcopy_3360s.dsk",
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
            TestFile   = "mf2hd_qcopy_3486s.dsk",
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
            TestFile   = "mf2hd_xdf_c.dsk",
            MediaType  = MediaType.XDF_35,
            Sectors    = 3680,
            SectorSize = 512,
            Md5        = "2770e5b1b7935ca6e9695a32008b936a"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_xdf_ck.dsk",
            MediaType  = MediaType.XDF_35,
            Sectors    = 3680,
            SectorSize = 512,
            Md5        = "2770e5b1b7935ca6e9695a32008b936a"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_xdf.dsk",
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
            TestFile   = "mf2hd_xdf_na.dsk",
            MediaType  = MediaType.XDF_35,
            Sectors    = 3680,
            SectorSize = 512,
            Md5        = "34b4bdab5fcc17076cceb7c1a39ea430"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_xdf_nak.dsk",
            MediaType  = MediaType.XDF_35,
            Sectors    = 3680,
            SectorSize = 512,
            Md5        = "34b4bdab5fcc17076cceb7c1a39ea430"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_xdf_n.dsk",
            MediaType  = MediaType.XDF_35,
            Sectors    = 3680,
            SectorSize = 512,
            Md5        = "2770e5b1b7935ca6e9695a32008b936a"
        },
        new BlockImageTestExpected
        {
            TestFile   = "mf2hd_xdf_nk.dsk",
            MediaType  = MediaType.XDF_35,
            Sectors    = 3680,
            SectorSize = 512,
            Md5        = "2770e5b1b7935ca6e9695a32008b936a"
        }
    };
}