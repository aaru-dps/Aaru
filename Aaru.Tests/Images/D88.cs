// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : D88.cs
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

using System.IO;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.Filters;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class D88
    {
        readonly string[] _testFiles =
        {
            "1942 (1987)(ASCII)(JP).d77.lz", "'Ashe (1988)(Quasar)(Disk 4 of 4)(User Disk).d88.lz",
            "Crimsin (1988)(Xtalsoft)(Disk 3 of 3).d88.lz", "Dragon Slayer (1986)(Falcom - Login)(JP).d88.lz",
            "D-Side - Lagrange L-2 Part II (1986)(Compaq)(JP).d88.lz",
            "File Master FM, The v1.01 (1986)(Kyoto Media)(JP).d77.lz", "Gandhara (1987)(Enix)(JP).d88.lz",
            "Might & Magic (198x)(Star Craft)(Disk 1 of 3)(Disk A).d88.lz", "msdos33d_md2dd.d88.lz",
            "msdos33d_md2hd.d88.lz", "msdos50_epson_md2dd.d88.lz", "msdos50_epson_md2hd.d88.lz", "msdos50_md2dd.d88.lz",
            "msdos50_md2hd.d88.lz", "msdos62_md2dd.d88.lz", "msdos62_md2hd.d88.lz",
            "R-Type (1988)(Irem)(Disk 1 of 2).d88.lz", "Towns System Software v1.1L30 (1992)(Fujitsu)(JP).d88.lz",
            "Visual Instrument Player (198x)(Kamiya)(JP)(Disk 1 of 2).d88.lz"
        };

        readonly ulong[] _sectors =
        {
            1280, 1280, 1280, 411, 1440, 1280, 1280, 4033, 1440, 1232, 1440, 1232, 1440, 1232, 1440, 1232, 1284, 1232,
            1280
        };

        readonly uint[] _sectorSize =
        {
            256, 256, 256, 256, 256, 256, 256, 128, 512, 1024, 512, 1024, 512, 1024, 512, 1024, 1024, 1024, 256
        };

        // TODO: Add "unknown" media types
        readonly MediaType[] _mediaTypes =
        {
            MediaType.NEC_525_SS, MediaType.NEC_525_SS, MediaType.NEC_525_SS, MediaType.Unknown, MediaType.Unknown,
            MediaType.NEC_525_SS, MediaType.NEC_525_SS, MediaType.Unknown, MediaType.Unknown, MediaType.NEC_525_HD,
            MediaType.Unknown, MediaType.NEC_525_HD, MediaType.Unknown, MediaType.NEC_525_HD, MediaType.Unknown,
            MediaType.NEC_525_HD, MediaType.Unknown, MediaType.NEC_525_HD, MediaType.NEC_525_SS
        };

        readonly string[] _md5S =
        {
            "a4103c39cd7fd9fc3de8418dfcf22364", "b948048c03e0b3d34d77f5c9dced0b41", "f91152fab791d4dc0677a289d90478a5",
            "39b01df04a6312b09f1b83c9f3a46b22", "ef775ec1f41b8b725ea83ec8c5ca04e2", "5c2b22f824524cd6c539aaeb2ecb84cd",
            "6bddf3dd32877f7b552cbf9da6b89f76", "003cd0292879733b6eab7ca79ab9cfeb", "acb738a5a945e4e2ba1504a14a529933",
            "106068dbdf13803979c7bbb63612f43d", "be916f25847b9cfc9776d88cc150ae7e", "ccc7f98e216db35c2b7a08634a9f3e20",
            "7a3332e82b0fe8c5673a2615f6c0b9a2", "62f5be96a8b8ccab9ee4aebf557cfcf7", "07fb4c225d4b5a2e2a1046ae66fc153c",
            "1f73980e45a384bed331eaa33c9ef65b", "9d675e5147b55cee0b2bc05476eef825", "bb48546ced9c61462e1c89dca4987143",
            "c7df67f4e66dad658fe856d3c8b36c7a"
        };

        [Test]
        public void Test()
        {
            for(int i = 0; i < _testFiles.Length; i++)
            {
                string  location = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "D88", _testFiles[i]);
                IFilter filter   = new LZip();
                filter.Open(location);
                IMediaImage image = new DiscImages.D88();
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