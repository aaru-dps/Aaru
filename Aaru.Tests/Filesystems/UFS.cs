// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : UFS.cs
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

using System.Collections.Generic;
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
    public class Ufs
    {
        readonly string[] testfiles =
        {
            "amix_mf2dd.adf.lz", "netbsd_1.6_mf2hd.img.lz", "att_unix_svr4v2.1_dsdd.img.lz",
            "att_unix_svr4v2.1_dshd.img.lz", "att_unix_svr4v2.1_mf2dd.img.lz", "att_unix_svr4v2.1_mf2hd.img.lz",
            "solaris_2.4_mf2dd.img.lz", "solaris_2.4_mf2hd.img.lz"
        };

        readonly MediaType[] mediatypes =
        {
            MediaType.CBM_AMIGA_35_DD, MediaType.DOS_35_HD, MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD,
            MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD, MediaType.DOS_35_DS_DD_9, MediaType.DOS_35_HD
        };

        readonly ulong[] sectors =
        {
            1760, 2880, 720, 2400, 1440, 2880, 1440, 2880
        };

        readonly uint[] sectorsize =
        {
            512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] clusters =
        {
            880, 2880, 360, 1200, 720, 1440, 711, 1422
        };

        readonly int[] clustersize =
        {
            1024, 512, 1024, 1024, 1024, 1024, 1024, 1024
        };

        readonly string[] volumename =
        {
            null, null, null, null, null, null, null, null
        };

        readonly string[] volumeserial =
        {
            null, null, null, null, null, null, null, null
        };

        readonly string[] type =
        {
            "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "Filesystems", "UNIX filesystem", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.Info.MediaType, testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                IFilesystem fs = new FFSPlugin();

                var wholePart = new Partition
                {
                    Name = "Whole device", Length = image.Info.Sectors,
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
    public class UfsApm
    {
        readonly string[] testfiles =
        {
            "ffs43/darwin_1.3.1.vdi.lz", "ffs43/darwin_1.4.1.vdi.lz", "ffs43/darwin_6.0.2.vdi.lz",
            "ffs43/darwin_8.0.1.vdi.lz", "ufs1/darwin_1.3.1.vdi.lz", "ufs1/darwin_1.4.1.vdi.lz",
            "ufs1/darwin_6.0.2.vdi.lz", "ufs1/darwin_8.0.1.vdi.lz", "ufs1/macosx_10.2.vdi.lz",
            "ufs1/macosx_10.3.vdi.lz", "ufs1/macosx_10.4.vdi.lz"
        };

        readonly ulong[] sectors =
        {
            1024000, 1024000, 1024000, 1024000, 204800, 204800, 204800, 204800, 2097152, 2097152, 2097152
        };

        readonly uint[] sectorsize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] clusters =
        {
            511488, 511488, 511488, 511488, 102368, 102368, 102368, 102368, 1047660, 1038952, 1038952
        };

        readonly int[] clustersize =
        {
            1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024
        };

        readonly string[] volumename =
        {
            null, null, null, null, null, null, null, null, null, null, null
        };

        readonly string[] volumeserial =
        {
            null, null, null, null, null, null, null, null, null, null, null
        };

        readonly string[] type =
        {
            "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "Filesystems", "UNIX filesystem (APM)",
                                               testfiles[i]);

                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new FFSPlugin();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "Apple_UFS")
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

    [TestFixture]
    public class UfsMbr
    {
        readonly string[] testfiles =
        {
            "ufs1/linux.vdi.lz", "ufs2/linux.vdi.lz", "ffs43/darwin_1.3.1.vdi.lz", "ffs43/darwin_1.4.1.vdi.lz",
            "ffs43/darwin_6.0.2.vdi.lz", "ffs43/darwin_8.0.1.vdi.lz", "ffs43/dflybsd_1.2.0.vdi.lz",
            "ffs43/dflybsd_3.6.1.vdi.lz", "ffs43/dflybsd_4.0.5.vdi.lz", "ffs43/netbsd_1.6.vdi.lz",
            "ffs43/netbsd_7.1.vdi.lz", "ufs1/darwin_1.3.1.vdi.lz", "ufs1/darwin_1.4.1.vdi.lz",
            "ufs1/darwin_6.0.2.vdi.lz", "ufs1/darwin_8.0.1.vdi.lz", "ufs1/dflybsd_1.2.0.vdi.lz",
            "ufs1/dflybsd_3.6.1.vdi.lz", "ufs1/dflybsd_4.0.5.vdi.lz", "ufs1/freebsd_6.1.vdi.lz",
            "ufs1/freebsd_7.0.vdi.lz", "ufs1/freebsd_8.2.vdi.lz", "ufs1/netbsd_1.6.vdi.lz", "ufs1/netbsd_7.1.vdi.lz",
            "ufs1/solaris_7.vdi.lz", "ufs1/solaris_9.vdi.lz", "ufs2/freebsd_6.1.vdi.lz", "ufs2/freebsd_7.0.vdi.lz",
            "ufs2/freebsd_8.2.vdi.lz", "ufs2/netbsd_7.1.vdi.lz"
        };

        readonly ulong[] sectors =
        {
            262144, 262144, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 1024000, 409600, 204800,
            204800, 204800, 204800, 2097152, 2097152, 2097152, 2097152, 8388608, 8388608, 2097152, 1024000, 2097152,
            2097152, 16777216, 16777216, 16777216, 2097152
        };

        readonly uint[] sectorsize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512,
            512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] clusters =
        {
            65024, 65024, 511024, 511024, 511024, 511488, 511950, 255470, 255470, 511992, 204768, 102280, 102280,
            102280, 102368, 1048500, 523758, 523758, 262138, 1048231, 2096462, 524284, 511968, 1038240, 1046808,
            2096472, 2096472, 4192945, 524272
        };

        readonly int[] clustersize =
        {
            2048, 2048, 1024, 1024, 1024, 1024, 1024, 2048, 2048, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 2048, 2048,
            4096, 4096, 2048, 2048, 1024, 1024, 1024, 4096, 4096, 2048, 2048
        };

        readonly string[] volumename =
        {
            null, "VolumeLabel", null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, "VolumeLabel", "VolumeLabel", "VolumeLabel", ""
        };

        readonly string[] volumeserial =
        {
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null, null, null, null, null, null, null, null, null, null
        };

        readonly string[] type =
        {
            "UFS", "UFS2", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS",
            "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS2", "UFS2", "UFS2", "UFS2"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "Filesystems", "UNIX filesystem (MBR)",
                                               testfiles[i]);

                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new FFSPlugin();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x63"                    ||
                       partitions[j].Type == "0xA8"                    ||
                       partitions[j].Type == "0xA5"                    ||
                       partitions[j].Type == "0xA9"                    ||
                       partitions[j].Type == "0x82"                    ||
                       partitions[j].Type == "0x83"                    ||
                       partitions[j].Type == "4.2BSD Fast File System" ||
                       partitions[j].Type == "Sun boot")
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

    [TestFixture]
    public class UfsNeXt
    {
        readonly string[] testfiles =
        {
            "nextstep_3.3.vdi.lz", "openstep_4.0.vdi.lz", "openstep_4.2.vdi.lz", "rhapsody_dr1.vdi.lz",
            "rhapsody_dr2.vdi.lz"
        };

        readonly ulong[] sectors =
        {
            409600, 409600, 409600, 409600, 409600
        };

        readonly uint[] sectorsize =
        {
            512, 512, 512, 512, 512
        };

        readonly long[] clusters =
        {
            204640, 204640, 204640, 204640, 204464
        };

        readonly int[] clustersize =
        {
            1024, 1024, 1024, 1024, 1024
        };

        readonly string[] volumename =
        {
            null, null, null, null, null
        };

        readonly string[] volumeserial =
        {
            null, null, null, null, null
        };

        readonly string[] type =
        {
            "UFS", "UFS", "UFS", "UFS", "UFS"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "Filesystems", "UNIX filesystem (NeXT)",
                                               testfiles[i]);

                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new FFSPlugin();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "4.3BSD" ||
                       partitions[j].Type == "4.4BSD")
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

    [TestFixture]
    public class UfsNeXtFloppy
    {
        readonly string[] testfiles =
        {
            "nextstep_3.3_mf2dd.img.lz", "nextstep_3.3_mf2hd.img.lz", "openstep_4.0_mf2dd.img.lz",
            "openstep_4.0_mf2hd.img.lz", "openstep_4.2_mf2dd.img.lz", "openstep_4.2_mf2hd.img.lz",
            "rhapsody_dr1_mf2dd.img.lz", "rhapsody_dr1_mf2hd.img.lz", "rhapsody_dr2_mf2dd.img.lz",
            "rhapsody_dr2_mf2hd.img.lz"
        };

        readonly ulong[] sectors =
        {
            1440, 2880, 1440, 2880, 1440, 2880, 1440, 2880, 1440, 2880
        };

        readonly uint[] sectorsize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] clusters =
        {
            624, 1344, 624, 1344, 624, 1344, 624, 1344, 624, 1344
        };

        readonly int[] clustersize =
        {
            1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024, 1024
        };

        readonly string[] volumename =
        {
            null, null, null, null, null, null, null, null, null, null
        };

        readonly string[] volumeserial =
        {
            null, null, null, null, null, null, null, null, null, null
        };

        readonly string[] type =
        {
            "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS", "UFS"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "Filesystems", "UNIX filesystem (NeXT)",
                                               testfiles[i]);

                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new FFSPlugin();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "4.3BSD" ||
                       partitions[j].Type == "4.4BSD")
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

    [TestFixture]
    public class UfsRdb
    {
        readonly string[] testfiles =
        {
            "amix.vdi.lz"
        };

        readonly ulong[] sectors =
        {
            1024128
        };

        readonly uint[] sectorsize =
        {
            512
        };

        readonly long[] clusters =
        {
            511424
        };

        readonly int[] clustersize =
        {
            1024
        };

        readonly string[] volumename =
        {
            null
        };

        readonly string[] volumeserial =
        {
            null
        };

        readonly string[] type =
        {
            "UFS"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "Filesystems", "UNIX filesystem (RDB)",
                                               testfiles[i]);

                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new FFSPlugin();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "\"UNI\\2\"")
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

    [TestFixture]
    public class UfsSuni86
    {
        readonly string[] testfiles =
        {
            "solaris_7.vdi.lz"
        };

        readonly ulong[] sectors =
        {
            4194304
        };

        readonly uint[] sectorsize =
        {
            512
        };

        readonly long[] clusters =
        {
            2063376
        };

        readonly int[] clustersize =
        {
            1024
        };

        readonly string[] volumename =
        {
            null
        };

        readonly string[] volumeserial =
        {
            null
        };

        readonly string[] type =
        {
            "UFS"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "Filesystems", "UNIX filesystem (SunOS x86)",
                                               testfiles[i]);

                IFilter filter = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new FFSPlugin();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "Replacement sectors")
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