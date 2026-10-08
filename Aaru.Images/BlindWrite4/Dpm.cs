// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dpm.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Reads the Data Position Measurement BlindWrite stores in BWA files.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Logging;

namespace Aaru.Images;

public sealed partial class BlindWrite4
{
    /// <summary>Offset of the start sector in a BWA file, after three fields of fixed values (1, 8, 1)</summary>
    const int BWA_DPM_HEADER = 12;

    DataPositionMeasurement? _dpm;

    /// <inheritdoc />
    public ErrorNumber ReadDpm(out DataPositionMeasurement dpm)
    {
        dpm = _dpm ?? default(DataPositionMeasurement);

        return _dpm.HasValue ? ErrorNumber.NoError : ErrorNumber.NoData;
    }

    /// <summary>Reads the BWA file that sits next to a BlindWrite descriptor, with the same name in any case</summary>
    /// <param name="imageFilter">Filter of the descriptor</param>
    /// <returns>The DPM, or <c>null</c> if there is no valid BWA file</returns>
    internal static DataPositionMeasurement? LoadBwa(IFilter imageFilter)
    {
        string folder = imageFilter.ParentFolder;
        string name   = Path.GetFileNameWithoutExtension(imageFilter.Filename);

        if(string.IsNullOrEmpty(folder) || string.IsNullOrEmpty(name) || !Directory.Exists(folder)) return null;

        string bwaPath = Directory.EnumerateFiles(folder)
                                  .FirstOrDefault(f => string.Equals(Path.GetFileName(f),
                                                                     name + ".bwa",
                                                                     StringComparison.OrdinalIgnoreCase));

        if(bwaPath is null) return null;

        AaruLogging.Debug(MODULE_NAME, "Found BWA file {0}", bwaPath);

        try
        {
            DataPositionMeasurement? dpm = DecodeBwa(File.ReadAllBytes(bwaPath));

            if(dpm is null) AaruLogging.Debug(MODULE_NAME, "Invalid BWA file, ignoring it");

            return dpm;
        }
        catch(IOException ex)
        {
            AaruLogging.Debug(MODULE_NAME, "Could not read BWA file: {0}", ex.Message);

            return null;
        }
    }

    /// <summary>Decodes a BWA file</summary>
    /// <remarks>
    ///     After three fields of fixed values come the start sector, the resolution, and a count of values, then that
    ///     many values. The first value is not an angle, it is bigger than the ones after it and its meaning is unknown,
    ///     so the angles are the rest, cumulative, 256 per turn, the first one at the start sector plus the resolution.
    ///     libmirage takes the first value as an angle too.
    /// </remarks>
    /// <param name="bwa">Contents of the BWA file</param>
    /// <returns>The DPM, or <c>null</c> if the file is not valid</returns>
    internal static DataPositionMeasurement? DecodeBwa(byte[] bwa)
    {
        if(bwa.Length < BWA_DPM_HEADER + 12) return null;

        uint start      = BinaryPrimitives.ReadUInt32LittleEndian(bwa.AsSpan(BWA_DPM_HEADER));
        uint resolution = BinaryPrimitives.ReadUInt32LittleEndian(bwa.AsSpan(BWA_DPM_HEADER + 4));
        uint count      = BinaryPrimitives.ReadUInt32LittleEndian(bwa.AsSpan(BWA_DPM_HEADER + 8));
        int  first      = BWA_DPM_HEADER + 12;

        AaruLogging.Debug(MODULE_NAME,
                          "BWA DPM start sector {0}, resolution {1}, {2} values",
                          start,
                          resolution,
                          count);

        if(count < 2 || bwa.Length < first + (long)count * 4) return null;

        var values = new uint[count];

        for(int i = 0; i < values.Length; i++)
            values[i] = BinaryPrimitives.ReadUInt32LittleEndian(bwa.AsSpan(first + i * 4));

        if(values[0] <= values[1]) return Dpm.FromGrid(start, resolution, values);

        AaruLogging.Debug(MODULE_NAME, "BWA first value {0} is not an angle, skipping it", values[0]);

        return Dpm.FromGrid(start, resolution, values.AsSpan(1));
    }
}