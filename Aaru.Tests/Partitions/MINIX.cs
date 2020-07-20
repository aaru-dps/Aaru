// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MINIX.cs
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
    public class Minix
    {
        readonly string[] testfiles =
        {
            "minix_3.1.2a.aif"
        };

        readonly Partition[][] wanted =
        {
            // Parted
            new[]
            {
                new Partition
                {
                    Description = null,
                    Size        = 268369408,
                    Name        = "MINIX",
                    Type        = "MINIX",
                    Offset      = 2064896,
                    Length      = 524159,
                    Sequence    = 0,
                    Start       = 4033
                },
                new Partition
                {
                    Description = null,
                    Size        = 270434304,
                    Name        = "MINIX",
                    Type        = "MINIX",
                    Offset      = 270434304,
                    Length      = 528192,
                    Sequence    = 1,
                    Start       = 528192
                },
                new Partition
                {
                    Description = null,
                    Size        = 270434304,
                    Name        = "MINIX",
                    Type        = "MINIX",
                    Offset      = 540868608,
                    Length      = 528192,
                    Sequence    = 2,
                    Start       = 1056384
                },
                new Partition
                {
                    Description = null,
                    Size        = 262176768,
                    Name        = "MINIX",
                    Type        = "MINIX",
                    Offset      = 811302912,
                    Length      = 512064,
                    Sequence    = 3,
                    Start       = 1584576
                }
            }
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "Partitioning schemes", "MINIX", testfiles[i]);
                IFilter filter   = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                Assert.AreEqual(wanted[i].Length, partitions.Count, testfiles[i]);

                for(int j = 0; j < partitions.Count; j++)
                {
                    // Too chatty
                    //Assert.AreEqual(wanted[i][j].PartitionDescription, partitions[j].PartitionDescription, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Size, partitions[j].Size, testfiles[i]);

                    //                    Assert.AreEqual(wanted[i][j].Name, partitions[j].Name, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Type, partitions[j].Type, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Offset, partitions[j].Offset, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Length, partitions[j].Length, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Sequence, partitions[j].Sequence, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].Start, partitions[j].Start, testfiles[i]);
                }
            }
        }
    }
}