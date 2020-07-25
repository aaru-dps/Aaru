// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : NWFS386.cs
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems
{
    [TestFixture, SuppressMessage("ReSharper", "UnusedMember.Local")]
    public class Nwfs386
    {
        readonly string[] _testFiles =
        {
            "netware_3.12.aif"
        };

        readonly ulong[] _sectors =
        {
            104857600
        };

        readonly uint[] _sectorSize =
        {
            512
        };

        readonly long[] _clusters =
        {
            104856192
        };

        readonly int[] _clusterSize =
        {
            512
        };

        readonly string[] _volumeName =
        {
            "Volume label"
        };

        readonly string[] _volumeSerial =
        {
            "UNKNOWN"
        };

        [Test]
        public void Test() => throw new NotImplementedException("NWFS386 filesystem is not yet implemented");

        /*
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "Filesystems", "NetWare File System (32-bit)", testfiles[i]);
                Filter filter = new ZZZNoFilter();
                filter.Open(location);
                ImagePlugin image = new AaruFormat();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                Filesystem fs = new Aaru.Filesystems.NetWare();
                int part = -1;
                for(int j = 0; j < partitions.Count; j++)
                {
                    if(partitions[j].PartitionType == "0x65")
                    {
                        part = j;
                        break;
                    }
                }
                Assert.AreNotEqual(-1, part, string.Format("Partition not found on {0}", testfiles[i]));
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _);
                Assert.AreEqual(clusters[i], fs.XmlFSType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFSType.ClusterSize, testfiles[i]);
                Assert.AreEqual("NWFS386", fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSerial, testfiles[i]);
            }*/
    }
}