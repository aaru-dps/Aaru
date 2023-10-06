// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DiskDupe.cs
// Author(s)      : Michael Drüing <michael@drueing.de>
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
// Copyright © 2021-2023 Michael Drüing
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using NUnit.Framework;

namespace Aaru.Tests.Images;

[TestFixture]
public class DiskDupe : BlockMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "DiskDupe");
    public override IMediaImage Plugin     => new Aaru.Images.DiskDupe();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "1.DDI.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "0d5735269cb9c3d0e63ec9ccfb38e4e2"
            /* TODO: IndexOutOfRangeException
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
            TestFile   = "2.DDI.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "fa639b4bd96d2fb7be33a1725e9c7c4f"
            /* TODO: IndexOutOfRangeException
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
            TestFile   = "3.DDI.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "f63e676310b2f1a9e44e9a471c7cf1f2",
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
            TestFile   = "DSKA0000.DDI.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "0fca5f810ce3179bbc67c7967370f1c2"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0009.DDI.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "9fbe4254ed34991d38a4cde57e867360",
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
            TestFile   = "DSKA0024.DDI.lz",
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
            TestFile   = "DSKA0052.DDI.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "015f4f812fd5b03741e3dcad534a4a8d",
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
            TestFile   = "DSKA0076.DDI.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "dc2b7b7eb6d83ce25a6f51d1e457ca24",
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
            TestFile   = "DSKA0078.DDI.lz",
            MediaType  = MediaType.DOS_525_HD,
            Sectors    = 2400,
            SectorSize = 512,
            Md5        = "410203d55d05581ad377fc8ffda0f4e8"
        },
        new BlockImageTestExpected
        {
            TestFile   = "DSKA0080.DDI.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "1cc0a19579c841ace37c36ef1cd57a05",
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
            TestFile   = "DSKA0082.DDI.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "323322148c4c7394a92fa6a73542e32a",
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
            TestFile   = "DSKA0111.DDI.lz",
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
            TestFile   = "DSKA0123.DDI.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "28f3cda83fa1e22a420e06704abc6139",
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
            TestFile   = "DSKA0124.DDI.lz",
            MediaType  = MediaType.DOS_525_HD,
            Sectors    = 2400,
            SectorSize = 512,
            Md5        = "b057202adb98964e8f630a3299e86490",
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
            TestFile   = "DSKA0125.DDI.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "d987313a46843017e906ee122163ded6",
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
            TestFile   = "DSKA0126.DDI.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "c64ef58dc6d875b9f6e7c0a7362c6832",
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
            TestFile   = "DSKA0163.DDI.lz",
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
            TestFile   = "DSKA0166.DDI.lz",
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
            TestFile   = "DSKA0168.DDI.lz",
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
            TestFile   = "DSKA0169.DDI.lz",
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
            TestFile   = "DSKA0283.DDI.lz",
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
            TestFile   = "DSKA0287.DDI.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "0e002201126260afa26b03df227175d7",
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
            TestFile   = "DSKA0302.DDI.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "07eefdd2a6261be61af5b29de9dd56ee",
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
            TestFile   = "DSKA0304.DDI.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "b7c377c7456071b7e886210d9b002bf3",
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
            TestFile   = "DSKA0305.DDI.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "fe4ce9782a5a42bf2bf6b41f7a51d744",
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
            TestFile   = "DSKA0314.DDI.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "b28f4850eaca9909db3aa8d9b185d1a2",
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
            TestFile   = "DSKA0316.DDI.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "0c438ab43509da1863b1fecff8d806aa",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2880
                }
            }
        },

        // TODO: CP/M false positive
        new BlockImageTestExpected
        {
            TestFile   = "md2dd.ddi.lz",
            MediaType  = MediaType.DOS_525_DS_DD_9,
            Sectors    = 720,
            SectorSize = 512,
            Md5        = "6715d0ed2097a762e24e64165bd6c801"
        },
        new BlockImageTestExpected
        {
            TestFile   = "md2hd.ddi.lz",
            MediaType  = MediaType.DOS_525_HD,
            Sectors    = 2400,
            SectorSize = 512,
            Md5        = "834cf0380eba331b4dc43ad55edd42a6",
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
            TestFile   = "mf2dd.ddi.lz",
            MediaType  = MediaType.DOS_35_DS_DD_9,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "3201d13f82cb3d933158d2c5208c20a1",
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
            TestFile   = "mf2hd_alt.ddi.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "b4b8eea68483ad5ba983c865e93f2ec6",
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
            TestFile   = "mf2hd.ddi.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "3335dc14ff1efa58d410afc045a9b425",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 2880
                }
            }
        }
    };
}