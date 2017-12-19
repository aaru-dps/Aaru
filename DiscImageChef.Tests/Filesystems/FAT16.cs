// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : FAT16.cs
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
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;
using NUnit.Framework;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class FAT16
    {
        readonly string[] testfiles = {
            // MS-DOS 3.30A
            "msdos_3.30A_mf2ed.img.lz",
            // MS-DOS 3.31
            "msdos_3.31_mf2ed.img.lz",
        };

        readonly MediaType[] mediatypes = {
            // MS-DOS 3.30A
            MediaType.DOS_35_ED,
            // MS-DOS 3.31
            MediaType.DOS_35_ED,
        };

        readonly ulong[] sectors = {
            // MS-DOS 3.30A
            5760,
            // MS-DOS 3.31
            5760,
        };

        readonly uint[] sectorsize = {
            // MS-DOS 3.30A
            512,
            // MS-DOS 3.31
            512,
        };

        readonly long[] clusters = {
            // MS-DOS 3.30A
            5760,
            // MS-DOS 3.31
            5760,
        };

        readonly int[] clustersize = {
            // MS-DOS 3.30A
            512,
            // MS-DOS 3.31
            512,
        };

        readonly string[] volumename = {
            // MS-DOS 3.30A
            null,
            // MS-DOS 3.31
            null,
        };

        readonly string[] volumeserial = {
            // MS-DOS 3.30A
            null,
            // MS-DOS 3.31
            null,
        };

        readonly string[] oemid = {
            // MS-DOS 3.30A
            "MSDOS3.3",
            // MS-DOS 3.31
            "IBM  3.3",
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "fat16", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new ZZZRawImage();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.ImageInfo.mediaType, testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                Filesystem fs = new FAT();
                Partition wholePart = new Partition
                {
                    Name = "Whole device",
                    Length = image.ImageInfo.sectors,
                    Size = image.ImageInfo.sectors * image.ImageInfo.sectorSize
                };
                Assert.AreEqual(true, fs.Identify(image, wholePart), testfiles[i]);
                fs.GetInformation(image, wholePart, out string information);
                Assert.AreEqual(clusters[i], fs.XmlFSType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFSType.ClusterSize, testfiles[i]);
                Assert.AreEqual("FAT16", fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSerial, testfiles[i]);
                Assert.AreEqual(oemid[i], fs.XmlFSType.SystemIdentifier, testfiles[i]);
            }
        }
    }
}
