// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : MINIXv1.cs
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using NUnit.Framework;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class MinixV1
    {
        readonly string[] testfiles =
        {
            "minix_3.1.2a_dsdd.img.lz", "minix_3.1.2a_dshd.img.lz", "minix_3.1.2a_mf2dd.img.lz",
            "minix_3.1.2a_mf2hd.img.lz"
        };

        readonly MediaType[] mediatypes =
            {MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD};

        readonly ulong[] sectors = {720, 2400, 1440, 2880};

        readonly uint[] sectorsize = {512, 512, 512, 512};

        readonly long[] clusters = {360, 1200, 720, 1440};

        readonly int[] clustersize = {1024, 1024, 1024, 1024};

        readonly string[] types = {"Minix 3 v1", "Minix 3 v1", "Minix 3 v1", "Minix 3 v1"};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "minixv1", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new ZZZRawImage();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.ImageInfo.MediaType, testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.SectorSize, testfiles[i]);
                Filesystem fs = new MinixFS();
                Partition wholePart = new Partition
                {
                    Name = "Whole device",
                    Length = image.ImageInfo.Sectors,
                    Size = image.ImageInfo.Sectors * image.ImageInfo.SectorSize
                };
                Assert.AreEqual(true, fs.Identify(image, wholePart), testfiles[i]);
                fs.GetInformation(image, wholePart, out _);
                Assert.AreEqual(clusters[i], fs.XmlFSType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFSType.ClusterSize, testfiles[i]);
                Assert.AreEqual(types[i], fs.XmlFSType.Type, testfiles[i]);
            }
        }
    }
}