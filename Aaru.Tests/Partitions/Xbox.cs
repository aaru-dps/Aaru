// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Xbox.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef unit testing.
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
// Copyright © 2011-2019 Natalia Portillo
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
    public class Xbox
    {
        readonly string[] _testfiles =
        {
            "microsoft256mb.aif"
        };

        readonly Partition[][] _wanted =
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

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Partitioning schemes", "Xbox", _testfiles[i]);
                IFilter filter   = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                Assert.AreEqual(_wanted[i].Length, partitions.Count, _testfiles[i]);

                for(int j = 0; j < partitions.Count; j++)
                {
                    // Too chatty
                    Assert.AreEqual(_wanted[i][j].Description, partitions[j].Description, _testfiles[i]);
                    Assert.AreEqual(_wanted[i][j].Length * 512, partitions[j].Size, _testfiles[i]);
                    Assert.AreEqual(_wanted[i][j].Start  * 512, partitions[j].Offset, _testfiles[i]);
                    Assert.AreEqual(_wanted[i][j].Length, partitions[j].Length, _testfiles[i]);
                    Assert.AreEqual(_wanted[i][j].Sequence, partitions[j].Sequence, _testfiles[i]);
                    Assert.AreEqual(_wanted[i][j].Start, partitions[j].Start, _testfiles[i]);
                }
            }
        }
    }
}