// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 2MG.cs
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
    public class Apple2Mg
    {
        readonly string[] _testFiles =
        {
            "blank140.2mg.lz", "dos32.2mg.lz", "dos32_alt.2mg.lz", "dos33_dic.2mg.lz", "dos33-do.2mg.lz",
            "dos33-nib.2mg.lz", "dos33_nib.2mg.lz", "dos33-po.2mg.lz", "dos33_po.2mg.lz", "hfs1440.2mg.lz",
            "hfs800_dic.2mg.lz", "hfs_do.2mg.lz", "hfs_po.2mg.lz", "modified_do.2mg.lz", "modified_po.2mg.lz",
            "pascal800_do.2mg.lz", "pascal800_p.2mg.lz", "pascal_dic.2mg.lz", "pascal_do.2mg.lz", "pascal_nib.2mg.lz",
            "pascal_po.2mg.lz", "prodos1440.2mg.lz", "prodos1440_po.2mg.lz", "prodos5mb.2mg.lz", "prodos5m_dic.2mg.lz",
            "prodos800_dic.2mg.lz", "prodos800_do.2mg.lz", "prodos800_po.2mg.lz", "prodos_dic.2mg.lz",
            "prodos_do.2mg.lz", "prodos_nib.2mg.lz", "prodos_po.2mg.lz", "prodos.2mg.lz"
        };

        readonly ulong[] _sectors =
        {
            560, 455, 455, 560, 560, 560, 560, 560, 560, 2880, 1600, 1600, 1600, 1600, 1600, 1600, 1600, 560, 560, 560,
            560, 2880, 2880, 10240, 10240, 1600, 1600, 1600, 560, 560, 560, 560, 560
        };

        readonly uint[] _sectorSize =
        {
            256, 256, 256, 256, 256, 256, 256, 256, 256, 512, 512, 512, 512, 512, 512, 512, 512, 256, 256, 256, 256,
            512, 512, 512, 512, 512, 512, 512, 256, 256, 256, 256, 256
        };

        readonly MediaType[] _mediaTypes =
        {
            MediaType.Apple33SS, MediaType.Apple32SS, MediaType.Apple32SS, MediaType.Apple33SS, MediaType.Apple33SS,
            MediaType.Apple33SS, MediaType.Apple33SS, MediaType.Apple33SS, MediaType.Apple33SS, MediaType.DOS_35_HD,
            MediaType.AppleSonyDS, MediaType.AppleSonyDS, MediaType.AppleSonyDS, MediaType.AppleSonyDS,
            MediaType.AppleSonyDS, MediaType.AppleSonyDS, MediaType.AppleSonyDS, MediaType.Apple33SS,
            MediaType.Apple33SS, MediaType.Apple33SS, MediaType.Apple33SS, MediaType.DOS_35_HD, MediaType.DOS_35_HD,
            MediaType.Unknown, MediaType.Unknown, MediaType.AppleSonyDS, MediaType.AppleSonyDS, MediaType.AppleSonyDS,
            MediaType.Apple33SS, MediaType.Apple33SS, MediaType.Apple33SS, MediaType.Apple33SS, MediaType.Apple33SS
        };

        readonly string[] _md5S =
        {
            "7db5d585270ab858043d50e60068d45f", "906c1bdbf76bf089ea47aae98151df5d", "76f8fe4c5bc1976f99641ad7cdf53109",
            "0ffcbd4180306192726926b43755db2f", "91d020725d081500caa1fd8aad959397", "91d020725d081500caa1fd8aad959397",
            "0ffcbd4180306192726926b43755db2f", "91d020725d081500caa1fd8aad959397", "0ffcbd4180306192726926b43755db2f",
            "535648d1f9838b695403f2f48d5ac94c", "2762f41d0379b476042fc62891baac84", "2762f41d0379b476042fc62891baac84",
            "2762f41d0379b476042fc62891baac84", "b748f6df3e60e7169d42ec6fcc857ea4", "b748f6df3e60e7169d42ec6fcc857ea4",
            "dbd0ec8a3126236910709faf923adcf2", "dbd0ec8a3126236910709faf923adcf2", "4c4926103a32ac15f7e430ec3ced4be5",
            "4c4926103a32ac15f7e430ec3ced4be5", "4c4926103a32ac15f7e430ec3ced4be5", "4c4926103a32ac15f7e430ec3ced4be5",
            "eb9b60c78b30d2b6541ed0781944b6da", "1fe841b418ede51133878641e01544b5", "b156441e159a625ee00a0659dfb6e2f8",
            "b156441e159a625ee00a0659dfb6e2f8", "fcf747bd356b48d442ff74adb8f3516b", "fcf747bd356b48d442ff74adb8f3516b",
            "fcf747bd356b48d442ff74adb8f3516b", "11ef56c80c94347d2e3f921d5c36c8de", "11ef56c80c94347d2e3f921d5c36c8de",
            "11ef56c80c94347d2e3f921d5c36c8de", "11ef56c80c94347d2e3f921d5c36c8de", "6f692a8fadfaa243d9f2d8d41f0e4cad"
        };

        readonly string _dataFolder = Path.Combine(Consts.TEST_FILES_ROOT, "Media image formats", "2mg");

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

                    var  image  = new DiscImages.Apple2Mg();
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

                    var   image       = new DiscImages.Apple2Mg();
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