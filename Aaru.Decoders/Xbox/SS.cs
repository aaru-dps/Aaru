// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : SS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes Xbox security sectors
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
// Copyright © 2011-2024 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Decoders.DVD;

namespace Aaru.Decoders.Xbox;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
public static class SS
{
    public static SecuritySector? Decode(byte[] response)
    {
        if(response == null) return null;

        if(response.Length < 2048) return null;

        var ss = new SecuritySector
        {
            DiskCategory = (DiskCategory)((response[0] & 0xF0) >> 4),
            PartVersion = (byte)(response[0] & 0x0F),
            DiscSize = (DVDSize)((response[1] & 0xF0) >> 4),
            MaximumRate = (MaximumRateField)(response[1] & 0x0F),
            Reserved3 = (response[2] & 0x80) == 0x80,
            Layers = (byte)((response[2] & 0x60) >> 5),
            TrackPath = (response[2] & 0x08) == 0x08,
            LayerType = (LayerTypeFieldMask)(response[2] & 0x07),
            LinearDensity = (LinearDensityField)((response[3] & 0xF0) >> 4),
            TrackDensity = (TrackDensityField)(response[3] & 0x0F),
            DataAreaStartPSN = (uint)((response[4] << 24) + (response[5] << 16) + (response[6] << 8) + response[7]),
            DataAreaEndPSN = (uint)((response[8] << 24) + (response[9] << 16) + (response[10] << 8) + response[11]),
            Layer0EndPSN = (uint)((response[12] << 24) + (response[13] << 16) + (response[14] << 8) + response[15]),
            Unknown1 = response[27],
            Unknown2 = new byte[28],
            Unknown3 = new byte[436],
            Unknown4 = new byte[4],
            Unknown5 = new byte[43],
            ChallengeTableVersion = response[768],
            NoChallengeEntries = response[769],
            ChallengeEntries = new ChallengeEntry[23],
            Unknown6 = response[1023],
            Unknown7 = new byte[48],
            Unknown8 = new byte[16],
            Unknown9 = new byte[16],
            Unknown10 = new byte[303],
            Unknown11 = new byte[104],
            Extents = new SecuritySectorExtent[23],
            ExtentsCopy = new SecuritySectorExtent[23]
        };

        Array.Copy(response, 256, ss.Unknown2, 0, 28);
        Array.Copy(response, 284, ss.Unknown3, 0, 436);
        Array.Copy(response, 720, ss.Unknown4, 0, 4);
        Array.Copy(response, 724, ss.Unknown5, 0, 43);

        for(var i = 0; i < 23; i++)
        {
            ss.ChallengeEntries[i] = new ChallengeEntry
            {
                Level       = response[770 + i * 11 + 0],
                ChallengeId = response[770 + i * 11 + 1],
                ChallengeValue =
                    (uint)((response[770 + i * 11 + 2] << 24) +
                           (response[770 + i * 11 + 3] << 16) +
                           (response[770 + i * 11 + 4] << 8)  +
                           response[770 + i * 11 + 5]),
                ResponseModifier = response[770 + i * 11 + 6],
                ResponseValue = (uint)((response[770 + i * 11 + 7] << 24) +
                                       (response[770 + i * 11 + 8] << 16) +
                                       (response[770 + i * 11 + 9] << 8)  +
                                       response[770 + i * 11 + 10])
            };
        }

        Array.Copy(response, 1052, ss.Unknown7,  0, 48);
        Array.Copy(response, 1120, ss.Unknown8,  0, 16);
        Array.Copy(response, 1180, ss.Unknown9,  0, 16);
        Array.Copy(response, 1208, ss.Unknown10, 0, 303);
        Array.Copy(response, 1528, ss.Unknown11, 0, 104);

        for(var i = 0; i < 23; i++)
        {
            ss.Extents[i] = new SecuritySectorExtent
            {
                Unknown =
                    (uint)((response[1633 + i * 9 + 0] << 16) +
                           (response[1633 + i * 9 + 1] << 8)  +
                           response[1633 + i * 9 + 2]),
                StartPSN = (uint)((response[1633 + i * 9 + 3] << 16) +
                                  (response[1633 + i * 9 + 4] << 8)  +
                                  response[1633 + i * 9 + 5]),
                EndPSN = (uint)((response[1633 + i * 9 + 6] << 16) +
                                (response[1633 + i * 9 + 7] << 8)  +
                                response[1633 + i * 9 + 8])
            };
        }

        for(var i = 0; i < 23; i++)
        {
            ss.ExtentsCopy[i] = new SecuritySectorExtent
            {
                Unknown =
                    (uint)((response[1840 + i * 9 + 0] << 16) +
                           (response[1840 + i * 9 + 1] << 8)  +
                           response[1840 + i * 9 + 2]),
                StartPSN = (uint)((response[1840 + i * 9 + 3] << 16) +
                                  (response[1840 + i * 9 + 4] << 8)  +
                                  response[1840 + i * 9 + 5]),
                EndPSN = (uint)((response[1840 + i * 9 + 6] << 16) +
                                (response[1840 + i * 9 + 7] << 8)  +
                                response[1840 + i * 9 + 8])
            };
        }

        return ss;
    }

