// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UDF.cs
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

using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using Aaru.Filters;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.UDF._200
{
    [TestFixture]
    public class Whole
    {
        readonly string[] _testFiles =
        {
            "1.02/linux.aif", "1.02/macosx_10.11.aif", "1.50/linux.aif", "1.50/macosx_10.11.aif", "2.00/linux.aif",
            "2.00/macosx_10.11.aif", "2.01/linux.aif", "2.01/macosx_10.11.aif", "2.50/linux.aif",
            "2.50/macosx_10.11.aif", "2.60/macosx_10.11.aif", "1.50/solaris_7.aif", "1.50/solaris_9.aif",
            "2.01/netbsd_7.1.aif", "1.02/linux_4.19_udf_1.02_flashdrive.aif", "1.50/linux_4.19_udf_1.50_flashdrive.aif",
            "2.00/linux_4.19_udf_2.00_flashdrive.aif", "2.01/linux_4.19_udf_2.01_flashdrive.aif"
        };

        readonly ulong[] _sectors =
        {
            1024000, 204800, 1024000, 409600, 1024000, 614400, 1024000, 819200, 1024000, 1024000, 1228800, 8388608,
            8388608, 8388608, 1024000, 1024000, 1024000, 1024000
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] _clusters =
        {
            1024000, 204800, 1024000, 409600, 1024000, 614400, 1024000, 819200, 1024000, 1024000, 1228800, 8388608,
            8388608, 8388608, 1024000, 1024000, 1024000, 1024000
        };

        readonly int[] _clusterSize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly string[] _udfversion =
        {
            "UDF v1.02", "UDF v1.02", "UDF v1.50", "UDF v1.50", "UDF v2.00", "UDF v2.00", "UDF v2.01", "UDF v2.01",
            "UDF v2.50", "UDF v2.50", "UDF v2.60", "UDF v1.50", "UDF v1.50", "UDF v2.01", "UDF v2.01", "UDF v2.01",
            "UDF v2.01", "UDF v2.01"
        };

        readonly string[] _volumeName =
        {
            "Volume label", "Volume label", "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label", "Volume label", "Volume label", "Volume label", "Volume label", "*NoLabel*", "*NoLabel*",
            "anonymous", "DicSetter", "DicSetter", "DicSetter", "DicSetter", "DicSetter", "DicSetter"
        };

        readonly string[] _volumeSerial =
        {
            "595c5cfa38ce8b66LinuxUDF", "6D02A231 (Mac OS X newfs_udf) UDF Volume Set", "595c5d00c5b3405aLinuxUDF",
            "4DD0458B (Mac OS X newfs_udf) UDF Volume Set", "595c5d07f4fc8e8dLinuxUDF",
            "5D91CB4F (Mac OS X newfs_udf) UDF Volume Set", "595c5d0bee60c3bbLinuxUDF",
            "48847EB3 (Mac OS X newfs_udf) UDF Volume Set", "595c5d0e4f338552LinuxUDF",
            "709E84A1 (Mac OS X newfs_udf) UDF Volume Set", "78CE3237 (Mac OS X newfs_udf) UDF Volume Set", "595EB2A9",
            "595EB55A", "7cc94d726669d773", "5cc7882441a86e93LinuxUDF", "5cc78f8bba4dfe00LinuxUDF",
            "5cc7f4183e0d5f7aLinuxUDF", "5cc8816fcb3a3b38LinuxUDF", "595EB55A", "7cc94d726669d773"
        };

        readonly string[] _oemId =
        {
            "*Linux UDFFS", "*Apple Mac OS X UDF FS", "*Linux UDFFS", "*Apple Mac OS X UDF FS", "*Linux UDFFS",
            "*Apple Mac OS X UDF FS", "*Linux UDFFS", "*Apple Mac OS X UDF FS", "*Linux UDFFS",
            "*Apple Mac OS X UDF FS", "*Apple Mac OS X UDF FS", "*SUN SOLARIS UDF", "*SUN SOLARIS UDF",
            "*NetBSD userland UDF", "*Linux UDFFS", "*Linux UDFFS", "*Linux UDFFS", "*Linux UDFFS", "*Linux UDFFS",
            "*Linux UDFFS"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Universal Disc Format",
                                               _testFiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                IFilesystem fs = new Aaru.Filesystems.UDF();

                var wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };

                Assert.AreEqual(true, fs.Identify(image, wholePart), _testFiles[i]);
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual(_udfversion[i], fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSetIdentifier, _testFiles[i]);
                Assert.AreEqual(_oemId[i], fs.XmlFsType.SystemIdentifier, _testFiles[i]);
            }
        }
    }
}