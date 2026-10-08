// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DpmMeasurementTests.cs
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
using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Devices.Dpm;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.DataPosition;

/// <summary>A simulated constant angular velocity drive, timing reads from a known angle of every sector</summary>
sealed class SimulatedDrive : IDpmDrive
{
    readonly double[]      _cumulative;
    readonly DpmMedium     _medium;
    readonly Random        _random;
    readonly int           _resolution;
    readonly HashSet<uint> _unreadable;
    readonly uint          _unit;
    uint                   _currentLba;
    double                 _phase;
    double                 _previousEnd = -1;
    double                 _time;

    /// <param name="medium">Medium the drive reports</param>
    /// <param name="cumulative">Angle, in rotations, at every <paramref name="resolution" /> sectors</param>
    /// <param name="resolution">Sectors between values of <paramref name="cumulative" /></param>
    /// <param name="unit">Sectors the drive reads as a whole</param>
    /// <param name="unreadable">Unreadable sectors</param>
    /// <param name="seed">Random seed</param>
    public SimulatedDrive(DpmMedium     medium, double[] cumulative, int resolution, uint unit,
                          HashSet<uint> unreadable, int seed)
    {
        _medium     = medium;
        _cumulative = cumulative;
        _resolution = resolution;
        _unit       = unit;
        _unreadable = unreadable;
        _random     = new Random(seed);
    }

    /// <summary>Nominal rotation period, s</summary>
    public double Period { get; init; } = 0.0080;
    /// <summary>Command overhead, s</summary>
    public double Overhead { get; init; } = 0.0011;
    /// <summary>Probability of waiting an extra rotation</summary>
    public double MissProbability { get; init; }
    /// <summary>Probability of a random extra delay</summary>
    public double JunkProbability { get; init; }
    /// <summary>Rotation period multiplier past the first layer</summary>
    public double UpperLayerSlowdown { get; init; } = 1.0;
    /// <summary>Maximum seconds the host takes between commands, random, the disc turning meanwhile</summary>
    public double HostDelay { get; init; }
    /// <summary>Seconds a failed read costs</summary>
    public double FailureCost { get; init; } = 2.0;
    /// <summary>Reads that hit unreadable sectors</summary>
    public int FailedReads { get; private set; }

    /// <inheritdoc />
    public long Now => (long)(_time * 1e9);

    /// <inheritdoc />
    public bool TimedRead(uint lba, out long elapsed, out long sincePrevious)
    {
        HostPause();

        double start = _time;
        bool   ok    = Read(lba, 1);
        elapsed       = (long)((_time - start) * 1e9);
        sincePrevious = _previousEnd < 0 ? elapsed : (long)((_time - _previousEnd) * 1e9);
        _previousEnd  = _time;

        return ok;
    }

    /// <inheritdoc />
    public void ReadUntimed(uint lba, ushort blocks)
    {
        HostPause();
        Read(lba, blocks);
        _previousEnd = _time;
    }

    /// <summary>The time the host takes before sending a command, logging, updating progress or collecting garbage</summary>
    void HostPause()
    {
        if(HostDelay > 0) Advance(_random.NextDouble() * HostDelay);
    }

    /// <inheritdoc />
    public bool SetSpeed(ushort kbps)
    {
        _previousEnd = -1;

        return true;
    }

    /// <inheritdoc />
    public bool DisableReadCache() => false;

    /// <inheritdoc />
    public void RestoreReadCache() {}

    /// <inheritdoc />
    public bool DetectMedium(out DpmMedium medium)
    {
        medium = _medium;

        return true;
    }

    /// <summary>Angle of a sector in rotations</summary>
    public double SectorAngle(double lba)
    {
        int k = (int)(lba / _resolution);

        if(k >= _cumulative.Length - 1) k = _cumulative.Length - 2;

        double f = (lba - k * (double)_resolution) / _resolution;

        return _cumulative[k] + (_cumulative[k + 1] - _cumulative[k]) * f;
    }

    double PeriodNow() =>
        _medium.LayerEnds.Length > 0 && _currentLba > _medium.LayerEnds[0] ? Period * UpperLayerSlowdown : Period;

