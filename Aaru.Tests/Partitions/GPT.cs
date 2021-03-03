// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : GPT.cs
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
    public class Gpt : PartitionSchemeTest
    {
        public override string DataFolder =>
            Path.Combine(Consts.TEST_FILES_ROOT, "Partitioning schemes", "GUID Partition Table");

        public override string[] TestFiles => new[]
        {
            "linux.aif", "parted.aif"
        };

        public override Partition[][] Wanted => new[]
        {
            // Linux
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 10485760,
                    Name        = "EFI System",
                    Type        = "EFI System",
                    Offset      = 1048576,
                    Length      = 20480,
                    Sequence    = 0,
                    Start       = 2048
                },
                new Partition
                {
                    Description = null,
                    Size        = 15728640,
                    Name        = "Microsoft basic data",
                    Type        = "Microsoft Basic data",
                    Offset      = 11534336,
                    Length      = 30720,
                    Sequence    = 1,
                    Start       = 22528
                },
                new Partition
                {
                    Description = null,
                    Size        = 20971520,
                    Name        = "Apple label",
                    Type        = "Apple Label",
                    Offset      = 27262976,
                    Length      = 40960,
                    Sequence    = 2,
                    Start       = 53248
                },
                new Partition
                {
                    Description = null,
                    Size        = 26214400,
                    Name        = "Solaris /usr & Mac ZFS",
                    Type        = "Solaris /usr or Apple ZFS",
                    Offset      = 48234496,
                    Length      = 51200,
                    Sequence    = 3,
                    Start       = 94208
                },
                new Partition
                {
                    Description = null,
                    Size        = 31457280,
                    Name        = "FreeBSD ZFS",
                    Type        = "FreeBSD ZFS",
                    Offset      = 74448896,
                    Length      = 61440,
                    Sequence    = 4,
                    Start       = 145408
                },
                new Partition
                {
                    Description = null,
                    Size        = 28294656,
                    Name        = "HP-UX data",
                    Type        = "HP-UX Data",
                    Offset      = 105906176,
                    Length      = 55263,
                    Sequence    = 5,
                    Start       = 206848
                }
            },

            // Parted
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 42991616,
                    Name        = "",
                    Type        = "Apple HFS",
                    Offset      = 1048576,
                    Length      = 83968,
                    Sequence    = 0,
                    Start       = 2048
                },
                new Partition
                {
                    Description = null,
                    Size        = 52428800,
                    Name        = "",
                    Type        = "Linux filesystem",
                    Offset      = 44040192,
                    Length      = 102400,
                    Sequence    = 1,
                    Start       = 86016
                },
                new Partition
                {
                    Description = null,
                    Size        = 36700160,
                    Name        = "",
                    Type        = "Microsoft Basic data",
                    Offset      = 96468992,
                    Length      = 71680,
                    Sequence    = 2,
                    Start       = 188416
                }
            }
        };
    }
}