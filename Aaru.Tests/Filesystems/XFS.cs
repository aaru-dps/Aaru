// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : XFS.cs
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
    public class XfsMbr
    {
        readonly string[] _testFiles =
        {
            "linux.aif", "linux_4.19_xfs_flashdrive.aif"
        };

        readonly ulong[] _sectors =
        {
            1048576, 1024000
        };

        readonly uint[] _sectorSize =
        {
            512, 512
        };

        readonly long[] _clusters =
        {
            130816, 127744
        };

        readonly int[] _clusterSize =
        {
            4096, 4096
        };

        readonly string[] _volumeName =
        {
            "Volume label", "DicSetter"
        };

        readonly string[] _volumeSerial =
        {
            "230075b7-9834-b44e-a257-982a058311d8", "ed6b4d35-aa66-ce4a-9d8f-c56dbc6d7c8c"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "XFS", _testFiles[i]);
                IFilter filter   = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new XFS();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x83")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testFiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testFiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testFiles[i]);
                Assert.AreEqual(_clusterSize[i], fs.XmlFsType.ClusterSize, _testFiles[i]);
                Assert.AreEqual("XFS filesystem", fs.XmlFsType.Type, _testFiles[i]);
                Assert.AreEqual(_volumeName[i], fs.XmlFsType.VolumeName, _testFiles[i]);
                Assert.AreEqual(_volumeSerial[i], fs.XmlFsType.VolumeSerial, _testFiles[i]);
            }
        }
    }
}