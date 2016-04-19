// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : BCA.cs
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
using DiscImageChef.Console;
using System.Text;

namespace DiscImageChef.Decoders.Bluray
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
    public static class BCA
    {
        #region Public methods
        public static BurstCuttingArea? Decode(byte[] BCAResponse)
        {
            if(BCAResponse == null)
                return null;

            if(BCAResponse.Length != 68)
            {
                DicConsole.DebugWriteLine("BD BCA decoder", "Found incorrect Blu-ray BCA size ({0} bytes)", BCAResponse.Length);
                return null;
            }

            BurstCuttingArea decoded = new BurstCuttingArea();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.DataLength = BigEndianBitConverter.ToUInt16(BCAResponse, 0);
            decoded.Reserved1 = BCAResponse[2];
            decoded.Reserved2 = BCAResponse[3];
            decoded.BCA = new byte[64];
            Array.Copy(BCAResponse, 4, decoded.BCA, 0, 64);

            return decoded;
        }

        public static string Prettify(BurstCuttingArea? BCAResponse)
        {
            if(BCAResponse == null)
                return null;

            BurstCuttingArea response = BCAResponse.Value;

            StringBuilder sb = new StringBuilder();

#if DEBUG
            if(response.Reserved1 != 0)
                sb.AppendFormat("Reserved1 = 0x{0:X2}", response.Reserved1).AppendLine();
            if(response.Reserved2 != 0)
                sb.AppendFormat("Reserved2 = 0x{0:X2}", response.Reserved2).AppendLine();
#endif

            sb.AppendFormat("Blu-ray Burst Cutting Area in hex follows:");
            sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.BCA, 80));

            return sb.ToString();
        }

        public static string Prettify(byte[] BCAResponse)
        {
            return Prettify(Decode(BCAResponse));
        }
        #endregion Public methods

        #region Public structures
        public struct BurstCuttingArea
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Always 66
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Byte 4 to 67
            /// BCA data
            /// </summary>
            public byte[] BCA;
        }
        #endregion Public structures
    }
}

