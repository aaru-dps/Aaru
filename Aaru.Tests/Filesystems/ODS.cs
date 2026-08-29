// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ODS.cs
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

using System.Collections.Generic;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.Filesystems;
using Aaru.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Filesystems;

/// <summary>
///     Corpus-free tests for the Files-11 On-Disk Structure plugin, covering the file ID and mapping arithmetic and
///     the directory record parser.
/// </summary>
[TestFixture]
[Category("Unit")]
public class Ods
{
    static readonly Encoding _encoding = Encoding.Latin1;

    static ODS.FileId MakeFid(ushort num, ushort seq, byte rvn = 0, byte nmx = 0)
    {
        byte[] raw = [(byte)(num & 0xFF), (byte)(num >> 8), (byte)(seq & 0xFF), (byte)(seq >> 8), rvn, nmx];

        return Marshal.ByteArrayToStructureLittleEndian<ODS.FileId>(raw, 0, 6);
    }

    /// <summary>
    ///     Builds a single directory record: size word, version limit word, flags, name count, name, then one
    ///     (version, file ID) pair per version.
    /// </summary>
    static byte[] MakeRecord(string name, params (ushort Version, ODS.FileId Fid)[] versions)
    {
        var body = new List<byte>();

        // Version limit
        body.Add(0);
        body.Add(0);

        // Flags: ODS-2 name type, FID entries
        body.Add(0);

        body.Add((byte)name.Length);
        body.AddRange(_encoding.GetBytes(name));

        // Word-align the value area
        if((name.Length & 1) == 1) body.Add(0);

        foreach((ushort version, ODS.FileId fid) in versions)
        {
            body.Add((byte)(version & 0xFF));
            body.Add((byte)(version >> 8));
            body.AddRange(Marshal.StructureToByteArrayLittleEndian(fid));
        }

        var record = new List<byte>
        {
            (byte)(body.Count & 0xFF),
            (byte)(body.Count >> 8)
        };

        record.AddRange(body);

        return record.ToArray();
    }

    static byte[] MakeBlock(params byte[][] records)
    {
        var block = new byte[512];
        var pos   = 0;

        foreach(byte[] record in records)
        {
            record.CopyTo(block, pos);
            pos += record.Length;
        }

        // End of records marker
        block[pos]     = 0xFF;
        block[pos + 1] = 0xFF;

        return block;
    }

    [Test(Description = "All versions of a directory record are parsed, not just the first")]
    public void ParseDirectoryBlock_ParsesEveryVersion()
    {
        byte[] block = MakeBlock(MakeRecord("FOO.TXT",
                                            ((ushort)3, MakeFid(100, 1)),
                                            ((ushort)2, MakeFid(101, 1)),
                                            ((ushort)1, MakeFid(102, 1))));

        var cache = new Dictionary<string, ODS.CachedFile>();

        ODS.ParseDirectoryBlockToCache(block, cache, _encoding);

        cache.Should().ContainKey("FOO.TXT;1");
        cache.Should().ContainKey("FOO.TXT;2");
        cache.Should().ContainKey("FOO.TXT;3");

        cache["FOO.TXT;1"].Fid.num.Should().Be(102);
        cache["FOO.TXT;2"].Fid.num.Should().Be(101);
        cache["FOO.TXT;3"].Fid.num.Should().Be(100);
    }

    [Test(Description = "The bare name resolves to the highest version, whatever order versions appear in")]
    public void ParseDirectoryBlock_BareNameIsHighestVersion()
    {
        byte[] block = MakeBlock(MakeRecord("BAR.TXT",
                                            ((ushort)1, MakeFid(200, 1)),
                                            ((ushort)7, MakeFid(201, 1)),
                                            ((ushort)4, MakeFid(202, 1))));

        var cache = new Dictionary<string, ODS.CachedFile>();

        ODS.ParseDirectoryBlockToCache(block, cache, _encoding);

        cache["BAR.TXT"].Version.Should().Be(7);
        cache["BAR.TXT"].Fid.num.Should().Be(201);
    }

    [Test(Description = "Self-referential entries are matched on the whole file ID, not on the file number alone")]
    public void ParseDirectoryBlock_SkipsOnlyTheExactFileId()
    {
        ODS.FileId self  = MakeFid(4, 4);
        ODS.FileId other = MakeFid(4, 9);

        byte[] block = MakeBlock(MakeRecord("000000.DIR", ((ushort)1, self)),
                                 MakeRecord("OTHER.DIR", ((ushort)1, other)));

        var cache = new Dictionary<string, ODS.CachedFile>();

        ODS.ParseDirectoryBlockToCache(block, cache, _encoding, self);

        cache.Should().NotContainKey("000000.DIR");
        cache.Should().ContainKey("OTHER.DIR");
    }

    [Test(Description = "A record whose size is too small to hold its own name and one version is rejected")]
    public void ParseDirectoryBlock_RejectsUndersizedRecord()
    {
        byte[] block = MakeBlock(MakeRecord("GOOD.TXT", ((ushort)1, MakeFid(300, 1))));

        // Corrupt the record size so it can no longer contain the name plus a version
        var offset = 0;
        block[offset]     = 6;
        block[offset + 1] = 0;

        var cache = new Dictionary<string, ODS.CachedFile>();

        ODS.ParseDirectoryBlockToCache(block, cache, _encoding);

        cache.Should().BeEmpty();
    }

    [Test(Description = "The file number extension (nmx) contributes the high bits of the header LBN")]
    public void FileHeaderLbn_HonorsFileNumberExtension()
    {
        const uint   ibmaplbn  = 600099;
        const ushort ibmapsize = 63;

        ODS.FileId low  = MakeFid(1000, 1);
        ODS.FileId high = MakeFid(1000, 1, 0, 1);

        var lowNum  = (uint)(low.num + (low.nmx << 16));
        var highNum = (uint)(high.num + (high.nmx << 16));

        highNum.Should().Be(lowNum + 65536);

        uint lowLbn  = ODS.FileHeaderLbn(ibmaplbn, ibmapsize, lowNum);
        uint highLbn = ODS.FileHeaderLbn(ibmaplbn, ibmapsize, highNum);

        lowLbn.Should().Be(ibmaplbn + ibmapsize + 999);
        highLbn.Should().Be(lowLbn + 65536);
    }

    [Test(Description = "A VBN below an extension map's base does not match that map's first extent")]
    public void MapVbnToLbnWithSum_RejectsVbnBelowBase()
    {
        // One format 2 pointer: 4 blocks at LBN 5000
        byte[] mapData = [0x03, 0x40, 0x88, 0x13, 0x00, 0x00];

        // This map covers VBNs 101..104
        const uint startSum = 100;

        ErrorNumber errno = ODS.MapVbnToLbnWithSum(mapData, 3, 101, startSum, out uint lbn, out _, out _);

        errno.Should().Be(ErrorNumber.NoError);
        lbn.Should().Be(5000);

        errno = ODS.MapVbnToLbnWithSum(mapData, 3, 104, startSum, out lbn, out _, out _);

        errno.Should().Be(ErrorNumber.NoError);
        lbn.Should().Be(5003);

        // VBN 50 belongs to the primary map, not to this extension
        errno = ODS.MapVbnToLbnWithSum(mapData, 3, 50, startSum, out _, out _, out _);

        errno.Should().Be(ErrorNumber.InvalidArgument);
    }
}
