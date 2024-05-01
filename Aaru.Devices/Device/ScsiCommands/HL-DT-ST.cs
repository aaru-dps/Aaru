// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : HL-DT-ST.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : HL-DT-ST vendor commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains vendor commands for HL-DT-ST SCSI devices.
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

using System.Collections.Generic;
using Aaru.CommonTypes.Enums;
using Aaru.Console;
using Aaru.Decoders.DVD;
using Aaru.Helpers;

namespace Aaru.Devices;

public partial class Device
{
    readonly Sector _decoding = new();

    /// <summary>Reads a "raw" sector from DVD on HL-DT-ST drives.</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the HL-DT-ST READ DVD (RAW) response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="lba">Start block address.</param>
    /// <param name="transferLength">How many blocks to read.</param>
    public bool HlDtStReadRawDvd(out byte[] buffer, out byte[] senseBuffer, uint lba, uint transferLength, uint timeout,
                                 out double duration)
    {
        senseBuffer = new byte[64];
        var cdb = new byte[12];
        buffer = new byte[2064 * transferLength];

        uint cacheDataOffset = 0x80000000 + lba % 96 * 2064;

        cdb[0]  = (byte)ScsiCommands.HlDtStVendor;
        cdb[1]  = 0x48;
        cdb[2]  = 0x49;
        cdb[3]  = 0x54;
        cdb[4]  = 0x01;
        cdb[6]  = (byte)((cacheDataOffset & 0xFF000000) >> 24);
        cdb[7]  = (byte)((cacheDataOffset & 0xFF0000)   >> 16);
        cdb[8]  = (byte)((cacheDataOffset & 0xFF00)     >> 8);
        cdb[9]  = (byte)(cacheDataOffset & 0xFF);
        cdb[10] = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[11] = (byte)(buffer.Length & 0xFF);

        LastError = SendScsiCommand(cdb,
                                    ref buffer,
                                    out senseBuffer,
                                    timeout,
                                    ScsiDirection.In,
                                    out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.HL_DT_ST_READ_DVD_RAW_took_0_ms, duration);

        if(!CheckSectorNumber(buffer, lba, transferLength)) return true;

        if(_decoding.Scramble(buffer, transferLength, out byte[] scrambledBuffer) != ErrorNumber.NoError) return true;

        buffer = scrambledBuffer;

        return sense;
    }

    /// <summary>
    ///     Makes sure the data's sector number is the one expected.
    /// </summary>
    /// <param name="buffer">Data buffer</param>
    /// <param name="firstLba">First consecutive LBA of the buffer</param>
    /// <param name="transferLength">How many blocks to in buffer</param>
    /// <returns><c>false</c> if any sector is not matching expected value, else <c>true</c></returns>
    static bool CheckSectorNumber(IReadOnlyList<byte> buffer, uint firstLba, uint transferLength)
    {
        for(var i = 0; i < transferLength; i++)
        {
            byte[] sectorBuffer =
            {
                0x0, buffer[1 + 2064 * i], buffer[2 + 2064 * i], buffer[3 + 2064 * i]
            };

            var sectorNumber = BigEndianBitConverter.ToUInt32(sectorBuffer, 0);

            if(sectorNumber != firstLba + i + 0x30000) return false;
        }

        return true;
    }
}