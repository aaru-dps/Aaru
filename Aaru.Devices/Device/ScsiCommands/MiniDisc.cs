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
// Copyright © 2011-2020 Natalia Portillo
// ****************************************************************************/

using Aaru.Console;

namespace Aaru.Devices
{
    public sealed partial class Device
    {
        public bool MiniDiscReadDataTOC(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            ushort transferLength = 2336;
            senseBuffer = new byte[32];
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

        public bool MiniDiscReadUserTOC(out byte[] buffer, out byte[] senseBuffer, uint sector, uint timeout,
                                        out double duration)
        {
            ushort transferLength = 2336;
            senseBuffer = new byte[32];
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

        public bool MiniDiscD5(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            ushort transferLength = 4;
            senseBuffer = new byte[32];
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

        public bool MiniDiscStopPlaying(out byte[] buffer, out byte[] senseBuffer, uint sector, uint timeout,
                                        out double duration)
        {
            senseBuffer = new byte[32];
            byte[] cdb = new byte[10];

            cdb[0] = (byte)ScsiCommands.MiniDiscStopPlay;

            buffer = new byte[0];

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.None, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "MINIDISC STOP PLAY took {0} ms.", duration);

            return sense;
        }

        public bool MiniDiscReadPosition(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            ushort transferLength = 4;
            senseBuffer = new byte[32];
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

        public bool MiniDiscGetType(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            ushort transferLength = 8;
            senseBuffer = new byte[32];
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