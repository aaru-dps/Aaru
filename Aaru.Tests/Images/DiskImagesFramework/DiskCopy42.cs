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

namespace Aaru.Tests.Images.DiskImagesFramework
{
    [TestFixture]
    public class DiskCopy42
    {
        readonly string[] _testFiles =
        {
            "DOS_1440.img.lz", "DOS_720.img.lz", "HFS_1440.img.lz", "HFS_800.img.lz", "ProDOS_1440.img.lz",
            "ProDOS_800.img.lz", "UFS_1440.img.lz", "UFS_720.img.lz", "UFS_800.img.lz"
        };

        readonly ulong[] _sectors =
        {
            // DOS_1440.img.lz
            2880,

            // DOS_720.img.lz
            1440,

            // HFS_1440.img.lz
            2880,

            // HFS_800.img.lz
            1600,

            // ProDOS_1440.img.lz
            2880,

            // ProDOS_800.img.lz
            1600,

            // UFS_1440.img.lz
            2880,

            // UFS_720.img.lz
            1440,

            // UFS_800.img.lz
            1600
        };

        readonly uint[] _sectorSize =
        {
            // DOS_1440.img.lz
            512,

            // DOS_720.img.lz
            512,

            // HFS_1440.img.lz
            512,

            // HFS_800.img.lz
            512,

            // ProDOS_1440.img.lz
            512,

            // ProDOS_800.img.lz
            512,

            // UFS_1440.img.lz
            512,

            // UFS_720.img.lz
            512,

            // UFS_800.img.lz
            512
        };

        readonly MediaType[] _mediaTypes =
        {
            // DOS_1440.img.lz
            MediaType.DOS_35_HD,

            // DOS_720.img.lz
            MediaType.DOS_35_DS_DD_9,

            // HFS_1440.img.lz
            MediaType.DOS_35_HD,

            // HFS_800.img.lz
            MediaType.AppleSonyDS,

            // ProDOS_1440.img.lz
            MediaType.DOS_35_HD,

            // ProDOS_800.img.lz
            MediaType.AppleSonyDS,

            // UFS_1440.img.lz
            MediaType.DOS_35_HD,

            // UFS_720.img.lz
            MediaType.DOS_35_DS_DD_9,

            // UFS_800.img.lz
            MediaType.AppleSonyDS
        };

        readonly string[] _md5S =
        {
            // DOS_1440.img.lz
            "ff419213080574056ebd9adf7bab3d32",

            // DOS_720.img.lz
            "c2be571406cf6353269faa59a4a8c0a4",

            // HFS_1440.img.lz
            "3160038ca028ccf52ad7863790072145",

            // HFS_800.img.lz
            "5e255c4bc0f6a26ecd27845b37e65aaa",

            // ProDOS_1440.img.lz
            "7975e8cf7579a6848d6fb4e546d1f682",

            // ProDOS_800.img.lz
            "a72da7aedadbe194c22a3d71c62e4766",

            // UFS_1440.img.lz
            "b37823c7a90d1917f719ba5927b23da8",

            // UFS_720.img.lz
            "4942032f7bf1d115237ea1764424828b",

            // UFS_800.img.lz
            "85574aebeef03eb355bf8541955d06ea"
        };

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "DiskImagesFramework",
                                                   "DiskCopy 4.2");

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