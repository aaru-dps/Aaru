// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : RDB.cs
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using Aaru.Filters;
using NUnit.Framework;

namespace Aaru.Tests.Partitions
{
    [TestFixture]
    public class Rdb
    {
        readonly string[] _testFiles =
        {
            "amigaos_3.9.aif", "amigaos_4.0.aif", "parted.aif"
        };

        readonly Partition[][] _wanted =
        {
            // AmigaOS 3.9
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 87392256,
                    Name        = "UDH0",
                    Type        = "\"DOS\\0\"",
                    Offset      = 2080768,
                    Length      = 170688,
                    Sequence    = 0,
                    Start       = 4064
                },
                new Partition
                {
                    Description = null,
                    Size        = 87392256,
                    Name        = "UDH1",
                    Type        = "\"DOS\\2\"",
                    Offset      = 89473024,
                    Length      = 170688,
                    Sequence    = 1,
                    Start       = 174752
                },
                new Partition
                {
                    Description = null,
                    Size        = 87392256,
                    Name        = "UDH2",
                    Type        = "\"DOS\\1\"",
                    Offset      = 176865280,
                    Length      = 170688,
                    Sequence    = 2,
                    Start       = 345440
                },
                new Partition
                {
                    Description = null,
                    Size        = 87392256,
                    Name        = "UDH3",
                    Type        = "\"DOS\\3\"",
                    Offset      = 264257536,
                    Length      = 170688,
                    Sequence    = 3,
                    Start       = 516128
                },
                new Partition
                {
                    Description = null,
                    Size        = 87300096,
                    Name        = "FAT16",
                    Type        = "0x06",
                    Offset      = 351663104,
                    Length      = 170508,
                    Sequence    = 4,
                    Start       = 686842
                },
                new Partition
                {
                    Description = null,
                    Size        = 85311488,
                    Name        = "UDH5",
                    Type        = "\"RES\\86\"",
                    Offset      = 439042048,
                    Length      = 166624,
                    Sequence    = 5,
                    Start       = 857504
                }
            },

            // AmigaOS 4.0
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 91455488,
                    Name        = "DH1",
                    Type        = "\"DOS\\1\"",
                    Offset      = 1048576,
                    Length      = 178624,
                    Sequence    = 0,
                    Start       = 2048
                },
                new Partition
                {
                    Description = null,
                    Size        = 76546048,
                    Name        = "DH2",
                    Type        = "\"DOS\\3\"",
                    Offset      = 92504064,
                    Length      = 149504,
                    Sequence    = 1,
                    Start       = 180672
                },
                new Partition
                {
                    Description = null,
                    Size        = 78741504,
                    Name        = "DH3",
                    Type        = "\"DOS\\3\"",
                    Offset      = 169050112,
                    Length      = 153792,
                    Sequence    = 2,
                    Start       = 330176
                },
                new Partition
                {
                    Description = null,
                    Size        = 78020608,
                    Name        = "DH4",
                    Type        = "\"DOS\\7\"",
                    Offset      = 247791616,
                    Length      = 152384,
                    Sequence    = 3,
                    Start       = 483968
                },
                new Partition
                {
                    Description = null,
                    Size        = 85000192,
                    Name        = "DH5",
                    Type        = "\"SFS\\0\"",
                    Offset      = 325812224,
                    Length      = 166016,
                    Sequence    = 4,
                    Start       = 636352
                },
                new Partition
                {
                    Description = null,
                    Size        = 113541120,
                    Name        = "DH6",
                    Type        = "\"SFS\\2\"",
                    Offset      = 410812416,
                    Length      = 221760,
                    Sequence    = 5,
                    Start       = 802368
                }
            },

            // Parted
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 8225280,
                    Name        = "primary",
                    Type        = "\"\0\0\0\\0\"",
                    Offset      = 8225280,
                    Length      = 16065,
                    Sequence    = 0,
                    Start       = 16065
                },
                new Partition
                {
                    Description = null,
                    Size        = 24675840,
                    Name        = "name",
                    Type        = "\"FAT\\1\"",
                    Offset      = 16450560,
                    Length      = 48195,
                    Sequence    = 1,
                    Start       = 32130
                },
                new Partition
                {
                    Description = null,
                    Size        = 90478080,
                    Name        = "partition",
                    Type        = "\"\0\0\0\\0\"",
                    Offset      = 41126400,
                    Length      = 176715,
                    Sequence    = 2,
                    Start       = 80325
                }
            }
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Partitioning schemes", "Rigid Disk Block",
                                               _testFiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                Assert.AreEqual(_wanted[i].Length, partitions.Count, _testFiles[i]);

                for(int j = 0; j < partitions.Count; j++)
                {
                    // Too chatty
                    //Assert.AreEqual(wanted[i][j].PartitionDescription, partitions[j].PartitionDescription, testfiles[i]);
                    Assert.AreEqual(_wanted[i][j].Size, partitions[j].Size, _testFiles[i]);
                    Assert.AreEqual(_wanted[i][j].Name, partitions[j].Name, _testFiles[i]);
                    Assert.AreEqual(_wanted[i][j].Type, partitions[j].Type, _testFiles[i]);
                    Assert.AreEqual(_wanted[i][j].Offset, partitions[j].Offset, _testFiles[i]);
                    Assert.AreEqual(_wanted[i][j].Length, partitions[j].Length, _testFiles[i]);
                    Assert.AreEqual(_wanted[i][j].Sequence, partitions[j].Sequence, _testFiles[i]);
                    Assert.AreEqual(_wanted[i][j].Start, partitions[j].Start, _testFiles[i]);
                }
            }
        }
    }
}