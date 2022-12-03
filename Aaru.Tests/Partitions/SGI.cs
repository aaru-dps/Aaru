// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SGI.cs
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using NUnit.Framework;

namespace Aaru.Tests.Partitions;

[TestFixture]
public class Sgi : PartitionSchemeTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Partitioning schemes", "SGI");

    public override PartitionTest[] Tests => new[]
    {
        new PartitionTest
        {
            TestFile = "linux.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 40961,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 16065,
                    Type     = "XFS"
                },
                new Partition
                {
                    Length   = 61441,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 64260,
                    Type     = "Linux RAID"
                },
                new Partition
                {
                    Length   = 81921,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 128520,
                    Type     = "Track replacements"
                },
                new Partition
                {
                    Length   = 92161,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 224910,
                    Type     = "Sector replacements"
                },
                new Partition
                {
                    Length   = 102401,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 321300,
                    Type     = "Raw data (swap)"
                },
                new Partition
                {
                    Length   = 30721,
                    Offset   = 0,
                    Sequence = 5,
                    Size     = 0,
                    Start    = 433755,
                    Type     = "4.2BSD Fast File System"
                },
                new Partition
                {
                    Length   = 71681,
                    Offset   = 0,
                    Sequence = 6,
                    Size     = 0,
                    Start    = 465885,
                    Type     = "UNIX System V"
                },
                new Partition
                {
                    Length   = 10241,
                    Offset   = 0,
                    Sequence = 7,
                    Size     = 0,
                    Start    = 546210,
                    Type     = "EFS"
                },
                new Partition
                {
                    Length   = 122881,
                    Offset   = 0,
                    Sequence = 8,
                    Size     = 0,
                    Start    = 562275,
                    Type     = "Logical volume"
                },
                new Partition
                {
                    Length   = 133121,
                    Offset   = 0,
                    Sequence = 9,
                    Size     = 0,
                    Start    = 690795,
                    Type     = "Raw logical volume"
                },
                new Partition
                {
                    Length   = 51201,
                    Offset   = 0,
                    Sequence = 10,
                    Size     = 0,
                    Start    = 835380,
                    Type     = "XFS log device"
                },
                new Partition
                {
                    Length   = 30721,
                    Offset   = 0,
                    Sequence = 11,
                    Size     = 0,
                    Start    = 899640,
                    Type     = "Linux swap"
                },
                new Partition
                {
                    Length   = 6145,
                    Offset   = 0,
                    Sequence = 12,
                    Size     = 0,
                    Start    = 931770,
                    Type     = "SGI XVM"
                },
                new Partition
                {
                    Length   = 64260,
                    Offset   = 0,
                    Sequence = 13,
                    Size     = 0,
                    Start    = 947835,
                    Type     = "Linux"
                }
            }
        },
        new PartitionTest
        {
            TestFile = "parted.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 22528,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 6144,
                    Type     = "Raw data (swap)"
                },
                new Partition
                {
                    Length   = 67584,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 30720,
                    Type     = "Raw data (swap)"
                },
                new Partition
                {
                    Length   = 94208,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 100352,
                    Type     = "Raw data (swap)"
                },
                new Partition
                {
                    Length   = 36864,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 196608,
                    Type     = "XFS"
                }
            }
        }
    };
}