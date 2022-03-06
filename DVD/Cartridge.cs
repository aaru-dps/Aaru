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
//     Decodes DVD cartridge structures.
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

using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Aaru.Decoders.DVD;

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
// ECMA 365
[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "NotAccessedField.Global")]
public static class Cartridge
{
    public static MediumStatus? Decode(byte[] response)
    {
        if(response?.Length != 8)
            return null;

        return new MediumStatus
        {
            DataLength = (ushort)((response[0] << 8) + response[1]),
            Reserved1  = response[2],
            Reserved2  = response[3],
            Cartridge  = (response[4] & 0x80) == 0x80,
            OUT        = (response[4] & 0x40) == 0x40,
            Reserved3  = (byte)((response[4] & 0x30) >> 4),
            MSWI       = (response[4] & 0x08) == 0x08,
            CWP        = (response[4] & 0x04) == 0x04,
            PWP        = (response[4] & 0x02) == 0x02,
            Reserved4  = (response[4] & 0x01) == 0x01,
            DiscType   = response[5],
            Reserved5  = response[6],
            RAMSWI     = response[7]
        };
    }

    public static string Prettify(MediumStatus? status)
    {
        if(status == null)
            return null;

        MediumStatus decoded = status.Value;
        var          sb      = new StringBuilder();

        if(decoded.PWP)
            sb.AppendLine("Disc surface is set to write protected status");

        if(decoded.Cartridge)
        {
            sb.AppendLine("Disc comes in a cartridge");

            if(decoded.OUT)
                sb.AppendLine("Disc has been extracted from the cartridge");

            if(decoded.CWP)
                sb.AppendLine("Cartridge is set to write protected");
        }

        switch(decoded.DiscType)
        {
            case 0:
                sb.AppendLine("Disc shall not be written without a cartridge");

                break;
            case 0x10:
                sb.AppendLine("Disc may be written without a cartridge");

                break;
            default:
                sb.AppendFormat("Unknown disc type id {0}", decoded.DiscType).AppendLine();

                break;
        }

        if(!decoded.MSWI)
            return sb.ToString();

        switch(decoded.RAMSWI)
        {
            case 0: break;
            case 1:
                sb.AppendLine("Disc is write inhibited because it has been extracted from the cartridge");

                break;
            case 0xFF:
                sb.AppendLine("Disc is write inhibited for an unspecified reason");

                break;
            default:
                sb.AppendFormat("Disc has unknown reason {0} for write inhibition", decoded.RAMSWI).AppendLine();

                break;
        }

        return sb.ToString();
    }

    public static string Prettify(byte[] response) => Prettify(Decode(response));

    public struct MediumStatus
    {
        /// <summary>Bytes 0 to 1 Data length</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Byte 4, bit 7 Medium is in a cartridge</summary>
        public bool Cartridge;
        /// <summary>Byte 4, bit 6 Medium has been taken out/inserted in a cartridge</summary>
        public bool OUT;
        /// <summary>Byte 4, bits 5 to 4 Reserved</summary>
        public byte Reserved3;
        /// <summary>Byte 4, bit 3 Media is write protected by reason stablished in RAMSWI</summary>
        public bool MSWI;
        /// <summary>Byte 4, bit 2 Media is write protected by cartridge</summary>
        public bool CWP;
        /// <summary>Byte 4, bit 1 Media is persistently write protected</summary>
        public bool PWP;
        /// <summary>Byte 4, bit 0 Reserved</summary>
        public bool Reserved4;
        /// <summary>Byte 5 Writable status depending on cartridge</summary>
        public byte DiscType;
        /// <summary>Byte 6 Reserved</summary>
        public byte Reserved5;
        /// <summary>
        ///     Byte 7 Reason of specific write protection, only defined 0x01 as "bare disc wp", and 0xFF as unspecified. Rest
        ///     reserved.
        /// </summary>
        public byte RAMSWI;
    }
}