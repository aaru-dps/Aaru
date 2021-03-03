// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Xbox.cs
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

using System.IO;
using Aaru.CommonTypes;
using NUnit.Framework;

namespace Aaru.Tests.Partitions
{
    [TestFixture]
    public class Xbox : PartitionSchemeTest
    {
        public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Partitioning schemes", "Xbox");

        public override string[] TestFiles => new[]
        {
            "microsoft256mb.aif"
        };

        public override Partition[][] Wanted => new[]
        {
            new[]
            {
                new Partition
                {
                    Description = "System cache",
                    Name        = null,
                    Type        = null,
                    Length      = 16376,
                    Sequence    = 0,
                    Start       = 0
                },
                new Partition
                {
                    Description = "Data volume",
                    Name        = null,
                    Type        = null,
                    Length      = 475144,
                    Sequence    = 1,
                    Start       = 16376
                }
            }
        };
    }
}