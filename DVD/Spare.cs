// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Spare.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes DVD spare area information.
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
[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "NotAccessedField.Global")]
public static class Spare
{
    public static SpareAreaInformation? Decode(byte[] response)
    {
        if(response?.Length != 16)
            return null;

        return new SpareAreaInformation
        {
            DataLength          = (ushort)((response[0] << 8) + response[1]),
            Reserved1           = response[2],
            Reserved2           = response[3],
            UnusedPrimaryBlocks = (uint)((response[4] << 24) + (response[5] << 16) + (response[6] << 8) + response[7]),
            UnusedSupplementaryBlocks =
                (uint)((response[8] << 24) + (response[9] << 16) + (response[10] << 8) + response[11]),
            AllocatedSupplementaryBlocks =
                (uint)((response[12] << 24) + (response[13] << 16) + (response[14] << 8) + response[15])
        };
    }

    public static string Prettify(SpareAreaInformation? sai)
    {
        if(sai == null)
            return null;

        SpareAreaInformation decoded = sai.Value;
        var                  sb      = new StringBuilder();

        sb.AppendFormat("{0} unused primary spare blocks", decoded.UnusedPrimaryBlocks).AppendLine();
        sb.AppendFormat("{0} unused supplementary spare blocks", decoded.UnusedSupplementaryBlocks).AppendLine();

        sb.AppendFormat("{0} allocated supplementary spare blocks", decoded.AllocatedSupplementaryBlocks).AppendLine();

        return sb.ToString();
    }

    public static string Prettify(byte[] response) => Prettify(Decode(response));

    public struct SpareAreaInformation
    {
        /// <summary>Bytes 0 to 1 Data length</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Bytes 4 to 7 Data length</summary>
        public uint UnusedPrimaryBlocks;
        /// <summary>Bytes 8 to 11 Data length</summary>
        public uint UnusedSupplementaryBlocks;
        /// <summary>Bytes 12 to 15 Data length</summary>
        public uint AllocatedSupplementaryBlocks;
    }
}