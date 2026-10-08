// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Csv.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Writes Data Position Measurement as per sector CSV.
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General public License for more details.
//
//     You should have received a copy of the GNU General public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System.Globalization;
using System.IO;
using Aaru.CommonTypes.Structs;

namespace Aaru.Core.Devices.Dpm;

/// <summary>Writes a Data Position Measurement as a per sector CSV</summary>
public static class DpmCsv
{
    /// <summary>
    ///     Writes the degrees per sector of every sector, in the format of the reference dpm tool, so measurements can be
    ///     compared directly. Each bin's mean density sits at its centre, linear between the centres of neighbouring bins
    ///     in the same layer.
    /// </summary>
    /// <param name="dpm">DPM</param>
    /// <param name="path">Path to the CSV file</param>
    public static void Write(DataPositionMeasurement dpm, string path)
    {
        DpmEntry[] entries = dpm.Entries;
        int        nb      = entries.Length - 1;
        var        rate    = new double[nb];

        for(int j = 0; j < nb; j++)
        {
            rate[j] = (entries[j + 1].Angle - entries[j].Angle) *
                      360.0                                       /
                      CommonTypes.Structs.Dpm.UNITS_PER_TURN      /
                      (entries[j + 1].Lba - entries[j].Lba);
        }

        using var writer = new StreamWriter(path);
        writer.NewLine = "\n";
        writer.WriteLine("sector,degrees_per_sector,hex_units_per_sector");

        for(int j = 0; j < nb; j++)
        {
            ulong  first = entries[j].Lba, last = entries[j + 1].Lba;
            double mid   = (first + last) / 2.0;
            int    layer = CommonTypes.Structs.Dpm.LayerOf(dpm, first);

            for(ulong lba = first; lba < last; lba++)
            {
                double degrees  = rate[j];
                bool   before   = lba < mid;
                int    k        = before ? j - 1 : j + 1;

                if((before ? j > 0 : j + 1 < nb)                             &&
                   CommonTypes.Structs.Dpm.LayerOf(dpm, entries[k].Lba) == layer &&
                   entries[k + 1].Lba - entries[k].Lba                  > 1)
                {
                    double mk = (entries[k].Lba + entries[k + 1].Lba) / 2.0;
                    degrees += (rate[k] - rate[j]) * (lba - mid) / (mk - mid);
                }

                WriteRow(writer, lba, degrees);
            }
        }

        WriteRow(writer, entries[^1].Lba, rate[nb - 1]);
    }

    static void WriteRow(TextWriter writer, ulong lba, double degrees)
    {
        writer.WriteLine(string.Format(CultureInfo.InvariantCulture,
                                       "{0},{1:F6},{2:F6}",
                                       lba,
                                       degrees,
                                       degrees * 256.0 / 360.0));
    }
}