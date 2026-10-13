// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : RetryProgress.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Computes the progress of the error retry passes.
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

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    /// <summary>
    ///     Computes the current progress of the retry passes. The maximum is the worst case, every block that was bad
    ///     when retrying started being retried in every pass. Every block still bad can be retried once in each of the
    ///     passes left, so recovering a block advances the progress by the passes it will not need.
    /// </summary>
    /// <param name="maximum">Blocks bad when retrying started multiplied by number of passes</param>
    /// <param name="passes">Number of retry passes</param>
    /// <param name="pass">Current pass, starting at 1</param>
    /// <param name="index">Index of the block being retried in the current pass, starting at 0</param>
    /// <param name="passLength">Number of blocks to retry in the current pass</param>
    /// <param name="stillBad">Number of blocks still bad</param>
    /// <returns>Current progress, between 0 and <paramref name="maximum" /></returns>
    static long RetryProgressCurrent(long maximum, ushort passes, int pass, int index, int passLength, int stillBad)
    {
        long remaining = passLength - index + (long)(passes - pass) * stillBad;

        return Math.Clamp(maximum - remaining, 0, maximum);
    }
}