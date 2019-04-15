// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : FATX.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef unit testing.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using DiscImageChef.Checksums;
using DiscImageChef.CommonTypes;
using DiscImageChef.CommonTypes.Interfaces;
using DiscImageChef.CommonTypes.Structs;
using DiscImageChef.DiscImages;
using DiscImageChef.Filesystems.FATX;
using DiscImageChef.Filters;
using NUnit.Framework;
using FileAttributes = DiscImageChef.CommonTypes.Structs.FileAttributes;
using FileSystemInfo = DiscImageChef.CommonTypes.Structs.FileSystemInfo;

namespace DiscImageChef.Tests.Filesystems
{
    [TestFixture]
    public class XboxFat
    {
        string              location;
        IFilter             filter;
        IMediaImage         image;
        IReadOnlyFilesystem fs;
        Partition           wholePart;

        [SetUp]
        public void Init()
        {
            location = Path.Combine(Consts.TestFilesRoot, "filesystems", "fatx16", "le", "fatx.img.lz");
            filter   = new LZip();
            filter.Open(location);
            image = new ZZZRawImage();
            Assert.AreEqual(true, image.Open(filter));
            fs = new XboxFatPlugin();
            wholePart = new Partition
            {
                Name   = "Whole device",
                Length = image.Info.Sectors,
                Size   = image.Info.Sectors * image.Info.SectorSize
            };

            Errno error = fs.Mount(image, wholePart, null, null);
            Assert.AreEqual(Errno.NoError, error);
        }

        [Test]
        public void Information()
        {
            Assert.AreEqual(62720,             image.Info.Sectors);
            Assert.AreEqual(1960,              fs.XmlFsType.Clusters);
            Assert.AreEqual(16384,             fs.XmlFsType.ClusterSize);
            Assert.AreEqual("FATX filesystem", fs.XmlFsType.Type);
            Assert.AreEqual("Volume láb€l",    fs.XmlFsType.VolumeName);
            Assert.AreEqual("4639B7D0",        fs.XmlFsType.VolumeSerial);
        }

