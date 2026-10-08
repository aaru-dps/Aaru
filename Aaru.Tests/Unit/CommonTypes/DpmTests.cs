// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DpmTests.cs
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

using Aaru.CommonTypes.Structs;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.CommonTypes;

[TestFixture]
[Category("Unit")]
public class DpmTests
{
    /// <summary>Builds a DPM with entries at the given sectors and a constant density in units per sector</summary>
    static DataPositionMeasurement Build(ulong[] lbas, ulong unitsPerSector, ulong[] layerEnds = null)
    {
        var entries = new DpmEntry[lbas.Length];

        for(int i = 0; i < lbas.Length; i++)
        {
            entries[i] = new DpmEntry
            {
                Lba    = lbas[i],
                Angle  = (lbas[i] - lbas[0]) * unitsPerSector,
                Status = i == 0 ? DpmEntryStatus.Unknown : DpmEntryStatus.Measured
            };
        }

        return new DataPositionMeasurement
        {
            NominalSpacing = 50,
            LayerEnds      = layerEnds ?? [],
            Entries        = entries,
            Calibrations   = []
        };
    }

    [Test]
    public void ValidAcceptsMonotonicEntries() => Dpm.Validate(Build([0, 50, 100, 150], 2560)).Should().BeTrue();

    [Test]
    public void ValidateRejectsInconsistentDpm()
    {
        DataPositionMeasurement dpm = Build([0, 50, 100], 2560);
        dpm.Entries[2].Lba = 50;
        Dpm.Validate(dpm).Should().BeFalse("addresses must strictly increase");

        dpm                  = Build([0, 50, 100], 2560);
        dpm.Entries[2].Angle = 1;
        Dpm.Validate(dpm).Should().BeFalse("angles must not decrease");

        dpm                  = Build([0, 50, 100], 2560);
        dpm.Entries[0].Angle = 1;
        Dpm.Validate(dpm).Should().BeFalse("the first angle must be 0");

        dpm                = Build([0, 50, 100], 2560);
        dpm.NominalSpacing = 0;
        Dpm.Validate(dpm).Should().BeFalse("the spacing must not be 0");

        Dpm.Validate(Build([0], 2560)).Should().BeFalse("at least two entries are needed");

        Dpm.Validate(Build([0, 50, 100], 2560, [80, 60])).Should().BeFalse("layer ends must increase");
    }

    [Test]
    public void LayerOfHandlesAnyNumberOfLayers()
    {
        Dpm.LayerOf(Build([0, 100], 1), 99).Should().Be(0);

        DataPositionMeasurement dual = Build([0, 100], 1, [49]);
        Dpm.LayerOf(dual, 49).Should().Be(0);
        Dpm.LayerOf(dual, 50).Should().Be(1);

        DataPositionMeasurement triple = Build([0, 300], 1, [99, 199]);
        Dpm.LayerOf(triple, 150).Should().Be(1);
        Dpm.LayerOf(triple, 200).Should().Be(2);

        DataPositionMeasurement quad = Build([0, 400], 1, [99, 199, 299]);
        Dpm.LayerOf(quad, 0).Should().Be(0);
        Dpm.LayerOf(quad, 299).Should().Be(2);
        Dpm.LayerOf(quad, 300).Should().Be(3);
        Dpm.LayerOf(quad, 399).Should().Be(3);
    }

    [Test]
    public void AngleAtInterpolatesInsideBins()
    {
        DataPositionMeasurement dpm = Build([10, 60, 70, 170], 3);

        Dpm.AngleAt(dpm, 10).Should().Be(0);
        Dpm.AngleAt(dpm, 60).Should().Be(150);
        Dpm.AngleAt(dpm, 35).Should().Be(75);
        Dpm.AngleAt(dpm, 170).Should().Be(480);
        Dpm.AngleAt(dpm, 9).Should().BeNull();
        Dpm.AngleAt(dpm, 171).Should().BeNull();

        // Rounds to the nearest unit
        dpm.Entries[1].Angle = 151;
        dpm.Entries[2].Angle = 181;
        dpm.Entries[3].Angle = 481;
        Dpm.AngleAt(dpm, 35).Should().Be(76);
    }

    [Test]
    public void DensityAtIsDegreesPerSector()
    {
        // 2560 units per sector is 1/1000 turn per sector
        Dpm.DensityAt(Build([0, 50, 100], 2560), 75).Should().BeApproximately(0.36, 1e-12);
        Dpm.DensityAt(Build([0, 50, 100], 2560), 101).Should().BeNull();
    }

    [Test]
    public void ToGridResamplesUnevenEntries()
    {
        // Entries around a layer break, and a short last bin, as measured
        DataPositionMeasurement dpm = Build([0, 50, 99, 100, 150, 175], 7);

        ulong[] grid = Dpm.ToGrid(dpm, 50);

        grid.Should().Equal(350, 700, 1050);
    }

    [Test]
    public void ToGridOfUniformEntriesIsIdentity()
    {
        DataPositionMeasurement dpm = Build([5, 55, 105, 155], 2560);

        Dpm.ToGrid(dpm, 50).Should().Equal(dpm.Entries[1].Angle, dpm.Entries[2].Angle, dpm.Entries[3].Angle);
    }
}