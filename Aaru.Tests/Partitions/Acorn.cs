// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Acorn.cs
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
public class Acorn : PartitionSchemeTest
{
    public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Partitioning schemes", "Acorn");

    public override PartitionTest[] Tests =>
    [
        new PartitionTest
        {
            TestFile = "linux_ics.aif",
            Partitions =
            [
                new Partition
                {
                    Length   = 99792,
                    Offset   = 519096,
                    Sequence = 0,
                    Size     = 51093504,
                    Start    = 1008,
                    Type     = "Linux"
                },
                new Partition
                {
                    Length   = 368928,
                    Offset   = 103219200,
                    Sequence = 1,
                    Size     = 188891136,
                    Start    = 201600,
                    Type     = "Linux"
                },
                new Partition
                {
                    Length   = 359856,
                    Offset   = 343719936,
                    Sequence = 2,
                    Size     = 184246272,
                    Start    = 671328,
                    Type     = "Linux swap"
                }
            ]
        }
    ];
}