    void Advance(double dt)
    {
        // Integrate the phase with small steps so a varying period is honoured
        while(dt > 0)
        {
            double h = dt > 1e-4 ? 1e-4 : dt;
            _phase += h / PeriodNow();
            _time  += h;
            dt     -= h;
        }
    }

    bool Read(uint lba, uint blocks)
    {
        _currentLba = lba;

        uint first = lba   - lba % _unit;
        uint last  = (lba + blocks + _unit - 1) / _unit * _unit;

        for(uint s = first; s < last; s++)
        {
            if(!_unreadable.Contains(s)) continue;

            // The drive retries, then fails
            FailedReads++;
            Advance(FailureCost);

            return false;
        }

        Advance(Overhead + 0.0001 * _random.NextDouble());

        double target = SectorAngle(first) % 1.0;
        double wait   = (target - _phase % 1.0 + 2.0) % 1.0;

        if(_random.NextDouble() < MissProbability) wait += 1.0;

        Advance(wait * PeriodNow());

        if(_random.NextDouble() < JunkProbability) Advance(_random.NextDouble() * PeriodNow());

        Advance((SectorAngle(last) - SectorAngle(first)) * PeriodNow());

        return true;
    }
}

[TestFixture]
[Category("Unit")]
public class DpmMeasurementTests
{
    /// <summary>Builds the angle, in rotations, at every <paramref name="resolution" /> sectors of a spiral</summary>
    /// <param name="sectors">Sectors</param>
    /// <param name="resolution">Sectors between values</param>
    /// <param name="sectorLength">Sector length along the track, m</param>
    /// <param name="pitch">Track pitch, m</param>
    /// <param name="layerEnds">Last sector of every layer but the last</param>
    /// <param name="bands">Ranges of sectors, as [start, end), that are 2.3% denser</param>
    static double[] BuildSpiral(uint sectors, int resolution, double sectorLength, double pitch, uint[] layerEnds,
                                (uint start, uint end)[] bands)
    {
        const double r0     = 24.5e-3;
        var          result = new double[sectors / resolution + 2];
        double       angle  = 0;

        for(int i = 1; i < result.Length; i++)
        {
            double lba = (i - 0.5) * resolution;

            // Radial position, every odd layer spiralling back inward
            double x     = lba;
            int    layer = layerEnds.Count(e => lba > e);

            if(layer > 0)
            {
                double layerStart    = layerEnds[layer - 1] + 1;
                double previousStart = layer == 1 ? 0 : layerEnds[layer - 2] + 1;
                x = lba - layerStart;

                if(layer % 2 == 1) x = layerEnds[layer - 1] - previousStart - x;
            }

            double radius   = Math.Sqrt(r0 * r0 + x * sectorLength * pitch / Math.PI);
            double turns    = sectorLength / (2 * Math.PI * radius);
            bool   inBand   = bands.Any(b => lba >= b.start && lba < b.end);

            angle     += turns * resolution * (inBand ? 1.023 : 1.0);
            result[i] =  angle;
        }

        return result;
    }

    /// <summary>Checks every measured bin against the real density</summary>
    static void ShouldMatch(DataPositionMeasurement dpm, SimulatedDrive drive, double maxRms, double maxError)
    {
        DpmEntry[]   entries = dpm.Entries;
        List<double> errors  = [];

        for(int j = 0; j + 1 < entries.Length; j++)
        {
            DpmEntry first = entries[j], last = entries[j + 1];

            if(last.Status is not (DpmEntryStatus.Measured or DpmEntryStatus.Remeasured)) continue;

            double measured = (double)(last.Angle - first.Angle) / Aaru.CommonTypes.Structs.Dpm.UNITS_PER_TURN;
            double real     = drive.SectorAngle(last.Lba) - drive.SectorAngle(first.Lba);

            errors.Add(measured / real - 1.0);
        }

        errors.Should().NotBeEmpty();

        double rms   = Math.Sqrt(errors.Average(e => e * e));
        double worst = errors.Max(Math.Abs);

        TestContext.Out.WriteLine($"{errors.Count} bins, rms {rms:P3}, worst {worst:P3}, " +
                                  $"{dpm.Calibrations.Length} calibrations");

        rms.Should().BeLessThan(maxRms);
        worst.Should().BeLessThan(maxError);

        // The whole angle must be right too, no whole rotation may be lost
        double totalMeasured = (double)entries[^1].Angle / Aaru.CommonTypes.Structs.Dpm.UNITS_PER_TURN;
        double totalReal     = drive.SectorAngle(entries[^1].Lba) - drive.SectorAngle(entries[0].Lba);

        Math.Abs(totalMeasured - totalReal).Should().BeLessThan(0.5);
    }

