// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DpmAnalysisTests.cs
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
using System.Linq;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Image;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.DataPosition;

[TestFixture]
public class DpmAnalysisTests
{
    const double R0            = 24.5e-3;
    const double SECTOR_LENGTH = 17.3e-3;
    const double PITCH         = 1.6e-6;

    /// <summary>Degrees per sector of a spiral at a sector, odd layers spiralling back inward if opposite</summary>
    static double Spiral(double lba, ulong[] layerEnds, bool opposite)
    {
        double x     = lba;
        int    layer = layerEnds.Count(e => lba > e);

        if(layer > 0)
        {
            double layerStart    = layerEnds[layer - 1] + 1;
            double previousStart = layer == 1 ? 0 : layerEnds[layer - 2] + 1;
            x = lba - layerStart;

            if(opposite && layer % 2 == 1) x = layerEnds[layer - 1] - previousStart - x;
        }

        double radius = Math.Sqrt(R0 * R0 + x * SECTOR_LENGTH * PITCH / Math.PI);

        return 360.0 * SECTOR_LENGTH / (2 * Math.PI * radius);
    }

    /// <summary>Builds a DPM of a spiral</summary>
    /// <param name="spacing">Sectors between entries</param>
    /// <param name="sectors">Last sector</param>
    /// <param name="layerEnds">Last sector of every layer but the last</param>
    /// <param name="opposite">Whether odd layers spiral back inward</param>
    /// <param name="noise">Relative standard deviation of the density of every bin</param>
    /// <param name="density">Density changes, multiplying the spiral at a sector</param>
    static DataPositionMeasurement Build(uint   spacing, ulong sectors, ulong[] layerEnds = null,
                                         bool   opposite = false, double noise = 0,
                                         Func<ulong, double> density = null)
    {
        layerEnds ??= [];

        var    random  = new Random(1);
        int    count   = (int)(sectors / spacing) + 1;
        var    entries = new DpmEntry[count];
        double angle   = 0;

        for(int i = 1; i < count; i++)
        {
            ulong  lba    = (ulong)i * spacing;
            ulong  middle = lba - spacing / 2;
            double gauss  = Math.Sqrt(-2.0 * Math.Log(1.0 - random.NextDouble())) *
                            Math.Cos(2.0 * Math.PI * random.NextDouble());
            double d = Spiral(middle, layerEnds, opposite) * (density?.Invoke(middle) ?? 1.0) * (1.0 + noise * gauss);

            angle += d * spacing / 360.0 * Dpm.UNITS_PER_TURN;

            entries[i] = new DpmEntry
            {
                Lba    = lba,
                Angle  = (ulong)Math.Round(angle),
                Status = DpmEntryStatus.Measured
            };
        }

        return new DataPositionMeasurement
        {
            NominalSpacing    = spacing,
            TimingUnit        = 1,
            OppositeTrackPath = opposite,
            LayerEnds         = layerEnds,
            Entries           = entries,
            Calibrations      = []
        };
    }

    [Test]
    public void InvalidDpm()
    {
        DpmAnalysis.Analyse(new DataPositionMeasurement()).Should().BeNull();
    }

    [Test]
    public void Bins()
    {
        DataPositionMeasurement dpm    = Build(1000, 100000);
        DpmAnalysisResult       result = DpmAnalysis.Analyse(dpm).Value;

        result.Entries.Should().Be(101);
        result.Bins.Should().HaveCount(100);
        result.FirstLba.Should().Be(0);
        result.LastLba.Should().Be(100000);
        result.TotalTurns.Should().BeApproximately((double)dpm.Entries[^1].Angle / Dpm.UNITS_PER_TURN, 1e-9);

        for(int i = 0; i < result.Bins.Length; i++)
        {
            DpmBin bin = result.Bins[i];

            bin.FirstLba.Should().Be(dpm.Entries[i].Lba);
            bin.EndLba.Should().Be(dpm.Entries[i + 1].Lba);
            bin.Density.Should().BeApproximately(Spiral(bin.FirstLba + 500, [], false), 1e-4);
            bin.SectorsPerTurn.Should().BeApproximately(360.0 / bin.Density, 1e-9);
            bin.Measured.Should().BeTrue();
        }

        result.Bins[50].Turn.Should().BeApproximately((double)dpm.Entries[50].Angle / Dpm.UNITS_PER_TURN, 1e-9);
    }

