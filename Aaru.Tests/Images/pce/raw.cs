// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : raw.cs
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

namespace Aaru.Tests.Images.pce
{
    [TestFixture]
    public class Raw
    {
        readonly string[] _testFiles =
        {
            "md1dd_8.img.lz", "md1dd.img.lz", "md2dd_8.img.lz", "md2dd.img.lz", "md2hd.img.lz", "md2hd_nec.img.lz",
            "mf1dd_10.img.lz", "mf1dd_11.img.lz", "mf1dd_gcr.img.lz", "mf2dd_10.img.lz", "mf2dd_11.img.lz",
            "mf2dd_fdformat_800.img.lz", "mf2dd_fdformat_820.img.lz", "mf2dd_freedos.img.lz", "mf2dd_gcr.img.lz",
            "mf2dd.img.lz", "mf2ed.img.lz", "mf2hd_2m.img.lz", "mf2hd_2m_max.img.lz", "mf2hd_fdformat_168.img.lz",
            "mf2hd_fdformat_172.img.lz", "mf2hd_freedos.img.lz", "mf2hd.img.lz", "mf2hd_xdf.img.lz",
            "mf2hd_xdf_teledisk.img.lz", "rx01.img.lz", "rx50.img.lz"
        };

        readonly ulong[] _sectors =
        {
            // md1dd_8.img.lz
            320,

            // md1dd.img.lz
            360,

            // md2dd_8.img.lz
            640,

            // md2dd.img.lz
            720,

            // md2hd.img.lz
            2400,

            // md2hd_nec.img.lz
            1232,

            // mf1dd_10.img.lz
            800,

            // mf1dd_11.img.lz
            880,

            // mf1dd_gcr.img.lz
            800,

            // mf2dd_10.img.lz
            1600,

            // mf2dd_11.img.lz
            1760,

            // mf2dd_fdformat_800.img.lz
            1600,

            // mf2dd_fdformat_820.img.lz
            1640,

            // mf2dd_freedos.img.lz
            1640,

            // mf2dd_gcr.img.lz
            1600,

            // mf2dd.img.lz
            1440,

            // mf2ed.img.lz
            5760,

            // mf2hd_2m.img.lz
            3605,

            // mf2hd_2m_max.img.lz
            3768,

            // mf2hd_fdformat_168.img.lz
            3372,

            // mf2hd_fdformat_172.img.lz
            3448,

            // mf2hd_freedos.img.lz
            3486,

            // mf2hd.img.lz
            2888,

            // mf2hd_xdf.img.lz
            670,

            // mf2hd_xdf_teledisk.img.lz
            3680,

            // rx01.img.lz
            2002,

            // rx50.img.lz
            800
        };

        readonly uint[] _sectorSize =
        {
            // md1dd_8.img.lz
            512,

            // md1dd.img.lz
            512,

            // md2dd_8.img.lz
            512,

            // md2dd.img.lz
            512,

            // md2hd.img.lz
            512,

            // md2hd_nec.img.lz
            1024,

            // mf1dd_10.img.lz
            512,

            // mf1dd_11.img.lz
            512,

            // mf1dd_gcr.img.lz
            512,

            // mf2dd_10.img.lz
            512,

            // mf2dd_11.img.lz
            512,

            // mf2dd_fdformat_800.img.lz
            512,

            // mf2dd_fdformat_820.img.lz
            512,

            // mf2dd_freedos.img.lz
            512,

            // mf2dd_gcr.img.lz
            512,

            // mf2dd.img.lz
            512,

            // mf2ed.img.lz
            512,

            // mf2hd_2m.img.lz
            512,

            // mf2hd_2m_max.img.lz
            512,

            // mf2hd_fdformat_168.img.lz
            512,

            // mf2hd_fdformat_172.img.lz
            512,

            // mf2hd_freedos.img.lz
            512,

            // mf2hd.img.lz
            512,

            // mf2hd_xdf.img.lz
            8192,

            // mf2hd_xdf_teledisk.img.lz
            512,

            // rx01.img.lz
            128,

            // rx50.img.lz
            512
        };

