// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CopyQM.cs
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
// Copyright © 2011-2021 Natalia Portillo
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
    public class CopyQm
    {
        readonly string[] _testFiles =
        {
            "mf2dd.cqm.lz", "mf2dd_fdformat_800.cqm.lz", "mf2dd_freedos.cqm.lz", "mf2hd_blind.cqm.lz", "mf2hd.cqm.lz",
            "mf2hd_fdformat_168.cqm.lz", "mf2hd_freedos.cqm.lz"
        };

        readonly ulong[] _sectors =
        {
            1440, 1600, 1600, 2880, 2880, 3360, 3360
        };

        readonly uint[] _sectorSize =
        {
            512, 512, 512, 512, 512, 512, 512
        };

        // TODO: Add "unknown" media types
        readonly MediaType[] _mediaTypes =
        {
            MediaType.DOS_35_DS_DD_9, MediaType.CBM_35_DD, MediaType.CBM_35_DD, MediaType.DOS_35_HD,
            MediaType.DOS_35_HD, MediaType.DMF, MediaType.DMF
        };

        readonly string[] _md5S =
        {
            "de3f85896f771b7e5bc4c9e3926d64e4", "c533488a21098a62c85f1649abda2803", "1ff7649b679ba22ff20d39ff717dbec8",
            "b4a602f67903c46eef62addb0780aa56", "b4a602f67903c46eef62addb0780aa56", "03c2af6a8ebf4bd6f530335de34ae5dd",
            "1a9f2eeb3cbeeb057b9a9a5c6e9b0cc6"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "CopyQM", _testFiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new DiscImages.CopyQm();
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