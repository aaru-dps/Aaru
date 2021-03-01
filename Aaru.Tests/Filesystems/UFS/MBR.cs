// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UFS.cs
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
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.UFS
{
    [TestFixture]
    public class MBR
    {
        readonly string[] _testFiles =
        {
            "ufs1/linux.aif", "ufs2/linux.aif", "ffs43/darwin_1.3.1.aif", "ffs43/darwin_1.4.1.aif",
            "ffs43/darwin_6.0.2.aif", "ffs43/darwin_8.0.1.aif", "ffs43/dflybsd_1.2.0.aif", "ffs43/dflybsd_3.6.1.aif",
            "ffs43/dflybsd_4.0.5.aif", "ffs43/netbsd_1.6.aif", "ffs43/netbsd_7.1.aif", "ufs1/darwin_1.3.1.aif",
            "ufs1/darwin_1.4.1.aif", "ufs1/darwin_6.0.2.aif", "ufs1/darwin_8.0.1.aif", "ufs1/dflybsd_1.2.0.aif",
            "ufs1/dflybsd_3.6.1.aif", "ufs1/dflybsd_4.0.5.aif", "ufs1/freebsd_6.1.aif", "ufs1/freebsd_7.0.aif",
            "ufs1/freebsd_8.2.aif", "ufs1/netbsd_1.6.aif", "ufs1/netbsd_7.1.aif", "ufs1/solaris_7.aif",
            "ufs1/solaris_9.aif", "ufs2/freebsd_6.1.aif", "ufs2/freebsd_7.0.aif", "ufs2/freebsd_8.2.aif",
            "ufs2/netbsd_7.1.aif"
        };

        readonly ulong[] _sectors =
        {
            262144, 262144, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 409600, 204800,
            204800, 204800, 204800, 2097152, 2097152, 2097152, 2097152, 8388608, 8388608, 2097152, 1024000, 2097152,
            2097152, 16777216, 16777216, 16777216, 2097152
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512,
            512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] _clusters =
        {
            65024, 65024, 511024, 511024, 511024, 511488, 511950, 255470, 255470, 511992, 204768, 102280, 102280,
            102280, 102368, 1048500, 523758, 523758, 262138, 1048231, 2096462, 524284, 511968, 1038240, 1046808,
            2096472, 2096472, 4192945, 524272
        };

        readonly int[] _clusterSize =
        {
            2048, 2048, 1024, 1024, 1024, 1024, 1024, 2048, 2048, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 2048, 2048,
            4096, 4096, 2048, 2048, 1024, 1024, 1024, 4096, 4096, 2048, 2048
        };

        readonly string[] _volumeName =
        {
            null, "VolumeLabel", null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, "VolumeLabel", "VolumeLabel", "VolumeLabel", ""
        };

        readonly string[] _volumeSerial =
        {
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null
        };

        readonly string[] _type =
        {
            "UFS", "UFS2", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS",
            "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS2", "UFS2", "UFS2", "UFS2"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "UNIX filesystem (MBR)",
                                               _testFiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new FFSPlugin();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x63"                    ||
                       partitions[j].Type == "0xA8"                    ||
                       partitions[j].Type == "0xA5"                    ||
                       partitions[j].Type == "0xA9"                    ||
                       partitions[j].Type == "0x82"                    ||
                       partitions[j].Type == "0x83"                    ||
                       partitions[j].Type == "4.2BSD Fast File System" ||
                       partitions[j].Type == "Sun boot")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual(_type[i], fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
            }
        }
    }
}