// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : FullTOC.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes CD full Table of Contents.
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

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Aaru.CommonTypes.Enums;
using Aaru.CommonTypes.Structs;
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
// ISO/IEC 61104: Compact disc video system - 12 cm CD-V
// ISO/IEC 60908: Audio recording - Compact disc digital audio system
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class FullTOC
{
    const string MODULE_NAME = "CD full TOC decoder";

    public static CDFullTOC? Decode(byte[] CDFullTOCResponse)
    {
        if(CDFullTOCResponse is not { Length: > 4 }) return null;

        var decoded = new CDFullTOC
        {
            DataLength           = BigEndianBitConverter.ToUInt16(CDFullTOCResponse, 0),
            FirstCompleteSession = CDFullTOCResponse[2],
            LastCompleteSession  = CDFullTOCResponse[3]
        };

        decoded.TrackDescriptors = new TrackDataDescriptor[(decoded.DataLength - 2) / 11];

        if(decoded.DataLength + 2 != CDFullTOCResponse.Length)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       Localization
                                          .Expected_CDFullTOC_size_0_bytes_is_not_received_size_1_bytes_not_decoding,
                                       decoded.DataLength + 2,
                                       CDFullTOCResponse.Length);

            return null;
        }

        for(var i = 0; i < (decoded.DataLength - 2) / 11; i++)
        {
            decoded.TrackDescriptors[i].SessionNumber = CDFullTOCResponse[0 + i * 11 + 4];
            decoded.TrackDescriptors[i].ADR           = (byte)((CDFullTOCResponse[1 + i * 11 + 4] & 0xF0) >> 4);
            decoded.TrackDescriptors[i].CONTROL       = (byte)(CDFullTOCResponse[1 + i * 11 + 4] & 0x0F);
            decoded.TrackDescriptors[i].TNO           = CDFullTOCResponse[2 + i * 11 + 4];
            decoded.TrackDescriptors[i].POINT         = CDFullTOCResponse[3 + i * 11 + 4];
            decoded.TrackDescriptors[i].Min           = CDFullTOCResponse[4 + i * 11 + 4];
            decoded.TrackDescriptors[i].Sec           = CDFullTOCResponse[5 + i * 11 + 4];
            decoded.TrackDescriptors[i].Frame         = CDFullTOCResponse[6 + i * 11 + 4];
            decoded.TrackDescriptors[i].Zero          = CDFullTOCResponse[7 + i * 11 + 4];
            decoded.TrackDescriptors[i].HOUR          = (byte)((CDFullTOCResponse[7 + i * 11 + 4] & 0xF0) >> 4);
            decoded.TrackDescriptors[i].PHOUR         = (byte)(CDFullTOCResponse[7 + i * 11 + 4] & 0x0F);
            decoded.TrackDescriptors[i].PMIN          = CDFullTOCResponse[8  + i * 11 + 4];
            decoded.TrackDescriptors[i].PSEC          = CDFullTOCResponse[9  + i * 11 + 4];
            decoded.TrackDescriptors[i].PFRAME        = CDFullTOCResponse[10 + i * 11 + 4];
        }

        return decoded;
    }

    public static string Prettify(CDFullTOC? CDFullTOCResponse)
    {
        if(CDFullTOCResponse == null) return null;

        CDFullTOC response = CDFullTOCResponse.Value;

        var sb = new StringBuilder();

        var lastSession = 0;

        sb.AppendFormat(Localization.First_complete_session_number_0, response.FirstCompleteSession).AppendLine();
        sb.AppendFormat(Localization.Last_complete_session_number_0,  response.LastCompleteSession).AppendLine();

        foreach(TrackDataDescriptor descriptor in response.TrackDescriptors)
        {
            if((descriptor.CONTROL & 0x08) == 0x08                                                      ||
               descriptor.ADR != 1 && descriptor.ADR != 5 && descriptor.ADR != 4 && descriptor.ADR != 6 ||
               descriptor.TNO != 0)
            {
                sb.AppendLine(Localization.Unknown_TOC_entry_format_printing_values_as_is);
                sb.AppendLine($"SessionNumber = {descriptor.SessionNumber}");
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
            }
            else
            {
                if(descriptor.SessionNumber > lastSession)
                {
                    sb.AppendFormat(Localization.Session_0, descriptor.SessionNumber).AppendLine();
                    lastSession = descriptor.SessionNumber;
                }

                switch(descriptor.ADR)
                {
                    case 1:
                    case 4:
                    {
                        switch(descriptor.POINT)
                        {
                            case 0xA0 when descriptor.ADR == 4:
                            {
                                sb.AppendFormat(Localization.First_video_track_number_0, descriptor.PMIN).AppendLine();

                                switch(descriptor.PSEC)
                                {
                                    case 0x10:
                                        sb.AppendLine(Localization
                                                         .CD_V_single_in_NTSC_format_with_digital_stereo_sound);

                                        break;
                                    case 0x11:
                                        sb.AppendLine(Localization
                                                         .CD_V_single_in_NTSC_format_with_digital_bilingual_sound);

                                        break;
                                    case 0x12:
                                        sb.AppendLine(Localization.CD_V_disc_in_NTSC_format_with_digital_stereo_sound);

                                        break;
                                    case 0x13:
                                        sb.AppendLine(Localization
                                                         .CD_V_disc_in_NTSC_format_with_digital_bilingual_sound);

                                        break;
                                    case 0x20:
                                        sb.AppendLine(Localization.CD_V_single_in_PAL_format_with_digital_stereo_sound);

                                        break;
                                    case 0x21:
                                        sb.AppendLine(Localization
                                                         .CD_V_single_in_PAL_format_with_digital_bilingual_sound);

                                        break;
                                    case 0x22:
                                        sb.AppendLine(Localization.CD_V_disc_in_PAL_format_with_digital_stereo_sound);

                                        break;
                                    case 0x23:
                                        sb.AppendLine(Localization
                                                         .CD_V_disc_in_PAL_format_with_digital_bilingual_sound);

                                        break;
                                }

                                break;
                            }

                            case 0xA0 when descriptor.ADR == 1:
                            {
                                sb.AppendFormat(Localization.First_track_number_0_open_parenthesis, descriptor.PMIN);

                                switch((TocControl)(descriptor.CONTROL & 0x0D))
                                {
                                    case TocControl.TwoChanNoPreEmph:
                                        sb.Append(Localization.Stereo_audio_track_with_no_pre_emphasis);

                                        break;
                                    case TocControl.TwoChanPreEmph:
                                        sb.Append(Localization.Stereo_audio_track_with_50_15_us_pre_emphasis);

                                        break;
                                    case TocControl.FourChanNoPreEmph:
                                        sb.Append(Localization.Quadraphonic_audio_track_with_no_pre_emphasis);

                                        break;
                                    case TocControl.FourChanPreEmph:
                                        sb.Append(Localization.Quadraphonic_audio_track_with_50_15_us_pre_emphasis);

                                        break;
                                    case TocControl.DataTrack:
                                        sb.Append(Localization.Data_track_recorded_uninterrupted);

                                        break;
                                    case TocControl.DataTrackIncremental:
                                        sb.Append(Localization.Data_track_recorded_incrementally);

                                        break;
                                }

                                sb.AppendLine(Localization.close_parenthesis);
                                sb.AppendFormat(Localization.Disc_type_0, descriptor.PSEC).AppendLine();

                                //sb.AppendFormat("Absolute time: {3:D2}:{0:D2}:{1:D2}:{2:D2}", descriptor.Min, descriptor.Sec, descriptor.Frame, descriptor.HOUR).AppendLine();
                                break;
                            }

                            case 0xA1 when descriptor.ADR == 4:
                                sb.AppendFormat(Localization.Last_video_track_number_0, descriptor.PMIN).AppendLine();

                                break;
                            case 0xA1 when descriptor.ADR == 1:
                            {
                                sb.AppendFormat(Localization.Last_track_number_0_open_parenthesis, descriptor.PMIN);

                                switch((TocControl)(descriptor.CONTROL & 0x0D))
                                {
                                    case TocControl.TwoChanNoPreEmph:
                                        sb.Append(Localization.Stereo_audio_track_with_no_pre_emphasis);

                                        break;
                                    case TocControl.TwoChanPreEmph:
                                        sb.Append(Localization.Stereo_audio_track_with_50_15_us_pre_emphasis);

                                        break;
                                    case TocControl.FourChanNoPreEmph:
                                        sb.Append(Localization.Quadraphonic_audio_track_with_no_pre_emphasis);

                                        break;
                                    case TocControl.FourChanPreEmph:
                                        sb.Append(Localization.Quadraphonic_audio_track_with_50_15_us_pre_emphasis);

                                        break;
                                    case TocControl.DataTrack:
                                        sb.Append(Localization.Data_track_recorded_uninterrupted);

                                        break;
                                    case TocControl.DataTrackIncremental:
                                        sb.Append(Localization.Data_track_recorded_incrementally);

                                        break;
                                }

                                sb.AppendLine(Localization.close_parenthesis);

                                //sb.AppendFormat("Absolute time: {3:D2}:{0:D2}:{1:D2}:{2:D2}", descriptor.Min, descriptor.Sec, descriptor.Frame, descriptor.HOUR).AppendLine();
                                break;
                            }

                            case 0xA2:
                            {
                                if(descriptor.PHOUR > 0)
                                {
                                    sb.AppendFormat(Localization.Lead_out_start_position_3_0_1_2,
                                                    descriptor.PMIN,
                                                    descriptor.PSEC,
                                                    descriptor.PFRAME,
                                                    descriptor.PHOUR)
                                      .AppendLine();
                                }
                                else
                                {
                                    sb.AppendFormat(Localization.Lead_out_start_position_0_1_2,
                                                    descriptor.PMIN,
                                                    descriptor.PSEC,
                                                    descriptor.PFRAME)
                                      .AppendLine();
                                }

                                //sb.AppendFormat("Absolute time: {3:D2}:{0:D2}:{1:D2}:{2:D2}", descriptor.Min, descriptor.Sec, descriptor.Frame, descriptor.HOUR).AppendLine();

                                switch((TocControl)(descriptor.CONTROL & 0x0D))
                                {
                                    case TocControl.TwoChanNoPreEmph:
                                    case TocControl.TwoChanPreEmph:
                                    case TocControl.FourChanNoPreEmph:
                                    case TocControl.FourChanPreEmph:
                                        sb.AppendLine(Localization.Lead_out_is_audio_type);

                                        break;
                                    case TocControl.DataTrack:
                                    case TocControl.DataTrackIncremental:
                                        sb.AppendLine(Localization.Lead_out_is_data_type);

                                        break;
                                }

                                break;
                            }

                            case 0xF0:
                            {
                                sb.AppendFormat(Localization.Book_type_0,         descriptor.PMIN);
                                sb.AppendFormat(Localization.Material_type_0,     descriptor.PSEC);
                                sb.AppendFormat(Localization.Moment_of_inertia_0, descriptor.PFRAME);

                                if(descriptor.PHOUR > 0)
                                {
                                    sb.AppendFormat(Localization.Absolute_time_3_0_1_2,
                                                    descriptor.Min,
                                                    descriptor.Sec,
                                                    descriptor.Frame,
                                                    descriptor.HOUR)
                                      .AppendLine();
                                }
                                else
                                {
                                    sb.AppendFormat(Localization.Absolute_time_0_1_2,
                                                    descriptor.Min,
                                                    descriptor.Sec,
                                                    descriptor.Frame)
                                      .AppendLine();
                                }

                                break;
                            }

                            default:
                            {
                                if(descriptor.POINT is >= 0x01 and <= 0x63)
                                {
                                    if(descriptor.ADR == 4)
                                    {
                                        sb.AppendFormat(Localization.Video_track_3_starts_at_0_1_2,
                                                        descriptor.PMIN,
                                                        descriptor.PSEC,
                                                        descriptor.PFRAME,
                                                        descriptor.POINT)
                                          .AppendLine();
                                    }
                                    else
                                    {
                                        bool data = (TocControl)(descriptor.CONTROL & 0x0D) == TocControl.DataTrack ||
                                                    (TocControl)(descriptor.CONTROL & 0x0D) ==
                                                    TocControl.DataTrackIncremental;

                                        if(descriptor.PHOUR > 0)
                                        {
                                            sb.AppendFormat(data
                                                                ? Localization
                                                                   .Data_track_3_starts_at_4_0_1_2_open_parenthesis
                                                                : Localization
                                                                   .Audio_track_3_starts_at_4_0_1_2_open_parenthesis,
                                                            descriptor.PMIN,
                                                            descriptor.PSEC,
                                                            descriptor.PFRAME,
                                                            descriptor.POINT,
                                                            descriptor.PHOUR);
                                        }

                                        else
                                        {
                                            sb.AppendFormat(data
                                                                ? Localization
                                                                   .Data_track_3_starts_at_0_1_2_open_parenthesis
                                                                : Localization
                                                                   .Audio_track_3_starts_at_0_1_2_open_parenthesis,
                                                            descriptor.PMIN,
                                                            descriptor.PSEC,
                                                            descriptor.PFRAME,
                                                            descriptor.POINT);
                                        }

                                        switch((TocControl)(descriptor.CONTROL & 0x0D))
                                        {
                                            case TocControl.TwoChanNoPreEmph:
                                                sb.Append(Localization.Stereo_audio_track_with_no_pre_emphasis);

                                                break;
                                            case TocControl.TwoChanPreEmph:
                                                sb.Append(Localization.Stereo_audio_track_with_50_15_us_pre_emphasis);

                                                break;
                                            case TocControl.FourChanNoPreEmph:
                                                sb.Append(Localization.Quadraphonic_audio_track_with_no_pre_emphasis);

                                                break;
                                            case TocControl.FourChanPreEmph:
                                                sb.Append(Localization
                                                             .Quadraphonic_audio_track_with_50_15_us_pre_emphasis);

                                                break;
                                            case TocControl.DataTrack:
                                                sb.Append(Localization.Data_track_recorded_uninterrupted);

                                                break;
                                            case TocControl.DataTrackIncremental:
                                                sb.Append(Localization.Data_track_recorded_incrementally);

                                                break;
                                        }

                                        sb.AppendLine(Localization.close_parenthesis);
                                    }
                                }
                                else
                                {
                                    sb.Append($"ADR = {descriptor.ADR}").AppendLine();
                                    sb.Append($"CONTROL = {descriptor.CONTROL}").AppendLine();
                                    sb.Append($"TNO = {descriptor.TNO}").AppendLine();
                                    sb.Append($"POINT = {descriptor.POINT}").AppendLine();
                                    sb.Append($"Min = {descriptor.Min}").AppendLine();
                                    sb.Append($"Sec = {descriptor.Sec}").AppendLine();
                                    sb.Append($"Frame = {descriptor.Frame}").AppendLine();
                                    sb.Append($"HOUR = {descriptor.HOUR}").AppendLine();
                                    sb.Append($"PHOUR = {descriptor.PHOUR}").AppendLine();
                                    sb.Append($"PMIN = {descriptor.PMIN}").AppendLine();
                                    sb.Append($"PSEC = {descriptor.PSEC}").AppendLine();
                                    sb.Append($"PFRAME = {descriptor.PFRAME}").AppendLine();
                                }

                                break;
                            }
                        }

                        break;
                    }

                    case 5:
                    {
                        switch(descriptor.POINT)
                        {
                            case 0xB0:
                            {
                                if(descriptor.PHOUR > 0)
                                {
                                    sb.AppendFormat(Localization
                                                       .Start_of_next_possible_program_in_the_recordable_area_of_the_disc_3_0_1_2,
                                                    descriptor.Min,
                                                    descriptor.Sec,
                                                    descriptor.Frame,
                                                    descriptor.HOUR)
                                      .AppendLine();

                                    sb.AppendFormat(Localization
                                                       .Maximum_start_of_outermost_Lead_out_in_the_recordable_area_of_the_disc_3_0_1_2,
                                                    descriptor.PMIN,
                                                    descriptor.PSEC,
                                                    descriptor.PFRAME,
                                                    descriptor.PHOUR)
                                      .AppendLine();
                                }
                                else
                                {
                                    sb.AppendFormat(Localization
                                                       .Start_of_next_possible_program_in_the_recordable_area_of_the_disc_0_1_2,
                                                    descriptor.Min,
                                                    descriptor.Sec,
                                                    descriptor.Frame)
                                      .AppendLine();

                                    sb.AppendFormat(Localization
                                                       .Maximum_start_of_outermost_Lead_out_in_the_recordable_area_of_the_disc_0_1_2,
                                                    descriptor.PMIN,
                                                    descriptor.PSEC,
                                                    descriptor.PFRAME)
                                      .AppendLine();
                                }

                                break;
                            }

                            case 0xB1:
                            {
                                sb.AppendFormat(Localization.Number_of_skip_interval_pointers_0, descriptor.PMIN)
                                  .AppendLine();

                                sb.AppendFormat(Localization.Number_of_skip_track_pointers_0, descriptor.PSEC)
                                  .AppendLine();

                                break;
                            }

                            case 0xB2:
                            case 0xB3:
                            case 0xB4:
                            {
                                sb.AppendFormat(Localization.Skip_track_0, descriptor.Min).AppendLine();
                                sb.AppendFormat(Localization.Skip_track_0, descriptor.Sec).AppendLine();
                                sb.AppendFormat(Localization.Skip_track_0, descriptor.Frame).AppendLine();
                                sb.AppendFormat(Localization.Skip_track_0, descriptor.Zero).AppendLine();
                                sb.AppendFormat(Localization.Skip_track_0, descriptor.PMIN).AppendLine();
                                sb.AppendFormat(Localization.Skip_track_0, descriptor.PSEC).AppendLine();
                                sb.AppendFormat(Localization.Skip_track_0, descriptor.PFRAME).AppendLine();

                                break;
                            }

                            case 0xC0:
                            {
                                sb.AppendFormat(Localization.Optimum_recording_power_0, descriptor.Min).AppendLine();

                                if(descriptor.PHOUR > 0)
                                {
                                    sb.AppendFormat(Localization
                                                       .Start_time_of_the_first_Lead_in_area_in_the_disc_3_0_1_2,
                                                    descriptor.PMIN,
                                                    descriptor.PSEC,
                                                    descriptor.PFRAME,
                                                    descriptor.PHOUR)
                                      .AppendLine();
                                }
                                else
                                {
                                    sb.AppendFormat(Localization.Start_time_of_the_first_Lead_in_area_in_the_disc_0_1_2,
                                                    descriptor.PMIN,
                                                    descriptor.PSEC,
                                                    descriptor.PFRAME)
                                      .AppendLine();
                                }

                                break;
                            }

                            case 0xC1:
                            {
                                sb.AppendFormat(Localization.Copy_of_information_of_A1_from_ATIP_found);
                                sb.Append($"Min = {descriptor.Min}").AppendLine();
                                sb.Append($"Sec = {descriptor.Sec}").AppendLine();
                                sb.Append($"Frame = {descriptor.Frame}").AppendLine();
                                sb.Append($"Zero = {descriptor.Zero}").AppendLine();
                                sb.Append($"PMIN = {descriptor.PMIN}").AppendLine();
                                sb.Append($"PSEC = {descriptor.PSEC}").AppendLine();
                                sb.Append($"PFRAME = {descriptor.PFRAME}").AppendLine();

                                break;
                            }

                            case 0xCF:
                            {
                                if(descriptor.PHOUR > 0)
                                {
                                    sb.AppendFormat(Localization.Start_position_of_outer_part_lead_in_area_3_0_1_2,
                                                    descriptor.PMIN,
                                                    descriptor.PSEC,
                                                    descriptor.PFRAME,
                                                    descriptor.PHOUR)
                                      .AppendLine();

                                    sb.AppendFormat(Localization.Stop_position_of_inner_part_lead_out_area_3_0_1_2,
                                                    descriptor.Min,
                                                    descriptor.Sec,
                                                    descriptor.Frame,
                                                    descriptor.HOUR)
                                      .AppendLine();
                                }
                                else
                                {
                                    sb.AppendFormat(Localization.Start_position_of_outer_part_lead_in_area_0_1_2,
                                                    descriptor.PMIN,
                                                    descriptor.PSEC,
                                                    descriptor.PFRAME)
                                      .AppendLine();

                                    sb.AppendFormat(Localization.Stop_position_of_inner_part_lead_out_area_0_1_2,
                                                    descriptor.Min,
                                                    descriptor.Sec,
                                                    descriptor.Frame)
                                      .AppendLine();
                                }

                                break;
                            }

                            default:
                            {
                                if(descriptor.POINT is >= 0x01 and <= 0x40)
                                {
                                    sb.AppendFormat(Localization.Start_time_for_interval_that_should_be_skipped_0_1_2,
                                                    descriptor.PMIN,
                                                    descriptor.PSEC,
                                                    descriptor.PFRAME)
                                      .AppendLine();

                                    sb.AppendFormat(Localization.Ending_time_for_interval_that_should_be_skipped_0_1_2,
                                                    descriptor.Min,
                                                    descriptor.Sec,
                                                    descriptor.Frame)
                                      .AppendLine();
                                }
                                else
                                {
                                    sb.Append($"ADR = {descriptor.ADR}").AppendLine();
                                    sb.Append($"CONTROL = {descriptor.CONTROL}").AppendLine();
                                    sb.Append($"TNO = {descriptor.TNO}").AppendLine();
                                    sb.Append($"POINT = {descriptor.POINT}").AppendLine();
                                    sb.Append($"Min = {descriptor.Min}").AppendLine();
                                    sb.Append($"Sec = {descriptor.Sec}").AppendLine();
                                    sb.Append($"Frame = {descriptor.Frame}").AppendLine();
                                    sb.Append($"HOUR = {descriptor.HOUR}").AppendLine();
                                    sb.Append($"PHOUR = {descriptor.PHOUR}").AppendLine();
                                    sb.Append($"PMIN = {descriptor.PMIN}").AppendLine();
                                    sb.Append($"PSEC = {descriptor.PSEC}").AppendLine();
                                    sb.Append($"PFRAME = {descriptor.PFRAME}").AppendLine();
                                }

                                break;
                            }
                        }

                        break;
                    }

                    case 6:
                    {
                        var id = (uint)((descriptor.Min << 16) + (descriptor.Sec << 8) + descriptor.Frame);
                        sb.AppendFormat(Localization.Disc_ID_0_X6, id & 0x00FFFFFF).AppendLine();

                        break;
                    }
                }
            }
        }

        return sb.ToString();
    }

    public static string Prettify(byte[] CDFullTOCResponse)
    {
        CDFullTOC? decoded = Decode(CDFullTOCResponse);

        return Prettify(decoded);
    }

    public static CDFullTOC Create(List<Track> tracks, Dictionary<byte, byte> trackFlags, bool createC0Entry = false)
    {
        var                    toc                = new CDFullTOC();
        Dictionary<byte, byte> sessionEndingTrack = new();
        toc.FirstCompleteSession = byte.MaxValue;
        toc.LastCompleteSession  = byte.MinValue;
        List<TrackDataDescriptor> trackDescriptors = [];
        byte                      currentTrack     = 0;

        foreach(Track track in tracks.OrderBy(t => t.Session).ThenBy(t => t.Sequence))
        {
            if(track.Session < toc.FirstCompleteSession) toc.FirstCompleteSession = (byte)track.Session;

            if(track.Session <= toc.LastCompleteSession)
            {
                currentTrack = (byte)track.Sequence;

                continue;
            }

            if(toc.LastCompleteSession > 0) sessionEndingTrack.Add(toc.LastCompleteSession, currentTrack);

            toc.LastCompleteSession = (byte)track.Session;
        }

        sessionEndingTrack.TryAdd(toc.LastCompleteSession,
                                  (byte)tracks.Where(t => t.Session == toc.LastCompleteSession).Max(t => t.Sequence));

        byte currentSession = 0;

        foreach(Track track in tracks.OrderBy(t => t.Session).ThenBy(t => t.Sequence))
        {
            trackFlags.TryGetValue((byte)track.Sequence, out byte trackControl);

            if(trackControl == 0 && track.Type != TrackType.Audio) trackControl = (byte)CdFlags.DataTrack;

            // Lead-Out
            if(track.Session > currentSession && currentSession != 0)
            {
                (byte minute, byte second, byte frame) leadoutAmsf = LbaToMsf(track.StartSector - 150);

                (byte minute, byte second, byte frame) leadoutPmsf =
                    LbaToMsf(tracks.OrderBy(t => t.Session).ThenBy(t => t.Sequence).Last().StartSector);

                // Lead-out
                trackDescriptors.Add(new TrackDataDescriptor
                {
                    SessionNumber = currentSession,
                    POINT         = 0xB0,
                    ADR           = 5,
                    CONTROL       = 0,
                    HOUR          = 0,
                    Min           = leadoutAmsf.minute,
                    Sec           = leadoutAmsf.second,
                    Frame         = leadoutAmsf.frame,
                    PHOUR         = 2,
                    PMIN          = leadoutPmsf.minute,
                    PSEC          = leadoutPmsf.second,
                    PFRAME        = leadoutPmsf.frame
                });

                // This seems to be constant? It should not exist on CD-ROM but CloneCD creates them anyway
                // Format seems like ATIP, but ATIP should not be as 0xC0 in TOC...
                if(createC0Entry)
                {
                    trackDescriptors.Add(new TrackDataDescriptor
                    {
                        SessionNumber = currentSession,
                        POINT         = 0xC0,
                        ADR           = 5,
                        CONTROL       = 0,
                        Min           = 128,
                        PMIN          = 97,
                        PSEC          = 25
                    });
                }
            }

            // Lead-in
            if(track.Session > currentSession)
            {
                currentSession = (byte)track.Session;
                sessionEndingTrack.TryGetValue(currentSession, out byte endingTrackNumber);

                (byte minute, byte second, byte frame) leadinPmsf =
                    LbaToMsf((tracks.FirstOrDefault(t => t.Sequence == endingTrackNumber)?.EndSector ?? 0) + 1);

                // Starting track
                trackDescriptors.Add(new TrackDataDescriptor
                {
                    SessionNumber = currentSession,
                    POINT         = 0xA0,
                    ADR           = 1,
                    CONTROL       = trackControl,
                    PMIN          = (byte)track.Sequence
                });

                // Ending track
                trackDescriptors.Add(new TrackDataDescriptor
                {
                    SessionNumber = currentSession,
                    POINT         = 0xA1,
                    ADR           = 1,
                    CONTROL       = trackControl,
                    PMIN          = endingTrackNumber
                });

                // Lead-out start
                trackDescriptors.Add(new TrackDataDescriptor
                {
                    SessionNumber = currentSession,
                    POINT         = 0xA2,
                    ADR           = 1,
                    CONTROL       = trackControl,
                    PHOUR         = 0,
                    PMIN          = leadinPmsf.minute,
                    PSEC          = leadinPmsf.second,
                    PFRAME        = leadinPmsf.frame
                });
            }

            (byte minute, byte second, byte frame) pmsf = LbaToMsf((ulong)track.Indexes[1]);

            // Track
            trackDescriptors.Add(new TrackDataDescriptor
            {
                SessionNumber = (byte)track.Session,
                POINT         = (byte)track.Sequence,
                ADR           = 1,
                CONTROL       = trackControl,
                PHOUR         = 0,
                PMIN          = pmsf.minute,
                PSEC          = pmsf.second,
                PFRAME        = pmsf.frame
            });
        }

        toc.TrackDescriptors = trackDescriptors.ToArray();

        return toc;
    }

    static (byte minute, byte second, byte frame) LbaToMsf(ulong sector) =>
        ((byte)((sector + 150) / 75 / 60), (byte)((sector + 150) / 75 % 60), (byte)((sector + 150) % 75));

#region Nested type: CDFullTOC

    public struct CDFullTOC
    {
        /// <summary>Total size of returned session information minus this field</summary>
        public ushort DataLength;
        /// <summary>First complete session number in hex</summary>
        public byte FirstCompleteSession;
        /// <summary>Last complete session number in hex</summary>
        public byte LastCompleteSession;
        /// <summary>Track descriptors</summary>
        public TrackDataDescriptor[] TrackDescriptors;
    }

#endregion

#region Nested type: TrackDataDescriptor

    public struct TrackDataDescriptor
    {
        /// <summary>Byte 0 Session number in hex</summary>
        public byte SessionNumber;
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
        /// <summary>Byte 7, CD only</summary>
        public byte Zero;
        /// <summary>Byte 7, bits 7 to 4, DDCD only</summary>
        public byte HOUR;
        /// <summary>Byte 7, bits 3 to 0, DDCD only</summary>
        public byte PHOUR;
        /// <summary>Byte 8</summary>
        public byte PMIN;
        /// <summary>Byte 9</summary>
        public byte PSEC;
        /// <summary>Byte 10</summary>
        public byte PFRAME;
    }

#endregion
}