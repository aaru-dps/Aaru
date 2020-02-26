// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : XFS.cs
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.DiscImages;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using NUnit.Framework;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class XfsMbr
    {
        readonly string[] testfiles = {"linux.vdi.lz", "linux_4.19_xfs_flashdrive.vdi.lz"};

        readonly ulong[] sectors = {1048576, 1024000};

        readonly uint[] sectorsize = {512, 512};

        readonly long[] clusters = {130816, 127744};

        readonly int[] clustersize = {4096, 4096};

        readonly string[] volumename = {"Volume label", "DicSetter"};

        readonly string[] volumeserial =
        {
            "230075b7-9834-b44e-a257-982a058311d8", "ed6b4d35-aa66-ce4a-9d8f-c56dbc6d7c8c"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "xfs_mbr", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new XFS();
                int             part       = -1;
                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x83")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],      fs.XmlFsType.Clusters,     testfiles[i]);
                Assert.AreEqual(clustersize[i],   fs.XmlFsType.ClusterSize,  testfiles[i]);
                Assert.AreEqual("XFS filesystem", fs.XmlFsType.Type,         testfiles[i]);
                Assert.AreEqual(volumename[i],    fs.XmlFsType.VolumeName,   testfiles[i]);
                Assert.AreEqual(volumeserial[i],  fs.XmlFsType.VolumeSerial, testfiles[i]);
            }
        }
    }
}