        [Test]
        public void RootDirectory()
        {
            Errno error = fs.ReadDir("", out List<string> directory);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(6,             directory.Count);

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
        public void SubDirectory()
        {
            Errno error = fs.ReadDir("49470015", out List<string> directory);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(4,             directory.Count);

            Assert.AreEqual(true, directory.Contains("TitleImage.xbx"));
            Assert.AreEqual(true, directory.Contains("SaveImage.xbx"));
            Assert.AreEqual(true, directory.Contains("7AC2FE88C908"));
            Assert.AreEqual(true, directory.Contains("TitleMeta.xbx"));

            Assert.AreEqual(false, directory.Contains("TitleImage"));
            Assert.AreEqual(false, directory.Contains(".xbx"));
            Assert.AreEqual(false, directory.Contains("7ac2fe88c908"));
            Assert.AreEqual(false, directory.Contains("xbx"));

            error = fs.ReadDir("49470015/7AC2FE88C908", out directory);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(3,             directory.Count);

            Assert.AreEqual(true, directory.Contains("SaveMeta.xbx"));
            Assert.AreEqual(true, directory.Contains("savedata.dat"));
            Assert.AreEqual(true, directory.Contains("saveimage.xbx"));

            Assert.AreEqual(false, directory.Contains("savemeta.xbx"));
            Assert.AreEqual(false, directory.Contains("SaveData.dat"));
            Assert.AreEqual(false, directory.Contains("SaveImage.xbx"));
        }

        [Test]
        public void Statfs()
        {
            Errno error = fs.StatFs(out FileSystemInfo stat);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(1960,          stat.Blocks);
            Assert.AreEqual(42,            stat.FilenameLength);
            Assert.AreEqual(0,             stat.Files);
            Assert.AreEqual(0,             stat.FreeBlocks);
            Assert.AreEqual(0,             stat.FreeFiles);
            Assert.AreEqual(0x58544146,    stat.Id.Serial32);
            Assert.AreEqual("Xbox FAT",    stat.Type);
        }

        [Test]
        public void MapBlock()
        {
            Errno error = fs.MapBlock("49470015", 0, out long block);
            Assert.AreEqual(Errno.IsDirectory, error);

            error = fs.MapBlock("49470015/TitleImage", 0, out block);
            Assert.AreEqual(Errno.NoSuchFile, error);

            error = fs.MapBlock("49470015/TitleImage.xbx", 0, out block);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(80,            block);

            error = fs.MapBlock("49470015/7AC2FE88C908/savedata.dat", 2, out block);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(272,           block);

            error = fs.MapBlock("49470015/7AC2FE88C908/savedata.dat", 200, out block);
            Assert.AreEqual(Errno.InvalidArgument, error);
        }

        [Test]
        public void Stat()
        {
            Errno error = fs.Stat("49470015", out FileEntryInfo stat);
            Assert.AreEqual(Errno.NoError,                                         error);
            Assert.AreEqual(new DateTime(2007, 3, 6, 15, 8, 44, DateTimeKind.Utc), stat.AccessTimeUtc);
            Assert.AreEqual(FileAttributes.Directory,                              stat.Attributes);
            Assert.AreEqual(DateTime.MinValue,                                     stat.BackupTimeUtc);
            Assert.AreEqual(1,                                                     stat.Blocks);
            Assert.AreEqual(16384,                                                 stat.BlockSize);
            Assert.AreEqual(new DateTime(2007, 3, 6, 15, 8, 44, DateTimeKind.Utc), stat.CreationTimeUtc);
            Assert.AreEqual(0,                                                     stat.DeviceNo);
            Assert.AreEqual(0,                                                     stat.GID);
            Assert.AreEqual(2,                                                     stat.Inode);
            Assert.AreEqual(new DateTime(2007, 3, 6, 15, 8, 44, DateTimeKind.Utc), stat.LastWriteTimeUtc);
            Assert.AreEqual(16384,                                                 stat.Length);
            Assert.AreEqual(1,                                                     stat.Links);
            Assert.AreEqual(365,                                                   stat.Mode);
            Assert.AreEqual(DateTime.MinValue,                                     stat.StatusChangeTimeUtc);
            Assert.AreEqual(0,                                                     stat.UID);

            error = fs.Stat("49470015/TitleImage", out stat);
            Assert.AreEqual(Errno.NoSuchFile, error);

            error = fs.Stat("49470015/TitleImage.xbx", out stat);
            Assert.AreEqual(Errno.NoError,                                          error);
            Assert.AreEqual(new DateTime(2013, 5, 14, 12, 50, 8, DateTimeKind.Utc), stat.AccessTimeUtc);
            Assert.AreEqual(FileAttributes.None,                                    stat.Attributes);
            Assert.AreEqual(DateTime.MinValue,                                      stat.BackupTimeUtc);
            Assert.AreEqual(1,                                                      stat.Blocks);
            Assert.AreEqual(16384,                                                  stat.BlockSize);
            Assert.AreEqual(new DateTime(2013, 5, 14, 12, 50, 8, DateTimeKind.Utc), stat.CreationTimeUtc);
            Assert.AreEqual(0,                                                      stat.DeviceNo);
            Assert.AreEqual(0,                                                      stat.GID);
            Assert.AreEqual(3,                                                      stat.Inode);
            Assert.AreEqual(new DateTime(2013, 5, 14, 12, 50, 8, DateTimeKind.Utc), stat.LastWriteTimeUtc);
            Assert.AreEqual(10240,                                                  stat.Length);
            Assert.AreEqual(1,                                                      stat.Links);
            Assert.AreEqual(292,                                                    stat.Mode);
            Assert.AreEqual(DateTime.MinValue,                                      stat.StatusChangeTimeUtc);
            Assert.AreEqual(0,                                                      stat.UID);
        }

        [Test]
        public void Read()
        {
            byte[] buffer = new byte[0];
            Errno  error  = fs.Read("49470015", 0, 0, ref buffer);
            Assert.AreEqual(Errno.IsDirectory, error);

            error = fs.Read("49470015/TitleImage", 0, 0, ref buffer);
            Assert.AreEqual(Errno.NoSuchFile, error);

            error = fs.Read("49470015/7AC2FE88C908/savedata.dat", 0, 0, ref buffer);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(0,             buffer.Length);
            Assert.AreEqual("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855",
                            Sha256Context.Data(buffer, out _));

            error = fs.Read("49470015/7AC2FE88C908/savedata.dat", 1, 16, ref buffer);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(16,            buffer.Length);
            Assert.AreEqual("ff82559d2d0c610ac25b78dcb53a8312e32b56192044deb1f01540581bd54e80",
                            Sha256Context.Data(buffer, out _));

            error = fs.Read("49470015/7AC2FE88C908/savedata.dat", 248, 131072, ref buffer);
            Assert.AreEqual(Errno.NoError, error);
            Assert.AreEqual(61996,         buffer.Length);
            Assert.AreEqual("2eb0d62a96ad28473ce0dd67052efdfae31f371992e1d8309beeeff6f2b46a59",
                            Sha256Context.Data(buffer, out _));

            error = fs.Read("49470015/7AC2FE88C908/savedata.dat", 131072, 0, ref buffer);
            Assert.AreEqual(Errno.InvalidArgument, error);
        }

        [TearDown]
        public void Destroy()
        {
            fs?.Unmount();
            fs = null;
        }
    }
}