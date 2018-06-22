// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HPOFS.cs
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
using DiscImageChef.DiscImages;
using DiscImageChef.Filesystems;
using DiscImageChef.Filters;
using NUnit.Framework;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class Hpofs
    {
        readonly string[] testfiles = {"rid1.img.lz", "rid10.img.lz", "rid66percent.img.lz", "rid266.img.lz"};

        readonly MediaType[] mediatypes =
            {MediaType.DOS_35_HD, MediaType.DOS_35_HD, MediaType.DOS_35_HD, MediaType.DOS_35_HD};

        readonly ulong[] sectors = {2880, 2880, 2880, 2880};

        readonly uint[] sectorsize = {512, 512, 512, 512};

        readonly long[] clusters = {2880, 2880, 2880, 2880};

        readonly int[] clustersize = {512, 512, 512, 512};

        readonly string[] volumename = {"VOLUME LABEL", "VOLUME LABEL", "VOLUME LABEL", "VOLUME LABEL"};

        readonly string[] volumeserial = {"AC226814", "AC160814", "AC306C14", "ABEF2C14"};

        readonly string[] oemid = {"IBM 10.2", "IBM 10.2", "IBM 10.2", "IBM 10.2"};

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "hpofs", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new ZZZRawImage();
                Assert.AreEqual(true,          image.Open(filter),    testfiles[i]);
                Assert.AreEqual(mediatypes[i], image.Info.MediaType,  testfiles[i]);
                Assert.AreEqual(sectors[i],    image.Info.Sectors,    testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                IFilesystem fs = new HPOFS();
                Partition wholePart = new Partition
                {
                    Name   = "Whole device",
                    Length = image.Info.Sectors,
                    Size   = image.Info.Sectors * image.Info.SectorSize
                };
                Assert.AreEqual(true, fs.Identify(image, wholePart), testfiles[i]);
                fs.GetInformation(image, wholePart, out _, null);
                Assert.AreEqual(clusters[i],     fs.XmlFsType.Clusters,         testfiles[i]);
                Assert.AreEqual(clustersize[i],  fs.XmlFsType.ClusterSize,      testfiles[i]);
                Assert.AreEqual("HPOFS",         fs.XmlFsType.Type,             testfiles[i]);
                Assert.AreEqual(volumename[i],   fs.XmlFsType.VolumeName,       testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial,     testfiles[i]);
                Assert.AreEqual(oemid[i],        fs.XmlFsType.SystemIdentifier, testfiles[i]);
            }
        }
    }
}