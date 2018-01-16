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
    public class Hfs
    {
        readonly string[] testfiles =
        {
            "macos_1.1_mf2dd.img.lz", "macos_2.0_mf2dd.img.lz", "macos_6.0.7_mf2dd.img.lz", "nextstep_3.3_mf2hd.img.lz",
            "openstep_4.0_mf2hd.img.lz", "openstep_4.2_mf2hd.img.lz", "rhapsody_dr1_mf2hd.img.lz",
            "ecs20_mf2hd_fstester.img.lz"
        };

        readonly MediaType[] mediatypes =
        {
            MediaType.AppleSonyDS, MediaType.AppleSonyDS, MediaType.AppleSonyDS, MediaType.DOS_35_HD,
            MediaType.DOS_35_HD, MediaType.DOS_35_HD, MediaType.DOS_35_HD, MediaType.DOS_35_HD
        };

        readonly ulong[] sectors = {1600, 1600, 1600, 2880, 2880, 2880, 2880, 2880};

        readonly uint[] sectorsize = {512, 512, 512, 512, 512, 512, 512, 512};

        readonly long[] clusters = {1594, 1594, 1594, 2874, 2874, 2874, 2874, 2874};

        readonly int[] clustersize = {512, 512, 512, 512, 512, 512, 512, 512};

        readonly string[] volumename =
        {
            "Volume label", "Volume label", "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label", "VOLUME LABEL"
        };

        readonly string[] volumeserial = {null, null, null, null, null, null, null, null};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "hfs", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.Info.MediaType,  testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                IFilesystem fs        = new AppleHFS();
                Partition   wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };
                Assert.AreEqual(true, fs.Identify(image, wholePart), testfiles[i]);
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(clusters[i],     fs.XmlFsType.Clusters,     testfiles[i]);
                Assert.AreEqual(clustersize[i],  fs.XmlFsType.ClusterSize,  testfiles[i]);
                Assert.AreEqual("HFS",           fs.XmlFsType.Type,         testfiles[i]);
                Assert.AreEqual(volumename[i],   fs.XmlFsType.VolumeName,   testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class HfsApm
    {
        readonly string[] testfiles =
        {
            "amigaos_3.9.vdi.lz", "darwin_1.3.1.vdi.lz", "darwin_1.4.1.vdi.lz", "darwin_6.0.2.vdi.lz",
            "darwin_8.0.1.vdi.lz", "macos_1.1.vdi.lz", "macos_2.0.vdi.lz", "macos_6.0.7.vdi.lz", "macos_7.5.3.vdi.lz",
            "macos_7.5.vdi.lz", "macos_7.6.vdi.lz", "macos_8.0.vdi.lz", "macos_8.1.vdi.lz", "macos_9.0.4.vdi.lz",
            "macos_9.1.vdi.lz", "macos_9.2.1.vdi.lz", "macos_9.2.2.vdi.lz", "macosx_10.2.vdi.lz", "macosx_10.3.vdi.lz",
            "macosx_10.4.vdi.lz", "rhapsody_dr1.vdi.lz", "d2_driver.vdi.lz", "hdt_1.8.vdi.lz", "macos_4.2.vdi.lz",
            "macos_4.3.vdi.lz", "macos_6.0.2.vdi.lz", "macos_6.0.3.vdi.lz", "macos_6.0.4.vdi.lz", "macos_6.0.5.vdi.lz",
            "macos_6.0.8.vdi.lz", "macos_6.0.vdi.lz", "macos_7.0.vdi.lz", "macos_7.1.1.vdi.lz", "parted.vdi.lz",
            "silverlining_2.2.1.vdi.lz", "speedtools_3.6.vdi.lz", "vcpformatter_2.1.1.vdi.lz"
        };

        readonly ulong[] sectors =
        {
            1024128, 409600, 409600, 409600, 409600, 41820, 41820, 81648, 1024000, 1024000, 1024000, 1024000, 1024000,
            1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 409600, 51200, 51200, 41820, 41820, 54840,
            54840, 54840, 54840, 54840, 41820, 54840, 54840, 262144, 51200, 51200, 54840
        };

        readonly uint[] sectorsize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512,
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] clusters =
        {
            64003, 51189, 51189, 58502, 58502, 41788, 38950, 39991, 63954, 63990, 63954, 63954, 63954, 63922, 63922,
            63922, 63922, 63884, 63883, 63883, 58506, 50926, 50094, 38950, 38950, 38950, 38950, 7673, 38950, 38950,
            38950, 38950, 38950, 46071, 50382, 49135, 54643
        };

        readonly int[] clustersize =
        {
            8192, 4096, 4096, 3584, 3584, 512, 512, 1024, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192, 8192,
            8192, 8192, 3584, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 1024, 512, 512, 512
        };

        readonly string[] volumename =
        {
            "Volume label", "Volume label", "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label", "Volume label", "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label", "Volume label", "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label", "Volume label", "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label", "Volume label", "Volume label", "Test disk", "Volume label", "Volume label", "Volume label",
            "Volume label", "Volume label", "Untitled", "Untitled  #1", "24 MB Disk", "Volume label"
        };

        readonly string[] volumeserial =
        {
            null, null, null, null, "AAFE1382AF5AA898", null, null, null, null, null, null, null, null, null, null,
            null, null, "5A7C38B0CAF279C4", "FB49083EBD150509", "632C0B1DB46FD188", null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "hfs_apm", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AppleHFS();
                int             part       = -1;
                for(int j = 0; j          < partitions.Count; j++)
                    if(partitions[j].Type == "Apple_HFS")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],     fs.XmlFsType.Clusters,     testfiles[i]);
                Assert.AreEqual(clustersize[i],  fs.XmlFsType.ClusterSize,  testfiles[i]);
                Assert.AreEqual("HFS",           fs.XmlFsType.Type,         testfiles[i]);
                Assert.AreEqual(volumename[i],   fs.XmlFsType.VolumeName,   testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class HfsCdrom
    {
        readonly string[] testfiles =
        {
            "toast_3.5.7_hfs_from_volume.iso.lz", "toast_3.5.7_iso9660_hfs.iso.lz",
            "toast_4.1.3_hfs_from_volume.iso.lz", "toast_4.1.3_iso9660_hfs.iso.lz", "toast_3.5.7_hfs_from_files.iso.lz",
            "toast_4.1.3_hfs_from_files.iso.lz"
        };

        readonly ulong[] sectors = {942, 1880, 943, 1882, 1509, 1529};

        readonly uint[] sectorsize = {2048, 2048, 2048, 2048, 2048, 2048};

        readonly long[] clusters = {3724, 931, 931, 931, 249, 249};

        readonly int[] clustersize = {512, 2048, 2048, 2048, 12288, 12288};

        readonly string[] volumename =
            {"Disk utils", "Disk utils", "Disk utils", "Disk utils", "Disk utils", "Disk utils"};

        readonly string[] volumeserial = {null, null, null, null, null, null};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "hfs_cdrom", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AppleHFS();
                int             part       = -1;
                for(int j = 0; j          < partitions.Count; j++)
                    if(partitions[j].Type == "Apple_HFS")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],     fs.XmlFsType.Clusters,     testfiles[i]);
                Assert.AreEqual(clustersize[i],  fs.XmlFsType.ClusterSize,  testfiles[i]);
                Assert.AreEqual("HFS",           fs.XmlFsType.Type,         testfiles[i]);
                Assert.AreEqual(volumename[i],   fs.XmlFsType.VolumeName,   testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class HfsMbr
    {
        readonly string[] testfiles =
        {
            "linux.vdi.lz", "darwin_1.3.1.vdi.lz", "darwin_1.4.1.vdi.lz", "darwin_6.0.2.vdi.lz", "darwin_8.0.1.vdi.lz"
        };

        readonly ulong[] sectors = {262144, 409600, 409600, 409600, 409600};

        readonly uint[] sectorsize = {512, 512, 512, 512, 512};

        readonly long[] clusters = {65018, 51145, 51145, 58452, 58502};

        readonly int[] clustersize = {2048, 4096, 4096, 3584, 3584};

        readonly string[] volumename = {"Volume label", "Volume label", "Volume label", "Volume label", "Volume label"};

        readonly string[] volumeserial = {null, null, null, null, "81FE805D61458753"};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "hfs_mbr", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AppleHFS();
                int             part       = -1;
                for(int j = 0; j          < partitions.Count; j++)
                    if(partitions[j].Type == "0xAF")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],     fs.XmlFsType.Clusters,     testfiles[i]);
                Assert.AreEqual(clustersize[i],  fs.XmlFsType.ClusterSize,  testfiles[i]);
                Assert.AreEqual("HFS",           fs.XmlFsType.Type,         testfiles[i]);
                Assert.AreEqual(volumename[i],   fs.XmlFsType.VolumeName,   testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class HfsRdb
    {
        readonly string[] testfiles = {"amigaos_3.9.vdi.lz"};

        readonly ulong[] sectors = {1024128};

        readonly uint[] sectorsize = {512};

        readonly long[] clusters = {63752};

        readonly int[] clustersize = {8192};

        readonly string[] volumename = {"Volume label"};

        readonly string[] volumeserial = {null};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "hfs_rdb", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AppleHFS();
                int             part       = -1;
                for(int j = 0; j          < partitions.Count; j++)
                    if(partitions[j].Type == "\"RES\\86\"")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],     fs.XmlFsType.Clusters,     testfiles[i]);
                Assert.AreEqual(clustersize[i],  fs.XmlFsType.ClusterSize,  testfiles[i]);
                Assert.AreEqual("HFS",           fs.XmlFsType.Type,         testfiles[i]);
                Assert.AreEqual(volumename[i],   fs.XmlFsType.VolumeName,   testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
            }
        }
    }
}