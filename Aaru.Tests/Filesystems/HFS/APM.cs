// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HFS.cs
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

namespace Aaru.Tests.Filesystems.HFS
{
    [TestFixture]
    public class APM
    {
        readonly string[] _testFiles =
        {
            "amigaos_3.9.aif", "darwin_1.3.1.aif", "darwin_1.4.1.aif", "darwin_6.0.2.aif", "darwin_8.0.1.aif",
            "macos_1.1.aif", "macos_2.0.aif", "macos_6.0.7.aif", "macos_7.5.3.aif", "macos_7.5.aif", "macos_7.6.aif",
            "macos_8.0.aif", "macos_8.1.aif", "macos_9.0.4.aif", "macos_9.1.aif", "macos_9.2.1.aif", "macos_9.2.2.aif",
            "macosx_10.2.aif", "macosx_10.3.aif", "macosx_10.4.aif", "rhapsody_dr1.aif", "d2_driver.aif", "hdt_1.8.aif",
            "macos_4.2.aif", "macos_4.3.aif", "macos_6.0.2.aif", "macos_6.0.3.aif", "macos_6.0.4.aif",
            "macos_6.0.5.aif", "macos_6.0.8.aif", "macos_6.0.aif", "macos_7.0.aif", "macos_7.1.1.aif", "parted.aif",
            "silverlining_2.2.1.aif", "speedtools_3.6.aif", "vcpformatter_2.1.1.aif"
        };

        readonly ulong[] _sectors =
        {
            1024128, 409600, 409600, 409600, 409600, 41820, 41820, 81648, 1024000, 1024000, 1024000, 1024000, 1024000,
            1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 409600, 51200, 51200, 41820, 41820, 54840,
            54840, 54840, 54840, 54840, 41820, 54840, 54840, 262144, 51200, 51200, 54840
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512,
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] _clusters =
        {
            64003, 51189, 51189, 58502, 58502, 41788, 38950, 39991, 63954, 63990, 63954, 63954, 63954, 63922, 63922,
            63922, 63922, 63884, 63883, 63883, 58506, 50926, 50094, 38950, 38950, 38950, 38950, 7673, 38950, 38950,
            38950, 38950, 38950, 46071, 50382, 49135, 54643
        };

        readonly int[] _clusterSize =
        {
            8192, 4096, 4096, 3584, 3584, 512, 512, 1024, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192,
            8192, 8192, 3584, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 1024, 512, 512, 512
        };

        readonly string[] _volumeName =
        {
            "Volume label", "Volume label", "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label", "Volume label", "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label", "Volume label", "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label", "Volume label", "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label", "Volume label", "Volume label", "Test disk", "Volume label", "Volume label", "Volume label",
            "Volume label", "Volume label", "Untitled", "Untitled  #1", "24 MB Disk", "Volume label"
        };

        readonly string[] _volumeSerial =
        {
            null, null, null, null, "AAFE1382AF5AA898", null, null, null, null, null, null, null, null, null, null,
            null, null, "5A7C38B0CAF279C4", "FB49083EBD150509", "632C0B1DB46FD188", null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Apple HFS (APM)", _testFiles[i]);
                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AppleHFS();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "Apple_HFS")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual("HFS", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
            }
        }
    }
}