    [Test]
    public void CompactDisc()
    {
        var medium = new DpmMedium
        {
            Kind      = DpmMediumKind.Cd,
            LastLba   = 334188,
            LayerEnds = []
        };

        double[] spiral = BuildSpiral(334189, 50, 1.2 / 75, 1.6e-6, [], [(2000, 6000), (9000, 12000)]);

        var drive = new SimulatedDrive(medium, spiral, 50, 1, [], 1)
        {
            MissProbability = 0.01,
            JunkProbability = 0.01
        };

        var measurement = new DpmMeasurement(drive);
        measurement.Prepare(0).Should().BeTrue();

        DataPositionMeasurement? dpm = measurement.Measure(0, 15000, 0);

        dpm.Should().NotBeNull();
        Aaru.CommonTypes.Structs.Dpm.Validate(dpm.Value).Should().BeTrue();
        dpm.Value.NominalSpacing.Should().Be(50);
        dpm.Value.TimingUnit.Should().Be(1);
        dpm.Value.Speed.Should().Be(0xFFFF);
        dpm.Value.Entries[^1].Lba.Should().Be(15000);
        dpm.Value.Calibrations.Should().NotBeEmpty();

        ShouldMatch(dpm.Value, drive, 0.005, 0.03);
    }

    [Test]
    public void CompactDiscWithUnreadableRing()
    {
        var medium = new DpmMedium
        {
            Kind      = DpmMediumKind.Cd,
            LastLba   = 334188,
            LayerEnds = []
        };

        double[] spiral = BuildSpiral(334189, 50, 1.2 / 75, 1.6e-6, [], [(2000, 6000)]);

        // A ring of unreadable sectors with a readable island, and a few known before measuring
        HashSet<uint> unreadable = [];

        for(uint s = 7000; s < 8500; s++)
            if(s is < 7600 or >= 7700)
                unreadable.Add(s);

        var drive = new SimulatedDrive(medium, spiral, 50, 1, unreadable, 2);

        var measurement = new DpmMeasurement(drive);
        measurement.Prepare(0).Should().BeTrue();
        measurement.SeedUnreadable(Enumerable.Range(8000, 500).Select(s => (ulong)s));

        DataPositionMeasurement? dpm = measurement.Measure(0, 12000, 0);

        dpm.Should().NotBeNull();
        dpm.Value.Entries.Should().Contain(e => e.Status == DpmEntryStatus.Unreadable);

        ShouldMatch(dpm.Value, drive, 0.005, 0.03);

        // Known unreadable sectors are never read
        drive.FailedReads.Should().BeLessThan(40);
    }

    [Test]
    public void DualLayerDvdWithSpeedDrop()
    {
        const uint layer0End = 2084959;

        var medium = new DpmMedium
        {
            Kind              = DpmMediumKind.Dvd,
            LastLba           = 4169919,
            LayerEnds         = [layer0End],
            OppositeTrackPath = true,
            ChannelBit        = 0.293e-6 / 2
        };

        double[] spiral = BuildSpiral(4169920,
                                      16,
                                      38688 * 0.293e-6 / 2,
                                      0.74e-6,
                                      [layer0End],
                                      [(2050000, 2070000), (2100000, 2120000)]);

        var drive = new SimulatedDrive(medium, spiral, 16, 16, [], 3)
        {
            Period             = 0.0077,
            UpperLayerSlowdown = 1.5,
            JunkProbability    = 0.005
        };

        var measurement = new DpmMeasurement(drive);
        measurement.Prepare(0).Should().BeTrue();

        DataPositionMeasurement? dpm = measurement.Measure(2040000, 2130000, 0);

        dpm.Should().NotBeNull();
        dpm.Value.LayerEnds.Should().Equal(layer0End);
        dpm.Value.TimingUnit.Should().Be(16);
        dpm.Value.Entries.Should().Contain(e => e.Lba == layer0End);
        dpm.Value.Entries.Should().Contain(e => e.Lba == layer0End + 1);

        // The second layer is calibrated on its own
        dpm.Value.Calibrations.Should().Contain(c => c.Lba > layer0End);

        ShouldMatch(dpm.Value, drive, 0.005, 0.03);
    }

