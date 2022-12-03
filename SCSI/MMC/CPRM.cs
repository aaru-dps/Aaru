// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CPRM.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes CPRM structures.
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
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Helpers;

namespace Aaru.Decoders.SCSI.MMC;

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
public static class CPRM
{
    public static CPRMMediaKeyBlock? DecodeCPRMMediaKeyBlock(byte[] CPRMMKBResponse)
    {
        if(CPRMMKBResponse == null)
            return null;

        var decoded = new CPRMMediaKeyBlock
        {
            MKBPackData = new byte[CPRMMKBResponse.Length - 4],
            DataLength  = BigEndianBitConverter.ToUInt16(CPRMMKBResponse, 0),
            Reserved    = CPRMMKBResponse[2],
            TotalPacks  = CPRMMKBResponse[3]
        };

        Array.Copy(CPRMMKBResponse, 4, decoded.MKBPackData, 0, CPRMMKBResponse.Length - 4);

        return decoded;
    }

    public static string PrettifyCPRMMediaKeyBlock(CPRMMediaKeyBlock? CPRMMKBResponse)
    {
        if(CPRMMKBResponse == null)
            return null;

        CPRMMediaKeyBlock response = CPRMMKBResponse.Value;

        var sb = new StringBuilder();

    #if DEBUG
        if(response.Reserved != 0)
            sb.AppendFormat(Localization.Reserved_equals_0_X2, response.Reserved).AppendLine();
    #endif
        sb.AppendFormat(Localization.Total_number_of_CPRM_Media_Key_Blocks_available_to_transfer_0,
                        response.TotalPacks).AppendLine();

        sb.AppendFormat(Localization.CPRM_Media_Key_Blocks_in_hex_follows);
        sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.MKBPackData, 80));

        return sb.ToString();
    }

    public static string PrettifyCPRMMediaKeyBlock(byte[] CPRMMKBResponse)
    {
        CPRMMediaKeyBlock? decoded = DecodeCPRMMediaKeyBlock(CPRMMKBResponse);

        return PrettifyCPRMMediaKeyBlock(decoded);
    }

    public struct CPRMMediaKeyBlock
    {
        /// <summary>Bytes 0 to 1 Data Length</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved;
        /// <summary>Byte 3 Number of MKB packs available to transfer</summary>
        public byte TotalPacks;
        /// <summary>Byte 4 MKB Packs</summary>
        public byte[] MKBPackData;
    }
}