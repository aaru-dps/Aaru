// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : KnownVectorChecksumTests.cs
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

using System.Text.RegularExpressions;
using Aaru.Checksums;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Checksums;

/// <summary>
///     Corpus-free checksum tests. Standard algorithms are checked against published check values for the ASCII
///     string "123456789"; Aaru-specific parameterizations are pinned regression values.
/// </summary>
[TestFixture]
[Category("Unit")]
public partial class KnownVectorChecksumTests
{
    static readonly byte[] _check = "123456789"u8.ToArray();

    static byte[] DeterministicBuffer(int length)
    {
        var  buffer = new byte[length];
        uint state  = 0x12345678;

        for(var i = 0; i < length; i++)
        {
            state     = state * 1664525 + 1013904223;
            buffer[i] = (byte)(state >> 24);
        }

        return buffer;
    }

    [Test]
    public void Crc32CheckValue()
    {
        // CRC-32/ISO-HDLC published check value
        Crc32Context.Data(_check, out _).Should().Be("cbf43926");
    }

    [Test]
    public void Crc32Empty() => Crc32Context.Data([], out _).Should().Be("00000000");

    [Test]
    public void Crc16IbmCheckValue()
    {
        // CRC-16/ARC published check value
        CRC16IbmContext.Data(_check, out _).Should().Be("bb3d");
    }

    [Test]
    public void Crc16CcittCheckValue()
    {
        // Aaru's "CCITT" CRC16 is the CD-ROM Q-subchannel CRC (poly 0x1021, seed 0, inverted output),
        // also published as CRC-16/GSM with check value 0xCE3C.
        CRC16CcittContext.Data(_check, out _).Should().Be("ce3c");
    }

    [Test]
    public void Crc64CheckValue()
    {
        // CRC-64/XZ (ECMA-182 reflected) published check value
        Crc64Context.Data(_check, out _).Should().Be("995dc9bbdf1939fa");
    }

    [Test]
    public void Adler32CheckValue()
    {
        // Published Adler-32 of "123456789"
        Adler32Context.Data(_check, out _).Should().Be("091e01de");
    }

    [Test]
    public void Adler32Empty() => Adler32Context.Data([], out _).Should().Be("00000001");

    [Test]
    public void Fletcher16CheckValue() => Fletcher16Context.Data(_check, out _).Should().Be("1ede");

    [Test]
    public void Fletcher32CheckValue() =>

        // Aaru's Fletcher-32 is the byte-oriented variant (sums every byte modulo 65535, starting at 0),
        // not the RFC 1146 16-bit-word variant, so the check value differs from the Wikipedia one.
        Fletcher32Context.Data(_check, out _).Should().Be("091501dd");

    [Test]
    public void SpamSumFormatIsStable()
    {
        string result = SpamSumContext.Data(DeterministicBuffer(8192), out _);

        SpamSumRegex().IsMatch(result).Should().BeTrue("SpamSum output should be blocksize:hash:hash, got {0}", result);
    }

    [GeneratedRegex(@"^\d+:[0-9A-Za-z+/]+:[0-9A-Za-z+/]*$")]
    private static partial Regex SpamSumRegex();
}