// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Sun.cs
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
    // TODO: Get SunOS and VTOC16 disk labels
    [TestFixture]
    public class Sun
    {
        readonly string[] _testFiles =
        {
            "linux.aif", "parted.aif"
        };

        readonly Partition[][] _wanted =
        {
            // Linux's fdisk
            new[]
            {
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Linux",
                    Length      = 204800,
                    Sequence    = 0,
                    Start       = 0
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Sun boot",
                    Length      = 102400,
                    Sequence    = 1,
                    Start       = 208845
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Sun /",
                    Length      = 102400,
                    Sequence    = 2,
                    Start       = 321300
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Sun /home",
                    Length      = 102400,
                    Sequence    = 3,
                    Start       = 433755
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Sun swap",
                    Length      = 153600,
                    Sequence    = 4,
                    Start       = 546210
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Sun /usr",
                    Length      = 208845,
                    Sequence    = 5,
                    Start       = 706860
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Linux swap",
                    Length      = 96390,
                    Sequence    = 6,
                    Start       = 915705
                }
            },

            // GNU Parted
            new[]
            {
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Linux",
                    Length      = 49152,
                    Sequence    = 0,
                    Start       = 0
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Linux",
                    Length      = 80325,
                    Sequence    = 1,
                    Start       = 64260
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Linux",
                    Length      = 96390,
                    Sequence    = 2,
                    Start       = 144585
                }
            }
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Partitioning schemes", "Sun", _testFiles[i]);
                IFilter filter   = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                Assert.AreEqual(_wanted[i].Length, partitions.Count, _testFiles[i]);

                for(int j = 0; j < partitions.Count; j++)
                {
                    // Too chatty
                    //Assert.AreEqual(wanted[i][j].PartitionDescription, partitions[j].PartitionDescription, testfiles[i]);
                    Assert.AreEqual(_wanted[i][j].Length * 512, partitions[j].Size, _testFiles[i]);

                    //                    Assert.AreEqual(wanted[i][j].Name, partitions[j].Name, testfiles[i]);
                    Assert.AreEqual(_wanted[i][j].Type, partitions[j].Type, _testFiles[i]);
                    Assert.AreEqual(_wanted[i][j].Start * 512, partitions[j].Offset, _testFiles[i]);
                    Assert.AreEqual(_wanted[i][j].Length, partitions[j].Length, _testFiles[i]);
                    Assert.AreEqual(_wanted[i][j].Sequence, partitions[j].Sequence, _testFiles[i]);
                    Assert.AreEqual(_wanted[i][j].Start, partitions[j].Start, _testFiles[i]);
                }
            }
        }
    }
}