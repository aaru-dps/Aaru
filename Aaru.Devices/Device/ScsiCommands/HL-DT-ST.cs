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

using Aaru.Console;

namespace Aaru.Devices;

public partial class Device
{
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
        byte[] cdb = new byte[12];
        buffer = new byte[2064 * transferLength];

        cdb[0]  = (byte)ScsiCommands.HlDtStVendor;
        cdb[1]  = 0x48;
        cdb[2]  = 0x49;
        cdb[3]  = 0x54;
        cdb[4]  = 0x01;
        cdb[6]  = (byte)((lba & 0xFF000000) >> 24);
        cdb[7]  = (byte)((lba & 0xFF0000)   >> 16);
        cdb[8]  = (byte)((lba & 0xFF00)     >> 8);
        cdb[9]  = (byte)(lba & 0xFF);
        cdb[10] = (byte)((buffer.Length & 0xFF00) >> 8);
        cdb[11] = (byte)(buffer.Length & 0xFF);

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.HL_DT_ST_READ_DVD_RAW_took_0_ms, duration);

        return sense;
    }
}