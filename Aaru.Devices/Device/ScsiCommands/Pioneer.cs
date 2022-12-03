// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Pioneer.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Pioneer vendor commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains vendor commands for Pioneer SCSI devices.
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
    /// <summary>Sends the Pioneer READ CD-DA command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the Pioneer READ CD-DA response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="lba">Start block address.</param>
    /// <param name="transferLength">How many blocks to read.</param>
    /// <param name="blockSize">Block size.</param>
    /// <param name="subchannel">Subchannel selection.</param>
    public bool PioneerReadCdDa(out byte[] buffer, out byte[] senseBuffer, uint lba, uint blockSize,
                                uint transferLength, PioneerSubchannel subchannel, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[12];

        cdb[0]  = (byte)ScsiCommands.ReadCdDa;
        cdb[2]  = (byte)((lba & 0xFF000000) >> 24);
        cdb[3]  = (byte)((lba & 0xFF0000)   >> 16);
        cdb[4]  = (byte)((lba & 0xFF00)     >> 8);
        cdb[5]  = (byte)(lba & 0xFF);
        cdb[7]  = (byte)((transferLength & 0xFF0000) >> 16);
        cdb[8]  = (byte)((transferLength & 0xFF00)   >> 8);
        cdb[9]  = (byte)(transferLength & 0xFF);
        cdb[10] = (byte)subchannel;

        buffer = new byte[blockSize * transferLength];

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.PIONEER_READ_CD_DA_took_0_ms, duration);

        return sense;
    }

    /// <summary>Sends the Pioneer READ CD-DA MSF command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the Pioneer READ CD-DA MSF response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="startMsf">Start MM:SS:FF of read encoded as 0x00MMSSFF.</param>
    /// <param name="endMsf">End MM:SS:FF of read encoded as 0x00MMSSFF.</param>
    /// <param name="blockSize">Block size.</param>
    /// <param name="subchannel">Subchannel selection.</param>
    public bool PioneerReadCdDaMsf(out byte[] buffer, out byte[] senseBuffer, uint startMsf, uint endMsf,
                                   uint blockSize, PioneerSubchannel subchannel, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[12];

        cdb[0]  = (byte)ScsiCommands.ReadCdDaMsf;
        cdb[3]  = (byte)((startMsf & 0xFF0000) >> 16);
        cdb[4]  = (byte)((startMsf & 0xFF00)   >> 8);
        cdb[5]  = (byte)(startMsf & 0xFF);
        cdb[7]  = (byte)((endMsf & 0xFF0000) >> 16);
        cdb[8]  = (byte)((endMsf & 0xFF00)   >> 8);
        cdb[9]  = (byte)(endMsf & 0xFF);
        cdb[10] = (byte)subchannel;

        uint transferLength = (uint)(((cdb[7] - cdb[3]) * 60 * 75) + ((cdb[8] - cdb[4]) * 75) + (cdb[9] - cdb[5]));
        buffer = new byte[blockSize * transferLength];

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.PIONEER_READ_CD_DA_MSF_took_0_ms, duration);

        return sense;
    }

    /// <summary>Sends the Pioneer READ CD-XA command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the Pioneer READ CD-XA response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="errorFlags">
    ///     If set to <c>true</c>, returns all sector data with 294 bytes of error flags. Superseedes
    ///     <paramref name="wholeSector" />
    /// </param>
    /// <param name="wholeSector">If set to <c>true</c>, returns all 2352 bytes of sector data.</param>
    /// <param name="lba">Start block address.</param>
    /// <param name="transferLength">How many blocks to read.</param>
    public bool PioneerReadCdXa(out byte[] buffer, out byte[] senseBuffer, uint lba, uint transferLength,
                                bool errorFlags, bool wholeSector, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        byte[] cdb = new byte[12];

        cdb[0] = (byte)ScsiCommands.ReadCdXa;
        cdb[2] = (byte)((lba & 0xFF000000) >> 24);
        cdb[3] = (byte)((lba & 0xFF0000)   >> 16);
        cdb[4] = (byte)((lba & 0xFF00)     >> 8);
        cdb[5] = (byte)(lba & 0xFF);
        cdb[7] = (byte)((transferLength & 0xFF0000) >> 16);
        cdb[8] = (byte)((transferLength & 0xFF00)   >> 8);
        cdb[9] = (byte)(transferLength & 0xFF);

        if(errorFlags)
        {
            buffer = new byte[2646 * transferLength];
            cdb[6] = 0x1F;
        }
        else if(wholeSector)
        {
            buffer = new byte[2352 * transferLength];
            cdb[6] = 0x0F;
        }
        else
        {
            buffer = new byte[2048 * transferLength];
            cdb[6] = 0x00;
        }

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine("SCSI Device", Localization.PIONEER_READ_CD_XA_took_0_ms, duration);

        return sense;
    }
}