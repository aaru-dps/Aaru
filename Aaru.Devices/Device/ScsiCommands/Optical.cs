// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Optical.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SCSI Block Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains SCSI commands for optical memory devices.
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
using Aaru.Console;
using Aaru.Decoders.SCSI;

namespace Aaru.Devices;

public partial class Device
{
    /// <summary>Scan the medium for a contiguous set of written or blank logical blocks</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="relAddr">Set to <c>true</c> if <paramref name="lba" /> is relative</param>
    /// <param name="lba">Logical block address where to start the search.</param>
    /// <param name="scanLength">Number of blocks to scan</param>
    /// <param name="foundBlocks">How many blocks were found</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    /// <param name="written">
    ///     If set to <c>true</c> drive will search for written blocks, otherwise it will search for blank
    ///     blocks
    /// </param>
    /// <param name="advancedScan">If set to <c>true</c> drive will consider the search area has contiguous blocks</param>
    /// <param name="reverse">If set to <c>true</c> drive will search in reverse</param>
    /// <param name="partial">
    ///     If set to <c>true</c> return even if the total number of blocks requested is not found but the
    ///     other parameters are met
    /// </param>
    /// <param name="requested">Number of contiguous blocks to find</param>
    /// <param name="foundLba">First LBA found</param>
    public bool MediumScan(out byte[] senseBuffer, bool written, bool advancedScan, bool reverse, bool partial,
                           bool relAddr, uint lba, uint requested, uint scanLength, out uint foundLba,
                           out uint foundBlocks, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb    = new byte[10];
        byte[] buffer = Array.Empty<byte>();
        foundLba    = 0;
        foundBlocks = 0;

        cdb[0] = (byte)ScsiCommands.MediumScan;

        if(written)
            cdb[1] += 0x10;

        if(advancedScan)
            cdb[1] += 0x08;

        if(reverse)
            cdb[1] += 0x04;

        if(partial)
            cdb[1] += 0x02;

        if(relAddr)
            cdb[1] += 0x01;

        cdb[2] = (byte)((lba & 0xFF000000) >> 24);
        cdb[3] = (byte)((lba & 0xFF0000)   >> 16);
        cdb[4] = (byte)((lba & 0xFF00)     >> 8);
        cdb[5] = (byte)(lba & 0xFF);

        if(requested  > 0 ||
           scanLength > 1)
        {
            buffer    = new byte[8];
            buffer[0] = (byte)((requested & 0xFF000000) >> 24);
            buffer[1] = (byte)((requested & 0xFF0000)   >> 16);
            buffer[2] = (byte)((requested & 0xFF00)     >> 8);
            buffer[3] = (byte)(requested & 0xFF);
            buffer[4] = (byte)((scanLength & 0xFF000000) >> 24);
            buffer[5] = (byte)((scanLength & 0xFF0000)   >> 16);
            buffer[6] = (byte)((scanLength & 0xFF00)     >> 8);
            buffer[7] = (byte)(scanLength & 0xFF);
            cdb[8]    = 8;
        }

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout,
                                    buffer.Length == 0 ? ScsiDirection.None : ScsiDirection.Out, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.MEDIUM_SCAN_took_0_ms, duration);

        if(Error)
            return sense;

        DecodedSense? decodedSense = Sense.Decode(senseBuffer);

        switch(decodedSense?.SenseKey)
        {
            case SenseKeys.NoSense: return false;
            case SenseKeys.Equal when decodedSense.Value.Fixed?.InformationValid == true:
                foundBlocks = decodedSense.Value.Fixed.Value.CommandSpecific;
                foundLba    = decodedSense.Value.Fixed.Value.Information;

                return false;
            default: return sense;
        }
    }
}