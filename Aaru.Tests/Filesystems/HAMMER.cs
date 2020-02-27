// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HAMMER.cs
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
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using Aaru.Filesystems;
using Aaru.Filters;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems
{
    [TestFixture]
    public class HammerMbr
    {
        readonly string[] testfiles = {"dflybsd_3.6.1.vdi.lz", "dflybsd_4.0.5.vdi.lz"};

        readonly ulong[] sectors = {104857600, 104857600};

        readonly uint[] sectorsize = {512, 512};

        readonly long[] clusters = {6310, 6310};

        readonly int[] clustersize = {8388608, 8388608};

        readonly string[] volumename = {"Volume label", "Volume label"};

        readonly string[] volumeserial =
        {
            "f8e1a8bb-626d-11e7-94b5-0900274691e4", "ff4dc664-6276-11e7-983f-090027c41b46"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "hammer_mbr", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new HAMMER();
                int             part       = -1;
                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "Hammer")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],     fs.XmlFsType.Clusters,     testfiles[i]);
                Assert.AreEqual(clustersize[i],  fs.XmlFsType.ClusterSize,  testfiles[i]);
                Assert.AreEqual("HAMMER",        fs.XmlFsType.Type,         testfiles[i]);
                Assert.AreEqual(volumename[i],   fs.XmlFsType.VolumeName,   testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
            }
        }
    }
}