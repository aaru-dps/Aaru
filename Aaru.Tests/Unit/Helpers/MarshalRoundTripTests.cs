// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MarshalRoundTripTests.cs
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

using System.Runtime.InteropServices;
using Aaru.Helpers;
using Marshal = Aaru.Helpers.Marshal;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Helpers;

[TestFixture]
[Category("Unit")]
public class MarshalRoundTripTests
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct TestStruct : ISwapEndian<TestStruct>, ISwapPdpEndian<TestStruct>
    {
        public byte   B;
        public ushort U16;
        public uint   U32;
        public ulong  U64;
        public short  S16;
        public int    S32;

        public readonly TestStruct SwapEndian() => this with
        {
            U16 = Swapping.Swap(U16),
            U32 = Swapping.Swap(U32),
            U64 = Swapping.Swap(U64),
            S16 = Swapping.Swap(S16),
            S32 = Swapping.Swap(S32)
        };

        public readonly TestStruct SwapPdpEndian() => this with
        {
            U32 = Swapping.PDPFromLittleEndian(U32),
            S32 = (int)Swapping.PDPFromLittleEndian((uint)S32)
        };
    }

    // Little-endian on-disk layout of the struct above:
    // B=0xAA, U16=0x1122, U32=0x33445566, U64=0x8899AABBCCDDEEFF, S16=0x0102, S32=0x03040506
    static readonly byte[] _littleEndianBytes =
    [
        0xAA, 0x22, 0x11, 0x66, 0x55, 0x44, 0x33, 0xFF, 0xEE, 0xDD, 0xCC, 0xBB, 0xAA, 0x99, 0x88, 0x02, 0x01, 0x06,
        0x05, 0x04, 0x03
    ];

    static readonly byte[] _bigEndianBytes =
    [
        0xAA, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF, 0x01, 0x02, 0x03,
        0x04, 0x05, 0x06
    ];

    static void AssertExpectedValues(TestStruct s)
    {
        s.B.Should().Be(0xAA);
        s.U16.Should().Be(0x1122);
        s.U32.Should().Be(0x33445566u);
        s.U64.Should().Be(0x8899AABBCCDDEEFFul);
        s.S16.Should().Be(0x0102);
        s.S32.Should().Be(0x03040506);
    }

    [Test]
    public void SizeOfMatchesPackedLayout() => Marshal.SizeOf<TestStruct>().Should().Be(21);

    [Test]
    public void ByteArrayToStructureLittleEndian() =>
        AssertExpectedValues(Marshal.ByteArrayToStructureLittleEndian<TestStruct>(_littleEndianBytes));

    [Test]
    public void ByteArrayToStructureBigEndian() =>
        AssertExpectedValues(Marshal.ByteArrayToStructureBigEndian<TestStruct>(_bigEndianBytes));

    [Test]
    public void SpanToStructureLittleEndian() =>
        AssertExpectedValues(Marshal.SpanToStructureLittleEndian<TestStruct>(_littleEndianBytes));

    [Test]
    public void SpanToStructureBigEndian() =>
        AssertExpectedValues(Marshal.SpanToStructureBigEndian<TestStruct>(_bigEndianBytes));

    [Test]
    public void SpanToStructureLittleEndianWithOffset()
    {
        var buffer = new byte[_littleEndianBytes.Length + 7];
        _littleEndianBytes.CopyTo(buffer, 7);

        AssertExpectedValues(Marshal.SpanToStructureLittleEndian<TestStruct>(buffer, 7, _littleEndianBytes.Length));
    }

    [Test]
    public void StructureToByteArrayLittleEndianRoundTrip()
    {
        TestStruct s = Marshal.ByteArrayToStructureLittleEndian<TestStruct>(_littleEndianBytes);

        Marshal.StructureToByteArrayLittleEndian(s).Should().Equal(_littleEndianBytes);
    }

    [Test]
    public void StructureToByteArrayBigEndianRoundTrip()
    {
        TestStruct s = Marshal.ByteArrayToStructureBigEndian<TestStruct>(_bigEndianBytes);

        Marshal.StructureToByteArrayBigEndian(s).Should().Equal(_bigEndianBytes);
    }

    [Test]
    public void ByteArrayToStructurePdpEndian()
    {
        // PDP endian reads little-endian, then swaps the 16-bit halves of 32-bit fields only
        TestStruct s = Marshal.ByteArrayToStructurePdpEndian<TestStruct>(_littleEndianBytes);

        s.B.Should().Be(0xAA);
        s.U16.Should().Be(0x1122);
        s.U32.Should().Be(Swapping.PDPFromLittleEndian(0x33445566u));
        s.S16.Should().Be(0x0102);
        s.S32.Should().Be((int)Swapping.PDPFromLittleEndian(0x03040506u));
    }
}