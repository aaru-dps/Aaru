// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : UDCO.cs
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

namespace Aaru.Tests.Images.DiskCopy65
{
    [TestFixture]
    public class UDCO
    {
        readonly string[] _testFiles =
        {
            "DC6_UDCO_DOS_1440.img.lz", "DC6_UDCO_DOS_720.img.lz", "DC6_UDCO_DOS_DMF.img.lz",
            "DC6_UDCO_HFS_1440.img.lz", "DC6_UDCO_HFS_800.img.lz", "DC6_UDCO_HFS_DMF.img.lz", "DC6_UDCO_PD_1440.img.lz",
            "DC6_UDCO_PD_800.img.lz", "DC6_UDCO_PD_DMF.img.lz"
        };

        readonly ulong[] _sectors =
        {
            // DC6_UDCO_DOS_1440.img.lz
            2880,

            // DC6_UDCO_DOS_720.img.lz
            1440,

            // DC6_UDCO_DOS_DMF.img.lz
            3360,

            // DC6_UDCO_HFS_1440.img.lz
            2880,

            // DC6_UDCO_HFS_800.img.lz
            1600,

            // DC6_UDCO_HFS_DMF.img.lz
            3360,

            // DC6_UDCO_PD_1440.img.lz
            2880,

            // DC6_UDCO_PD_800.img.lz
            1600,

            // DC6_UDCO_PD_DMF.img.lz
            3360
        };

        readonly uint[] _sectorSize =
        {
            // DC6_UDCO_DOS_1440.img.lz
            512,

            // DC6_UDCO_DOS_720.img.lz
            512,

            // DC6_UDCO_DOS_DMF.img.lz
            512,

            // DC6_UDCO_HFS_1440.img.lz
            512,

            // DC6_UDCO_HFS_800.img.lz
            512,

            // DC6_UDCO_HFS_DMF.img.lz
            512,

            // DC6_UDCO_PD_1440.img.lz
            512,

            // DC6_UDCO_PD_800.img.lz
            512,

            // DC6_UDCO_PD_DMF.img.lz
            512
        };

        readonly MediaType[] _mediaTypes =
        {
            // DC6_UDCO_DOS_1440.img.lz
            MediaType.DOS_35_HD,

            // DC6_UDCO_DOS_720.img.lz
            MediaType.DOS_35_DS_DD_9,

            // DC6_UDCO_DOS_DMF.img.lz
            MediaType.DMF,

            // DC6_UDCO_HFS_1440.img.lz
            MediaType.DOS_35_HD,

            // DC6_UDCO_HFS_800.img.lz
            MediaType.AppleSonyDS,

            // DC6_UDCO_HFS_DMF.img.lz
            MediaType.DMF,

            // DC6_UDCO_PD_1440.img.lz
            MediaType.DOS_35_HD,

            // DC6_UDCO_PD_800.img.lz
            MediaType.AppleSonyDS,

            // DC6_UDCO_PD_DMF.img.lz
            MediaType.DMF
        };

        readonly string[] _md5S =
        {
            // DC6_UDCO_DOS_1440.img.lz
            "ff419213080574056ebd9adf7bab3d32",

            // DC6_UDCO_DOS_720.img.lz
            "c2be571406cf6353269faa59a4a8c0a4",

            // DC6_UDCO_DOS_DMF.img.lz
            "92ea7a359957012a682ba126cfdef0ce",

            // DC6_UDCO_HFS_1440.img.lz
            "3160038ca028ccf52ad7863790072145",

            // DC6_UDCO_HFS_800.img.lz
            "5e255c4bc0f6a26ecd27845b37e65aaa",

            // DC6_UDCO_HFS_DMF.img.lz
            "652dc979c177f2d8e846587158b38478",

            // DC6_UDCO_PD_1440.img.lz
            "7975e8cf7579a6848d6fb4e546d1f682",

            // DC6_UDCO_PD_800.img.lz
            "a72da7aedadbe194c22a3d71c62e4766",

            // DC6_UDCO_PD_DMF.img.lz
            "7fbf0251a93cb36d98e68b7d19624de5"
        };

        readonly string _dataFolder =
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "DiskCopy 6.5", "UDIF", "UDCO");

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

                    var  image  = new Udif();
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

                    var   image       = new Udif();
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

                    Assert.AreEqual(_md5S[i], ctx.End(), $"Hash: {_testFiles[i]}");
                }
            });
        }
    }
}