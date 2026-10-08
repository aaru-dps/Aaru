// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Helpers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Aaru unit testing.
//
// --[ Description ] ----------------------------------------------------------
//
//     Shared helpers for the tests that mirror the Aaru.Compression.Native
//     test suite through the Aaru.Compression wrappers.
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
using System.IO;
using Aaru.Checksums;
using Aaru.Helpers;
using FluentAssertions;

namespace Aaru.Tests.Compression;

static class Helpers
{
    /// <summary>Size of alice29.txt from the Canterbury corpus, decoded by most archiver fixtures</summary>
    internal const int ALICE29_SIZE = 152089;
    /// <summary>CRC32 of alice29.txt</summary>
    internal const uint ALICE29_CRC = 0x66007DBA;
    /// <summary>Size of the first MiB of data.bin</summary>
    internal const int MIB_SIZE = 1048576;
    /// <summary>CRC32 of the first MiB of data.bin</summary>
    internal const uint MIB_CRC = 0xC64059C0;
    /// <summary>Size of data.bin</summary>
    internal const int DATA_SIZE = 8388608;
    /// <summary>CRC32 of data.bin</summary>
    internal const uint DATA_CRC = 0x954BF76E;

    internal static readonly string DataFolder = Path.Combine(Consts.TestFilesRoot, "Compression test files");

    /// <summary>Fails if the native library is not loadable, so no test silently uses a managed fallback</summary>
    internal static void AssertNative()
    {
        Aaru.Compression.Native.IsSupported.Should().BeTrue("Aaru.Compression.Native library must be available");
    }

    internal static byte[] ReadFixture(string name)
    {
        string     path = Path.Combine(DataFolder, name);
        FileStream fs   = new FileStream(path, FileMode.Open, FileAccess.Read);
        byte[]     data = new byte[fs.Length];
        fs.EnsureRead(data, 0, data.Length);
        fs.Close();

        return data;
    }

    internal static uint Crc32(byte[] data, int length)
    {
        Crc32Context.Data(data, (uint)length, out byte[] hash);

        return BigEndianBitConverter.ToUInt32(hash, 0);
    }

    /// <summary>Reads a whole decompression stream into a buffer</summary>
    internal static byte[] ReadAll(Stream stream)
    {
        MemoryStream ms = new MemoryStream();
        stream.CopyTo(ms);

        return ms.ToArray();
    }

    /// <summary>Checks size and CRC32 of a decoded buffer</summary>
    internal static void AssertDecoded(byte[] decoded, int length, int expectedSize, uint expectedCrc)
    {
        length.Should().Be(expectedSize, "decoded size must match");
        Crc32(decoded, length).Should().Be(expectedCrc, "decoded CRC32 must match");
    }

    /// <summary>Checks size and CRC32 of the whole contents of a decompression stream</summary>
    internal static void AssertStream(Stream stream, int expectedSize, uint expectedCrc)
    {
        byte[] decoded = ReadAll(stream);
        AssertDecoded(decoded, decoded.Length, expectedSize, expectedCrc);
    }

    /// <summary>Checks that a buffer decodes to the same contents it had before being encoded</summary>
    internal static void AssertRoundtrip(byte[] original, byte[] decoded, int decodedLength)
    {
        decodedLength.Should().Be(original.Length, "decoded size must match the original");

        Crc32(decoded, decodedLength)
           .Should()
           .Be(Crc32(original, original.Length), "decoded CRC32 must match the original");

        decoded.AsSpan(0, decodedLength).SequenceEqual(original).Should().BeTrue("decoded contents must match");
    }
}