// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : D88.cs
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
public class D88 : BlockMediaImageTest
{
    public override string      DataFolder => Path.Combine(Consts.TestFilesRoot, "Media image formats", "D88");
    public override IMediaImage Plugin     => new Aaru.Images.D88();

    public override BlockImageTestExpected[] Tests => new[]
    {
        new BlockImageTestExpected
        {
            TestFile   = "1942 (1987)(ASCII)(JP).d77.lz",
            MediaType  = MediaType.NEC_525_SS,
            Sectors    = 1280,
            SectorSize = 256,
            Md5        = "a4103c39cd7fd9fc3de8418dfcf22364"
        },
        new BlockImageTestExpected
        {
            TestFile   = "'Ashe (1988)(Quasar)(Disk 4 of 4)(User Disk).d88.lz",
            MediaType  = MediaType.NEC_525_SS,
            Sectors    = 1280,
            SectorSize = 256,
            Md5        = "b948048c03e0b3d34d77f5c9dced0b41"
        },
        new BlockImageTestExpected
        {
            TestFile   = "Crimsin (1988)(Xtalsoft)(Disk 3 of 3).d88.lz",
            MediaType  = MediaType.NEC_525_SS,
            Sectors    = 1280,
            SectorSize = 256,
            Md5        = "f91152fab791d4dc0677a289d90478a5"
        },
        new BlockImageTestExpected
        {
            TestFile   = "Dragon Slayer (1986)(Falcom - Login)(JP).d88.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 411,
            SectorSize = 256,
            Md5        = "39b01df04a6312b09f1b83c9f3a46b22"
        },
        new BlockImageTestExpected
        {
            TestFile   = "D-Side - Lagrange L-2 Part II (1986)(Compaq)(JP).d88.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1440,
            SectorSize = 256,
            Md5        = "ef775ec1f41b8b725ea83ec8c5ca04e2"
        },
        new BlockImageTestExpected
        {
            TestFile   = "File Master FM, The v1.01 (1986)(Kyoto Media)(JP).d77.lz",
            MediaType  = MediaType.NEC_525_SS,
            Sectors    = 1280,
            SectorSize = 256,
            Md5        = "5c2b22f824524cd6c539aaeb2ecb84cd"
        },
        new BlockImageTestExpected
        {
            TestFile   = "Gandhara (1987)(Enix)(JP).d88.lz",
            MediaType  = MediaType.NEC_525_SS,
            Sectors    = 1280,
            SectorSize = 256,
            Md5        = "6bddf3dd32877f7b552cbf9da6b89f76"
        },
        new BlockImageTestExpected
        {
            TestFile   = "Might & Magic (198x)(Star Craft)(Disk 1 of 3)(Disk A).d88.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 4033,
            SectorSize = 128,
            Md5        = "003cd0292879733b6eab7ca79ab9cfeb"
        },
        new BlockImageTestExpected
        {
            TestFile   = "msdos33d_md2dd.d88.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "acb738a5a945e4e2ba1504a14a529933"
        },
        new BlockImageTestExpected
        {
            TestFile   = "msdos33d_md2hd.d88.lz",
            MediaType  = MediaType.SHARP_525,
            Sectors    = 1232,
            SectorSize = 1024,
            Md5        = "106068dbdf13803979c7bbb63612f43d"
        },
        new BlockImageTestExpected
        {
            TestFile   = "msdos50_epson_md2dd.d88.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "be916f25847b9cfc9776d88cc150ae7e",
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
            TestFile   = "msdos50_epson_md2hd.d88.lz",
            MediaType  = MediaType.SHARP_525,
            Sectors    = 1232,
            SectorSize = 1024,
            Md5        = "ccc7f98e216db35c2b7a08634a9f3e20",
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
            TestFile   = "msdos50_md2dd.d88.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "7a3332e82b0fe8c5673a2615f6c0b9a2"
        },
        new BlockImageTestExpected
        {
            TestFile   = "msdos50_md2hd.d88.lz",
            MediaType  = MediaType.SHARP_525,
            Sectors    = 1232,
            SectorSize = 1024,
            Md5        = "62f5be96a8b8ccab9ee4aebf557cfcf7",
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
            TestFile   = "msdos62_md2dd.d88.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1440,
            SectorSize = 512,
            Md5        = "07fb4c225d4b5a2e2a1046ae66fc153c"
        },
        new BlockImageTestExpected
        {
            TestFile   = "msdos62_md2hd.d88.lz",
            MediaType  = MediaType.SHARP_525,
            Sectors    = 1232,
            SectorSize = 1024,
            Md5        = "1f73980e45a384bed331eaa33c9ef65b",
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
            TestFile   = "R-Type (1988)(Irem)(Disk 1 of 2).d88.lz",
            MediaType  = MediaType.Unknown,
            Sectors    = 1284,
            SectorSize = 1024,
            Md5        = "9d675e5147b55cee0b2bc05476eef825"
        },
        new BlockImageTestExpected
        {
            TestFile   = "Towns System Software v1.1L30 (1992)(Fujitsu)(JP).d88.lz",
            MediaType  = MediaType.SHARP_525,
            Sectors    = 1232,
            SectorSize = 1024,
            Md5        = "bb48546ced9c61462e1c89dca4987143",
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
            TestFile   = "Visual Instrument Player (198x)(Kamiya)(JP)(Disk 1 of 2).d88.lz",
            MediaType  = MediaType.NEC_525_SS,
            Sectors    = 1280,
            SectorSize = 256,
            Md5        = "c7df67f4e66dad658fe856d3c8b36c7a"
        }
    };
}