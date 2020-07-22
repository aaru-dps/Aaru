// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HPFS.cs
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
    public class Hpfs
    {
        readonly string[] _testFiles =
        {
            "ecs.aif", "msos2_1.21.aif", "msos2_1.30.1.aif", "os2_1.20.aif", "os2_1.30.aif", "os2_6.307.aif",
            "os2_6.514.aif", "os2_6.617.aif", "os2_8.162.aif", "os2_9.023.aif", "winnt_3.10.aif", "winnt_3.50.aif",
            "ecs20_fstester.aif"
        };

        readonly ulong[] _sectors =
        {
            262144, 1024000, 1024000, 1024000, 1024000, 1024000, 262144, 262144, 262144, 262144, 262144, 262144, 1024000
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly long[] _clusters =
        {
            261072, 1023056, 1023056, 1023056, 1023056, 1023056, 262016, 262016, 262016, 262016, 262016, 262112, 1022112
        };

        readonly int[] _clusterSize =
        {
            512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly string[] _volumeName =
        {
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL",
            "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUMELABEL", "VOLUME LABE"
        };

        readonly string[] _volumeSerial =
        {
            "2BBBD814", "AC0DDC15", "ABEB2C15", "6C4EE015", "6C406015", "6C49B015", "2BCEB414", "2C157414", "2BF55414",
            "2BE31414", "E851CB14", "A4EDC29C", "AC096014"
        };

        readonly string[] _oemId =
        {
            "IBM 4.50", "OS2 10.1", "OS2 10.0", "OS2 10.0", "OS2 10.0", "OS2 20.0", "OS2 20.0", "OS2 20.1", "OS2 20.0",
            "OS2 20.0", "MSDOS5.0", "MSDOS5.0", "IBM 4.50"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "High Performance File System",
                                               _testFiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new HPFS();
                Assert.AreEqual(true, fs.Identify(image, partitions[0]), _testFiles[i]);
                fs.GetInformation(image, partitions[0], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual("HPFS", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
                Assert.AreEqual(_oemId[i], fs.XmlFsType.SystemIdentifier, _testFiles[i]);
            }
        }
    }
}