// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : LayerBreakTests.cs    
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


using Aaru.Core.Graphics;
using Aaru.Decoders.DVD;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Media;

[TestFixture]
public class LayerBreakTests
{
    const uint DATA_START = 0x30000;

    static PFI.PhysicalFormatInformation Pfi(bool otp, uint dataEnd, uint layer0End) => new()
    {
        Layers           = 1,
        TrackPath        = otp,
        DataAreaStartPSN = DATA_START,
        DataAreaEndPSN   = dataEnd,
        Layer0EndPSN     = layer0End
    };

    [Test]
    public void OtpUsesLayer0End()
    {
        (ulong? layerBreak, bool otp) = Spiral.LayerBreakFromPfi(Pfi(true, 0xF8FFFE, 0x1F0000), 0x340001);

        layerBreak.Should().Be(0x1C0000);
        otp.Should().BeTrue();
    }

    [Test]
    public void OtpWithoutLayer0EndDerivesItFromCapacity()
    {
        // Layer 0 is 0x30000-0x1F0000, layer 1 runs from 0xE0FFFF, the complement of 0x1F0000, to 0xF8FFFE
        (ulong? layerBreak, bool otp) = Spiral.LayerBreakFromPfi(Pfi(true, 0xF8FFFE, 0), 0x340001);

        layerBreak.Should().Be(0x1C0000);
        otp.Should().BeTrue();
    }

    [Test]
    public void OtpWithoutLayer0EndNorCapacityHasNoBreak() =>
        Spiral.LayerBreakFromPfi(Pfi(true, 0xF8FFFE, 0), 0).layerBreak.Should().BeNull();

    [Test]
    public void OtpWithImpossibleDerivedBreakHasNoBreak() =>

        // A data area end that is not a layer 1 address puts the derived break past the end of the medium
        Spiral.LayerBreakFromPfi(Pfi(true, 0x14E32F, 0), 2939493).layerBreak.Should().BeNull();

    [Test]
    public void PtpUsesDataAreaEnd()
    {
        (ulong? layerBreak, bool otp) = Spiral.LayerBreakFromPfi(Pfi(false, 0x14E32F, 0), 2939493);

        layerBreak.Should().Be(1172271);
        otp.Should().BeFalse();
    }

    [Test]
    public void BreakPastTheMediumIsDiscarded() =>
        Spiral.LayerBreakFromPfi(Pfi(false, 0x14E32F, 0), 1000).layerBreak.Should().BeNull();
}