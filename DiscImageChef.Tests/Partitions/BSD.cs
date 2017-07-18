// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Atari.cs
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
    public class Atari
    {
        readonly string[] testfiles = {
            /*"linux_ahdi.vdi.lz",*/"linux_icd.vdi.lz","tos_1.04.vdi.lz",
        };

        readonly Partition[][] wanted = {/*
            // Linux (AHDI)
            new []{ 
                new Partition{ PartitionDescription = null, PartitionLength = 31457280, PartitionName = null, PartitionType = "GEM", PartitionStart = 512, PartitionSectors = 61440,
                    PartitionSequence = 0, PartitionStartSector = 1 },
                new Partition{ PartitionDescription = null, PartitionLength = 41943040, PartitionName = null, PartitionType = "BGM", PartitionStart = 31457792, PartitionSectors = 81920,
                    PartitionSequence = 1, PartitionStartSector = 61441 },
                new Partition{ PartitionDescription = null, PartitionLength = 56402432, PartitionName = null, PartitionType = "LNX", PartitionStart = 73400832, PartitionSectors = 110161,
                    PartitionSequence = 2, PartitionStartSector = 143361 },
                new Partition{ PartitionDescription = null, PartitionLength = 43212800, PartitionName = null, PartitionType = "MAC", PartitionStart = 129803264, PartitionSectors = 84400,
                    PartitionSequence = 3, PartitionStartSector = 253522 },
                new Partition{ PartitionDescription = null, PartitionLength = 57671680, PartitionName = null, PartitionType = "MIX", PartitionStart = 173016064, PartitionSectors = 112640,
                    PartitionSequence = 4, PartitionStartSector = 337922 },
                new Partition{ PartitionDescription = null, PartitionLength = 62914560, PartitionName = null, PartitionType = "MNX", PartitionStart = 230687744, PartitionSectors = 122880,
                    PartitionSequence = 5, PartitionStartSector = 450562 },
                new Partition{ PartitionDescription = null, PartitionLength = 73400320, PartitionName = null, PartitionType = "RAW", PartitionStart = 293602304, PartitionSectors = 143360,
                    PartitionSequence = 6, PartitionStartSector = 573442 },
                new Partition{ PartitionDescription = null, PartitionLength = 78643200, PartitionName = null, PartitionType = "SWP", PartitionStart = 367002624, PartitionSectors = 153600,
                    PartitionSequence = 7, PartitionStartSector = 716802 },
                new Partition{ PartitionDescription = null, PartitionLength = 1048576, PartitionName = null, PartitionType = "UNX", PartitionStart = 445645824, PartitionSectors = 2048,
                    PartitionSequence = 8, PartitionStartSector = 870402 },
                new Partition{ PartitionDescription = null, PartitionLength = 77593600, PartitionName = null, PartitionType = "LNX", PartitionStart = 446694400, PartitionSectors = 151550,
                    PartitionSequence = 9, PartitionStartSector = 872450 },
            },*/
            // Linux (ICD)
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 15728640, PartitionName = null, PartitionType = "GEM", PartitionStart = 512, PartitionSectors = 30720,
                    PartitionSequence = 0, PartitionStartSector = 1 },
                new Partition{ PartitionDescription = null, PartitionLength = 20971520, PartitionName = null, PartitionType = "UNX", PartitionStart = 15729152, PartitionSectors = 40960,
                    PartitionSequence = 1, PartitionStartSector = 30721 },
                new Partition{ PartitionDescription = null, PartitionLength = 31457280, PartitionName = null, PartitionType = "LNX", PartitionStart = 36700672, PartitionSectors = 61440,
                    PartitionSequence = 2, PartitionStartSector = 71681 },
                new Partition{ PartitionDescription = null, PartitionLength = 41943040, PartitionName = null, PartitionType = "BGM", PartitionStart = 68157952, PartitionSectors = 81920,
                    PartitionSequence = 3, PartitionStartSector = 133121 },
                new Partition{ PartitionDescription = null, PartitionLength = 52428800, PartitionName = null, PartitionType = "MAC", PartitionStart = 110100992, PartitionSectors = 102400,
                    PartitionSequence = 4, PartitionStartSector = 215041 },
                new Partition{ PartitionDescription = null, PartitionLength = 62914560, PartitionName = null, PartitionType = "MIX", PartitionStart = 162529792, PartitionSectors = 122880,
                    PartitionSequence = 5, PartitionStartSector = 317441 },
                new Partition{ PartitionDescription = null, PartitionLength = 83886080, PartitionName = null, PartitionType = "SWP", PartitionStart = 225444352, PartitionSectors = 163840,
                    PartitionSequence = 6, PartitionStartSector = 440321 },
                new Partition{ PartitionDescription = null, PartitionLength = 103809024, PartitionName = null, PartitionType = "MNX", PartitionStart = 309330432, PartitionSectors = 202752,
                    PartitionSequence = 7, PartitionStartSector = 604161 },
                new Partition{ PartitionDescription = null, PartitionLength = 104857600, PartitionName = null, PartitionType = "LNX", PartitionStart = 413139456, PartitionSectors = 204800,
                    PartitionSequence = 8, PartitionStartSector = 806913 },
            },
            // TOS 1.04
            new []{
                new Partition{ PartitionDescription = null, PartitionLength = 7340032, PartitionName = null, PartitionType = "GEM", PartitionStart = 1024, PartitionSectors = 14336,
                    PartitionSequence = 0, PartitionStartSector = 2 },
                new Partition{ PartitionDescription = null, PartitionLength = 7340032, PartitionName = null, PartitionType = "GEM", PartitionStart = 7341056, PartitionSectors = 14336,
                    PartitionSequence = 1, PartitionStartSector = 14338 },
                new Partition{ PartitionDescription = null, PartitionLength = 7340032, PartitionName = null, PartitionType = "GEM", PartitionStart = 14681088, PartitionSectors = 14336,
                    PartitionSequence = 2, PartitionStartSector = 28674 },
                new Partition{ PartitionDescription = null, PartitionLength = 7339008, PartitionName = null, PartitionType = "GEM", PartitionStart = 22021120, PartitionSectors = 14334,
                    PartitionSequence = 3, PartitionStartSector = 43010 },
            },
        };  

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "partitions", "apm", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new VDI();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                PartPlugin parts = new DiscImageChef.PartPlugins.AtariPartitions();
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
