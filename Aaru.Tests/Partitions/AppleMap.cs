// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AppleMap.cs
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
public class AppleMap : PartitionSchemeTest
{
    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Partitioning schemes", "Apple Partition Map");

    public override PartitionTest[] Tests =>
    [
        new PartitionTest
        {
            TestFile = "d2_driver.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 2,
                    Offset   = 32768,
                    Sequence = 0,
                    Size     = 1024,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 83,
                    Name     = "Macintosh",
                    Offset   = 32768,
                    Sequence = 1,
                    Size     = 42496,
                    Start    = 64,
                    Type     = "Apple_Driver43"
                },
                new Partition
                {
                    Length   = 109,
                    Name     = "Empty",
                    Offset   = 75264,
                    Sequence = 2,
                    Size     = 55808,
                    Start    = 147,
                    Type     = "Apple_Free"
                },
                new Partition
                {
                    Length   = 50944,
                    Name     = "Volume label",
                    Offset   = 131072,
                    Sequence = 3,
                    Size     = 26083328,
                    Start    = 256,
                    Type     = "Apple_HFS"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "hdt_1.8_encrypted1.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 14,
                    Offset   = 32768,
                    Sequence = 0,
                    Size     = 7168,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 1024,
                    Name     = "FWB Disk Driver",
                    Offset   = 32768,
                    Sequence = 1,
                    Size     = 524288,
                    Start    = 64,
                    Type     = "Apple_Driver43"
                },
                new Partition
                {
                    Length   = 50112,
                    Name     = "MacOS",
                    Offset   = 557056,
                    Sequence = 2,
                    Size     = 25657344,
                    Start    = 1088,
                    Type     = "Apple_HFS"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "hdt_1.8_encrypted2.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 14,
                    Offset   = 32768,
                    Sequence = 0,
                    Size     = 7168,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 1024,
                    Name     = "FWB Disk Driver",
                    Offset   = 32768,
                    Sequence = 1,
                    Size     = 524288,
                    Start    = 64,
                    Type     = "Apple_Driver43"
                },
                new Partition
                {
                    Length   = 50112,
                    Name     = "MacOS",
                    Offset   = 557056,
                    Sequence = 2,
                    Size     = 25657344,
                    Start    = 1088,
                    Type     = "Apple_HFS"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "hdt_1.8_password.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 14,
                    Offset   = 32768,
                    Sequence = 0,
                    Size     = 7168,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 1024,
                    Name     = "FWB Disk Driver",
                    Offset   = 32768,
                    Sequence = 1,
                    Size     = 524288,
                    Start    = 64,
                    Type     = "Apple_Driver43"
                },
                new Partition
                {
                    Length   = 50112,
                    Name     = "MacOS",
                    Offset   = 557056,
                    Sequence = 2,
                    Size     = 25657344,
                    Start    = 1088,
                    Type     = "Apple_HFS"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "hdt_1.8.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 14,
                    Offset   = 32768,
                    Sequence = 0,
                    Size     = 7168,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 1024,
                    Name     = "FWB Disk Driver",
                    Offset   = 32768,
                    Sequence = 1,
                    Size     = 524288,
                    Start    = 64,
                    Type     = "Apple_Driver43"
                },
                new Partition
                {
                    Length   = 50112,
                    Name     = "MacOS",
                    Offset   = 557056,
                    Sequence = 2,
                    Size     = 25657344,
                    Start    = 1088,
                    Type     = "Apple_HFS"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "linux.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 1,
                    Name     = "Extra",
                    Offset   = 32768,
                    Sequence = 0,
                    Size     = 512,
                    Start    = 64,
                    Type     = "Apple_Free"
                },
                new Partition
                {
                    Length   = 1600,
                    Name     = "bootstrap",
                    Offset   = 33280,
                    Sequence = 1,
                    Size     = 819200,
                    Start    = 65,
                    Type     = "Apple_Bootstrap"
                },
                new Partition
                {
                    Length   = 1,
                    Name     = "Extra",
                    Offset   = 852480,
                    Sequence = 2,
                    Size     = 512,
                    Start    = 1665,
                    Type     = "Apple_Free"
                },
                new Partition
                {
                    Length   = 102400,
                    Name     = "Linux",
                    Offset   = 852992,
                    Sequence = 3,
                    Size     = 52428800,
                    Start    = 1666,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 40960,
                    Name     = "ProDOS",
                    Offset   = 53281792,
                    Sequence = 4,
                    Size     = 20971520,
                    Start    = 104066,
                    Type     = "Apple_PRODOS"
                },
                new Partition
                {
                    Length   = 102400,
                    Name     = "Macintosh",
                    Offset   = 74253312,
                    Sequence = 5,
                    Size     = 52428800,
                    Start    = 145026,
                    Type     = "Apple_HFS"
                },
                new Partition
                {
                    Length   = 14718,
                    Name     = "Extra",
                    Offset   = 126682112,
                    Sequence = 6,
                    Size     = 7535616,
                    Start    = 247426,
                    Type     = "Apple_Free"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "macos_1.1.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 10,
                    Offset   = 2048,
                    Sequence = 0,
                    Size     = 5120,
                    Start    = 4,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 41804,
                    Name     = "Macintosh",
                    Offset   = 8192,
                    Sequence = 1,
                    Size     = 21403648,
                    Start    = 16,
                    Type     = "Apple_HFS"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "macos_2.0.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 10,
                    Offset   = 2048,
                    Sequence = 0,
                    Size     = 5120,
                    Start    = 4,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 38965,
                    Name     = "Macintosh",
                    Offset   = 8192,
                    Sequence = 1,
                    Size     = 19950080,
                    Start    = 16,
                    Type     = "Apple_HFS"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "macos_4.2.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 11,
                    Offset   = 2048,
                    Sequence = 0,
                    Size     = 5632,
                    Start    = 4,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 38965,
                    Name     = "Macintosh",
                    Offset   = 8192,
                    Sequence = 1,
                    Size     = 19950080,
                    Start    = 16,
                    Type     = "Apple_HFS"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "macos_4.3.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 11,
                    Offset   = 2048,
                    Sequence = 0,
                    Size     = 5632,
                    Start    = 4,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 38965,
                    Name     = "Macintosh",
                    Offset   = 8192,
                    Sequence = 1,
                    Size     = 19950080,
                    Start    = 16,
                    Type     = "Apple_HFS"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "macos_6.0.2.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 10,
                    Offset   = 32768,
                    Sequence = 0,
                    Size     = 5120,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 32,
                    Name     = "Macintosh",
                    Offset   = 32768,
                    Sequence = 1,
                    Size     = 16384,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 6256,
                    Name     = "MacOS",
                    Offset   = 49152,
                    Sequence = 2,
                    Size     = 3203072,
                    Start    = 96,
                    Type     = "Apple_HFS"
                },
                new Partition
                {
                    Length   = 1024,
                    Name     = "Scratch",
                    Offset   = 3252224,
                    Sequence = 3,
                    Size     = 524288,
                    Start    = 6352,
                    Type     = "Apple_Scratch"
                },
                new Partition
                {
                    Length   = 2048,
                    Name     = "Eschatology 1",
                    Offset   = 3776512,
                    Sequence = 4,
                    Size     = 1048576,
                    Start    = 7376,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 4280,
                    Name     = "A/UX Root",
                    Offset   = 4825088,
                    Sequence = 5,
                    Size     = 2191360,
                    Start    = 9424,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 2377,
                    Name     = "Swap",
                    Offset   = 7016448,
                    Sequence = 6,
                    Size     = 1217024,
                    Start    = 13704,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 3072,
                    Name     = "Eschatology 2",
                    Offset   = 8233472,
                    Sequence = 7,
                    Size     = 1572864,
                    Start    = 16081,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 2560,
                    Name     = "Root file system",
                    Offset   = 9806336,
                    Sequence = 8,
                    Size     = 1310720,
                    Start    = 19153,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 4981,
                    Name     = "Usr file system",
                    Offset   = 11117056,
                    Sequence = 9,
                    Size     = 2550272,
                    Start    = 21713,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 4000,
                    Name     = "Random A/UX fs",
                    Offset   = 13667328,
                    Sequence = 10,
                    Size     = 2048000,
                    Start    = 26694,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 2532,
                    Name     = "Random A/UX fs",
                    Offset   = 15715328,
                    Sequence = 11,
                    Size     = 1296384,
                    Start    = 30694,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 2666,
                    Name     = "Usr file system",
                    Offset   = 17011712,
                    Sequence = 12,
                    Size     = 1364992,
                    Start    = 33226,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 7786,
                    Name     = "Usr file system",
                    Offset   = 18376704,
                    Sequence = 13,
                    Size     = 3986432,
                    Start    = 35892,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 11162,
                    Name     = "Extra",
                    Offset   = 22363136,
                    Sequence = 14,
                    Size     = 5714944,
                    Start    = 43678,
                    Type     = "Apple_Free"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "macos_6.0.3.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 10,
                    Offset   = 32768,
                    Sequence = 0,
                    Size     = 5120,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 32,
                    Name     = "Macintosh",
                    Offset   = 32768,
                    Sequence = 1,
                    Size     = 16384,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 11619,
                    Name     = "MacOS",
                    Offset   = 49152,
                    Sequence = 2,
                    Size     = 5948928,
                    Start    = 96,
                    Type     = "Apple_HFS"
                },
                new Partition
                {
                    Length   = 2011,
                    Name     = "Scratch",
                    Offset   = 5998080,
                    Sequence = 3,
                    Size     = 1029632,
                    Start    = 11715,
                    Type     = "Apple_Scratch"
                },
                new Partition
                {
                    Length   = 4796,
                    Name     = "Eschatology 1",
                    Offset   = 7027712,
                    Sequence = 4,
                    Size     = 2455552,
                    Start    = 13726,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 7680,
                    Name     = "A/UX Root",
                    Offset   = 9483264,
                    Sequence = 5,
                    Size     = 3932160,
                    Start    = 18522,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 8192,
                    Name     = "Swap",
                    Offset   = 13415424,
                    Sequence = 6,
                    Size     = 4194304,
                    Start    = 26202,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 1148,
                    Name     = "Eschatology 2",
                    Offset   = 17609728,
                    Sequence = 7,
                    Size     = 587776,
                    Start    = 34394,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 12768,
                    Name     = "Root file system",
                    Offset   = 18197504,
                    Sequence = 8,
                    Size     = 6537216,
                    Start    = 35542,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 3450,
                    Name     = "Usr file system",
                    Offset   = 24734720,
                    Sequence = 9,
                    Size     = 1766400,
                    Start    = 48310,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 36,
                    Name     = "Extra",
                    Offset   = 26501120,
                    Sequence = 10,
                    Size     = 18432,
                    Start    = 51760,
                    Type     = "Apple_Free"
                },
                new Partition
                {
                    Length   = 3044,
                    Name     = "Random A/UX fs",
                    Offset   = 26519552,
                    Sequence = 11,
                    Size     = 1558528,
                    Start    = 51796,
                    Type     = "Apple_UNIX_SVR2"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "macos_6.0.4.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 10,
                    Offset   = 32768,
                    Sequence = 0,
                    Size     = 5120,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 32,
                    Name     = "Macintosh",
                    Offset   = 32768,
                    Sequence = 1,
                    Size     = 16384,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 7680,
                    Name     = "MacOS",
                    Offset   = 49152,
                    Sequence = 2,
                    Size     = 3932160,
                    Start    = 96,
                    Type     = "Apple_HFS"
                },
                new Partition
                {
                    Length   = 6245,
                    Name     = "Scratch",
                    Offset   = 3981312,
                    Sequence = 3,
                    Size     = 3197440,
                    Start    = 7776,
                    Type     = "Apple_Scratch"
                },
                new Partition
                {
                    Length   = 6245,
                    Name     = "Eschatology 1",
                    Offset   = 7178752,
                    Sequence = 4,
                    Size     = 3197440,
                    Start    = 14021,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 5130,
                    Name     = "A/UX Root",
                    Offset   = 10376192,
                    Sequence = 5,
                    Size     = 2626560,
                    Start    = 20266,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 2676,
                    Name     = "Swap",
                    Offset   = 13002752,
                    Sequence = 6,
                    Size     = 1370112,
                    Start    = 25396,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 5751,
                    Name     = "Eschatology 2",
                    Offset   = 14372864,
                    Sequence = 7,
                    Size     = 2944512,
                    Start    = 28072,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 5423,
                    Name     = "Root file system",
                    Offset   = 17317376,
                    Sequence = 8,
                    Size     = 2776576,
                    Start    = 33823,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 5650,
                    Name     = "Usr file system",
                    Offset   = 20093952,
                    Sequence = 9,
                    Size     = 2892800,
                    Start    = 39246,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 6706,
                    Name     = "Random A/UX fs",
                    Offset   = 22986752,
                    Sequence = 10,
                    Size     = 3433472,
                    Start    = 44896,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 3238,
                    Name     = "Extra",
                    Offset   = 26420224,
                    Sequence = 11,
                    Size     = 1657856,
                    Start    = 51602,
                    Type     = "Apple_Free"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "macos_6.0.5.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 10,
                    Offset   = 32768,
                    Sequence = 0,
                    Size     = 5120,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 32,
                    Name     = "Macintosh",
                    Offset   = 32768,
                    Sequence = 1,
                    Size     = 16384,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 4096,
                    Name     = "MacOS",
                    Offset   = 49152,
                    Sequence = 2,
                    Size     = 2097152,
                    Start    = 96,
                    Type     = "Apple_HFS"
                },
                new Partition
                {
                    Length   = 669,
                    Name     = "Scratch",
                    Offset   = 2146304,
                    Sequence = 3,
                    Size     = 342528,
                    Start    = 4192,
                    Type     = "Apple_Scratch"
                },
                new Partition
                {
                    Length   = 2768,
                    Name     = "Eschatology 1",
                    Offset   = 2488832,
                    Sequence = 4,
                    Size     = 1417216,
                    Start    = 4861,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 3576,
                    Name     = "A/UX Root",
                    Offset   = 3906048,
                    Sequence = 5,
                    Size     = 1830912,
                    Start    = 7629,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 2830,
                    Name     = "Swap",
                    Offset   = 5736960,
                    Sequence = 6,
                    Size     = 1448960,
                    Start    = 11205,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 5249,
                    Name     = "Root file system",
                    Offset   = 7185920,
                    Sequence = 7,
                    Size     = 2687488,
                    Start    = 14035,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 5011,
                    Name     = "Usr file system",
                    Offset   = 9873408,
                    Sequence = 8,
                    Size     = 2565632,
                    Start    = 19284,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 3818,
                    Name     = "Unreserved 1",
                    Offset   = 12439040,
                    Sequence = 9,
                    Size     = 1954816,
                    Start    = 24295,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 6920,
                    Name     = "Unreserved 2",
                    Offset   = 14393856,
                    Sequence = 10,
                    Size     = 3543040,
                    Start    = 28113,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 5011,
                    Name     = "Unreserved 3",
                    Offset   = 17936896,
                    Sequence = 11,
                    Size     = 2565632,
                    Start    = 35033,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 5727,
                    Name     = "Unreserved 4",
                    Offset   = 20502528,
                    Sequence = 12,
                    Size     = 2932224,
                    Start    = 40044,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 2386,
                    Name     = "Random A/UX fs",
                    Offset   = 23434752,
                    Sequence = 13,
                    Size     = 1221632,
                    Start    = 45771,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 6683,
                    Name     = "Extra",
                    Offset   = 24656384,
                    Sequence = 14,
                    Size     = 3421696,
                    Start    = 48157,
                    Type     = "Apple_Free"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "macos_6.0.7.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 10,
                    Offset   = 32768,
                    Sequence = 0,
                    Size     = 5120,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 32,
                    Name     = "Macintosh",
                    Offset   = 32768,
                    Sequence = 1,
                    Size     = 16384,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 27371,
                    Name     = "MacOS",
                    Offset   = 49152,
                    Sequence = 2,
                    Size     = 14013952,
                    Start    = 96,
                    Type     = "Apple_HFS"
                },
                new Partition
                {
                    Length   = 2916,
                    Name     = "Eschatology 1",
                    Offset   = 14063104,
                    Sequence = 3,
                    Size     = 1492992,
                    Start    = 27467,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 1795,
                    Name     = "A/UX Root",
                    Offset   = 15556096,
                    Sequence = 4,
                    Size     = 919040,
                    Start    = 30383,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 2543,
                    Name     = "Swap",
                    Offset   = 16475136,
                    Sequence = 5,
                    Size     = 1302016,
                    Start    = 32178,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 3509,
                    Name     = "Root file system",
                    Offset   = 17777152,
                    Sequence = 6,
                    Size     = 1796608,
                    Start    = 34721,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 3796,
                    Name     = "Usr file system",
                    Offset   = 19573760,
                    Sequence = 7,
                    Size     = 1943552,
                    Start    = 38230,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 4271,
                    Name     = "Random A/UX fs",
                    Offset   = 21517312,
                    Sequence = 8,
                    Size     = 2186752,
                    Start    = 42026,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 1024,
                    Name     = "Unreserved 1",
                    Offset   = 23704064,
                    Sequence = 9,
                    Size     = 524288,
                    Start    = 46297,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 1280,
                    Name     = "Unreserved 2",
                    Offset   = 24228352,
                    Sequence = 10,
                    Size     = 655360,
                    Start    = 47321,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 1559,
                    Name     = "Unreserved 3",
                    Offset   = 24883712,
                    Sequence = 11,
                    Size     = 798208,
                    Start    = 48601,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 280,
                    Name     = "Extra",
                    Offset   = 25681920,
                    Sequence = 12,
                    Size     = 143360,
                    Start    = 50160,
                    Type     = "Apple_Free"
                },
                new Partition
                {
                    Length   = 4400,
                    Name     = "Unreserved 4",
                    Offset   = 25825280,
                    Sequence = 13,
                    Size     = 2252800,
                    Start    = 50440,
                    Type     = "Apple_UNIX_SVR2"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "macos_6.0.8.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 10,
                    Offset   = 32768,
                    Sequence = 0,
                    Size     = 5120,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 32,
                    Name     = "Macintosh",
                    Offset   = 32768,
                    Sequence = 1,
                    Size     = 16384,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 8937,
                    Name     = "MacOS",
                    Offset   = 49152,
                    Sequence = 2,
                    Size     = 4575744,
                    Start    = 96,
                    Type     = "Apple_HFS"
                },
                new Partition
                {
                    Length   = 2234,
                    Name     = "Scratch",
                    Offset   = 4624896,
                    Sequence = 3,
                    Size     = 1143808,
                    Start    = 9033,
                    Type     = "Apple_Scratch"
                },
                new Partition
                {
                    Length   = 5900,
                    Name     = "Eschatology 1",
                    Offset   = 5768704,
                    Sequence = 4,
                    Size     = 3020800,
                    Start    = 11267,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 3156,
                    Name     = "Unreserved 1",
                    Offset   = 8789504,
                    Sequence = 5,
                    Size     = 1615872,
                    Start    = 17167,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 2705,
                    Name     = "Unreserved 3",
                    Offset   = 10405376,
                    Sequence = 6,
                    Size     = 1384960,
                    Start    = 20323,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 1861,
                    Name     = "Unreserved 4",
                    Offset   = 11790336,
                    Sequence = 7,
                    Size     = 952832,
                    Start    = 23028,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 2434,
                    Name     = "Extra",
                    Offset   = 12743168,
                    Sequence = 8,
                    Size     = 1246208,
                    Start    = 24889,
                    Type     = "Apple_Free"
                },
                new Partition
                {
                    Length   = 2920,
                    Name     = "Random A/UX fs",
                    Offset   = 13989376,
                    Sequence = 9,
                    Size     = 1495040,
                    Start    = 27323,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 3156,
                    Name     = "Unreserved 2",
                    Offset   = 15484416,
                    Sequence = 10,
                    Size     = 1615872,
                    Start    = 30243,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 5635,
                    Name     = "Usr file system",
                    Offset   = 17100288,
                    Sequence = 11,
                    Size     = 2885120,
                    Start    = 33399,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 4508,
                    Name     = "Root file system",
                    Offset   = 19985408,
                    Sequence = 12,
                    Size     = 2308096,
                    Start    = 39034,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 7213,
                    Name     = "Swap",
                    Offset   = 22293504,
                    Sequence = 13,
                    Size     = 3693056,
                    Start    = 43542,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 4085,
                    Name     = "A/UX Root",
                    Offset   = 25986560,
                    Sequence = 14,
                    Size     = 2091520,
                    Start    = 50755,
                    Type     = "Apple_UNIX_SVR2"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "macos_6.0.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 10,
                    Offset   = 32768,
                    Sequence = 0,
                    Size     = 5120,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 32,
                    Name     = "Macintosh",
                    Offset   = 32768,
                    Sequence = 1,
                    Size     = 16384,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 4096,
                    Name     = "MacOS",
                    Offset   = 49152,
                    Sequence = 2,
                    Size     = 2097152,
                    Start    = 96,
                    Type     = "Apple_HFS"
                },
                new Partition
                {
                    Length   = 4096,
                    Name     = "Scratch",
                    Offset   = 2146304,
                    Sequence = 3,
                    Size     = 2097152,
                    Start    = 4192,
                    Type     = "Apple_Scratch"
                },
                new Partition
                {
                    Length   = 4096,
                    Name     = "Eschatology 1",
                    Offset   = 4243456,
                    Sequence = 4,
                    Size     = 2097152,
                    Start    = 8288,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 4096,
                    Name     = "A/UX Root",
                    Offset   = 6340608,
                    Sequence = 5,
                    Size     = 2097152,
                    Start    = 12384,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 2048,
                    Name     = "Swap",
                    Offset   = 8437760,
                    Sequence = 6,
                    Size     = 1048576,
                    Start    = 16480,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 4096,
                    Name     = "Eschatology 2",
                    Offset   = 9486336,
                    Sequence = 7,
                    Size     = 2097152,
                    Start    = 18528,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 4096,
                    Name     = "Root file system",
                    Offset   = 11583488,
                    Sequence = 8,
                    Size     = 2097152,
                    Start    = 22624,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 4512,
                    Name     = "Usr file system",
                    Offset   = 13680640,
                    Sequence = 9,
                    Size     = 2310144,
                    Start    = 26720,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 10580,
                    Name     = "Random A/UX fs",
                    Offset   = 15990784,
                    Sequence = 10,
                    Size     = 5416960,
                    Start    = 31232,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 8,
                    Name     = "Extra",
                    Offset   = 21407744,
                    Sequence = 11,
                    Size     = 4096,
                    Start    = 41812,
                    Type     = "Apple_Free"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "macos_7.0.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 10,
                    Offset   = 32768,
                    Sequence = 0,
                    Size     = 5120,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 32,
                    Name     = "Macintosh",
                    Offset   = 32768,
                    Sequence = 1,
                    Size     = 16384,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 6002,
                    Name     = "Scratch",
                    Offset   = 49152,
                    Sequence = 2,
                    Size     = 3073024,
                    Start    = 96,
                    Type     = "Apple_Scratch"
                },
                new Partition
                {
                    Length   = 5325,
                    Name     = "Root file system",
                    Offset   = 3122176,
                    Sequence = 3,
                    Size     = 2726400,
                    Start    = 6098,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 6212,
                    Name     = "Extra",
                    Offset   = 5848576,
                    Sequence = 4,
                    Size     = 3180544,
                    Start    = 11423,
                    Type     = "Apple_Free"
                },
                new Partition
                {
                    Length   = 8210,
                    Name     = "Random A/UX fs",
                    Offset   = 9029120,
                    Sequence = 5,
                    Size     = 4203520,
                    Start    = 17635,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 5104,
                    Name     = "Extra",
                    Offset   = 13232640,
                    Sequence = 6,
                    Size     = 2613248,
                    Start    = 25845,
                    Type     = "Apple_Free"
                },
                new Partition
                {
                    Length   = 10278,
                    Name     = "MacOS",
                    Offset   = 15845888,
                    Sequence = 7,
                    Size     = 5262336,
                    Start    = 30949,
                    Type     = "Apple_HFS"
                },
                new Partition
                {
                    Length   = 3335,
                    Name     = "Eschatology 1",
                    Offset   = 21108224,
                    Sequence = 8,
                    Size     = 1707520,
                    Start    = 41227,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 10278,
                    Name     = "Extra",
                    Offset   = 22815744,
                    Sequence = 9,
                    Size     = 5262336,
                    Start    = 44562,
                    Type     = "Apple_Free"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "macos_7.1.1.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 17,
                    Offset   = 32768,
                    Sequence = 0,
                    Size     = 8704,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 32,
                    Name     = "Macintosh",
                    Offset   = 32768,
                    Sequence = 1,
                    Size     = 16384,
                    Start    = 64,
                    Type     = "Apple_Driver43"
                },
                new Partition
                {
                    Length   = 2904,
                    Name     = "Random A/UX fs",
                    Offset   = 49152,
                    Sequence = 2,
                    Size     = 1486848,
                    Start    = 96,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 4096,
                    Name     = "ProDOS",
                    Offset   = 1536000,
                    Sequence = 3,
                    Size     = 2097152,
                    Start    = 3000,
                    Type     = "Apple_PRODOS"
                },
                new Partition
                {
                    Length   = 3055,
                    Name     = "Extra",
                    Offset   = 3633152,
                    Sequence = 4,
                    Size     = 1564160,
                    Start    = 7096,
                    Type     = "Apple_Free"
                },
                new Partition
                {
                    Length   = 4096,
                    Name     = "ProDOS",
                    Offset   = 5197312,
                    Sequence = 5,
                    Size     = 2097152,
                    Start    = 10151,
                    Type     = "Apple_PRODOS"
                },
                new Partition
                {
                    Length   = 10055,
                    Name     = "MacOS",
                    Offset   = 7294464,
                    Sequence = 6,
                    Size     = 5148160,
                    Start    = 14247,
                    Type     = "Apple_HFS"
                },
                new Partition
                {
                    Length   = 8607,
                    Name     = "Extra",
                    Offset   = 12442624,
                    Sequence = 7,
                    Size     = 4406784,
                    Start    = 24302,
                    Type     = "Apple_Free"
                },
                new Partition
                {
                    Length   = 4855,
                    Name     = "Random A/UX fs",
                    Offset   = 16849408,
                    Sequence = 8,
                    Size     = 2485760,
                    Start    = 32909,
                    Type     = "Apple_UNIX_SVR2"
                },
                new Partition
                {
                    Length   = 9270,
                    Name     = "Extra",
                    Offset   = 19335168,
                    Sequence = 9,
                    Size     = 4746240,
                    Start    = 37764,
                    Type     = "Apple_Free"
                },
                new Partition
                {
                    Length   = 7806,
                    Name     = "A/UX Root",
                    Offset   = 24081408,
                    Sequence = 10,
                    Size     = 3996672,
                    Start    = 47034,
                    Type     = "Apple_UNIX_SVR2"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "macos_7.5.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 18,
                    Offset   = 32768,
                    Sequence = 0,
                    Size     = 9216,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 32,
                    Name     = "Macintosh",
                    Offset   = 32768,
                    Sequence = 1,
                    Size     = 16384,
                    Start    = 64,
                    Type     = "Apple_Driver43"
                },
                new Partition
                {
                    Length   = 54744,
                    Name     = "MacOS",
                    Offset   = 49152,
                    Sequence = 2,
                    Size     = 28028928,
                    Start    = 96,
                    Type     = "Apple_HFS"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "parted.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 4032,
                    Name     = "Extra",
                    Offset   = 32768,
                    Sequence = 0,
                    Size     = 2064384,
                    Start    = 64,
                    Type     = "Apple_Free"
                },
                new Partition
                {
                    Length   = 92160,
                    Name     = "untitled",
                    Offset   = 2097152,
                    Sequence = 1,
                    Size     = 47185920,
                    Start    = 4096,
                    Type     = "Apple_HFS"
                },
                new Partition
                {
                    Length   = 165888,
                    Name     = "untitled",
                    Offset   = 49283072,
                    Sequence = 2,
                    Size     = 84934656,
                    Start    = 96256,
                    Type     = "Apple_UNIX_SVR2"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "silverlining_2.2.1.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 6,
                    Offset   = 32768,
                    Sequence = 0,
                    Size     = 3072,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 128,
                    Name     = "Macintosh_SL",
                    Offset   = 32768,
                    Sequence = 1,
                    Size     = 65536,
                    Start    = 64,
                    Type     = "Apple_Driver43"
                },
                new Partition
                {
                    Length   = 49,
                    Offset   = 98304,
                    Sequence = 2,
                    Size     = 25088,
                    Start    = 192,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 128,
                    Name     = "Macintosh_SL",
                    Offset   = 98304,
                    Sequence = 3,
                    Size     = 65536,
                    Start    = 192,
                    Type     = "Apple_Driver_ATA"
                },
                new Partition
                {
                    Length   = 50400,
                    Name     = "Untitled  #1",
                    Offset   = 163840,
                    Sequence = 4,
                    Size     = 25804800,
                    Start    = 320,
                    Type     = "Apple_HFS"
                },
                new Partition
                {
                    Length   = 464,
                    Name     = "Extra",
                    Offset   = 25968640,
                    Sequence = 5,
                    Size     = 237568,
                    Start    = 50720,
                    Type     = "Apple_Free"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "speedtools_3.6.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 27,
                    Offset   = 32768,
                    Sequence = 0,
                    Size     = 13824,
                    Start    = 64,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 100,
                    Name     = "Macintosh",
                    Offset   = 32768,
                    Sequence = 1,
                    Size     = 51200,
                    Start    = 64,
                    Type     = "Apple_Driver43"
                },
                new Partition
                {
                    Length   = 49152,
                    Name     = "untitled",
                    Offset   = 83968,
                    Sequence = 2,
                    Size     = 25165824,
                    Start    = 164,
                    Type     = "Apple_HFS"
                },
                new Partition
                {
                    Length   = 1882,
                    Name     = "Extra",
                    Offset   = 25249792,
                    Sequence = 3,
                    Size     = 963584,
                    Start    = 49316,
                    Type     = "Apple_Free"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "vcpformatter_2.1.1.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 24,
                    Offset   = 57344,
                    Sequence = 0,
                    Size     = 12288,
                    Start    = 112,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 32,
                    Name     = "Macintosh",
                    Offset   = 57344,
                    Sequence = 1,
                    Size     = 16384,
                    Start    = 112,
                    Type     = "Apple_Driver"
                },
                new Partition
                {
                    Length   = 32,
                    Name     = "Extra",
                    Offset   = 73728,
                    Sequence = 2,
                    Size     = 16384,
                    Start    = 144,
                    Type     = "Apple_Free"
                },
                new Partition
                {
                    Length   = 54662,
                    Name     = "MacOS",
                    Offset   = 90112,
                    Sequence = 3,
                    Size     = 27986944,
                    Start    = 176,
                    Type     = "Apple_HFS"
                },
                new Partition
                {
                    Length   = 2,
                    Name     = "Extra",
                    Offset   = 28077056,
                    Sequence = 4,
                    Size     = 1024,
                    Start    = 54838,
                    Type     = "Apple_Free"
                }
            ]
        }
    ];
}