    [Test]
    public void StatusCounts()
    {
        DataPositionMeasurement dpm = Build(1000, 100000);
        dpm.Entries[10].Status = DpmEntryStatus.Interpolated;
        dpm.Entries[11].Status = DpmEntryStatus.Unreadable;
        dpm.Entries[12].Status = DpmEntryStatus.Unreadable;
        dpm.Entries[20].Status = DpmEntryStatus.Remeasured;
        dpm.Entries[30].Status = DpmEntryStatus.LayerBreak;

        DpmAnalysisResult result = DpmAnalysis.Analyse(dpm).Value;

        result.StatusCounts[(int)DpmEntryStatus.Measured].Should().Be(95);
        result.StatusCounts[(int)DpmEntryStatus.Remeasured].Should().Be(1);
        result.StatusCounts[(int)DpmEntryStatus.Interpolated].Should().Be(1);
        result.StatusCounts[(int)DpmEntryStatus.Unreadable].Should().Be(2);
        result.StatusCounts[(int)DpmEntryStatus.LayerBreak].Should().Be(1);

        // A bin takes the status of the entry it ends at
        result.Bins[9].Measured.Should().BeFalse();
        result.Bins[10].Measured.Should().BeFalse();
        result.Bins[19].Measured.Should().BeTrue();
        result.Bins[29].Measured.Should().BeFalse();
        result.Layers.Single().MeasuredBins.Should().Be(96);
    }

    [Test]
    public void Layers()
    {
        DataPositionMeasurement dpm    = Build(1000, 200000, [99999], true);
        DpmAnalysisResult       result = DpmAnalysis.Analyse(dpm).Value;

        result.Layers.Should().HaveCount(2);

        DpmLayerSummary first  = result.Layers[0];
        DpmLayerSummary second = result.Layers[1];

        first.FirstLba.Should().Be(0);
        first.Bins.Should().Be(100);
        second.FirstLba.Should().Be(100000);
        second.LastLba.Should().Be(200000);
        second.Bins.Should().Be(100);
        (first.Turns + second.Turns).Should().BeApproximately(result.TotalTurns, 1e-6);

        // The first layer spirals outward, the second one back inward
        first.FirstSectorsPerTurn.Should().BeLessThan(first.LastSectorsPerTurn);
        second.FirstSectorsPerTurn.Should().BeGreaterThan(second.LastSectorsPerTurn);
        first.MinimumDensity.Should().BeLessThan(first.MedianDensity);
        first.MaximumDensity.Should().BeGreaterThan(first.MedianDensity);
    }

    [Test]
    public void Calibrations()
    {
        DataPositionMeasurement dpm = Build(1000, 200000, [99999]);

        dpm.Calibrations =
        [
            new DpmCalibration
            {
                Lba            = 0,
                RotationPeriod = 120_000_000,
                SectorsPerTurn = 9_000_000
            },
            new DpmCalibration
            {
                Lba            = 150000,
                RotationPeriod = 60_000_000,
                SectorsPerTurn = 21_500
            },
            new DpmCalibration
            {
                Lba = 160000
            }
        ];

        DpmAnalysisResult result = DpmAnalysis.Analyse(dpm).Value;

        result.Calibrations.Should().HaveCount(2);
        result.Calibrations[0].Rpm.Should().BeApproximately(500, 1e-9);
        result.Calibrations[0].RotationPeriod.Should().BeApproximately(120, 1e-9);
        result.Calibrations[0].SectorsPerTurn.Should().BeApproximately(9000, 1e-9);
        result.Calibrations[0].Layer.Should().Be(0);
        result.Calibrations[1].Rpm.Should().BeApproximately(1000, 1e-9);
        result.Calibrations[1].SectorsPerTurn.Should().BeApproximately(21.5, 1e-9);
        result.Calibrations[1].Layer.Should().Be(1);
    }

