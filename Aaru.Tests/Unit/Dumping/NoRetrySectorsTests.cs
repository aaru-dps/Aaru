// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : NoRetrySectorsTests.cs
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
using Aaru.Core.Devices.Dumping;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Dumping;

[TestFixture]
public class NoRetrySectorsTests
{
    [Test]
    public void EmptyGivesNoRanges()
    {
        Dump.TryParseSectorRanges(null, out List<(ulong Start, ulong End)> ranges).Should().BeTrue();
        ranges.Should().BeEmpty();

        Dump.TryParseSectorRanges("  ", out ranges).Should().BeTrue();
        ranges.Should().BeEmpty();
    }

    [Test]
    public void SingleSector()
    {
        Dump.TryParseSectorRanges("15000", out List<(ulong Start, ulong End)> ranges).Should().BeTrue();
        ranges.Should().Equal((15000UL, 15000UL));
    }

    [Test]
    public void ListOfRangesAndSectors()
    {
        Dump.TryParseSectorRanges(" 800 - 10000, 15000,20000-20001 ", out List<(ulong Start, ulong End)> ranges)
            .Should()
            .BeTrue();

        ranges.Should().Equal((800UL, 10000UL), (15000UL, 15000UL), (20000UL, 20001UL));
        Dump.FormatSectorRanges(ranges).Should().Be("800-10000, 15000, 20000-20001");
    }

    [TestCase("10-5")]
    [TestCase("abc")]
    [TestCase("1,,2")]
    [TestCase("1-2-3")]
    [TestCase("-5")]
    [TestCase("5-")]
    [TestCase("+5")]
    public void InvalidIsRejected(string text)
    {
        Dump.TryParseSectorRanges(text, out List<(ulong Start, ulong End)> ranges).Should().BeFalse();
        ranges.Should().BeNull();
    }
}