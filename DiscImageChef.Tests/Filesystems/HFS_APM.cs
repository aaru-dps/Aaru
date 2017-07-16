// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HFS_APM.cs
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
    public class HFS_APM
    {
        readonly string[] testfiles = {
            "amigaos_3.9.vdi.lz","darwin_1.3.1.vdi.lz","darwin_1.4.1.vdi.lz","darwin_6.0.2.vdi.lz",
            "darwin_8.0.1.vdi.lz","macos_1.1.vdi.lz","macos_2.0.vdi.lz","macos_6.0.7.vdi.lz",
            "macos_7.5.3.vdi.lz","macos_7.5.vdi.lz","macos_7.6.vdi.lz","macos_8.0.vdi.lz",
            "macos_8.1.vdi.lz","macos_9.0.4.vdi.lz","macos_9.1.vdi.lz","macos_9.2.1.vdi.lz",
            "macos_9.2.2.vdi.lz","macosx_10.2.vdi.lz","macosx_10.3.vdi.lz","macosx_10.4.vdi.lz",
            "rhapsody_dr1.vdi.lz","d2_driver.vdi.lz","hdt_1.8.vdi.lz","macos_4.2.vdi.lz",
            "macos_4.3.vdi.lz","macos_6.0.2.vdi.lz","macos_6.0.3.vdi.lz","macos_6.0.4.vdi.lz",
            "macos_6.0.5.vdi.lz","macos_6.0.8.vdi.lz","macos_6.0.vdi.lz","macos_7.0.vdi.lz",
            "macos_7.1.1.vdi.lz","parted.vdi.lz","silverlining_2.2.1.vdi.lz","speedtools_3.6.vdi.lz",
            "vcpformatter_2.1.1.vdi.lz",
        };

        readonly ulong[] sectors = {
            1024128,409600,409600,409600,
            409600,41820,41820,81648,
            1024000,1024000,1024000,1024000,
            1024000,1024000,1024000,1024000,
            1024000,1024000,1024000,1024000,
            409600,51200,51200,41820,
            41820,54840,54840,54840,
            54840,54840,41820,54840,
            54840,262144,51200,51200,
            54840,
        };

        readonly uint[] sectorsize = {
            512,512,512,512,
            512,512,512,512,
            512,512,512,512,
            512,512,512,512,
            512,512,512,512,
            512,512,512,512,
            512,512,512,512,
            512,512,512,512,
            512,512,512,512,
            512,
        };

        readonly long[] clusters = {
            64003,51189,51189,58502,
            58502,41788,38950,39991,
            63954,63990,63954,63954,
            63954,63922,63922,63922,
            63922,63884,63883,63883,
            58506,50926,50094,38950,
            38950,38950,38950,7673,
            38950,38950,38950,38950,
            38950,46071,50382,49135,
            54643,
        };

        readonly int[] clustersize = {
            8192,4096,4096,3584,
            3584,512,512,1024,
            8192,8192,8192,8192,
            8192,8192,8192,8192,
            8192,8192,8192,8192,
            3584,512,512,512,
            512,512,512,512,
            512,512,512,512,
            512,1024,512,512,
            512,
        };

        readonly string[] volumename = {
            "Volume label","Volume label","Volume label","Volume label",
            "Volume label","Volume label","Volume label","Volume label",
            "Volume label","Volume label","Volume label","Volume label",
            "Volume label","Volume label","Volume label","Volume label",
            "Volume label","Volume label","Volume label","Volume label",
            "Volume label","Volume label","Volume label","Volume label",
            "Volume label","Volume label","Volume label","Test disk",
            "Volume label","Volume label","Volume label","Volume label",
            "Volume label","Untitled","Untitled  #1","24 MB Disk",
            "Volume label",
        };

        readonly string[] volumeserial = {
            null,null,null,null,
            "AAFE1382AF5AA898",null,null,null,
            null,null,null,null,
            null,null,null,null,
            null,"5A7C38B0CAF279C4","FB49083EBD150509","632C0B1DB46FD188",
            null,null,null,null,
            null,null,null,null,
            null,null,null,null,
            null,null,null,null,
            null,
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "hfs_apm", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new VDI();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                PartPlugin parts = new DiscImageChef.PartPlugins.AppleMap();
                Assert.AreEqual(true, parts.GetInformation(image, out List<Partition> partitions), testfiles[i]);
                Filesystem fs = new DiscImageChef.Filesystems.AppleHFS();
                int part = -1;
                for(int j = 0; j < partitions.Count; j++)
                {
                    if(partitions[j].PartitionType == "Apple_HFS")
                    {
                        part = j;
                        break;
                    }
                }
                Assert.AreNotEqual(-1, part, "Partition not found");
                Assert.AreEqual(true, fs.Identify(image, partitions[part].PartitionStartSector, partitions[part].PartitionStartSector + partitions[part].PartitionSectors - 1), testfiles[i]);
                fs.GetInformation(image, partitions[part].PartitionStartSector, partitions[part].PartitionStartSector + partitions[part].PartitionSectors - 1, out string information);
                Assert.AreEqual(clusters[i], fs.XmlFSType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFSType.ClusterSize, testfiles[i]);
                Assert.AreEqual("HFS", fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSerial, testfiles[i]);
            }
        }
    }
}
