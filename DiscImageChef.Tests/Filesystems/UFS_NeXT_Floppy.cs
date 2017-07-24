// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : UFS_NeXT.cs
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
    public class UFS_NeXT_Floppy
    {
        readonly string[] testfiles = {
            "nextstep_3.3_mf2dd.img.lz","nextstep_3.3_mf2hd.img.lz",
            "openstep_4.0_mf2dd.img.lz","openstep_4.0_mf2hd.img.lz",
            "openstep_4.2_mf2dd.img.lz","openstep_4.2_mf2hd.img.lz",
            "rhapsody_dr1_mf2dd.img.lz","rhapsody_dr1_mf2hd.img.lz",
            "rhapsody_dr2_mf2dd.img.lz","rhapsody_dr2_mf2hd.img.lz",
        };

        readonly ulong[] sectors = {
            1440, 2880,
            1440, 2880,
            1440, 2880,
            1440, 2880,
            1440, 2880,
        };

        readonly uint[] sectorsize = {
            512, 512,
            512, 512,
            512, 512,
            512, 512,
            512, 512,
        };

        readonly long[] clusters = {
            624, 1344,
            624, 1344,
            624, 1344,
            624, 1344,
            624, 1344,
        };

        readonly int[] clustersize = {
            1024, 1024,
            1024, 1024,
            1024, 1024,
            1024, 1024,
            1024, 1024,
        };

        readonly string[] volumename = {
            null, null,
            null, null,
            null, null,
            null, null,
            null, null,
        };

        readonly string[] volumeserial = {
            null, null,
            null, null,
            null, null,
            null, null,
            null, null,
        };

        readonly string[] type = {
            "UFS", "UFS",
            "UFS", "UFS",
            "UFS", "UFS",
            "UFS", "UFS",
            "UFS", "UFS",
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "ufs_next", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new ZZZRawImage();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                Filesystem fs = new DiscImageChef.Filesystems.FFSPlugin();
                int part = -1;
                for(int j = 0; j < partitions.Count; j++)
                {
                    if(partitions[j].Type == "4.3BSD" || partitions[j].Type == "4.4BSD")
                    {
                        part = j;
                        break;
                    }
                }
                Assert.AreNotEqual(-1, part, string.Format("Partition not found on {0}", testfiles[i]));
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out string information);
                Assert.AreEqual(clusters[i], fs.XmlFSType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFSType.ClusterSize, testfiles[i]);
                Assert.AreEqual(type[i], fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSerial, testfiles[i]);
            }
        }
    }
}
