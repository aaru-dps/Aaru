// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : WriteProtect.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MMC write protection structures.
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

namespace Aaru.Decoders.SCSI.MMC;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
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
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "NotAccessedField.Global")]
public static class WriteProtect
{
    public static WriteProtectionStatus? DecodeWriteProtectionStatus(byte[] WPSResponse)
    {
        if(WPSResponse == null)
            return null;

        var decoded = new WriteProtectionStatus
        {
            DataLength = BigEndianBitConverter.ToUInt16(WPSResponse, 0),
            Reserved1  = WPSResponse[2],
            Reserved2  = WPSResponse[3],
            Reserved3  = (byte)((WPSResponse[4] & 0xF0) >> 4),
            MSWI       = Convert.ToBoolean(WPSResponse[4] & 0x08),
            CWP        = Convert.ToBoolean(WPSResponse[4] & 0x04),
            PWP        = Convert.ToBoolean(WPSResponse[4] & 0x02),
            SWPP       = Convert.ToBoolean(WPSResponse[4] & 0x01),
            Reserved4  = WPSResponse[5],
            Reserved5  = WPSResponse[6],
            Reserved6  = WPSResponse[7]
        };

        return decoded;
    }

    public static string PrettifyWriteProtectionStatus(WriteProtectionStatus? WPSResponse)
    {
        if(WPSResponse == null)
            return null;

        WriteProtectionStatus response = WPSResponse.Value;

        var sb = new StringBuilder();

        if(response.MSWI)
            sb.AppendLine("Writing inhibited by media specific reason");

        if(response.CWP)
            sb.AppendLine("Cartridge sets write protection");

        if(response.PWP)
            sb.AppendLine("Media surface sets write protection");

        if(response.SWPP)
            sb.AppendLine("Software write protection is set until power down");

    #if DEBUG
        if(response.Reserved1 != 0)
            sb.AppendFormat("Reserved1 = 0x{0:X2}", response.Reserved1).AppendLine();

        if(response.Reserved2 != 0)
            sb.AppendFormat("Reserved2 = 0x{0:X2}", response.Reserved2).AppendLine();

        if(response.Reserved3 != 0)
            sb.AppendFormat("Reserved3 = 0x{0:X2}", response.Reserved3).AppendLine();

        if(response.Reserved4 != 0)
            sb.AppendFormat("Reserved4 = 0x{0:X2}", response.Reserved4).AppendLine();

        if(response.Reserved5 != 0)
            sb.AppendFormat("Reserved5 = 0x{0:X2}", response.Reserved5).AppendLine();

        if(response.Reserved6 != 0)
            sb.AppendFormat("Reserved6 = 0x{0:X2}", response.Reserved6).AppendLine();
    #endif

        return sb.ToString();
    }

    public static string PrettifyWriteProtectionStatus(byte[] WPSResponse)
    {
        WriteProtectionStatus? decoded = DecodeWriteProtectionStatus(WPSResponse);

        return PrettifyWriteProtectionStatus(decoded);
    }

    public struct WriteProtectionStatus
    {
        /// <summary>Bytes 0 to 1 Data Length</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Byte 4, bits 7 to 4 Reserved</summary>
        public byte Reserved3;
        /// <summary>Byte 4, bit 3 Writing inhibited by media specific reason</summary>
        public bool MSWI;
        /// <summary>Byte 4, bit 2 Cartridge sets write protection</summary>
        public bool CWP;
        /// <summary>Byte 4, bit 1 Media surface sets write protection</summary>
        public bool PWP;
        /// <summary>Byte 4, bit 0 Software write protection until power down</summary>
        public bool SWPP;
        /// <summary>Byte 5 Reserved</summary>
        public byte Reserved4;
        /// <summary>Byte 6 Reserved</summary>
        public byte Reserved5;
        /// <summary>Byte 7 Reserved</summary>
        public byte Reserved6;
    }
}