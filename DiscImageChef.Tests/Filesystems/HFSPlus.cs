// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HFSPlus.cs
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
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.DiscImages;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using NUnit.Framework;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class HfsPlusApm
    {
        // Missing Darwin 1.4.1
        readonly string[] testfiles =
        {
            "macosx_10.11.vdi.lz", "macosx_10.11_journal.vdi.lz", "darwin_1.3.1.vdi.lz",
            "darwin_1.3.1_wrapped.vdi.lz", "darwin_1.4.1_wrapped.vdi.lz", "darwin_6.0.2.vdi.lz",
            "darwin_6.0.2_wrapped.vdi.lz", "darwin_8.0.1_journal.vdi.lz", "darwin_8.0.1.vdi.lz",
            "darwin_8.0.1_wrapped.vdi.lz", "macos_8.1.vdi.lz", "macos_9.0.4.vdi.lz", "macos_9.1.vdi.lz",
            "macos_9.2.1.vdi.lz", "macos_9.2.2.vdi.lz", "macosx_10.2.vdi.lz", "macosx_10.3_journal.vdi.lz",
            "macosx_10.3.vdi.lz", "macosx_10.4_journal.vdi.lz", "macosx_10.4.vdi.lz"
        };

        readonly ulong[] sectors =
        {
            409600, 614400, 819200, 614400, 614400, 819200, 614400, 1228800, 819200, 614400, 4194304, 4194304,
            4194304, 4194304, 4194304, 4194304, 2097152, 4194304, 2097152, 4194304
        };

        readonly uint[] sectorsize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] clusters =
        {
            51190, 76790, 102392, 76774, 76774, 102392, 76774, 153592, 102392, 76774, 524152, 524088, 524088,
            524088, 524088, 524008, 261884, 491240, 261884, 491240
        };

        readonly int[] clustersize =
        {
            4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096,
            4096, 4096, 4096
        };

        readonly string[] volumename =
        {
            null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
            null, null, null
        };

        readonly string[] volumeserial =
        {
            "FA94762D086A18A9", "33D4A309C8E7BD10", null, null, null, null, null, "4D5140EB8F14A385",
            "0D592249833E2DC4", "AA616146576BD9BC", null, null, null, null, null, "EFA132FFFAC1ADA6",
            "009D570FFCF8F20B", "17F6F33AB313EE32", "AD5690C093F66FCF", "A7D63854DF76DDE6"
        };

        readonly string[] oemid =
        {
            "10.0", "HFSJ", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "8.10", "8.10", "8.10",
            "8.10", "8.10", "10.0", "HFSJ", "10.0", "HFSJ", "10.0"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "hfsplus_apm", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AppleHFSPlus();
                int             part       = -1;
                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "Apple_HFS")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],     fs.XmlFsType.Clusters,         testfiles[i]);
                Assert.AreEqual(clustersize[i],  fs.XmlFsType.ClusterSize,      testfiles[i]);
                Assert.AreEqual("HFS+",          fs.XmlFsType.Type,             testfiles[i]);
                Assert.AreEqual(volumename[i],   fs.XmlFsType.VolumeName,       testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial,     testfiles[i]);
                Assert.AreEqual(oemid[i],        fs.XmlFsType.SystemIdentifier, testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class HfsPlusGpt
    {
        readonly string[] testfiles = {"macosx_10.11.vdi.lz", "macosx_10.11_journal.vdi.lz"};

        readonly ulong[] sectors = {409600, 614400};

        readonly uint[] sectorsize = {512, 512};

        readonly long[] clusters = {51190, 76790};

        readonly int[] clustersize = {4096, 4096};

        readonly string[] volumename = {null, null};

        readonly string[] volumeserial = {"D8C68470046E67BE", "FD3CB598F3C6294A"};

        readonly string[] oemid = {"10.0", "HFSJ"};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "hfsplus_gpt", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AppleHFSPlus();
                int             part       = -1;
                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "Apple HFS")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],     fs.XmlFsType.Clusters,         testfiles[i]);
                Assert.AreEqual(clustersize[i],  fs.XmlFsType.ClusterSize,      testfiles[i]);
                Assert.AreEqual("HFS+",          fs.XmlFsType.Type,             testfiles[i]);
                Assert.AreEqual(volumename[i],   fs.XmlFsType.VolumeName,       testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial,     testfiles[i]);
                Assert.AreEqual(oemid[i],        fs.XmlFsType.SystemIdentifier, testfiles[i]);
            }
        }
    }

    // Mising Darwin 6.0.2 wrapped
    [TestFixture]
    public class HfsPlusMbr
    {
        readonly string[] testfiles =
        {
            "macosx_10.11.vdi.lz", "macosx_10.11_journal.vdi.lz", "linux.vdi.lz", "linux_journal.vdi.lz",
            "darwin_1.3.1.vdi.lz", "darwin_1.3.1_wrapped.vdi.lz", "darwin_1.4.1.vdi.lz",
            "darwin_1.4.1_wrapped.vdi.lz", "darwin_6.0.2.vdi.lz", "darwin_8.0.1_journal.vdi.lz",
            "darwin_8.0.1.vdi.lz", "darwin_8.0.1_wrapped.vdi.lz", "linux_4.19_hfs+_flashdrive.vdi.lz"
        };

        readonly ulong[] sectors =
        {
            303104, 352256, 262144, 262144, 819200, 614400, 819200, 614400, 819200, 1228800, 819200, 614400, 1024000
        };

        readonly uint[] sectorsize = {512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512};

        readonly long[] clusters =
        {
            37878, 44021, 32512, 32512, 102178, 76708, 102178, 76708, 102178, 153592, 102392, 76774, 127744
        };

        readonly int[] clustersize = {4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096, 4096};

        readonly string[] volumename = {null, null, null, null, null, null, null, null, null, null, null, null, null};

        readonly string[] volumeserial =
        {
            "C84F550907D13F50", "016599F88029F73D", null, null, null, null, null, null, null, "F92964F9B3F64ABB",
            "A8FAC484A0A2B177", "D5D5BF1346AD2B8D", "B9BAC6856878A404"
        };

        readonly string[] oemid =
        {
            "10.0", "HFSJ", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "10.0", "H+Lx"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "hfsplus_mbr", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AppleHFSPlus();
                int             part       = -1;
                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0xAF")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],     fs.XmlFsType.Clusters,         testfiles[i]);
                Assert.AreEqual(clustersize[i],  fs.XmlFsType.ClusterSize,      testfiles[i]);
                Assert.AreEqual("HFS+",          fs.XmlFsType.Type,             testfiles[i]);
                Assert.AreEqual(volumename[i],   fs.XmlFsType.VolumeName,       testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial,     testfiles[i]);
                Assert.AreEqual(oemid[i],        fs.XmlFsType.SystemIdentifier, testfiles[i]);
            }
        }
    }
}