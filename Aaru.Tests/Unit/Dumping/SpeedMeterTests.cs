// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SpeedMeterTests.cs
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
using Aaru.Core.Devices.Dumping;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Dumping;

[TestFixture]
public class SpeedMeterTests
{
    const uint BLOCK_SIZE = 2448;
    const uint READ_BYTES = 32 * BLOCK_SIZE;

    /// <summary>Feeds reads of 32 sectors at a fixed interval, returning the end time and the highest speed</summary>
    static TimeSpan Feed(SpeedMeter meter, TimeSpan start, double intervalSeconds, double seconds, ref double speed,
                         out double fastest)
    {
        fastest = 0;
        int reads = (int)(seconds / intervalSeconds);

        for(int r = 1; r <= reads; r++)
        {
            meter.Update(READ_BYTES, start + TimeSpan.FromSeconds(r * intervalSeconds), ref speed);

            if(speed > fastest) fastest = speed;
        }

        return start + TimeSpan.FromSeconds(reads * intervalSeconds);
    }

    [Test]
    public void DoesNotReportBeforeFirstBucketCloses()
    {
        SpeedMeter meter = new();
        double     speed = 123;

        meter.Update(READ_BYTES, TimeSpan.Zero, ref speed).Should().BeFalse();
        meter.Update(READ_BYTES, TimeSpan.FromMilliseconds(50), ref speed).Should().BeFalse();
        speed.Should().Be(123);
    }

    [Test]
    public void FirstBucketUsesItsOwnLength()
    {
        SpeedMeter meter = new();
        double     speed = 0;

        meter.Update(READ_BYTES, TimeSpan.Zero, ref speed);
        meter.Update(1048576, TimeSpan.FromMilliseconds(100), ref speed).Should().BeTrue();
        speed.Should().BeApproximately(10, 0.001);
    }

    [Test]
    public void SpinUpBeforeFirstReadDoesNotLowerSpeed()
    {
        SpeedMeter meter = new();
        double     speed = 0;

        // The first read returns after 15 seconds of spinning up, followed by reads at 48x
        TimeSpan spunUp = TimeSpan.FromSeconds(15);
        meter.Update(READ_BYTES, spunUp, ref speed).Should().BeFalse();

        double expected = 3600d * BLOCK_SIZE / 1048576;

        for(int r = 1; r <= 12; r++) meter.Update(READ_BYTES, spunUp + TimeSpan.FromSeconds(r * 32d / 3600), ref speed);

        speed.Should().BeApproximately(expected, 0.5);
    }

    [Test]
    public void SteadyReadsReportTheirRate()
    {
        SpeedMeter meter = new();
        double     speed = 0;

        // 48x is 3600 sectors per second
        Feed(meter, TimeSpan.Zero, 32d / 3600, 10, ref speed, out _);

        speed.Should().BeApproximately(3600d * BLOCK_SIZE / 1048576, 0.1);
    }

    [Test]
    public void TimeBetweenReadsIsCounted()
    {
        SpeedMeter meter = new();
        double     speed = 0;

        // Reads returning from cache in 2 ms with 10 ms of host work between them
        Feed(meter, TimeSpan.Zero, 0.012, 10, ref speed, out double fastest);

        double expected = READ_BYTES / 0.012 / 1048576;

        speed.Should().BeApproximately(expected, 0.1);
        fastest.Should().BeLessThan(expected * 1.1);
    }

    [Test]
    public void BurstFromCacheAfterStallStaysNearTrueRate()
    {
        SpeedMeter meter = new();
        double     speed = 0;

        // 8x
        const double interval = 32d / 600;
        TimeSpan     now      = Feed(meter, TimeSpan.Zero, interval, 10, ref speed, out _);
        double       steady   = speed;

        // The host stalls while the drive fills 4 MiB of cache, that is then read at bus speed
        now += TimeSpan.FromSeconds(2.8);

        const int burstReads = (int)(4194304 / READ_BYTES);
        double    fastest    = 0;

        for(int r = 1; r <= burstReads; r++)
        {
            meter.Update(READ_BYTES, now + TimeSpan.FromSeconds(0.12 * r / burstReads), ref speed);

            if(speed > fastest) fastest = speed;
        }

        now += TimeSpan.FromSeconds(0.12);

        Feed(meter, now, interval, 10, ref speed, out double fastestAfter);

        // Command time alone would report 33 MiB/s here
        Math.Max(fastest, fastestAfter).Should().BeLessThan(steady * 2.5);
        speed.Should().BeApproximately(steady, 0.1);
    }

    [Test]
    public void RecoversAfterLongIdleGap()
    {
        SpeedMeter meter = new();
        double     speed = 0;

        TimeSpan now    = Feed(meter, TimeSpan.Zero, 0.01, 5, ref speed, out _);
        double   steady = speed;

        now += TimeSpan.FromSeconds(30);

        meter.Update(READ_BYTES, now, ref speed);
        speed.Should().BeLessThan(steady / 2);

        Feed(meter, now, 0.01, 5, ref speed, out _);
        speed.Should().BeApproximately(steady, 0.1);
    }

    [Test]
    public void NoBytesReportNoSpeed()
    {
        SpeedMeter meter = new();
        double     speed = 5;

        meter.Update(0, TimeSpan.Zero, ref speed);
        meter.Update(0, TimeSpan.FromMilliseconds(200), ref speed).Should().BeTrue();
        speed.Should().Be(0);
    }

    [Test]
    public void ReadsThatTransferNothingLowerTheSpeed()
    {
        SpeedMeter meter = new();
        double     speed = 0;

        for(var ms = 0; ms <= 1000; ms += 100) meter.Update(1048576, TimeSpan.FromMilliseconds(ms), ref speed);

        double before = speed;

        for(var ms = 1100; ms <= 2000; ms += 100) meter.Update(0, TimeSpan.FromMilliseconds(ms), ref speed);

        speed.Should().BeLessThan(before);
    }
}