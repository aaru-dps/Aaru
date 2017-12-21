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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.DiscImages;
using NUnit.Framework;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class ADFS
    {
        readonly string[] testfiles =
        {
            "adfs_d.adf.lz", "adfs_e.adf.lz", "adfs_f.adf.lz", "adfs_e+.adf.lz", "adfs_f+.adf.lz", "adfs_s.adf.lz",
            "adfs_m.adf.lz", "adfs_l.adf.lz", "hdd_old.hdf.lz", "hdd_new.hdf.lz"
        };

        readonly MediaType[] mediatypes =
        {
            MediaType.ACORN_35_DS_DD, MediaType.ACORN_35_DS_DD, MediaType.ACORN_35_DS_HD, MediaType.ACORN_35_DS_DD,
            MediaType.ACORN_35_DS_HD, MediaType.ACORN_525_SS_DD_40, MediaType.ACORN_525_SS_DD_80,
            MediaType.ACORN_525_DS_DD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD
        };

        readonly ulong[] sectors = {800, 800, 1600, 800, 1600, 640, 1280, 2560, 78336, 78336};

        readonly uint[] sectorsize = {1024, 1024, 1024, 1024, 1024, 256, 256, 256, 256, 256};

        readonly bool[] bootable = {false, false, false, false, false, false, false, false, false, false};

        readonly long[] clusters = {800, 800, 1600, 800, 1600, 640, 1280, 2560, 78336, 78336};

        readonly uint[] clustersize = {1024, 1024, 1024, 1024, 1024, 256, 256, 256, 256, 256};

        readonly string[] volumename =
            {"ADFSD", "ADFSE     ", null, "ADFSE+    ", null, "$", "$", "$", "VolLablOld", null};

        readonly string[] volumeserial = {"3E48", "E13A", null, "1142", null, "F20D", "D6CA", "0CA6", "080E", null};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "adfs", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new ZZZRawImage();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.ImageInfo.MediaType, testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.SectorSize, testfiles[i]);
                Filesystem fs = new AcornADFS();
                Partition wholePart = new Partition
                {
                    Name = "Whole device",
                    Length = image.ImageInfo.Sectors,
                    Size = image.ImageInfo.Sectors * image.ImageInfo.SectorSize
                };
                Assert.AreEqual(true, fs.Identify(image, wholePart), testfiles[i]);
                fs.GetInformation(image, wholePart, out string information);
                Assert.AreEqual(bootable[i], fs.XmlFSType.Bootable, testfiles[i]);
                Assert.AreEqual(clusters[i], fs.XmlFSType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFSType.ClusterSize, testfiles[i]);
                Assert.AreEqual("Acorn Advanced Disc Filing System", fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSerial, testfiles[i]);
            }
        }
    }
}