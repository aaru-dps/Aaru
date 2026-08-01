// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BigEndianBitConverterTests.cs
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
using Aaru.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Helpers;

[TestFixture]
[Category("Unit")]
public class BigEndianBitConverterTests
{
    static readonly byte[] _sample = [0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF, 0x00, 0xFF, 0x80, 0x7F];

    [Test]
    public void GetBytesUInt16() => BigEndianBitConverter.GetBytes((ushort)0x1234).Should().Equal(0x12, 0x34);

    [Test]
    public void GetBytesInt16() => BigEndianBitConverter.GetBytes(unchecked((short)0xFEDC)).Should().Equal(0xFE, 0xDC);

    [Test]
    public void GetBytesUInt32() => BigEndianBitConverter.GetBytes(0x12345678u).Should().Equal(0x12, 0x34, 0x56, 0x78);

    [Test]
    public void GetBytesInt32() => BigEndianBitConverter.GetBytes(unchecked((int)0xDEADBEEF))
                                                        .Should()
                                                        .Equal(0xDE, 0xAD, 0xBE, 0xEF);

    [Test]
    public void GetBytesUInt64() => BigEndianBitConverter.GetBytes(0x0123456789ABCDEFul)
                                                         .Should()
                                                         .Equal(0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF);

    [Test]
    public void GetBytesInt64() => BigEndianBitConverter.GetBytes(-2L)
                                                        .Should()
                                                        .Equal(0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFE);

    [Test]
    public void ToUInt16ReadsBigEndian()
    {
        BigEndianBitConverter.ToUInt16(_sample, 0).Should().Be(0x0123);
        BigEndianBitConverter.ToUInt16(_sample, 3).Should().Be(0x6789);
    }

    [Test]
    public void ToInt16ReadsBigEndian() => BigEndianBitConverter.ToInt16(_sample, 4)
                                                                .Should()
                                                                .Be(unchecked((short)0x89AB));

    [Test]
    public void ToUInt32ReadsBigEndian()
    {
        BigEndianBitConverter.ToUInt32(_sample, 0).Should().Be(0x01234567u);
        BigEndianBitConverter.ToUInt32(_sample, 4).Should().Be(0x89ABCDEFu);
    }

    [Test]
    public void ToInt32ReadsBigEndian() => BigEndianBitConverter.ToInt32(_sample, 4)
                                                                .Should()
                                                                .Be(unchecked((int)0x89ABCDEF));

    [Test]
    public void ToUInt64ReadsBigEndian() =>
        BigEndianBitConverter.ToUInt64(_sample, 0).Should().Be(0x0123456789ABCDEFul);

    [Test]
    public void ToInt64ReadsBigEndian() =>
        BigEndianBitConverter.ToInt64(_sample, 0).Should().Be(unchecked((long)0x0123456789ABCDEFul));

    [Test]
    public void ToSingleReadsBigEndian()
    {
        byte[] buffer = [0x3F, 0x80, 0x00, 0x00];
        BigEndianBitConverter.ToSingle(buffer, 0).Should().Be(1.0f);
    }

    [Test]
    public void SingleRoundTrip()
    {
        byte[] bytes = BigEndianBitConverter.GetBytes(12.375f);
        BigEndianBitConverter.ToSingle(bytes, 0).Should().Be(12.375f);
    }

    [Test]
    public void ToGuidReadsAllSixteenBytes()
    {
        byte[] buffer =
        [
            0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88, 0x99, 0xAA, 0xBB, 0xCC, 0xDD, 0xEE, 0xFF
        ];

        BigEndianBitConverter.ToGuid(buffer, 0).Should().Be(new Guid("00112233-4455-6677-8899-aabbccddeeff"));
    }

    [Test]
    public void NotImplementedMembersThrow()
    {
        byte[] buffer = new byte[8];

        ((Action)(static () => BigEndianBitConverter.DoubleToInt64Bits(1.0))).Should().Throw<NotImplementedException>();

        ((Action)(static () => BigEndianBitConverter.Int64BitsToDouble(1L))).Should().Throw<NotImplementedException>();
        ((Action)(() => BigEndianBitConverter.ToBoolean(buffer, 0))).Should().Throw<NotImplementedException>();
        ((Action)(() => BigEndianBitConverter.ToChar(buffer, 0))).Should().Throw<NotImplementedException>();
        ((Action)(() => BigEndianBitConverter.ToDouble(buffer, 0))).Should().Throw<NotImplementedException>();
    }
}