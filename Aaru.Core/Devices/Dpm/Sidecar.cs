// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Sidecar.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Core algorithms.
//
// --[ Description ] ----------------------------------------------------------
//
//     Writes the Data Position Measurement sidecar files.
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

using System;
using System.IO;
using Aaru.CommonTypes.Structs;
using Aaru.Core.Graphics;
using Aaru.Logging;

namespace Aaru.Core.Devices.Dpm;

/// <summary>Writes the Data Position Measurement files that sit next to an image</summary>
public static class DpmSidecar
{
    const string MODULE_NAME = "DPM";

    /// <summary>Writes the DPM graph as <c>prefix.dpm.png</c></summary>
    /// <param name="dpm">DPM</param>
    /// <param name="prefix">Path prefix of the image</param>
    /// <param name="width">Width of the graph in pixels</param>
    /// <param name="height">Height of the graph in pixels</param>
    /// <returns><c>true</c> if the graph was written, <c>false</c> otherwise</returns>
    public static bool Write(DataPositionMeasurement dpm, string prefix, int width = DpmGraph.DEFAULT_WIDTH,
                             int                     height = DpmGraph.DEFAULT_HEIGHT)
    {
        try
        {
            DpmGraph.Write(dpm, prefix + ".dpm.png", Path.GetFileName(prefix), width, height);

            return true;
        }
        catch(Exception ex)
        {
            AaruLogging.Debug(MODULE_NAME, "Could not write the DPM graph: {0}", ex.Message);
            AaruLogging.Exception(ex, "Could not write the DPM graph");

            return false;
        }
    }
}