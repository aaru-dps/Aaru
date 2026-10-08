// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Codecs.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru unit testing.
//
// --[ Description ] ----------------------------------------------------------
//
//     Tests for the general purpose codecs, mirroring the Aaru.Compression.Native test suite.
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

using Aaru.Compression;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Compression;

[TestFixture]
public class CodecTests
{
    [OneTimeSetUp]
    public void Init()
    {
        Helpers.AssertNative();
    }

    [Test]
    public void BZip2Decode()
    {
        byte[] source      = Helpers.ReadFixture("bzip2.bz2");
        byte[] destination = new byte[Helpers.MIB_SIZE];
        int    size        = BZip2.DecodeBuffer(source, destination);

        Helpers.AssertDecoded(destination, size, Helpers.MIB_SIZE, Helpers.MIB_CRC);
    }

    [Test]
    public void BZip2Roundtrip()
    {
        byte[] original   = Helpers.ReadFixture("data.bin");
        byte[] compressed = new byte[original.Length];
        int    cmpSize    = BZip2.EncodeBuffer(original, compressed, 9);

        cmpSize.Should().BePositive("compression must succeed");

        byte[] decoded = new byte[original.Length];
        int    size    = BZip2.DecodeBuffer(compressed[..cmpSize], decoded);

        Helpers.AssertRoundtrip(original, decoded, size);
    }

    [Test]
    public void LzipDecode()
    {
        byte[] source      = Helpers.ReadFixture("lzip.lz");
        byte[] destination = new byte[Helpers.MIB_SIZE];
        int    size        = LZIP.DecodeBuffer(source, destination);

        Helpers.AssertDecoded(destination, size, Helpers.MIB_SIZE, Helpers.MIB_CRC);
    }

    [Test]
    public void LzipRoundtrip()
    {
        byte[] original   = Helpers.ReadFixture("data.bin");
        byte[] compressed = new byte[original.Length];
        int    cmpSize    = LZIP.EncodeBuffer(original, compressed, 1048576, 273);

        cmpSize.Should().BePositive("compression must succeed");

        byte[] decoded = new byte[original.Length];
        int    size    = LZIP.DecodeBuffer(compressed[..cmpSize], decoded);

        Helpers.AssertRoundtrip(original, decoded, size);
    }

    [Test]
    public void LzfseDecode()
    {
        byte[] source      = Helpers.ReadFixture("lzfse.bin");
        byte[] destination = new byte[Helpers.MIB_SIZE];
        int    size        = LZFSE.DecodeBuffer(source, destination);

        Helpers.AssertDecoded(destination, size, Helpers.MIB_SIZE, Helpers.MIB_CRC);
    }

    [Test]
    public void LzfseRoundtrip()
    {
        byte[] original   = Helpers.ReadFixture("data.bin");
        byte[] compressed = new byte[original.Length];
        int    cmpSize    = LZFSE.EncodeBuffer(original, compressed);

        cmpSize.Should().BePositive("compression must succeed");

        byte[] decoded = new byte[original.Length];
        int    size    = LZFSE.DecodeBuffer(compressed[..cmpSize], decoded);

        Helpers.AssertRoundtrip(original, decoded, size);
    }

    [Test]
    public void ZstdDecode()
    {
        byte[] source      = Helpers.ReadFixture("zstd.zst");
        byte[] destination = new byte[Helpers.MIB_SIZE];
        int    size        = ZSTD.DecodeBuffer(source, destination);

        Helpers.AssertDecoded(destination, size, Helpers.MIB_SIZE, Helpers.MIB_CRC);
    }

    [Test]
    public void ZstdRoundtrip()
    {
        byte[] original   = Helpers.ReadFixture("data.bin");
        byte[] compressed = new byte[original.Length];
        int    cmpSize    = ZSTD.EncodeBuffer(original, compressed, 22);

        cmpSize.Should().BePositive("compression must succeed");

        byte[] decoded = new byte[original.Length];
        int    size    = ZSTD.DecodeBuffer(compressed[..cmpSize], decoded);

        Helpers.AssertRoundtrip(original, decoded, size);
    }

    [Test]
    public void XzDecode()
    {
        byte[] source      = Helpers.ReadFixture("xz.xz");
        byte[] destination = new byte[Helpers.MIB_SIZE];
        int    size        = XZ.DecodeBuffer(source, destination);

        Helpers.AssertDecoded(destination, size, Helpers.MIB_SIZE, Helpers.MIB_CRC);
    }

    [Test]
    public void XzRoundtrip()
    {
        byte[] original   = Helpers.ReadFixture("data.bin");
        byte[] compressed = new byte[original.Length];
        int    cmpSize    = XZ.EncodeBuffer(original, compressed, 9, XZ.CheckType.Sha256);

        cmpSize.Should().BePositive("compression must succeed");

        byte[] decoded = new byte[original.Length];
        int    size    = XZ.DecodeBuffer(compressed[..cmpSize], decoded);

        Helpers.AssertRoundtrip(original, decoded, size);
    }

    [Test]
    public void LzmaDecode()
    {
        byte[] source      = Helpers.ReadFixture("lzma.bin");
        byte[] destination = new byte[Helpers.DATA_SIZE];
        int    size        = LZMA.DecodeBuffer(source, destination, [0x5D, 0x00, 0x00, 0x00, 0x02]);

        Helpers.AssertDecoded(destination, size, Helpers.DATA_SIZE, Helpers.DATA_CRC);
    }

    [Test]
    public void LzmaRoundtrip()
    {
        byte[] original   = Helpers.ReadFixture("data.bin");
        byte[] compressed = new byte[original.Length];
        int    cmpSize    = LZMA.EncodeBuffer(original, compressed, out byte[] properties, 9, 1048576, 3, 0, 2, 273);

        cmpSize.Should().BePositive("compression must succeed");

        byte[] decoded = new byte[original.Length];
        int    size    = LZMA.DecodeBuffer(compressed[..cmpSize], decoded, properties);

        Helpers.AssertRoundtrip(original, decoded, size);
    }

    [Test]
    public void Lz4Decode()
    {
        byte[] source      = Helpers.ReadFixture("lz4.bin");
        byte[] destination = new byte[Helpers.DATA_SIZE];
        int    size        = LZ4.DecodeBuffer(source, destination);

        Helpers.AssertDecoded(destination, size, Helpers.DATA_SIZE, Helpers.DATA_CRC);
    }

    [Test]
    public void FlacDecode()
    {
        byte[] source      = Helpers.ReadFixture("flac.flac");
        byte[] destination = new byte[9633792];
        int    size        = FLAC.DecodeBuffer(source, destination);

        Helpers.AssertDecoded(destination, size, 9633792, 0xDFBC99BB);
    }

    [Test]
    public void FlacRoundtrip()
    {
        byte[] original   = Helpers.ReadFixture("audio.bin");
        byte[] compressed = new byte[original.Length];
        int    cmpSize    = FLAC.EncodeBuffer(original,
                                            compressed,
                                            4608,
                                            true,
                                            false,
                                            "partial_tukey(0/1.0/1.0)",
                                            12,
                                            0,
                                            true,
                                            false,
                                            0,
                                            8,
                                            "Aaru.Compression.Native.Tests");

        cmpSize.Should().BePositive("compression must succeed");

        byte[] decoded = new byte[original.Length];
        int    size    = FLAC.DecodeBuffer(compressed[..cmpSize], decoded);

        Helpers.AssertRoundtrip(original, decoded, size);
    }
}