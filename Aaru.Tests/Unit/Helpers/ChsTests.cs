// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ChsTests.cs
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

using Aaru.Helpers;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Helpers;

[TestFixture]
[Category("Unit")]
public class ChsTests
{
    [TestCase(0u,  0u, 1u,  16u, 63u, ExpectedResult = 0u)]
    [TestCase(0u,  0u, 2u,  16u, 63u, ExpectedResult = 1u)]
    [TestCase(0u,  1u, 1u,  2u,  18u, ExpectedResult = 18u)]
    [TestCase(1u,  0u, 1u,  2u,  18u, ExpectedResult = 36u)]
    [TestCase(1u,  1u, 1u,  2u,  18u, ExpectedResult = 54u)]
    [TestCase(79u, 1u, 18u, 2u,  18u, ExpectedResult = 2879u)]
    public uint ToLbaKnownValues(uint cyl, uint head, uint sector, uint maxHead, uint maxSector) =>
        CHS.ToLBA(cyl, head, sector, maxHead, maxSector);

    [Test]
    public void ToLbaFallsBackTo16HeadsAnd63SectorsWhenGeometryIsZero()
    {
        CHS.ToLBA(0, 0, 1, 0, 0).Should().Be(0);
        CHS.ToLBA(1, 2, 3, 0, 0).Should().Be((1u  * 16 + 2) * 63 + 2);
        CHS.ToLBA(1, 2, 3, 0, 63).Should().Be((1u * 16 + 2) * 63 + 2);
    }

    [Test]
    public void ToLbaSectorZeroDoesNotUnderflow()
    {
        // CHS sector numbers are 1-based; a corrupt sector 0 is treated as sector 1
        CHS.ToLBA(0, 0, 0, 16, 63).Should().Be(0);
        CHS.ToLBA(1, 1, 0, 2,  18).Should().Be(54);
    }
}