// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : ADFS.cs
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
using System;
using NUnit.Framework;
using System.IO;
using DiscImageChef.Filters;
using DiscImageChef.ImagePlugins;
using DiscImageChef.CommonTypes;
using DiscImageChef.Filesystems;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class ADFS
    {
        readonly string[] testfiles = {
            "adfs_d.adf.lz",
            "adfs_e.adf.lz",
            "adfs_f.adf.lz",
            "adfs_e+.adf.lz",
            "adfs_f+.adf.lz",
            "adfs_s.adf.lz",
            "adfs_m.adf.lz",
            "adfs_l.adf.lz"
        };

        readonly MediaType[] mediatypes = {
            MediaType.ACORN_35_DS_DD,
            MediaType.ACORN_35_DS_DD,
            MediaType.ACORN_35_DS_HD,
            MediaType.ACORN_35_DS_DD,
            MediaType.ACORN_35_DS_HD,
            MediaType.ACORN_525_SS_DD_40,
            MediaType.ACORN_525_SS_DD_80,
            MediaType.ACORN_525_DS_DD
        };

        readonly ulong[] sectors = { 1600, 1600, 1600, 1600, 1600, 640, 1280, 2560 };

        readonly uint[] sectorsize = { 512, 512, 1024, 512, 1024, 256, 256, 256 };

        readonly bool[] bootable = { false, false, false, false, false, false, false, false };

        readonly long[] clusters = { 1600, 1600, 1600, 1600, 1600, 640, 1280, 2560 };

        readonly int[] clustersize = { 512, 512, 1024, 512, 1024, 256, 256, 256 };

        readonly string[] volumename = { "ADFSD", "ADFSE", "", "ADFSE+", "", "", "", "" };

        readonly string[] volumeserial = { "0", "0", "0", "0", "0", "0", "0", "0" };

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
                Assert.AreEqual(mediatypes[i], image.ImageInfo.mediaType, testfiles[i]);
                Assert.AreEqual(sectors[i], image.ImageInfo.sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.ImageInfo.sectorSize, testfiles[i]);
                Filesystem fs = new AcornADFS();
                Assert.AreEqual(true, fs.Identify(image, 0, image.ImageInfo.sectors - 1), testfiles[i]);
                fs.GetInformation(image, 0, image.ImageInfo.sectors - 1, out string information);
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
