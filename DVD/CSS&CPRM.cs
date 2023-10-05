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
// Copyright © 2011-2023 Natalia Portillo
// Copyright © 2020-2023 Rebecca Wallander
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
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
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
                sb.AppendLine(Localization.No_drive_region_setting);

                break;
            case TypeCode.Set:
                sb.AppendLine(Localization.Drive_region_is_set);

                break;
            case TypeCode.LastChance:
                sb.AppendLine(Localization.Drive_region_is_set_with_additional_restrictions_required_to_make_a_change);

                break;
            case TypeCode.Perm:
                sb.AppendLine(Localization.
                                  Drive_region_has_been_set_permanently_but_may_be_reset_by_the_vendor_if_necessary);

                break;
        }

        sb.AppendLine(string.Format(Localization.Drive_has_0_vendor_resets_available,           vendorResets));
        sb.AppendLine(string.Format(Localization.Drive_has_0_user_controlled_changes_available, userControlledChanges));

        switch(decoded.RegionMask)
        {
            case 0xFF:
                sb.AppendLine(Localization.Drive_has_no_region_set);

                break;
            case 0x00:
                sb.AppendLine(Localization.Drive_is_region_free);

                break;
            default:
            {
                sb.Append(Localization.Drive_has_the_following_regions_set);

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
                sb.AppendLine(Localization.The_Logical_Unit_does_not_enforce_Region_Playback_Controls_RPC);

                break;
            case 0x01:
                sb.AppendLine(Localization.
                                  The_Logical_Unit_shall_adhere_to_the_specification_and_all_requirements_of_the_CSS_license_agreement_concerning_RPC);

                break;
            default:
                sb.AppendLine(Localization.The_Logical_Unit_uses_an_unknown_region_enforcement_scheme);

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
                sb.AppendLine(Localization.Disc_has_no_encryption);

                break;
            case CopyrightType.CSS:
                sb.AppendLine(Localization.Disc_is_encrypted_using_CSS_or_CPPM);

                break;
            case CopyrightType.CPRM:
                sb.AppendLine(Localization.Disc_is_encrypted_using_CPRM);

                break;
            case CopyrightType.AACS:
                sb.AppendLine(Localization.Disc_is_encrypted_using_AACS);

                break;
            default:
                sb.AppendFormat(Localization.Disc_is_encrypted_using_unknown_algorithm_with_ID_0,
                                decoded.CopyrightType);

                break;
        }

        if(decoded.CopyrightType == 0)
            return sb.ToString();

        switch(decoded.RegionInformation)
        {
            case 0xFF:
                sb.AppendLine(Localization.Disc_cannot_be_played_in_any_region_at_all);

                break;
            case 0x00:
                sb.AppendLine(Localization.Disc_can_be_played_in_any_region);

                break;
            default:
            {
                sb.Append(Localization.Disc_can_be_played_in_the_following_regions);

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

#region Nested type: AuthenticationSuccessFlag

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

#endregion

#region Nested type: DiscKey

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

#endregion

#region Nested type: LeadInCopyright

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

#endregion

#region Nested type: RegionalPlaybackControlState

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

#endregion

#region Nested type: TitleKey

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

#endregion

#region Nested type: TypeCode

    enum TypeCode
    {
        None       = 0,
        Set        = 1,
        LastChance = 2,
        Perm       = 3
    }

#endregion
}