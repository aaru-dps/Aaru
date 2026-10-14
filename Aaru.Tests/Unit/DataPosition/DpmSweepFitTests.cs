// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DpmSweepFitTests.cs
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
using System.Globalization;
using System.Linq;
using Aaru.Core.Devices.Dpm;
using FluentAssertions;
using NUnit.Framework;

namespace Aaru.Tests.Unit.DataPosition;

/// <summary>Calibration sweep fits of samples logged from real drives</summary>
[TestFixture]
[Category("Unit")]
public class DpmSweepFitTests
{
    static double[] Steps(string samples)
    {
        double[] e = samples.Split(' ').Select(static v => double.Parse(v, CultureInfo.InvariantCulture)).ToArray();

        return e.Skip(1).Select((v, i) => v - e[i]).ToArray();
    }

    // CD at sector 0 on an ASUS BW-16D1HT at 16x, where most samples come one or more turns late, so the falling steps
    // are mostly two turns or more
    [TestCase("49.672 84.642 102.946 71.817 73.724 42.576 44.395 46.262 48.110 50.041 51.913 119.961 55.658 140.247 " +
              "42.878 44.739 46.636 31.957 33.838 35.728 37.601 39.486 41.313 43.236")]
    [TestCase("49.654 84.581 53.425 88.404 73.747 42.503 44.406 46.300 48.155 83.093 51.885 53.793 55.844 74.103 " +
              "42.918 94.489 46.652 31.989 33.865 35.727 37.600 39.468 41.368 43.235")]
    public void LateAnswersFitOneTurn(string samples)
    {
        double[] steps = Steps(samples);

        DpmMeasurement.FitSweep(steps, steps.Length, out double s, out double t, out _).Should().BeTrue();

        // 8.81 sectors per turn, as the sweeps at 8x and at the maximum that calibrated measured
        t.Should().BeApproximately(16.54, 0.05, "the period is one turn, not two");
        s.Should().BeApproximately(1.878, 0.005);
    }

    // Same drive at its maximum speed, with retries of hundreds of ms to seconds: nothing to calibrate from
    [Test]
    public void RetriesAreRejected()
    {
        double[] steps = Steps("381.462 72.879 62.333 1088.835 100.818 149.835 31.992 1678.239 34.682 36.027 323.372 " +
                               "38.766 397.672 101.094 31.054 437.478 1094.389 22.999 24.348 25.690 27.093 28.420 " +
                               "29.793 31.135");

        DpmMeasurement.FitSweep(steps, steps.Length, out _, out _, out _).Should().BeFalse();
    }
}