// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DOS.cs
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

namespace Aaru.Tests.Images.AppleDOS
{
    [TestFixture]
    public class DOS
    {
        readonly string[] _testFiles =
        {
            "dos33.do.lz", "hfs.do.lz", "pascal800.do.lz", "pascal.do.lz", "prodos800.do.lz", "prodos.do.lz",
            "prodosmod.do.lz"
        };

        readonly ulong[] _sectors =
        {
            // dos33.do.lz
            560,

            // hfs.do.lz
            560,

            // pascal800.do.lz
            560,

            // pascal.do.lz
            560,

            // prodos800.do.lz
            560,

            // prodos.do.lz
            560,

            // prodosmod.do.lz
            560
        };

        readonly uint[] _sectorSize =
        {
            // dos33.do.lz
            256,

            // hfs.do.lz
            256,

            // pascal800.do.lz
            256,

            // pascal.do.lz
            256,

            // prodos800.do.lz
            256,

            // prodos.do.lz
            256,

            // prodosmod.do.lz
            256
        };

        readonly MediaType[] _mediaTypes =
        {
            // dos33.do.lz
            MediaType.Apple33SS,

            // hfs.do.lz
            MediaType.Apple33SS,

            // pascal800.do.lz
            MediaType.Apple33SS,

            // pascal.do.lz
            MediaType.Apple33SS,

            // prodos800.do.lz
            MediaType.Apple33SS,

            // prodos.do.lz
            MediaType.Apple33SS,

            // prodosmod.do.lz
            MediaType.Apple33SS
        };

        readonly string[] _md5S =
        {
            // dos33.do.lz
            "0ffcbd4180306192726926b43755db2f",

            // hfs.do.lz
            "ddd04ef378552c789f85382b4f49da06",

            // pascal800.do.lz
            "5158e2fe9d8e7ae1f7db73156478e4f4",

            // pascal.do.lz
            "4c4926103a32ac15f7e430ec3ced4be5",

            // prodos800.do.lz
            "193c5cc22f07e5aeb96eb187cb59c2d9",

            // prodos.do.lz
            "23f42e529c9fde2a8033f1bc6a7bca93",

            // prodosmod.do.lz
            "a7ec980472c320da5ea6f2f0aec0f502"
        };

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Apple DOS Order");

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

                    var  image  = new AppleDos();
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

                    var   image       = new AppleDos();
                    bool  opened      = image.Open(filter);
                    ulong doneSectors = 0;

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");
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

                    Assert.AreEqual(_md5S[i], ctx.End(), _testFiles[i]);
                }
            });
        }
    }
}