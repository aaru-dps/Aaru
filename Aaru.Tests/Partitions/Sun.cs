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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.CommonTypes;
using NUnit.Framework;

namespace Aaru.Tests.Partitions
{
    // TODO: Get SunOS and VTOC16 disk labels
    [TestFixture]
    public class Sun : PartitionSchemeTest
    {
        public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Partitioning schemes", "Sun");

        public override string[] TestFiles => new[]
        {
            "linux.aif", "parted.aif"
        };

        public override Partition[][] Wanted => new[]
        {
            // Linux's fdisk
            new[]
            {
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Linux",
                    Length      = 204800,
                    Sequence    = 0,
                    Start       = 0
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Sun boot",
                    Length      = 102400,
                    Sequence    = 1,
                    Start       = 208845
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Sun /",
                    Length      = 102400,
                    Sequence    = 2,
                    Start       = 321300
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Sun /home",
                    Length      = 102400,
                    Sequence    = 3,
                    Start       = 433755
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Sun swap",
                    Length      = 153600,
                    Sequence    = 4,
                    Start       = 546210
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Sun /usr",
                    Length      = 208845,
                    Sequence    = 5,
                    Start       = 706860
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Linux swap",
                    Length      = 96390,
                    Sequence    = 6,
                    Start       = 915705
                }
            },

            // GNU Parted
            new[]
            {
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Linux",
                    Length      = 49152,
                    Sequence    = 0,
                    Start       = 0
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Linux",
                    Length      = 80325,
                    Sequence    = 1,
                    Start       = 64260
                },
                new Partition
                {
                    Description = null,
                    Name        = null,
                    Type        = "Linux",
                    Length      = 96390,
                    Sequence    = 2,
                    Start       = 144585
                }
            }
        };
    }
}