// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FATX.cs
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
using System.Collections.Generic;
using System.IO;
using Aaru.Checksums;
using Aaru.CommonTypes;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.DiscImages;
using Aaru.Filesystems;
using Aaru.Filters;
using NUnit.Framework;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;
using FileSystemInfo = Aaru.CommonTypes.Structs.FileSystemInfo;

namespace Aaru.Tests.Filesystems.FATX
{
    [TestFixture]
    public class Xbox : FilesystemTest
    {
        public Xbox() : base("FATX filesystem") {}

        public override string _dataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Xbox FAT16", "le");
        public override IFilesystem _plugin => new XboxFatPlugin();
        public override bool _partitions => false;

        [SetUp]
        public void Init()
        {
            _location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Xbox FAT16", "le", "fatx.img.lz");
            _filter   = new LZip();
            _filter.Open(_location);
            _image = new ZZZRawImage();
            Assert.AreEqual(true, _image.Open(_filter));
            _fs = new XboxFatPlugin();

            _wholePart = new Partition
            {
                Name   = "Whole device",
                Length = _image.Info.Sectors,
                Size   = _image.Info.Sectors * _image.Info.SectorSize
            };

            Errno error = _fs.Mount(_image, _wholePart, null, null, null);
            Assert.AreEqual(Errno.NoError, error);
        }

        [TearDown]
        public void Destroy()
        {
            _fs?.Unmount();
            _fs = null;
        }

        string              _location;
        IFilter             _filter;
        IMediaImage         _image;
        IReadOnlyFilesystem _fs;
        Partition           _wholePart;

        [Test]
        public void MapBlock()
        {
            Errno error = _fs.MapBlock("49470015", 0, out long block);
            Assert.AreEqual(Errno.IsDirectory, error);

            error = _fs.MapBlock("49470015/TitleImage", 0, out block);
            Assert.AreEqual(Errno.NoSuchFile, error);

            error = _fs.MapBlock("49470015/TitleImage.xbx", 0, out block);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(80, block);

            error = _fs.MapBlock("49470015/7AC2FE88C908/savedata.dat", 2, out block);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(272, block);

            error = _fs.MapBlock("49470015/7AC2FE88C908/savedata.dat", 200, out block);
            Assert.AreEqual(Errno.InvalidArgument, error);
        }

        [Test]
        public void Read()
        {
            byte[] buffer = new byte[0];
            Errno  error  = _fs.Read("49470015", 0, 0, ref buffer);
            Assert.AreEqual(Errno.IsDirectory, error);

            error = _fs.Read("49470015/TitleImage", 0, 0, ref buffer);
            Assert.AreEqual(Errno.NoSuchFile, error);

            error = _fs.Read("49470015/7AC2FE88C908/savedata.dat", 0, 0, ref buffer);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(0, buffer.Length);

            Assert.AreEqual("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
                            Sha256Context.Data(buffer, out _));

            error = _fs.Read("49470015/7AC2FE88C908/savedata.dat", 1, 16, ref buffer);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(16, buffer.Length);

            Assert.AreEqual("ff82559d2d0c610ac25b78dcb53a8312e32b56192044deb1f01540581bd54e80",
                            Sha256Context.Data(buffer, out _));

            error = _fs.Read("49470015/7AC2FE88C908/savedata.dat", 248, 131072, ref buffer);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(61996, buffer.Length);

            Assert.AreEqual("2eb0d62a96ad28473ce0dd67052efdfae31f371992e1d8309beeeff6f2b46a59",
                            Sha256Context.Data(buffer, out _));

            error = _fs.Read("49470015/7AC2FE88C908/savedata.dat", 131072, 0, ref buffer);
            Assert.AreEqual(Errno.InvalidArgument, error);
        }

        [Test]
        public void RootDirectory()
        {
            Errno error = _fs.ReadDir("", out List<string> directory);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(6, directory.Count);

            Assert.AreEqual(true, directory.Contains("49470015"));
            Assert.AreEqual(true, directory.Contains("4d5300d1"));
            Assert.AreEqual(true, directory.Contains("4d53006e"));
            Assert.AreEqual(true, directory.Contains("4d530004"));
            Assert.AreEqual(true, directory.Contains("4947007c"));
            Assert.AreEqual(true, directory.Contains("4541003e"));

            Assert.AreEqual(false, directory.Contains("d530004"));
            Assert.AreEqual(false, directory.Contains("4947007"));
        }

