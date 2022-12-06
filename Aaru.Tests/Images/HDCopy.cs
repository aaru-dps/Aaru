// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HDCopy.cs
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

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class HDCopy : BlockMediaImageTest
    {
        public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "HD-COPY");
        public override IMediaImage _plugin => new HdCopy();

        public override BlockImageTestExpected[] Tests => new[]
        {
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0000.IMG.lz",
                MediaType  = MediaType.DOS_35_HD,
                Sectors    = 2880,
                SectorSize = 512,
                MD5        = "0fca5f810ce3179bbc67c7967370f1c2"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0001.IMG.lz",
                MediaType  = MediaType.CBM_35_DD,
                Sectors    = 1600,
                SectorSize = 512,
                MD5        = "7abbee308589a097e434c52099b1905c"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0009.IMG.lz",
                MediaType  = MediaType.DOS_35_HD,
                Sectors    = 2880,
                SectorSize = 512,
                MD5        = "9fbe4254ed34991d38a4cde57e867360",
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
                MD5        = "f42a1f0d64378f5fca289d60f65cac91"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0024.IMG.lz",
                MediaType  = MediaType.DOS_35_HD,
                Sectors    = 2880,
                SectorSize = 512,
                MD5        = "72abac3d635b24555b3b8fe4d71c6c50",
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
                MediaType  = MediaType.DOS_35_DS_DD_8,
                Sectors    = 1280,
                SectorSize = 512,
                MD5        = "758a18f67c5b609ff18a49910df3ac8e"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0030.IMG.lz",
                MediaType  = MediaType.DOS_35_DS_DD_9,
                Sectors    = 1440,
                SectorSize = 512,
                MD5        = "dacbe6b6677a76004bc0f8fbeb6c3a83",
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
                TestFile   = "DSKA0045.IMG.lz",
                MediaType  = MediaType.DOS_525_HD,
                Sectors    = 2400,
                SectorSize = 512,
                MD5        = "1a642606e79b5fb8e41536c320ba81ea",
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
                MD5        = "66a8bec544008abcd735e51fd19ed00b",
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
                MD5        = "2840b7b7fb457cbabe3cc8b2f50411d9",
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
                MD5        = "28c7d3ee80055b4fd19cd154ac610a3c",
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
                MD5        = "19c6430529da8f390bf2cbfc0eaf21e3",
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
                MD5        = "0c7ba41e67b07f0f203165b10c0a4e89",
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
                MD5        = "63267008556ad5704b2cf91049e5e255",
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
                MD5        = "015f4f812fd5b03741e3dcad534a4a8d",
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
                MD5        = "60ebe39494bccd4d74daaa47003395e4",
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
                MD5        = "00260b1a20f19fa618d2acdee59dc471",
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
                TestFile   = "DSKA0055.IMG.lz",
                MediaType  = MediaType.Unknown,
                Sectors    = 3280,
                SectorSize = 512,
                MD5        = "dfb67a1f3c1fe596db30eeac92887583",
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
                MD5        = "fa5a213b8827cf7341b946e3fd1866cd",
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
                MD5        = "f59f095594fc408cb2de1a977538afad",
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
                MD5        = "1826a1a29d65411a2d21ed28998d7c43",
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
                MD5        = "16a5d02a2e73be36fec6abb8a671c13e",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 3528
                    }
                }
            },
            /* TODO: Open error
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0060.IMG.lz",
                MediaType  = MediaType.CD,
                Sectors    = 0,
                SectorSize = 512,
                MD5        = "UNKNOWN",
            },
            */ new BlockImageTestExpected
            {
                TestFile   = "DSKA0069.IMG.lz",
                MediaType  = MediaType.DOS_35_DS_DD_9,
                Sectors    = 1440,
                SectorSize = 512,
                MD5        = "4c45b8baeef58e9ed76eb6782dd8535b",
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
                TestFile   = "DSKA0075.IMG.lz",
                MediaType  = MediaType.DOS_525_DS_DD_8,
                Sectors    = 640,
                SectorSize = 512,
                MD5        = "3c1e16778895a28f15d119c426ed4332",
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
                MD5        = "dc2b7b7eb6d83ce25a6f51d1e457ca24",
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
                TestFile   = "DSKA0078.IMG.lz",
                MediaType  = MediaType.DOS_525_HD,
                Sectors    = 2400,
                SectorSize = 512,
                MD5        = "e800a80461a46369564e3bcdb8e54dbc"
            },
            new BlockImageTestExpected
            {
                TestFile   = "DSKA0080.IMG.lz",
                MediaType  = MediaType.DOS_35_DS_DD_9,
                Sectors    = 1440,
                SectorSize = 512,
                MD5        = "4e502fab83012d988d9c915cfea00901",
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
                TestFile   = "DSKA0082.IMG.lz",
                MediaType  = MediaType.DOS_35_HD,
                Sectors    = 2880,
                SectorSize = 512,
                MD5        = "323322148c4c7394a92fa6a73542e32a",
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
                TestFile   = "DSKA0084.IMG.lz",
                MediaType  = MediaType.DMF,
                Sectors    = 3360,
                SectorSize = 512,
                MD5        = "38d8177e53175fb2cd60362339a548c1",
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
                TestFile   = "DSKA0107.IMG.lz",
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
                TestFile   = "DSKA0108.IMG.lz",
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
                TestFile   = "DSKA0111.IMG.lz",
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
                TestFile   = "DSKA0112.IMG.lz",
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
                TestFile   = "DSKA0113.IMG.lz",
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
                TestFile   = "DSKA0114.IMG.lz",
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
                TestFile   = "DSKA0115.IMG.lz",
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
                TestFile   = "DSKA0116.IMG.lz",
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
                TestFile   = "DSKA0117.IMG.lz",
                MediaType  = MediaType.Unknown,
                Sectors    = 3280,
                SectorSize = 512,
                MD5        = "56471a253f4d6803b634e2bbff6c0931",
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
                TestFile   = "DSKA0122.IMG.lz",
                MediaType  = MediaType.DOS_525_DS_DD_8,
                Sectors    = 640,
                SectorSize = 512,
                MD5        = "9f91d2cfe918c6701d6b267294b092bc",
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
                MD5        = "28f3cda83fa1e22a420e06704abc6139",
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
                MD5        = "b057202adb98964e8f630a3299e86490",
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
                MD5        = "387f7d0468559b619e929db4451b3074",
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
                MD5        = "c64ef58dc6d875b9f6e7c0a7362c6832",
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
                TestFile   = "DSKA0164.IMG.lz",
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
                TestFile   = "DSKA0168.IMG.lz",
                MediaType  = MediaType.DOS_525_HD,
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
                TestFile   = "DSKA0169.IMG.lz",
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
                TestFile   = "DSKA0170.IMG.lz",
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
                TestFile   = "DSKA0171.IMG.lz",
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
                TestFile   = "DSKA0174.IMG.lz",
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
                TestFile   = "DSKA0175.IMG.lz",
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
                TestFile   = "DSKA0176.IMG.lz",
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
                TestFile   = "DSKA0177.IMG.lz",
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
                TestFile   = "DSKA0180.IMG.lz",
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
                TestFile   = "DSKA0181.IMG.lz",
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
                TestFile   = "DSKA0182.IMG.lz",
                MediaType  = MediaType.FDFORMAT_35_HD,
                Sectors    = 3444,
                SectorSize = 512,
                MD5        = "36dd03967a2a3369538cad29b8b74b71",
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
                MD5        = "4f5c02448e75bbc086e051c728414513",
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
                MD5        = "f8f951518283d395b6dd662a303e088d",
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
                MD5        = "e6a87175b9dbac1916a735eb2418abd0",
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
                MD5        = "2f576fb4c408d16fa49ef3093a2a3969",
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
                MD5        = "dab393b265b3b8d82b0eb920bc316299",
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
                MD5        = "7c13a4f0c223d30916ba218186a42fad",
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
                MD5        = "e3797cf190f00a7205c0cc68e3977e04",
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
                MD5        = "e9de4f065fd056b90b16c3464d501daa",
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
                MD5        = "cb7cedbe89c2859779f921c44ff0807a",
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
                MD5        = "db5f924b17bd7f1bf29784ede7b45dbb",
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
                MD5        = "2df2eaef283e5be894c0be29ba2feae1",
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
                MD5        = "57af8042541c13d673ebb04bcdbca81b",
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
                MD5        = "afcf6b2f8d762295ea8450aadf8b4319",
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
                TestFile   = "DSKA0282.IMG.lz",
                MediaType  = MediaType.DOS_525_DS_DD_8,
                Sectors    = 640,
                SectorSize = 512,
                MD5        = "ecfc55db0d383c1a2c5e639014954f85",
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
                MD5        = "0f11deec629979e6ea351ac18479c805"
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
                TestFile   = "DSKA0284.IMG.lz",
                MediaType  = MediaType.DOS_525_DS_DD_9,
                Sectors    = 720,
                SectorSize = 512,
                MD5        = "0f11deec629979e6ea351ac18479c805"
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
                TestFile   = "DSKA0285.IMG.lz",
                MediaType  = MediaType.Unknown,
                Sectors    = 840,
                SectorSize = 512,
                MD5        = "976b335e4fe2356d16f45c123330249c",
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
                TestFile   = "DSKA0301.IMG.lz",
                MediaType  = MediaType.DOS_525_DS_DD_8,
                Sectors    = 640,
                SectorSize = 512,
                MD5        = "9812da7e10dc3ff388907c135360b8bc",
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
                MD5        = "07eefdd2a6261be61af5b29de9dd56ee",
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
                MD5        = "6539a5d8ed493940e6a97e39eae0ca3e",
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
                MD5        = "01f01805a6b22cad7e82a9cf614b8040",
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
                MD5        = "fe4ce9782a5a42bf2bf6b41f7a51d744",
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
                TestFile   = "DSKA0311.IMG.lz",
                MediaType  = MediaType.FDFORMAT_35_HD,
                Sectors    = 3444,
                SectorSize = 512,
                MD5        = "f8514aa0d100ad7eb14ef0f472416b67",
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
                MD5        = "5487ed4ca8e165d10ac0f04d8b96bbce",
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
                MD5        = "0c438ab43509da1863b1fecff8d806aa",
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
                MD5        = "cd0a831f1668b6ccd99d284513b86461",
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
                MD5        = "2a09063703e21f2440d2a9128c29147f",
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
                MD5        = "ca24bcbfe70de3c1fd4955a6c12b9a0f",
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
                MD5        = "ca24bcbfe70de3c1fd4955a6c12b9a0f",
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
                TestFile   = "TFULL.IMG.lz",
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
                TestFile   = "TFULLPAS.IMG.lz",
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
                TestFile   = "TNORMAL.IMG.lz",
                MediaType  = MediaType.DOS_35_DS_DD_9,
                Sectors    = 1440,
                SectorSize = 512,
                MD5        = "387f7d0468559b619e929db4451b3074",
                Partitions = new[]
                {
                    new BlockPartitionVolumes
                    {
                        Start  = 0,
                        Length = 1440
                    }
                }
            }
        };
    }
}