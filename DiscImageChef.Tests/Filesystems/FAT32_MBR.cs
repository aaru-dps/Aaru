// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : FAT32_MBR.cs
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
using DiscImageChef.PartPlugins;
using NUnit.Framework;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class FAT32_MBR
    {
        readonly string[] testfiles = {
            "drdos_7.03.vdi.lz", "drdos_8.00.vdi.lz", "msdos_7.10.vdi.lz", "macosx_10.11.vdi.lz",
            "win10.vdi.lz", "win2000.vdi.lz", "win95osr2.1.vdi.lz", "win95osr2.5.vdi.lz",
            "win95osr2.vdi.lz", "win98se.vdi.lz", "win98.vdi.lz", "winme.vdi.lz",
            "winvista.vdi.lz", "beos_r4.5.vdi.lz", "linux.vdi.lz","aros.vdi.lz",
            "freebsd_6.1.vdi.lz","freebsd_7.0.vdi.lz","freebsd_8.2.vdi.lz","freedos_1.2.vdi.lz",
        };

        readonly ulong[] sectors = {
            8388608, 8388608, 8388608, 4194304,
            4194304, 8388608, 4194304, 4194304,
            4194304, 4194304, 4194304, 4194304,
            4194304, 4194304, 262144, 4194304,
            4194304, 4194304, 4194304, 8388608,
        };

        readonly uint[] sectorsize = {
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
        };

        readonly long[] clusters = {
            1048233, 1048233, 1048233, 524287,
            524016, 1048233, 524152, 524152,
            524152, 524112, 524112, 524112,
            523520, 1048560, 260096, 524160,
            524112, 524112, 65514, 1048233,
        };

        readonly int[] clustersize = {
            4096, 4096, 4096, 4096,
            4096, 4096, 4096, 4096,
            4096, 4096, 4096, 4096,
            4096, 2048, 512, 4096,
            4096, 4096, 32768, 4096,
        };

        readonly string[] volumename = {
            "VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",
            "VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",
            "VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",
            "VOLUMELABEL","VOLUMELABEL","VolumeLabel","VOLUMELABEL",
            "VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",
        };

        readonly string[] volumeserial = {
            "5955996C","1BFB1A43","3B331809","42D51EF1",
            "48073346","EC62E6DE","2A310DE4","0C140DFC",
            "3E310D18","0D3D0EED","0E131162","3F500F02",
            "82EB4C04","00000000","B488C502","5CAC9B4E",
            "41540E0E","4E600E0F","26E20E0F","3E0C1BE8",
        };

        readonly string[] oemid = {
            "DRDOS7.X", "IBM  7.1", "MSWIN4.1", "BSD  4.4",
            "MSDOS5.0", "MSDOS5.0", "MSWIN4.1", "MSWIN4.1",
            "MSWIN4.1", "MSWIN4.1", "MSWIN4.1", "MSWIN4.1",
            "MSDOS5.0", "BeOS    ", "mkfs.fat", "MSWIN4.1",
            "BSD  4.4", "BSD  4.4", "BSD4.4  ", "FRDOS4.1",
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "fat32_mbr", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new VDI();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                Filesystem fs = new FAT();
                Assert.AreEqual(true, fs.Identify(image, partitions[0]), testfiles[i]);
                fs.GetInformation(image, partitions[0], out string information);
                Assert.AreEqual(clusters[i], fs.XmlFSType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFSType.ClusterSize, testfiles[i]);
                Assert.AreEqual("FAT32", fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSerial, testfiles[i]);
                Assert.AreEqual(oemid[i], fs.XmlFSType.SystemIdentifier, testfiles[i]);
            }
        }
    }
}
