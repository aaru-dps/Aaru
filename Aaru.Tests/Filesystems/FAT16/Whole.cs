// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FAT16.cs
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
using Aaru.Filesystems;
using Aaru.Filters;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems.FAT16
{
    [TestFixture]
    public class Whole
    {
        readonly string[] _testFiles =
        {
            // MS-DOS 3.30A
            "msdos_3.30A_mf2ed.img.lz",

            // MS-DOS 3.31
            "msdos_3.31_mf2ed.img.lz"
        };

        readonly MediaType[] _mediaTypes =
        {
            // MS-DOS 3.30A
            MediaType.DOS_35_ED,

            // MS-DOS 3.31
            MediaType.DOS_35_ED
        };

        readonly ulong[] _sectors =
        {
            // MS-DOS 3.30A
            5760,

            // MS-DOS 3.31
            5760
        };

        readonly uint[] _sectorSize =
        {
            // MS-DOS 3.30A
            512,

            // MS-DOS 3.31
            512
        };

        readonly long[] _clusters =
        {
            // MS-DOS 3.30A
            5760,

            // MS-DOS 3.31
            5760
        };

        readonly int[] _clusterSize =
        {
            // MS-DOS 3.30A
            512,

            // MS-DOS 3.31
            512
        };

        readonly string[] _volumeName =
        {
            // MS-DOS 3.30A
            null,

            // MS-DOS 3.31
            null
        };

        readonly string[] _volumeSerial =
        {
            // MS-DOS 3.30A
            null,

            // MS-DOS 3.31
            null
        };

        readonly string[] _oemId =
        {
            // MS-DOS 3.30A
            "MSDOS3.3",

            // MS-DOS 3.31
            "IBM  3.3"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "FAT16", _testFiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                IFilesystem fs = new FAT();

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
                Assert.AreEqual("FAT16", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
                Assert.AreEqual(_oemId[i], fs.XmlFsType.SystemIdentifier, _testFiles[i]);
            }
        }
    }
}