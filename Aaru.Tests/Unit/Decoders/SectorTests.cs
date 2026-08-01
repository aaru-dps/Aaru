// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SectorTests.cs
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
using Aaru.Decoders.CD;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Decoders;

[TestFixture]
[Category("Unit")]
public class SectorTests
{
    static byte[] MakeSectorWithSync(uint seed)
    {
        var buffer = new byte[2352];
        Sector.SyncMark.CopyTo(buffer, 0);

        uint state = seed;

        for(var i = 12; i < buffer.Length; i++)
        {
            state     = state * 1664525 + 1013904223;
            buffer[i] = (byte)(state >> 24);
        }

        return buffer;
    }

    [Test]
    public void ScrambleTwiceIsIdentity()
    {
        byte[] sector    = MakeSectorWithSync(0xDEADBEEF);
        byte[] scrambled = Sector.Scramble(sector);

        scrambled.Should().NotEqual(sector, "scrambling must change the payload");
        Sector.Scramble(scrambled).Should().Equal(sector);
    }

    [Test]
    public void ScrambleLeavesSyncMarkIntact()
    {
        byte[] scrambled = Sector.Scramble(MakeSectorWithSync(1));

        scrambled[..12].Should().Equal(Sector.SyncMark);
    }

    [Test]
    public void ScrambleWithoutSyncMarkIsNoOp()
    {
        var noSync = new byte[2352];
        noSync[0] = 0x55;

        Sector.Scramble(noSync).Should().BeSameAs(noSync);
    }

    [Test]
    public void GetUserDataExtractsMode1Payload()
    {
        byte[] sector = MakeSectorWithSync(2);
        sector[15] = 1; // mode 1

        byte[] user = Sector.GetUserData(sector);

        user.Length.Should().Be(2048);

        var expected = new byte[2048];
        Array.Copy(sector, 16, expected, 0, 2048);
        user.Should().Equal(expected);
    }

    [Test]
    public void GetUserDataMode0IsZeroFilled()
    {
        byte[] sector = MakeSectorWithSync(3);
        sector[15] = 0; // mode 0

        Sector.GetUserData(sector).Should().OnlyContain(static b => b == 0);
    }

    [Test]
    public void GetUserDataWithoutSyncReturnsInput()
    {
        var noSync = new byte[2352];

        Sector.GetUserData(noSync).Should().BeSameAs(noSync);
    }
}
