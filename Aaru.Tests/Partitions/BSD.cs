// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BSD.cs
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using NUnit.Framework;

namespace Aaru.Tests.Partitions;

[TestFixture]
public class Bsd : PartitionSchemeTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Partitioning schemes", "BSD slices");

    public override PartitionTest[] Tests => new[]
    {
        new PartitionTest
        {
            TestFile = "parted.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 75776,
                    Offset   = 1048576,
                    Sequence = 0,
                    Size     = 38797312,
                    Start    = 2048,
                    Type     = "FAT"
                },
                new Partition
                {
                    Length   = 38912,
                    Offset   = 40894464,
                    Sequence = 1,
                    Size     = 19922944,
                    Start    = 79872,
                    Type     = "FAT"
                },
                new Partition
                {
                    Length   = 94208,
                    Offset   = 61865984,
                    Sequence = 2,
                    Size     = 48234496,
                    Start    = 120832,
                    Type     = "FAT"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "netbsd_1.6.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 20417,
                    Offset   = 516096,
                    Sequence = 0,
                    Size     = 10453504,
                    Start    = 1008,
                    Type     = "FAT"
                },
                new Partition
                {
                    Length   = 409600,
                    Offset   = 11354112,
                    Sequence = 1,
                    Size     = 209715200,
                    Start    = 22176,
                    Type     = "FAT"
                },
                new Partition
                {
                    Length   = 1572864,
                    Offset   = 221405184,
                    Sequence = 2,
                    Size     = 805306368,
                    Start    = 432432,
                    Type     = "FAT"
                },
                new Partition
                {
                    Length   = 1460266,
                    Offset   = 1027031040,
                    Sequence = 3,
                    Size     = 747656192,
                    Start    = 2005920,
                    Type     = "4.2BSD Fast File System"
                },
                new Partition
                {
                    Length   = 524288,
                    Offset   = 1774854144,
                    Sequence = 4,
                    Size     = 268435456,
                    Start    = 3466512,
                    Type     = "4.4LFS"
                },
                new Partition
                {
                    Length   = 202624,
                    Offset   = 2043740160,
                    Sequence = 5,
                    Size     = 103743488,
                    Start    = 3991680,
                    Type     = "4.2BSD Fast File System"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "netbsd_6.1.5.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 20480,
                    Offset   = 516096,
                    Sequence = 0,
                    Size     = 10485760,
                    Start    = 1008,
                    Type     = "FAT"
                },
                new Partition
                {
                    Length   = 204800,
                    Offset   = 11354112,
                    Sequence = 1,
                    Size     = 104857600,
                    Start    = 22176,
                    Type     = "FAT"
                },
                new Partition
                {
                    Length   = 409600,
                    Offset   = 116637696,
                    Sequence = 2,
                    Size     = 209715200,
                    Start    = 227808,
                    Type     = "FAT"
                },
                new Partition
                {
                    Length   = 79632,
                    Offset   = 326688768,
                    Sequence = 3,
                    Size     = 40771584,
                    Start    = 638064,
                    Type     = "4.2BSD Fast File System"
                },
                new Partition
                {
                    Length   = 819200,
                    Offset   = 367460352,
                    Sequence = 4,
                    Size     = 419430400,
                    Start    = 717696,
                    Type     = "4.2BSD Fast File System"
                },
                new Partition
                {
                    Length   = 921600,
                    Offset   = 787562496,
                    Sequence = 5,
                    Size     = 471859200,
                    Start    = 1538208,
                    Type     = "4.2BSD Fast File System"
                },
                new Partition
                {
                    Length   = 153600,
                    Offset   = 1259790336,
                    Sequence = 6,
                    Size     = 78643200,
                    Start    = 2460528,
                    Type     = "Hammer"
                },
                new Partition
                {
                    Length   = 194560,
                    Offset   = 1338753024,
                    Sequence = 7,
                    Size     = 99614720,
                    Start    = 2614752,
                    Type     = "UNIX 7th Edition"
                },
                new Partition
                {
                    Length   = 477184,
                    Offset   = 1438875648,
                    Sequence = 8,
                    Size     = 244318208,
                    Start    = 2810304,
                    Type     = "4.4LFS"
                },
                new Partition
                {
                    Length   = 906208,
                    Offset   = 1683505152,
                    Sequence = 9,
                    Size     = 463978496,
                    Start    = 3288096,
                    Type     = "Digital LSM Public Region"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "netbsd_7.1.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 20160,
                    Offset   = 516096,
                    Sequence = 0,
                    Size     = 10321920,
                    Start    = 1008,
                    Type     = "FAT"
                },
                new Partition
                {
                    Length   = 204624,
                    Offset   = 11354112,
                    Sequence = 1,
                    Size     = 104767488,
                    Start    = 22176,
                    Type     = "FAT"
                },
                new Partition
                {
                    Length   = 409248,
                    Offset   = 116637696,
                    Sequence = 2,
                    Size     = 209534976,
                    Start    = 227808,
                    Type     = "FAT"
                },
                new Partition
                {
                    Length   = 78624,
                    Offset   = 326688768,
                    Sequence = 3,
                    Size     = 40255488,
                    Start    = 638064,
                    Type     = "4.2BSD Fast File System"
                },
                new Partition
                {
                    Length   = 818496,
                    Offset   = 367460352,
                    Sequence = 4,
                    Size     = 419069952,
                    Start    = 717696,
                    Type     = "4.2BSD Fast File System"
                },
                new Partition
                {
                    Length   = 921312,
                    Offset   = 787562496,
                    Sequence = 5,
                    Size     = 471711744,
                    Start    = 1538208,
                    Type     = "4.2BSD Fast File System"
                },
                new Partition
                {
                    Length   = 153216,
                    Offset   = 1259790336,
                    Sequence = 6,
                    Size     = 78446592,
                    Start    = 2460528,
                    Type     = "Hammer"
                },
                new Partition
                {
                    Length   = 194544,
                    Offset   = 1338753024,
                    Sequence = 7,
                    Size     = 99606528,
                    Start    = 2614752,
                    Type     = "UNIX 7th Edition"
                },
                new Partition
                {
                    Length   = 475776,
                    Offset   = 1438875648,
                    Sequence = 8,
                    Size     = 243597312,
                    Start    = 2810304,
                    Type     = "4.4LFS"
                },
                new Partition
                {
                    Length   = 906192,
                    Offset   = 1683505152,
                    Sequence = 9,
                    Size     = 463970304,
                    Start    = 3288096,
                    Type     = "Digital LSM Public Region"
                }
            }
        }
    };
}