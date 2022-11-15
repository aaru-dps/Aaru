// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// Copyright © 2020-2022 Rebecca Wallander
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
public static class CSS_CPRM
{
    public static LeadInCopyright? DecodeLeadInCopyright(byte[] response)
    {
        if(response?.Length != 8)
            return null;

        return new LeadInCopyright
        {
            DataLength        = (ushort)((response[0] << 8) + response[1]),
            Reserved1         = response[2],
            Reserved2         = response[3],
            CopyrightType     = (CopyrightType)response[4],
            RegionInformation = response[5],
            Reserved3         = response[6],
            Reserved4         = response[7]
        };
    }

    public static RegionalPlaybackControlState? DecodeRegionalPlaybackControlState(byte[] response)
    {
        if(response?.Length != 8)
            return null;

        return new RegionalPlaybackControlState
        {
            DataLength                                                    = (ushort)((response[0] << 8) + response[1]),
            Reserved1                                                     = response[2],
            Reserved2                                                     = response[3],
            TypeCode_VendorResetsAvailable_UserControlledChangesAvailable = response[4],
            RegionMask                                                    = response[5],
            RPCScheme                                                     = response[6],
            Reserved3                                                     = response[7]
        };
    }

    public static string PrettifyRegionalPlaybackControlState(RegionalPlaybackControlState? rpc)
    {
        if(rpc == null)
            return null;

        RegionalPlaybackControlState decoded = rpc.Value;
        var                          sb      = new StringBuilder();

        var typeCode = (TypeCode)((decoded.TypeCode_VendorResetsAvailable_UserControlledChangesAvailable & 0xc0) >> 6);

        int vendorResets = (decoded.TypeCode_VendorResetsAvailable_UserControlledChangesAvailable & 0x38) >> 3;

        int userControlledChanges = decoded.TypeCode_VendorResetsAvailable_UserControlledChangesAvailable & 0x7;

        switch(typeCode)
        {
            case TypeCode.None:
                sb.AppendLine("No drive region setting.");

                break;
            case TypeCode.Set:
                sb.AppendLine("Drive region is set.");

                break;
            case TypeCode.LastChance:
                sb.AppendLine("Drive region is set, with additional restrictions required to make a change.");

                break;
            case TypeCode.Perm:
                sb.AppendLine("Drive region has been set permanently, but may be reset by the vendor if necessary.");

                break;
        }

        sb.AppendLine($"Drive has {vendorResets} vendor resets available.");
        sb.AppendLine($"Drive has {userControlledChanges} user controlled changes available.");

        switch(decoded.RegionMask)
        {
            case 0xFF:
                sb.AppendLine("Drive has no region set.");

                break;
            case 0x00:
                sb.AppendLine("Drive is region free.");

                break;
            default:
            {
                sb.Append("Drive has the following regions set:");

                if((decoded.RegionMask & 0x01) != 0x01)
                    sb.Append(" 1");

                if((decoded.RegionMask & 0x02) != 0x02)
                    sb.Append(" 2");

                if((decoded.RegionMask & 0x04) != 0x04)
                    sb.Append(" 3");

                if((decoded.RegionMask & 0x08) != 0x08)
                    sb.Append(" 4");

                if((decoded.RegionMask & 0x10) != 0x10)
                    sb.Append(" 5");

                if((decoded.RegionMask & 0x20) != 0x20)
                    sb.Append(" 6");

                if((decoded.RegionMask & 0x40) != 0x40)
                    sb.Append(" 7");

                if((decoded.RegionMask & 0x80) != 0x80)
                    sb.Append(" 8");

                break;
            }
        }

        sb.AppendLine("");

        switch(decoded.RPCScheme)
        {
            case 0x00:
                sb.AppendLine("The Logical Unit does not enforce Region Playback Controls (RPC).");

                break;
            case 0x01:
                sb.AppendLine("The Logical Unit shall adhere to the specification and all requirements of the CSS license agreement concerning RPC.");

                break;
            default:
                sb.AppendLine("The Logical Unit uses an unknown region enforcement scheme.");

                break;
        }

        return sb.ToString();
    }

    public static string PrettifyRegionalPlaybackControlState(byte[] response) =>
        PrettifyRegionalPlaybackControlState(DecodeRegionalPlaybackControlState(response));

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

        switch(decoded.RegionInformation)
        {
            case 0xFF:
                sb.AppendLine("Disc cannot be played in any region at all.");

                break;
            case 0x00:
                sb.AppendLine("Disc can be played in any region.");

                break;
            default:
            {
                sb.Append("Disc can be played in the following regions:");

                if((decoded.RegionInformation & 0x01) != 0x01)
                    sb.Append(" 1");

                if((decoded.RegionInformation & 0x02) != 0x02)
                    sb.Append(" 2");

                if((decoded.RegionInformation & 0x04) != 0x04)
                    sb.Append(" 3");

                if((decoded.RegionInformation & 0x08) != 0x08)
                    sb.Append(" 4");

                if((decoded.RegionInformation & 0x10) != 0x10)
                    sb.Append(" 5");

                if((decoded.RegionInformation & 0x20) != 0x20)
                    sb.Append(" 6");

                if((decoded.RegionInformation & 0x40) != 0x40)
                    sb.Append(" 7");

                if((decoded.RegionInformation & 0x80) != 0x80)
                    sb.Append(" 8");

                break;
            }
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

    public struct TitleKey
    {
        /// <summary>Bytes 0 to 1 Data length</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Byte 4 CPM</summary>
        public byte CMI;
        /// <summary>Bytes 5 to 10 Title key for CSS</summary>
        public byte[] Key;
        /// <summary>Byte 11 Reserved</summary>
        public byte Reserved3;
        /// <summary>Byte 12 Reserved</summary>
        public byte Reserved4;
    }

    public struct AuthenticationSuccessFlag
    {
        /// <summary>Bytes 0 to 1 Data length</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Byte 4 Reserved</summary>
        public byte Reserved3;
        /// <summary>Byte 5 Reserved</summary>
        public byte Reserved4;
        /// <summary>Byte 6 Reserved</summary>
        public byte Reserved5;
        /// <summary>Byte 7 Reserved and ASF</summary>
        public byte ASF;
    }

    public struct RegionalPlaybackControlState
    {
        /// <summary>Bytes 0 to 1 Data length</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Byte 4 Type Code and # of Vendor Resets Available and # of User Controlled Changes Available</summary>
        public byte TypeCode_VendorResetsAvailable_UserControlledChangesAvailable;
        /// <summary>Byte 5 Region Mask</summary>
        public byte RegionMask;
        /// <summary>Byte 6 RPC Scheme</summary>
        public byte RPCScheme;
        /// <summary>Byte 7 Reserved</summary>
        public byte Reserved3;
    }

    enum TypeCode
    {
        None = 0, Set = 1, LastChance = 2,
        Perm = 3
    }
}