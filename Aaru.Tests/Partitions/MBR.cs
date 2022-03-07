// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MBR.cs
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

namespace Aaru.Tests.Partitions;

using System.IO;
using Aaru.CommonTypes;
using NUnit.Framework;

[TestFixture]
public class Mbr : PartitionSchemeTest
{
    public override string DataFolder =>
        Path.Combine(Consts.TEST_FILES_ROOT, "Partitioning schemes", "Master Boot Record");

    public override PartitionTest[] Tests => new[]
    {
        new PartitionTest
        {
            TestFile = "concurrentdos_6.0.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 100800,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 1008,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 99792,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 102816,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 100800,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 202608,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 303408,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 352800,
                    Type     = "0x04"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "darwin_1.4.1.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 409248,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 409248,
                    Type     = "0x07"
                },
                new Partition
                {
                    Length   = 204624,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 818496,
                    Type     = "0xA8"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "darwin_6.0.2.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 204561,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0xA8"
                },
                new Partition
                {
                    Length   = 81648,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 204624,
                    Type     = "0xAB"
                },
                new Partition
                {
                    Length   = 245952,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 286272,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 488880,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 532224,
                    Type     = "0x06"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "darwin_8.0.1.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 150000,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 176000,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 150063,
                    Type     = "0xAF"
                },
                new Partition
                {
                    Length   = 350000,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 326063,
                    Type     = "0xA8"
                },
                new Partition
                {
                    Length   = 347937,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 676063,
                    Type     = "0x0C"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "drdos_3.40.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 100800,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 1008,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 402129,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 101871,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 152145,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 504063,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 365841,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 656271,
                    Type     = "0x06"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "drdos_3.41.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 126945,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 124929,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 127071,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 101745,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 252063,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 668241,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 353871,
                    Type     = "0x06"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "drdos_5.00.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 128016,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 124992,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 99729,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 253071,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 100737,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 352863,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 313425,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 453663,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 254961,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 767151,
                    Type     = "0x06"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "drdos_6.00.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 101745,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 18081,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 102879,
                    Type     = "0x01"
                },
                new Partition
                {
                    Length   = 130977,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 121023,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 202545,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 252063,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 567441,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 454671,
                    Type     = "0x06"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "drdos_7.02.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 102753,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 307377,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 102879,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 384993,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 410319,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 17073,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 795375,
                    Type     = "0x01"
                },
                new Partition
                {
                    Length   = 209601,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 812511,
                    Type     = "0x06"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "drdos_7.03.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 202545,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 141057,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 202671,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 152145,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 352863,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 364833,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 505071,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 152145,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 869967,
                    Type     = "0x06"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "drdos_8.0.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 205569,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 138033,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 205695,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 303345,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 352863,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 249921,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 656271,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 115857,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 906255,
                    Type     = "0x06"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "linux.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 20480,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 2048,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 40960,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 22528,
                    Type     = "0x24"
                },
                new Partition
                {
                    Length   = 61440,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 65536,
                    Type     = "0xA7"
                },
                new Partition
                {
                    Length   = 81920,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 129024,
                    Type     = "0x42"
                },
                new Partition
                {
                    Length   = 49152,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 212992,
                    Type     = "0x83"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "macosx_10.3.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 204800,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 8,
                    Type     = "0xA8"
                },
                new Partition
                {
                    Length   = 102400,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 204816,
                    Type     = "0xAF"
                },
                new Partition
                {
                    Length   = 102400,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 307224,
                    Type     = "0x0B"
                },
                new Partition
                {
                    Length   = 204800,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 409632,
                    Type     = "0xAF"
                },
                new Partition
                {
                    Length   = 204800,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 614440,
                    Type     = "0xAF"
                },
                new Partition
                {
                    Length   = 204752,
                    Offset   = 0,
                    Sequence = 5,
                    Size     = 0,
                    Start    = 819248,
                    Type     = "0xAF"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "macosx_10.4.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 102400,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0xAF"
                },
                new Partition
                {
                    Length   = 204800,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 102501,
                    Type     = "0xAF"
                },
                new Partition
                {
                    Length   = 204800,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 307314,
                    Type     = "0x0B"
                },
                new Partition
                {
                    Length   = 204800,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 512127,
                    Type     = "0xA8"
                },
                new Partition
                {
                    Length   = 102400,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 716940,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 204622,
                    Offset   = 0,
                    Sequence = 5,
                    Size     = 0,
                    Start    = 819378,
                    Type     = "0xAF"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "msdos_3.30a.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 65583,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 131103,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 196623,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 262143,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 5,
                    Size     = 0,
                    Start    = 327663,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 6,
                    Size     = 0,
                    Start    = 393183,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 7,
                    Size     = 0,
                    Start    = 458703,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 8,
                    Size     = 0,
                    Start    = 524223,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 9,
                    Size     = 0,
                    Start    = 589743,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 10,
                    Size     = 0,
                    Start    = 655263,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 11,
                    Size     = 0,
                    Start    = 720783,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 12,
                    Size     = 0,
                    Start    = 786303,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 13,
                    Size     = 0,
                    Start    = 851823,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 14,
                    Size     = 0,
                    Start    = 917343,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 39249,
                    Offset   = 0,
                    Sequence = 15,
                    Size     = 0,
                    Start    = 982863,
                    Type     = "0x04"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "msdos_5.00.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 102753,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 31185,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 102879,
                    Type     = "0x01"
                },
                new Partition
                {
                    Length   = 41265,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 134127,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 51345,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 175455,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 61425,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 226863,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 72513,
                    Offset   = 0,
                    Sequence = 5,
                    Size     = 0,
                    Start    = 288351,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 82593,
                    Offset   = 0,
                    Sequence = 6,
                    Size     = 0,
                    Start    = 360927,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 92673,
                    Offset   = 0,
                    Sequence = 7,
                    Size     = 0,
                    Start    = 443583,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 102753,
                    Offset   = 0,
                    Sequence = 8,
                    Size     = 0,
                    Start    = 536319,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 112833,
                    Offset   = 0,
                    Sequence = 9,
                    Size     = 0,
                    Start    = 639135,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 122913,
                    Offset   = 0,
                    Sequence = 10,
                    Size     = 0,
                    Start    = 752031,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 134001,
                    Offset   = 0,
                    Sequence = 11,
                    Size     = 0,
                    Start    = 875007,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 13041,
                    Offset   = 0,
                    Sequence = 12,
                    Size     = 0,
                    Start    = 1009071,
                    Type     = "0x01"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "msdos_6.00.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 51345,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 72513,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 51471,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 92673,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 124047,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 112833,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 216783,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 134001,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 329679,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 154161,
                    Offset   = 0,
                    Sequence = 5,
                    Size     = 0,
                    Start    = 463743,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 178353,
                    Offset   = 0,
                    Sequence = 6,
                    Size     = 0,
                    Start    = 617967,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 184401,
                    Offset   = 0,
                    Sequence = 7,
                    Size     = 0,
                    Start    = 796383,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 41265,
                    Offset   = 0,
                    Sequence = 8,
                    Size     = 0,
                    Start    = 980847,
                    Type     = "0x04"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "msdos_6.20.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 205569,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 225729,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 205695,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 245889,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 431487,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 267057,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 677439,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 61425,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 944559,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 16065,
                    Offset   = 0,
                    Sequence = 5,
                    Size     = 0,
                    Start    = 1006047,
                    Type     = "0x01"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "msdos_6.21.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 205569,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 225729,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 205695,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 245889,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 431487,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 267057,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 677439,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 51345,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 944559,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 6993,
                    Offset   = 0,
                    Sequence = 5,
                    Size     = 0,
                    Start    = 995967,
                    Type     = "0x01"
                },
                new Partition
                {
                    Length   = 19089,
                    Offset   = 0,
                    Sequence = 6,
                    Size     = 0,
                    Start    = 1003023,
                    Type     = "0x01"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "msdos_6.22.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 245889,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 205569,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 246015,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 307377,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 451647,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 225729,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 759087,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 37233,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 984879,
                    Type     = "0x04"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "multiuserdos_7.22r04.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 152145,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 99729,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 152271,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 202545,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 252063,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 1953,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 454671,
                    Type     = "0x01"
                },
                new Partition
                {
                    Length   = 565425,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 456687,
                    Type     = "0x06"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "novelldos_7.00.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 252945,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 4977,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 253071,
                    Type     = "0x01"
                },
                new Partition
                {
                    Length   = 202545,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 352863,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 348705,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 555471,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 117873,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 904239,
                    Type     = "0x06"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "opendos_7.01.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 307377,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 4977,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 307503,
                    Type     = "0x01"
                },
                new Partition
                {
                    Length   = 40257,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 312543,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 202545,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 352863,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 466641,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 555471,
                    Type     = "0x06"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "parted.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 67584,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 4096,
                    Type     = "0x83"
                },
                new Partition
                {
                    Length   = 59392,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 73728,
                    Type     = "0x07"
                },
                new Partition
                {
                    Length   = 129024,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 133120,
                    Type     = "0x06"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "pcdos_2000.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 205569,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 225729,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 205695,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 245889,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 431487,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 287217,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 677439,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 57393,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 964719,
                    Type     = "0x04"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "pcdos_2.00.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 1022111,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 1,
                    Type     = "0x01"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "pcdos_2.10.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 1022111,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 1,
                    Type     = "0x01"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "pcdos_3.00.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 66465,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x01"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "pcdos_3.10.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 66465,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x01"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "pcdos_3.30.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 65583,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 131103,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 196623,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 262143,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 5,
                    Size     = 0,
                    Start    = 327663,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 6,
                    Size     = 0,
                    Start    = 393183,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 7,
                    Size     = 0,
                    Start    = 458703,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 8,
                    Size     = 0,
                    Start    = 524223,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 9,
                    Size     = 0,
                    Start    = 589743,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 10,
                    Size     = 0,
                    Start    = 655263,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 11,
                    Size     = 0,
                    Start    = 720783,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 12,
                    Size     = 0,
                    Start    = 786303,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 13,
                    Size     = 0,
                    Start    = 851823,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 65457,
                    Offset   = 0,
                    Sequence = 14,
                    Size     = 0,
                    Start    = 917343,
                    Type     = "0x04"
                },
                new Partition
                {
                    Length   = 39249,
                    Offset   = 0,
                    Sequence = 15,
                    Size     = 0,
                    Start    = 982863,
                    Type     = "0x04"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "pcdos_4.00.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 205569,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 25137,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 205695,
                    Type     = "0x01"
                },
                new Partition
                {
                    Length   = 245889,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 230895,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 307377,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 476847,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 237825,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 784287,
                    Type     = "0x06"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "pcdos_5.00.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 205569,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 25137,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 205695,
                    Type     = "0x01"
                },
                new Partition
                {
                    Length   = 245889,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 230895,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 287217,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 476847,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 257985,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 764127,
                    Type     = "0x06"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "pcdos_6.10.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 205569,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 25137,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 205695,
                    Type     = "0x01"
                },
                new Partition
                {
                    Length   = 225729,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 230895,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 245889,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 456687,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 319473,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 702639,
                    Type     = "0x06"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "win95.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 205569,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 245889,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 205695,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 267057,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 451647,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 287217,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 718767,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 17073,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 1006047,
                    Type     = "0x01"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "win96osr25.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 307377,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 245889,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 307503,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 328545,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 553455,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 102753,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 882063,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 21105,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 984879,
                    Type     = "0x01"
                },
                new Partition
                {
                    Length   = 17073,
                    Offset   = 0,
                    Sequence = 5,
                    Size     = 0,
                    Start    = 1006047,
                    Type     = "0x01"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "winnt_3.10.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 204561,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 63,
                    Type     = "0x07"
                },
                new Partition
                {
                    Length   = 307377,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 204687,
                    Type     = "0x07"
                },
                new Partition
                {
                    Length   = 224721,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 512127,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 214641,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 736911,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 10017,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 951615,
                    Type     = "0x01"
                },
                new Partition
                {
                    Length   = 60480,
                    Offset   = 0,
                    Sequence = 5,
                    Size     = 0,
                    Start    = 962640,
                    Type     = "0x07"
                }
            }
        }
    };
}