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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Decoders.Xbox;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Decoders.DVD;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "NotAccessedField.Global")]
public static class SS
{
    public static SecuritySector? Decode(byte[] response)
    {
        if(response == null)
            return null;

        if(response.Length < 2048)
            return null;

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
            ss.ChallengeEntries[i] = new ChallengeEntry
            {
                Level       = response[770 + i * 11 + 0],
                ChallengeId = response[770 + i * 11 + 1],
                ChallengeValue = (uint)((response[770 + i * 11 + 2] << 24) + (response[770 + i * 11 + 3] << 16) +
                                        (response[770 + i * 11 + 4] << 8)  + response[770 + i * 11 + 5]),
                ResponseModifier = response[770 + i * 11 + 6],
                ResponseValue = (uint)((response[770 + i * 11 + 7] << 24) + (response[770 + i * 11 + 8] << 16) +
                                       (response[770 + i * 11 + 9] << 8)  + response[770 + i * 11 + 10])
            };

        Array.Copy(response, 1052, ss.Unknown7, 0, 48);
        Array.Copy(response, 1120, ss.Unknown8, 0, 16);
        Array.Copy(response, 1180, ss.Unknown9, 0, 16);
        Array.Copy(response, 1208, ss.Unknown10, 0, 303);
        Array.Copy(response, 1528, ss.Unknown11, 0, 104);

        for(var i = 0; i < 23; i++)
            ss.Extents[i] = new SecuritySectorExtent
            {
                Unknown = (uint)((response[1633 + i * 9 + 0] << 16) + (response[1633 + i * 9 + 1] << 8) +
                                 response[1633 + i * 9 + 2]),
                StartPSN = (uint)((response[1633 + i * 9 + 3] << 16) + (response[1633 + i * 9 + 4] << 8) +
                                  response[1633 + i * 9 + 5]),
                EndPSN = (uint)((response[1633 + i * 9 + 6] << 16) + (response[1633 + i * 9 + 7] << 8) +
                                response[1633 + i * 9 + 8])
            };

        for(var i = 0; i < 23; i++)
            ss.ExtentsCopy[i] = new SecuritySectorExtent
            {
                Unknown = (uint)((response[1840 + i * 9 + 0] << 16) + (response[1840 + i * 9 + 1] << 8) +
                                 response[1840 + i * 9 + 2]),
                StartPSN = (uint)((response[1840 + i * 9 + 3] << 16) + (response[1840 + i * 9 + 4] << 8) +
                                  response[1840 + i * 9 + 5]),
                EndPSN = (uint)((response[1840 + i * 9 + 6] << 16) + (response[1840 + i * 9 + 7] << 8) +
                                response[1840 + i * 9 + 8])
            };

        return ss;
    }

