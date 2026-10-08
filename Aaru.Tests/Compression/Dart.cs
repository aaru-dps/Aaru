// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dart.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru unit testing.
//
// --[ Description ] ----------------------------------------------------------
//
//     Tests for the Apple LZH decompressor using DART disk images, mirroring the
//     Aaru.Compression.Native test suite.
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
using System.Linq;
using Aaru.Compression;
using Aaru.Compression.Apple;
using Aaru.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Compression;

[TestFixture]
public class DartTests
{
    [OneTimeSetUp]
    public void Init()
    {
        Helpers.AssertNative();
    }

    const int DART_COMPRESS_RLE = 0;
    const int DART_COMPRESS_LZH = 1;
    /// <summary>40 sectors of 512 bytes of data plus 12 bytes of tags</summary>
    const int DART_BUFFER_SIZE = 20960;
    const int DART_HEADER_SIZE = 4;
    const int DART_TYPE_MAC_HD = 16;
    const int DART_TYPE_DOS_HD = 18;

    [TestCase("mf2dd_hfs_fast.dart.lz", "mf2dd_hfs_best.dart.lz")]
    [TestCase("mf2dd_mfs_fast.dart.lz", "mf2dd_mfs_best.dart.lz")]
    [TestCase("mf1dd_hfs_fast.dart.lz", "mf1dd_hfs_best.dart.lz")]
    [TestCase("mf1dd_mfs_fast.dart.lz", "mf1dd_mfs_best.dart.lz")]
    public void AppleLzh(string fastFixture, string bestFixture)
    {
        DartImage fast = LoadDart(fastFixture);
        DartImage best = LoadDart(bestFixture);

        fast.Compression.Should().Be(DART_COMPRESS_RLE, "fast image must use RLE");
        best.Compression.Should().Be(DART_COMPRESS_LZH, "best image must use LZH");

        byte[] reference     = ExpandRle(in fast);
        int    inPos         = best.DataOffset;
        int    refBlockIndex = 0;
        int    blocksOk      = 0;

        for(int i = 0; i < best.BlockLengths.Length; i++)
        {
            if(best.BlockLengths[i] == 0)
            {
                if(fast.BlockLengths[i] != 0) refBlockIndex++;

                continue;
            }

            ReadOnlySpan<byte> expected = reference.AsSpan(refBlockIndex * DART_BUFFER_SIZE, DART_BUFFER_SIZE);

            if(best.BlockLengths[i] == -1)
            {
                best.Raw.AsSpan(inPos, DART_BUFFER_SIZE)
                    .SequenceEqual(expected)
                    .Should()
                    .BeTrue($"stored block {i} must match the RLE image");

                inPos += DART_BUFFER_SIZE;
            }
            else
            {
                int    compressedSize = best.BlockLengths[i];
                byte[] decoded        = new byte[DART_BUFFER_SIZE];
                int    size           = Lzh.DecodeBuffer(best.Raw[inPos..(inPos + compressedSize)], decoded);

                size.Should().Be(DART_BUFFER_SIZE, $"LZH block {i} must decode to a full block");
                decoded.AsSpan().SequenceEqual(expected).Should().BeTrue($"LZH block {i} must match the RLE image");

                inPos += compressedSize;
            }

            refBlockIndex++;
            blocksOk++;
        }

        blocksOk.Should().BePositive("at least one block must be compared");
    }

    /// <summary>Decompresses a lzip compressed DART image and parses its header</summary>
    static DartImage LoadDart(string fixture)
    {
        byte[] lz  = Helpers.ReadFixture(fixture);
        byte[] raw = null;

        foreach(int capacity in new[]
                {
                    512 * 1024, 1024 * 1024, 2 * 1024 * 1024
                })
        {
            byte[] buffer = new byte[capacity];
            int    size   = LZIP.DecodeBuffer(lz, buffer);

            if(size <= 0) continue;

            raw = buffer[..size];

            break;
        }

        raw.Should().NotBeNull($"{fixture} must decompress");
        raw.Length.Should().BeGreaterThanOrEqualTo(DART_HEADER_SIZE, "image must contain a header");

        int numBlocks = raw[1] is DART_TYPE_MAC_HD or DART_TYPE_DOS_HD ? 72 : 40;
        int dataStart = DART_HEADER_SIZE + numBlocks * 2;

        raw.Length.Should().BeGreaterThanOrEqualTo(dataStart, "image must contain the block lengths");

        short[] blockLengths = new short[numBlocks];

        for(int i = 0; i < numBlocks; i++)
            blockLengths[i] = BigEndianBitConverter.ToInt16(raw, DART_HEADER_SIZE + i * 2);

        return new DartImage(raw[0], blockLengths, raw, dataStart);
    }

    /// <summary>Expands every block of a RLE compressed DART image, raw blocks are copied as is</summary>
    static byte[] ExpandRle(in DartImage image)
    {
        int    activeBlocks = image.BlockLengths.Count(l => l != 0);
        byte[] data         = new byte[activeBlocks * DART_BUFFER_SIZE];
        int    outPos       = 0;
        int    inPos        = image.DataOffset;

        foreach(short length in image.BlockLengths)
        {
            if(length == 0) continue;

            if(length == -1)
            {
                if(inPos + DART_BUFFER_SIZE <= image.Raw.Length)
                    Array.Copy(image.Raw, inPos, data, outPos, DART_BUFFER_SIZE);

                inPos += DART_BUFFER_SIZE;
            }
            else
            {
                int compressedSize = length * 2;

                if(inPos + compressedSize <= image.Raw.Length)
                {
                    byte[] block = new byte[DART_BUFFER_SIZE];
                    Rle.DecodeBuffer(image.Raw[inPos..(inPos + compressedSize)], block);
                    Array.Copy(block, 0, data, outPos, DART_BUFFER_SIZE);
                }

                inPos += compressedSize;
            }

            outPos += DART_BUFFER_SIZE;
        }

        return data;
    }

    readonly record struct DartImage(byte Compression, short[] BlockLengths, byte[] Raw, int DataOffset);
}