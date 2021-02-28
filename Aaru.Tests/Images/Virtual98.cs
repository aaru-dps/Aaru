// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Virtual98.cs
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
    public class Virtual98
    {
        readonly string[] _testFiles =
        {
            "v98_128.hdd.lz", "v98_20.hdd.lz", "v98_256.hdd.lz", "v98_41.hdd.lz", "v98_512.hdd.lz", "v98_65.hdd.lz",
            "v98_80.hdd.lz"
        };

        readonly ulong[] _sectors =
        {
            // v98_128.hdd.lz
            524288,

            // v98_20.hdd.lz
            81920,

            // v98_256.hdd.lz
            1048576,

            // v98_41.hdd.lz
            167936,

            // v98_512.hdd.lz
            2097152,

            // v98_65.hdd.lz
            266240,

            // v98_80.hdd.lz
            327680
        };

        readonly uint[] _sectorSize =
        {
            // v98_128.hdd.lz
            256,

            // v98_20.hdd.lz
            256,

            // v98_256.hdd.lz
            256,

            // v98_41.hdd.lz
            256,

            // v98_512.hdd.lz
            256,

            // v98_65.hdd.lz
            256,

            // v98_80.hdd.lz
            256
        };

        readonly MediaType[] _mediaTypes =
        {
            // v98_128.hdd.lz
            MediaType.GENERIC_HDD,

            // v98_20.hdd.lz
            MediaType.GENERIC_HDD,

            // v98_256.hdd.lz
            MediaType.GENERIC_HDD,

            // v98_41.hdd.lz
            MediaType.GENERIC_HDD,

            // v98_512.hdd.lz
            MediaType.GENERIC_HDD,

            // v98_65.hdd.lz
            MediaType.GENERIC_HDD,

            // v98_80.hdd.lz
            MediaType.GENERIC_HDD
        };

        readonly string[] _md5S =
        {
            // v98_128.hdd.lz
            "be3693b92a5242101e80087611b33092",

            // v98_20.hdd.lz
            "811b2a9d08abbecf4cb75531d5e51808",

            // v98_256.hdd.lz
            "cf4375422f50d62e163d697a18542eca",

            // v98_41.hdd.lz
            "fe4fc08015f1e3a4562e8e867107b561",

            // v98_512.hdd.lz
            "afb49485f0ef2b39e8377c1fe880e77b",

            // v98_65.hdd.lz
            "9e4c0bc8bc955b1a21a94df0f7bec3ab",

            // v98_80.hdd.lz
            "f5906261c390ea5c5a0e46864fb066cd"
        };

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Virtual98");

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

                    var  image  = new DiscImages.Virtual98();
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

                    var   image       = new DiscImages.Virtual98();
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