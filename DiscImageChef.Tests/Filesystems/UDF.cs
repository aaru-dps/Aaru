// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : UDF.cs
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
    public class Udf
    {
        readonly string[] testfiles =
        {
            "1.02/linux.vdi.lz", "1.02/macosx_10.11.vdi.lz", "1.50/linux.vdi.lz", "1.50/macosx_10.11.vdi.lz",
            "2.00/linux.vdi.lz", "2.00/macosx_10.11.vdi.lz", "2.01/linux.vdi.lz", "2.01/macosx_10.11.vdi.lz",
            "2.50/linux.vdi.lz", "2.50/macosx_10.11.vdi.lz", "2.60/macosx_10.11.vdi.lz", "1.50/solaris_7.vdi.lz",
            "1.50/solaris_9.vdi.lz", "2.01/netbsd_7.1.vdi.lz"
        };

        readonly ulong[] sectors =
        {
            1024000, 204800, 1024000, 409600, 1024000, 614400, 1024000, 819200, 1024000, 1024000, 1228800, 8388608,
            8388608, 8388608
        };

        readonly uint[] sectorsize = {512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512};

        readonly long[] clusters =
        {
            1024000, 204800, 1024000, 409600, 1024000, 614400, 1024000, 819200, 1024000, 1024000, 1228800, 8388608,
            8388608, 8388608
        };

        readonly int[] clustersize = {512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512};

        readonly string[] udfversion =
        {
            "UDF v1.02", "UDF v1.02", "UDF v1.50", "UDF v1.50", "UDF v2.00", "UDF v2.00", "UDF v2.01", "UDF v2.01",
            "UDF v2.50", "UDF v2.50", "UDF v2.60", "UDF v1.50", "UDF v1.50", "UDF v2.01"
        };

        readonly string[] volumename =
        {
            "Volume label", "Volume label", "Volume label", "Volume label", "Volume label", "Volume label",
            "Volume label", "Volume label", "Volume label", "Volume label", "Volume label", "*NoLabel*",
            "*NoLabel*", "anonymous"
        };

        readonly string[] volumeserial =
        {
            "595c5cfa38ce8b66LinuxUDF", "6D02A231 (Mac OS X newfs_udf) UDF Volume Set", "595c5d00c5b3405aLinuxUDF",
            "4DD0458B (Mac OS X newfs_udf) UDF Volume Set", "595c5d07f4fc8e8dLinuxUDF",
            "5D91CB4F (Mac OS X newfs_udf) UDF Volume Set", "595c5d0bee60c3bbLinuxUDF",
            "48847EB3 (Mac OS X newfs_udf) UDF Volume Set", "595c5d0e4f338552LinuxUDF",
            "709E84A1 (Mac OS X newfs_udf) UDF Volume Set", "78CE3237 (Mac OS X newfs_udf) UDF Volume Set",
            "595EB2A9", "595EB55A", "7cc94d726669d773"
        };

        readonly string[] oemid =
        {
            "*Linux UDFFS", "*Apple Mac OS X UDF FS", "*Linux UDFFS", "*Apple Mac OS X UDF FS", "*Linux UDFFS",
            "*Apple Mac OS X UDF FS", "*Linux UDFFS", "*Apple Mac OS X UDF FS", "*Linux UDFFS",
            "*Apple Mac OS X UDF FS", "*Apple Mac OS X UDF FS", "*SUN SOLARIS UDF", "*SUN SOLARIS UDF",
            "*NetBSD userland UDF"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "udf", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                IFilesystem fs = new UDF();
                Partition wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };
                Assert.AreEqual(true, fs.Identify(image, wholePart), testfiles[i]);
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(clusters[i],     fs.XmlFsType.Clusters,            testfiles[i]);
                Assert.AreEqual(clustersize[i],  fs.XmlFsType.ClusterSize,         testfiles[i]);
                Assert.AreEqual(udfversion[i],   fs.XmlFsType.Type,                testfiles[i]);
                Assert.AreEqual(volumename[i],   fs.XmlFsType.VolumeName,          testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSetIdentifier, testfiles[i]);
                Assert.AreEqual(oemid[i],        fs.XmlFsType.SystemIdentifier,    testfiles[i]);
            }
        }
    }

    [TestFixture]
    public class UdfOptical
    {
        readonly string[] testfiles =
        {
            "1.50/ecs20.iso.lz", "2.00/ecs20.iso.lz", "2.01/ecs20.iso.lz", "2.01/ecs20_cdrw.iso.lz"
        };

        readonly ulong[] sectors = {2295104, 2295104, 2295104, 295264};

        readonly uint[] sectorsize = {2048, 2048, 2048, 2048};

        readonly long[] clusters = {2295104, 2295104, 2295104, 295264};

        readonly int[] clustersize = {2048, 2048, 2048, 2048};

        readonly string[] udfversion = {"UDF v2.01", "UDF v2.01", "UDF v2.01", "UDF v2.01"};

        readonly string[] volumename = {"Volume label", "UDF5A5DEF48", "VolLabel", "UDF5A5DFF10"};

        readonly string[] volumeserial =
        {
            "Volume Set ID not specified", "Volume Set ID not specified", "VolumeSetId",
            "Volume Set ID not specified"
        };

        readonly string[] oemid = {"*ExpressUDF", "*ExpressUDF", "*ExpressUDF", "*ExpressUDF"};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "udf", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                IFilesystem fs = new UDF();
                Partition wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };
                Assert.AreEqual(true, fs.Identify(image, wholePart), testfiles[i]);
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(clusters[i],     fs.XmlFsType.Clusters,            testfiles[i]);
                Assert.AreEqual(clustersize[i],  fs.XmlFsType.ClusterSize,         testfiles[i]);
                Assert.AreEqual(udfversion[i],   fs.XmlFsType.Type,                testfiles[i]);
                Assert.AreEqual(volumename[i],   fs.XmlFsType.VolumeName,          testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSetIdentifier, testfiles[i]);
                Assert.AreEqual(oemid[i],        fs.XmlFsType.SystemIdentifier,    testfiles[i]);
            }
        }
    }
}