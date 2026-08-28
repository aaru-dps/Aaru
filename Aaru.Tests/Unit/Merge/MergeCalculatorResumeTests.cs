// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MergeCalculatorResumeTests.cs
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
using System.IO;
using System.Linq;
using System.Text.Json;
using Aaru.CommonTypes.AaruMetadata;
using Aaru.CommonTypes.Metadata;
using Aaru.Core.Image;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Merge;

/// <summary>
///     Drives the merge calculator with a real resume file, from the dump of "iTunes 4.2 CD de instalación
///     (L302119A-ES) (693-5295-3)". That merge reported only 34,999 of the 40,725 recorded bad blocks: the 5,726 bad
///     blocks that happen to fall inside one of the drive's successful dump try extents were being dropped, because the
///     bad block pass required the sector to be covered by a secondary try extent.
/// </summary>
[TestFixture]
[Category("Unit")]
public class MergeCalculatorResumeTests
{
    /// <summary>Sector count of the medium the resume file below was recorded from.</summary>
    const ulong SECTORS = 185323;

    /// <summary>Bad blocks recorded by the primary dump.</summary>
    const int TOTAL_BAD_BLOCKS = 40725;

    /// <summary>Of those, the ones lying in a gap between successful try extents.</summary>
    const int BAD_BLOCKS_OUTSIDE_EXTENTS = 34999;

    /// <summary>Of those, the ones lying inside a successful try extent. These are what used to be lost.</summary>
    const int BAD_BLOCKS_INSIDE_EXTENTS = 5726;

    Resume _resume;

    [OneTimeSetUp]
    public void LoadResume()
    {
        string path = Path.Combine(AppContext.BaseDirectory, "Unit", "Merge", "Data", "itunes42.resume.json");

        using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);

        _resume = (JsonSerializer.Deserialize(fs, typeof(ResumeJson), ResumeJsonContext.Default) as ResumeJson)?.Resume;

        _resume.Should().NotBeNull();
    }

    /// <summary>When no secondary resume is given the whole secondary image counts as covered.</summary>
    static List<DumpHardware> WholeImageTries() =>
    [
        new()
        {
            Extents =
            [
                new Extent
                {
                    Start = 0,
                    End   = SECTORS - 1
                }
            ]
        }
    ];

    [Test]
    public void ResumeFileHoldsTheExpectedBadBlocks()
    {
        _resume.BadBlocks.Should().HaveCount(TOTAL_BAD_BLOCKS);
        _resume.BadBlocks.Should().OnlyHaveUniqueItems();

        _resume.Tries.Should()
               .ContainSingle()
               .Which.Extents.Select(static e => (e.Start, e.End))
               .Should()
               .Equal((0UL, 85022UL),
                      (85024UL, 85222UL),
                      (85917UL, 94173UL),
                      (122846UL, 122868UL),
                      (128501UL, 185333UL));
    }

    /// <summary>A secondary covering the whole medium and reporting nothing bad must offer every recorded bad block.</summary>
    [Test]
    public void EveryRecordedBadBlockIsMerged()
    {
        List<ulong> result = Merger.CalculateSectorsToCopy(SECTORS,
                                                           SECTORS,
                                                           _resume.Tries,
                                                           WholeImageTries(),
                                                           _resume.BadBlocks,
                                                           null,
                                                           []);

        result.Should().HaveCount(TOTAL_BAD_BLOCKS);

        // Set comparison rather than BeEquivalentTo: forty thousand items is too many for the latter.
        new HashSet<ulong>(result).SetEquals(_resume.BadBlocks).Should().BeTrue();
    }

    /// <summary>
    ///     The regression, on real data. A secondary that only got as far as sector 122,845 still holds good data for
    ///     every bad block below that point, including the ones sitting inside one of the primary's successful try
    ///     extents. Requiring secondary try coverage used to discard all of them.
    /// </summary>
    [Test]
    public void BadBlocksAreMergedEvenWhereTheSecondaryReportsNoTryCoverage()
    {
        const ulong secondaryReach = 122846;

        List<DumpHardware> secondaryTries =
        [
            new()
            {
                Extents =
                [
                    new Extent
                    {
                        Start = 0,
                        End   = secondaryReach - 1
                    }
                ]
            }
        ];

        List<ulong> result = Merger.CalculateSectorsToCopy(SECTORS,
                                                           SECTORS,
                                                           _resume.Tries,
                                                           secondaryTries,
                                                           _resume.BadBlocks,
                                                           null,
                                                           []);

        // Every bad block is still offered: the ones past the secondary's tries come from the bad block pass, the
        // ones below it from either pass.
        result.Should().HaveCount(TOTAL_BAD_BLOCKS);
        new HashSet<ulong>(result).SetEquals(_resume.BadBlocks).Should().BeTrue();
    }

    /// <summary>
    ///     Characterises what the merge that prompted all this actually did. With no primary resume the bad block list
    ///     is unknown, so only the gaps between try extents are merged and the 5,726 bad blocks inside them are lost
    ///     with no warning. This is the 34,999 that was reported instead of 40,725.
    /// </summary>
    [Test]
    public void WithoutAPrimaryResumeOnlyTheGapsAreMerged()
    {
        List<ulong> result = Merger.CalculateSectorsToCopy(SECTORS,
                                                           SECTORS,
                                                           _resume.Tries,
                                                           WholeImageTries(),
                                                           null,
                                                           null,
                                                           []);

        result.Should().HaveCount(BAD_BLOCKS_OUTSIDE_EXTENTS);
        result.Should().NotContain(_resume.BadBlocks.Where(b => b is >= 85917 and <= 94173));
    }

    /// <summary>
    ///     Pins the split the bug hinged on: the extents pass alone can only ever find the bad blocks sitting in the gaps
    ///     between successful try extents, which is exactly the short count the broken merge reported.
    /// </summary>
    [Test]
    public void TheExtentsPassAloneFindsOnlyTheGaps()
    {
        var covered = new HashSet<ulong>(_resume.Tries.SelectMany(static h => h.Extents)
                                                .SelectMany(static e =>
                                                                Enumerable.Range(0, (int)(e.End - e.Start + 1))
                                                                          .Select(i => e.Start + (ulong)i)));

        int inside  = _resume.BadBlocks.Count(covered.Contains);
        int outside = _resume.BadBlocks.Count - inside;

        inside.Should().Be(BAD_BLOCKS_INSIDE_EXTENTS);
        outside.Should().Be(BAD_BLOCKS_OUTSIDE_EXTENTS);

        // Without any bad block list, only the gaps are found — the count the broken merge printed.
        List<ulong> extentsOnly = Merger.CalculateSectorsToCopy(SECTORS,
                                                                SECTORS,
                                                                _resume.Tries,
                                                                WholeImageTries(),
                                                                null,
                                                                null,
                                                                []);

        extentsOnly.Should().HaveCount(BAD_BLOCKS_OUTSIDE_EXTENTS);
    }

    /// <summary>A secondary that failed on the very same sectors has nothing to contribute.</summary>
    [Test]
    public void ASecondaryWithTheSameBadBlocksContributesNothing()
    {
        List<ulong> result = Merger.CalculateSectorsToCopy(SECTORS,
                                                           SECTORS,
                                                           _resume.Tries,
                                                           _resume.Tries,
                                                           _resume.BadBlocks,
                                                           _resume.BadBlocks,
                                                           []);

        result.Should().BeEmpty();
    }
}
