// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : SGI.cs
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
    public class Sgi
    {
        readonly string[] testfiles = {"linux.vdi.lz", "parted.vdi.lz"};

        readonly Partition[][] wanted =
        {
            // Linux's fdisk
            new[]
            {
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "XFS",
                    Length      = 40961,
                    Sequence    = 0,
                    Start       = 16065
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Linux RAID",
                    Length      = 61441,
                    Sequence    = 1,
                    Start       = 64260
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Track replacements",
                    Length      = 81921,
                    Sequence    = 2,
                    Start       = 128520
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Sector replacements",
                    Length      = 92161,
                    Sequence    = 3,
                    Start       = 224910
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Raw data (swap)",
                    Length      = 102401,
                    Sequence    = 4,
                    Start       = 321300
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "4.2BSD Fast File System",
                    Length      = 30721,
                    Sequence    = 5,
                    Start       = 433755
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "UNIX System V",
                    Length      = 71681,
                    Sequence    = 6,
                    Start       = 465885
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "EFS",
                    Length      = 10241,
                    Sequence    = 7,
                    Start       = 546210
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Logical volume",
                    Length      = 122881,
                    Sequence    = 8,
                    Start       = 562275
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Raw logical volume",
                    Length      = 133121,
                    Sequence    = 9,
                    Start       = 690795
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "XFS log device",
                    Length      = 51201,
                    Sequence    = 10,
                    Start       = 835380
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Linux swap",
                    Length      = 30721,
                    Sequence    = 11,
                    Start       = 899640
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "SGI XVM",
                    Length      = 6145,
                    Sequence    = 12,
                    Start       = 931770
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Linux",
                    Length      = 64260,
                    Sequence    = 13,
                    Start       = 947835
                }
            },
            // GNU Parted
            new[]
            {
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Raw data (swap)",
                    Length      = 22528,
                    Sequence    = 0,
                    Start       = 6144
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Raw data (swap)",
                    Length      = 67584,
                    Sequence    = 1,
                    Start       = 30720
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Raw data (swap)",
                    Length      = 94208,
                    Sequence    = 2,
                    Start       = 100352
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "XFS",
                    Length      = 36864,
                    Sequence    = 3,
                    Start       = 196608
                }
            }
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "partitions", "sgi", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                Assert.AreEqual(wanted[i].Length, partitions.Count, testfiles[i]);
                for(int j = 0; j < partitions.Count; j++)
                {
                    // Too chatty
                    //Assert.AreEqual(wanted[i][j].PartitionDescription, partitions[j].PartitionDescription, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Length * 512, partitions[j].Size, testfiles[i]);
                    //                    Assert.AreEqual(wanted[i][j].Name, partitions[j].Name, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Type,        partitions[j].Type,     testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Start * 512, partitions[j].Offset,   testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Length,      partitions[j].Length,   testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Sequence,    partitions[j].Sequence, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Start,       partitions[j].Start,    testfiles[i]);
                }
            }
        }
    }
}