    [Test]
    public void FitCleanSpiral()
    {
        DpmAnalysisResult result = DpmAnalysis.Analyse(Build(500, 300000, noise: 0.0005)).Value;

        result.Layers.Single().Fitted.Should().BeTrue();
        result.Layers.Single().FitB.Should().BePositive();
        result.Layers.Single().BandThreshold.Should().BeApproximately(0.005, 1e-9);
        result.Bins.Max(static b => Math.Abs(b.Fit / Spiral(b.FirstLba + 250, [], false) - 1))
              .Should()
              .BeLessThan(0.001);
        result.Bands.Should().BeEmpty();
    }

    [Test]
    public void FitDenseBand()
    {
        DpmAnalysisResult result =
            DpmAnalysis.Analyse(Build(500,
                                      300000,
                                      noise: 0.0005,
                                      density: static lba => lba is >= 100000 and < 110000 ? 1.023 : 1.0))
                       .Value;

        DpmBand band = result.Bands.Should().ContainSingle().Subject;

        band.FirstLba.Should().Be(100000);
        band.LastLba.Should().Be(109999);
        band.Bins.Should().Be(20);
        band.MeanDeviation.Should().BeApproximately(0.023, 0.002);
    }

    [Test]
    public void FitAlternatingBand()
    {
        // Denser and sparser bins one after the other, the fit must not flatten them
        DpmAnalysisResult result =
            DpmAnalysis.Analyse(Build(500,
                                      300000,
                                      noise: 0.0005,
                                      density: static lba => lba is >= 200000 and < 205000
                                                                 ? lba / 500 % 2 == 0 ? 1.02 : 0.98
                                                                 : 1.0))
                       .Value;

        DpmBand band = result.Bands.Should().ContainSingle().Subject;

        band.FirstLba.Should().Be(200000);
        band.Bins.Should().Be(10);
        Math.Abs(band.MaximumDeviation).Should().BeApproximately(0.02, 0.002);
    }

    [Test]
    public void FitOppositeTrackPath()
    {
        DpmAnalysisResult result =
            DpmAnalysis.Analyse(Build(1000, 400000, [199999], true, 0.0005)).Value;

        result.Layers.Should().HaveCount(2);
        result.Layers.Should().OnlyContain(static l => l.Fitted);
        result.Layers[0].FitB.Should().BePositive();
        result.Layers[1].FitB.Should().BeNegative();
        result.Bands.Should().BeEmpty();
    }

    [Test]
    public void FitIgnoresUnmeasuredBins()
    {
        // Bins that were interpolated have a wrong density, that must neither bend the fit nor make a band
        DataPositionMeasurement dpm =
            Build(500, 300000, noise: 0.0005, density: static lba => lba is >= 50000 and < 55000 ? 1.5 : 1.0);

        for(int i = 101; i <= 110; i++) dpm.Entries[i].Status = DpmEntryStatus.Unreadable;

        DpmAnalysisResult result = DpmAnalysis.Analyse(dpm).Value;

        result.Bins.Count(static b => !b.Measured).Should().Be(10);
        result.Bins.Where(static b => !b.Measured).Should().OnlyContain(static b => double.IsNaN(b.Deviation));
        result.Bins.Where(static b => !b.Measured).Should().OnlyContain(static b => !double.IsNaN(b.Fit));
        result.Layers.Single().MaximumDeviation.Should().BeLessThan(0.005);
        result.Bands.Should().BeEmpty();
    }
}