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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using Aaru.Filters;
using NUnit.Framework;

namespace Aaru.Tests.Partitions
{
    [TestFixture]
    public class Acorn
    {
        readonly string[] _testFiles =
        {
            "linux_ics.aif"
        };

        readonly Partition[][] _wanted =
        {
            // Linux (ICS)
            // TODO: Values are incorrect
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 31457280,
                    Name        = null,
                    Type        = "GEM",
                    Offset      = 512,
                    Length      = 61440,
                    Sequence    = 0,
                    Start       = 1
                },
                new Partition
                {
                    Description = null,
                    Size        = 41943040,
                    Name        = null,
                    Type        = "BGM",
                    Offset      = 31457792,
                    Length      = 81920,
                    Sequence    = 1,
                    Start       = 61441
                },
                new Partition
                {
                    Description = null,
                    Size        = 56402432,
                    Name        = null,
                    Type        = "LNX",
                    Offset      = 73400832,
                    Length      = 110161,
                    Sequence    = 2,
                    Start       = 143361
                },
                new Partition
                {
                    Description = null,
                    Size        = 43212800,
                    Name        = null,
                    Type        = "MAC",
                    Offset      = 129803264,
                    Length      = 84400,
                    Sequence    = 3,
                    Start       = 253522
                }
            }
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Partitioning schemes", "Acorn", _testFiles[i]);
                IFilter filter   = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                Assert.AreEqual(_wanted[i].Length, partitions.Count, _testFiles[i]);

                for(int j = 0; j < partitions.Count; j++)
                {
                    // Too chatty
                    //Assert.AreEqual(wanted[i][j].PartitionDescription, partitions[j].PartitionDescription, testfiles[i]);
                    Assert.AreEqual(_wanted[i][j].Size, partitions[j].Size, _testFiles[i]);
                    Assert.AreEqual(_wanted[i][j].Name, partitions[j].Name, _testFiles[i]);
                    Assert.AreEqual(_wanted[i][j].Type, partitions[j].Type, _testFiles[i]);
                    Assert.AreEqual(_wanted[i][j].Offset, partitions[j].Offset, _testFiles[i]);
                    Assert.AreEqual(_wanted[i][j].Length, partitions[j].Length, _testFiles[i]);
                    Assert.AreEqual(_wanted[i][j].Sequence, partitions[j].Sequence, _testFiles[i]);
                    Assert.AreEqual(_wanted[i][j].Start, partitions[j].Start, _testFiles[i]);
                }
            }
        }
    }
}