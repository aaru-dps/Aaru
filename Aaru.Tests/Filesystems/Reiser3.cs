// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Reiser3.cs
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
    public class Reiser3
    {
        readonly string[] _testfiles =
        {
            "linux_r3.5.aif", "linux_r3.6.aif", "linux_4.19_reiser_3.5_flashdrive.aif",
            "linux_4.19_reiser_3.6_flashdrive.aif"
        };

        readonly ulong[] _sectors =
        {
            262144, 262144, 1024000, 1024000
        };

        readonly uint[] _sectorsize =
        {
            512, 512, 512, 512
        };

        readonly long[] _clusters =
        {
            32512, 32512, 127744, 127744
        };

        readonly int[] _clustersize =
        {
            4096, 4096, 4096, 4096
        };

        readonly string[] _reiserversion =
        {
            "Reiser 3.5 filesystem", "Reiser 3.6 filesystem", "Reiser 3.5 filesystem", "Reiser 3.6 filesystem"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testfiles.Length; i++)
            {
                string location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Reiser filesystem v3",
                                               _testfiles[i]);

                IFilter filter = new ZZZNoFilter();
                filter.Open(location);
                IMediaImage image = new AaruFormat();
                Assert.AreEqual(true, image.Open(filter), _testfiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testfiles[i]);
                Assert.AreEqual(_sectorsize[i], image.Info.SectorSize, _testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new Reiser();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x83")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {_testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), _testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(_clusters[i], fs.XmlFsType.Clusters, _testfiles[i]);
                Assert.AreEqual(_clustersize[i], fs.XmlFsType.ClusterSize, _testfiles[i]);
                Assert.AreEqual(_reiserversion[i], fs.XmlFsType.Type, _testfiles[i]);
            }
        }
    }
}