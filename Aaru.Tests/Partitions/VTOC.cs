// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : VTOC.cs
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
    public class Vtoc : PartitionSchemeTest
    {
        public override string DataFolder => Path.Combine(Consts.TestFilesRoot, "Partitioning schemes", "UNIX VTOC");

        public override PartitionTest[] Tests => new[]
        {
            new PartitionTest
            {
                TestFile = "att_unix_vtoc.aif",
                Partitions = new[]
                {
                    new Partition
                    {
                        Length   = 34,
                        Offset   = 0,
                        Sequence = 0,
                        Size     = 0,
                        Start    = 1,
                        Type     = "UNIX: Boot"
                    },
                    new Partition
                    {
                        Length   = 1023119,
                        Offset   = 0,
                        Sequence = 1,
                        Size     = 0,
                        Start    = 1,
                        Type     = "UNIX: Whole disk"
                    },
                    new Partition
                    {
                        Length   = 253,
                        Offset   = 0,
                        Sequence = 2,
                        Size     = 0,
                        Start    = 63,
                        Type     = "UNIX: Stand"
                    },
                    new Partition
                    {
                        Length   = 78624,
                        Offset   = 0,
                        Sequence = 3,
                        Size     = 0,
                        Start    = 378,
                        Type     = "UNIX: /usr"
                    },
                    new Partition
                    {
                        Length   = 78624,
                        Offset   = 0,
                        Sequence = 4,
                        Size     = 0,
                        Start    = 79002,
                        Type     = "UNIX: /usr"
                    },
                    new Partition
                    {
                        Length   = 78624,
                        Offset   = 0,
                        Sequence = 5,
                        Size     = 0,
                        Start    = 157626,
                        Type     = "UNIX: /usr"
                    },
                    new Partition
                    {
                        Length   = 78624,
                        Offset   = 0,
                        Sequence = 6,
                        Size     = 0,
                        Start    = 236250,
                        Type     = "UNIX: /usr"
                    },
                    new Partition
                    {
                        Length   = 78624,
                        Offset   = 0,
                        Sequence = 7,
                        Size     = 0,
                        Start    = 314874,
                        Type     = "UNIX: /usr"
                    },
                    new Partition
                    {
                        Length   = 78624,
                        Offset   = 0,
                        Sequence = 8,
                        Size     = 0,
                        Start    = 393498,
                        Type     = "UNIX: /usr"
                    },
                    new Partition
                    {
                        Length   = 78624,
                        Offset   = 0,
                        Sequence = 9,
                        Size     = 0,
                        Start    = 472122,
                        Type     = "UNIX: /usr"
                    },
                    new Partition
                    {
                        Length   = 78624,
                        Offset   = 0,
                        Sequence = 10,
                        Size     = 0,
                        Start    = 550746,
                        Type     = "UNIX: /usr"
                    },
                    new Partition
                    {
                        Length   = 78624,
                        Offset   = 0,
                        Sequence = 11,
                        Size     = 0,
                        Start    = 629370,
                        Type     = "UNIX: /usr"
                    },
                    new Partition
                    {
                        Length   = 76608,
                        Offset   = 0,
                        Sequence = 12,
                        Size     = 0,
                        Start    = 707994,
                        Type     = "UNIX: /usr"
                    },
                    new Partition
                    {
                        Length   = 77616,
                        Offset   = 0,
                        Sequence = 13,
                        Size     = 0,
                        Start    = 784602,
                        Type     = "UNIX: /usr"
                    },
                    new Partition
                    {
                        Length   = 75600,
                        Offset   = 0,
                        Sequence = 14,
                        Size     = 0,
                        Start    = 862218,
                        Type     = "UNIX: /usr"
                    },
                    new Partition
                    {
                        Length   = 84672,
                        Offset   = 0,
                        Sequence = 15,
                        Size     = 0,
                        Start    = 937818,
                        Type     = "UNIX: /usr"
                    }
                }
            }
        };
    }
}