// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Anex86.cs
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
using Aaru.Filters;
using FluentAssertions.Execution;
using NUnit.Framework;

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class Anex86
    {
        readonly string[] _testFiles =
        {
            "anex86_10mb.hdi.lz", "anex86_15mb.hdi.lz", "anex86_20mb.hdi.lz", "anex86_30mb.hdi.lz",
            "anex86_40mb.hdi.lz", "anex86_5mb.hdi.lz", "blank_md2hd.fdi.lz", "msdos33d_md2hd.fdi.lz",
            "msdos50_epson_md2hd.fdi.lz", "msdos50_md2hd.fdi.lz", "msdos62_md2hd.fdi.lz"
        };

        readonly ulong[] _sectors =
        {
            40920, 61380, 81840, 121770, 162360, 20196, 1232, 1232, 1232, 1232, 1232
        };

        readonly uint[] _sectorSize =
        {
            256, 256, 256, 256, 256, 256, 1024, 1024, 1024, 1024, 1024
        };

        readonly MediaType[] _mediaTypes =
        {
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.GENERIC_HDD,
            MediaType.GENERIC_HDD, MediaType.GENERIC_HDD, MediaType.NEC_525_HD, MediaType.NEC_525_HD,
            MediaType.NEC_525_HD, MediaType.NEC_525_HD, MediaType.NEC_525_HD
        };

        readonly string[] _md5S =
        {
            "1c5387e38e58165c517c059e5d48905d", "a84366658c1c3bd09af4d0d42fbf716e", "919c9eecf1b65b10870f617cb976668a",
            "02d35af02581afb2e56792dcaba2c1af", "b8c3f858f1a9d300d3e74f36eea04354", "c348bbbaf99fcb8c8e66de157aef62f4",
            "c3587f7020743067cf948c9d5c5edb27", "a23874a4474334b035a24c6924140744", "bc1ef3236e75cb09575037b884ee9dce",
            "243036c4617b666a6c886cc23d7274e0", "09bb2ff964a0c5c223a1900f085e3955"
        };

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Anex86");

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

                    var  image  = new DiscImages.Anex86();
                    bool opened = image.Open(filter);

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

                    using(new AssertionScope())
                    {
                        Assert.Multiple(() =>
                        {
                            Assert.AreEqual(_sectors[i], image.Info.Sectors, _testFiles[i]);
                            Assert.AreEqual(_sectorSize[i], image.Info.SectorSize, _testFiles[i]);
                            Assert.AreEqual(_mediaTypes[i], image.Info.MediaType, _testFiles[i]);
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

                    var   image       = new DiscImages.Anex86();
                    bool  opened      = image.Open(filter);
                    ulong doneSectors = 0;

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");
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

                    Assert.AreEqual(_md5S[i], ctx.End(), _testFiles[i]);
                }
            });
        }
    }
}