        [Test]
        public void Stat()
        {
            Errno error = _fs.Stat("49470015", out FileEntryInfo stat);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(new DateTime(2007, 3, 6, 15, 8, 44, DateTimeKind.Utc), stat.AccessTimeUtc);
            Assert.AreEqual(FileAttributes.Directory, stat.Attributes);
            Assert.AreEqual(null, stat.BackupTimeUtc);
            Assert.AreEqual(1, stat.Blocks);
            Assert.AreEqual(16384, stat.BlockSize);
            Assert.AreEqual(new DateTime(2007, 3, 6, 15, 8, 44, DateTimeKind.Utc), stat.CreationTimeUtc);
            Assert.AreEqual(null, stat.DeviceNo);
            Assert.AreEqual(null, stat.GID);
            Assert.AreEqual(2, stat.Inode);
            Assert.AreEqual(new DateTime(2007, 3, 6, 15, 8, 44, DateTimeKind.Utc), stat.LastWriteTimeUtc);
            Assert.AreEqual(16384, stat.Length);
            Assert.AreEqual(1, stat.Links);
            Assert.AreEqual(null, stat.Mode);
            Assert.AreEqual(null, stat.StatusChangeTimeUtc);
            Assert.AreEqual(null, stat.UID);

            error = _fs.Stat("49470015/TitleImage", out stat);
            Assert.AreEqual(Errno.NoSuchFile, error);

            error = _fs.Stat("49470015/TitleImage.xbx", out stat);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(new DateTime(2013, 5, 14, 12, 50, 8, DateTimeKind.Utc), stat.AccessTimeUtc);
            Assert.AreEqual(FileAttributes.None, stat.Attributes);
            Assert.AreEqual(null, stat.BackupTimeUtc);
            Assert.AreEqual(1, stat.Blocks);
            Assert.AreEqual(16384, stat.BlockSize);
            Assert.AreEqual(new DateTime(2013, 5, 14, 12, 50, 8, DateTimeKind.Utc), stat.CreationTimeUtc);
            Assert.AreEqual(null, stat.DeviceNo);
            Assert.AreEqual(null, stat.GID);
            Assert.AreEqual(3, stat.Inode);
            Assert.AreEqual(new DateTime(2013, 5, 14, 12, 50, 8, DateTimeKind.Utc), stat.LastWriteTimeUtc);
            Assert.AreEqual(10240, stat.Length);
            Assert.AreEqual(1, stat.Links);
            Assert.AreEqual(null, stat.Mode);
            Assert.AreEqual(null, stat.StatusChangeTimeUtc);
            Assert.AreEqual(null, stat.UID);
        }

        [Test]
        public void Statfs()
        {
            Errno error = _fs.StatFs(out FileSystemInfo stat);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(1960, stat.Blocks);
            Assert.AreEqual(42, stat.FilenameLength);
            Assert.AreEqual(0, stat.Files);
            Assert.AreEqual(0, stat.FreeBlocks);
            Assert.AreEqual(0, stat.FreeFiles);
            Assert.AreEqual(0x58544146, stat.Id.Serial32);
            Assert.AreEqual("Xbox FAT", stat.Type);
        }

        [Test]
        public void SubDirectory()
        {
            Errno error = _fs.ReadDir("49470015", out List<string> directory);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(4, directory.Count);

            Assert.AreEqual(true, directory.Contains("TitleImage.xbx"));
            Assert.AreEqual(true, directory.Contains("SaveImage.xbx"));
            Assert.AreEqual(true, directory.Contains("7AC2FE88C908"));
            Assert.AreEqual(true, directory.Contains("TitleMeta.xbx"));

            Assert.AreEqual(false, directory.Contains("TitleImage"));
            Assert.AreEqual(false, directory.Contains(".xbx"));
            Assert.AreEqual(false, directory.Contains("7ac2fe88c908"));
            Assert.AreEqual(false, directory.Contains("xbx"));

            error = _fs.ReadDir("49470015/7AC2FE88C908", out directory);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(3, directory.Count);

            Assert.AreEqual(true, directory.Contains("SaveMeta.xbx"));
            Assert.AreEqual(true, directory.Contains("savedata.dat"));
            Assert.AreEqual(true, directory.Contains("saveimage.xbx"));

            Assert.AreEqual(false, directory.Contains("savemeta.xbx"));
            Assert.AreEqual(false, directory.Contains("SaveData.dat"));
            Assert.AreEqual(false, directory.Contains("SaveImage.xbx"));
        }

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile     = "fatx.img.lz",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 62720,
                SectorSize   = 512,
                Clusters     = 1960,
                ClusterSize  = 16384,
                VolumeName   = "Volume láb€l",
                VolumeSerial = "4639B7D0"
            }
        };
    }
}