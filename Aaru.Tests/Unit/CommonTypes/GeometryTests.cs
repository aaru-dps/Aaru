// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : GeometryTests.cs
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

using Aaru.CommonTypes;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.CommonTypes;

[TestFixture]
[Category("Unit")]
public class GeometryTests
{
    [Test]
    public void KnownGeometriesMapToExpectedMediaTypes()
    {
        Geometry.GetMediaType((80, 2, 18, 512, MediaEncoding.MFM, false)).Should().Be(MediaType.DOS_35_HD);
        Geometry.GetMediaType((80, 2, 9, 512, MediaEncoding.MFM, false)).Should().Be(MediaType.DOS_35_DS_DD_9);
        Geometry.GetMediaType((80, 2, 15, 512, MediaEncoding.MFM, false)).Should().Be(MediaType.DOS_525_HD);
        Geometry.GetMediaType((80, 2, 36, 512, MediaEncoding.MFM, false)).Should().Be(MediaType.DOS_35_ED);
        Geometry.GetMediaType((80, 2, 11, 512, MediaEncoding.MFM, false)).Should().Be(MediaType.CBM_AMIGA_35_DD);
        Geometry.GetMediaType((35, 1, 13, 256, MediaEncoding.AppleGCR, false)).Should().Be(MediaType.Apple32SS);
        Geometry.GetMediaType((80, 2, 10, 512, MediaEncoding.AppleGCR, true)).Should().Be(MediaType.AppleSonyDS);
    }

    [Test]
    public void EncodingAndVariableSectorsAreDiscriminating()
    {
        // Same C/H/S/bps as AppleSonyDS but MFM and fixed sectors is a different media type
        Geometry.GetMediaType((80, 2, 10, 512, MediaEncoding.MFM, false)).Should().Be(MediaType.CBM_35_DD);

        Geometry.GetMediaType((80, 2, 10, 512, MediaEncoding.MFM, true)).Should().Be(MediaType.Unknown);
    }

    [Test]
    public void UnknownGeometryReturnsUnknownMediaType() =>
        Geometry.GetMediaType((81, 3, 7, 512, MediaEncoding.MFM, false)).Should().Be(MediaType.Unknown);

    [Test]
    public void GetGeometryRoundTripsThroughGetMediaType()
    {
        foreach(var geom in Geometry.KnownGeometries)
        {
            var found = Geometry.GetGeometry(geom.type);

            Geometry.GetMediaType((found.cylinders, found.heads, found.sectorsPerTrack, found.bytesPerSector,
                                   found.encoding, found.variableSectorsPerTrack))
                    .Should()
                    .Be(geom.type, "geometry table entries must round-trip for {0}", geom.type);
        }
    }
}