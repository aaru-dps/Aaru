// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Apridisk.cs
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
    public class Apridisk
    {
        readonly string[] _testFiles =
        {
            "apr00001.dsk.lz", "apr00002.dsk.lz", "apr00006.dsk.lz", "apr00203.dsk.lz"
        };
        readonly ulong[] _sectors =
        {
            // apr00001.dsk.lz
            1440,

            // apr00002.dsk.lz
            1440,

            // apr00006.dsk.lz
            1440,

            // apr00203.dsk.lz
            1440
        };
        readonly uint[] _sectorSize =
        {
            // apr00001.dsk.lz
            512,

            // apr00002.dsk.lz
            512,

            // apr00006.dsk.lz
            512,

            // apr00203.dsk.lz
            512
        };
        readonly MediaType[] _mediaTypes =
        {
            // apr00001.dsk.lz
            MediaType.DOS_35_DS_DD_9,

            // apr00002.dsk.lz
            MediaType.DOS_35_DS_DD_9,

            // apr00006.dsk.lz
            MediaType.DOS_35_DS_DD_9,

            // apr00203.dsk.lz
            MediaType.DOS_35_DS_DD_9
        };
        readonly string[] _md5S =
        {
            // apr00001.dsk.lz
            "6c264287a3260a6d89e36dfcb1c98dce",

            // apr00002.dsk.lz
            "dd8e04939baeb0fcdb11ddade60c9a93",

            // apr00006.dsk.lz
            "89132d303ef6b0ff69f4cfd38e2a22a6",

            // apr00203.dsk.lz
            "cd34832ca3aa7f55e0dd8ba126372f97"
        };

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Apridisk");

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

                    var  image  = new DiscImages.Apridisk();
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

                    var   image       = new DiscImages.Apridisk();
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