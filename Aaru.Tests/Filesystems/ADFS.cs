// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ADFS.cs
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
    public class Adfs
    {
        readonly string[] _testFiles =
        {
            "adfs_d.adf.lz", "adfs_e.adf.lz", "adfs_f.adf.lz", "adfs_e+.adf.lz", "adfs_f+.adf.lz", "adfs_s.adf.lz",
            "adfs_m.adf.lz", "adfs_l.adf.lz", "hdd_old.hdf.lz", "hdd_new.hdf.lz"
        };

        readonly MediaType[] _mediaTypes =
        {
            MediaType.ACORN_35_DS_DD, MediaType.ACORN_35_DS_DD, MediaType.ACORN_35_DS_HD, MediaType.ACORN_35_DS_DD,
            MediaType.ACORN_35_DS_HD, MediaType.ACORN_525_SS_DD_40, MediaType.ACORN_525_SS_DD_80,
            MediaType.ACORN_525_DS_DD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD
        };

        readonly ulong[] _sectors =
        {
            800, 800, 1600, 800, 1600, 640, 1280, 2560, 78336, 78336
        };

        readonly uint[] _sectorSize =
        {
            1024, 1024, 1024, 1024, 1024, 256, 256, 256, 256, 256
        };

        readonly bool[] _bootable =
        {
            false, false, false, false, false, false, false, false, false, false
        };

        readonly long[] _clusters =
        {
            800, 800, 1600, 800, 1600, 640, 1280, 2560, 78336, 78336
        };

        readonly uint[] _clusterSize =
        {
            1024, 1024, 1024, 1024, 1024, 256, 256, 256, 256, 256
        };

        readonly string[] _volumeName =
        {
            "ADFSD", "ADFSE     ", null, "ADFSE+    ", null, "$", "$", "$", "VolLablOld", null
        };

        readonly string[] _volumeSerial =
        {
            "3E48", "E13A", null, "1142", null, "F20D", "D6CA", "0CA6", "080E", null
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems",
                                               "Acorn Advanced Disc Filing System", _testFiles[i]);

                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                IFilesystem fs = new AcornADFS();

                var wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };

                Assert.AreEqual(true, fs.Identify(image, wholePart), _testFiles[i]);
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(_bootable[i], fs.XmlFsType.Bootable, _testFiles[i]);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual("Acorn Advanced Disc Filing System", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
            }
        }
    }
}