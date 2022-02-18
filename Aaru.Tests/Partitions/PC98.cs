// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PC98.cs
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
using NUnit.Framework;

namespace Aaru.Tests.Partitions
{
    [TestFixture]
    public class Pc98 : PartitionSchemeTest
    {
        public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Partitioning schemes", "PC-98");

        public override PartitionTest[] Tests => new[]
        {
            new PartitionTest
            {
                TestFile = "msdos330.aif",
                Partitions = new[]
                {
                    new Partition
                    {
                        Length   = 19536,
                        Name     = "MS-DOS 3.30",
                        Offset   = 0,
                        Sequence = 0,
                        Size     = 0,
                        Start    = 264,
                        Type     = "FAT12"
                    },
                    new Partition
                    {
                        Length   = 39336,
                        Name     = "MS-DOS 3.30",
                        Offset   = 0,
                        Sequence = 1,
                        Size     = 0,
                        Start    = 20064,
                        Type     = "FAT12"
                    },
                    new Partition
                    {
                        Length   = 59136,
                        Name     = "MS-DOS 3.30",
                        Offset   = 0,
                        Sequence = 2,
                        Size     = 0,
                        Start    = 59664,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 78936,
                        Name     = "MS-DOS 3.30",
                        Offset   = 0,
                        Sequence = 3,
                        Size     = 0,
                        Start    = 119064,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 118536,
                        Name     = "MS-DOS 3.30",
                        Offset   = 0,
                        Sequence = 4,
                        Size     = 0,
                        Start    = 198264,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 197736,
                        Name     = "MS-DOS 3.30",
                        Offset   = 0,
                        Sequence = 5,
                        Size     = 0,
                        Start    = 317064,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 237336,
                        Name     = "MS-DOS 3.30",
                        Offset   = 0,
                        Sequence = 6,
                        Size     = 0,
                        Start    = 515064,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 245256,
                        Name     = "MS-DOS 3.30",
                        Offset   = 0,
                        Sequence = 7,
                        Size     = 0,
                        Start    = 752664,
                        Type     = "FAT16"
                    }
                }
            },
            new PartitionTest
            {
                TestFile = "msdos330_alt.aif",
                Partitions = new[]
                {
                    new Partition
                    {
                        Length   = 59136,
                        Name     = "MS-DOS 3.30",
                        Offset   = 0,
                        Sequence = 0,
                        Size     = 0,
                        Start    = 264,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 158136,
                        Name     = "MS-DOS 3.30",
                        Offset   = 0,
                        Sequence = 1,
                        Size     = 0,
                        Start    = 59664,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 94776,
                        Name     = "MS-DOS 3.30",
                        Offset   = 0,
                        Sequence = 2,
                        Size     = 0,
                        Start    = 218064,
                        Type     = "FAT16"
                    }
                }
            },
            new PartitionTest
            {
                TestFile = "msdos500_epson.aif",
                Partitions = new[]
                {
                    new Partition
                    {
                        Length   = 35639,
                        Name     = "NamenameName",
                        Offset   = 0,
                        Sequence = 0,
                        Size     = 0,
                        Start    = 264,
                        Type     = "FAT12"
                    },
                    new Partition
                    {
                        Length   = 59399,
                        Name     = "12BitFAT",
                        Offset   = 0,
                        Sequence = 1,
                        Size     = 0,
                        Start    = 35904,
                        Type     = "FAT12"
                    },
                    new Partition
                    {
                        Length   = 79199,
                        Name     = "16BitFAT",
                        Offset   = 0,
                        Sequence = 2,
                        Size     = 0,
                        Start    = 95304,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 118799,
                        Name     = "PartLblMaxNameXX",
                        Offset   = 0,
                        Sequence = 3,
                        Size     = 0,
                        Start    = 174504,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 158399,
                        Name     = "BigFAT12",
                        Offset   = 0,
                        Sequence = 4,
                        Size     = 0,
                        Start    = 293304,
                        Type     = "FAT12"
                    },
                    new Partition
                    {
                        Length   = 197999,
                        Name     = "Lalalalalalalala",
                        Offset   = 0,
                        Sequence = 5,
                        Size     = 0,
                        Start    = 451704,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 237599,
                        Name     = "MS-DOS Ver 5.0",
                        Offset   = 0,
                        Sequence = 6,
                        Size     = 0,
                        Start    = 649704,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 118799,
                        Name     = "MS-DOS Ver 5.0",
                        Offset   = 0,
                        Sequence = 7,
                        Size     = 0,
                        Start    = 887304,
                        Type     = "FAT16"
                    }
                }
            },
            new PartitionTest
            {
                TestFile = "msdos500.aif",
                Partitions = new[]
                {
                    new Partition
                    {
                        Length   = 28512,
                        Name     = "MS-DOS 5.00",
                        Offset   = 0,
                        Sequence = 0,
                        Size     = 0,
                        Start    = 264,
                        Type     = "FAT12"
                    },
                    new Partition
                    {
                        Length   = 49104,
                        Name     = "MS-DOS 5.00",
                        Offset   = 0,
                        Sequence = 1,
                        Size     = 0,
                        Start    = 29040,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 93984,
                        Name     = "MS-DOS 5.00",
                        Offset   = 0,
                        Sequence = 2,
                        Size     = 0,
                        Start    = 78408,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 122760,
                        Name     = "MS-DOS 5.00",
                        Offset   = 0,
                        Sequence = 3,
                        Size     = 0,
                        Start    = 172656,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 163680,
                        Name     = "MS-DOS 5.00",
                        Offset   = 0,
                        Sequence = 4,
                        Size     = 0,
                        Start    = 295680,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 204600,
                        Name     = "MS-DOS 5.00",
                        Offset   = 0,
                        Sequence = 5,
                        Size     = 0,
                        Start    = 459624,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 204600,
                        Name     = "MS-DOS 5.00",
                        Offset   = 0,
                        Sequence = 6,
                        Size     = 0,
                        Start    = 664488,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 139128,
                        Name     = "MS-DOS 5.00",
                        Offset   = 0,
                        Sequence = 7,
                        Size     = 0,
                        Start    = 869352,
                        Type     = "FAT16"
                    }
                }
            },
            new PartitionTest
            {
                TestFile = "msdos620.aif",
                Partitions = new[]
                {
                    new Partition
                    {
                        Length   = 61248,
                        Name     = "MS-DOS 6.20",
                        Offset   = 0,
                        Sequence = 0,
                        Size     = 0,
                        Start    = 264,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 81840,
                        Name     = "MS-DOS 6.20",
                        Offset   = 0,
                        Sequence = 1,
                        Size     = 0,
                        Start    = 61776,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 122760,
                        Name     = "MS-DOS 6.20",
                        Offset   = 0,
                        Sequence = 2,
                        Size     = 0,
                        Start    = 143880,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 163680,
                        Name     = "MS-DOS 6.20",
                        Offset   = 0,
                        Sequence = 3,
                        Size     = 0,
                        Start    = 266904,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 20328,
                        Name     = "MS-DOS 6.20",
                        Offset   = 0,
                        Sequence = 4,
                        Size     = 0,
                        Start    = 430848,
                        Type     = "FAT12"
                    },
                    new Partition
                    {
                        Length   = 245520,
                        Name     = "MS-DOS 6.20",
                        Offset   = 0,
                        Sequence = 5,
                        Size     = 0,
                        Start    = 451440,
                        Type     = "FAT16"
                    },
                    new Partition
                    {
                        Length   = 315216,
                        Name     = "MS-DOS 6.20",
                        Offset   = 0,
                        Sequence = 6,
                        Size     = 0,
                        Start    = 697224,
                        Type     = "FAT16"
                    }
                }
            }
        };
    }
}