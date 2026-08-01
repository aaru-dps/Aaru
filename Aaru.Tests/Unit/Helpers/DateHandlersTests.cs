// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DateHandlersTests.cs
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
public class DateHandlersTests
{
    [Test]
    public void MacEpochIs1904() => DateHandlers.MacToDateTime(0).Should().Be(new DateTime(1904, 1, 1));

    [Test]
    public void MacToUnixEpochDelta() => DateHandlers.MacToDateTime(2082844800).Should().Be(new DateTime(1970, 1, 1));

    [Test]
    public void LisaEpochIs1901() => DateHandlers.LisaToDateTime(0).Should().Be(new DateTime(1901, 1, 1));

    [Test]
    public void UnixEpochIs1970() => DateHandlers.UnixToDateTime(0).Should().Be(new DateTime(1970, 1, 1));

    [Test]
    public void UnixNegativeGoesBeforeEpoch() =>
        DateHandlers.UnixToDateTime(-1).Should().Be(new DateTime(1969, 12, 31, 23, 59, 59));

    [Test]
    public void UnixUnsignedWithNanoseconds() => DateHandlers.UnixUnsignedToDateTime(0, 1000u)
                                                             .Should()
                                                             .Be(new DateTime(1970, 1, 1).AddTicks(10));

    [Test]
    public void UnixUnsignedOverflowReturnsMinValue()
    {
        DateHandlers.UnixUnsignedToDateTime(ulong.MaxValue).Should().Be(DateTime.MinValue);
        DateHandlers.UnixUnsignedToDateTime(ulong.MaxValue, 0).Should().Be(DateTime.MinValue);
    }

    [Test]
    public void UnixHrTimeIsNanoseconds() => DateHandlers.UnixHrTimeToDateTime(1_000_000_000)
                                                         .Should()
                                                         .Be(new DateTime(1970, 1, 1, 0, 0, 1));

    [Test]
    public void VmsEpochIsJulianDay0() => DateHandlers.VmsToDateTime(0).Should().Be(new DateTime(1858, 11, 17));

    [Test]
    public void AmigaEpochIs1978() => DateHandlers.AmigaToDateTime(0, 0, 0).Should().Be(new DateTime(1978, 1, 1));

    [Test]
    public void AmigaTicksAreFiftiethsOfSecond() => DateHandlers.AmigaToDateTime(1, 1, 50)
                                                                .Should()
                                                                .Be(new DateTime(1978, 1, 2, 0, 1, 1));

    [Test]
    public void AmigaOverflowReturnsMinValue() => DateHandlers
                                                 .AmigaToDateTime(uint.MaxValue, uint.MaxValue, uint.MaxValue)
                                                 .Should()
                                                 .Be(DateTime.MinValue);

    [Test]
    public void DosKnownTimestamp() =>

        // Year 21 (+1980), month 1, day 1; hour 10, minute 0, second 0
        DateHandlers.DosToDateTime(0x2A21, 0x5000).Should().Be(new DateTime(2001, 1, 1, 10, 0, 0));

    [Test]
    public void DosInvalidDateFallsBackTo1980() =>
        DateHandlers.DosToDateTime(0, 0).Should().Be(new DateTime(1980, 1, 1));

    [Test]
    public void ProDosKnownTimestamp()
    {
        // year 20 -> 1920 -> +100 = 2020, month 6, day 15, hour 13, minute 45
        const ushort date = 20 << 9 | 6 << 5 | 15;
        const ushort time = 13 << 8 | 45;

        DateHandlers.ProDosToDateTime(date, time).Should().Be(new DateTime(2020, 6, 15, 13, 45, 0));
    }

    [Test]
    public void ProDosYear40StaysIn19Xx()
    {
        const ushort date = 40 << 9 | 1 << 5 | 1;

        DateHandlers.ProDosToDateTime(date, 0).Should().Be(new DateTime(1940, 1, 1));
    }

    [Test]
    public void ProDosInvalidMonthReturnsMinValue() =>
        DateHandlers.ProDosToDateTime(0, 0).Should().Be(DateTime.MinValue);

    [Test]
    public void UcsdPascalKnownTimestamp()
    {
        // year 84 (1984), day 15, month 6
        const short record = unchecked((short)(84 << 9 | 15 << 4 | 6));

        DateHandlers.UcsdPascalToDateTime(record).Should().Be(new DateTime(1984, 6, 15));
    }

    [Test]
    public void UcsdPascalZeroRecordReturnsMinValue() =>
        DateHandlers.UcsdPascalToDateTime(0).Should().Be(DateTime.MinValue);

    [Test]
    public void CpmKnownTimestamp()
    {
        // 1 day after the 1978 epoch, 05:30 (binary hours/minutes as currently implemented)
        byte[] timestamp = [1, 0, 5, 30];

        DateHandlers.CpmToDateTime(timestamp).Should().Be(new DateTime(1978, 1, 2, 5, 30, 0));
    }

    [Test]
    public void Os9KnownTimestamps()
    {
        DateHandlers.Os9ToDateTime([90, 6, 15, 12, 30]).Should().Be(new DateTime(1990, 6, 15, 12, 30, 0));
        DateHandlers.Os9ToDateTime([90, 6, 15]).Should().Be(new DateTime(1990,         6, 15));
    }

    [Test]
    public void Os9WrongLengthReturnsMinValue()
    {
        DateHandlers.Os9ToDateTime(null).Should().Be(DateTime.MinValue);
        DateHandlers.Os9ToDateTime([1, 2, 3, 4]).Should().Be(DateTime.MinValue);
    }

