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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using NUnit.Framework;

namespace Aaru.Tests.Partitions
{
    [TestFixture]
    public class Acorn : PartitionSchemeTest
    {
        public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Partitioning schemes", "Acorn");

        public override PartitionTest[] Tests => new[]
        {
            new PartitionTest
            {
                TestFile = "linux_ics.aif",
                Partitions = new[]
                {
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
                        Length   = 110161,
                        Offset   = 73400832,
                        Sequence = 2,
                        Size     = 56402432,
                        Start    = 143361,
                        Type     = "LNX"
                    },
                    new Partition
                    {
                        Length   = 84400,
                        Offset   = 129803264,
                        Sequence = 3,
                        Size     = 43212800,
                        Start    = 253522,
                        Type     = "MAC"
                    }
                }
            }
        };
    }
}