// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Plextor.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CompactDisc dumping.
//
// --[ Description ] ----------------------------------------------------------
//
//     Enables reading subchannel using Plextor vendor command.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Core.Devices.Dumping;

using System;
using Aaru.Devices;

partial class Dump
{
    /// <summary>Reads a sector using Plextor's D8h READ CDDA command with subchannel</summary>
    /// <param name="cmdBuf">Data buffer</param>
    /// <param name="senseBuf">Sense buffer</param>
    /// <param name="firstSectorToRead">Fix sector to read</param>
    /// <param name="blockSize">Sector size in bytes</param>
    /// <param name="blocksToRead">How many sectors to read</param>
    /// <param name="supportedPlextorSubchannel">Supported subchannel type</param>
    /// <param name="cmdDuration">Time spent sending commands to the drive</param>
    /// <returns><c>true</c> if an error occured, <c>false</c> otherwise</returns>
    bool ReadPlextorWithSubchannel(out byte[] cmdBuf, out byte[] senseBuf, uint firstSectorToRead, uint blockSize,
                                   uint blocksToRead, PlextorSubchannel supportedPlextorSubchannel,
                                   out double cmdDuration)
    {
        bool sense;
        cmdBuf = null;

        if(supportedPlextorSubchannel == PlextorSubchannel.None)
        {
            sense = _dev.PlextorReadCdDa(out cmdBuf, out senseBuf, firstSectorToRead, blockSize, blocksToRead,
                                         supportedPlextorSubchannel, 0, out cmdDuration);

            if(!sense)
                return false;

            // As a workaround for some firmware bugs, seek far away.
            _dev.PlextorReadCdDa(out _, out senseBuf, firstSectorToRead - 32, blockSize, blocksToRead,
                                 supportedPlextorSubchannel, 0, out _);

            sense = _dev.PlextorReadCdDa(out cmdBuf, out senseBuf, firstSectorToRead, blockSize, blocksToRead,
                                         supportedPlextorSubchannel, _dev.Timeout, out cmdDuration);

            return sense;
        }

        byte[] subBuf;

        uint subSize = supportedPlextorSubchannel == PlextorSubchannel.Q16 ? 16u : 96u;

        if(supportedPlextorSubchannel is PlextorSubchannel.Q16 or PlextorSubchannel.Pack)
        {
            sense = _dev.PlextorReadCdDa(out cmdBuf, out senseBuf, firstSectorToRead, 2352 + subSize, blocksToRead,
                                         supportedPlextorSubchannel, _dev.Timeout, out cmdDuration);

            if(!sense)
                return false;
        }

        // As a workaround for some firmware bugs, seek far away.
        _dev.PlextorReadCdDa(out _, out senseBuf, firstSectorToRead - 32, blockSize, blocksToRead,
                             supportedPlextorSubchannel, 0, out _);

        sense = _dev.PlextorReadCdDa(out byte[] dataBuf, out senseBuf, firstSectorToRead, 2352, blocksToRead,
                                     PlextorSubchannel.None, 0, out cmdDuration);

        if(sense)
            return true;

        sense = _dev.PlextorReadCdDa(out subBuf, out senseBuf, firstSectorToRead, subSize, blocksToRead,
                                     supportedPlextorSubchannel == PlextorSubchannel.Pack ? PlextorSubchannel.All
                                         : supportedPlextorSubchannel, 0, out cmdDuration);

        if(sense)
            return true;

        cmdBuf = new byte[2352 * blocksToRead + subSize * blocksToRead];

        for(var b = 0; b < blocksToRead; b++)
        {
            Array.Copy(dataBuf, 2352   * b, cmdBuf, (2352 + subSize) * b, 2352);
            Array.Copy(subBuf, subSize * b, cmdBuf, (2352 + subSize) * b + 2352, subSize);
        }

        return false;
    }
}