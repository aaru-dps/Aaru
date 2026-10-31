// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : IbgLogTests.cs
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

using System.Reflection;
using System.Threading;
using Aaru.Core.Logging;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.Dumping;

[TestFixture]
public class IbgLogTests
{
    [Test]
    public void MaximumSpeedIsAveragedOverTheSamplesOfARow()
    {
        // CD-ROM profile, where 1x is 150 KiB/s. Nothing is written to the file until the log is closed.
        IbgLog log = new("unused.ibg", 0x0008);

        for(ulong s = 0; s < 6; s++)
        {
            log.Write(s, 1500);
            Thread.Sleep(30);
        }

        FieldInfo maxSpeedField =
            typeof(IbgLog).GetField("_ibgMaxSpeed", BindingFlags.Instance | BindingFlags.NonPublic);

        ((double)maxSpeedField!.GetValue(log)!).Should().BeApproximately(10, 0.001);
    }
}