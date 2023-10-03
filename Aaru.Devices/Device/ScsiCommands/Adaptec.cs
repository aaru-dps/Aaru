// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Adaptec.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Adaptec vendor commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains vendor commands for Adaptec ACB-4000A and
//     ACB-4070 ST-506 to SCSI controllers.
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

namespace Aaru.Devices;

public partial class Device
{
    /// <summary>Gets the underlying drive cylinder, head and index bytes for the specified SCSI LBA.</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="lba">SCSI Logical Block Address.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool AdaptecTranslate(out byte[] buffer, out byte[] senseBuffer, uint lba, uint timeout,
                                 out double duration) =>
        AdaptecTranslate(out buffer, out senseBuffer, false, lba, timeout, out duration);

    /// <summary>Gets the underlying drive cylinder, head and index bytes for the specified SCSI LBA.</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="drive1">If set to <c>true</c> request the data from drive 1.</param>
    /// <param name="lba">SCSI Logical Block Address.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool AdaptecTranslate(out byte[] buffer, out byte[] senseBuffer, bool drive1, uint lba, uint timeout,
                                 out double duration)
    {
        buffer = new byte[8];
        var cdb = new byte[6];
        senseBuffer = new byte[64];

        cdb[0] = (byte)ScsiCommands.AdaptecTranslate;
        cdb[1] = (byte)((lba & 0x1F0000) >> 16);
        cdb[2] = (byte)((lba & 0xFF00)   >> 8);
        cdb[3] = (byte)(lba & 0xFF);

        if(drive1)
            cdb[1] += 0x20;

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.ADAPTEC_TRANSLATE_took_0_ms, duration);

        return sense;
    }

    /// <summary>Sets the error threshold</summary>
    /// <returns><c>true</c>, if set error threshold was adapteced, <c>false</c> otherwise.</returns>
    /// <param name="threshold">Threshold. 0 to disable error reporting.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool AdaptecSetErrorThreshold(byte threshold, out byte[] senseBuffer, uint timeout, out double duration) =>
        AdaptecSetErrorThreshold(threshold, out senseBuffer, false, timeout, out duration);

    /// <summary>Sets the error threshold</summary>
    /// <returns><c>true</c>, if set error threshold was adapteced, <c>false</c> otherwise.</returns>
    /// <param name="threshold">Threshold. 0 to disable error reporting.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="drive1">If set to <c>true</c> set the threshold from drive 1.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool AdaptecSetErrorThreshold(byte       threshold, out byte[] senseBuffer, bool drive1, uint timeout,
                                         out double duration)
    {
        var buffer = new byte[1];
        buffer[0] = threshold;
        var cdb = new byte[6];
        senseBuffer = new byte[64];

        cdb[0] = (byte)ScsiCommands.AdaptecSetErrorThreshold;

        if(drive1)
            cdb[1] += 0x20;

        cdb[4] = 1;

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.Out, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.ADAPTEC_SET_ERROR_THRESHOLD_took_0_ms, duration);

        return sense;
    }

    /// <summary>Requests the usage, seek and error counters, and resets them</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool AdaptecReadUsageCounter(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration) =>
        AdaptecReadUsageCounter(out buffer, out senseBuffer, false, timeout, out duration);

    /// <summary>Requests the usage, seek and error counters, and resets them</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="drive1">If set to <c>true</c> get the counters from drive 1.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool AdaptecReadUsageCounter(out byte[] buffer, out byte[] senseBuffer, bool drive1, uint timeout,
                                        out double duration)
    {
        buffer = new byte[9];
        var cdb = new byte[6];
        senseBuffer = new byte[64];

        cdb[0] = (byte)ScsiCommands.AdaptecTranslate;

        if(drive1)
            cdb[1] += 0x20;

        cdb[4] = (byte)buffer.Length;

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.ADAPTEC_READ_RESET_USAGE_COUNTER_took_0_ms, duration);

        return sense;
    }

    /// <summary>Fills the Adaptec controller RAM with 1K bytes of data</summary>
    /// <param name="buffer">Data to fill the buffer with.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool AdaptecWriteBuffer(byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
    {
        var oneKBuffer = new byte[1024];
        Array.Copy(buffer, 0, oneKBuffer, 0, buffer.Length < 1024 ? buffer.Length : 1024);

        var cdb = new byte[6];
        senseBuffer = new byte[64];

        cdb[0] = (byte)ScsiCommands.AdaptecWriteBuffer;

        LastError = SendScsiCommand(cdb, ref oneKBuffer, out senseBuffer, timeout, ScsiDirection.Out, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.ADAPTEC_WRITE_DATA_BUFFER_took_0_ms, duration);

        return sense;
    }

    /// <summary>Reads 1K bytes of data from the Adaptec controller RAM</summary>
    /// <param name="buffer">Buffer.</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool AdaptecReadBuffer(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
    {
        buffer = new byte[1024];
        var cdb = new byte[6];
        senseBuffer = new byte[64];

        cdb[0] = (byte)ScsiCommands.AdaptecReadBuffer;

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.ADAPTEC_READ_DATA_BUFFER_took_0_ms, duration);

        return sense;
    }
}