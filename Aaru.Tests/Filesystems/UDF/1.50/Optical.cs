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

namespace Aaru.Tests.Filesystems.UDF._150
{
    [TestFixture]
    public class Optical
    {
        readonly string[] _testFiles =
        {
            "1.50/ecs20.aif", "2.00/ecs20.aif", "2.01/ecs20.aif", "2.01/ecs20_cdrw.aif"
        };

        readonly ulong[] _sectors =
        {
            2295104, 2295104, 2295104, 295264
        };

        readonly uint[] _sectorSize =
        {
            2048, 2048, 2048, 2048
        };

        readonly long[] _clusters =
        {
            2295104, 2295104, 2295104, 295264
        };

        readonly int[] _clusterSize =
        {
            2048, 2048, 2048, 2048
        };

        readonly string[] _udfversion =
        {
            "UDF v2.01", "UDF v2.01", "UDF v2.01", "UDF v2.01"
        };

        readonly string[] _volumeName =
        {
            "Volume label", "UDF5A5DEF48", "VolLabel", "UDF5A5DFF10"
        };

        readonly string[] _volumeSerial =
        {
            "Volume Set ID not specified", "Volume Set ID not specified", "VolumeSetId", "Volume Set ID not specified"
        };

        readonly string[] _oemId =
        {
            "*ExpressUDF", "*ExpressUDF", "*ExpressUDF", "*ExpressUDF"
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