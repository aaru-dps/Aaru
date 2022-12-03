// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PMA.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes CD Power Management Area.
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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Decoders.CD;

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
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class PMA
{
    public static CDPMA? Decode(byte[] CDPMAResponse)
    {
        if(CDPMAResponse is not { Length: > 4 })
            return null;

        var decoded = new CDPMA
        {
            DataLength = BigEndianBitConverter.ToUInt16(CDPMAResponse, 0),
            Reserved1  = CDPMAResponse[2],
            Reserved2  = CDPMAResponse[3]
        };

        decoded.PMADescriptors = new CDPMADescriptors[(decoded.DataLength - 2) / 11];

        if(decoded.PMADescriptors.Length == 0)
            return null;

        if(decoded.DataLength + 2 != CDPMAResponse.Length)
        {
            AaruConsole.DebugWriteLine("CD PMA decoder",
                                       Localization.
                                           Expected_CD_PMA_size_0_bytes_is_not_received_size_1_bytes_not_decoding,
                                       decoded.DataLength + 2, CDPMAResponse.Length);

            return null;
        }

        for(int i = 0; i < (decoded.DataLength - 2) / 11; i++)
        {
            decoded.PMADescriptors[i].Reserved = CDPMAResponse[0 + (i * 11) + 4];
            decoded.PMADescriptors[i].ADR      = (byte)((CDPMAResponse[1 + (i * 11) + 4] & 0xF0) >> 4);
            decoded.PMADescriptors[i].CONTROL  = (byte)(CDPMAResponse[1 + (i * 11) + 4] & 0x0F);
            decoded.PMADescriptors[i].TNO      = CDPMAResponse[2 + (i * 11) + 4];
            decoded.PMADescriptors[i].POINT    = CDPMAResponse[3 + (i * 11) + 4];
            decoded.PMADescriptors[i].Min      = CDPMAResponse[4 + (i * 11) + 4];
            decoded.PMADescriptors[i].Sec      = CDPMAResponse[5 + (i * 11) + 4];
            decoded.PMADescriptors[i].Frame    = CDPMAResponse[6 + (i * 11) + 4];
            decoded.PMADescriptors[i].HOUR     = (byte)((CDPMAResponse[7 + (i * 11) + 4] & 0xF0) >> 4);
            decoded.PMADescriptors[i].PHOUR    = (byte)(CDPMAResponse[7 + (i * 11) + 4] & 0x0F);
            decoded.PMADescriptors[i].PMIN     = CDPMAResponse[8  + (i * 11) + 4];
            decoded.PMADescriptors[i].PSEC     = CDPMAResponse[9  + (i * 11) + 4];
            decoded.PMADescriptors[i].PFRAME   = CDPMAResponse[10 + (i * 11) + 4];
        }

        return decoded;
    }

    public static string Prettify(CDPMA? CDPMAResponse)
    {
        if(CDPMAResponse == null)
            return null;

        CDPMA response = CDPMAResponse.Value;

        var sb = new StringBuilder();

    #if DEBUG
        if(response.Reserved1 != 0)
            sb.AppendFormat(Localization.Reserved1_equals_0_X8, response.Reserved1).AppendLine();

        if(response.Reserved2 != 0)
            sb.AppendFormat(Localization.Reserved2_equals_0_X8, response.Reserved2).AppendLine();
    #endif

        List<string> tracks;

        foreach(CDPMADescriptors descriptor in response.PMADescriptors)
        {
        #if DEBUG
            if(descriptor.Reserved != 0)
                sb.AppendFormat(Localization.Reserved_equals_0_X2, descriptor.Reserved).AppendLine();
        #endif

            switch(descriptor.ADR)
            {
                case 1:
                    if(descriptor.POINT > 0)
                    {
                        switch((TocControl)(descriptor.CONTROL & 0x0D))
                        {
                            case TocControl.TwoChanNoPreEmph:
                                if(descriptor.PHOUR > 0)
                                    sb.AppendFormat(Localization.Track_0_Stereo_audio_track_with_no_pre_emphasis_starts_at_4_1_2_3_and_ends_at_8_5_6_7,
                                                    descriptor.POINT, descriptor.PMIN, descriptor.PSEC,
                                                    descriptor.PFRAME, descriptor.PHOUR, descriptor.Min, descriptor.Sec,
                                                    descriptor.Frame, descriptor.HOUR);
                                else
                                    sb.AppendFormat(Localization.Track_0_Stereo_audio_track_with_no_pre_emphasis_starts_at_1_2_3_and_ends_at_4_5_6,
                                                    descriptor.POINT, descriptor.PMIN, descriptor.PSEC,
                                                    descriptor.PFRAME, descriptor.Min, descriptor.Sec,
                                                    descriptor.Frame);

                                break;
                            case TocControl.TwoChanPreEmph:
                                if(descriptor.PHOUR > 0)
                                    sb.AppendFormat(Localization.Track_0_Stereo_audio_track_with_50_15_s_pre_emphasis_starts_at_4_1_2_3_and_ends_at_8_5_6_7,
                                                    descriptor.POINT, descriptor.PMIN, descriptor.PSEC,
                                                    descriptor.PFRAME, descriptor.PHOUR, descriptor.Min, descriptor.Sec,
                                                    descriptor.Frame, descriptor.HOUR);
                                else
                                    sb.AppendFormat(Localization.Track_0_Stereo_audio_track_with_50_15_us_pre_emphasis_starts_at_1_2_3_and_ends_at_4_5_6,
                                                    descriptor.POINT, descriptor.PMIN, descriptor.PSEC,
                                                    descriptor.PFRAME, descriptor.Min, descriptor.Sec,
                                                    descriptor.Frame);

                                break;
                            case TocControl.FourChanNoPreEmph:
                                if(descriptor.PHOUR > 0)
                                    sb.AppendFormat(Localization.Track_0_Quadraphonic_audio_track_with_no_pre_emphasis_starts_at_4_1_2_3_and_ends_at_8_5_6_7,
                                                    descriptor.POINT, descriptor.PMIN, descriptor.PSEC,
                                                    descriptor.PFRAME, descriptor.PHOUR, descriptor.Min, descriptor.Sec,
                                                    descriptor.Frame, descriptor.HOUR);
                                else
                                    sb.AppendFormat(Localization.Track_0_Quadraphonic_audio_track_with_no_pre_emphasis_starts_at_1_2_3_and_ends_at_4_5_6,
                                                    descriptor.POINT, descriptor.PMIN, descriptor.PSEC,
                                                    descriptor.PFRAME, descriptor.Min, descriptor.Sec,
                                                    descriptor.Frame);

                                break;
                            case TocControl.FourChanPreEmph:
                                if(descriptor.PHOUR > 0)
                                    sb.AppendFormat(Localization.Track_0_Quadraphonic_audio_track_with_50_15_us_pre_emphasis_starts_at_4_1_2_3_and_ends_at_8_5_6_7,
                                                    descriptor.POINT, descriptor.PMIN, descriptor.PSEC,
                                                    descriptor.PFRAME, descriptor.PHOUR, descriptor.Min, descriptor.Sec,
                                                    descriptor.Frame, descriptor.HOUR);
                                else
                                    sb.AppendFormat(Localization.Track_0_Quadraphonic_audio_track_with_50_15_us_pre_emphasis_starts_at_1_2_3_and_ends_at_4_5_6,
                                                    descriptor.POINT, descriptor.PMIN, descriptor.PSEC,
                                                    descriptor.PFRAME, descriptor.Min, descriptor.Sec,
                                                    descriptor.Frame);

                                break;
                            case TocControl.DataTrack:
                                if(descriptor.PHOUR > 0)
                                    sb.AppendFormat(Localization.Track_0_Data_track_recorded_uninterrupted_starts_at_4_1_2_3_and_ends_at_8_5_6_7,
                                                    descriptor.POINT, descriptor.PMIN, descriptor.PSEC,
                                                    descriptor.PFRAME, descriptor.PHOUR, descriptor.Min, descriptor.Sec,
                                                    descriptor.Frame, descriptor.HOUR);
                                else
                                    sb.AppendFormat(Localization.Track_0_Data_track_recorded_uninterrupted_starts_at_1_2_3_and_ends_at_4_5_6,
                                                    descriptor.POINT, descriptor.PMIN, descriptor.PSEC,
                                                    descriptor.PFRAME, descriptor.Min, descriptor.Sec,
                                                    descriptor.Frame);

                                break;
                            case TocControl.DataTrackIncremental:
                                if(descriptor.PHOUR > 0)
                                    sb.AppendFormat(Localization.Track_0_Data_track_recorded_incrementally_starts_at_4_1_2_3_and_ends_at_8_5_6_7,
                                                    descriptor.POINT, descriptor.PMIN, descriptor.PSEC,
                                                    descriptor.PFRAME, descriptor.PHOUR, descriptor.Min, descriptor.Sec,
                                                    descriptor.Frame, descriptor.HOUR);
                                else
                                    sb.AppendFormat(Localization.Track_0_Data_track_recorded_incrementally_starts_at_1_2_3_and_ends_at_4_5_6,
                                                    descriptor.POINT, descriptor.PMIN, descriptor.PSEC,
                                                    descriptor.PFRAME, descriptor.Min, descriptor.Sec,
                                                    descriptor.Frame);

                                break;
                        }

                        sb.AppendLine();
                    }
                    else
                        goto default;

                    break;
                case 2:
                    uint id = (uint)((descriptor.Min << 16) + (descriptor.Sec << 8) + descriptor.Frame);
                    sb.AppendFormat(Localization.Disc_ID_0_X6, id & 0x00FFFFFF).AppendLine();

                    break;
                case 3:
                    tracks = new List<string>();

                    if(descriptor.Min > 0)
                        tracks.Add($"{descriptor.Min}");

                    if(descriptor.Sec > 0)
                        tracks.Add($"{descriptor.Sec}");

                    if(descriptor.Frame > 0)
                        tracks.Add($"{descriptor.Frame}");

                    if(descriptor.PMIN > 0)
                        tracks.Add($"{descriptor.PMIN}");

                    if(descriptor.PSEC > 0)
                        tracks.Add($"{descriptor.PSEC}");

                    if(descriptor.PFRAME > 0)
                        tracks.Add($"{descriptor.PFRAME}");

                    sb.AppendFormat(Localization.Skip_track_assignment_0_says_that_tracks_1_should_be_skipped,
                                    descriptor.POINT, string.Join(' ', tracks));

                    break;
                case 4:
                    tracks = new List<string>();

                    if(descriptor.Min > 0)
                        tracks.Add($"{descriptor.Min}");

                    if(descriptor.Sec > 0)
                        tracks.Add($"{descriptor.Sec}");

                    if(descriptor.Frame > 0)
                        tracks.Add($"{descriptor.Frame}");

                    if(descriptor.PMIN > 0)
                        tracks.Add($"{descriptor.PMIN}");

                    if(descriptor.PSEC > 0)
                        tracks.Add($"{descriptor.PSEC}");

                    if(descriptor.PFRAME > 0)
                        tracks.Add($"{descriptor.PFRAME}");

                    sb.AppendFormat(Localization.Unskip_track_assignment_0_says_that_tracks_1_should_not_be_skipped,
                                    descriptor.POINT, string.Join(' ', tracks));

                    break;
                case 5:
                    if(descriptor.PHOUR > 0)
                        sb.AppendFormat(Localization.Skip_time_interval_assignment_0_says_that_from_4_1_2_3_to_8_5_6_7_should_be_skipped,
                                        descriptor.POINT, descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME,
                                        descriptor.PHOUR, descriptor.Min, descriptor.Sec, descriptor.Frame,
                                        descriptor.HOUR);
                    else
                        sb.AppendFormat(Localization.Skip_time_interval_assignment_0_says_that_from_1_2_3_to_4_5_6_should_be_skipped,
                                        descriptor.POINT, descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME,
                                        descriptor.Min, descriptor.Sec, descriptor.Frame);

                    break;
                case 6:
                    if(descriptor.PHOUR > 0)
                        sb.AppendFormat(Localization.Unskip_time_interval_assignment_0_says_that_from_4_1_2_3_to_8_5_6_7_should_not_be_skipped,
                                        descriptor.POINT, descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME,
                                        descriptor.PHOUR, descriptor.Min, descriptor.Sec, descriptor.Frame,
                                        descriptor.HOUR);
                    else
                        sb.AppendFormat(Localization.Unskip_time_interval_assignment_0_says_that_from_1_2_3_to_4_5_6_should_not_be_skipped,
                                        descriptor.POINT, descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME,
                                        descriptor.Min, descriptor.Sec, descriptor.Frame);

                    break;
                default:

                    sb.AppendLine($"ADR = {descriptor.ADR}");
                    sb.AppendLine($"CONTROL = {descriptor.CONTROL}");
                    sb.AppendLine($"TNO = {descriptor.TNO}");
                    sb.AppendLine($"POINT = {descriptor.POINT}");
                    sb.AppendLine($"Min = {descriptor.Min}");
                    sb.AppendLine($"Sec = {descriptor.Sec}");
                    sb.AppendLine($"Frame = {descriptor.Frame}");
                    sb.AppendLine($"HOUR = {descriptor.HOUR}");
                    sb.AppendLine($"PHOUR = {descriptor.PHOUR}");
                    sb.AppendLine($"PMIN = {descriptor.PMIN}");
                    sb.AppendLine($"PSEC = {descriptor.PSEC}");
                    sb.AppendLine($"PFRAME = {descriptor.PFRAME}");

                    break;
            }
        }

        return sb.ToString();
    }

    public static string Prettify(byte[] CDPMAResponse)
    {
        CDPMA? decoded = Decode(CDPMAResponse);

        return Prettify(decoded);
    }

    public struct CDPMA
    {
        /// <summary>Total size of returned session information minus this field</summary>
        public ushort DataLength;
        /// <summary>Reserved</summary>
        public byte Reserved1;
        /// <summary>Reserved</summary>
        public byte Reserved2;
        /// <summary>Track descriptors</summary>
        public CDPMADescriptors[] PMADescriptors;
    }

    public struct CDPMADescriptors
    {
        /// <summary>Byte 0 Reserved</summary>
        public byte Reserved;
        /// <summary>Byte 1, bits 7 to 4 Type of information in Q subchannel of block where this TOC entry was found</summary>
        public byte ADR;
        /// <summary>Byte 1, bits 3 to 0 Track attributes</summary>
        public byte CONTROL;
        /// <summary>Byte 2</summary>
        public byte TNO;
        /// <summary>Byte 3</summary>
        public byte POINT;
        /// <summary>Byte 4</summary>
        public byte Min;
        /// <summary>Byte 5</summary>
        public byte Sec;
        /// <summary>Byte 6</summary>
        public byte Frame;
        /// <summary>Byte 7, bits 7 to 4</summary>
        public byte HOUR;
        /// <summary>Byte 7, bits 3 to 0</summary>
        public byte PHOUR;
        /// <summary>Byte 8</summary>
        public byte PMIN;
        /// <summary>Byte 9</summary>
        public byte PSEC;
        /// <summary>Byte 10</summary>
        public byte PFRAME;
    }
}