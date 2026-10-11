// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DpmComparisonTests.cs
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
using Aaru.CommonTypes.Structs;
using Aaru.Core.Image;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.DataPosition;

[TestFixture]
public class DpmComparisonTests
{
    const ulong START   = 0;
    const ulong SECTORS = 300000;

    /// <summary>Degrees per sector of a CD-like spiral, decreasing with the radius</summary>
    static double Profile(ulong lba) => 36.0 / Math.Sqrt(1.0 + lba / 20000.0);

    /// <summary>Builds a DPM from a density profile</summary>
    /// <param name="spacing">Sectors between entries</param>
    /// <param name="scale">Calibration error, multiplying every density</param>
    /// <param name="noise">Relative standard deviation of the density of every bin</param>
    /// <param name="seed">Random seed</param>
    /// <param name="density">Density changes, multiplying the profile at a sector</param>
    /// <param name="end">Last sector</param>
    static DataPositionMeasurement Build(uint                spacing, double scale, double noise, int seed,
                                         Func<ulong, double> density = null, ulong end = SECTORS)
    {
        var    random  = new Random(seed);
        int    count   = (int)((end - START) / spacing) + 1;
        var    entries = new DpmEntry[count];
        double angle   = 0;

        entries[0] = new DpmEntry
        {
            Lba = START
        };

        for(int i = 1; i < count; i++)
        {
            ulong  lba    = START + (ulong)i * spacing;
            ulong  middle = lba - spacing / 2;
            double gauss  = Math.Sqrt(-2.0 * Math.Log(1.0 - random.NextDouble())) *
                            Math.Cos(2.0 * Math.PI * random.NextDouble());
            double d = Profile(middle) * (density?.Invoke(middle) ?? 1.0) * scale * (1.0 + noise * gauss);

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
            NominalSpacing = spacing,
            TimingUnit     = 1,
            LayerEnds      = [],
            Entries        = entries,
            Calibrations   = []
        };
    }

    [Test]
    public void SameDiscWithCalibrationError()
    {
        DpmComparisonResult result = DpmComparison.Compare(Build(500, 1.0, 0.001, 1), Build(500, 1.0005, 0.001, 2));

        result.Verdict.Should().Be(DpmComparisonVerdict.Match);
        result.Scales.Should().ContainSingle().Which.Should().BeApproximately(1.0005, 0.0002);
        result.Differences.Should().BeEmpty();
        result.MedianDeviation.Should().BeLessThan(0.002);
    }

    [Test]
    public void DensityBand()
    {
        DpmComparisonResult result =
            DpmComparison.Compare(Build(500, 1.0, 0.001, 1),
                                  Build(500, 1.0, 0.001, 2, static lba => lba is >= 100000 and < 110000 ? 1.03 : 1.0));

        result.Verdict.Should().Be(DpmComparisonVerdict.Differ);
        result.Differences.Should().ContainSingle();
        result.Differences[0].FirstLba.Should().Be(100000);
        result.Differences[0].LastLba.Should().Be(109999);
        result.Differences[0].MeanDeviation.Should().BeApproximately(0.03, 0.005);
    }

    [Test]
    public void IsolatedMisread()
    {
        DpmComparisonResult result =
            DpmComparison.Compare(Build(500, 1.0, 0.001, 1),
                                  Build(500, 1.0, 0.001, 2, static lba => lba is >= 150000 and < 150500 ? 1.1 : 1.0));

        result.Verdict.Should().Be(DpmComparisonVerdict.Match);
        result.IsolatedOutliers.Should().Be(1);
    }

    [Test]
    public void DifferentSpacing()
    {
        DpmComparisonResult result = DpmComparison.Compare(Build(50, 1.0, 0.002, 1), Build(500, 1.0, 0.001, 2));

        result.Verdict.Should().Be(DpmComparisonVerdict.Match);
        result.Spacing.Should().Be(500);
        result.SpacingDiffers.Should().BeTrue();
    }

    [Test]
    public void InterpolatedBinsAreExcluded()
    {
        DataPositionMeasurement first = Build(500, 1.0, 0.001, 1);

        DataPositionMeasurement second =
            Build(500, 1.0, 0.001, 2, static lba => lba is >= 200000 and < 205000 ? 1.2 : 1.0);

        // The second DPM interpolated the bins where it differs
        for(int i = 0; i < second.Entries.Length; i++)
            if(second.Entries[i].Lba is > 200000 and <= 205000)
                second.Entries[i].Status = DpmEntryStatus.Interpolated;

        DpmComparisonResult result = DpmComparison.Compare(first, second);

        result.Verdict.Should().Be(DpmComparisonVerdict.Match);
        result.ExcludedBins.Should().Be(10);
    }

    [Test]
    public void LayerBoundaries()
    {
        DataPositionMeasurement first  = Build(500, 1.0, 0.001, 1);
        DataPositionMeasurement second = Build(500, 1.002, 0.001, 2);
        first.LayerEnds = [149999];

        DpmComparisonResult result = DpmComparison.Compare(first, second);

        result.LayersDiffer.Should().BeTrue();
        result.Scales.Should().HaveCount(2);
        result.Verdict.Should().Be(DpmComparisonVerdict.Match);

        foreach(DpmComparisonBin bin in result.Grid)
            (bin.FirstLba <= 149999 && bin.EndLba > 150000).Should().BeFalse();
    }

    [Test]
    public void NoOverlap()
    {
        DataPositionMeasurement second = Build(500, 1.0, 0.001, 2);

        for(int i = 0; i < second.Entries.Length; i++) second.Entries[i].Lba += SECTORS + 1000;

        DpmComparison.Compare(Build(500, 1.0, 0.001, 1), second)
                     .Verdict.Should()
                     .Be(DpmComparisonVerdict.NotComparable);
    }

    [Test]
    public void DifferentTimingUnit()
    {
        DataPositionMeasurement second = Build(500, 1.0, 0.001, 2);
        second.TimingUnit = 16;

        DpmComparisonResult result = DpmComparison.Compare(Build(500, 1.0, 0.001, 1), second);

        result.TimingUnitDiffers.Should().BeTrue();
        result.Verdict.Should().Be(DpmComparisonVerdict.NotComparable);
    }
}