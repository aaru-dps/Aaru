// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Atari.cs
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
// Copyright © 2011-2019 Natalia Portillo
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
    public class Atari
    {
        readonly string[] _testFiles =
        {
            "linux_ahdi.aif", "linux_icd.aif", "tos_1.04.aif"
        };

        readonly Partition[][] _wanted =
        {
            // Linux (AHDI)
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 31457280,
                    Name        = null,
                    Type        = "GEM",
                    Offset      = 512,
                    Length      = 61440,
                    Sequence    = 0,
                    Start       = 1
                },
                new Partition
                {
                    Description = null,
                    Size        = 41943040,
                    Name        = null,
                    Type        = "BGM",
                    Offset      = 31457792,
                    Length      = 81920,
                    Sequence    = 1,
                    Start       = 61441
                },
                new Partition
                {
                    Description = null,
                    Size        = 56402432,
                    Name        = null,
                    Type        = "LNX",
                    Offset      = 73400832,
                    Length      = 110161,
                    Sequence    = 2,
                    Start       = 143361
                },
                new Partition
                {
                    Description = null,
                    Size        = 43212800,
                    Name        = null,
                    Type        = "MAC",
                    Offset      = 129803264,
                    Length      = 84400,
                    Sequence    = 3,
                    Start       = 253522
                },
                new Partition
                {
                    Description = null,
                    Size        = 57671680,
                    Name        = null,
                    Type        = "MIX",
                    Offset      = 173016064,
                    Length      = 112640,
                    Sequence    = 4,
                    Start       = 337922
                },
                new Partition
                {
                    Description = null,
                    Size        = 62914560,
                    Name        = null,
                    Type        = "MNX",
                    Offset      = 230687744,
                    Length      = 122880,
                    Sequence    = 5,
                    Start       = 450562
                },
                new Partition
                {
                    Description = null,
                    Size        = 73400320,
                    Name        = null,
                    Type        = "RAW",
                    Offset      = 293602304,
                    Length      = 143360,
                    Sequence    = 6,
                    Start       = 573442
                },
                new Partition
                {
                    Description = null,
                    Size        = 78643200,
                    Name        = null,
                    Type        = "SWP",
                    Offset      = 367002624,
                    Length      = 153600,
                    Sequence    = 7,
                    Start       = 716802
                },
                new Partition
                {
                    Description = null,
                    Size        = 1048576,
                    Name        = null,
                    Type        = "UNX",
                    Offset      = 445645824,
                    Length      = 2048,
                    Sequence    = 8,
                    Start       = 870402
                },
                new Partition
                {
                    Description = null,
                    Size        = 77593600,
                    Name        = null,
                    Type        = "LNX",
                    Offset      = 446694400,
                    Length      = 151550,
                    Sequence    = 9,
                    Start       = 872450
                }
            },

            // Linux (ICD)
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 15728640,
                    Name        = null,
                    Type        = "GEM",
                    Offset      = 512,
                    Length      = 30720,
                    Sequence    = 0,
                    Start       = 1
                },
                new Partition
                {
                    Description = null,
                    Size        = 20971520,
                    Name        = null,
                    Type        = "UNX",
                    Offset      = 15729152,
                    Length      = 40960,
                    Sequence    = 1,
                    Start       = 30721
                },
                new Partition
                {
                    Description = null,
                    Size        = 31457280,
                    Name        = null,
                    Type        = "LNX",
                    Offset      = 36700672,
                    Length      = 61440,
                    Sequence    = 2,
                    Start       = 71681
                },
                new Partition
                {
                    Description = null,
                    Size        = 41943040,
                    Name        = null,
                    Type        = "BGM",
                    Offset      = 68157952,
                    Length      = 81920,
                    Sequence    = 3,
                    Start       = 133121
                },
                new Partition
                {
                    Description = null,
                    Size        = 52428800,
                    Name        = null,
                    Type        = "MAC",
                    Offset      = 110100992,
                    Length      = 102400,
                    Sequence    = 4,
                    Start       = 215041
                },
                new Partition
                {
                    Description = null,
                    Size        = 62914560,
                    Name        = null,
                    Type        = "MIX",
                    Offset      = 162529792,
                    Length      = 122880,
                    Sequence    = 5,
                    Start       = 317441
                },
                new Partition
                {
                    Description = null,
                    Size        = 83886080,
                    Name        = null,
                    Type        = "SWP",
                    Offset      = 225444352,
                    Length      = 163840,
                    Sequence    = 6,
                    Start       = 440321
                },
                new Partition
                {
                    Description = null,
                    Size        = 103809024,
                    Name        = null,
                    Type        = "MNX",
                    Offset      = 309330432,
                    Length      = 202752,
                    Sequence    = 7,
                    Start       = 604161
                },
                new Partition
                {
                    Description = null,
                    Size        = 104857600,
                    Name        = null,
                    Type        = "LNX",
                    Offset      = 413139456,
                    Length      = 204800,
                    Sequence    = 8,
                    Start       = 806913
                }
            },

            // TOS 1.04
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 7340032,
                    Name        = null,
                    Type        = "GEM",
                    Offset      = 1024,
                    Length      = 14336,
                    Sequence    = 0,
                    Start       = 2
                },
                new Partition
                {
                    Description = null,
                    Size        = 7340032,
                    Name        = null,
                    Type        = "GEM",
                    Offset      = 7341056,
                    Length      = 14336,
                    Sequence    = 1,
                    Start       = 14338
                },
                new Partition
                {
                    Description = null,
                    Size        = 7340032,
                    Name        = null,
                    Type        = "GEM",
                    Offset      = 14681088,
                    Length      = 14336,
                    Sequence    = 2,
                    Start       = 28674
                },
                new Partition
                {
                    Description = null,
                    Size        = 7339008,
                    Name        = null,
                    Type        = "GEM",
                    Offset      = 22021120,
                    Length      = 14334,
                    Sequence    = 3,
                    Start       = 43010
                }
            }
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Partitioning schemes", "Atari ST",
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