    public static string Prettify(SecuritySector? ss)
    {
        if(ss == null) return null;

        SecuritySector decoded = ss.Value;
        var            sb      = new StringBuilder();

        string sizeString = decoded.DiscSize switch
                            {
                                DVDSize.Eighty => Localization._80mm,
                                DVDSize.OneTwenty => Localization._120mm,
                                _ => string.Format(Localization.unknown_size_identifier_0, decoded.DiscSize)
                            };

        string categorySentence = Localization.Disc_is_a_0_1_version_2;

        switch(decoded.DiskCategory)
        {
            case DiskCategory.DVDPRWDL:
                sb.AppendFormat(categorySentence, sizeString, Localization.Xbox_Game_Disc, decoded.PartVersion)
                  .AppendLine();

                break;
            case DiskCategory.DVDPRDL:
                sb.AppendFormat(categorySentence, sizeString, Localization.Xbox_360_Game_Disc, decoded.PartVersion)
                  .AppendLine();

                break;
            default:
                sb.AppendFormat(categorySentence, sizeString, Localization.unknown_disc_type, decoded.PartVersion)
                  .AppendLine();

                break;
        }

        switch(decoded.MaximumRate)
        {
            case MaximumRateField.TwoMbps:
                sb.AppendLine(Localization.Disc_maximum_transfer_rate_is_2_52_Mbit_sec);

                break;
            case MaximumRateField.FiveMbps:
                sb.AppendLine(Localization.Disc_maximum_transfer_rate_is_5_04_Mbit_sec);

                break;
            case MaximumRateField.TenMbps:
                sb.AppendLine(Localization.Disc_maximum_transfer_rate_is_10_08_Mbit_sec);

                break;
            case MaximumRateField.TwentyMbps:
                sb.AppendLine(Localization.Disc_maximum_transfer_rate_is_20_16_Mbit_sec);

                break;
            case MaximumRateField.ThirtyMbps:
                sb.AppendLine(Localization.Disc_maximum_transfer_rate_is_30_24_Mbit_sec);

                break;
            case MaximumRateField.Unspecified:
                sb.AppendLine(Localization.Disc_maximum_transfer_rate_is_unspecified);

                break;
            default:
                sb.AppendFormat(Localization.Disc_maximum_transfer_rate_is_specified_by_unknown_key_0,
                                decoded.MaximumRate)
                  .AppendLine();

                break;
        }

        sb.AppendFormat(Localization.Disc_has_0_layers, decoded.Layers + 1).AppendLine();

        switch(decoded.TrackPath)
        {
            case true when decoded.Layers == 1:
                sb.AppendLine(Localization.Layers_are_in_parallel_track_path);

                break;
            case false when decoded.Layers == 1:
                sb.AppendLine(Localization.Layers_are_in_opposite_track_path);

                break;
        }

        switch(decoded.LinearDensity)
        {
            case LinearDensityField.TwoSix:
                sb.AppendLine(Localization.Pitch_size_is_0_267_μm_bit);

                break;
            case LinearDensityField.TwoNine:
                sb.AppendLine(Localization.Pitch_size_is_0_147_μm_bit);

                break;
            case LinearDensityField.FourZero:
                sb.AppendLine(Localization.Pitch_size_is_between_0_409_μm_bit_and_0_435_μm_bit);

                break;
            case LinearDensityField.TwoEight:
                sb.AppendLine(Localization.Pitch_size_is_between_0_140_μm_bit_and_0_148_μm_bit);

                break;
            case LinearDensityField.OneFive:
                sb.AppendLine(Localization.Pitch_size_is_0_153_μm_bit);

                break;
            case LinearDensityField.OneThree:
                sb.AppendLine(Localization.Pitch_size_is_between_0_130_μm_bit_and_0_140_μm_bit);

                break;
            case LinearDensityField.ThreeFive:
                sb.AppendLine(Localization.Pitch_size_is_0_353_μm_bit);

                break;
            default:
                sb.AppendFormat(Localization.Unknown_pitch_size_key_0, decoded.LinearDensity).AppendLine();

                break;
        }

        switch(decoded.TrackDensity)
        {
            case TrackDensityField.Seven:
                sb.AppendLine(Localization.Track_size_is_0_74_μm);

                break;
            case TrackDensityField.Eight:
                sb.AppendLine(Localization.Track_size_is_0_80_μm);

                break;
            case TrackDensityField.Six:
                sb.AppendLine(Localization.Track_size_is_0_615_μm);

                break;
            case TrackDensityField.Four:
                sb.AppendLine(Localization.Track_size_is_0_40_μm);

                break;
            case TrackDensityField.Three:
                sb.AppendLine(Localization.Track_size_is_0_34_μm);

                break;
            default:
                sb.AppendFormat(Localization.Unknown_track_size_key__0_, decoded.LinearDensity).AppendLine();

                break;
        }

        if(decoded.DataAreaStartPSN > 0)
        {
            if(decoded.DataAreaEndPSN > 0)
            {
                sb.AppendFormat(Localization.Data_area_starts_at_PSN_0, decoded.DataAreaStartPSN).AppendLine();
                sb.AppendFormat(Localization.Data_area_ends_at_PSN_0,   decoded.DataAreaEndPSN).AppendLine();

                if(decoded is { Layers: 1, TrackPath: false })
                    sb.AppendFormat(Localization.Layer_zero_ends_at_PSN_0, decoded.Layer0EndPSN).AppendLine();
            }
            else
                sb.AppendLine(Localization.Disc_is_empty);
        }
        else
            sb.AppendLine(Localization.Disc_is_empty);

        sb.AppendLine("Challenges:");

        foreach(ChallengeEntry entry in decoded.ChallengeEntries)
        {
            sb.AppendFormat("\t" + Localization.Challenge_ID_0,      entry.ChallengeId).AppendLine();
            sb.AppendFormat("\t" + Localization.Challenge_level_0,   entry.Level).AppendLine();
            sb.AppendFormat("\t" + Localization.Challenge_value_0,   entry.ChallengeValue).AppendLine();
            sb.AppendFormat("\t" + Localization.Response_modifier_0, entry.ResponseModifier).AppendLine();
            sb.AppendFormat("\t" + Localization.Response_value_0,    entry.ResponseValue).AppendLine();
        }

        for(var i = 0; i < 16; i++)
        {
            sb.AppendFormat(Localization.Extent_starts_at_PSN_0_and_ends_at_PSN_1,
                            decoded.Extents[i].StartPSN,
                            decoded.Extents[i].EndPSN)
              .AppendLine();
        }

        return sb.ToString();
    }

