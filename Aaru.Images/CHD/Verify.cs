// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Verify.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Disk image plugins.
//
// --[ Description ] ----------------------------------------------------------
//
//     Verifies MAME Compressed Hunks of Data disk images.
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
using System.Collections.Generic;
using Aaru.Checksums;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Interfaces;

namespace Aaru.Images;

public sealed partial class Chd
{
#region IVerifiableImage Members

    /// <inheritdoc />
    public bool? VerifyMediaImage()
    {
        if(_expectedChecksum is null) return null;

        // Checksums only cover the logical size, not the padding of the last hunk
        IChecksum ctx       = _mapVersion >= 3 ? new Sha1Context() : new Md5Context();
        ulong     remaining = _imageInfo.ImageSize;

        for(uint i = 0; i < _totalHunks && remaining > 0; i++)
        {
            ErrorNumber errno = GetHunk(i, out byte[] buffer);

            if(errno != ErrorNumber.NoError) return null;

            var length = (int)Math.Min((ulong)buffer.Length, remaining);

            ctx.Update(length == buffer.Length ? buffer : buffer[..length]);
            remaining -= (ulong)length;
        }

        byte[] calculated = ctx.Final();

        return _expectedChecksum.SequenceEqual(calculated);
    }

#endregion

#region IOpticalMediaImage Members

    /// <inheritdoc />
    public bool? VerifySector(ulong sectorAddress)
    {
        if(_isHdd) return null;

        ErrorNumber errno = ReadSectorLong(sectorAddress, false, out byte[] buffer, out _);

        return errno != ErrorNumber.NoError ? null : CdChecksums.CheckCdSector(buffer);
    }

    /// <inheritdoc />
    public bool? VerifySectors(ulong           sectorAddress, uint length, out List<ulong> failingLbas,
                               out List<ulong> unknownLbas)
    {
        unknownLbas = [];
        failingLbas = [];

        if(_isHdd) return null;

        ErrorNumber errno = ReadSectorsLong(sectorAddress, false, length, out byte[] buffer, out _);

        if(errno != ErrorNumber.NoError) return null;

        var bps    = (int)(buffer.Length / length);
        var sector = new byte[bps];

        for(var i = 0; i < length; i++)
        {
            Array.Copy(buffer, i * bps, sector, 0, bps);
            bool? sectorStatus = CdChecksums.CheckCdSector(sector);

            switch(sectorStatus)
            {
                case null:
                    unknownLbas.Add((ulong)i + sectorAddress);

                    break;
                case false:
                    failingLbas.Add((ulong)i + sectorAddress);

                    break;
            }
        }

        if(unknownLbas.Count > 0) return null;

        return failingLbas.Count <= 0;
    }

    /// <inheritdoc />
    public bool? VerifySectors(ulong           sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                               out List<ulong> unknownLbas)
    {
        unknownLbas = [];
        failingLbas = [];

        if(_isHdd) return null;

        ErrorNumber errno = ReadSectorsLong(sectorAddress, length, track, out byte[] buffer, out _);

        if(errno != ErrorNumber.NoError) return null;

        var bps    = (int)(buffer.Length / length);
        var sector = new byte[bps];

        for(var i = 0; i < length; i++)
        {
            Array.Copy(buffer, i * bps, sector, 0, bps);
            bool? sectorStatus = CdChecksums.CheckCdSector(sector);

            switch(sectorStatus)
            {
                case null:
                    unknownLbas.Add((ulong)i + sectorAddress);

                    break;
                case false:
                    failingLbas.Add((ulong)i + sectorAddress);

                    break;
            }
        }

        if(unknownLbas.Count > 0) return null;

        return failingLbas.Count <= 0;
    }

#endregion
}