// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SwappingTests.cs
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

using System.Buffers.Binary;
using Aaru.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Helpers;

[TestFixture]
[Category("Unit")]
public class SwappingTests
{
    [TestCase((ushort)0x0000, (ushort)0x0000)]
    [TestCase((ushort)0x1234, (ushort)0x3412)]
    [TestCase((ushort)0x00FF, (ushort)0xFF00)]
    [TestCase((ushort)0xFFFF, (ushort)0xFFFF)]
    [TestCase((ushort)0x8001, (ushort)0x0180)]
    public void SwapUInt16(ushort value, ushort expected)
    {
        Swapping.Swap(value).Should().Be(expected);
        Swapping.Swap(Swapping.Swap(value)).Should().Be(value);
    }

    [TestCase((short)0)]
    [TestCase((short)0x1234)]
    [TestCase((short)-1)]
    [TestCase(short.MinValue)]
    [TestCase(short.MaxValue)]
    [TestCase(unchecked((short)0xFF00))]
    [TestCase((short)0x00FF)]
    public void SwapInt16MatchesReverseEndianness(short value)
    {
        Swapping.Swap(value).Should().Be(BinaryPrimitives.ReverseEndianness(value));
        Swapping.Swap(Swapping.Swap(value)).Should().Be(value);
    }

    [TestCase(0x00000000u, 0x00000000u)]
    [TestCase(0x12345678u, 0x78563412u)]
    [TestCase(0x000000FFu, 0xFF000000u)]
    [TestCase(0xFFFFFFFFu, 0xFFFFFFFFu)]
    [TestCase(0xDEADBEEFu, 0xEFBEADDEu)]
    public void SwapUInt32(uint value, uint expected)
    {
        Swapping.Swap(value).Should().Be(expected);
        Swapping.Swap(Swapping.Swap(value)).Should().Be(value);
    }

    [TestCase(0)]
    [TestCase(0x12345678)]
    [TestCase(-1)]
    [TestCase(int.MinValue)]
    [TestCase(int.MaxValue)]
    [TestCase(unchecked((int)0xDEADBEEF))]
    public void SwapInt32MatchesReverseEndianness(int value)
    {
        Swapping.Swap(value).Should().Be(BinaryPrimitives.ReverseEndianness(value));
        Swapping.Swap(Swapping.Swap(value)).Should().Be(value);
    }

    [TestCase(0x0000000000000000ul, 0x0000000000000000ul)]
    [TestCase(0x0123456789ABCDEFul, 0xEFCDAB8967452301ul)]
    [TestCase(0x00000000000000FFul, 0xFF00000000000000ul)]
    [TestCase(0xFFFFFFFFFFFFFFFFul, 0xFFFFFFFFFFFFFFFFul)]
    public void SwapUInt64(ulong value, ulong expected)
    {
        Swapping.Swap(value).Should().Be(expected);
        Swapping.Swap(Swapping.Swap(value)).Should().Be(value);
    }

    [TestCase(0L)]
    [TestCase(0x0123456789ABCDEFL)]
    [TestCase(-1L)]
    [TestCase(long.MinValue)]
    [TestCase(long.MaxValue)]
    public void SwapInt64MatchesReverseEndianness(long value)
    {
        Swapping.Swap(value).Should().Be(BinaryPrimitives.ReverseEndianness(value));
        Swapping.Swap(Swapping.Swap(value)).Should().Be(value);
    }

    [TestCase(0x11223344u, ExpectedResult = 0x33441122u)]
    [TestCase(0x00000000u, ExpectedResult = 0x00000000u)]
    [TestCase(0xAABBCCDDu, ExpectedResult = 0xCCDDAABBu)]
    public uint PdpFromLittleEndian(uint value) => Swapping.PDPFromLittleEndian(value);

    [TestCase(0x11223344u, ExpectedResult = 0x22114433u)]
    [TestCase(0x00000000u, ExpectedResult = 0x00000000u)]
    [TestCase(0xAABBCCDDu, ExpectedResult = 0xBBAADDCCu)]
    public uint PdpFromBigEndian(uint value) => Swapping.PDPFromBigEndian(value);
}