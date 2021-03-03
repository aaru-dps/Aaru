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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using NUnit.Framework;

namespace Aaru.Tests.Partitions
{
    [TestFixture]
    public class Vtoc : PartitionSchemeTest
    {
        public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Partitioning schemes", "UNIX VTOC");

        public override string[] TestFiles => new[]
        {
            "att_unix_vtoc.aif"
        };

        public override Partition[][] Wanted => new[]
        {
            // AT&T UNIX System V Release 4 Version 2.1 for 386
            new[]
            {
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "UNIX: Boot",
                    Length      = 34,
                    Sequence    = 0,
                    Start       = 1
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "UNIX: Whole disk",
                    Length      = 1023119,
                    Sequence    = 1,
                    Start       = 1
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "UNIX: Stand",
                    Length      = 253,
                    Sequence    = 2,
                    Start       = 63
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "UNIX: /usr",
                    Length      = 78624,
                    Sequence    = 3,
                    Start       = 378
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "UNIX: /usr",
                    Length      = 78624,
                    Sequence    = 4,
                    Start       = 79002
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "UNIX: /usr",
                    Length      = 78624,
                    Sequence    = 5,
                    Start       = 157626
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "UNIX: /usr",
                    Length      = 78624,
                    Sequence    = 6,
                    Start       = 236250
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "UNIX: /usr",
                    Length      = 78624,
                    Sequence    = 7,
                    Start       = 314874
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "UNIX: /usr",
                    Length      = 78624,
                    Sequence    = 8,
                    Start       = 393498
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "UNIX: /usr",
                    Length      = 78624,
                    Sequence    = 9,
                    Start       = 472122
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "UNIX: /usr",
                    Length      = 78624,
                    Sequence    = 10,
                    Start       = 550746
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "UNIX: /usr",
                    Length      = 78624,
                    Sequence    = 11,
                    Start       = 629370
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "UNIX: /usr",
                    Length      = 76608,
                    Sequence    = 12,
                    Start       = 707994
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "UNIX: /usr",
                    Length      = 77616,
                    Sequence    = 13,
                    Start       = 784602
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "UNIX: /usr",
                    Length      = 75600,
                    Sequence    = 14,
                    Start       = 862218
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "UNIX: /usr",
                    Length      = 84672,
                    Sequence    = 15,
                    Start       = 937818
                }
            }
        };
    }
}