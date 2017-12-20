// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HFSPlus_MBR.cs
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
using NUnit.Framework;

namespace DiscImageChef.Tests.Filesystems
{
    // Mising Darwin 6.0.2 wrapped
    [TestFixture]
    public class HFSPlus_MBR
    {
        readonly string[] testfiles =
        {
            "macosx_10.11.vdi.lz", "macosx_10.11_journal.vdi.lz", "linux.vdi.lz", "linux_journal.vdi.lz",
            "darwin_1.3.1.vdi.lz", "darwin_1.3.1_wrapped.vdi.lz", "darwin_1.4.1.vdi.lz", "darwin_1.4.1_wrapped.vdi.lz",
            "darwin_6.0.2.vdi.lz", "darwin_8.0.1_journal.vdi.lz", "darwin_8.0.1.vdi.lz", "darwin_8.0.1_wrapped.vdi.lz",
        };

        readonly ulong[] sectors =
            {303104, 352256, 262144, 262144, 819200, 614400, 819200, 614400, 819200, 1228800, 819200, 614400,};

        readonly uint[] sectorsize = {512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512,};

        readonly long[] clusters =
            {37878, 44021, 32512, 32512, 102178, 76708, 102178, 76708, 102178, 153592, 102392, 76774,};

        readonly int[] clustersize = {4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096,};

        readonly string[] volumename = {null, null, null, null, null, null, null, null, null, null, null, null,};

        readonly string[] volumeserial =
        {
            "C84F550907D13F50", "016599F88029F73D", null, null, null, null, null, null, null, "F92964F9B3F64ABB",
            "A8FAC484A0A2B177", "D5D5BF1346AD2B8D",
        };

        readonly string[] oemid =
            {"10.0", "HFSJ", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0",};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "hfsplus_mbr", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new Vdi();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                Filesystem fs = new DiscImageChef.Filesystems.AppleHFSPlus();
                int part = -1;
                for(int j = 0; j < partitions.Count; j++)
                {
                    if(partitions[j].Type == "0xAF")
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
                Assert.AreEqual("HFS+", fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSerial, testfiles[i]);
                Assert.AreEqual(oemid[i], fs.XmlFSType.SystemIdentifier, testfiles[i]);
            }
        }
    }
}