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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using NUnit.Framework;

namespace Aaru.Tests.Partitions;

// TODO: Get SunOS and VTOC16 disk labels
[TestFixture]
public class Sun : PartitionSchemeTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Partitioning schemes", "Sun");

    public override PartitionTest[] Tests =>
    [
        new PartitionTest
        {
            TestFile = "linux.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 204800,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 0,
                    Type     = "Linux"
                },
                new Partition
                {
                    Length   = 102400,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 208845,
                    Type     = "Sun boot"
                },
                new Partition
                {
                    Length   = 102400,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 321300,
                    Type     = "Sun /"
                },
                new Partition
                {
                    Length   = 102400,
                    Offset   = 0,
                    Sequence = 3,
                    Size     = 0,
                    Start    = 433755,
                    Type     = "Sun /home"
                },
                new Partition
                {
                    Length   = 153600,
                    Offset   = 0,
                    Sequence = 4,
                    Size     = 0,
                    Start    = 546210,
                    Type     = "Sun swap"
                },
                new Partition
                {
                    Length   = 208845,
                    Offset   = 0,
                    Sequence = 5,
                    Size     = 0,
                    Start    = 706860,
                    Type     = "Sun /usr"
                },
                new Partition
                {
                    Length   = 96390,
                    Offset   = 0,
                    Sequence = 6,
                    Size     = 0,
                    Start    = 915705,
                    Type     = "Linux swap"
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
                    Length   = 49152,
                    Offset   = 0,
                    Sequence = 0,
                    Size     = 0,
                    Start    = 0,
                    Type     = "Linux"
                },
                new Partition
                {
                    Length   = 80325,
                    Offset   = 0,
                    Sequence = 1,
                    Size     = 0,
                    Start    = 64260,
                    Type     = "Linux"
                },
                new Partition
                {
                    Length   = 96390,
                    Offset   = 0,
                    Sequence = 2,
                    Size     = 0,
                    Start    = 144585,
                    Type     = "Linux"
                }
            ]
        }
    ];
}