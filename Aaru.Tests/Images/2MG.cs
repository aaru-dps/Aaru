// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 2MG.cs
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
public class Apple2Mg : BlockMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "2mg");
    public override IMediaImage Plugin    => new DiscImages.Apple2Mg();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "blank140.2mg.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "7db5d585270ab858043d50e60068d45f"
        },
        new BlockImageTestExpected
        {
            TestFile   = "dos32.2mg.lz",
            MediaType  = MediaType.Apple32SS,
            Sectors    = 455,
            SectorSize = 256,
            Md5        = "906c1bdbf76bf089ea47aae98151df5d",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 455
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "dos32_alt.2mg.lz",
            MediaType  = MediaType.Apple32SS,
            Sectors    = 455,
            SectorSize = 256,
            Md5        = "76f8fe4c5bc1976f99641ad7cdf53109",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 455
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "dos33_dic.2mg.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "0ffcbd4180306192726926b43755db2f",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 560
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "dos33-do.2mg.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "91d020725d081500caa1fd8aad959397",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 560
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "dos33-nib.2mg.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "91d020725d081500caa1fd8aad959397",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 560
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "dos33_nib.2mg.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "0ffcbd4180306192726926b43755db2f",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 560
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "dos33-po.2mg.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "91d020725d081500caa1fd8aad959397",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 560
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "dos33_po.2mg.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "0ffcbd4180306192726926b43755db2f",
            Partitions = new[]
            {
                new BlockPartitionVolumes
                {
                    Start  = 0,
                    Length = 560
                }
            }
        },
        new BlockImageTestExpected
        {
            TestFile   = "hfs1440.2mg.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "535648d1f9838b695403f2f48d5ac94c"
        },
        new BlockImageTestExpected
        {
            TestFile   = "hfs800_dic.2mg.lz",
            MediaType  = MediaType.AppleSonyDS,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "2762f41d0379b476042fc62891baac84"
        },
        new BlockImageTestExpected
        {
            TestFile   = "hfs_do.2mg.lz",
            MediaType  = MediaType.AppleSonyDS,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "2762f41d0379b476042fc62891baac84"
        },
        new BlockImageTestExpected
        {
            TestFile   = "hfs_po.2mg.lz",
            MediaType  = MediaType.AppleSonyDS,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "2762f41d0379b476042fc62891baac84"
        },
        new BlockImageTestExpected
        {
            TestFile   = "modified_do.2mg.lz",
            MediaType  = MediaType.AppleSonyDS,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "b748f6df3e60e7169d42ec6fcc857ea4"
        },
        new BlockImageTestExpected
        {
            TestFile   = "modified_po.2mg.lz",
            MediaType  = MediaType.AppleSonyDS,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "b748f6df3e60e7169d42ec6fcc857ea4"
        },
        new BlockImageTestExpected
        {
            TestFile   = "pascal800_do.2mg.lz",
            MediaType  = MediaType.AppleSonyDS,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "dbd0ec8a3126236910709faf923adcf2"
        },
        new BlockImageTestExpected
        {
            TestFile   = "pascal800_p.2mg.lz",
            MediaType  = MediaType.AppleSonyDS,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "dbd0ec8a3126236910709faf923adcf2"
        },
        new BlockImageTestExpected
        {
            TestFile   = "pascal_dic.2mg.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "4c4926103a32ac15f7e430ec3ced4be5"
        },
        new BlockImageTestExpected
        {
            TestFile   = "pascal_do.2mg.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "4c4926103a32ac15f7e430ec3ced4be5"
        },
        new BlockImageTestExpected
        {
            TestFile   = "pascal_nib.2mg.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "4c4926103a32ac15f7e430ec3ced4be5"
        },
        new BlockImageTestExpected
        {
            TestFile   = "pascal_po.2mg.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "4c4926103a32ac15f7e430ec3ced4be5"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodos1440.2mg.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "eb9b60c78b30d2b6541ed0781944b6da"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodos1440_po.2mg.lz",
            MediaType  = MediaType.DOS_35_HD,
            Sectors    = 2880,
            SectorSize = 512,
            Md5        = "1fe841b418ede51133878641e01544b5"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodos5mb.2mg.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 10240,
            SectorSize = 512,
            Md5        = "b156441e159a625ee00a0659dfb6e2f8"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodos5m_dic.2mg.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 10240,
            SectorSize = 512,
            Md5        = "b156441e159a625ee00a0659dfb6e2f8"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodos800_dic.2mg.lz",
            MediaType  = MediaType.AppleSonyDS,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "fcf747bd356b48d442ff74adb8f3516b"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodos800_do.2mg.lz",
            MediaType  = MediaType.AppleSonyDS,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "fcf747bd356b48d442ff74adb8f3516b"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodos800_po.2mg.lz",
            MediaType  = MediaType.AppleSonyDS,
            Sectors    = 1600,
            SectorSize = 512,
            Md5        = "fcf747bd356b48d442ff74adb8f3516b"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodos_dic.2mg.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "11ef56c80c94347d2e3f921d5c36c8de"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodos_do.2mg.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "11ef56c80c94347d2e3f921d5c36c8de"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodos_nib.2mg.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "11ef56c80c94347d2e3f921d5c36c8de"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodos_po.2mg.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "11ef56c80c94347d2e3f921d5c36c8de"
        },
        new BlockImageTestExpected
        {
            TestFile   = "prodos.2mg.lz",
            MediaType  = MediaType.Apple33SS,
            Sectors    = 560,
            SectorSize = 256,
            Md5        = "6f692a8fadfaa243d9f2d8d41f0e4cad"
        }
    };
}