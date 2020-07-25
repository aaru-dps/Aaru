// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : LisaFS.cs
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

using System;
using System.IO;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.DiscImages;
using Aaru.Filesystems.LisaFS;
using Aaru.Filters;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems
{
    [TestFixture]
    public class LisaFs
    {
        readonly string[] _testFiles =
        {
            "166files.dc42.lz", "222files.dc42.lz", "blank2.0.dc42.lz", "blank-disk.dc42.lz",
            "file-with-a-password.dc42.lz", "tfwdndrc-has-been-erased.dc42.lz", "tfwdndrc-has-been-restored.dc42.lz",
            "three-empty-folders.dc42.lz", "three-folders-with-differently-named-docs.dc42.lz",
            "three-folders-with-differently-named-docs-root-alphabetical.dc42.lz",
            "three-folders-with-differently-named-docs-root-chronological.dc42.lz",
            "three-folders-with-identically-named-docs.dc42.lz", "lisafs1.dc42.lz", "lisafs2.dc42.lz",
            "lisafs3.dc42.lz", "lisafs3_with_desktop.dc42.lz"
        };

        readonly MediaType[] _mediaTypes =
        {
            MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS,
            MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS,
            MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS,
            MediaType.AppleFileWare, MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS
        };

        readonly ulong[] _sectors =
        {
            800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 1702, 800, 800, 800
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] _clusters =
        {
            800, 800, 792, 800, 800, 800, 800, 800, 800, 800, 800, 800, 1684, 792, 800, 800
        };

        readonly int[] _clusterSize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly string[] _volumeName =
        {
            "166Files", "222Files", "AOS  4:59 pm 10/02/87", "AOS 3.0", "AOS 3.0", "AOS 3.0", "AOS 3.0", "AOS 3.0",
            "AOS 3.0", "AOS 3.0", "AOS 3.0", "AOS 3.0", "AOS 4:15 pm 5/06/1983", "Office System 1 2.0",
            "Office System 1 3.0", "AOS 3.0"
        };

        readonly string[] _volumeSerial =
        {
            "A23703A202010663", "A23703A201010663", "A32D261301010663", "A22CB48D01010663", "A22CC3A702010663",
            "A22CB48D14010663", "A22CB48D14010663", "A22CB48D01010663", "A22CB48D01010663", "A22CB48D01010663",
            "A22CB48D01010663", "A22CB48D01010663", "9924151E190001E1", "9497F10016010D10", "9CF9CF89070100A8",
            "A4FE1A191F011652"
        };

        readonly string[] _oemId =
        {
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Apple Lisa filesystem",
                                               _testFiles[i]);

                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new DiskCopy42();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                IFilesystem fs = new LisaFS();

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
                Assert.AreEqual("LisaFS", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
                Assert.AreEqual(_oemId[i], fs.XmlFsType.SystemIdentifier, _testFiles[i]);
            }
        }

        [Test]
        public void TestContents() => throw new NotImplementedException();
    }
}