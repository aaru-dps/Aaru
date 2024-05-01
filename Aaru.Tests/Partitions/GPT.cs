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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using NUnit.Framework;

namespace Aaru.Tests.Partitions;

[TestFixture]
public class Gpt : PartitionSchemeTest
{
    public override string DataFolder =>
        Path.Combine(Consts.TestFilesRoot, "Partitioning schemes", "GUID Partition Table");

    public override PartitionTest[] Tests => new[]
    {
        new PartitionTest
        {
            TestFile = "linux.aif",
            Partitions = new[]
            {
                new Partition
                {
                    Length   = 20480,
                    Name     = "EFI System",
                    Offset   = 1048576,
                    Sequence = 0,
                    Size     = 10485760,
                    Start    = 2048,
                    Type     = "EFI System"
                },
                new Partition
                {
                    Length   = 30720,
                    Name     = "Microsoft basic data",
                    Offset   = 11534336,
                    Sequence = 1,
                    Size     = 15728640,
                    Start    = 22528,
                    Type     = "Microsoft Basic data"
                },
                new Partition
                {
                    Length   = 40960,
                    Name     = "Apple label",
                    Offset   = 27262976,
                    Sequence = 2,
                    Size     = 20971520,
                    Start    = 53248,
                    Type     = "Apple Label"
                },
                new Partition
                {
                    Length   = 51200,
                    Name     = "Solaris /usr & Mac ZFS",
                    Offset   = 48234496,
                    Sequence = 3,
                    Size     = 26214400,
                    Start    = 94208,
                    Type     = "Solaris /usr or Apple ZFS"
                },
                new Partition
                {
                    Length   = 61440,
                    Name     = "FreeBSD ZFS",
                    Offset   = 74448896,
                    Sequence = 4,
                    Size     = 31457280,
                    Start    = 145408,
                    Type     = "FreeBSD ZFS"
                },
                new Partition
                {
                    Length   = 55263,
                    Name     = "HP-UX data",
                    Offset   = 105906176,
                    Sequence = 5,
                    Size     = 28294656,
                    Start    = 206848,
                    Type     = "HP-UX Data"
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
                    Length   = 83968,
                    Name     = "",
                    Offset   = 1048576,
                    Sequence = 0,
                    Size     = 42991616,
                    Start    = 2048,
                    Type     = "Apple HFS"
                },
                new Partition
                {
                    Length   = 102400,
                    Name     = "",
                    Offset   = 44040192,
                    Sequence = 1,
                    Size     = 52428800,
                    Start    = 86016,
                    Type     = "Linux filesystem"
                },
                new Partition
                {
                    Length   = 71680,
                    Name     = "",
                    Offset   = 96468992,
                    Sequence = 2,
                    Size     = 36700160,
                    Start    = 188416,
                    Type     = "Microsoft Basic data"
                }
            }
        }
    };
}