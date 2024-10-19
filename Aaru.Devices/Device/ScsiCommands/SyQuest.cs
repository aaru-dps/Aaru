// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SyQuest.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SyQuest vendor commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains vendor commands for SyQuest SCSI devices.
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using Aaru.Console;

// ReSharper disable UnusedMember.Global

namespace Aaru.Devices;

public partial class Device
{
    /// <summary>Sends the SyQuest READ (6) command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="lba">Starting block.</param>
    /// <param name="blockSize">Block size in bytes.</param>
    public bool SyQuestRead6(out byte[] buffer, out byte[] senseBuffer, uint lba, uint blockSize, uint timeout,
                             out double duration) => SyQuestRead6(out buffer,
                                                                  out senseBuffer,
                                                                  lba,
                                                                  blockSize,
                                                                  1,
                                                                  false,
                                                                  false,
                                                                  timeout,
                                                                  out duration);

    /// <summary>Sends the SyQuest READ LONG (6) command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="lba">Starting block.</param>
    /// <param name="blockSize">Block size in bytes.</param>
    public bool SyQuestReadLong6(out byte[] buffer, out byte[] senseBuffer, uint lba, uint blockSize, uint timeout,
                                 out double duration) =>
        SyQuestRead6(out buffer, out senseBuffer, lba, blockSize, 1, false, true, timeout, out duration);

    /// <summary>Sends the SyQuest READ (6) command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="lba">Starting block.</param>
    /// <param name="inhibitDma">If set to <c>true</c>, block will not be transfer and will reside in the drive's buffer.</param>
    /// <param name="readLong">If set to <c>true</c> drive will return ECC bytes and disable error detection.</param>
    /// <param name="blockSize">Block size in bytes.</param>
    /// <param name="transferLength">How many blocks to read.</param>
    public bool SyQuestRead6(out byte[] buffer, out byte[] senseBuffer, uint lba, uint blockSize, byte transferLength,
                             bool       inhibitDma, bool readLong, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        var  cdb = new byte[6];
        bool sense;

        cdb[0] = (byte)ScsiCommands.Read6;
        cdb[1] = (byte)((lba & 0x1F0000) >> 16);
        cdb[2] = (byte)((lba & 0xFF00)   >> 8);
        cdb[3] = (byte)(lba & 0xFF);
        cdb[4] = transferLength;

        if(inhibitDma) cdb[5] += 0x80;

        if(readLong) cdb[5] += 0x40;

        if(!inhibitDma && !readLong)
            buffer = transferLength == 0 ? new byte[256 * blockSize] : new byte[transferLength * blockSize];
        else if(readLong)
        {
            buffer = new byte[blockSize];
            cdb[4] = 1;
        }
        else
            buffer = [];

        if(!inhibitDma)
        {
            LastError = SendScsiCommand(cdb,
                                        ref buffer,
                                        out senseBuffer,
                                        timeout,
                                        ScsiDirection.In,
                                        out duration,
                                        out sense);
        }
        else
        {
            LastError = SendScsiCommand(cdb,
                                        ref buffer,
                                        out senseBuffer,
                                        timeout,
                                        ScsiDirection.None,
                                        out duration,
                                        out sense);
        }

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.SYQUEST_READ_6_took_0_ms, duration);

        return sense;
    }

    /// <summary>Requests the usage, seek and error counters, and resets them</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool SyQuestReadUsageCounter(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration) =>
        AdaptecReadUsageCounter(out buffer, out senseBuffer, false, timeout, out duration);

    /// <summary>Sends the SyQuest READ LONG (10) command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="lba">Starting block.</param>
    /// <param name="blockSize">Block size in bytes.</param>
    public bool SyQuestReadLong10(out byte[] buffer, out byte[] senseBuffer, uint lba, uint blockSize, uint timeout,
                                  out double duration) =>
        SyQuestRead10(out buffer, out senseBuffer, lba, blockSize, 1, false, true, timeout, out duration);

    /// <summary>Sends the SyQuest READ (10) command</summary>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    /// <param name="buffer">Buffer where the SCSI READ response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <param name="lba">Starting block.</param>
    /// <param name="inhibitDma">If set to <c>true</c>, block will not be transfer and will reside in the drive's buffer.</param>
    /// <param name="readLong">If set to <c>true</c> drive will return ECC bytes and disable error detection.</param>
    /// <param name="blockSize">Block size in bytes.</param>
    /// <param name="transferLength">How many blocks to read.</param>
    public bool SyQuestRead10(out byte[] buffer, out byte[] senseBuffer, uint lba, uint blockSize,
                              ushort transferLength, bool inhibitDma, bool readLong, uint timeout, out double duration)
    {
        senseBuffer = new byte[64];
        var  cdb = new byte[10];
        bool sense;

        cdb[0] = (byte)ScsiCommands.Read10;
        cdb[2] = (byte)((lba & 0xFF000000) >> 24);
        cdb[3] = (byte)((lba & 0xFF0000)   >> 16);
        cdb[4] = (byte)((lba & 0xFF00)     >> 8);
        cdb[5] = (byte)(lba & 0xFF);
        cdb[7] = (byte)((transferLength & 0xFF00) >> 8);
        cdb[8] = (byte)(transferLength & 0xFF);

        if(inhibitDma) cdb[9] += 0x80;

        if(readLong) cdb[9] += 0x40;

        if(!inhibitDma && !readLong)
            buffer = new byte[transferLength * blockSize];
        else if(readLong)
        {
            buffer = new byte[blockSize];
            cdb[4] = 1;
        }
        else
            buffer = [];

        if(!inhibitDma)
        {
            LastError = SendScsiCommand(cdb,
                                        ref buffer,
                                        out senseBuffer,
                                        timeout,
                                        ScsiDirection.In,
                                        out duration,
                                        out sense);
        }
        else
        {
            LastError = SendScsiCommand(cdb,
                                        ref buffer,
                                        out senseBuffer,
                                        timeout,
                                        ScsiDirection.None,
                                        out duration,
                                        out sense);
        }

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.SYQUEST_READ_10_took_0_ms, duration);

        return sense;
    }
}