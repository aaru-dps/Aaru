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
//     Reads the Data Position Measurement of BlindWrite 5/6/7 disc images.
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
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;
using Aaru.CommonTypes.Structs;
using Aaru.Logging;

namespace Aaru.Images;

public sealed partial class BlindWrite5
{
    /// <summary>Offset of the start sector in the DPM block stored in the descriptor</summary>
    const int INTERNAL_DPM_HEADER = 32;

    DataPositionMeasurement? _dpmData;

    /// <inheritdoc />
    public ErrorNumber ReadDpm(out DataPositionMeasurement dpm)
    {
        dpm = _dpmData ?? default(DataPositionMeasurement);

        return _dpmData.HasValue ? ErrorNumber.NoError : ErrorNumber.NoData;
    }

    /// <summary>
    ///     Gets the Data Position Measurement from the external BWA file, or else from the block stored in the
    ///     descriptor.
    /// </summary>
    /// <remarks>
    ///     The block in the descriptor does not necessarily cover the whole disc, only its beginning, that copy
    ///     protections usually check, while the BWA file usually covers the whole disc, so it is preferred, as libmirage
    ///     does. Both store a start sector, a resolution and cumulative angles, 256 per turn, like Alcohol 120%.
    /// </remarks>
    internal void LoadDpm(IFilter imageFilter)
    {
        _dpmData = null;

        DataPositionMeasurement? external = BlindWrite4.LoadBwa(imageFilter);

        if(external.HasValue)
        {
            _dpmData = external;

            return;
        }

        // Four fields of fixed values (1, 1, 0, 0), the block length twice, and two more fields of fixed values (0, 1)
        if(_dpm is { Length: > INTERNAL_DPM_HEADER })
        {
            AaruLogging.Debug(MODULE_NAME,
                              "Internal DPM block length: {0}, {1} (should be the same)",
                              BinaryPrimitives.ReadUInt32LittleEndian(_dpm.AsSpan(16)),
                              BinaryPrimitives.ReadUInt32LittleEndian(_dpm.AsSpan(20)));

            _dpmData = DecodeDpm(_dpm, INTERNAL_DPM_HEADER);
        }

        if(_dpmData is null) AaruLogging.Debug(MODULE_NAME, "No DPM found");
    }

    /// <summary>Decodes a start sector, a resolution, a count and that many 256 per turn cumulative angles</summary>
    internal static DataPositionMeasurement? DecodeDpm(byte[] data, int offset)
    {
        if(data.Length < offset + 12) return null;

        uint start      = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset));
        uint resolution = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset + 4));
        uint count      = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset + 8));

        AaruLogging.Debug(MODULE_NAME, "DPM start sector {0}, resolution {1}, {2} entries", start, resolution, count);

        if(count == 0 || data.Length < offset + 12 + (long)count * 4) return null;

        var angles = new uint[count];

        for(int i = 0; i < angles.Length; i++)
            angles[i] = BinaryPrimitives.ReadUInt32LittleEndian(data.AsSpan(offset + 12 + i * 4));

        return Dpm.FromGrid(start, resolution, angles);
    }
}