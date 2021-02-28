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

namespace Aaru.Tests.Images.VirtualBox
{
    [TestFixture]
    public class VirtualPc
    {
        readonly string[] _testFiles =
        {
            "virtualbox_linux_dynamic_250mb.vhd.lz", "virtualbox_linux_fixed_10mb.vhd.lz",
            "virtualbox_macos_dynamic_250mb.vhd.lz", "virtualbox_macos_fixed_10mb.vhd.lz",
            "virtualbox_windows_dynamic_250mb.vhd.lz", "virtualbox_windows_fixed_10mb.vhd.lz"
        };

        readonly ulong[] _sectors =
        {
            // virtualbox_linux_dynamic_250mb.vhd.lz
            512000,

            // virtualbox_linux_fixed_10mb.vhd.lz
            20480,

            // virtualbox_macos_dynamic_250mb.vhd.lz
            512000,

            // virtualbox_macos_fixed_10mb.vhd.lz
            20480,

            // virtualbox_windows_dynamic_250mb.vhd.lz
            512000,

            // virtualbox_windows_fixed_10mb.vhd.lz
            20480
        };

        readonly uint[] _sectorSize =
        {
            // virtualbox_linux_dynamic_250mb.vhd.lz
            512,

            // virtualbox_linux_fixed_10mb.vhd.lz
            512,

            // virtualbox_macos_dynamic_250mb.vhd.lz
            512,

            // virtualbox_macos_fixed_10mb.vhd.lz
            512,

            // virtualbox_windows_dynamic_250mb.vhd.lz
            512,

            // virtualbox_windows_fixed_10mb.vhd.lz
            512
        };

        readonly MediaType[] _mediaTypes =
        {
            // virtualbox_linux_dynamic_250mb.vhd.lz
            MediaType.Unknown,

            // virtualbox_linux_fixed_10mb.vhd.lz
            MediaType.Unknown,

            // virtualbox_macos_dynamic_250mb.vhd.lz
            MediaType.Unknown,

            // virtualbox_macos_fixed_10mb.vhd.lz
            MediaType.Unknown,

            // virtualbox_windows_dynamic_250mb.vhd.lz
            MediaType.Unknown,

            // virtualbox_windows_fixed_10mb.vhd.lz
            MediaType.Unknown
        };

        readonly string[] _md5S =
        {
            // virtualbox_linux_dynamic_250mb.vhd.lz
            "f968f0e74dd1b254de9eac589a5d687d",

            // virtualbox_linux_fixed_10mb.vhd.lz
            "f1c9645dbc14efddc7d8a322685f26eb",

            // virtualbox_macos_dynamic_250mb.vhd.lz
            "09d3dce9e60e9d1a997ad3f04d33c8c5",

            // virtualbox_macos_fixed_10mb.vhd.lz
            "f1c9645dbc14efddc7d8a322685f26eb",

            // virtualbox_windows_dynamic_250mb.vhd.lz
            "284af271786e7def9bf8af7c2da1c4f2",

            // virtualbox_windows_fixed_10mb.vhd.lz
            "f1c9645dbc14efddc7d8a322685f26eb"
        };

        readonly string _dataFolder =
            Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "VirtualBox", "VirtualPC");

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

                    var  image  = new Vhd();
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

                    var   image       = new Vhd();
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