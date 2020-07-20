// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : LisaFS.cs
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
        readonly string[] testfiles =
        {
            "166files.dc42.lz", "222files.dc42.lz", "blank2.0.dc42.lz", "blank-disk.dc42.lz",
            "file-with-a-password.dc42.lz", "tfwdndrc-has-been-erased.dc42.lz", "tfwdndrc-has-been-restored.dc42.lz",
            "three-empty-folders.dc42.lz", "three-folders-with-differently-named-docs.dc42.lz",
            "three-folders-with-differently-named-docs-root-alphabetical.dc42.lz",
            "three-folders-with-differently-named-docs-root-chronological.dc42.lz",
            "three-folders-with-identically-named-docs.dc42.lz", "lisafs1.dc42.lz", "lisafs2.dc42.lz",
            "lisafs3.dc42.lz", "lisafs3_with_desktop.dc42.lz"
        };

        readonly MediaType[] mediatypes =
        {
            MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS,
            MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS,
            MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS,
            MediaType.AppleFileWare, MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS
        };

        readonly ulong[] sectors =
        {
            800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 800, 1702, 800, 800, 800
        };

        readonly uint[] sectorsize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] clusters =
        {
            800, 800, 792, 800, 800, 800, 800, 800, 800, 800, 800, 800, 1684, 792, 800, 800
        };

        readonly int[] clustersize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly string[] volumename =
        {
            "166Files", "222Files", "AOS  4:59 pm 10/02/87", "AOS 3.0", "AOS 3.0", "AOS 3.0", "AOS 3.0", "AOS 3.0",
            "AOS 3.0", "AOS 3.0", "AOS 3.0", "AOS 3.0", "AOS 4:15 pm 5/06/1983", "Office System 1 2.0",
            "Office System 1 3.0", "AOS 3.0"
        };

        readonly string[] volumeserial =
        {
            "A23703A202010663", "A23703A201010663", "A32D261301010663", "A22CB48D01010663", "A22CC3A702010663",
            "A22CB48D14010663", "A22CB48D14010663", "A22CB48D01010663", "A22CB48D01010663", "A22CB48D01010663",
            "A22CB48D01010663", "A22CB48D01010663", "9924151E190001E1", "9497F10016010D10", "9CF9CF89070100A8",
            "A4FE1A191F011652"
        };

        readonly string[] oemid =
        {
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "Filesystems", "Apple Lisa filesystem",
                                               testfiles[i]);

                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new DiskCopy42();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.Info.MediaType, testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                IFilesystem fs = new LisaFS();

                var wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };

                Assert.AreEqual(true, fs.Identify(image, wholePart), testfiles[i]);
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(clusters[i], fs.XmlFsType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFsType.ClusterSize, testfiles[i]);
                Assert.AreEqual("LisaFS", fs.XmlFsType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFsType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
                Assert.AreEqual(oemid[i], fs.XmlFsType.SystemIdentifier, testfiles[i]);
            }
        }

        [Test]
        public void TestContents() => throw new NotImplementedException();
    }
}