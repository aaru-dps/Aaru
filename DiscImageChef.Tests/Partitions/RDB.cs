// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : RDB.cs
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
    public class RDB
    {
        readonly string[] testfiles = {
            "amigaos_3.9.vdi.lz","amigaos_4.0.vdi.lz","parted.vdi.lz",
        };

        readonly Partition[][] wanted = {
            // AmigaOS 3.9
            new []{ 
                new Partition{ PartitionDescription = null, PartitionLength = 87392256, PartitionName = "UDH0", PartitionType = "\"DOS\\0\"", PartitionStart = 2080768, PartitionSectors = 170688,
                    PartitionSequence = 0, PartitionStartSector = 4064 },
                new Partition{ PartitionDescription = null, PartitionLength = 87392256, PartitionName = "UDH1", PartitionType = "\"DOS\\2\"", PartitionStart = 89473024, PartitionSectors = 170688,
                    PartitionSequence = 1, PartitionStartSector = 174752 },
                new Partition{ PartitionDescription = null, PartitionLength = 87392256, PartitionName = "UDH2", PartitionType = "\"DOS\\1\"", PartitionStart = 176865280, PartitionSectors = 170688,
                    PartitionSequence = 2, PartitionStartSector = 345440 },
                new Partition{ PartitionDescription = null, PartitionLength = 87392256, PartitionName = "UDH3", PartitionType = "\"DOS\\3\"", PartitionStart = 264257536, PartitionSectors = 170688,
                    PartitionSequence = 3, PartitionStartSector = 516128 },
                new Partition{ PartitionDescription = null, PartitionLength = 87392256, PartitionName = "UDH4", PartitionType = "\"RES\\86\"", PartitionStart = 351649792, PartitionSectors = 170688,
                    PartitionSequence = 4, PartitionStartSector = 686816 },
                new Partition{ PartitionDescription = null, PartitionLength = 85311488, PartitionName = "UDH5", PartitionType = "\"RES\\86\"", PartitionStart = 439042048, PartitionSectors = 166624,
                    PartitionSequence = 5, PartitionStartSector = 857504 },
            },
            // AmigaOS 4.0
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 91455488, PartitionName = "DH1", PartitionType = "\"DOS\\1\"", PartitionStart = 1048576, PartitionSectors = 178624,
                    PartitionSequence = 0, PartitionStartSector = 2048 },
                new Partition{ PartitionDescription = null, PartitionLength = 76546048, PartitionName = "DH2", PartitionType = "\"DOS\\3\"", PartitionStart = 92504064, PartitionSectors = 149504,
                    PartitionSequence = 1, PartitionStartSector = 180672 },
                new Partition{ PartitionDescription = null, PartitionLength = 78741504, PartitionName = "DH3", PartitionType = "\"DOS\\3\"", PartitionStart = 169050112, PartitionSectors = 153792,
                    PartitionSequence = 2, PartitionStartSector = 330176 },
                new Partition{ PartitionDescription = null, PartitionLength = 78020608, PartitionName = "DH4", PartitionType = "\"DOS\\7\"", PartitionStart = 247791616, PartitionSectors = 152384,
                    PartitionSequence = 3, PartitionStartSector = 483968 },
                new Partition{ PartitionDescription = null, PartitionLength = 85000192, PartitionName = "DH5", PartitionType = "\"SFS\\0\"", PartitionStart = 325812224, PartitionSectors = 166016,
                    PartitionSequence = 4, PartitionStartSector = 636352 },
                new Partition{ PartitionDescription = null, PartitionLength = 113541120, PartitionName = "DH6", PartitionType = "\"SFS\\2\"", PartitionStart = 410812416, PartitionSectors = 221760,
                    PartitionSequence = 5, PartitionStartSector = 802368 },
            },
            // Parted
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 8225280, PartitionName = "primary", PartitionType = "\"\0\0\0\\0\"", PartitionStart = 8225280, PartitionSectors = 16065,
                    PartitionSequence = 0, PartitionStartSector = 16065 },
                new Partition{ PartitionDescription = null, PartitionLength = 24675840, PartitionName = "name", PartitionType = "\"FAT\\1\"", PartitionStart = 16450560, PartitionSectors = 48195,
                    PartitionSequence = 1, PartitionStartSector = 32130 },
                new Partition{ PartitionDescription = null, PartitionLength = 90478080, PartitionName = "partition", PartitionType = "\"\0\0\0\\0\"", PartitionStart = 41126400, PartitionSectors = 176715,
                    PartitionSequence = 2, PartitionStartSector = 80325 },
            },
        };  

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "partitions", "rdb", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new VDI();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                PartPlugin parts = new DiscImageChef.PartPlugins.AmigaRigidDiskBlock();
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