    [Test]
    public void CompactDiscWithHostDelays()
    {
        var medium = new DpmMedium
        {
            Kind      = DpmMediumKind.Cd,
            LastLba   = 334188,
            LayerEnds = []
        };

        double[] spiral = BuildSpiral(334189, 50, 1.2 / 75, 1.6e-6, [], [(2000, 6000), (9000, 12000)]);

        // Up to 3 ms between commands, more than writing the debug log takes, which is a quarter of a turn here
        var drive = new SimulatedDrive(medium, spiral, 50, 1, [], 5)
        {
            HostDelay = 0.003
        };

        var measurement = new DpmMeasurement(drive);
        measurement.Prepare(0).Should().BeTrue();

        DataPositionMeasurement? dpm = measurement.Measure(0, 15000, 0);

        dpm.Should().NotBeNull();
        ShouldMatch(dpm.Value, drive, 0.005, 0.03);
    }

    [Test]
    public void DualLayerDvdWithHostDelays()
    {
        const uint layer0End = 2084959;

        var medium = new DpmMedium
        {
            Kind              = DpmMediumKind.Dvd,
            LastLba           = 4169919,
            LayerEnds         = [layer0End],
            OppositeTrackPath = true,
            ChannelBit        = 0.293e-6 / 2
        };

        double[] spiral = BuildSpiral(4169920,
                                      16,
                                      38688 * 0.293e-6 / 2,
                                      0.74e-6,
                                      [layer0End],
                                      [(2050000, 2070000), (2100000, 2120000)]);

        var drive = new SimulatedDrive(medium, spiral, 16, 16, [], 6)
        {
            Period             = 0.0077,
            UpperLayerSlowdown = 1.5,
            HostDelay          = 0.003
        };

        var measurement = new DpmMeasurement(drive);
        measurement.Prepare(0).Should().BeTrue();

        DataPositionMeasurement? dpm = measurement.Measure(2040000, 2130000, 0);

        dpm.Should().NotBeNull();
        ShouldMatch(dpm.Value, drive, 0.005, 0.03);
    }

    [TestCase(3)]
    [TestCase(4)]
    public void MultiLayerBluray(int layers)
    {
        const uint layerSize = 100000;
        uint[]     layerEnds = Enumerable.Range(1, layers - 1).Select(l => (uint)(l * layerSize - 1)).ToArray();

        var medium = new DpmMedium
        {
            Kind              = DpmMediumKind.Bd,
            LastLba           = (uint)(layers * layerSize - 1),
            LayerEnds         = layerEnds,
            OppositeTrackPath = true,
            ChannelBit        = 74.5e-9
        };

        double[] spiral = BuildSpiral(medium.LastLba + 1,
                                      32,
                                      498.0 * 1932 / 32 * 74.5e-9,
                                      0.32e-6,
                                      layerEnds,
                                      [(120000, 140000)]);

        var drive = new SimulatedDrive(medium, spiral, 32, 32, [], 4)
        {
            Period          = 0.0125,
            JunkProbability = 0.005
        };

        var measurement = new DpmMeasurement(drive);
        measurement.Prepare(0).Should().BeTrue();

        DataPositionMeasurement? dpm = measurement.Measure(80000, 220000, 0);

        dpm.Should().NotBeNull();
        dpm.Value.TimingUnit.Should().Be(32);
        dpm.Value.LayerEnds.Should().HaveCount(layers - 1);

        // Both crossed layer breaks are control points on both sides
        foreach(uint end in layerEnds.Where(e => e is > 80000 and < 220000))
        {
            dpm.Value.Entries.Should().Contain(e => e.Lba == end);
            dpm.Value.Entries.Should().Contain(e => e.Lba == end + 1);
        }

        ShouldMatch(dpm.Value, drive, 0.005, 0.03);
    }
}