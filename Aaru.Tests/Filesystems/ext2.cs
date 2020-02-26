// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ext2.cs
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
using DiscImageChef.Partitions;
using NUnit.Framework;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class Ext2
    {
        readonly string[] testfiles =
        {
            "linux_ext2.vdi.lz", "linux_ext3.vdi.lz", "linux_ext4.vdi.lz", "netbsd_7.1.vdi.lz",
            "netbsd_7.1_r0.vdi.lz", "linux_4.19_ext2_flashdrive.vdi.lz", "linux_4.19_ext3_flashdrive.vdi.lz",
            "linux_4.19_ext4_flashdrive.vdi.lz"
        };

        readonly ulong[] sectors = {262144, 262144, 262144, 8388608, 2097152, 1024000, 1024000, 1024000};

        readonly uint[] sectorsize = {512, 512, 512, 512, 512, 512, 512, 512};

        readonly long[] clusters = {130048, 130048, 130048, 1046567, 260135, 510976, 510976, 510976};

        readonly int[] clustersize = {1024, 1024, 1024, 4096, 4096, 1024, 1024, 1024};

        readonly string[] volumename =
        {
            "VolumeLabel", "VolumeLabel", "VolumeLabel", "Volume label", "Volume label", "DicSetter", "DicSetter",
            "DicSetter"
        };

        readonly string[] volumeserial =
        {
            "8e3992cf-7d98-e44a-b753-0591a35913eb", "1b411516-5415-4b42-95e6-1a247056a960",
            "b2f8f305-770f-ad47-abe4-f0484aa319e9", "e72aee05-627b-11e7-a573-0800272a08ec",
            "072756f2-627c-11e7-a573-0800272a08ec", "f5b2500f-99fb-764b-a6c4-c4db0b98a653",
            "a3914b55-260f-7245-8c72-7ccdf45436cb", "10413797-43d1-6545-8fbc-6ebc9d328be9"
        };

        readonly string[] extversion = {"ext2", "ext3", "ext4", "ext2", "ext2", "ext2", "ext3", "ext4"};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "ext2", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                IPartition parts = new MBR();
                Assert.AreEqual(true, parts.GetInformation(image, out List<Partition> partitions, 0), testfiles[i]);
                IFilesystem fs   = new ext2FS();
                int         part = -1;
                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x83")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],     fs.XmlFsType.Clusters,     testfiles[i]);
                Assert.AreEqual(clustersize[i],  fs.XmlFsType.ClusterSize,  testfiles[i]);
                Assert.AreEqual(extversion[i],   fs.XmlFsType.Type,         testfiles[i]);
                Assert.AreEqual(volumename[i],   fs.XmlFsType.VolumeName,   testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
            }
        }
    }
}