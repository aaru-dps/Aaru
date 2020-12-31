// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : GPT.cs
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
// Copyright © 2011-2021 Natalia Portillo
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
    public class Gpt
    {
        readonly string[] _testFiles =
        {
            "linux.aif", "parted.aif"
        };

        readonly Partition[][] _wanted =
        {
            // Linux
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 10485760,
                    Name        = "EFI System",
                    Type        = "EFI System",
                    Offset      = 1048576,
                    Length      = 20480,
                    Sequence    = 0,
                    Start       = 2048
                },
                new Partition
                {
                    Description = null,
                    Size        = 15728640,
                    Name        = "Microsoft basic data",
                    Type        = "Microsoft Basic data",
                    Offset      = 11534336,
                    Length      = 30720,
                    Sequence    = 1,
                    Start       = 22528
                },
                new Partition
                {
                    Description = null,
                    Size        = 20971520,
                    Name        = "Apple label",
                    Type        = "Apple Label",
                    Offset      = 27262976,
                    Length      = 40960,
                    Sequence    = 2,
                    Start       = 53248
                },
                new Partition
                {
                    Description = null,
                    Size        = 26214400,
                    Name        = "Solaris /usr & Mac ZFS",
                    Type        = "Solaris /usr or Apple ZFS",
                    Offset      = 48234496,
                    Length      = 51200,
                    Sequence    = 3,
                    Start       = 94208
                },
                new Partition
                {
                    Description = null,
                    Size        = 31457280,
                    Name        = "FreeBSD ZFS",
                    Type        = "FreeBSD ZFS",
                    Offset      = 74448896,
                    Length      = 61440,
                    Sequence    = 4,
                    Start       = 145408
                },
                new Partition
                {
                    Description = null,
                    Size        = 28294656,
                    Name        = "HP-UX data",
                    Type        = "HP-UX Data",
                    Offset      = 105906176,
                    Length      = 55263,
                    Sequence    = 5,
                    Start       = 206848
                }
            },

            // Parted
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 42991616,
                    Name        = "",
                    Type        = "Apple HFS",
                    Offset      = 1048576,
                    Length      = 83968,
                    Sequence    = 0,
                    Start       = 2048
                },
                new Partition
                {
                    Description = null,
                    Size        = 52428800,
                    Name        = "",
                    Type        = "Linux filesystem",
                    Offset      = 44040192,
                    Length      = 102400,
                    Sequence    = 1,
                    Start       = 86016
                },
                new Partition
                {
                    Description = null,
                    Size        = 36700160,
                    Name        = "",
                    Type        = "Microsoft Basic data",
                    Offset      = 96468992,
                    Length      = 71680,
                    Sequence    = 2,
                    Start       = 188416
                }
            }
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Partitioning schemes", "GUID Partition Table",
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