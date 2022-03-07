// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Cartridge.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes Blu-ray cartridge structures.
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

namespace Aaru.Decoders.Bluray;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Console;
using Aaru.Helpers;

// Information from the following standards:
// ANSI X3.304-1997
// T10/1048-D revision 9.0
// T10/1048-D revision 10a
// T10/1228-D revision 7.0c
// T10/1228-D revision 11a
// T10/1363-D revision 10g
// T10/1545-D revision 1d
// T10/1545-D revision 5
// T10/1545-D revision 5a
// T10/1675-D revision 2c
// T10/1675-D revision 4
// T10/1836-D revision 2g
[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "NotAccessedField.Global"),
 SuppressMessage("ReSharper", "UnassignedField.Global")]
public static class Cartridge
{
    #region Public structures
    public struct CartridgeStatus
    {
        /// <summary>Bytes 0 to 1 Always 6</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Byte 4, bit 7 Medium is inserted in a cartridge</summary>
        public bool Cartridge;
        /// <summary>Byte 4, bit 6 Medium taken out / put in a cartridge</summary>
        public bool OUT;
        /// <summary>Byte 4, bits 5 to 3 Reserved</summary>
        public byte Reserved3;
        /// <summary>Byte 4, bit 2 Cartridge sets write protection</summary>
        public bool CWP;
        /// <summary>Byte 4, bits 1 to 0 Reserved</summary>
        public byte Reserved4;
        /// <summary>Byte 5 Reserved</summary>
        public byte Reserved5;
        /// <summary>Byte 6 Reserved</summary>
        public byte Reserved6;
        /// <summary>Byte 7 Reserved</summary>
        public byte Reserved7;
    }
    #endregion Public structures
    #region Public methods
    public static CartridgeStatus? Decode(byte[] CSResponse)
    {
        if(CSResponse == null)
            return null;

        if(CSResponse.Length != 8)
        {
            AaruConsole.DebugWriteLine("BD Cartridge Status decoder",
                                       "Found incorrect Blu-ray Cartridge Status size ({0} bytes)", CSResponse.Length);

            return null;
        }

        var decoded = new CartridgeStatus
        {
            DataLength = BigEndianBitConverter.ToUInt16(CSResponse, 0),
            Reserved1  = CSResponse[2],
            Reserved2  = CSResponse[3],
            Cartridge  = Convert.ToBoolean(CSResponse[4] & 0x80),
            OUT        = Convert.ToBoolean(CSResponse[4] & 0x40),
            Reserved3  = (byte)((CSResponse[4] & 0x38) >> 3),
            CWP        = Convert.ToBoolean(CSResponse[4] & 0x04),
            Reserved4  = (byte)(CSResponse[4] & 0x03),
            Reserved5  = CSResponse[5],
            Reserved6  = CSResponse[6],
            Reserved7  = CSResponse[7]
        };

        return decoded;
    }

    public static string Prettify(CartridgeStatus? CSResponse)
    {
        if(CSResponse == null)
            return null;

        CartridgeStatus response = CSResponse.Value;

        var sb = new StringBuilder();

    #if DEBUG
        if(response.Reserved1 != 0)
            sb.AppendFormat("Reserved1 = 0x{0:X2}", response.Reserved1).AppendLine();

        if(response.Reserved2 != 0)
            sb.AppendFormat("Reserved2 = 0x{0:X2}", response.Reserved2).AppendLine();

        if(response.Reserved3 != 0)
            sb.AppendFormat("Reserved3 = 0x{0:X8}", response.Reserved3).AppendLine();

        if(response.Reserved4 != 0)
            sb.AppendFormat("Reserved4 = 0x{0:X8}", response.Reserved4).AppendLine();

        if(response.Reserved5 != 0)
            sb.AppendFormat("Reserved5 = 0x{0:X8}", response.Reserved5).AppendLine();

        if(response.Reserved6 != 0)
            sb.AppendFormat("Reserved6 = 0x{0:X8}", response.Reserved6).AppendLine();

        if(response.Reserved7 != 0)
            sb.AppendFormat("Reserved7 = 0x{0:X8}", response.Reserved7).AppendLine();
    #endif

        if(response.Cartridge)
        {
            sb.AppendLine("Media is inserted in a cartridge");

            if(response.OUT)
                sb.AppendLine("Media has been taken out, or inserted in, the cartridge");

            if(response.CWP)
                sb.AppendLine("Media is write protected");
        }
        else
        {
            sb.AppendLine("Media is not in a cartridge");

        #if DEBUG
            if(response.OUT)
                sb.AppendLine("Media has out bit marked, shouldn't");

            if(response.CWP)
                sb.AppendLine("Media has write protection bit marked, shouldn't");
        #endif
        }

        return sb.ToString();
    }

    public static string Prettify(byte[] CSResponse) => Prettify(Decode(CSResponse));
    #endregion Public methods
}