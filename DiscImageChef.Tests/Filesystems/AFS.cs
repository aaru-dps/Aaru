// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AFS.cs
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

using System.Collections.Generic;
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using NUnit.Framework;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class Afs
    {
        readonly string[] testfiles =
        {
            "scoopenserver_5.0.7hw_dmf.img.lz", "scoopenserver_5.0.7hw_dshd.img.lz",
            "scoopenserver_5.0.7hw_mf2dd.img.lz", "scoopenserver_5.0.7hw_mf2ed.img.lz",
            "scoopenserver_5.0.7hw_mf2hd.img.lz"
        };

        readonly MediaType[] mediatypes =
            {MediaType.DMF, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_ED, MediaType.DOS_35_HD};

        readonly ulong[] sectors = {3360, 2400, 1440, 5760, 2880};

        readonly uint[] sectorsize = {512, 512, 512, 512, 512};

        readonly long[] clusters = {1680, 1200, 720, 2880, 1440};

        readonly int[] clustersize = {1024, 1024, 1024, 1024, 1024};

        readonly string[] volumename = {"", "", "", "", ""};

        readonly string[] volumeserial = {null, null, null, null, null};

        readonly string[] type =
        {
            "Acer Fast Filesystem", "Acer Fast Filesystem", "Acer Fast Filesystem", "Acer Fast Filesystem",
            "Acer Fast Filesystem"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "afs", testfiles[i]);
                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.Info.MediaType, testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                IFilesystem fs = new SysVfs();
                Partition wholePart = new Partition
                {
                    Name = "Whole device",
                    Length = image.Info.Sectors,
                    Size = image.Info.Sectors * image.Info.SectorSize
                };
                Assert.AreEqual(true, fs.Identify(image, wholePart), testfiles[i]);
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(clusters[i], fs.XmlFsType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFsType.ClusterSize, testfiles[i]);
                Assert.AreEqual(type[i], fs.XmlFsType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFsType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class AfsMbr
    {
        readonly string[] testfiles = {"scoopenserver_5.0.7hw.vdi.lz"};

        readonly ulong[] sectors = {1024000};

        readonly uint[] sectorsize = {512};

        readonly long[] clusters = {510048};

        readonly int[] clustersize = {1024};

        readonly string[] volumename = {"Volume label"};

        readonly string[] volumeserial = {null, null};

        readonly string[] type = {"Acer Fast Filesystem"};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "afs_mbr", testfiles[i]);
                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem fs = new SysVfs();
                int part = -1;
                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "XENIX")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i], fs.XmlFsType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFsType.ClusterSize, testfiles[i]);
                Assert.AreEqual(type[i], fs.XmlFsType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFsType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
            }
        }
    }
}