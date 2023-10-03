// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Session.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes CD session structures.
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
// T10/1675-D revision 4 T10/1836-D revision 2g
[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class Session
{
    const string MODULE_NAME = "CD Session Info decoder";

    public static CDSessionInfo? Decode(byte[] CDSessionInfoResponse)
    {
        if(CDSessionInfoResponse is not { Length: > 4 })
            return null;

        var decoded = new CDSessionInfo
        {
            DataLength           = BigEndianBitConverter.ToUInt16(CDSessionInfoResponse, 0),
            FirstCompleteSession = CDSessionInfoResponse[2],
            LastCompleteSession  = CDSessionInfoResponse[3]
        };

        decoded.TrackDescriptors = new TrackDataDescriptor[(decoded.DataLength - 2) / 8];

        if(decoded.DataLength + 2 != CDSessionInfoResponse.Length)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       Localization.
                                           Expected_CDSessionInfo_size_0_bytes_is_not_received_size_1_bytes_not_decoding,
                                       decoded.DataLength + 2, CDSessionInfoResponse.Length);

            return null;
        }

        for(int i = 0; i < (decoded.DataLength - 2) / 8; i++)
        {
            decoded.TrackDescriptors[i].Reserved1   = CDSessionInfoResponse[0 + (i * 8) + 4];
            decoded.TrackDescriptors[i].ADR         = (byte)((CDSessionInfoResponse[1 + (i * 8) + 4] & 0xF0) >> 4);
            decoded.TrackDescriptors[i].CONTROL     = (byte)(CDSessionInfoResponse[1 + (i * 8) + 4] & 0x0F);
            decoded.TrackDescriptors[i].TrackNumber = CDSessionInfoResponse[2 + (i * 8) + 4];
            decoded.TrackDescriptors[i].Reserved2   = CDSessionInfoResponse[3 + (i * 8) + 4];

            decoded.TrackDescriptors[i].TrackStartAddress =
                BigEndianBitConverter.ToUInt32(CDSessionInfoResponse, 4 + (i * 8) + 4);
        }

        return decoded;
    }

    public static string Prettify(CDSessionInfo? CDSessionInfoResponse)
    {
        if(CDSessionInfoResponse == null)
            return null;

        CDSessionInfo response = CDSessionInfoResponse.Value;

        var sb = new StringBuilder();

        sb.AppendFormat(Localization.First_complete_session_number_0, response.FirstCompleteSession).AppendLine();
        sb.AppendFormat(Localization.Last_complete_session_number_0, response.LastCompleteSession).AppendLine();

        foreach(TrackDataDescriptor descriptor in response.TrackDescriptors)
        {
            sb.AppendFormat(Localization.First_track_number_in_last_complete_session_0, descriptor.TrackNumber).
               AppendLine();

            sb.AppendFormat(Localization.Track_starts_at_LBA_0_or_MSF_2_3, descriptor.TrackStartAddress,
                            (descriptor.TrackStartAddress & 0x0000FF00) >> 8,
                            (descriptor.TrackStartAddress & 0x00FF0000) >> 16,
                            (descriptor.TrackStartAddress & 0xFF000000) >> 24).AppendLine();

            switch((TocAdr)descriptor.ADR)
            {
                case TocAdr.NoInformation:
                    sb.AppendLine(Localization.Q_subchannel_mode_not_given);

                    break;
                case TocAdr.CurrentPosition:
                    sb.AppendLine(Localization.Q_subchannel_stores_current_position);

                    break;
                case TocAdr.ISRC:
                    sb.AppendLine(Localization.Q_subchannel_stores_ISRC);

                    break;
                case TocAdr.MediaCatalogNumber:
                    sb.AppendLine(Localization.Q_subchannel_stores_media_catalog_number);

                    break;
            }

            if((descriptor.CONTROL & (byte)TocControl.ReservedMask) == (byte)TocControl.ReservedMask)
                sb.AppendFormat(Localization.Reserved_flags_0_set, descriptor.CONTROL).AppendLine();
            else
            {
                switch((TocControl)(descriptor.CONTROL & 0x0D))
                {
                    case TocControl.TwoChanNoPreEmph:
                        sb.AppendLine(Localization.Stereo_audio_track_with_no_pre_emphasis);

                        break;
                    case TocControl.TwoChanPreEmph:
                        sb.AppendLine(Localization.Stereo_audio_track_with_50_15_us_pre_emphasis);

                        break;
                    case TocControl.FourChanNoPreEmph:
                        sb.AppendLine(Localization.Quadraphonic_audio_track_with_no_pre_emphasis);

                        break;
                    case TocControl.FourChanPreEmph:
                        sb.AppendLine(Localization.Stereo_audio_track_with_50_15_us_pre_emphasis);

                        break;
                    case TocControl.DataTrack:
                        sb.AppendLine(Localization.Data_track_recorded_uninterrupted);

                        break;
                    case TocControl.DataTrackIncremental:
                        sb.AppendLine(Localization.Data_track_recorded_incrementally);

                        break;
                }

                sb.AppendLine((descriptor.CONTROL & (byte)TocControl.CopyPermissionMask) ==
                              (byte)TocControl.CopyPermissionMask ? Localization.Digital_copy_of_track_is_permitted
                                  : Localization.Digital_copy_of_track_is_prohibited);

            #if DEBUG
                if(descriptor.Reserved1 != 0)
                    sb.AppendFormat(Localization.Reserved1_equals_0_X8, descriptor.Reserved1).AppendLine();

                if(descriptor.Reserved2 != 0)
                    sb.AppendFormat(Localization.Reserved2_equals_0_X8, descriptor.Reserved2).AppendLine();
            #endif

                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    public static string Prettify(byte[] CDSessionInfoResponse)
    {
        CDSessionInfo? decoded = Decode(CDSessionInfoResponse);

        return Prettify(decoded);
    }

    public struct CDSessionInfo
    {
        /// <summary>Total size of returned session information minus this field</summary>
        public ushort DataLength;
        /// <summary>First track number in hex</summary>
        public byte FirstCompleteSession;
        /// <summary>Last track number in hex</summary>
        public byte LastCompleteSession;
        /// <summary>Track descriptors</summary>
        public TrackDataDescriptor[] TrackDescriptors;
    }

    public struct TrackDataDescriptor
    {
        /// <summary>Byte 0 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 1, bits 7 to 4 Type of information in Q subchannel of block where this TOC entry was found</summary>
        public byte ADR;
        /// <summary>Byte 1, bits 3 to 0 Track attributes</summary>
        public byte CONTROL;
        /// <summary>Byte 2 First track number in last complete session</summary>
        public byte TrackNumber;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Bytes 4 to 7 First track number in last complete session start address in LBA or in MSF</summary>
        public uint TrackStartAddress;
    }
}