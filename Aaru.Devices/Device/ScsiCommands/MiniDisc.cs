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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using Aaru.Console;
// ReSharper disable InconsistentNaming

namespace Aaru.Devices
{
    public sealed partial class Device
    {
        /// <summary>Reads the data TOC from an MD-DATA</summary>
        /// <param name="buffer">Buffer where the response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
        public bool MiniDiscReadDataTOC(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            ushort transferLength = 2336;
            senseBuffer = new byte[64];
            byte[] cdb = new byte[10];

            cdb[0] = (byte)ScsiCommands.MiniDiscReadDTOC;

            cdb[7] = (byte)((transferLength & 0xFF00) >> 8);
            cdb[8] = (byte)(transferLength & 0xFF);

            buffer = new byte[transferLength];

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "MINIDISC READ DTOC took {0} ms.", duration);

            return sense;
        }

        /// <summary>Reads the user TOC from an MD-DATA</summary>
        /// <param name="buffer">Buffer where the response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="sector">TOC sector to read</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
        public bool MiniDiscReadUserTOC(out byte[] buffer, out byte[] senseBuffer, uint sector, uint timeout,
                                        out double duration)
        {
            ushort transferLength = 2336;
            senseBuffer = new byte[64];
            byte[] cdb = new byte[10];

            cdb[0] = (byte)ScsiCommands.MiniDiscReadUTOC;

            cdb[2] = (byte)((sector & 0xFF000000) >> 24);
            cdb[3] = (byte)((sector & 0xFF0000)   >> 16);
            cdb[4] = (byte)((sector & 0xFF00)     >> 8);
            cdb[5] = (byte)(sector & 0xFF);
            cdb[7] = (byte)((transferLength & 0xFF00) >> 8);
            cdb[8] = (byte)(transferLength & 0xFF);

            buffer = new byte[transferLength];

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "MINIDISC READ UTOC took {0} ms.", duration);

            return sense;
        }

        /// <summary>Sends a D5h command to a MD-DATA drive (harmless)</summary>
        /// <param name="buffer">Buffer where the response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
        public bool MiniDiscD5(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            ushort transferLength = 4;
            senseBuffer = new byte[64];
            byte[] cdb = new byte[10];

            cdb[0] = (byte)ScsiCommands.MiniDiscD5;

            cdb[7] = (byte)((transferLength & 0xFF00) >> 8);
            cdb[8] = (byte)(transferLength & 0xFF);

            buffer = new byte[transferLength];

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "MINIDISC command D5h took {0} ms.", duration);

            return sense;
        }

        /// <summary>Stops playing MiniDisc audio from an MD-DATA drive</summary>
        /// <param name="buffer">Buffer where the response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
        public bool MiniDiscStopPlaying(out byte[] buffer, out byte[] senseBuffer, uint timeout,
                                        out double duration)
        {
            senseBuffer = new byte[64];
            byte[] cdb = new byte[10];

            cdb[0] = (byte)ScsiCommands.MiniDiscStopPlay;

            buffer = Array.Empty<byte>();

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.None, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "MINIDISC STOP PLAY took {0} ms.", duration);

            return sense;
        }

        /// <summary>Gets current position while playing MiniDisc audio from an MD-DATA drive</summary>
        /// <param name="buffer">Buffer where the response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
        public bool MiniDiscReadPosition(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            ushort transferLength = 4;
            senseBuffer = new byte[64];
            byte[] cdb = new byte[10];

            cdb[0] = (byte)ScsiCommands.MiniDiscReadPosition;

            cdb[7] = (byte)((transferLength & 0xFF00) >> 8);
            cdb[8] = (byte)(transferLength & 0xFF);

            buffer = new byte[transferLength];

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "MINIDISC READ POSITION took {0} ms.", duration);

            return sense;
        }

        /// <summary>Gets MiniDisc type from an MD-DATA drive</summary>
        /// <param name="buffer">Buffer where the response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
        public bool MiniDiscGetType(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            ushort transferLength = 8;
            senseBuffer = new byte[64];
            byte[] cdb = new byte[10];

            cdb[0] = (byte)ScsiCommands.MiniDiscGetType;

            cdb[7] = (byte)((transferLength & 0xFF00) >> 8);
            cdb[8] = (byte)(transferLength & 0xFF);

            buffer = new byte[transferLength];

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "MINIDISC GET TYPE took {0} ms.", duration);

            return sense;
        }
    }
}