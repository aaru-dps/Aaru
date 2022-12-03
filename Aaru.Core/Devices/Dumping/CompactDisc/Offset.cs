// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Offset.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : CompactDisc dumping.
//
// --[ Description ] ----------------------------------------------------------
//
//     Calculates CompactDisc data offset.
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

// ReSharper disable JoinDeclarationAndInitializer
// ReSharper disable InlineOutVariableDeclaration
// ReSharper disable TooWideLocalVariableScope

using System;
using Aaru.Devices;

namespace Aaru.Core.Devices.Dumping;

partial class Dump
{
    /// <summary>Fix offset in audio/scrambled sectors</summary>
    /// <param name="offsetBytes">Offset in bytes</param>
    /// <param name="sectorSize">Sector size in bytes</param>
    /// <param name="sectorsForOffset">How many extra sectors we got for offset</param>
    /// <param name="supportedSubchannel">Subchannel type</param>
    /// <param name="blocksToRead">How many sectors did we got</param>
    /// <param name="subSize">Subchannel size in bytes</param>
    /// <param name="cmdBuf">Data buffer</param>
    /// <param name="blockSize">Block size in bytes</param>
    /// <param name="failedCrossingLeadOut">Set if we failed to cross into the Lead-Out</param>
    static void FixOffsetData(int offsetBytes, uint sectorSize, int sectorsForOffset, MmcSubchannel supportedSubchannel,
                              ref uint blocksToRead, uint subSize, ref byte[] cmdBuf, uint blockSize,
                              bool failedCrossingLeadOut)
    {
        if(cmdBuf.Length == 0)
            return;

        int offsetFix = offsetBytes < 0 ? (int)((sectorSize * sectorsForOffset) + offsetBytes) : offsetBytes;

        byte[] tmpBuf;

        if(supportedSubchannel != MmcSubchannel.None)
        {
            // De-interleave subchannel
            byte[] data = new byte[sectorSize * blocksToRead];
            byte[] sub  = new byte[subSize    * blocksToRead];

            for(int b = 0; b < blocksToRead; b++)
            {
                Array.Copy(cmdBuf, (int)(0          + (b * blockSize)), data, sectorSize * b, sectorSize);
                Array.Copy(cmdBuf, (int)(sectorSize + (b * blockSize)), sub, subSize     * b, subSize);
            }

            if(failedCrossingLeadOut)
            {
                blocksToRead += (uint)sectorsForOffset;

                tmpBuf = new byte[sectorSize * blocksToRead];
                Array.Copy(data, 0, tmpBuf, 0, data.Length);
                data   = tmpBuf;
                tmpBuf = new byte[subSize * blocksToRead];
                Array.Copy(sub, 0, tmpBuf, 0, sub.Length);
                sub = tmpBuf;
            }

            tmpBuf = new byte[sectorSize * (blocksToRead - sectorsForOffset)];
            Array.Copy(data, offsetFix, tmpBuf, 0, tmpBuf.Length);
            data = tmpBuf;

            blocksToRead -= (uint)sectorsForOffset;

            // Re-interleave subchannel
            cmdBuf = new byte[blockSize * blocksToRead];

            for(int b = 0; b < blocksToRead; b++)
            {
                Array.Copy(data, sectorSize * b, cmdBuf, (int)(0          + (b * blockSize)), sectorSize);
                Array.Copy(sub, subSize     * b, cmdBuf, (int)(sectorSize + (b * blockSize)), subSize);
            }
        }
        else
        {
            if(failedCrossingLeadOut)
            {
                blocksToRead += (uint)sectorsForOffset;

                tmpBuf = new byte[blockSize * blocksToRead];
                Array.Copy(cmdBuf, 0, tmpBuf, 0, cmdBuf.Length);
                cmdBuf = tmpBuf;
            }

            tmpBuf = new byte[blockSize * (blocksToRead - sectorsForOffset)];
            Array.Copy(cmdBuf, offsetFix, tmpBuf, 0, tmpBuf.Length);
            cmdBuf       =  tmpBuf;
            blocksToRead -= (uint)sectorsForOffset;
        }
    }
}