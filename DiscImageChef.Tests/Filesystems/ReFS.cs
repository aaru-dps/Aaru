// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ReFS_MBR.cs
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
    [TestFixture]
    public class ReFsMbr
    {
        readonly string[] testfiles = {"win10.vdi.lz"};

        readonly ulong[] sectors = {67108864};

        readonly uint[] sectorsize = {512};

        readonly long[] clusters = {8372224};

        readonly int[] clustersize = {4096};

        readonly string[] volumename = {null};

        readonly string[] volumeserial = {null};

        readonly string[] oemid = {null};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "refs_mbr", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                int             part       = -1;
                for(int j = 0; j          < partitions.Count; j++)
                    if(partitions[j].Type == "0x07")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                IFilesystem fs = new ReFS();
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],             fs.XmlFsType.Clusters,         testfiles[i]);
                Assert.AreEqual(clustersize[i],          fs.XmlFsType.ClusterSize,      testfiles[i]);
                Assert.AreEqual("Resilient File System", fs.XmlFsType.Type,             testfiles[i]);
                Assert.AreEqual(volumename[i],           fs.XmlFsType.VolumeName,       testfiles[i]);
                Assert.AreEqual(volumeserial[i],         fs.XmlFsType.VolumeSerial,     testfiles[i]);
                Assert.AreEqual(oemid[i],                fs.XmlFsType.SystemIdentifier, testfiles[i]);
            }
        }
    }
}