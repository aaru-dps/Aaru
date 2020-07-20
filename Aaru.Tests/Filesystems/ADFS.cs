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
// Copyright © 2011-2019 Natalia Portillo
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
        readonly string[] _testfiles =
        {
            "adfs_d.adf.lz", "adfs_e.adf.lz", "adfs_f.adf.lz", "adfs_e+.adf.lz", "adfs_f+.adf.lz", "adfs_s.adf.lz",
            "adfs_m.adf.lz", "adfs_l.adf.lz", "hdd_old.hdf.lz", "hdd_new.hdf.lz"
        };

        readonly MediaType[] _mediatypes =
        {
            MediaType.ACORN_35_DS_DD, MediaType.ACORN_35_DS_DD, MediaType.ACORN_35_DS_HD, MediaType.ACORN_35_DS_DD,
            MediaType.ACORN_35_DS_HD, MediaType.ACORN_525_SS_DD_40, MediaType.ACORN_525_SS_DD_80,
            MediaType.ACORN_525_DS_DD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD
        };

        readonly ulong[] _sectors =
        {
            800, 800, 1600, 800, 1600, 640, 1280, 2560, 78336, 78336
        };

        readonly uint[] _sectorsize =
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

        readonly uint[] _clustersize =
        {
            1024, 1024, 1024, 1024, 1024, 256, 256, 256, 256, 256
        };

        readonly string[] _volumename =
        {
            "ADFSD", "ADFSE     ", null, "ADFSE+    ", null, "$", "$", "$", "VolLablOld", null
        };

        readonly string[] _volumeserial =
        {
            "3E48", "E13A", null, "1142", null, "F20D", "D6CA", "0CA6", "080E", null
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems",
                                               "Acorn Advanced Disc Filing System", _testfiles[i]);

                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true, image.Open(filter), _testfiles[i]);
                Assert.AreEqual(_mediatypes[i], image.Info.MediaType, _testfiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testfiles[i]);
                Assert.AreEqual(_sectorsize[i], image.Info.SectorSize, _testfiles[i]);
                IFilesystem fs = new AcornADFS();

                var wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };

                Assert.AreEqual(true, fs.Identify(image, wholePart), _testfiles[i]);
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(_bootable[i], fs.XmlFsType.Bootable, _testfiles[i]);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testfiles[i]);
                Assert.AreEqual(_clustersize[i], fs.XmlFsType.ClusterSize, _testfiles[i]);
                Assert.AreEqual("Acorn Advanced Disc Filing System", fs.XmlFsType.Type, _testfiles[i]);
                Assert.AreEqual(_volumename[i], fs.XmlFsType.VolumeName, _testfiles[i]);
                Assert.AreEqual(_volumeserial[i], fs.XmlFsType.VolumeSerial, _testfiles[i]);
            }
        }
    }
}