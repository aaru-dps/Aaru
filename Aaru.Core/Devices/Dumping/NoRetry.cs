// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : NoRetry.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Handles the sectors that must not be trimmed nor retried.
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
using System.Globalization;
using System.Linq;

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    /// <summary>
    ///     Parses a list of sector ranges, separated by commas, each one either a single sector or two sectors separated
    ///     by a dash, both included, as in <c>800-10000,15000</c>
    /// </summary>
    /// <param name="text">Text to parse, an empty or <c>null</c> text gives an empty list</param>
    /// <param name="ranges">Parsed ranges, <c>null</c> if the text is not valid</param>
    /// <returns><c>true</c> if the text is valid</returns>
    public static bool TryParseSectorRanges(string text, out List<(ulong Start, ulong End)> ranges)
    {
        ranges = [];

        if(string.IsNullOrWhiteSpace(text)) return true;

        foreach(string item in text.Split(','))
        {
            string[] limits = item.Split('-');

            if(limits.Length > 2 ||
               !ulong.TryParse(limits[0].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out ulong start))
            {
                ranges = null;

                return false;
            }

            ulong end = start;

            if(limits.Length == 2 &&
               (!ulong.TryParse(limits[1].Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out end) ||
                end < start))
            {
                ranges = null;

                return false;
            }

            ranges.Add((start, end));
        }

        return true;
    }

    /// <summary>Formats a list of sector ranges the same way <see cref="TryParseSectorRanges" /> parses them</summary>
    /// <param name="ranges">Sector ranges</param>
    /// <returns>Formatted ranges</returns>
    public static string FormatSectorRanges(IEnumerable<(ulong Start, ulong End)> ranges)
    {
        return string.Join(", ", ranges.Select(r => r.Start == r.End ? $"{r.Start}" : $"{r.Start}-{r.End}"));
    }

    /// <summary>Checks if a sector must not be trimmed nor retried</summary>
    /// <param name="sector">Sector</param>
    /// <returns><c>true</c> if the sector must be left as it is</returns>
    bool NoRetry(ulong sector)
    {
        return _noRetrySectors.Any(r => sector >= r.Start && sector <= r.End);
    }
}