    public static string Prettify(byte[] response) => Prettify(Decode(response));

#region Nested type: ChallengeEntry

    public struct ChallengeEntry
    {
        public byte Level;
        public byte ChallengeId;
        public uint ChallengeValue;
        public byte ResponseModifier;
        public uint ResponseValue;
    }

#endregion

#region Nested type: SecuritySector

    public struct SecuritySector
    {
        /// <summary>Byte 0, bits 7 to 4 Disk category field</summary>
        public DiskCategory DiskCategory;
        /// <summary>Byte 0, bits 3 to 0 Media version</summary>
        public byte PartVersion;
        /// <summary>Byte 1, bits 7 to 4 120mm if 0, 80mm if 1. If UMD (60mm) 0 also. Reserved rest of values</summary>
        public DVDSize DiscSize;
        /// <summary>Byte 1, bits 3 to 0 Maximum data rate</summary>
        public MaximumRateField MaximumRate;
        /// <summary>Byte 2, bit 7 Reserved</summary>
        public bool Reserved3;
        /// <summary>Byte 2, bits 6 to 5 Number of layers</summary>
        public byte Layers;
        /// <summary>Byte 2, bit 4 Track path</summary>
        public bool TrackPath;
        /// <summary>Byte 2, bits 3 to 0 Layer type</summary>
        public LayerTypeFieldMask LayerType;
        /// <summary>Byte 3, bits 7 to 4 Linear density field</summary>
        public LinearDensityField LinearDensity;
        /// <summary>Byte 3, bits 3 to 0 Track density field</summary>
        public TrackDensityField TrackDensity;
        /// <summary>Bytes 4 to 7 PSN where Data Area starts</summary>
        public uint DataAreaStartPSN;
        /// <summary>Bytes 8 to 11 PSN where Data Area ends</summary>
        public uint DataAreaEndPSN;
        /// <summary>Bytes 12 to 15 PSN where Data Area ends in Layer 0</summary>
        public uint Layer0EndPSN;

