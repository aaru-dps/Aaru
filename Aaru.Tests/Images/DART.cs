// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DART.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru unit testing.
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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using System.IO;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filters;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class Dart
    {
        readonly string[] _testFiles =
        {
            "mf1dd_hfs_best.dart.lz", "mf1dd_hfs_fast.dart.lz", "mf1dd_mfs_best.dart.lz", "mf1dd_mfs_fast.dart.lz",
            "mf2dd_hfs_best.dart.lz", "mf2dd_hfs_fast.dart.lz", "mf2dd_mfs_best.dart.lz", "mf2dd_mfs_fast.dart.lz"
        };

        readonly ulong[] _sectors =
        {
            800, 800, 800, 800, 1600, 1600, 1600, 1600
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512, 512, 512, 512, 512, 512
        };

        readonly MediaType[] _mediaTypes =
        {
            MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS, MediaType.AppleSonySS,
            MediaType.AppleSonyDS, MediaType.AppleSonyDS, MediaType.AppleSonyDS, MediaType.AppleSonyDS
        };

        readonly string[] _md5S =
        {
            "eae3a95671d077deb702b3549a769f56", "eae3a95671d077deb702b3549a769f56", "c5d92544c3e78b7f0a9b4baaa9a64eec",
            "c5d92544c3e78b7f0a9b4baaa9a64eec", "a99744348a70b62b57bce2dec9132ced", "a99744348a70b62b57bce2dec9132ced",
            "93e71b9ecdb39d3ec9245b4f451856d4", "93e71b9ecdb39d3ec9245b4f451856d4"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "DART", _testFiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new DiscImages.Dart();
                Assert.AreEqual(true, image.Open(filter), _testFiles[i]);
                Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, _testFiles[i]);

                // How many sectors to read at once
                const uint sectorsToRead = 256;
                ulong      doneSectors   = 0;

                var ctx = new Md5Context();

                while(doneSectors < image.Info.Sectors)
                {
                    byte[] sector;

                    if(image.Info.Sectors - doneSectors >= sectorsToRead)
                    {
                        sector      =  image.ReadSectors(doneSectors, sectorsToRead);
                        doneSectors += sectorsToRead;
                    }
                    else
                    {
                        sector      =  image.ReadSectors(doneSectors, (uint)(image.Info.Sectors - doneSectors));
                        doneSectors += image.Info.Sectors - doneSectors;
                    }

                    ctx.Update(sector);
                }

                Assert.AreEqual(_md5S[i], ctx.End(), _testFiles[i]);
            }
        }
    }
}