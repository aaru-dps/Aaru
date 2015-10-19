// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CPRM.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using System.Text;

namespace DiscImageChef.Decoders.SCSI.MMC
{
    /// <summary>
    /// Information from the following standards:
    /// ANSI X3.304-1997
    /// T10/1048-D revision 9.0
    /// T10/1048-D revision 10a
    /// T10/1228-D revision 7.0c
    /// T10/1228-D revision 11a
    /// T10/1363-D revision 10g
    /// T10/1545-D revision 1d
    /// T10/1545-D revision 5
    /// T10/1545-D revision 5a
    /// T10/1675-D revision 2c
    /// T10/1675-D revision 4
    /// T10/1836-D revision 2g
    /// </summary>
    public static class CPRM
    {
        public struct CPRMMediaKeyBlock
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data Length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved;
            /// <summary>
            /// Byte 3
            /// Number of MKB packs available to transfer
            /// </summary>
            public byte TotalPacks;
            /// <summary>
            /// Byte 4
            /// MKB Packs
            /// </summary>
            public byte[] MKBPackData;
        }

        public static CPRMMediaKeyBlock? DecodeCPRMMediaKeyBlock(byte[] CPRMMKBResponse)
        {
            if (CPRMMKBResponse == null)
                return null;

            CPRMMediaKeyBlock decoded = new CPRMMediaKeyBlock();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.MKBPackData = new byte[CPRMMKBResponse.Length - 4];

            decoded.DataLength = BigEndianBitConverter.ToUInt16(CPRMMKBResponse, 0);
            decoded.Reserved = CPRMMKBResponse[2];
            decoded.TotalPacks = CPRMMKBResponse[3];
            Array.Copy(CPRMMKBResponse, 4, decoded.MKBPackData, 0, CPRMMKBResponse.Length - 4);

            return decoded;
        }

        public static string PrettifyCPRMMediaKeyBlock(CPRMMediaKeyBlock? CPRMMKBResponse)
        {
            if (CPRMMKBResponse == null)
                return null;

            CPRMMediaKeyBlock response = CPRMMKBResponse.Value;

            StringBuilder sb = new StringBuilder();

            #if DEBUG
            if(response.Reserved != 0)
                sb.AppendFormat("Reserved = 0x{0:X2}", response.Reserved).AppendLine();
            #endif
            sb.AppendFormat("Total number of CPRM Media Key Blocks available to transfer: {0}", response.TotalPacks).AppendLine();
            sb.AppendFormat("CPRM Media Key Blocks in hex follows:");
            sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.MKBPackData, 80));

            return sb.ToString();
        }

        public static string PrettifyCPRMMediaKeyBlock(byte[] CPRMMKBResponse)
        {
            CPRMMediaKeyBlock? decoded = DecodeCPRMMediaKeyBlock(CPRMMKBResponse);
            return PrettifyCPRMMediaKeyBlock(decoded);
        }
    }
}

