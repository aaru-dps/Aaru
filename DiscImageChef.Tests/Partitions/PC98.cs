// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : PC98.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef unit testing.
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using DiscImageChef.Filters;
using NUnit.Framework;

namespace DiscImageChef.Tests.Partitions
{
    [TestFixture]
    public class Pc98
    {
        readonly string[] testfiles =
            {"msdos330.thd.lz", "msdos330_alt.thd.lz", "msdos500_epson.thd.lz", "msdos500.thd.lz", "msdos620.thd.lz"};

        readonly Partition[][] wanted =
        {
            // NEC MS-DOS 3.30 (256Mb HDD)
            new[]
            {
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 3.30",
                    Type        = "FAT12",
                    Length      = 19536,
                    Sequence    = 0,
                    Start       = 264
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 3.30",
                    Type        = "FAT12",
                    Length      = 39336,
                    Sequence    = 1,
                    Start       = 20064
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 3.30",
                    Type        = "FAT16",
                    Length      = 59136,
                    Sequence    = 2,
                    Start       = 59664
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 3.30",
                    Type        = "FAT16",
                    Length      = 78936,
                    Sequence    = 3,
                    Start       = 119064
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 3.30",
                    Type        = "FAT16",
                    Length      = 118536,
                    Sequence    = 4,
                    Start       = 198264
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 3.30",
                    Type        = "FAT16",
                    Length      = 197736,
                    Sequence    = 5,
                    Start       = 317064
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 3.30",
                    Type        = "FAT16",
                    Length      = 237336,
                    Sequence    = 6,
                    Start       = 515064
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 3.30",
                    Type        = "FAT16",
                    Length      = 245256,
                    Sequence    = 7,
                    Start       = 752664
                }
            },
            // NEC MS-DOS 3.30 (80Mb HDD)
            new[]
            {
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 3.30",
                    Type        = "FAT16",
                    Length      = 59136,
                    Sequence    = 0,
                    Start       = 264
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 3.30",
                    Type        = "FAT16",
                    Length      = 158136,
                    Sequence    = 1,
                    Start       = 59664
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 3.30",
                    Type        = "FAT16",
                    Length      = 94776,
                    Sequence    = 2,
                    Start       = 218064
                }
            },
            // Epson MS-DOS 3.30
            new[]
            {
                new Partition
                {
                    Description = null,
                    Name        = "NamenameName",
                    Type        = "FAT12",
                    Length      = 35639,
                    Sequence    = 0,
                    Start       = 264
                },
                new Partition
                {
                    Description = null,
                    Name        = "12BitFAT",
                    Type        = "FAT12",
                    Length      = 59399,
                    Sequence    = 1,
                    Start       = 35904
                },
                new Partition
                {
                    Description = null,
                    Name        = "16BitFAT",
                    Type        = "FAT16",
                    Length      = 79199,
                    Sequence    = 2,
                    Start       = 95304
                },
                new Partition
                {
                    Description = null,
                    Name        = "PartLblMaxNameXX",
                    Type        = "FAT16",
                    Length      = 118799,
                    Sequence    = 3,
                    Start       = 174504
                },
                new Partition
                {
                    Description = null,
                    Name        = "BigFAT12",
                    Type        = "FAT12",
                    Length      = 158399,
                    Sequence    = 4,
                    Start       = 293304
                },
                new Partition
                {
                    Description = null,
                    Name        = "Lalalalalalalala",
                    Type        = "FAT16",
                    Length      = 197999,
                    Sequence    = 5,
                    Start       = 451704
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS Ver 5.0",
                    Type        = "FAT16",
                    Length      = 237599,
                    Sequence    = 6,
                    Start       = 649704
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS Ver 5.0",
                    Type        = "FAT16",
                    Length      = 118799,
                    Sequence    = 7,
                    Start       = 887304
                }
            },
            // NEC MS-DOS 5.00
            new[]
            {
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 5.00",
                    Type        = "FAT12",
                    Length      = 28512,
                    Sequence    = 0,
                    Start       = 264
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 5.00",
                    Type        = "FAT16",
                    Length      = 49104,
                    Sequence    = 1,
                    Start       = 29040
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 5.00",
                    Type        = "FAT16",
                    Length      = 93984,
                    Sequence    = 2,
                    Start       = 78408
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 5.00",
                    Type        = "FAT16",
                    Length      = 122760,
                    Sequence    = 3,
                    Start       = 172656
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 5.00",
                    Type        = "FAT16",
                    Length      = 163680,
                    Sequence    = 4,
                    Start       = 295680
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 5.00",
                    Type        = "FAT16",
                    Length      = 204600,
                    Sequence    = 5,
                    Start       = 459624
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 5.00",
                    Type        = "FAT16",
                    Length      = 204600,
                    Sequence    = 6,
                    Start       = 664488
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 5.00",
                    Type        = "FAT16",
                    Length      = 139128,
                    Sequence    = 7,
                    Start       = 869352
                }
            },
            // NEC MS-DOS 6.20
            new[]
            {
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 6.20",
                    Type        = "FAT16",
                    Length      = 61248,
                    Sequence    = 0,
                    Start       = 264
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 6.20",
                    Type        = "FAT16",
                    Length      = 81840,
                    Sequence    = 1,
                    Start       = 61776
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 6.20",
                    Type        = "FAT16",
                    Length      = 122760,
                    Sequence    = 2,
                    Start       = 143880
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 6.20",
                    Type        = "FAT16",
                    Length      = 163680,
                    Sequence    = 3,
                    Start       = 266904
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 6.20",
                    Type        = "FAT12",
                    Length      = 20328,
                    Sequence    = 4,
                    Start       = 430848
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 6.20",
                    Type        = "FAT16",
                    Length      = 245520,
                    Sequence    = 5,
                    Start       = 451440
                },
                new Partition
                {
                    Description = null,
                    Name        = "MS-DOS 6.20",
                    Type        = "FAT16",
                    Length      = 315216,
                    Sequence    = 6,
                    Start       = 697224
                }
            }
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "partitions", "pc98", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new T98();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                Assert.AreEqual(wanted[i].Length, partitions.Count, testfiles[i]);
                for(int j = 0; j < partitions.Count; j++)
                {
                    // Too chatty
                    //Assert.AreEqual(wanted[i][j].PartitionDescription, partitions[j].PartitionDescription, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Length,   partitions[j].Length,   testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Name,     partitions[j].Name,     testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Type,     partitions[j].Type,     testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Sequence, partitions[j].Sequence, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Start,    partitions[j].Start,    testfiles[i]);
                }
            }
        }
    }
}