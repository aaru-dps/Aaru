// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ArchiveCorp.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Archive Corp. vendor commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains vendor commands for Archive Corp. SCSI devices.
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
    /// <param name="lba">Address.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ArchiveCorpRequestBlockAddress(out byte[] buffer, out byte[] senseBuffer, uint lba, uint timeout,
                                               out double duration)
    {
        buffer = new byte[3];
        byte[] cdb = new byte[6];
        senseBuffer = new byte[64];

        cdb[0] = (byte)ScsiCommands.ArchiveRequestBlockAddress;
        cdb[1] = (byte)((lba & 0x1F0000) >> 16);
        cdb[2] = (byte)((lba & 0xFF00)   >> 8);
        cdb[3] = (byte)(lba & 0xFF);
        cdb[4] = 3;

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.ARCHIVE_CORP_REQUEST_BLOCK_ADDRESS_took_0_ms,
                                   duration);

        return sense;
    }

    /// <summary>Gets the underlying drive cylinder, head and index bytes for the specified SCSI LBA.</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="lba">Logical Block Address, starting from 1.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ArchiveCorpSeekBlock(out byte[] senseBuffer, uint lba, uint timeout, out double duration) =>
        ArchiveCorpSeekBlock(out senseBuffer, false, lba, timeout, out duration);

    /// <summary>Positions the tape at the specified block address</summary>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="immediate">If set to <c>true</c>, return from the command immediately.</param>
    /// <param name="lba">Logical Block Address, starting from 1.</param>
    /// <param name="timeout">Timeout.</param>
    /// <param name="duration">Duration.</param>
    public bool ArchiveCorpSeekBlock(out byte[] senseBuffer, bool immediate, uint lba, uint timeout,
                                     out double duration)
    {
        byte[] buffer = Array.Empty<byte>();
        byte[] cdb    = new byte[6];
        senseBuffer = new byte[64];

        cdb[0] = (byte)ScsiCommands.ArchiveSeekBlock;
        cdb[1] = (byte)((lba & 0x1F0000) >> 16);
        cdb[2] = (byte)((lba & 0xFF00)   >> 8);
        cdb[3] = (byte)(lba & 0xFF);

        if(immediate)
            cdb[1] += 0x01;

        LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.None, out duration,
                                    out bool sense);

        Error = LastError != 0;

        AaruConsole.DebugWriteLine(SCSI_MODULE_NAME, Localization.ARCHIVE_CORP_SEEK_BLOCK_took_0_ms, duration);

        return sense;
    }
}