// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : NHDr0.cs
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

namespace Aaru.Tests.Images
{
    [TestFixture]
    public class NHDr0
    {
        readonly string[] _testFiles =
        {
            "t98n_128.nhd.lz", "t98n_20.nhd.lz", "t98n_256.nhd.lz", "t98n_41.nhd.lz", "t98n_512.nhd.lz",
            "t98n_65.nhd.lz", "t98n_80.nhd.lz"
        };

        readonly ulong[] _sectors =
        {
            // t98n_128.nhd.lz
            0,

            // t98n_20.nhd.lz
            0,

            // t98n_256.nhd.lz
            0,

            // t98n_41.nhd.lz
            0,

            // t98n_512.nhd.lz
            0,

            // t98n_65.nhd.lz
            0,

            // t98n_80.nhd.lz
            0
        };

        readonly uint[] _sectorSize =
        {
            // t98n_128.nhd.lz
            0,

            // t98n_20.nhd.lz
            0,

            // t98n_256.nhd.lz
            0,

            // t98n_41.nhd.lz
            0,

            // t98n_512.nhd.lz
            0,

            // t98n_65.nhd.lz
            0,

            // t98n_80.nhd.lz
            0
        };

        readonly MediaType[] _mediaTypes =
        {
            // t98n_128.nhd.lz
            MediaType.GENERIC_HDD,

            // t98n_20.nhd.lz
            MediaType.GENERIC_HDD,

            // t98n_256.nhd.lz
            MediaType.GENERIC_HDD,

            // t98n_41.nhd.lz
            MediaType.GENERIC_HDD,

            // t98n_512.nhd.lz
            MediaType.GENERIC_HDD,

            // t98n_65.nhd.lz
            MediaType.GENERIC_HDD,

            // t98n_80.nhd.lz
            MediaType.GENERIC_HDD
        };

        readonly string[] _md5S =
        {
            // t98n_128.nhd.lz
            "UNKNOWN",

            // t98n_20.nhd.lz
            "UNKNOWN",

            // t98n_256.nhd.lz
            "UNKNOWN",

            // t98n_41.nhd.lz
            "UNKNOWN",

            // t98n_512.nhd.lz
            "UNKNOWN",

            // t98n_65.nhd.lz
            "UNKNOWN",

            // t98n_80.nhd.lz
            "UNKNOWN"
        };

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "T-98 Next");

        [Test]
        public void Info()
        {
            Environment.CurrentDirectory = _dataFolder;

            Assert.Multiple(() =>
            {
                for(int i = 0; i < _testFiles.Length; i++)
                {
                    var filter = new ZZZNoFilter();
                    filter.Open(_testFiles[i]);

                    var  image  = new Nhdr0();
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
                    var filter = new ZZZNoFilter();
                    filter.Open(_testFiles[i]);

                    var   image       = new Nhdr0();
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