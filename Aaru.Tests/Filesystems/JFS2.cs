// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : JFS2.cs
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
    public class Jfs2
    {
        readonly string[] testfiles =
        {
            "linux.vdi.lz", "linux_caseinsensitive.vdi.lz", "ecs20_fstester.vdi.lz", "linux_4.19_jfs_flashdrive.vdi.lz",
            "linux_4.19_jfs_os2_flashdrive.vdi.lz"
        };

        readonly ulong[] sectors =
        {
            262144, 262144, 1024000, 1024000, 1024000
        };

        readonly uint[] sectorsize =
        {
            512, 512, 512, 512, 512
        };

        readonly long[] clusters =
        {
            257632, 257632, 1017512, 1017416, 1017416
        };

        readonly int[] clustersize =
        {
            4096, 4096, 4096, 4096, 4096
        };

        readonly string[] volumename =
        {
            "Volume labe", "Volume labe", "Volume labe", "DicSetter", "DicSetter"
        };

        readonly string[] volumeserial =
        {
            "8033b783-0cd1-1645-8ecc-f8f113ad6a47", "d6cd91e9-3899-7e40-8468-baab688ee2e2",
            "f4077ce9-0000-0000-0000-000000007c10", "91746c77-eb51-7441-85e2-902c925969f8",
            "08fc8e22-0201-894e-89c9-31ec3f546203"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < testfiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TestFilesRoot, "filesystems", "jfs2", testfiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new Vdi();
                Assert.AreEqual(true, image.Open(filter), testfiles[i]);
                Assert.AreEqual(sectors[i], image.Info.Sectors, testfiles[i]);
                Assert.AreEqual(sectorsize[i], image.Info.SectorSize, testfiles[i]);
                List<Partition> partitions = Core.Partitions.GetAll(image);
                IFilesystem     fs         = new JFS();
                int             part       = -1;

                for(int j = 0; j < partitions.Count; j++)
                    if(partitions[j].Type == "0x83" ||
                       partitions[j].Type == "0x07")
                    {
                        part = j;

                        break;
                    }

                Assert.AreNotEqual(-1, part, $"Partition not found on {testfiles[i]}");
                Assert.AreEqual(true, fs.Identify(image, partitions[part]), testfiles[i]);
                fs.GetInformation(image, partitions[part], out _, null);
                Assert.AreEqual(clusters[i], fs.XmlFsType.Clusters, testfiles[i]);
                Assert.AreEqual(clustersize[i], fs.XmlFsType.ClusterSize, testfiles[i]);
                Assert.AreEqual("JFS filesystem", fs.XmlFsType.Type, testfiles[i]);
                Assert.AreEqual(volumename[i], fs.XmlFsType.VolumeName, testfiles[i]);
                Assert.AreEqual(volumeserial[i], fs.XmlFsType.VolumeSerial, testfiles[i]);
            }
        }
    }
}