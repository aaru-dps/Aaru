// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Plextor.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Plextor vendor commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains vendor commands for Plextor SCSI devices.
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Devices
{
    public sealed partial class Device
    {
        /// <summary>Sends the Plextor READ CD-DA command</summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the Plextor READ CD-DA response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="lba">Start block address.</param>
        /// <param name="transferLength">How many blocks to read.</param>
        /// <param name="blockSize">Block size.</param>
        /// <param name="subchannel">Subchannel selection.</param>
        public bool PlextorReadCdDa(out byte[] buffer, out byte[] senseBuffer, uint lba, uint blockSize,
                                    uint transferLength, PlextorSubchannel subchannel, uint timeout,
                                    out double duration)
        {
            senseBuffer = new byte[64];
            byte[] cdb = new byte[12];

            cdb[0]  = (byte)ScsiCommands.ReadCdDa;
            cdb[2]  = (byte)((lba & 0xFF000000) >> 24);
            cdb[3]  = (byte)((lba & 0xFF0000)   >> 16);
            cdb[4]  = (byte)((lba & 0xFF00)     >> 8);
            cdb[5]  = (byte)(lba & 0xFF);
            cdb[6]  = (byte)((transferLength & 0xFF000000) >> 24);
            cdb[7]  = (byte)((transferLength & 0xFF0000)   >> 16);
            cdb[8]  = (byte)((transferLength & 0xFF00)     >> 8);
            cdb[9]  = (byte)(transferLength & 0xFF);
            cdb[10] = (byte)subchannel;

            buffer = new byte[blockSize * transferLength];

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device",
                                       "Plextor READ CD-DA (LBA: {1}, Block Size: {2}, Transfer Length: {3}, Subchannel: {4}, Sense: {5}, Last Error: {6}) took {0} ms.",
                                       duration, lba, blockSize, transferLength, subchannel, sense, LastError);

            return sense;
        }

        /// <summary>Reads a "raw" sector from DVD on Plextor drives. Does it reading drive's cache.</summary>
        /// <returns><c>true</c> if the command failed and <paramref name="senseBuffer" /> contains the sense buffer.</returns>
        /// <param name="buffer">Buffer where the Plextor READ DVD (RAW) response will be stored</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout in seconds.</param>
        /// <param name="duration">Duration in milliseconds it took for the device to execute the command.</param>
        /// <param name="lba">Start block address.</param>
        /// <param name="transferLength">How many blocks to read.</param>
        public bool PlextorReadRawDvd(out byte[] buffer, out byte[] senseBuffer, uint lba, uint transferLength,
                                      uint timeout, out double duration)
        {
            senseBuffer = new byte[64];
            byte[] cdb = new byte[10];
            buffer = new byte[2064 * transferLength];

            cdb[0] = (byte)ScsiCommands.ReadBuffer;
            cdb[1] = 0x02;
            cdb[3] = (byte)((lba & 0xFF0000) >> 16);
            cdb[4] = (byte)((lba & 0xFF00)   >> 8);
            cdb[5] = (byte)(lba & 0xFF);
            cdb[3] = (byte)((buffer.Length & 0xFF0000) >> 16);
            cdb[4] = (byte)((buffer.Length & 0xFF00)   >> 8);
            cdb[5] = (byte)(buffer.Length & 0xFF);

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "Plextor READ DVD (RAW) took {0} ms.", duration);

            return sense;
        }

        /// <summary>Reads the statistics EEPROM from Plextor CD recorders</summary>
        /// <returns><c>true</c>, if EEPROM is correctly read, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorReadEepromCdr(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            buffer      = new byte[256];
            senseBuffer = new byte[64];
            byte[] cdb = new byte[12];

            cdb[0] = (byte)ScsiCommands.PlextorReadEeprom;
            cdb[8] = 1;

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "PLEXTOR READ EEPROM took {0} ms.", duration);

            return sense;
        }

        /// <summary>Reads the statistics EEPROM from Plextor PX-708 and PX-712 recorders</summary>
        /// <returns><c>true</c>, if EEPROM is correctly read, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorReadEeprom(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            buffer      = new byte[512];
            senseBuffer = new byte[64];
            byte[] cdb = new byte[12];

            cdb[0] = (byte)ScsiCommands.PlextorReadEeprom;
            cdb[8] = 2;

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "PLEXTOR READ EEPROM took {0} ms.", duration);

            return sense;
        }

        /// <summary>Reads a block from the statistics EEPROM from Plextor DVD recorders</summary>
        /// <returns><c>true</c>, if EEPROM is correctly read, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="block">EEPROM block to read</param>
        /// <param name="blockSize">How many bytes are in the EEPROM block</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorReadEepromBlock(out byte[] buffer, out byte[] senseBuffer, byte block, ushort blockSize,
                                           uint timeout, out double duration)
        {
            buffer      = new byte[blockSize];
            senseBuffer = new byte[64];
            byte[] cdb = new byte[12];

            cdb[0] = (byte)ScsiCommands.PlextorReadEeprom;
            cdb[1] = 1;
            cdb[7] = block;
            cdb[8] = (byte)((blockSize & 0xFF00) >> 8);
            cdb[9] = (byte)(blockSize & 0xFF);

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "PLEXTOR READ EEPROM took {0} ms.", duration);

            return sense;
        }

        /// <summary>Gets speeds set by Plextor PoweRec</summary>
        /// <returns><c>true</c>, if speeds were got correctly, <c>false</c> otherwise.</returns>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="selected">Selected write speed.</param>
        /// <param name="max">Max speed for currently inserted media.</param>
        /// <param name="last">Last actual speed.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetSpeeds(out byte[] senseBuffer, out ushort selected, out ushort max, out ushort last,
                                     uint timeout, out double duration)
        {
            byte[] buf = new byte[10];
            senseBuffer = new byte[64];
            byte[] cdb = new byte[12];

            selected = 0;
            max      = 0;
            last     = 0;

            cdb[0] = (byte)ScsiCommands.PlextorPoweRec;
            cdb[9] = (byte)buf.Length;

            LastError = SendScsiCommand(cdb, ref buf, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "PLEXTOR POWEREC GET SPEEDS took {0} ms.", duration);

            if(sense || Error)
                return sense;

            selected = BigEndianBitConverter.ToUInt16(buf, 4);
            max      = BigEndianBitConverter.ToUInt16(buf, 6);
            last     = BigEndianBitConverter.ToUInt16(buf, 8);

            return false;
        }

        /// <summary>Gets the Plextor PoweRec status</summary>
        /// <returns><c>true</c>, if PoweRec is supported, <c>false</c> otherwise.</returns>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="enabled">PoweRec is enabled.</param>
        /// <param name="speed">PoweRec recommended speed.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetPoweRec(out byte[] senseBuffer, out bool enabled, out ushort speed, uint timeout,
                                      out double duration)
        {
            byte[] buf = new byte[8];
            senseBuffer = new byte[64];
            byte[] cdb = new byte[12];

            enabled = false;
            speed   = 0;

            cdb[0] = (byte)ScsiCommands.PlextorExtend2;
            cdb[1] = (byte)PlextorSubCommands.GetMode;
            cdb[9] = (byte)buf.Length;

            LastError = SendScsiCommand(cdb, ref buf, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "PLEXTOR POWEREC GET SPEEDS took {0} ms.", duration);

            if(sense || Error)
                return sense;

            enabled = buf[2] != 0;
            speed   = BigEndianBitConverter.ToUInt16(buf, 4);

            return false;
        }

        /// <summary>Gets the Plextor SilentMode status</summary>
        /// <returns><c>true</c>, if SilentMode is supported, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetSilentMode(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            buffer      = new byte[8];
            senseBuffer = new byte[64];
            byte[] cdb = new byte[12];

            cdb[0]  = (byte)ScsiCommands.PlextorExtend;
            cdb[1]  = (byte)PlextorSubCommands.GetMode;
            cdb[2]  = (byte)PlextorSubCommands.Silent;
            cdb[3]  = 4;
            cdb[10] = (byte)buffer.Length;

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "PLEXTOR GET SILENT MODE took {0} ms.", duration);

            return sense;
        }

        /// <summary>Gets the Plextor GigaRec status</summary>
        /// <returns><c>true</c>, if GigaRec is supported, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetGigaRec(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            buffer      = new byte[8];
            senseBuffer = new byte[64];
            byte[] cdb = new byte[12];

            cdb[0]  = (byte)ScsiCommands.PlextorExtend;
            cdb[1]  = (byte)PlextorSubCommands.GetMode;
            cdb[2]  = (byte)PlextorSubCommands.GigaRec;
            cdb[10] = (byte)buffer.Length;

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "PLEXTOR GET GIGAREC took {0} ms.", duration);

            return sense;
        }

        /// <summary>Gets the Plextor VariRec status</summary>
        /// <returns><c>true</c>, if VariRec is supported, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="dvd">Set if request is for DVD.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetVariRec(out byte[] buffer, out byte[] senseBuffer, bool dvd, uint timeout,
                                      out double duration)
        {
            buffer      = new byte[8];
            senseBuffer = new byte[64];
            byte[] cdb = new byte[12];

            cdb[0]  = (byte)ScsiCommands.PlextorExtend;
            cdb[1]  = (byte)PlextorSubCommands.GetMode;
            cdb[2]  = (byte)PlextorSubCommands.VariRec;
            cdb[10] = (byte)buffer.Length;

            if(dvd)
                cdb[3] = 0x12;
            else
                cdb[3] = 0x02;

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "PLEXTOR GET VARIREC took {0} ms.", duration);

            return sense;
        }

        /// <summary>Gets the Plextor SecuRec status</summary>
        /// <returns><c>true</c>, if SecuRec is supported, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetSecuRec(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            buffer      = new byte[8];
            senseBuffer = new byte[64];
            byte[] cdb = new byte[12];

            cdb[0]  = (byte)ScsiCommands.PlextorExtend;
            cdb[2]  = (byte)PlextorSubCommands.SecuRec;
            cdb[10] = (byte)buffer.Length;

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "PLEXTOR GET SECUREC took {0} ms.", duration);

            return sense;
        }

        /// <summary>Gets the Plextor SpeedRead status</summary>
        /// <returns><c>true</c>, if SpeedRead is supported, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetSpeedRead(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            buffer      = new byte[8];
            senseBuffer = new byte[64];
            byte[] cdb = new byte[12];

            cdb[0]  = (byte)ScsiCommands.PlextorExtend;
            cdb[1]  = (byte)PlextorSubCommands.GetMode;
            cdb[2]  = (byte)PlextorSubCommands.SpeedRead;
            cdb[10] = (byte)buffer.Length;

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "PLEXTOR GET SPEEDREAD took {0} ms.", duration);

            return sense;
        }

        /// <summary>Gets the Plextor CD-R and multi-session hiding status</summary>
        /// <returns><c>true</c>, if CD-R and multi-session hiding is supported, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetHiding(out byte[] buffer, out byte[] senseBuffer, uint timeout, out double duration)
        {
            buffer      = new byte[8];
            senseBuffer = new byte[64];
            byte[] cdb = new byte[12];

            cdb[0] = (byte)ScsiCommands.PlextorExtend;
            cdb[1] = (byte)PlextorSubCommands.GetMode;
            cdb[2] = (byte)PlextorSubCommands.SessionHide;
            cdb[9] = (byte)buffer.Length;

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "PLEXTOR GET SINGLE-SESSION / HIDE CD-R took {0} ms.", duration);

            return sense;
        }

        /// <summary>Gets the Plextor DVD+ book bitsetting status</summary>
        /// <returns><c>true</c>, if DVD+ book bitsetting is supported, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="dualLayer">Set if bitset is for dual layer discs.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetBitsetting(out byte[] buffer, out byte[] senseBuffer, bool dualLayer, uint timeout,
                                         out double duration)
        {
            buffer      = new byte[8];
            senseBuffer = new byte[64];
            byte[] cdb = new byte[12];

            cdb[0] = (byte)ScsiCommands.PlextorExtend;
            cdb[1] = (byte)PlextorSubCommands.GetMode;
            cdb[2] = (byte)PlextorSubCommands.BitSet;
            cdb[9] = (byte)buffer.Length;

            if(dualLayer)
                cdb[3] = (byte)PlextorSubCommands.BitSetRdl;
            else
                cdb[3] = (byte)PlextorSubCommands.BitSetR;

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "PLEXTOR GET BOOK BITSETTING took {0} ms.", duration);

            return sense;
        }

        /// <summary>Gets the Plextor DVD+ test writing status</summary>
        /// <returns><c>true</c>, if DVD+ test writing is supported, <c>false</c> otherwise.</returns>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool PlextorGetTestWriteDvdPlus(out byte[] buffer, out byte[] senseBuffer, uint timeout,
                                               out double duration)
        {
            buffer      = new byte[8];
            senseBuffer = new byte[64];
            byte[] cdb = new byte[12];

            cdb[0]  = (byte)ScsiCommands.PlextorExtend;
            cdb[1]  = (byte)PlextorSubCommands.GetMode;
            cdb[2]  = (byte)PlextorSubCommands.TestWriteDvdPlus;
            cdb[10] = (byte)buffer.Length;

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "PLEXTOR GET TEST WRITE DVD+ took {0} ms.", duration);

            return sense;
        }
    }
}