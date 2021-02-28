// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DiskCopy42.cs
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
    public class DiskCopy42
    {
        readonly string[] _testFiles =
        {
            "hfs.dsk.lz", "mf1dd_hfs.img.lz", "mf1dd_mfs.img.lz", "mf2dd_hfs.img.lz", "mf2dd_mfs.img.lz",
            "modified.dsk.lz", "pascal800.dsk.lz", "prodos1440.dsk.lz", "prodos800.dsk.lz"
        };

        readonly ulong[] _sectors =
        {
            // hfs.dsk.lz
            1600,

            // mf1dd_hfs.img.lz
            800,

            // mf1dd_mfs.img.lz
            800,

            // mf2dd_hfs.img.lz
            1600,

            // mf2dd_mfs.img.lz
            1600,

            // modified.dsk.lz
            1600,

            // pascal800.dsk.lz
            1600,

            // prodos1440.dsk.lz
            1600,

            // prodos800.dsk.lz
            1600
        };

        readonly uint[] _sectorSize =
        {
            // hfs.dsk.lz
            512,

            // mf1dd_hfs.img.lz
            512,

            // mf1dd_mfs.img.lz
            512,

            // mf2dd_hfs.img.lz
            512,

            // mf2dd_mfs.img.lz
            512,

            // modified.dsk.lz
            512,

            // pascal800.dsk.lz
            512,

            // prodos1440.dsk.lz
            512,

            // prodos800.dsk.lz
            512
        };

        readonly MediaType[] _mediaTypes =
        {
            // hfs.dsk.lz
            MediaType.AppleSonyDS,

            // mf1dd_hfs.img.lz
            MediaType.AppleSonySS,

            // mf1dd_mfs.img.lz
            MediaType.AppleSonySS,

            // mf2dd_hfs.img.lz
            MediaType.AppleSonyDS,

            // mf2dd_mfs.img.lz
            MediaType.AppleSonyDS,

            // modified.dsk.lz
            MediaType.AppleSonyDS,

            // pascal800.dsk.lz
            MediaType.AppleSonyDS,

            // prodos1440.dsk.lz
            MediaType.AppleSonyDS,

            // prodos800.dsk.lz
            MediaType.AppleSonyDS
        };

        readonly string[] _md5S =
        {
            // hfs.dsk.lz
            "2762f41d0379b476042fc62891baac84",

            // mf1dd_hfs.img.lz
            "eae3a95671d077deb702b3549a769f56",

            // mf1dd_mfs.img.lz
            "c5d92544c3e78b7f0a9b4baaa9a64eec",

            // mf2dd_hfs.img.lz
            "a99744348a70b62b57bce2dec9132ced",

            // mf2dd_mfs.img.lz
            "93e71b9ecdb39d3ec9245b4f451856d4",

            // modified.dsk.lz
            "b748f6df3e60e7169d42ec6fcc857ea4",

            // pascal800.dsk.lz
            "dbd0ec8a3126236910709faf923adcf2",

            // prodos1440.dsk.lz
            "fcf747bd356b48d442ff74adb8f3516b",

            // prodos800.dsk.lz
            "fcf747bd356b48d442ff74adb8f3516b"
        };

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "DiskCopy 4.2");

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

                    var  image  = new DiscImages.DiskCopy42();
                    bool opened = image.Open(filter);

                    Assert.AreEqual(true, opened, $"Open: {_testFiles[i]}");

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

                    var   image       = new DiscImages.DiskCopy42();
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

                    Assert.AreEqual(_md5S[i], ctx.End(), $"Hash: {_testFiles[i]}");
                }
            });
        }
    }
}