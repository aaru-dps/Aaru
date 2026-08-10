// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SubchannelTests.cs
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
using Aaru.Checksums;
using Aaru.Decoders.CD;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Decoders;

[TestFixture]
[Category("Unit")]
public class SubchannelTests
{
    static byte[] DeterministicBuffer(int length, uint seed)
    {
        var  buffer = new byte[length];
        uint state  = seed;

        for(var i = 0; i < length; i++)
        {
            state     = state * 1664525 + 1013904223;
            buffer[i] = (byte)(state >> 24);
        }

        return buffer;
    }

    [TestCase(1u)]
    [TestCase(0xCAFEBABEu)]
    [TestCase(0x12345678u)]
    public void InterleaveDeinterleaveRoundTrip(uint seed)
    {
        byte[] original = DeterministicBuffer(96, seed);

        Subchannel.Deinterleave(Subchannel.Interleave(original)).Should().Equal(original);
        Subchannel.Interleave(Subchannel.Deinterleave(original)).Should().Equal(original);
    }

    [Test]
    public void BinaryToBcdQConvertsDataQ()
    {
        byte[] q = [0x01, 1, 1, 0, 2, 1, 0, 0, 6, 1, 0, 0];

        Subchannel.BinaryToBcdQ(q);

        q.Should().Equal(0x01, 0x01, 0x01, 0x00, 0x02, 0x01, 0x00, 0x00, 0x06, 0x01, 0x00, 0x00);

        byte[] withTens = [0x01, 12, 34, 56, 78, 90, 9, 99, 10, 25, 0, 0];

        Subchannel.BinaryToBcdQ(withTens);

        withTens.Should().Equal(0x01, 0x12, 0x34, 0x56, 0x78, 0x90, 0x09, 0x99, 0x10, 0x25, 0x00, 0x00);
    }

    [Test]
    public void BcdToBinaryQIsInverseOfBinaryToBcdQ()
    {
        byte[] q        = [0x01, 12, 34, 56, 78, 90, 9, 99, 10, 25, 0xAB, 0xCD];
        byte[] original = (byte[])q.Clone();

        Subchannel.BinaryToBcdQ(q);
        Subchannel.BcdToBinaryQ(q);

        q.Should().Equal(original);
    }

    [Test]
    public void BcdConversionSkipsNonDataQExceptFrame()
    {
        // ADR 2 (MCN): only q[9] is converted
        byte[] q = [0x02, 12, 34, 56, 78, 90, 9, 99, 10, 25, 0, 0];

        Subchannel.BinaryToBcdQ(q);

        q.Should().Equal(0x02, 12, 34, 56, 78, 90, 9, 99, 10, 0x25, 0, 0);
    }

    [Test]
    public void GenerateProgramAreaSector()
    {
        // Track 1 starts at LBA 150 with a 150-sector pregap; sector 301 is in the program area.
        // Arguments: sector, trackSequence, pregap, trackStart, flags, index
        byte[] sub = Subchannel.Generate(301, 1, 150, 150, 0, 0);

        sub.Length.Should().Be(96);

        byte[] deinterleaved = Subchannel.Deinterleave(sub);

        // P channel must be clear in the program area
        deinterleaved[..12].Should().OnlyContain(static b => b == 0);

        var q = new byte[12];
        Array.Copy(deinterleaved, 12, q, 0, 12);

        // ADR 1, control 0
        q[0].Should().Be(0x01);

        // BCD track number and index
        q[1].Should().Be(0x01);
        q[2].Should().Be(0x01);

        // Relative MSF: 151 frames = 00:02:01 in BCD
        q[3].Should().Be(0x00);
        q[4].Should().Be(0x02);
        q[5].Should().Be(0x01);

        // Absolute MSF: LBA 301 + 150 = 451 frames = 00:06:01 in BCD
        q[7].Should().Be(0x00);
        q[8].Should().Be(0x06);
        q[9].Should().Be(0x01);

        // CRC over the first 10 bytes must match the stored CRC
        CRC16CcittContext.Data(q, 10, out byte[] crc);
        q[10].Should().Be(crc[0]);
        q[11].Should().Be(crc[1]);
    }

    [Test]
    public void GeneratePregapSector()
    {
        // Arguments: sector, trackSequence, pregap, trackStart, flags, index
        byte[] sub = Subchannel.Generate(0, 1, 150, -150, 0, 0);

        byte[] deinterleaved = Subchannel.Deinterleave(sub);

        // P channel is set during the pregap
        deinterleaved[..12].Should().OnlyContain(static b => b == 0xFF);

        var q = new byte[12];
        Array.Copy(deinterleaved, 12, q, 0, 12);

        // Index 0 during pregap
        q[2].Should().Be(0x00);
    }

    [Test]
    public void ConvertQToRawRebuildsPAndQ()
    {
        var deinterleavedQ = new byte[16];
        deinterleavedQ[0]  = 0x41;
        deinterleavedQ[1]  = 0x01;
        deinterleavedQ[2]  = 0x01;
        deinterleavedQ[9]  = 0x02;
        deinterleavedQ[15] = 0x80; // P bit set

        byte[] raw           = Subchannel.ConvertQToRaw(deinterleavedQ);
        byte[] deinterleaved = Subchannel.Deinterleave(raw);

        raw.Length.Should().Be(96);
        deinterleaved[..12].Should().OnlyContain(static b => b == 0xFF);
        deinterleaved[12..24].Should().Equal(0x41, 0x01, 0x01, 0, 0, 0, 0, 0, 0, 0x02, 0, 0);
        deinterleaved[24..].Should().OnlyContain(static b => b == 0);
    }

    [TestCase("USRC17607839", ExpectedResult = true)]
    [TestCase("0#ZZ!17607__", ExpectedResult = true, Description = "Non-conformant ISRCs on real discs must be kept")]
    [TestCase("000000000000", ExpectedResult = false)]
    [TestCase("            ", ExpectedResult = false)]
    [TestCase("0000    \0\0\0\0", ExpectedResult = false)]
    [TestCase("", ExpectedResult = false)]
    [TestCase(null, ExpectedResult = false)]
    public bool IsrcIsPresentDetectsAbsentIsrcs(string isrc) => Subchannel.IsrcIsPresent(isrc);

    [Test]
    public void DecodeMcnFormatsBcdDigits()
    {
        byte[] q = [0x02, 0x12, 0x34, 0x56, 0x78, 0x90, 0x12, 0x30, 0x00, 0x25, 0x00, 0x00];

        Subchannel.DecodeMcn(q).Should().Be("1234567890123");
    }
}