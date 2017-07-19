// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : BSD.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System.Collections.Generic;
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;
using DiscImageChef.PartPlugins;
using NUnit.Framework;

namespace DiscImageChef.Tests.Partitions
{
    [TestFixture]
    public class GPT
    {
        readonly string[] testfiles = {
            "linux.vdi.lz","parted.vdi.lz",
        };

        readonly Partition[][] wanted = {
            // Linux
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 10485760, PartitionName = "EFI System", PartitionType = "EFI System", PartitionStart = 1048576, PartitionSectors = 20480,
                    PartitionSequence = 0, PartitionStartSector = 2048 },
                new Partition{ PartitionDescription = null, PartitionLength = 15728640, PartitionName = "Microsoft basic data", PartitionType = "Microsoft Basic data", PartitionStart = 11534336, PartitionSectors = 30720,
                    PartitionSequence = 1, PartitionStartSector = 22528 },
                new Partition{ PartitionDescription = null, PartitionLength = 20971520, PartitionName = "Apple label", PartitionType = "Apple Label", PartitionStart = 27262976, PartitionSectors = 40960,
                    PartitionSequence = 2, PartitionStartSector = 53248 },
                new Partition{ PartitionDescription = null, PartitionLength = 26214400, PartitionName = "Solaris /usr & Mac ZFS", PartitionType = "Solaris /usr or Apple ZFS", PartitionStart = 48234496, PartitionSectors = 51200,
                    PartitionSequence = 3, PartitionStartSector = 94208 },
                new Partition{ PartitionDescription = null, PartitionLength = 31457280, PartitionName = "FreeBSD ZFS", PartitionType = "FreeBSD ZFS", PartitionStart = 74448896, PartitionSectors = 61440,
                    PartitionSequence = 4, PartitionStartSector = 145408 },
                new Partition{ PartitionDescription = null, PartitionLength = 28294656, PartitionName = "HP-UX data", PartitionType = "HP-UX Data", PartitionStart = 105906176, PartitionSectors = 55263,
                    PartitionSequence = 5, PartitionStartSector = 206848 },
            },
            // Parted
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 42991616, PartitionName = "", PartitionType = "Apple HFS", PartitionStart = 1048576, PartitionSectors = 83968,
                    PartitionSequence = 0, PartitionStartSector = 2048 },
                new Partition{ PartitionDescription = null, PartitionLength = 52428800, PartitionName = "", PartitionType = "Linux filesystem", PartitionStart = 44040192, PartitionSectors = 102400,
                    PartitionSequence = 1, PartitionStartSector = 86016 },
                new Partition{ PartitionDescription = null, PartitionLength = 36700160, PartitionName = "", PartitionType = "Microsoft Basic data", PartitionStart = 96468992, PartitionSectors = 71680,
                    PartitionSequence = 2, PartitionStartSector = 188416 },
            },
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "partitions", "gpt", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new VDI();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                PartPlugin parts = new DiscImageChef.PartPlugins.GuidPartitionTable();
                Assert.AreEqual(true, parts.GetInformation(image, out List<Partition> partitions), testfiles[i]);
                Assert.AreEqual(wanted[i].Length, partitions.Count, testfiles[i]);
                for(int j = 0; j < partitions.Count; j++)
                {
                    // Too chatty
                    //Assert.AreEqual(wanted[i][j].PartitionDescription, partitions[j].PartitionDescription, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionLength, partitions[j].PartitionLength, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionName, partitions[j].PartitionName, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionType, partitions[j].PartitionType, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionStart, partitions[j].PartitionStart, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionSectors, partitions[j].PartitionSectors, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionSequence, partitions[j].PartitionSequence, testfiles[i]);
                    Assert.AreEqual(wanted[i][j].PartitionStartSector, partitions[j].PartitionStartSector, testfiles[i]);
                }
            }
        }
    }
}