        /// <summary>Byte 27 Always 0x06 on XGD3</summary>
        public byte Unknown1;
        /// <summary>Bytes 256 to 283 Unknown, XGD2 and XGD3</summary>
        public byte[] Unknown2;
        /// <summary>Bytes 284 to 719 Unknown, XGD3</summary>
        public byte[] Unknown3;
        /// <summary>Bytes 720 to 723 Unknown</summary>
        public byte[] Unknown4;
        /// <summary>Bytes 724 to 767 Unknown, XGD3</summary>
        public byte[] Unknown5;
        /// <summary>Byte 768 Version of challenge table</summary>
        public byte ChallengeTableVersion;
        /// <summary>Byte 769 Number of challenge entries</summary>
        public byte NoChallengeEntries;
        /// <summary>Bytes 770 to 1022 Unknown</summary>
        public ChallengeEntry[] ChallengeEntries;
        /// <summary>Byte 1023 Unknown</summary>
        public byte Unknown6;
        /// <summary>Bytes 1052 to 1099 Unknown, XGD1 only</summary>
        public byte[] Unknown7;
        /// <summary>Bytes 1120 to 1135 Unknown, XGD2 and XGD3</summary>
        public byte[] Unknown8;
        /// <summary>Bytes 1180 to 1195 Unknown</summary>
        public byte[] Unknown9;
        /// <summary>Bytes 1208 to 1511 Unknown</summary>
        public byte[] Unknown10;
        /// <summary>Bytes 1528 to 1632</summary>
        public byte[] Unknown11;
        /// <summary>Bytes 1633 to 1839 Security extents, 23 entries of 9 bytes</summary>
        public SecuritySectorExtent[] Extents;
        /// <summary>Bytes 1840 to 2047 Copy of the security extents, 23 entries of 9 bytes</summary>
        public SecuritySectorExtent[] ExtentsCopy;
    }

#endregion

#region Nested type: SecuritySectorExtent

    public struct SecuritySectorExtent
    {
        /// <summary>Bytes 0 to 2 Unknown</summary>
        public uint Unknown;
        /// <summary>Bytes 3 to 5 Start PSN of this security extent</summary>
        public uint StartPSN;
        /// <summary>Bytes 6 to 8 End PSN of this security extent</summary>
        public uint EndPSN;
    }

#endregion
}