    public static string Prettify(SecuritySector? ss)
    {
        if(ss == null)
            return null;

        SecuritySector decoded = ss.Value;
        var            sb      = new StringBuilder();

        string sizeString;

        switch(decoded.DiscSize)
        {
            case DVDSize.Eighty:
                sizeString = "80mm";

                break;
            case DVDSize.OneTwenty:
                sizeString = "120mm";

                break;
            default:
                sizeString = $"unknown size identifier {decoded.DiscSize}";

                break;
        }

        const string categorySentence = "Disc is a {0} {1} version {2}";

        switch(decoded.DiskCategory)
        {
            case DiskCategory.DVDPRWDL:
                sb.AppendFormat(categorySentence, sizeString, "Xbox Game Disc", decoded.PartVersion).AppendLine();

                break;
            case DiskCategory.DVDPRDL:
                sb.AppendFormat(categorySentence, sizeString, "Xbox 360 Game Disc", decoded.PartVersion).AppendLine();

                break;
            default:
                sb.AppendFormat(categorySentence, sizeString, "unknown disc type", decoded.PartVersion).AppendLine();

                break;
        }

        switch(decoded.MaximumRate)
        {
            case MaximumRateField.TwoMbps:
                sb.AppendLine("Disc maximum transfer rate is 2,52 Mbit/sec.");

                break;
            case MaximumRateField.FiveMbps:
                sb.AppendLine("Disc maximum transfer rate is 5,04 Mbit/sec.");

                break;
            case MaximumRateField.TenMbps:
                sb.AppendLine("Disc maximum transfer rate is 10,08 Mbit/sec.");

                break;
            case MaximumRateField.TwentyMbps:
                sb.AppendLine("Disc maximum transfer rate is 20,16 Mbit/sec.");

                break;
            case MaximumRateField.ThirtyMbps:
                sb.AppendLine("Disc maximum transfer rate is 30,24 Mbit/sec.");

                break;
            case MaximumRateField.Unspecified:
                sb.AppendLine("Disc maximum transfer rate is unspecified.");

                break;
            default:
                sb.AppendFormat("Disc maximum transfer rate is specified by unknown key {0}", decoded.MaximumRate).
                   AppendLine();

                break;
        }

        sb.AppendFormat("Disc has {0} layers", decoded.Layers + 1).AppendLine();

        switch(decoded.TrackPath)
        {
            case true when decoded.Layers == 1:
                sb.AppendLine("Layers are in parallel track path");

                break;
            case false when decoded.Layers == 1:
                sb.AppendLine("Layers are in opposite track path");

                break;
        }

        switch(decoded.LinearDensity)
        {
            case LinearDensityField.TwoSix:
                sb.AppendLine("Pitch size is 0,267 μm/bit");

                break;
            case LinearDensityField.TwoNine:
                sb.AppendLine("Pitch size is 0,147 μm/bit");

                break;
            case LinearDensityField.FourZero:
                sb.AppendLine("Pitch size is between 0,409 μm/bit and 0,435 μm/bit");

                break;
            case LinearDensityField.TwoEight:
                sb.AppendLine("Pitch size is between 0,140 μm/bit and 0,148 μm/bit");

                break;
            case LinearDensityField.OneFive:
                sb.AppendLine("Pitch size is 0,153 μm/bit");

                break;
            case LinearDensityField.OneThree:
                sb.AppendLine("Pitch size is between 0,130 μm/bit and 0,140 μm/bit");

                break;
            case LinearDensityField.ThreeFive:
                sb.AppendLine("Pitch size is 0,353 μm/bit");

                break;
            default:
                sb.AppendFormat("Unknown pitch size key {0}", decoded.LinearDensity).AppendLine();

                break;
        }

        switch(decoded.TrackDensity)
        {
            case TrackDensityField.Seven:
                sb.AppendLine("Track size is 0,74 μm");

                break;
            case TrackDensityField.Eight:
                sb.AppendLine("Track size is 0,80 μm");

                break;
            case TrackDensityField.Six:
                sb.AppendLine("Track size is 0,615 μm");

                break;
            case TrackDensityField.Four:
                sb.AppendLine("Track size is 0,40 μm");

                break;
            case TrackDensityField.Three:
                sb.AppendLine("Track size is 0,34 μm");

                break;
            default:
                sb.AppendFormat("Unknown track size key {0}", decoded.LinearDensity).AppendLine();

                break;
        }

        if(decoded.DataAreaStartPSN > 0)
            if(decoded.DataAreaEndPSN > 0)
            {
                sb.AppendFormat("Data area starts at PSN {0:X}h", decoded.DataAreaStartPSN).AppendLine();
                sb.AppendFormat("Data area ends at PSN {0:X}h", decoded.DataAreaEndPSN).AppendLine();

                if(decoded.Layers == 1 &&
                   !decoded.TrackPath)
                    sb.AppendFormat("Layer 0 ends at PSN {0:X}h", decoded.Layer0EndPSN).AppendLine();
            }
            else
                sb.AppendLine("Disc is empty");
        else
            sb.AppendLine("Disc is empty");

        sb.AppendLine("Challenges:");

        foreach(ChallengeEntry entry in decoded.ChallengeEntries)
        {
            sb.AppendFormat("\tChallenge ID: {0}", entry.ChallengeId).AppendLine();
            sb.AppendFormat("\tChallenge level: {0}", entry.Level).AppendLine();
            sb.AppendFormat("\tChallenge value: 0x{0:X8}", entry.ChallengeValue).AppendLine();
            sb.AppendFormat("\tResponse modifier: {0}", entry.ResponseModifier).AppendLine();
            sb.AppendFormat("\tResponse value: 0x{0:X8}", entry.ResponseValue).AppendLine();
        }

        for(var i = 0; i < 16; i++)
            sb.AppendFormat("Extent starts at PSN {0:X6}h and ends at PSN {1:X6}h", decoded.Extents[i].StartPSN,
                            decoded.Extents[i].EndPSN).AppendLine();

        return sb.ToString();
    }

    public static string Prettify(byte[] response) => Prettify(Decode(response));

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

    public struct SecuritySectorExtent
    {
        /// <summary>Bytes 0 to 2 Unknown</summary>
        public uint Unknown;
        /// <summary>Bytes 3 to 5 Start PSN of this security extent</summary>
        public uint StartPSN;
        /// <summary>Bytes 6 to 8 End PSN of this security extent</summary>
        public uint EndPSN;
    }

    public struct ChallengeEntry
    {
        public byte Level;
        public byte ChallengeId;
        public uint ChallengeValue;
        public byte ResponseModifier;
        public uint ResponseValue;
    }
}