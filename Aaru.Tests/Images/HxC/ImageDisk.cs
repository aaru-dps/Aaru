// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ImageDisk.cs
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

using System;
using System.IO;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.DiscImages;
using Aaru.Filters;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Images.HxC
{
    [TestFixture]
    public class ImageDisk
    {
        readonly string[] _testFiles =
        {
            "md1dd_8.imd.lz", "md1dd.imd.lz", "md2dd_8.imd.lz", "md2dd.imd.lz", "md2hd.imd.lz", "md2hd_nec.imd.lz",
            "mf1dd_10.imd.lz", "mf1dd_11.imd.lz", "mf2dd_10.imd.lz", "mf2dd_11.imd.lz", "mf2dd_acorn.imd.lz",
            "mf2dd_fdformat_800.imd.lz", "mf2dd_fdformat_820.imd.lz", "mf2dd_freedos.imd.lz", "mf2dd.imd.lz",
            "mf2ed.imd.lz", "mf2hd_2m.imd.lz", "mf2hd_2m_max.imd.lz", "mf2hd_fdformat_168.imd.lz",
            "mf2hd_fdformat_172.imd.lz", "mf2hd_freedos.imd.lz", "mf2hd.imd.lz", "rx01.imd.lz", "rx50.imd.lz",
            "mf2hd_xdf.imd.lz", "mf2hd_xdf_teledisk.imd.lz"
        };

        readonly ulong[] _sectors =
        {
            // md1dd_8.imd.lz
            320,

            // md1dd.imd.lz
            360,

            // md2dd_8.imd.lz
            640,

            // md2dd.imd.lz
            720,

            // md2hd.imd.lz
            2400,

            // md2hd_nec.imd.lz
            1232,

            // mf1dd_10.imd.lz
            800,

            // mf1dd_11.imd.lz
            880,

            // mf2dd_10.imd.lz
            1600,

            // mf2dd_11.imd.lz
            1760,

            // mf2dd_acorn.imd.lz
            800,

            // mf2dd_fdformat_800.imd.lz
            1600,

            // mf2dd_fdformat_820.imd.lz
            1640,

            // mf2dd_freedos.imd.lz
            1640,

            // mf2dd.imd.lz
            1440,

            // mf2ed.imd.lz
            5760,

            // mf2hd_2m.imd.lz
            1812,

            // mf2hd_2m_max.imd.lz
            1160,

            // mf2hd_fdformat_168.imd.lz
            3372,

            // mf2hd_fdformat_172.imd.lz
            3444,

            // mf2hd_freedos.imd.lz
            3486,

            // mf2hd.imd.lz
            2882,

            // rx01.imd.lz
            2002,

            // rx50.imd.lz
            800,

            // mf2hd_xdf.imd.lz
            0,

            // mf2hd_xdf_teledisk.imd.lz
            0
        };

        readonly uint[] _sectorSize =
        {
            // md1dd_8.imd.lz
            512,

            // md1dd.imd.lz
            512,

            // md2dd_8.imd.lz
            512,

            // md2dd.imd.lz
            512,

            // md2hd.imd.lz
            512,

            // md2hd_nec.imd.lz
            1024,

            // mf1dd_10.imd.lz
            512,

            // mf1dd_11.imd.lz
            512,

            // mf2dd_10.imd.lz
            512,

            // mf2dd_11.imd.lz
            512,

            // mf2dd_acorn.imd.lz
            1024,

            // mf2dd_fdformat_800.imd.lz
            512,

            // mf2dd_fdformat_820.imd.lz
            512,

            // mf2dd_freedos.imd.lz
            512,

            // mf2dd.imd.lz
            512,

            // mf2ed.imd.lz
            512,

            // mf2hd_2m.imd.lz
            1024,

            // mf2hd_2m_max.imd.lz
            2048,

            // mf2hd_fdformat_168.imd.lz
            512,

            // mf2hd_fdformat_172.imd.lz
            512,

            // mf2hd_freedos.imd.lz
            512,

            // mf2hd.imd.lz
            2048,

            // rx01.imd.lz
            128,

            // rx50.imd.lz
            512,

            // mf2hd_xdf.imd.lz
            0,

            // mf2hd_xdf_teledisk.imd.lz
            0
        };

        readonly MediaType[] _mediaTypes =
        {
            // Media type: md1dd_8.imd.lz
            MediaType.DOS_525_SS_DD_8,

            // Media type: md1dd.imd.lz
            MediaType.DOS_525_SS_DD_9,

            // Media type: md2dd_8.imd.lz
            MediaType.DOS_525_DS_DD_8,

            // Media type: md2dd.imd.lz
            MediaType.DOS_525_DS_DD_9,

            // Media type: md2hd.imd.lz
            MediaType.NEC_35_HD_15,

            // Media type: md2hd_nec.imd.lz
            MediaType.NEC_35_HD_8,

            // Media type: mf1dd_10.imd.lz
            MediaType.RX50,

            // Media type: mf1dd_11.imd.lz
            MediaType.ATARI_35_SS_DD_11,

            // Media type: mf2dd_10.imd.lz
            MediaType.CBM_35_DD,

            // Media type: mf2dd_11.imd.lz
            MediaType.CBM_AMIGA_35_DD,

            // Media type: mf2dd_acorn.imd.lz
            MediaType.Unknown,

            // Media type: mf2dd_fdformat_800.imd.lz
            MediaType.CBM_35_DD,

            // Media type: mf2dd_fdformat_820.imd.lz
            MediaType.Unknown,

            // Media type: mf2dd_freedos.imd.lz
            MediaType.FDFORMAT_35_DD,

            // Media type: mf2dd.imd.lz
            MediaType.DOS_35_DS_DD_9,

            // Media type: mf2ed.imd.lz
            MediaType.ECMA_147,

            // Media type: mf2hd_2m.imd.lz
            MediaType.Unknown,

            // Media type: mf2hd_2m_max.imd.lz
            MediaType.Unknown,

            // Media type: mf2hd_fdformat_168.imd.lz
            MediaType.Unknown,

            // Media type: mf2hd_fdformat_172.imd.lz
            MediaType.Unknown,

            // Media type: mf2hd_freedos.imd.lz
            MediaType.Unknown,

            // Media type: mf2hd.imd.lz
            MediaType.Unknown,

            // Media type: rx01.imd.lz
            MediaType.Unknown,

            // Media type: rx50.imd.lz
            MediaType.RX50,

            // mf2hd_xdf.imd.lz
            MediaType.XDF_35,

            // mf2hd_xdf_teledisk.imd.lz
            MediaType.XDF_35
        };

        readonly string[] _md5S =
        {
            // md1dd_8.imd.lz
            "8308e749af855a3ded48d474eb7c305e",

            // md1dd.imd.lz
            "b7b8a69b10ee4ec921aa8eea232fdd75",

            // md2dd_8.imd.lz
            "f4a77a2d2a1868dc18e8b92032d02fd2",

            // md2dd.imd.lz
            "099d95ac42d1a8010f914ac64ede7a70",

            // md2hd.imd.lz
            "3df7cd10044af75d77e8936af0dbf9ff",

            // md2hd_nec.imd.lz
            "fd54916f713d01b670c1a5df5e74a97f",

            // mf1dd_10.imd.lz
            "d75d3e79d9c5051922d4c2226fa4a6ff",

            // mf1dd_11.imd.lz
            "e16ed33a1a466826562c681d8bdf3e27",

            // mf2dd_10.imd.lz
            "fd48b2c12097cbc646b4a93ef4f92259",

            // mf2dd_11.imd.lz
            "512f7175e753e2e2ad620d448c42545d",

            // mf2dd_acorn.imd.lz
            "2626f65b49ec085253c41fa2e2a9e788",

            // mf2dd_fdformat_800.imd.lz
            "c533488a21098a62c85f1649abda2803",

            // mf2dd_fdformat_820.imd.lz
            "9d978dff1196b456b8372d78e6b17970",

            // mf2dd_freedos.imd.lz
            "456390a9c6ab05cb458a03c47296de08",

            // mf2dd.imd.lz
            "de3f85896f771b7e5bc4c9e3926d64e4",

            // mf2ed.imd.lz
            "854d0d49a522b64af698e319a24cd68e",

            // mf2hd_2m.imd.lz
            "7ee82cecd23b30cc9aa6f0ec59877851",

            // mf2hd_2m_max.imd.lz
            "90a3c86eb9f8bdf6e4c15c445dff121e",

            // mf2hd_fdformat_168.imd.lz
            "7f9164dc43bffc895db751ba1d9b55a9",

            // mf2hd_fdformat_172.imd.lz
            "9dea1e119a73a21a38d134f36b2e5564",

            // mf2hd_freedos.imd.lz
            "dbd52e9e684f97d9e2292811242bb24e",

            // mf2hd.imd.lz
            "f5fff7704fb677ebf23d27cd937c9403",

            // rx01.imd.lz
            "5b4e36d92b180c3845387391cb5a1c64",

            // rx50.imd.lz
            "ccd4431139755c58f340681f63510642",

            // mf2hd_xdf.imd.lz
            "UNKNOWN",

            // mf2hd_xdf_teledisk.imd.lz
            "UNKNOWN"
        };

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "HxC", "ImageDisk");

        [Test]
        public void Info()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var filter = new LZip();
                    filter.Open(_testFiles[i]);

                    var  image  = new Imd();
                    bool opened = image.Open(filter);

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

                    if(!opened)
                        continue;

                    using(new AssertionScope())
                    {
                        Assert.Multiple(() =>
                        {
                            Assert.AreEqual(_sectors[i], image.Info.Sectors, $"Sectors: {_testFiles[i]}");
                            Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, $"Sector size: {_testFiles[i]}");
                            Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, $"Media type: {_testFiles[i]}");
                        });
                    }
                }
            });
        }

        // How many sectors to read at once
        const uint _sectorsToRead = 256;

        [Test]
        public void Hashes()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var filter = new LZip();
                    filter.Open(_testFiles[i]);

                    var   image       = new Imd();
                    bool  opened      = image.Open(filter);
                    ulong doneSectors = 0;

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

                    if(!opened)
                        continue;

                    var ctx = new Md5Context();

                    while(doneSectors < image.Info.Sectors)
                    {
                        byte[] sector;

                        if(image.Info.Sectors - doneSectors >= _sectorsToRead)
                        {
                            sector      =  image.ReadSectors(doneSectors, _sectorsToRead);
                            doneSectors += _sectorsToRead;
                        }
                        else
                        {
                            sector      =  image.ReadSectors(doneSectors, (uint)(image.Info.Sectors - doneSectors));
                            doneSectors += image.Info.Sectors - doneSectors;
                        }

                        ctx.Update(sector);
                    }

                    Assert.AreEqual(_md5S[i], ctx.End(), $"Hash: {_testFiles[i]}");
                }
            });
        }
    }
}