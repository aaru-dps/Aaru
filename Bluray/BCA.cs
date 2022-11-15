// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : BCA.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes Blu-ray Burst Cutting Area.
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Decoders.Bluray;

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
public static class BCA
{
    #region Public structures
    public struct BurstCuttingArea
    {
        /// <summary>Bytes 0 to 1 Always 66</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Byte 4 to 67 BCA data</summary>
        public byte[] BCA;
    }
    #endregion Public structures
    #region Public methods
    public static BurstCuttingArea? Decode(byte[] BCAResponse)
    {
        if(BCAResponse == null)
            return null;

        if(BCAResponse.Length != 68)
        {
            AaruConsole.DebugWriteLine("BD BCA decoder", "Found incorrect Blu-ray BCA size ({0} bytes)",
                                       BCAResponse.Length);

            return null;
        }

        var decoded = new BurstCuttingArea
        {
            DataLength = BigEndianBitConverter.ToUInt16(BCAResponse, 0),
            Reserved1  = BCAResponse[2],
            Reserved2  = BCAResponse[3],
            BCA        = new byte[64]
        };

        Array.Copy(BCAResponse, 4, decoded.BCA, 0, 64);

        return decoded;
    }

    public static string Prettify(BurstCuttingArea? BCAResponse)
    {
        if(BCAResponse == null)
            return null;

        BurstCuttingArea response = BCAResponse.Value;

        var sb = new StringBuilder();

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

    public static string Prettify(byte[] BCAResponse) => Prettify(Decode(BCAResponse));
    #endregion Public methods
}