        readonly MediaType[] _mediaTypes =
        {
            // md1dd_8.img.lz
            MediaType.DOS_525_SS_DD_8,

            // md1dd.img.lz
            MediaType.DOS_525_SS_DD_9,

            // md2dd_8.img.lz
            MediaType.DOS_525_DS_DD_8,

            // md2dd.img.lz
            MediaType.DOS_525_DS_DD_9,

            // md2hd.img.lz
            MediaType.DOS_525_HD,

            // md2hd_nec.img.lz
            MediaType.SHARP_525,

            // mf1dd_10.img.lz
            MediaType.AppleSonySS,

            // mf1dd_11.img.lz
            MediaType.ATARI_35_SS_DD_11,

            // mf1dd_gcr.img.lz
            MediaType.AppleSonySS,

            // mf2dd_10.img.lz
            MediaType.AppleSonyDS,

            // mf2dd_11.img.lz
            MediaType.CBM_AMIGA_35_DD,

            // mf2dd_fdformat_800.img.lz
            MediaType.AppleSonyDS,

            // mf2dd_fdformat_820.img.lz
            MediaType.FDFORMAT_35_DD,

            // mf2dd_freedos.img.lz
            MediaType.FDFORMAT_35_DD,

            // mf2dd_gcr.img.lz
            MediaType.AppleSonyDS,

            // mf2dd.img.lz
            MediaType.DOS_35_DS_DD_9,

            // mf2ed.img.lz
            MediaType.ECMA_147,

            // mf2hd_2m.img.lz
            MediaType.GENERIC_HDD,

            // mf2hd_2m_max.img.lz
            MediaType.GENERIC_HDD,

            // mf2hd_fdformat_168.img.lz
            MediaType.GENERIC_HDD,

            // mf2hd_fdformat_172.img.lz
            MediaType.GENERIC_HDD,

            // mf2hd_freedos.img.lz
            MediaType.GENERIC_HDD,

            // mf2hd.img.lz
            MediaType.GENERIC_HDD,

            // mf2hd_xdf.img.lz
            MediaType.XDF_35,

            // mf2hd_xdf_teledisk.img.lz
            MediaType.XDF_35,

            // rx01.img.lz
            MediaType.ECMA_54,

            // rx50.img.lz
            MediaType.AppleSonySS
        };

        readonly string[] _md5S =
        {
            // md1dd_8.img.lz
            "8308e749af855a3ded48d474eb7c305e",

            // md1dd.img.lz
            "b7b8a69b10ee4ec921aa8eea232fdd75",

            // md2dd_8.img.lz
            "f4a77a2d2a1868dc18e8b92032d02fd2",

            // md2dd.img.lz
            "099d95ac42d1a8010f914ac64ede7a70",

            // md2hd.img.lz
            "3df7cd10044af75d77e8936af0dbf9ff",

            // md2hd_nec.img.lz
            "fd54916f713d01b670c1a5df5e74a97f",

            // mf1dd_10.img.lz
            "d75d3e79d9c5051922d4c2226fa4a6ff",

            // mf1dd_11.img.lz
            "e16ed33a1a466826562c681d8bdf3e27",

            // mf1dd_gcr.img.lz
            "c5d92544c3e78b7f0a9b4baaa9a64eec",

            // mf2dd_10.img.lz
            "fd48b2c12097cbc646b4a93ef4f92259",

            // mf2dd_11.img.lz
            "512f7175e753e2e2ad620d448c42545d",

            // mf2dd_fdformat_800.img.lz
            "c533488a21098a62c85f1649abda2803",

            // mf2dd_fdformat_820.img.lz
            "db9cfb6eea18820b7a7e0b5b45594471",

            // mf2dd_freedos.img.lz
            "456390a9c6ab05cb458a03c47296de08",

            // mf2dd_gcr.img.lz
            "93e71b9ecdb39d3ec9245b4f451856d4",

            // mf2dd.img.lz
            "de3f85896f771b7e5bc4c9e3926d64e4",

            // mf2ed.img.lz
            "854d0d49a522b64af698e319a24cd68e",

            // mf2hd_2m.img.lz
            "c741c78eecd673f8fc49e77459871940",

            // mf2hd_2m_max.img.lz
            "0393fbfee10e47c71e0fb7b39237be49",

            // mf2hd_fdformat_168.img.lz
            "7f9164dc43bffc895db751ba1d9b55a9",

            // mf2hd_fdformat_172.img.lz
            "d6ff5df3707887a6ba4cfdc30b3deff4",

            // mf2hd_freedos.img.lz
            "dbd52e9e684f97d9e2292811242bb24e",

            // mf2hd.img.lz
            "f5fff7704fb677ebf23d27cd937c9403",

            // mf2hd_xdf.img.lz
            "UNKNOWN",

            // mf2hd_xdf_teledisk.img.lz
            "UNKNOWN",

            // rx01.img.lz
            "5b4e36d92b180c3845387391cb5a1c64",

            // rx50.img.lz
            "ccd4431139755c58f340681f63510642"
        };

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "pce", "raw");

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

                    var  image  = new ZZZRawImage();
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
        const uint SECTORS_TO_READ = 256;

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

                    var   image       = new ZZZRawImage();
                    bool  opened      = image.Open(filter);
                    ulong doneSectors = 0;

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

                    if(!opened)
                        continue;

                    var ctx = new Md5Context();

                    while(doneSectors < image.Info.Sectors)
                    {
                        byte[] sector;

                        if(image.Info.Sectors - doneSectors >= SECTORS_TO_READ)
                        {
                            sector      =  image.ReadSectors(doneSectors, SECTORS_TO_READ);
                            doneSectors += SECTORS_TO_READ;
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