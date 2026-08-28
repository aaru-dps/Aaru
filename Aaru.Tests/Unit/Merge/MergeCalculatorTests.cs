// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MergeCalculatorTests.cs
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

using System.Collections.Generic;
using System.Linq;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.Core.Image;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Merge;

[TestFixture]
[Category("Unit")]
public class MergeCalculatorTests
{
    static List<DumpHardware> Tries(params (ulong start, ulong end)[] extents) =>
    [
        new()
        {
            Extents = extents.Select(static e => new Extent
                               {
                                   Start = e.start,
                                   End   = e.end
                               })
                             .ToList()
        }
    ];

    /// <summary>
    ///     A bad block that falls inside a successful dump try extent can never be surfaced by the extents pass, so
    ///     dropping it silently leaves the merged image with sectors the secondary could have repaired.
    /// </summary>
    [Test]
    public void BadBlocksInsideASuccessfulTryExtentAreStillMerged()
    {
        List<ulong> result = Merger.CalculateSectorsToCopy(1000,
                                                           1000,
                                                           Tries((0, 999)),
                                                           Tries((0, 499)),
                                                           [10, 20, 700],
                                                           null,
                                                           []);

        // 10 and 20 sit inside the secondary's extent, 700 outside it. Extent coverage must not matter.
        result.Should().BeEquivalentTo(new ulong[] { 10, 20, 700 });
    }

    /// <summary>A bad block the secondary also failed to read is worthless, so it must not be merged.</summary>
    [Test]
    public void BadBlocksAlsoBadInTheSecondaryAreNotMerged()
    {
        List<ulong> result = Merger.CalculateSectorsToCopy(1000,
                                                           1000,
                                                           Tries((0, 999)),
                                                           Tries((0, 999)),
                                                           [10, 20, 30],
                                                           [20],
                                                           []);

        result.Should().BeEquivalentTo(new ulong[] { 10, 30 });
    }

    /// <summary>Regression for the iTunes 4.2 merge: 40,725 bad blocks, of which 5,726 sat inside a try extent.</summary>
    [Test]
    public void ExtentGapsAndInExtentBadBlocksAreBothMerged()
    {
        // A primary that read 0-99 and 200-299, and recorded bad blocks on both sides of that boundary.
        List<DumpHardware> primaryTries = Tries((0, 99), (200, 299));

        List<ulong> badBlocks = [50, 150, 250];

        List<ulong> result = Merger.CalculateSectorsToCopy(300,
                                                           300,
                                                           primaryTries,
                                                           Tries((0, 299)),
                                                           badBlocks,
                                                           null,
                                                           []);

        // 100-199 is the gap the extents pass finds; 50 and 250 are only reachable through the bad block pass.
        result.Should().HaveCount(102);
        result.Should().Contain(badBlocks);
        result.Should().Contain(Enumerable.Range(100, 100).Select(static i => (ulong)i));
    }

    /// <summary>Sectors past the end of either image can neither be read nor stored, so they are dropped.</summary>
    [Test]
    public void BadBlocksBeyondEitherImageAreDropped()
    {
        List<ulong> result = Merger.CalculateSectorsToCopy(1000,
                                                           500,
                                                           Tries((0, 999)),
                                                           Tries((0, 999)),
                                                           [10, 600, 1500],
                                                           null,
                                                           []);

        result.Should().BeEquivalentTo(new ulong[] { 10 });
    }
}
