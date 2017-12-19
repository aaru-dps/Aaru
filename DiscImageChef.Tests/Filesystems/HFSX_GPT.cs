// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HFSX_GPT.cs
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
    public class HFSX_GPT
    {
        readonly string[] testfiles = {
            "macosx_10.11.vdi.lz", "macosx_10.11_journal.vdi.lz",
        };

        readonly ulong[] sectors = {
            819200, 1228800,
        };

        readonly uint[] sectorsize = {
            512, 512
        };

        readonly long[] clusters = {
            102390, 153590,
        };

        readonly int[] clustersize = {
            4096, 4096,
        };

        readonly string[] volumename = {
            null, null,
        };

        readonly string[] volumeserial = {
            "328343989312AE9F","FB98504073464C5C",
        };

        readonly string[] oemid = {
            "10.0","HFSJ"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "hfsx_gpt", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new VDI();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                Filesystem fs = new DiscImageChef.Filesystems.AppleHFSPlus();
                int part = -1;
                for(int j = 0; j < partitions.Count; j++)
                {
                    if(partitions[j].Type == "Apple HFS")
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
                Assert.AreEqual("HFSX", fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSerial, testfiles[i]);
                Assert.AreEqual(oemid[i], fs.XmlFSType.SystemIdentifier, testfiles[i]);
            }
        }
    }
}
