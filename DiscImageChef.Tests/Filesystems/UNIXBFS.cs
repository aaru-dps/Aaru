// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : unixbfs.cs
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
using System.Collections.Generic;
using System.IO;
using DiscImageChef.CommonTypes;
using DiscImageChef.DiscImages;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;
using DiscImageChef.PartPlugins;
using NUnit.Framework;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class unixbfs
    {
        readonly string[] testfiles = {
            "amix_mf2dd.adf.lz","att_unix_svr4v2.1_dsdd.img.lz","att_unix_svr4v2.1_dshd.img.lz","att_unix_svr4v2.1_mf2dd.img.lz",
            "att_unix_svr4v2.1_mf2hd.img.lz",
        };

        readonly MediaType[] mediatypes = {
            MediaType.CBM_AMIGA_35_DD, MediaType.DOS_525_DS_DD_9, MediaType.DOS_525_HD, MediaType.DOS_35_DS_DD_9,
            MediaType.DOS_35_HD,
        };

        readonly ulong[] sectors = {
            1760, 720, 2400, 1440,
            2880,
        };

        readonly uint[] sectorsize = {
            512, 512, 512, 512,
            512,
        };

        readonly long[] clusters = {
            1760, 720, 2400, 1440,
            2880,
        };

        readonly int[] clustersize = {
            512, 512, 512, 512,
            512,
        };

        readonly string[] volumename = {
            "Label",null,null,null,
            null,
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TestFilesRoot, "filesystems", "unixbfs", testfiles[i]);
                Filter filter = new LZip();
                filter.Open(location);
                ImagePlugin image = new ZZZRawImage();
                Assert.AreEqual(true, image.OpenImage(filter), testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.ImageInfo.mediaType, testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                Filesystem fs = new DiscImageChef.Filesystems.BFS();
                Assert.AreEqual(true, fs.Identify(image, 0, image.ImageInfo.sectors - 1), testfiles[i]);
                fs.GetInformation(image, 0, image.ImageInfo.sectors - 1, out string information);
                Assert.AreEqual(clusters[i], fs.XmlFSType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFSType.ClusterSize, testfiles[i]);
                Assert.AreEqual("BFS", fs.XmlFSType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFSType.VolumeName, testfiles[i]);
            }
        }
    }
}
