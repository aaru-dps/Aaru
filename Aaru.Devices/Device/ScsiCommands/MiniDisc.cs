// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : MiniDisc.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : MiniDisc vendor commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains vendor commands for MiniDisc drives.
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
// Copyright © 2011-2026 Natalia Portillo
// ****************************************************************************/

// ReSharper disable InconsistentNaming

using System;
using Aaru.Logging;

// ReSharper disable UnusedMember.Global

namespace Aaru.Devices;

public partial class Device
{
    /// <summary>
    ///     Reads a sector of the User TOC (U-TOC) from a recordable MiniDisc (vendor command D1h). On MD-DATA discs it
    ///     describes the system and data tracks; it is not available on blank or premastered discs. Byte 13h of sector 0
    ///     is a bitmap of the sectors in use.
    /// </summary>
    /// <param name="buffer">Buffer where the response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="sector">U-TOC sector to read, 0 to 7</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool MiniDiscReadDataTOC(out byte[] buffer, out ReadOnlySpan<byte> senseBuffer, uint sector, uint timeout,
                                    out double duration)
    {
        const ushort transferLength = 2336;
        senseBuffer = SenseBuffer;
        Span<byte> cdb = CdbBuffer[..10];
        cdb.Clear();

        cdb[0] = (byte)ScsiCommands.MiniDiscReadDTOC;

        cdb[2] = (byte)((sector & 0xFF000000) >> 24);
        cdb[3] = (byte)((sector & 0xFF0000)   >> 16);
        cdb[4] = (byte)((sector & 0xFF00)     >> 8);
        cdb[5] = (byte)(sector   & 0xFF);
        cdb[7] = (transferLength & 0xFF00) >> 8;
        cdb[8] = transferLength & 0xFF;

        buffer = new byte[transferLength];

        LastError = SendScsiCommand(cdb, ref buffer, timeout, ScsiDirection.In, out duration, out bool sense);

        Error = LastError != 0;

        AaruLogging.Debug(SCSI_MODULE_NAME, Localization.MINIDISC_READ_DTOC_took_0_ms, duration);

        return sense;
    }

    /// <summary>
    ///     Reads a sector of the Premastered TOC (P-TOC) from a MiniDisc (vendor command D4h). Sector 0 starts with the
    ///     "MINI" (audio) or "MINX" (MD-DATA) signature at offset 8, and its byte 13h is a bitmap of the sectors in use;
    ///     recordable discs only have sector 0, premastered discs up to 4.
    /// </summary>
    /// <param name="buffer">Buffer where the response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="sector">P-TOC sector to read</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool MiniDiscReadUserTOC(out byte[] buffer, out ReadOnlySpan<byte> senseBuffer, uint sector, uint timeout,
                                    out double duration)
    {
        const ushort transferLength = 2336;
        senseBuffer = SenseBuffer;
        Span<byte> cdb = CdbBuffer[..10];
        cdb.Clear();

        cdb[0] = (byte)ScsiCommands.MiniDiscReadUTOC;

        cdb[2] = (byte)((sector & 0xFF000000) >> 24);
        cdb[3] = (byte)((sector & 0xFF0000)   >> 16);
        cdb[4] = (byte)((sector & 0xFF00)     >> 8);
        cdb[5] = (byte)(sector   & 0xFF);
        cdb[7] = (transferLength & 0xFF00) >> 8;
        cdb[8] = transferLength & 0xFF;

        buffer = new byte[transferLength];

        LastError = SendScsiCommand(cdb, ref buffer, timeout, ScsiDirection.In, out duration, out bool sense);

        Error = LastError != 0;

        AaruLogging.Debug(SCSI_MODULE_NAME, Localization.MINIDISC_READ_UTOC_took_0_ms, duration);

        return sense;
    }

    /// <summary>Sends a D5h command to a MD-DATA drive (harmless)</summary>
    /// <param name="buffer">Buffer where the response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool MiniDiscD5(out byte[] buffer, out ReadOnlySpan<byte> senseBuffer, uint timeout, out double duration)
    {
        const ushort transferLength = 4;
        senseBuffer = SenseBuffer;
        Span<byte> cdb = CdbBuffer[..10];
        cdb.Clear();

        cdb[0] = (byte)ScsiCommands.MiniDiscD5;

        cdb[7] = 0;
        cdb[8] = transferLength & 0xFF;

        buffer = new byte[transferLength];

        LastError = SendScsiCommand(cdb, ref buffer, timeout, ScsiDirection.In, out duration, out bool sense);

        Error = LastError != 0;

        AaruLogging.Debug(SCSI_MODULE_NAME, Localization.MINIDISC_command_D5h_took_0_ms, duration);

        return sense;
    }

    /// <summary>Stops playing MiniDisc audio from an MD-DATA drive</summary>
    /// <param name="buffer">Buffer where the response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool MiniDiscStopPlaying(out byte[] buffer, out ReadOnlySpan<byte> senseBuffer, uint timeout,
                                    out double duration)
    {
        senseBuffer = SenseBuffer;
        Span<byte> cdb = CdbBuffer[..10];
        cdb.Clear();

        cdb[0] = (byte)ScsiCommands.MiniDiscStopPlay;

        buffer = [];

        LastError = SendScsiCommand(cdb, ref buffer, timeout, ScsiDirection.None, out duration, out bool sense);

        Error = LastError != 0;

        AaruLogging.Debug(SCSI_MODULE_NAME, Localization.MINIDISC_STOP_PLAY_took_0_ms, duration);

        return sense;
    }

    /// <summary>Gets current position while playing MiniDisc audio from an MD-DATA drive</summary>
    /// <param name="buffer">Buffer where the response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool MiniDiscReadPosition(out byte[] buffer, out ReadOnlySpan<byte> senseBuffer, uint timeout,
                                     out double duration)
    {
        const ushort transferLength = 4;
        senseBuffer = SenseBuffer;
        Span<byte> cdb = CdbBuffer[..10];
        cdb.Clear();

        cdb[0] = (byte)ScsiCommands.MiniDiscReadPosition;

        cdb[7] = 0;
        cdb[8] = transferLength & 0xFF;

        buffer = new byte[transferLength];

        LastError = SendScsiCommand(cdb, ref buffer, timeout, ScsiDirection.In, out duration, out bool sense);

        Error = LastError != 0;

        AaruLogging.Debug(SCSI_MODULE_NAME, Localization.MINIDISC_READ_POSITION_took_0_ms, duration);

        return sense;
    }

    /// <summary>Gets MiniDisc type from an MD-DATA drive</summary>
    /// <param name="buffer">Buffer where the response will be stored</param>
    /// <param name="senseBuffer">Sense buffer.</param>
    /// <param name="timeout">Timeout in seconds.</param>
    /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
    /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
    public bool MiniDiscGetType(out byte[] buffer, out ReadOnlySpan<byte> senseBuffer, uint timeout,
                                out double duration)
    {
        const ushort transferLength = 8;
        senseBuffer = SenseBuffer;
        Span<byte> cdb = CdbBuffer[..10];
        cdb.Clear();

        cdb[0] = (byte)ScsiCommands.MiniDiscGetType;

        // As sent by Sony's Windows driver, the response is 01h and the first sector of the data track on MD-DATA discs,
        // FFh otherwise
        cdb[2] = 0x01;
        cdb[7] = 0;
        cdb[8] = transferLength & 0xFF;

        buffer = new byte[transferLength];

        LastError = SendScsiCommand(cdb, ref buffer, timeout, ScsiDirection.In, out duration, out bool sense);

        Error = LastError != 0;

        AaruLogging.Debug(SCSI_MODULE_NAME, Localization.MINIDISC_GET_TYPE_took_0_ms, duration);

        return sense;
    }
}