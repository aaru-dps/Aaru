// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ISO9660Rrip.cs
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Filesystems;
using Aaru.Filters;
using Aaru.Images;
using FluentAssertions;
using NUnit.Framework;
using FileAttributes = Aaru.CommonTypes.Structs.FileAttributes;
using Partition = Aaru.CommonTypes.Partition;

namespace Aaru.Tests.Filesystems;

/// <summary>
///     Checks the Rock Ridge Interchange Protocol support of the ISO9660 plugin against images with known contents.
/// </summary>
/// <remarks>
///     <para>
///         xorriso_rrip.iso (RRIP 1.12) and xorriso_rrip_1.10.iso (RRIP 1.10, long form "TF") were created with
///         xorriso 1.5.8 running with TZ=Asia/Kolkata, -padding 0 and -compliance deep_paths_off:always_gmt_off, plus
///         new_rr or old_rr:rrip_tf_long. Without padding they are not mistaken for Super Nintendo ROMs, which can be
///         512KiB. Both have a Joliet tree and relocate /a/b/c/d/e/f/g/h to rr_moved. Every file belongs
///         to 1234:5678, has an access time of 2002-03-04 05:06:07 UTC, an attribute change time of 2003-04-05
///         06:07:08 UTC and a modification time of 2001-02-03 04:05:06 UTC, except /a/b/c/d/e/f/g/h, modified on
///         1999-12-31 23:59:58 UTC.
///     </para>
///     <para>
///         xorriso_rrip_sparse.iso was created with xorriso too, then the "TF" fields of /sp112.bin and /sp110.bin were
///         replaced by hand with RRIP 1.12 and RRIP 1.10 "SF" fields, as no mastering tool writes them. Both encode
///         the same 300000 bytes file: 64KiB of data, 64KiB alternating 256 bytes of data and zeros, 128KiB of zeros
///         and 37856 bytes of data.
///     </para>
/// </remarks>
[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class Iso9660Rrip
{
    const string RRIP_112 = "xorriso_rrip.iso";
    const string RRIP_110 = "xorriso_rrip_1.10.iso";
    const string SPARSE   = "xorriso_rrip_sparse.iso";

    const string LONG_NAME =
        "Long_Mixed_Case_Name_abcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghijabcdefghij.txt";

    const string SPARSE_SHA256          = "B838630B559729FB18E0CB4D562067D1B80154E63EB1EB1C37D0950C4AABC76C";
    const string SPARSE_PARTIAL_SHA256  = "2AE5E6387B3A171E68E302CED0CD6E7C677EEC494F21016C422EFA99063710DD";
    const long   SPARSE_SIZE            = 300000;
    const long   SPARSE_PARTIAL_OFFSET  = 60000;
    const long   SPARSE_PARTIAL_LENGTH  = 80000;
    const ulong  OWNER_UID              = 1234;
    const ulong  OWNER_GID              = 5678;
    const ulong  DEV_NULL               = 0x103;
    static readonly DateTime _modified       = new(2001, 2, 3, 4, 5, 6, DateTimeKind.Utc);
    static readonly DateTime _accessed       = new(2002, 3, 4, 5, 6, 7, DateTimeKind.Utc);
    static readonly DateTime _changed        = new(2003, 4, 5, 6, 7, 8, DateTimeKind.Utc);
    static readonly DateTime _relocatedMtime = new(1999, 12, 31, 23, 59, 58, DateTimeKind.Utc);

    static string DataFolder => Path.Combine(Consts.TestFilesRoot, "Filesystems", "ISO9660");

    static (ISO9660 fs, IMediaImage image) Mount(string testFile, string @namespace = "rrip")
    {
        string path = Path.Combine(DataFolder, testFile);

        if(!File.Exists(path)) Assert.Ignore($"{path} not found");

        var filter = new ZZZNoFilter();
        filter.Open(path).Should().Be(ErrorNumber.NoError);

        var image = new ZZZRawImage();
        image.Open(filter).Should().Be(ErrorNumber.NoError);

        var partition = new Partition
        {
            Name   = "Whole device",
            Length = image.Info.Sectors,
            Size   = image.Info.Sectors * image.Info.SectorSize
        };

        var fs = new ISO9660();

        fs.Mount(image, partition, Encoding.ASCII, [], @namespace)
          .Should()
          .Be(ErrorNumber.NoError);

        return (fs, image);
    }

    static void Unmount(ISO9660 fs, IMediaImage image)
    {
        fs.Unmount();
        FilesystemTest.DisposeImage(image);
    }

    static FileEntryInfo Stat(ISO9660 fs, string path)
    {
        fs.Stat(path, out FileEntryInfo stat).Should().Be(ErrorNumber.NoError, path);

        return stat;
    }

    static List<string> ReadDir(ISO9660 fs, string path)
    {
        List<string> entries = [];

        fs.OpenDir(path, out IDirNode node).Should().Be(ErrorNumber.NoError, path);

        while(fs.ReadDir(node, out string name) == ErrorNumber.NoError && name is not null) entries.Add(name);

        fs.CloseDir(node);

        return entries;
    }

    static byte[] ReadFile(ISO9660 fs, string path, long offset, long length, int chunk)
    {
        fs.OpenFile(path, out IFileNode node).Should().Be(ErrorNumber.NoError, path);

        node.Offset = offset;

        var  contents = new byte[length];
        var  buffer   = new byte[chunk];
        long done     = 0;

        while(done < length)
        {
            fs.ReadFile(node, Math.Min(chunk, length - done), buffer, out long read)
              .Should()
              .Be(ErrorNumber.NoError, path);

            if(read <= 0) break;

            Array.Copy(buffer, 0, contents, done, read);
            done += read;
        }

        fs.CloseFile(node);

        done.Should().Be(length, path);

        return contents;
    }

    [TestCase(RRIP_112)]
    [TestCase(RRIP_110)]
    public void AlternateNames(string testFile)
    {
        (ISO9660 fs, IMediaImage image) = Mount(testFile);

        try
        {
            // The Joliet tree, also on these images, cannot hold such a long name nor knows about relocations
            ReadDir(fs, "/")
               .Should()
               .BeEquivalentTo("a",
                               "devnull",
                               "fifo",
                               "file.txt",
                               "links",
                               LONG_NAME,
                               "name with spaces.txt",
                               "setuid");
        }
        finally
        {
            Unmount(fs, image);
        }
    }

    [TestCase(RRIP_112)]
    [TestCase(RRIP_110)]
    public void SymbolicLinks(string testFile)
    {
        var longTarget = $"{new string('x', 200)}/{new string('y', 200)}";

        Dictionary<string, string> expected = new()
        {
            ["/links/rel"]  = "../file.txt",
            ["/links/abs"]  = "/usr/bin/env",
            ["/links/dots"] = "./../links/./rel",
            ["/links/deep"] = "../a/b/c/d/e/f/g/h/i/j/deep.txt",
            ["/links/root"] = "/",
            ["/links/long"] = longTarget
        };

        (ISO9660 fs, IMediaImage image) = Mount(testFile);

        try
        {
            foreach(KeyValuePair<string, string> link in expected)
            {
                fs.ReadLink(link.Key, out string target).Should().Be(ErrorNumber.NoError, link.Key);
                target.Should().Be(link.Value, link.Key);

                Stat(fs, link.Key).Attributes.Should().Be(FileAttributes.Symlink, link.Key);
            }

            fs.ReadLink("/file.txt", out _).Should().Be(ErrorNumber.InvalidArgument);
        }
        finally
        {
            Unmount(fs, image);
        }
    }

    [TestCase(RRIP_112)]
    [TestCase(RRIP_110)]
    public void PosixAttributes(string testFile)
    {
        (ISO9660 fs, IMediaImage image) = Mount(testFile);

        try
        {
            FileEntryInfo file = Stat(fs, "/file.txt");
            file.Attributes.Should().Be(FileAttributes.None);
            file.Mode.Should().Be(Convert.ToUInt32("644", 8));
            file.UID.Should().Be(OWNER_UID);
            file.GID.Should().Be(OWNER_GID);
            file.Length.Should().Be(6);

            // file.txt, the long named file and the one with spaces are the same file
            file.Links.Should().Be(3);

            Stat(fs, "/setuid").Mode.Should().Be(Convert.ToUInt32("4755", 8));

            FileEntryInfo fifo = Stat(fs, "/fifo");
            fifo.Attributes.Should().Be(FileAttributes.Pipe);
            fifo.Mode.Should().Be(Convert.ToUInt32("640", 8));

            FileEntryInfo devNull = Stat(fs, "/devnull");
            devNull.Attributes.Should().Be(FileAttributes.CharDevice);
            devNull.DeviceNo.Should().Be(DEV_NULL);

            Stat(fs, "/links").Attributes.Should().Be(FileAttributes.Directory);
        }
        finally
        {
            Unmount(fs, image);
        }
    }

    [TestCase(RRIP_112, "/file.txt")]
    [TestCase(RRIP_110, "/file.txt")]
    [TestCase(RRIP_112, "/" + LONG_NAME)]
    [TestCase(RRIP_110, "/" + LONG_NAME)]
    [TestCase(RRIP_112, "/links")]
    [TestCase(RRIP_110, "/links")]
    public void Timestamps(string testFile, string path)
    {
        (ISO9660 fs, IMediaImage image) = Mount(testFile);

        try
        {
            // Recorded as India Standard Time, 5:30 ahead of GMT, in short form for RRIP 1.12 and long form for 1.10.
            // The long name pushes the "TF" field of the RRIP 1.10 image into a continuation area.
            FileEntryInfo stat = Stat(fs, path);
            stat.LastWriteTimeUtc.Should().Be(_modified, path);
            stat.AccessTimeUtc.Should().Be(_accessed, path);
            stat.StatusChangeTimeUtc.Should().Be(_changed, path);
        }
        finally
        {
            Unmount(fs, image);
        }
    }

    [TestCase(RRIP_112)]
    [TestCase(RRIP_110)]
    public void RelocatedDirectory(string testFile)
    {
        (ISO9660 fs, IMediaImage image) = Mount(testFile);

        try
        {
            ReadDir(fs, "/a/b/c/d/e/f/g").Should().BeEquivalentTo("h");

            // Attributes must come from the moved directory, not from the placeholder pointing to it
            FileEntryInfo relocated = Stat(fs, "/a/b/c/d/e/f/g/h");
            relocated.Attributes.Should().Be(FileAttributes.Directory);
            relocated.Mode.Should().Be(Convert.ToUInt32("700", 8));
            relocated.LastWriteTimeUtc.Should().Be(_relocatedMtime);

            ReadDir(fs, "/a/b/c/d/e/f/g/h/i/j").Should().BeEquivalentTo("deep.txt");

            Encoding.ASCII.GetString(ReadFile(fs, "/a/b/c/d/e/f/g/h/i/j/deep.txt", 0, 5, 5)).Should().Be("deep\n");
        }
        finally
        {
            Unmount(fs, image);
        }
    }

    [TestCase(RRIP_112)]
    [TestCase(RRIP_110)]
    public void FileContents(string testFile)
    {
        (ISO9660 fs, IMediaImage image) = Mount(testFile);

        try
        {
            Encoding.ASCII.GetString(ReadFile(fs, "/" + LONG_NAME, 0, 6, 6)).Should().Be("hello\n");
        }
        finally
        {
            Unmount(fs, image);
        }
    }

    [TestCase("/sp112.bin", 300000)]
    [TestCase("/sp110.bin", 300000)]
    [TestCase("/sp112.bin", 4097)]
    [TestCase("/sp110.bin", 4097)]
    [TestCase("/sp112.bin", 1000)]
    [TestCase("/sp110.bin", 1000)]
    public void SparseFile(string path, int chunk)
    {
        (ISO9660 fs, IMediaImage image) = Mount(SPARSE);

        try
        {
            FileEntryInfo stat = Stat(fs, path);
            stat.Attributes.Should().Be(FileAttributes.Sparse);
            stat.Length.Should().Be(SPARSE_SIZE);

            // Only the header, index blocks and non-zero data take space on disc
            stat.Blocks.Should().BeLessThan(SPARSE_SIZE / 2048);

            byte[] contents = ReadFile(fs, path, 0, SPARSE_SIZE, chunk);
            Convert.ToHexString(SHA256.HashData(contents)).Should().Be(SPARSE_SHA256);

            byte[] partial = ReadFile(fs, path, SPARSE_PARTIAL_OFFSET, SPARSE_PARTIAL_LENGTH, chunk);
            Convert.ToHexString(SHA256.HashData(partial)).Should().Be(SPARSE_PARTIAL_SHA256);
        }
        finally
        {
            Unmount(fs, image);
        }
    }

    [Test]
    public void NormalNamespaceIgnoresRrip()
    {
        (ISO9660 fs, IMediaImage image) = Mount(RRIP_112, "normal");

        try
        {
            // Without Rock Ridge the relocation directory is a normal directory and names are the ISO 9660 ones
            List<string> root = ReadDir(fs, "/");
            root.Should().Contain("RR_MOVED");
            root.Should().NotContain(LONG_NAME);
        }
        finally
        {
            Unmount(fs, image);
        }
    }
}