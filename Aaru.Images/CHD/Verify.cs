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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Aaru.Checksums;
using Aaru.CommonTypes.Enums;

namespace Aaru.DiscImages;

public sealed partial class Chd
{
#region IOpticalMediaImage Members

    /// <inheritdoc />
    public bool? VerifySector(ulong sectorAddress)
    {
        if(_isHdd)
            return null;

        ErrorNumber errno = ReadSectorLong(sectorAddress, out byte[] buffer);

        return errno != ErrorNumber.NoError ? null : CdChecksums.CheckCdSector(buffer);
    }

    /// <inheritdoc />
    public bool? VerifySectors(ulong           sectorAddress, uint length, out List<ulong> failingLbas,
                               out List<ulong> unknownLbas)
    {
        unknownLbas = new List<ulong>();
        failingLbas = new List<ulong>();

        if(_isHdd)
            return null;

        ErrorNumber errno = ReadSectorsLong(sectorAddress, length, out byte[] buffer);

        if(errno != ErrorNumber.NoError)
            return null;

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

        if(unknownLbas.Count > 0)
            return null;

        return failingLbas.Count <= 0;
    }

    /// <inheritdoc />
    public bool? VerifySectors(ulong           sectorAddress, uint length, uint track, out List<ulong> failingLbas,
                               out List<ulong> unknownLbas)
    {
        unknownLbas = new List<ulong>();
        failingLbas = new List<ulong>();

        if(_isHdd)
            return null;

        ErrorNumber errno = ReadSectorsLong(sectorAddress, length, track, out byte[] buffer);

        if(errno != ErrorNumber.NoError)
            return null;

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

        if(unknownLbas.Count > 0)
            return null;

        return failingLbas.Count <= 0;
    }

#endregion

#region IVerifiableImage Members

    /// <inheritdoc />
    public bool? VerifyMediaImage()
    {
        byte[] calculated;

        if(_mapVersion >= 3)
        {
            var sha1Ctx = new Sha1Context();

            for(uint i = 0; i < _totalHunks; i++)
            {
                ErrorNumber errno = GetHunk(i, out byte[] buffer);

                if(errno != ErrorNumber.NoError)
                    return null;

                sha1Ctx.Update(buffer);
            }

            calculated = sha1Ctx.Final();
        }
        else
        {
            var md5Ctx = new Md5Context();

            for(uint i = 0; i < _totalHunks; i++)
            {
                ErrorNumber errno = GetHunk(i, out byte[] buffer);

                if(errno != ErrorNumber.NoError)
                    return null;

                md5Ctx.Update(buffer);
            }

            calculated = md5Ctx.Final();
        }

        return _expectedChecksum.SequenceEqual(calculated);
    }

#endregion
}