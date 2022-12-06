// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MINIX.cs
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

namespace Aaru.Tests.Partitions
{
    [TestFixture]
    public class Minix : PartitionSchemeTest
    {
        public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Partitioning schemes", "MINIX");

        public override PartitionTest[] Tests => new[]
        {
            new PartitionTest
            {
                TestFile = "minix_3.1.2a.aif",
                Partitions = new[]
                {
                    new Partition
                    {
                        Length   = 524159,
                        Name     = "MINIX",
                        Offset   = 2064896,
                        Sequence = 0,
                        Size     = 268369408,
                        Start    = 4033,
                        Type     = "MINIX"
                    },
                    new Partition
                    {
                        Length   = 528192,
                        Name     = "MINIX",
                        Offset   = 270434304,
                        Sequence = 1,
                        Size     = 270434304,
                        Start    = 528192,
                        Type     = "MINIX"
                    },
                    new Partition
                    {
                        Length   = 528192,
                        Name     = "MINIX",
                        Offset   = 540868608,
                        Sequence = 2,
                        Size     = 270434304,
                        Start    = 1056384,
                        Type     = "MINIX"
                    },
                    new Partition
                    {
                        Length   = 512064,
                        Name     = "MINIX",
                        Offset   = 811302912,
                        Sequence = 3,
                        Size     = 262176768,
                        Start    = 1584576,
                        Type     = "MINIX"
                    }
                }
            }
        };
    }
}