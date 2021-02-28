// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : VirtualPC.cs
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

namespace Aaru.Tests.Images.VirtualPC
{
    [TestFixture]
    public class Raw
    {
        readonly string[] _testFiles =
        {
            "vpc106b_fixed_150mb_fat16.lz", "vpc213_fixed_50mb_fat16.lz", "vpc303_fixed_30mb_fat16.lz",
            "vpc30_fixed_30mb_fat16.lz", "vpc4_fixed_130mb_fat16.lz"
        };

        readonly ulong[] _sectors =
        {
            // vpc106b_fixed_150mb_fat16.lz
            307024,

            // vpc213_fixed_50mb_fat16.lz
            102306,

            // vpc303_fixed_30mb_fat16.lz
            62356,

            // vpc30_fixed_30mb_fat16.lz
            61404,

            // vpc4_fixed_130mb_fat16.lz
            266016
        };

        readonly uint[] _sectorSize =
        {
            // vpc106b_fixed_150mb_fat16.lz
            512,

            // vpc213_fixed_50mb_fat16.lz
            512,

            // vpc303_fixed_30mb_fat16.lz
            512,

            // vpc30_fixed_30mb_fat16.lz
            512,

            // vpc4_fixed_130mb_fat16.lz
            512
        };

        readonly MediaType[] _mediaTypes =
        {
            // vpc106b_fixed_150mb_fat16.lz
            MediaType.GENERIC_HDD,

            // vpc213_fixed_50mb_fat16.lz
            MediaType.GENERIC_HDD,

            // vpc303_fixed_30mb_fat16.lz
            MediaType.GENERIC_HDD,

            // vpc30_fixed_30mb_fat16.lz
            MediaType.GENERIC_HDD,

            // vpc4_fixed_130mb_fat16.lz
            MediaType.GENERIC_HDD
        };

        readonly string[] _md5S =
        {
            // vpc106b_fixed_150mb_fat16.lz
            "56eb1b7a4ea849e93de35f48b8912cd1",

            // vpc213_fixed_50mb_fat16.lz
            "f05abd9ff39f6b7e39834724b52a49e1",

            // vpc303_fixed_30mb_fat16.lz
            "46d5f39b1169a2721863b71e2944e3c2",

            // vpc30_fixed_30mb_fat16.lz
            "86b522d83ab057fa76eab0941357e1f6",

            // vpc4_fixed_130mb_fat16.lz
            "5f4d4c4f268ea19c91bf4fb49f4894b6"
        };

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "VirtualPC");

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