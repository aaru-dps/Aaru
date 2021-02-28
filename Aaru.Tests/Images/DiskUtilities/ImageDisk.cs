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

namespace Aaru.Tests.Images.DiskUtilities
{
    [TestFixture]
    public class ImageDisk
    {
        readonly string[] _testFiles =
        {
            "mf2dd_acorn.imd.lz", "mf2dd_fdformat_820.imd.lz", "mf2hd_2m.imd.lz", "mf2hd_fdformat_172.imd.lz"
        };

        readonly ulong[] _sectors =
        {
            // mf2dd_acorn.imd.lz
            800,

            // mf2dd_fdformat_820.imd.lz
            1640,

            // mf2hd_2m.imd.lz
            1812,

            // mf2hd_fdformat_172.imd.lz
            3444
        };

        readonly uint[] _sectorSize =
        {
            // mf2dd_acorn.imd.lz
            1024,

            // mf2dd_fdformat_820.imd.lz
            512,

            // mf2hd_2m.imd.lz
            1024,

            // mf2hd_fdformat_172.imd.lz
            512
        };

        readonly MediaType[] _mediaTypes =
        {
            // mf2dd_acorn.imd.lz
            MediaType.ACORN_35_DS_DD,

            // mf2dd_fdformat_820.imd.lz
            MediaType.FDFORMAT_35_DD,

            // mf2hd_2m.imd.lz
            MediaType.Unknown,

            // mf2hd_fdformat_172.imd.lz
            MediaType.FDFORMAT_35_HD
        };

        readonly string[] _md5S =
        {
            // mf2dd_acorn.imd.lz
            "2626f65b49ec085253c41fa2e2a9e788",

            // mf2dd_fdformat_820.imd.lz
            "9d978dff1196b456b8372d78e6b17970",

            // mf2hd_2m.imd.lz
            "7ee82cecd23b30cc9aa6f0ec59877851",

            // mf2hd_fdformat_172.imd.lz
            "9dea1e119a73a21a38d134f36b2e5564"
        };

        readonly string _dataFolder =
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "disk-analyse", "ImageDisk");

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