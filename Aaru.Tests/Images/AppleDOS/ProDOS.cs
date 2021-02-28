// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ProDOS.cs
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
    public class ProDOS
    {
        readonly string[] _testFiles =
        {
            "dos33.po.lz", "hfs1440.po.lz", "hfs.po.lz", "pascal800.po.lz", "pascal.po.lz", "prodos1440.po.lz",
            "prodos5mb.po.lz", "prodos800.po.lz", "prodosmod.po.lz", "prodos.po.lz"
        };

        readonly ulong[] _sectors =
        {
            // dos33.po.lz
            560,

            // hfs1440.po.lz
            560,

            // hfs.po.lz
            560,

            // pascal800.po.lz
            560,

            // pascal.po.lz
            560,

            // prodos1440.po.lz
            560,

            // prodos5mb.po.lz
            560,

            // prodos800.po.lz
            560,

            // prodosmod.po.lz
            560,

            // prodos.po.lz
            560
        };

        readonly uint[] _sectorSize =
        {
            // dos33.po.lz
            256,

            // hfs1440.po.lz
            256,

            // hfs.po.lz
            256,

            // pascal800.po.lz
            256,

            // pascal.po.lz
            256,

            // prodos1440.po.lz
            256,

            // prodos5mb.po.lz
            256,

            // prodos800.po.lz
            256,

            // prodosmod.po.lz
            256,

            // prodos.po.lz
            256
        };

        readonly MediaType[] _mediaTypes =
        {
            // dos33.po.lz
            MediaType.Apple33SS,

            // hfs1440.po.lz
            MediaType.Apple33SS,

            // hfs.po.lz
            MediaType.Apple33SS,

            // pascal800.po.lz
            MediaType.Apple33SS,

            // pascal.po.lz
            MediaType.Apple33SS,

            // prodos1440.po.lz
            MediaType.Apple33SS,

            // prodos5mb.po.lz
            MediaType.Apple33SS,

            // prodos800.po.lz
            MediaType.Apple33SS,

            // prodosmod.po.lz
            MediaType.Apple33SS,

            // prodos.po.lz
            MediaType.Apple33SS
        };

        readonly string[] _md5S =
        {
            // dos33.po.lz
            "0ffcbd4180306192726926b43755db2f",

            // hfs1440.po.lz
            "2c0b397aa3fe23a52cf7908340739f78",

            // hfs.po.lz
            "ddd04ef378552c789f85382b4f49da06",

            // pascal800.po.lz
            "5158e2fe9d8e7ae1f7db73156478e4f4",

            // pascal.po.lz
            "4c4926103a32ac15f7e430ec3ced4be5",

            // prodos1440.po.lz
            "55ff5838139c0e8fa3f904397dc22fa5",

            // prodos5mb.po.lz
            "137463bc1f758fb8f2c354b02603817b",

            // prodos800.po.lz
            "193c5cc22f07e5aeb96eb187cb59c2d9",

            // prodosmod.po.lz
            "26d9c57e262f61c4eb6c150eefafe4c0",

            // prodos.po.lz
            "11ef56c80c94347d2e3f921d5c36c8de"
        };

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "Apple ProDOS Order");

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

                    var   image       = new AppleDos();
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