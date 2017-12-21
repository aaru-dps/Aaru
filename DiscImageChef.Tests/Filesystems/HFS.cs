// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HFS.cs
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
    public class HFS
    {
        readonly string[] testfiles =
        {
            "macos_1.1_mf2dd.img.lz", "macos_2.0_mf2dd.img.lz", "macos_6.0.7_mf2dd.img.lz", "nextstep_3.3_mf2hd.img.lz",
            "openstep_4.0_mf2hd.img.lz", "openstep_4.2_mf2hd.img.lz", "rhapsody_dr1_mf2hd.img.lz"
        };

        readonly MediaType[] mediatypes =
        {
            MediaType.AppleSonyDS, MediaType.AppleSonyDS, MediaType.AppleSonyDS, MediaType.DOS_35_HD,
            MediaType.DOS_35_HD, MediaType.DOS_35_HD, MediaType.DOS_35_HD
        };

        readonly ulong[] sectors = {1600, 1600, 1600, 2880, 2880, 2880, 2880};

        readonly uint[] sectorsize = {512, 512, 512, 512, 512, 512, 512};

        readonly long[] clusters = {1594, 1594, 1594, 2874, 2874, 2874, 2874};

        readonly int[] clustersize = {512, 512, 512, 512, 512, 512, 512};

        readonly string[] volumename =
        {
            "Volume label", "Volume label", "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label"
        };

        readonly string[] volumeserial = {null, null, null, null, null, null, null};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "hfs", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new ZZZRawImage();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.ImageInfo.MediaType, testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.SectorSize, testfiles[i]);
                Filesystem fs = new AppleHFS();
                Partition wholePart = new Partition
                {
                    Name = "Whole device",
                    Length = image.ImageInfo.Sectors,
                    Size = image.ImageInfo.Sectors * image.ImageInfo.SectorSize
                };
                Assert.AreEqual(true, fs.Identify(image, wholePart), testfiles[i]);
                fs.GetInformation(image, wholePart, out string information);
                Assert.AreEqual(clusters[i], fs.XmlFSType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFSType.ClusterSize, testfiles[i]);
                Assert.AreEqual("HFS", fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSerial, testfiles[i]);
            }
        }
    }
}