// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CSS&CPRM.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes DVD CSS & CPRM structures.
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
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace DiscImageChef.Decoders.DVD
{
    /// <summary>
    ///     Information from the following standards: ANSI X3.304-1997 T10/1048-D revision 9.0 T10/1048-D revision 10a
    ///     T10/1228-D revision 7.0c T10/1228-D revision 11a T10/1363-D revision 10g T10/1545-D revision 1d T10/1545-D revision
    ///     5 T10/1545-D revision 5a T10/1675-D revision 2c T10/1675-D revision 4 T10/1836-D revision 2g ECMA 365
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "NotAccessedField.Global")]
    public static class CSS_CPRM
    {
        public static LeadInCopyright? DecodeLeadInCopyright(byte[] response)
        {
            if(response?.Length != 8)
                return null;

            return new LeadInCopyright
            {
                DataLength        = (ushort)((response[0] << 8) + response[1]), Reserved1 = response[2],
                Reserved2         = response[3], CopyrightType                            = (CopyrightType)response[4],
                RegionInformation = response[5],
                Reserved3         = response[6], Reserved4 = response[7]
            };
        }

        public static string PrettifyLeadInCopyright(LeadInCopyright? cmi)
        {
            if(cmi == null)
                return null;

            LeadInCopyright decoded = cmi.Value;
            var             sb      = new StringBuilder();

            switch(decoded.CopyrightType)
            {
                case CopyrightType.NoProtection:
                    sb.AppendLine("Disc has no encryption.");

                    break;
                case CopyrightType.CSS:
                    sb.AppendLine("Disc is encrypted using CSS or CPPM.");

                    break;
                case CopyrightType.CPRM:
                    sb.AppendLine("Disc is encrypted using CPRM.");

                    break;
                case CopyrightType.AACS:
                    sb.AppendLine("Disc is encrypted using AACS.");

                    break;
                default:
                    sb.AppendFormat("Disc is encrypted using unknown algorithm with ID {0}.", decoded.CopyrightType);

                    break;
            }

            if(decoded.CopyrightType == 0)
                return sb.ToString();

            if(decoded.RegionInformation == 0xFF)
                sb.AppendLine("Disc cannot be played in any region at all.");
            else if(decoded.RegionInformation == 0x00)
                sb.AppendLine("Disc can be played in any region.");
            else
            {
                sb.Append("Disc can be played in the following regions:");

                if((decoded.RegionInformation & 0x01) != 0x01)
                    sb.Append(" 0");

                if((decoded.RegionInformation & 0x02) != 0x02)
                    sb.Append(" 1");

                if((decoded.RegionInformation & 0x04) != 0x04)
                    sb.Append(" 2");

                if((decoded.RegionInformation & 0x08) != 0x08)
                    sb.Append(" 3");

                if((decoded.RegionInformation & 0x10) != 0x10)
                    sb.Append(" 4");

                if((decoded.RegionInformation & 0x20) != 0x20)
                    sb.Append(" 5");

                if((decoded.RegionInformation & 0x40) != 0x40)
                    sb.Append(" 6");

                if((decoded.RegionInformation & 0x80) != 0x80)
                    sb.Append(" 7");
            }

            return sb.ToString();
        }

        public static string PrettifyLeadInCopyright(byte[] response) =>
            PrettifyLeadInCopyright(DecodeLeadInCopyright(response));

        public struct LeadInCopyright
        {
            /// <summary>Bytes 0 to 1 Data length</summary>
            public ushort DataLength;
            /// <summary>Byte 2 Reserved</summary>
            public byte Reserved1;
            /// <summary>Byte 3 Reserved</summary>
            public byte Reserved2;
            /// <summary>Byte 4 Copy protection system type</summary>
            public CopyrightType CopyrightType;
            /// <summary>Byte 5 Bitmask of regions where this disc is playable</summary>
            public byte RegionInformation;
            /// <summary>Byte 6 Reserved</summary>
            public byte Reserved3;
            /// <summary>Byte 7 Reserved</summary>
            public byte Reserved4;
        }

        public struct DiscKey
        {
            /// <summary>Bytes 0 to 1 Data length</summary>
            public ushort DataLength;
            /// <summary>Byte 2 Reserved</summary>
            public byte Reserved1;
            /// <summary>Byte 3 Reserved</summary>
            public byte Reserved2;
            /// <summary>Bytes 4 to 2052 Disc key for CSS, Album Identifier for CPPM</summary>
            public byte[] Key;
        }
    }
}