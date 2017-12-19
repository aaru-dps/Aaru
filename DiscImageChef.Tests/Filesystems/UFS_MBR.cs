// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : UFS_MBR.cs
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;
using NUnit.Framework;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class UFS_MBR
    {
        readonly string[] testfiles =
        {
            "ufs1/linux.vdi.lz", "ufs2/linux.vdi.lz", "ffs43/darwin_1.3.1.vdi.lz", "ffs43/darwin_1.4.1.vdi.lz",
            "ffs43/darwin_6.0.2.vdi.lz", "ffs43/darwin_8.0.1.vdi.lz", "ffs43/dflybsd_1.2.0.vdi.lz",
            "ffs43/dflybsd_3.6.1.vdi.lz", "ffs43/dflybsd_4.0.5.vdi.lz", "ffs43/netbsd_1.6.vdi.lz",
            "ffs43/netbsd_7.1.vdi.lz", "ufs1/darwin_1.3.1.vdi.lz", "ufs1/darwin_1.4.1.vdi.lz",
            "ufs1/darwin_6.0.2.vdi.lz", "ufs1/darwin_8.0.1.vdi.lz", "ufs1/dflybsd_1.2.0.vdi.lz",
            "ufs1/dflybsd_3.6.1.vdi.lz", "ufs1/dflybsd_4.0.5.vdi.lz", "ufs1/freebsd_6.1.vdi.lz",
            "ufs1/freebsd_7.0.vdi.lz", "ufs1/freebsd_8.2.vdi.lz", "ufs1/netbsd_1.6.vdi.lz", "ufs1/netbsd_7.1.vdi.lz",
            "ufs1/solaris_7.vdi.lz", "ufs1/solaris_9.vdi.lz", "ufs2/freebsd_6.1.vdi.lz", "ufs2/freebsd_7.0.vdi.lz",
            "ufs2/freebsd_8.2.vdi.lz", "ufs2/netbsd_7.1.vdi.lz",
        };

        readonly ulong[] sectors =
        {
            262144, 262144, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 409600, 204800,
            204800, 204800, 204800, 2097152, 2097152, 2097152, 2097152, 8388608, 8388608, 2097152, 1024000, 2097152,
            2097152, 16777216, 16777216, 16777216, 2097152,
        };

        readonly uint[] sectorsize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512,
            512, 512, 512, 512, 512, 512, 512, 512,
        };

        readonly long[] clusters =
        {
            65024, 65024, 511024, 511024, 511024, 511488, 511950, 255470, 255470, 511992, 204768, 102280, 102280,
            102280, 102368, 1048500, 523758, 523758, 262138, 1048231, 2096462, 524284, 511968, 1038240, 1046808,
            2096472, 2096472, 4192945, 524272,
        };

        readonly int[] clustersize =
        {
            2048, 2048, 1024, 1024, 1024, 1024, 1024, 2048, 2048, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 2048, 2048,
            4096, 4096, 2048, 2048, 1024, 1024, 1024, 4096, 4096, 2048, 2048,
        };

        readonly string[] volumename =
        {
            null, "VolumeLabel", null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, "VolumeLabel", "VolumeLabel", "VolumeLabel", "",
        };

        readonly string[] volumeserial =
        {
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null,
        };

        readonly string[] type =
        {
            "UFS", "UFS2", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS",
            "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS2", "UFS2", "UFS2", "UFS2",
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "ufs_mbr", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new VDI();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                Filesystem fs = new DiscImageChef.Filesystems.FFSPlugin();
                int part = -1;
                for(int j = 0; j < partitions.Count; j++)
                {
                    if(partitions[j].Type == "0x63" || partitions[j].Type == "0xA8" || partitions[j].Type == "0xA5" ||
                       partitions[j].Type == "0xA9" || partitions[j].Type == "0x82" || partitions[j].Type == "0x83" ||
                       partitions[j].Type == "4.2BSD Fast File System" || partitions[j].Type == "Sun boot")
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