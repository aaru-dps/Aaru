// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SMC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : SCSI Media-changer Commands.
//
// --[ Description ] ----------------------------------------------------------
//
//     Contains SCSI commands defined in SMC standards.
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

namespace Aaru.Devices
{
    public sealed partial class Device
    {
        /// <summary>Reads an attribute from the medium auxiliary memory, or reports which elements in the changer contain one</summary>
        /// <param name="buffer">Buffer.</param>
        /// <param name="senseBuffer">Sense buffer.</param>
        /// <param name="action">What to do, <see cref="ScsiAttributeAction" />.</param>
        /// <param name="element">Element address.</param>
        /// <param name="elementType">Element type.</param>
        /// <param name="volume">Volume number.</param>
        /// <param name="partition">Partition number.</param>
        /// <param name="firstAttribute">First attribute identificator.</param>
        /// <param name="cache">If set to <c>true</c> device can return cached data.</param>
        /// <param name="timeout">Timeout.</param>
        /// <param name="duration">Duration.</param>
        public bool ReadAttribute(out byte[] buffer, out byte[] senseBuffer, ScsiAttributeAction action, ushort element,
                                  byte elementType, byte volume, byte partition, ushort firstAttribute, bool cache,
                                  uint timeout, out double duration)
        {
            buffer = new byte[256];
            byte[] cdb = new byte[16];
            senseBuffer = new byte[64];

            cdb[0]  = (byte)ScsiCommands.ReadAttribute;
            cdb[1]  = (byte)((byte)action & 0x1F);
            cdb[2]  = (byte)((element & 0xFF00) >> 8);
            cdb[3]  = (byte)(element     & 0xFF);
            cdb[4]  = (byte)(elementType & 0x0F);
            cdb[5]  = volume;
            cdb[7]  = partition;
            cdb[8]  = (byte)((firstAttribute & 0xFF00) >> 8);
            cdb[9]  = (byte)(firstAttribute & 0xFF);
            cdb[10] = (byte)((buffer.Length & 0xFF000000) >> 24);
            cdb[11] = (byte)((buffer.Length & 0xFF0000)   >> 16);
            cdb[12] = (byte)((buffer.Length & 0xFF00)     >> 8);
            cdb[13] = (byte)(buffer.Length & 0xFF);

            if(cache)
                cdb[14] += 0x01;

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out bool sense);

            Error = LastError != 0;

            if(sense)
                return true;

            uint attrLen = (uint)((buffer[0] << 24) + (buffer[1] << 16) + (buffer[2] << 8) + buffer[3] + 4);
            buffer      = new byte[attrLen];
            cdb[10]     = (byte)((buffer.Length & 0xFF000000) >> 24);
            cdb[11]     = (byte)((buffer.Length & 0xFF0000)   >> 16);
            cdb[12]     = (byte)((buffer.Length & 0xFF00)     >> 8);
            cdb[13]     = (byte)(buffer.Length & 0xFF);
            senseBuffer = new byte[64];

            LastError = SendScsiCommand(cdb, ref buffer, out senseBuffer, timeout, ScsiDirection.In, out duration,
                                        out sense);

            Error = LastError != 0;

            AaruConsole.DebugWriteLine("SCSI Device", "READ ATTRIBUTE took {0} ms.", duration);

            return sense;
        }
    }
}