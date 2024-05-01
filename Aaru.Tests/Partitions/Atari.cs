// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Atari.cs
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
public class Atari : PartitionSchemeTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Partitioning schemes", "Atari ST");

    public override PartitionTest[] Tests =>
    [
        new PartitionTest
        {
            TestFile = "linux_ahdi.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 61440,
                    Offset   = 512,
                    Sequence = 0,
                    Size     = 31457280,
                    Start    = 1,
                    Type     = "GEM"
                },
                new Partition
                {
                    Length   = 81920,
                    Offset   = 31457792,
                    Sequence = 1,
                    Size     = 41943040,
                    Start    = 61441,
                    Type     = "BGM"
                },
                new Partition
                {
                    Description = "Macintosh partition",
                    Length      = 102399,
                    Offset      = 512,
                    Sequence    = 1,
                    Size        = 52428288,
                    Start       = 1,
                    Type        = "MAC"
                },
                new Partition
                {
                    Description = "Linux partition",
                    Length      = 92160,
                    Offset      = 73400832,
                    Sequence    = 3,
                    Size        = 47185920,
                    Start       = 143361,
                    Type        = "LNX"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "linux_icd.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 30720,
                    Offset   = 512,
                    Sequence = 0,
                    Size     = 15728640,
                    Start    = 1,
                    Type     = "GEM"
                },
                new Partition
                {
                    Length   = 40960,
                    Offset   = 15729152,
                    Sequence = 1,
                    Size     = 20971520,
                    Start    = 30721,
                    Type     = "UNX"
                },
                new Partition
                {
                    Length   = 61440,
                    Offset   = 36700672,
                    Sequence = 2,
                    Size     = 31457280,
                    Start    = 71681,
                    Type     = "LNX"
                },
                new Partition
                {
                    Length   = 81920,
                    Offset   = 68157952,
                    Sequence = 3,
                    Size     = 41943040,
                    Start    = 133121,
                    Type     = "BGM"
                },
                new Partition
                {
                    Length   = 102400,
                    Offset   = 110100992,
                    Sequence = 4,
                    Size     = 52428800,
                    Start    = 215041,
                    Type     = "MAC"
                },
                new Partition
                {
                    Length   = 122880,
                    Offset   = 162529792,
                    Sequence = 5,
                    Size     = 62914560,
                    Start    = 317441,
                    Type     = "MIX"
                },
                new Partition
                {
                    Length   = 163840,
                    Offset   = 225444352,
                    Sequence = 6,
                    Size     = 83886080,
                    Start    = 440321,
                    Type     = "SWP"
                },
                new Partition
                {
                    Length   = 202752,
                    Offset   = 309330432,
                    Sequence = 7,
                    Size     = 103809024,
                    Start    = 604161,
                    Type     = "MNX"
                },
                new Partition
                {
                    Length   = 204800,
                    Offset   = 413139456,
                    Sequence = 8,
                    Size     = 104857600,
                    Start    = 806913,
                    Type     = "LNX"
                }
            ]
        },
        new PartitionTest
        {
            TestFile = "tos_1.04.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 14336,
                    Offset   = 1024,
                    Sequence = 0,
                    Size     = 7340032,
                    Start    = 2,
                    Type     = "GEM"
                },
                new Partition
                {
                    Length   = 14336,
                    Offset   = 7341056,
                    Sequence = 1,
                    Size     = 7340032,
                    Start    = 14338,
                    Type     = "GEM"
                },
                new Partition
                {
                    Length   = 14336,
                    Offset   = 14681088,
                    Sequence = 2,
                    Size     = 7340032,
                    Start    = 28674,
                    Type     = "GEM"
                },
                new Partition
                {
                    Length   = 14334,
                    Offset   = 22021120,
                    Sequence = 3,
                    Size     = 7339008,
                    Start    = 43010,
                    Type     = "GEM"
                }
            ]
        }
    ];
}