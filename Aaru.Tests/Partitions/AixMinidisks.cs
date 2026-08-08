// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AixMinidisks.cs
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using NUnit.Framework;

namespace Aaru.Tests.Partitions;

[TestFixture]
public class AixMinidisks : PartitionSchemeTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Partitioning schemes", "AIX minidisks");

    public override PartitionTest[] Tests =>
    [
        new()
        {
            TestFile = "aixps2_1.3.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 11608,
                    Offset   = 147456,
                    Sequence = 0,
                    Size     = 5943296,
                    Start    = 288,
                    Type     = "AIX filesystem"
                },
                new Partition
                {
                    Length   = 261792,
                    Offset   = 147456,
                    Sequence = 1,
                    Size     = 134037504,
                    Start    = 288,
                    Type     = "0x09"
                },
                new Partition
                {
                    Length   = 8000,
                    Offset   = 6090752,
                    Sequence = 2,
                    Size     = 4096000,
                    Start    = 11896,
                    Type     = "AIX dump space"
                },
                new Partition
                {
                    Length   = 8000,
                    Offset   = 10186752,
                    Sequence = 3,
                    Size     = 4096000,
                    Start    = 19896,
                    Type     = "AIX paging space"
                },
                new Partition
                {
                    Length   = 199128,
                    Offset   = 14282752,
                    Sequence = 4,
                    Size     = 101953536,
                    Start    = 27896,
                    Type     = "AIX filesystem"
                },
                new Partition
                {
                    Length   = 15408,
                    Offset   = 116236288,
                    Sequence = 5,
                    Size     = 7888896,
                    Start    = 227024,
                    Type     = "AIX boot"
                },
                new Partition
                {
                    Length   = 19648,
                    Offset   = 124125184,
                    Sequence = 6,
                    Size     = 10059776,
                    Start    = 242432,
                    Type     = "AIX filesystem"
                }
            ]
        }
    ];
}