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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using NUnit.Framework;

namespace Aaru.Tests.Partitions;

[TestFixture]
public class Rdb : PartitionSchemeTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Partitioning schemes", "Rigid Disk Block");

    public override PartitionTest[] Tests => new[]
    {
        new PartitionTest
        {
            TestFile = "amigaos_3.9.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 170688,
                    Name     = "UDH0",
                    Offset   = 2080768,
                    Sequence = 0,
                    Size     = 87392256,
                    Start    = 4064,
                    Type     = "\"DOS\\0\""
                },
                new Partition
                {
                    Length   = 170688,
                    Name     = "UDH1",
                    Offset   = 89473024,
                    Sequence = 1,
                    Size     = 87392256,
                    Start    = 174752,
                    Type     = "\"DOS\\2\""
                },
                new Partition
                {
                    Length   = 170688,
                    Name     = "UDH2",
                    Offset   = 176865280,
                    Sequence = 2,
                    Size     = 87392256,
                    Start    = 345440,
                    Type     = "\"DOS\\1\""
                },
                new Partition
                {
                    Length   = 170688,
                    Name     = "UDH3",
                    Offset   = 264257536,
                    Sequence = 3,
                    Size     = 87392256,
                    Start    = 516128,
                    Type     = "\"DOS\\3\""
                },
                new Partition
                {
                    Length   = 170508,
                    Name     = "FAT16",
                    Offset   = 351663104,
                    Sequence = 4,
                    Size     = 87300096,
                    Start    = 686842,
                    Type     = "0x06"
                },
                new Partition
                {
                    Length   = 166624,
                    Name     = "UDH5",
                    Offset   = 439042048,
                    Sequence = 5,
                    Size     = 85311488,
                    Start    = 857504,
                    Type     = "\"RES\\86\""
                }
            }
        },
        new PartitionTest
        {
            TestFile = "amigaos_4.0.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 178624,
                    Name     = "DH1",
                    Offset   = 1048576,
                    Sequence = 0,
                    Size     = 91455488,
                    Start    = 2048,
                    Type     = "\"DOS\\1\""
                },
                new Partition
                {
                    Length   = 149504,
                    Name     = "DH2",
                    Offset   = 92504064,
                    Sequence = 1,
                    Size     = 76546048,
                    Start    = 180672,
                    Type     = "\"DOS\\3\""
                },
                new Partition
                {
                    Length   = 153792,
                    Name     = "DH3",
                    Offset   = 169050112,
                    Sequence = 2,
                    Size     = 78741504,
                    Start    = 330176,
                    Type     = "\"DOS\\3\""
                },
                new Partition
                {
                    Length   = 152384,
                    Name     = "DH4",
                    Offset   = 247791616,
                    Sequence = 3,
                    Size     = 78020608,
                    Start    = 483968,
                    Type     = "\"DOS\\7\""
                },
                new Partition
                {
                    Length   = 166016,
                    Name     = "DH5",
                    Offset   = 325812224,
                    Sequence = 4,
                    Size     = 85000192,
                    Start    = 636352,
                    Type     = "\"SFS\\0\""
                },
                new Partition
                {
                    Length   = 221760,
                    Name     = "DH6",
                    Offset   = 410812416,
                    Sequence = 5,
                    Size     = 113541120,
                    Start    = 802368,
                    Type     = "\"SFS\\2\""
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
                    Length   = 16065,
                    Name     = "primary",
                    Offset   = 8225280,
                    Sequence = 0,
                    Size     = 8225280,
                    Start    = 16065,
                    Type     = "\"\0\0\0\\0\""
                },
                new Partition
                {
                    Length   = 48195,
                    Name     = "name",
                    Offset   = 16450560,
                    Sequence = 1,
                    Size     = 24675840,
                    Start    = 32130,
                    Type     = "\"FAT\\1\""
                },
                new Partition
                {
                    Length   = 176715,
                    Name     = "partition",
                    Offset   = 41126400,
                    Sequence = 2,
                    Size     = 90478080,
                    Start    = 80325,
                    Type     = "\"\0\0\0\\0\""
                }
            }
        }
    };
}