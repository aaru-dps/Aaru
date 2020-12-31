// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ext2.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru unit testing.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System.Collections.Generic;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using Aaru.Filesystems;
using Aaru.Filters;
using Aaru.Partitions;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems
{
    [TestFixture]
    public class Ext2
    {
        readonly string[] _testFiles =
        {
            "linux_ext2.aif", "linux_ext3.aif", "linux_ext4.aif", "netbsd_7.1.aif", "netbsd_7.1_r0.aif",
            "linux_4.19_ext2_flashdrive.aif", "linux_4.19_ext3_flashdrive.aif", "linux_4.19_ext4_flashdrive.aif"
        };

        readonly ulong[] _sectors =
        {
            262144, 262144, 262144, 8388608, 2097152, 1024000, 1024000, 1024000
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] _clusters =
        {
            130048, 130048, 130048, 1046567, 260135, 510976, 510976, 510976
        };

        readonly int[] _clusterSize =
        {
            1024, 1024, 1024, 4096, 4096, 1024, 1024, 1024
        };

        readonly string[] _volumeName =
        {
            "VolumeLabel", "VolumeLabel", "VolumeLabel", "Volume label", "Volume label", "DicSetter", "DicSetter",
            "DicSetter"
        };

        readonly string[] _volumeSerial =
        {
            "8e3992cf-7d98-e44a-b753-0591a35913eb", "1b411516-5415-4b42-95e6-1a247056a960",
            "b2f8f305-770f-ad47-abe4-f0484aa319e9", "e72aee05-627b-11e7-a573-0800272a08ec",
            "072756f2-627c-11e7-a573-0800272a08ec", "f5b2500f-99fb-764b-a6c4-c4db0b98a653",
            "a3914b55-260f-7245-8c72-7ccdf45436cb", "10413797-43d1-6545-8fbc-6ebc9d328be9"
        };

        readonly string[] _extversion =
        {
            "ext2", "ext3", "ext4", "ext2", "ext2", "ext2", "ext3", "ext4"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "ext2", _testFiles[i]);
                IFilter filter   = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                IPartition parts = new MBR();
                Assert.AreEqual(true, parts.GetInformation(image, out List<Partition> partitions, 0), _testFiles[i]);
                IFilesystem fs   = new ext2FS();
                int         part = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x83")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual(_extversion[i], fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
            }
        }
    }
}