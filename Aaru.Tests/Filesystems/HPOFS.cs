// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HPOFS.cs
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

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
    public class Hpofs
    {
        readonly string[] _testFiles =
        {
            "rid1.img.lz", "rid10.img.lz", "rid66percent.img.lz", "rid266.img.lz"
        };

        readonly MediaType[] _mediaTypes =
        {
            MediaType.DOS_35_HD, MediaType.DOS_35_HD, MediaType.DOS_35_HD, MediaType.DOS_35_HD
        };

        readonly ulong[] _sectors =
        {
            2880, 2880, 2880, 2880
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512, 512
        };

        readonly long[] _clusters =
        {
            2880, 2880, 2880, 2880
        };

        readonly int[] _clusterSize =
        {
            512, 512, 512, 512
        };

        readonly string[] _volumeName =
        {
            "VOLUME LABEL", "VOLUME LABEL", "VOLUME LABEL", "VOLUME LABEL"
        };

        readonly string[] _volumeSerial =
        {
            "AC226814", "AC160814", "AC306C14", "ABEF2C14"
        };

        readonly string[] _oemId =
        {
            "IBM 10.2", "IBM 10.2", "IBM 10.2", "IBM 10.2"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems",
                                               "High Performance Optical File System", _testFiles[i]);

                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                IFilesystem fs = new HPOFS();

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
                Assert.AreEqual("HPOFS", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
                Assert.AreEqual(_oemId[i], fs.XmlFsType.SystemIdentifier, _testFiles[i]);
            }
        }
    }
}