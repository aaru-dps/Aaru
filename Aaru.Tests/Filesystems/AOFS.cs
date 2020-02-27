// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AOFS.cs
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
    public class Aofs
    {
        readonly string[] testfiles = {"amigaos_3.9.adf.lz", "amigaos_3.9_intl.adf.lz"};

        readonly MediaType[] mediatypes = {MediaType.CBM_AMIGA_35_DD, MediaType.CBM_AMIGA_35_DD};

        readonly ulong[] sectors = {1760, 1760};

        readonly uint[] sectorsize = {512, 512};

        readonly long[] clusters = {1760, 1760};

        readonly int[] clustersize = {512, 512};

        readonly string[] volumename = {"Volume label", "Volume label"};

        readonly string[] volumeserial = {"A5D9FE71", "A5D9F14F"};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "aofs", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.Info.MediaType,  testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                IFilesystem fs = new AmigaDOSPlugin();
                Partition wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };
                Assert.AreEqual(true, fs.Identify(image, wholePart), testfiles[i]);
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(clusters[i],     fs.XmlFsType.Clusters,     testfiles[i]);
                Assert.AreEqual(clustersize[i],  fs.XmlFsType.ClusterSize,  testfiles[i]);
                Assert.AreEqual("Amiga OFS",     fs.XmlFsType.Type,         testfiles[i]);
                Assert.AreEqual(volumename[i],   fs.XmlFsType.VolumeName,   testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class AofsMbr
    {
        readonly string[] testfiles = {"aros.vdi.lz", "aros_intl.vdi.lz"};

        readonly ulong[] sectors = {409600, 409600};

        readonly uint[] sectorsize = {512, 512};

        readonly long[] clusters = {408240, 408240};

        readonly int[] clustersize = {512, 512};

        readonly string[] volumename = {"Volume label", "Volume label"};

        readonly string[] volumeserial = {"A582C90C", "A582CE0D"};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "aofs_mbr", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AmigaDOSPlugin();
                int             part       = -1;
                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x2C")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],     fs.XmlFsType.Clusters,     testfiles[i]);
                Assert.AreEqual(clustersize[i],  fs.XmlFsType.ClusterSize,  testfiles[i]);
                Assert.AreEqual("Amiga OFS",     fs.XmlFsType.Type,         testfiles[i]);
                Assert.AreEqual(volumename[i],   fs.XmlFsType.VolumeName,   testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class AofsMbrRdb
    {
        readonly string[] testfiles = {"aros.vdi.lz", "aros_intl.vdi.lz"};

        readonly ulong[] sectors = {409600, 409600};

        readonly uint[] sectorsize = {512, 512};

        readonly long[] clusters = {406224, 406224};

        readonly int[] clustersize = {512, 512};

        readonly string[] volumename = {"Volume label", "Volume label"};

        readonly string[] volumeserial = {"A5833C5B", "A5833085"};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "aofs_mbr_rdb", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AmigaDOSPlugin();
                int             part       = -1;
                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "\"DOS\\0\"")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],     fs.XmlFsType.Clusters,     testfiles[i]);
                Assert.AreEqual(clustersize[i],  fs.XmlFsType.ClusterSize,  testfiles[i]);
                Assert.AreEqual("Amiga OFS",     fs.XmlFsType.Type,         testfiles[i]);
                Assert.AreEqual(volumename[i],   fs.XmlFsType.VolumeName,   testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class AofsRdb
    {
        readonly string[] testfiles =
        {
            "amigaos_3.9.vdi.lz", "amigaos_3.9_intl.vdi.lz", "aros.vdi.lz", "aros_intl.vdi.lz"
        };

        readonly ulong[] sectors = {1024128, 1024128, 409600, 409600};

        readonly uint[] sectorsize = {512, 512, 512, 512};

        readonly long[] clusters = {510032, 510032, 407232, 407232};

        readonly int[] clustersize = {1024, 1024, 512, 512};

        readonly string[] volumename = {"Volume label", "Volume label", "Volume label", "Volume label"};

        readonly string[] volumeserial = {"A56D13BB", "A56D0415", "A582F3A0", "A5830B06"};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "aofs_rdb", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new AmigaDOSPlugin();
                int             part       = -1;
                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "\"DOS\\0\"" || partitions[j].Type == "\"DOS\\2\"" ||
                       partitions[j].Type == "\"DOS\\4\"")
                    {
                        part = j;
                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i],     fs.XmlFsType.Clusters,     testfiles[i]);
                Assert.AreEqual(clustersize[i],  fs.XmlFsType.ClusterSize,  testfiles[i]);
                Assert.AreEqual("Amiga OFS",     fs.XmlFsType.Type,         testfiles[i]);
                Assert.AreEqual(volumename[i],   fs.XmlFsType.VolumeName,   testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
            }
        }
    }
}