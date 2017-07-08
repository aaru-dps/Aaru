// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : UDF.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
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
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System.IO;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;
using NUnit.Framework;
using DiscImageChef.DiscImages;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class UDF
    {
        readonly string[] testfiles = {
            "1.02/linux.vdi.lz", "1.02/macosx.vdi.lz", "1.50/linux.vdi.lz", "1.50/macosx.vdi.lz",
            "2.00/linux.vdi.lz", "2.00/macosx.vdi.lz", "2.01/linux.vdi.lz", "2.01/macosx.vdi.lz",
            "2.50/linux.vdi.lz", "2.50/macosx.vdi.lz", "2.60/macosx.vdi.lz", "1.50/solaris_7.vdi.lz",
            "1.50/solaris_9.vdi.lz", "2.01/netbsd_7.1.vdi.lz",
        };

        readonly ulong[] sectors = {
            1024000, 204800, 1024000, 409600,
            1024000, 614400, 1024000, 819200,
            1024000, 1024000, 1228800, 8388608,
            8388608, 8388608,
        };

        readonly uint[] sectorsize = {
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512,
        };

        readonly long[] clusters = {
            1024000, 204800, 1024000, 409600,
            1024000, 614400, 1024000, 819200,
            1024000, 1024000, 1228800, 8388608,
            8388608, 8388608,
        };

        readonly int[] clustersize = {
            512, 512, 512, 512,
            512, 512, 512, 512,
            512, 512, 512,512,
            512, 512,
        };

        readonly string[] udfversion = {
            "UDF v1.02", "UDF v1.02", "UDF v1.50", "UDF v1.50",
            "UDF v2.00", "UDF v2.00", "UDF v2.01", "UDF v2.01",
            "UDF v2.50", "UDF v2.50", "UDF v2.50", "UDF v1.50",
            "UDF v1.50", "UDF v2.01",
        };

        readonly string[] volumename = {
            "Volume label", "Volume label", "Volume label", "Volume label", 
            "Volume label", "Volume label", "Volume label", "Volume label", 
            "Volume label", "Volume label", "Volume label", "*NoLabel*",
            "*NoLabel*", "anonymous",
        };

        readonly string[] volumeserial = {
            "595c5cfa38ce8b66LinuxUDF", "6D02A231 (Mac OS X newfs_udf) UDF Volume Set", "595c5d00c5b3405aLinuxUDF", "4DD0458B (Mac OS X newfs_udf) UDF Volume Set",
            "595c5d07f4fc8e8dLinuxUDF", "5D91CB4F (Mac OS X newfs_udf) UDF Volume Set", "595c5d0bee60c3bbLinuxUDF", "48847EB3 (Mac OS X newfs_udf) UDF Volume Set",
            "595c5d0e4f338552LinuxUDF", "709E84A1 (Mac OS X newfs_udf) UDF Volume Set", "78CE3237 (Mac OS X newfs_udf) UDF Volume Set","595EB2A9",
            "595EB55A", "7cc94d726669d773",
        };

        readonly string[] oemid = {
            "*Linux UDFFS", "*Apple Mac OS X UDF FS", "*Linux UDFFS", "*Apple Mac OS X UDF FS",
            "*Linux UDFFS", "*Apple Mac OS X UDF FS", "*Linux UDFFS", "*Apple Mac OS X UDF FS",
            "*Linux UDFFS", "*Apple Mac OS X UDF FS", "*Apple Mac OS X UDF FS","*SUN SOLARIS UDF",
            "*SUN SOLARIS UDF", "*NetBSD userland UDF",
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "udf", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new VDI();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                Filesystem fs = new DiscImageChef.Filesystems.UDF();
                Assert.AreEqual(true, fs.Identify(image, 0, image.ImageInfo.sectors - 1), testfiles[i]);
                fs.GetInformation(image, 0, image.ImageInfo.sectors - 1, out string information);
                Assert.AreEqual(clusters[i], fs.XmlFSType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFSType.ClusterSize, testfiles[i]);
                Assert.AreEqual(udfversion[i], fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFSType.VolumeSetIdentifier, testfiles[i]);
                Assert.AreEqual(oemid[i], fs.XmlFSType.SystemIdentifier, testfiles[i]);
            }
        }
    }
}
