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
    public class Xbox360 : FilesystemTest
    {
        public Xbox360() : base("FATX filesystem") {}

        public override string DataFolder => Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Xbox FAT16", "be");

        public override IFilesystem Plugin     => new XboxFatPlugin();
        public override bool        Partitions => true;

        [SetUp]
        public void Init()
        {
            _location = Path.Combine(Consts.TEST_FILES_ROOT, "Filesystems", "Xbox FAT16", "be",
                                     "microsoft256mb.img.lz");

            _filter = new LZip();
            _filter.Open(_location);
            _image = new ZZZRawImage();
            Assert.AreEqual(true, _image.Open(_filter));
            _fs = new XboxFatPlugin();
            List<Partition> partitions = Core.Partitions.GetAll(_image);
            Assert.AreEqual(2, partitions.Count);
            _dataPartition = partitions[1];
            Errno error = _fs.Mount(_image, _dataPartition, null, null, null);
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
        Partition           _dataPartition;

        [Test]
        public void MapBlock()
        {
            Errno error = _fs.MapBlock("Content/0000000000000000/FFFE07DF/00040000", 0, out long block);
            Assert.AreEqual(Errno.IsDirectory, error);

            error = _fs.MapBlock("Content/0000000000000000/FFFE07DF/00040000/ContentCache", 0, out block);
            Assert.AreEqual(Errno.NoSuchFile, error);

            error = _fs.MapBlock("Content/0000000000000000/FFFE07DF/00040000/ContentCache.pkg", 0, out block);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(16992, block);

            error = _fs.MapBlock("Content/0000000000000000/FFFE07DF/00040000/ContentCache.pkg", 2, out block);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(17056, block);

            error = _fs.MapBlock("Content/0000000000000000/FFFE07DF/00040000/ContentCache.pkg", 2000000, out block);
            Assert.AreEqual(Errno.InvalidArgument, error);
        }

        [Test]
        public void Read()
        {
            byte[] buffer = new byte[0];
            Errno  error  = _fs.Read("Content/0000000000000000/FFFE07DF/00040000", 0, 0, ref buffer);
            Assert.AreEqual(Errno.IsDirectory, error);

            error = _fs.Read("Content/0000000000000000/FFFE07DF/00040000/ContentCache", 0, 0, ref buffer);
            Assert.AreEqual(Errno.NoSuchFile, error);

            error = _fs.Read("Content/0000000000000000/FFFE07DF/00040000/ContentCache.pkg", 0, 0, ref buffer);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(0, buffer.Length);

            Assert.AreEqual("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
                            Sha256Context.Data(buffer, out _));

            error = _fs.Read("Content/0000000000000000/FFFE07DF/00040000/ContentCache.pkg", 1, 16, ref buffer);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(16, buffer.Length);

            Assert.AreEqual("f73a941675b8df16b0fc908f242c3c51382c5b159e709e0f9ffc1e5aac35f77d",
                            Sha256Context.Data(buffer, out _));

            error = _fs.Read("Content/0000000000000000/FFFE07DF/00040000/ContentCache.pkg", 248, 131072, ref buffer);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(85768, buffer.Length);

            Assert.AreEqual("19caf1365e1b7d5446ca0c2518d15e94c3ab0faaf2f8f3b31c9e1656dff57bd9",
                            Sha256Context.Data(buffer, out _));

            error = _fs.Read("Content/0000000000000000/FFFE07DF/00040000/ContentCache.pkg", 131072, 0, ref buffer);
            Assert.AreEqual(Errno.InvalidArgument, error);
        }

        [Test]
        public void RootDirectory()
        {
            Errno error = _fs.ReadDir("", out List<string> directory);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(2, directory.Count);

            Assert.AreEqual(true, directory.Contains("name.txt"));
            Assert.AreEqual(true, directory.Contains("Content"));

            Assert.AreEqual(false, directory.Contains("name"));
            Assert.AreEqual(false, directory.Contains("content"));
        }

        [Test]
        public void Stat()
        {
            Errno error = _fs.Stat("Content/0000000000000000/FFFE07DF/00040000", out FileEntryInfo stat);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(new DateTime(2013, 9, 25, 12, 49, 46, DateTimeKind.Utc), stat.AccessTimeUtc);
            Assert.AreEqual(FileAttributes.Directory, stat.Attributes);
            Assert.AreEqual(null, stat.BackupTimeUtc);
            Assert.AreEqual(1, stat.Blocks);
            Assert.AreEqual(16384, stat.BlockSize);
            Assert.AreEqual(new DateTime(2013, 9, 25, 12, 49, 46, DateTimeKind.Utc), stat.CreationTimeUtc);
            Assert.AreEqual(null, stat.DeviceNo);
            Assert.AreEqual(null, stat.GID);
            Assert.AreEqual(12, stat.Inode);
            Assert.AreEqual(new DateTime(2013, 9, 25, 12, 49, 46, DateTimeKind.Utc), stat.LastWriteTimeUtc);
            Assert.AreEqual(16384, stat.Length);
            Assert.AreEqual(1, stat.Links);
            Assert.AreEqual(null, stat.Mode);
            Assert.AreEqual(null, stat.StatusChangeTimeUtc);
            Assert.AreEqual(null, stat.UID);

            error = _fs.Stat("Content/0000000000000000/FFFE07DF/00040000/ContentCache", out stat);
            Assert.AreEqual(Errno.NoSuchFile, error);

            error = _fs.Stat("Content/0000000000000000/FFFE07DF/00040000/ContentCache.pkg", out stat);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(new DateTime(2016, 11, 18, 20, 34, 48, DateTimeKind.Utc), stat.AccessTimeUtc);
            Assert.AreEqual(FileAttributes.None, stat.Attributes);
            Assert.AreEqual(null, stat.BackupTimeUtc);
            Assert.AreEqual(6, stat.Blocks);
            Assert.AreEqual(16384, stat.BlockSize);
            Assert.AreEqual(new DateTime(2016, 11, 18, 20, 34, 48, DateTimeKind.Utc), stat.CreationTimeUtc);
            Assert.AreEqual(null, stat.DeviceNo);
            Assert.AreEqual(null, stat.GID);
            Assert.AreEqual(18, stat.Inode);
            Assert.AreEqual(new DateTime(2016, 11, 18, 20, 34, 48, DateTimeKind.Utc), stat.LastWriteTimeUtc);
            Assert.AreEqual(86016, stat.Length);
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
            Assert.AreEqual(14848, stat.Blocks);
            Assert.AreEqual(42, stat.FilenameLength);
            Assert.AreEqual(0, stat.Files);
            Assert.AreEqual(0, stat.FreeBlocks);
            Assert.AreEqual(0, stat.FreeFiles);
            Assert.AreEqual(0x58544146, stat.Id.Serial32);
            Assert.AreEqual("Xbox 360 FAT", stat.Type);
        }

        [Test]
        public void SubDirectory()
        {
            Errno error = _fs.ReadDir("Content", out List<string> directory);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(2, directory.Count);

            Assert.AreEqual(true, directory.Contains("0000000000000000"));
            Assert.AreEqual(true, directory.Contains("EBA4FD1C82295965"));

            Assert.AreEqual(false, directory.Contains("000000000000000"));
            Assert.AreEqual(false, directory.Contains("eba4FD1C82295965"));

            error = _fs.ReadDir("Content/EBA4FD1C82295965/454108CF/00000001", out directory);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(3, directory.Count);

            Assert.AreEqual(true, directory.Contains("DICP1-ES-PROFILE000000000000000"));
            Assert.AreEqual(true, directory.Contains("DI1-ES-117218325276118302153627"));
            Assert.AreEqual(true, directory.Contains("DI1-ES-120060617014626142312477"));

            Assert.AreEqual(false, directory.Contains("DICP1-es-PROFILE000000000000000"));
            Assert.AreEqual(false, directory.Contains("di1-ES-117218325276118302153627"));
            Assert.AreEqual(false, directory.Contains("DI1ES120060617014626142312477"));
        }

        public override FileSystemTest[] Tests => new[]
        {
            new FileSystemTest
            {
                TestFile     = "microsoft256mb.img.lz",
                MediaType    = MediaType.GENERIC_HDD,
                Sectors      = 491520,
                SectorSize   = 512,
                Clusters     = 14848,
                ClusterSize  = 16384,
                VolumeName   = "",
                VolumeSerial = "66C2E9D0"
            }
        };
    }
}