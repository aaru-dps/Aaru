// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HelperRegressionTests.cs
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
using System.Text;
using Aaru.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Helpers;

[TestFixture]
[Category("Unit")]
public class HelperRegressionTests
{
    [Test]
    public void ConvertFromHexAscii_SingleCharacter_DoesNotThrow()
    {
        int count = Marshal.ConvertFromHexAscii("0", out byte[] buf);

        count.Should().Be(0);
        buf.Should().BeEmpty();
    }

    [Test]
    public void ConvertFromHexAscii_OddLength_IgnoresTrailingNibble()
    {
        int count = Marshal.ConvertFromHexAscii("0x123", out byte[] buf);

        count.Should().Be(1);
        buf.Should().Equal(0x12);
    }

    [Test]
    public void SpacePaddedToString_WithStart_ReturnsSubstring()
    {
        byte[] data = "XXHELLO   "u8.ToArray();

        StringHandlers.SpacePaddedToString(data, Encoding.ASCII, 2).Should().Be("HELLO");
    }

    [Test]
    public void SpacePaddedToString_WithStartAndNoPadding_DoesNotThrow()
    {
        byte[] data = "XXHELLO"u8.ToArray();

        StringHandlers.SpacePaddedToString(data, Encoding.ASCII, 2).Should().Be("HELLO");
    }

    [Test]
    public void MacToDateTime_CorruptTimestamp_ReturnsEpoch() =>
        DateHandlers.MacToDateTime(ulong.MaxValue).Should().Be(DateTime.MinValue);

    [Test]
    public void MacToDateTime_ValidTimestamp_Converts() =>
        DateHandlers.MacToDateTime(0).Should().Be(new DateTime(1904, 1, 1, 0, 0, 0));

    [Test]
    public void VmsToDateTime_CorruptTimestamp_ReturnsEpoch() =>
        DateHandlers.VmsToDateTime(ulong.MaxValue).Should().Be(DateTime.MinValue);

    [Test]
    public void EcmaToDateTime_CorruptDate_ReturnsEpoch() =>
        DateHandlers.EcmaToDateTime(0, 1990, 0, 0, 0, 0, 0, 0, 0, 0).Should().Be(DateTime.MinValue);

    [Test]
    public void EcmaToDateTime_ValidUtcDate_Converts() =>
        DateHandlers.EcmaToDateTime(0, 1990, 6, 15, 12, 30, 45, 0, 0, 0)
                    .Should()
                    .Be(new DateTime(1990, 6, 15, 12, 30, 45, DateTimeKind.Utc));

    [Test]
    public void ArrayFill_EmptyValueArray_DoesNotHang()
    {
        var destination = new byte[16];

        ArrayHelpers.ArrayFill(destination, Array.Empty<byte>());

        destination.Should().OnlyContain(static b => b == 0);
    }

    [Test]
    public void BigEndianBitConverterToString_WithStartIndex_ReversesSlice() =>
        BigEndianBitConverter.ToString([0xAA, 0xBB, 0x01, 0x02, 0x03], 2).Should().Be("03-02-01");

    [Test]
    public void BigEndianBitConverterToString_WithStartIndexAndLength_ReversesSlice() =>
        BigEndianBitConverter.ToString([0xAA, 0xBB, 0x01, 0x02, 0x03, 0xCC], 2, 3).Should().Be("03-02-01");

    [Test]
    public void HighlightNumbers_DoesNotDoubleWrapHighlightedNumbers()
    {
        string once  = MarkupHelper.HighlightNumbers("wait 125 ms", "lime");
        string twice = MarkupHelper.HighlightNumbers(once, "lime");

        once.Should().Be("wait [lime]125[/] ms");
        twice.Should().Be(once);
    }
}
