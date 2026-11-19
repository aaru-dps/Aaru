// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : JaguarOffsetTests.cs
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
using System.Text;
using Aaru.Core.Media.Detection;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Media;

/// <summary>
///     Covers the Atari Jaguar CD combined offset search. The header of the first track of the second session belongs
///     at byte 2 of index 1, and the offset is how far after that it is found, following the convention of the data
///     track offset search: positive when the data comes later than it should.
/// </summary>
[TestFixture]
[Category("Unit")]
public class JaguarOffsetTests
{
    /// <summary>Sectors read before and after index 1, as <c>CompactDisc.GetOffset</c> does.</summary>
    const int SECTORS_AROUND_INDEX1 = 4;

    /// <summary>Size of the buffer handed to the search.</summary>
    const int BUFFER_SIZE = (SECTORS_AROUND_INDEX1 * 2 + 1) * 2352;

    /// <summary>Position of index 1 inside the buffer.</summary>
    const int INDEX1_POSITION = SECTORS_AROUND_INDEX1 * 2352;

    /// <summary>Where the header belongs, relative to index 1.</summary>
    const int HEADER_START = 2;

    /// <summary>Filler that cannot be part of the header, so the only match is the one we plant.</summary>
    const byte FILLER = 0x55;

    /// <summary>Sync run and header text, stored on disc with every 16-bit word byte swapped.</summary>
    static readonly byte[] _header =
        Encoding.ASCII.GetBytes("TAIRTAIRTAIRTAIRTAIRTAIRTAIRTAIRTAIRTAIRTAIRTAIRTAIRTAIRTAIRTAIR" +
                                "TARA IPARPVODED TA AEHDAREA RT I");

    static byte[] BufferWith(int headerPosition)
    {
        byte[] buffer = new byte[BUFFER_SIZE];
        buffer.AsSpan().Fill(FILLER);
        Array.Copy(_header, 0, buffer, headerPosition, _header.Length);

        return buffer;
    }

    [Test]
    public void OffsetIsNullWithoutData() => MMC.GetJaguarOffset(null, INDEX1_POSITION).Should().BeNull();

    [Test]
    public void OffsetIsNullWhenTheHeaderIsAbsent()
    {
        byte[] buffer = new byte[BUFFER_SIZE];
        buffer.AsSpan().Fill(FILLER);

        MMC.GetJaguarOffset(buffer, INDEX1_POSITION).Should().BeNull();
    }

    /// <summary>Unlike VideoNow, an unfilled buffer reports "not found" instead of a zero offset.</summary>
    [Test]
    public void AnUnfilledBufferYieldsNull() =>
        MMC.GetJaguarOffset(new byte[BUFFER_SIZE], INDEX1_POSITION).Should().BeNull();

    [Test]
    public void OffsetIsZeroWhenTheHeaderIsWhereItShouldBe() =>
        MMC.GetJaguarOffset(BufferWith(INDEX1_POSITION + HEADER_START), INDEX1_POSITION).Should().Be(0);

    /// <summary>Myst (J9069E) has its boot header 1986 bytes after index 1.</summary>
    [Test]
    public void OffsetIsPositiveWhenTheHeaderIsLate() =>
        MMC.GetJaguarOffset(BufferWith(INDEX1_POSITION + 1986), INDEX1_POSITION).Should().Be(1984);

    /// <summary>
    ///     Blue Lightning (J9016E) and VidGrid (J9075E) have their boot header in the pregap, about 1500 bytes before
    ///     index 1.
    /// </summary>
    [Test]
    public void OffsetIsNegativeWhenTheHeaderIsInThePregap() =>
        MMC.GetJaguarOffset(BufferWith(INDEX1_POSITION - 1486), INDEX1_POSITION).Should().Be(-1488);

    /// <summary>Offsets are whole samples, so the planted header positions used above give multiples of 4.</summary>
    [Test]
    public void RealWorldHeaderPositionsGiveWholeSamples()
    {
        foreach(int relative in new[] { 1986, -1486, -1570 })
        {
            int? offset = MMC.GetJaguarOffset(BufferWith(INDEX1_POSITION + relative), INDEX1_POSITION);

            offset.Should().NotBeNull();
            (offset.Value % 4).Should().Be(0);
        }
    }
}