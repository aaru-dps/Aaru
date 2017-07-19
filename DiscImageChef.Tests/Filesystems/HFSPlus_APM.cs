// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HFSPlus_APM.cs
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

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class HFSPlus_APM
    {
        // Missing Darwin 1.4.1
        readonly string[] testfiles = {
            "macosx_10.11.vdi.lz", "macosx_10.11_journal.vdi.lz","darwin_1.3.1.vdi.lz","darwin_1.3.1_wrapped.vdi.lz",
            "darwin_1.4.1_wrapped.vdi.lz","darwin_6.0.2.vdi.lz","darwin_6.0.2_wrapped.vdi.lz",
            "darwin_8.0.1_journal.vdi.lz","darwin_8.0.1.vdi.lz","darwin_8.0.1_wrapped.vdi.lz","macos_8.1.vdi.lz",
            "macos_9.0.4.vdi.lz","macos_9.1.vdi.lz","macos_9.2.1.vdi.lz","macos_9.2.2.vdi.lz",
            "macosx_10.2.vdi.lz","macosx_10.3_journal.vdi.lz","macosx_10.3.vdi.lz","macosx_10.4_journal.vdi.lz",
            "macosx_10.4.vdi.lz",
        };

        readonly ulong[] sectors = {
            409600, 614400, 819200, 614400,
            614400, 819200, 614400,
            1228800, 819200, 614400, 4194304,
            4194304, 4194304,4194304,4194304,
            4194304, 2097152, 4194304, 2097152,
            4194304
        };

        readonly uint[] sectorsize = {
            512, 512, 512, 512,
            512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
            512,
        };

        readonly long[] clusters = {
            51190, 76790, 102392, 76774,
            76774, 102392, 76774,
            153592, 102392, 76774, 524152,
            524088, 524088, 524088, 524088,
            524008, 261884, 491240, 261884,
            491240,
        };

        readonly int[] clustersize = {
            4096, 4096, 4096, 4096,
            4096, 4096, 4096,
            4096, 4096, 4096, 4096,
            4096, 4096, 4096, 4096,
            4096, 4096, 4096, 4096,
            4096,
        };

        readonly string[] volumename = {
            null, null, null, null,
            null, null, null,
            null, null, null, null,
            null, null, null, null,
            null, null, null, null,
            null,
        };

        readonly string[] volumeserial = {
            "FA94762D086A18A9","33D4A309C8E7BD10",null,null,
            null,null,null,
            "4D5140EB8F14A385","0D592249833E2DC4","AA616146576BD9BC",null,
            null,null,null,null,
            "EFA132FFFAC1ADA6","009D570FFCF8F20B","17F6F33AB313EE32","AD5690C093F66FCF",
            "A7D63854DF76DDE6",
        };

        readonly string[] oemid = {
            "10.0","HFSJ","10.0","10.0",
            "10.0","10.0","10.0",
            "10.0","10.0","10.0","8.10",
            "8.10","8.10","8.10","8.10",
            "10.0","HFSJ","10.0","HFSJ",
            "10.0",
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "hfsplus_apm", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new VDI();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                PartPlugin parts = new DiscImageChef.PartPlugins.AppleMap();
                Assert.AreEqual(true, parts.GetInformation(image, out List<Partition> partitions), testfiles[i]);
                Filesystem fs = new DiscImageChef.Filesystems.AppleHFSPlus();
                int part = -1;
                for(int j = 0; j < partitions.Count; j++)
                {
                    if(partitions[j].Type == "Apple_HFS")
                    {
                        part = j;
                        break;
                    }
                }
                Assert.AreNotEqual(-1, part, "Partition not found");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out string information);
                Assert.AreEqual(clusters[i], fs.XmlFSType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFSType.ClusterSize, testfiles[i]);
                Assert.AreEqual("HFS+", fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSerial, testfiles[i]);
                Assert.AreEqual(oemid[i], fs.XmlFSType.SystemIdentifier, testfiles[i]);
            }
        }
    }
}
