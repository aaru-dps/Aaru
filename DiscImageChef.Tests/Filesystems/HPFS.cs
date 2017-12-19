// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HPFS.cs
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
    public class HPFS
    {
        readonly string[] testfiles = {
            "ecs.vdi.lz", "msos2_1.21.vdi.lz", "msos2_1.30.1.vdi.lz", "os2_1.20.vdi.lz",
            "os2_1.30.vdi.lz", "os2_6.307.vdi.lz", "os2_6.514.vdi.lz", "os2_6.617.vdi.lz",
            "os2_8.162.vdi.lz", "os2_9.023.vdi.lz", "winnt_3.10.vdi.lz", "winnt_3.50.vdi.lz",
        };

        readonly ulong[] sectors = {
            262144, 1024000, 1024000, 1024000,
            1024000, 1024000, 262144, 262144,
            262144, 262144, 262144, 262144
        };

        readonly uint[] sectorsize = {
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512
        };

        readonly long[] clusters = {
            261072, 1023056, 1023056, 1023056,
            1023056, 1023056, 262016, 262016,
            262016, 262016, 262016, 262112,
        };

        readonly int[] clustersize = {
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
        };

        readonly string[] volumename = {
            "VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",
            "VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",
            "VOLUMELABEL","VOLUMELABEL","VOLUMELABEL","VOLUMELABEL",
        };

        readonly string[] volumeserial = {
            "2BBBD814","AC0DDC15","ABEB2C15","6C4EE015",
            "6C406015","6C49B015","2BCEB414","2C157414",
            "2BF55414","2BE31414","E851CB14","A4EDC29C",
        };

        readonly string[] oemid = {
            "IBM 4.50", "OS2 10.1", "OS2 10.0", "OS2 10.0",
            "OS2 10.0", "OS2 20.0", "OS2 20.0", "OS2 20.1",
            "OS2 20.0", "OS2 20.0", "MSDOS5.0", "MSDOS5.0",
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "hpfs", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new VDI();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                Filesystem fs = new DiscImageChef.Filesystems.HPFS();
                Assert.AreEqual(true, fs.Identify(image, partitions[0]), testfiles[i]);
                fs.GetInformation(image, partitions[0], out string information);
                Assert.AreEqual(clusters[i], fs.XmlFSType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFSType.ClusterSize, testfiles[i]);
                Assert.AreEqual("HPFS", fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSerial, testfiles[i]);
                Assert.AreEqual(oemid[i], fs.XmlFSType.SystemIdentifier, testfiles[i]);
            }
        }
    }
}
