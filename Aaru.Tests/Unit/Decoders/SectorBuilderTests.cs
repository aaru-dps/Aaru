// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SectorBuilderTests.cs
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

using Aaru.Checksums;
using Aaru.CommonTypes.Enums;
using Aaru.Decoders.CD;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Decoders;

/// <summary>
///     Builds sectors with SectorBuilder and validates them with CdChecksums — cross-checking the EDC/ECC
///     generator against the independent verifier without any image fixture.
/// </summary>
[TestFixture]
[Category("Unit")]
public class SectorBuilderTests
{
    static byte[] MakeUserData(int offset, int length, uint seed)
    {
        var buffer = new byte[2352];
        uint state = seed;

        for(int i = offset; i < offset + length; i++)
        {
            state     = state * 1664525 + 1013904223;
            buffer[i] = (byte)(state >> 24);
        }

        return buffer;
    }

    [Test]
    public void Mode1SectorValidatesAgainstCdChecksums()
    {
        var    builder = new SectorBuilder();
        byte[] sector  = MakeUserData(16, 2048, 0xCAFE);

        builder.ReconstructPrefix(ref sector, TrackType.CdMode1, 0);
        builder.ReconstructEcc(ref sector, TrackType.CdMode1);

        bool? valid = CdChecksums.CheckCdSector(sector, out bool? eccP, out bool? eccQ, out bool? edc);

        valid.Should().BeTrue();
        eccP.Should().BeTrue();
        eccQ.Should().BeTrue();
        edc.Should().BeTrue();
    }

    [Test]
    public void Mode1HeaderContainsBcdMsfAndMode()
    {
        var    builder = new SectorBuilder();
        byte[] sector  = MakeUserData(16, 2048, 1);

        // LBA 166 -> +150 = 316 frames = 00:04:16
        builder.ReconstructPrefix(ref sector, TrackType.CdMode1, 166);

        sector[0x00C].Should().Be(0x00);
        sector[0x00D].Should().Be(0x04);
        sector[0x00E].Should().Be(0x16);
        sector[0x00F].Should().Be(0x01);
    }

    [Test]
    public void Mode1CorruptedSectorFailsValidation()
    {
        var    builder = new SectorBuilder();
        byte[] sector  = MakeUserData(16, 2048, 0xCAFE);

        builder.ReconstructPrefix(ref sector, TrackType.CdMode1, 0);
        builder.ReconstructEcc(ref sector, TrackType.CdMode1);

        sector[100] ^= 0xFF;

        CdChecksums.CheckCdSector(sector).Should().NotBeTrue();
    }

    [Test]
    public void Mode2Form1SectorValidatesAgainstCdChecksums()
    {
        var    builder = new SectorBuilder();
        byte[] sector  = MakeUserData(0x18, 2048, 0xBEEF);

        // Subheader at 0x10, duplicated at 0x14: file 0, channel 0, submode data, coding 0
        sector[0x10] = sector[0x14] = 0x00;
        sector[0x11] = sector[0x15] = 0x00;
        sector[0x12] = sector[0x16] = 0x08;
        sector[0x13] = sector[0x17] = 0x00;

        builder.ReconstructPrefix(ref sector, TrackType.CdMode2Form1, 150);
        builder.ReconstructEcc(ref sector, TrackType.CdMode2Form1);

        CdChecksums.CheckCdSector(sector).Should().BeTrue();
    }

    [Test]
    public void Mode2Form2SectorValidatesAgainstCdChecksums()
    {
        var    builder = new SectorBuilder();
        byte[] sector  = MakeUserData(0x18, 2324, 0xF00D);

        // Submode with form 2 bit (0x20) set
        sector[0x10] = sector[0x14] = 0x00;
        sector[0x11] = sector[0x15] = 0x00;
        sector[0x12] = sector[0x16] = 0x28;
        sector[0x13] = sector[0x17] = 0x00;

        builder.ReconstructPrefix(ref sector, TrackType.CdMode2Form2, 150);
        builder.ReconstructEcc(ref sector, TrackType.CdMode2Form2);

        CdChecksums.CheckCdSector(sector).Should().BeTrue();
    }
}