    [Test]
    public void Os9InvalidMonthReturnsMinValue()
    {
        byte[] date = "Z\r(\0\0"u8.ToArray(); // 90, 13, 40, 0, 0: month 13, day 40

        DateHandlers.Os9ToDateTime(date).Should().Be(DateTime.MinValue);
    }

    [Test]
    public void LifKnownBcdTimestamp() => DateHandlers.LifToDateTime(0x21, 0x01, 0x02, 0x03, 0x04, 0x05)
                                                      .Should()
                                                      .Be(new DateTime(2021, 1, 2, 3, 4, 5));

    [Test]
    public void LifYear70IsNineteenSeventy() => DateHandlers.LifToDateTime(0x70, 0x01, 0x01, 0x00, 0x00, 0x00)
                                                            .Should()
                                                            .Be(new DateTime(1970, 1, 1));

    [Test]
    public void LifInvalidInputFallsBackTo1970()
    {
        // Wrong length array
        DateHandlers.LifToDateTime([1, 2, 3]).Should().Be(new DateTime(1970, 1, 1));

        // BCD month 0x13 = 19, out of range
        DateHandlers.LifToDateTime(0x21, 0x13, 0x01, 0x00, 0x00, 0x00).Should().Be(new DateTime(1970, 1, 1));
    }

    [Test]
    public void Iso9660KnownTimestamp()
    {
        var buffer = new byte[17];
        "2001010203040500"u8.CopyTo(buffer);

        DateHandlers.Iso9660ToDateTime(buffer).Should().Be(new DateTime(2001, 1, 2, 3, 4, 5));
    }

    [Test]
    public void Iso9660TimezoneOffsetIsQuarterHoursSubtracted()
    {
        var buffer = new byte[17];
        "2001010203040500"u8.CopyTo(buffer);

        // +4 quarters of an hour east of GMT -> one hour earlier in UTC
        buffer[16] = 4;

        DateHandlers.Iso9660ToDateTime(buffer).Should().Be(new DateTime(2001, 1, 2, 2, 4, 5));
    }

    [Test]
    public void Iso9660ZeroedBufferReturnsMinValue() =>
        DateHandlers.Iso9660ToDateTime(new byte[17]).Should().Be(DateTime.MinValue);

    [Test]
    public void HighSierraKnownTimestamp() =>

        // Same as ISO9660 but 16 bytes, no timezone byte
        DateHandlers.HighSierraToDateTime("2001010203040500"u8.ToArray())
                    .Should()
                    .Be(new DateTime(2001, 1, 2, 3, 4, 5));

    [Test]
    public void EcmaUtcTimestamp() => DateHandlers.EcmaToDateTime(0, 2000, 1, 2, 3, 4, 5, 50, 0, 0)
                                                  .Should()
                                                  .Be(new DateTime(2000, 1, 2, 3, 4, 5, 500));

    [Test]
    public void EcmaOffsetMinus2047MeansUnspecified() =>

        // preOffset 0x801 with bit 0x800 set -> offset -2047 -> Unspecified kind, no adjustment
        DateHandlers.EcmaToDateTime(0x1801, 2000, 1, 2, 3, 4, 5, 0, 0, 0)
                    .Should()
                    .Be(new DateTime(2000, 1, 2, 3, 4, 5));

    [Test]
    public void EcmaOutOfRangeOffsetIsClampedToZero() =>

        // preOffset 1500 (> 1440) -> clamped to 0
        DateHandlers.EcmaToDateTime(0x15DC, 2000, 1, 2, 3, 4, 5, 0, 0, 0)
                    .Should()
                    .Be(new DateTime(2000, 1, 2, 3, 4, 5));

    [Test]
    public void ExFatZeroReturnsNull() => Assert.That(DateHandlers.ExFatToDateTime(0), Is.Null);

    [Test]
    public void ExFatKnownTimestamp()
    {
        // year 45 (2025), month 8, day 1, hour 12, minute 30, 15 double-seconds (30 s)
        const uint timestamp = 45u << 25 | 8 << 21 | 1 << 16 | 12 << 11 | 30 << 5 | 15;

        Assert.That(DateHandlers.ExFatToDateTime(timestamp), Is.EqualTo(new DateTime(2025, 8, 1, 12, 30, 30)));
    }

    [Test]
    public void ExFatTenMsIncrementIsAdded()
    {
        const uint timestamp = 45u << 25 | 8 << 21 | 1 << 16;

        Assert.That(DateHandlers.ExFatToDateTime(timestamp, 50),
                    Is.EqualTo(new DateTime(2025, 8, 1).AddMilliseconds(500)));
    }

    [Test]
    public void ExFatUtcOffsetIsApplied()
    {
        const uint timestamp = 45u << 25 | 8 << 21 | 1 << 16 | 12 << 11;

        // Valid offset (bit 7), +4 quarters -> minus one hour
        Assert.That(DateHandlers.ExFatToDateTime(timestamp, 0, 0x80 | 4),
                    Is.EqualTo(new DateTime(2025, 8, 1, 11, 0, 0)));

        // Valid offset, -4 quarters (two's complement 124) -> plus one hour
        Assert.That(DateHandlers.ExFatToDateTime(timestamp, 0, 0x80 | 124),
                    Is.EqualTo(new DateTime(2025, 8, 1, 13, 0, 0)));
    }

    [Test]
    public void ExFatInvalidMonthReturnsNull()
    {
        // month 0
        const uint timestamp = 45u << 25 | 1 << 16;

        Assert.That(DateHandlers.ExFatToDateTime(timestamp), Is.Null);
    }
}