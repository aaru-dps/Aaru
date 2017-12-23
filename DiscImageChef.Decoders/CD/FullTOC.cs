// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using DiscImageChef.Console;

namespace DiscImageChef.Decoders.CD
{
    /// <summary>
    ///     Information from the following standards:
    ///     ANSI X3.304-1997
    ///     T10/1048-D revision 9.0
    ///     T10/1048-D revision 10a
    ///     T10/1228-D revision 7.0c
    ///     T10/1228-D revision 11a
    ///     T10/1363-D revision 10g
    ///     T10/1545-D revision 1d
    ///     T10/1545-D revision 5
    ///     T10/1545-D revision 5a
    ///     T10/1675-D revision 2c
    ///     T10/1675-D revision 4
    ///     T10/1836-D revision 2g
    ///     ISO/IEC 61104: Compact disc video system - 12 cm CD-V
    ///     ISO/IEC 60908: Audio recording - Compact disc digital audio system
    /// </summary>
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "MemberCanBeInternal")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class FullTOC
    {
        const string StereoNoPre = "Stereo audio track with no pre-emphasis";
        const string StereoPreEm = "Stereo audio track with 50/15 μs pre-emphasis";
        const string QuadNoPreEm = "Quadraphonic audio track with no pre-emphasis";
        const string QuadPreEmph = "Quadraphonic audio track with 50/15 μs pre-emphasis";
        const string DataUnintrp = "Data track, recorded uninterrupted";
        const string DataIncrtly = "Data track, recorded incrementally";

        public struct CDFullTOC
        {
            /// <summary>
            ///     Total size of returned session information minus this field
            /// </summary>
            public ushort DataLength;
            /// <summary>
            ///     First complete session number in hex
            /// </summary>
            public byte FirstCompleteSession;
            /// <summary>
            ///     Last complete session number in hex
            /// </summary>
            public byte LastCompleteSession;
            /// <summary>
            ///     Track descriptors
            /// </summary>
            public TrackDataDescriptor[] TrackDescriptors;
        }

        public struct TrackDataDescriptor
        {
            /// <summary>
            ///     Byte 0
            ///     Session number in hex
            /// </summary>
            public byte SessionNumber;
            /// <summary>
            ///     Byte 1, bits 7 to 4
            ///     Type of information in Q subchannel of block where this TOC entry was found
            /// </summary>
            public byte ADR;
            /// <summary>
            ///     Byte 1, bits 3 to 0
            ///     Track attributes
            /// </summary>
            public byte CONTROL;
            /// <summary>
            ///     Byte 2
            /// </summary>
            public byte TNO;
            /// <summary>
            ///     Byte 3
            /// </summary>
            public byte POINT;
            /// <summary>
            ///     Byte 4
            /// </summary>
            public byte Min;
            /// <summary>
            ///     Byte 5
            /// </summary>
            public byte Sec;
            /// <summary>
            ///     Byte 6
            /// </summary>
            public byte Frame;
            /// <summary>
            ///     Byte 7, CD only
            /// </summary>
            public byte Zero;
            /// <summary>
            ///     Byte 7, bits 7 to 4, DDCD only
            /// </summary>
            public byte HOUR;
            /// <summary>
            ///     Byte 7, bits 3 to 0, DDCD only
            /// </summary>
            public byte PHOUR;
            /// <summary>
            ///     Byte 8
            /// </summary>
            public byte PMIN;
            /// <summary>
            ///     Byte 9
            /// </summary>
            public byte PSEC;
            /// <summary>
            ///     Byte 10
            /// </summary>
            public byte PFRAME;
        }

        public static CDFullTOC? Decode(byte[] CDFullTOCResponse)
        {
            if(CDFullTOCResponse == null) return null;

            CDFullTOC decoded = new CDFullTOC();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.DataLength = BigEndianBitConverter.ToUInt16(CDFullTOCResponse, 0);
            decoded.FirstCompleteSession = CDFullTOCResponse[2];
            decoded.LastCompleteSession = CDFullTOCResponse[3];
            decoded.TrackDescriptors = new TrackDataDescriptor[(decoded.DataLength - 2) / 11];

            if(decoded.DataLength + 2 != CDFullTOCResponse.Length)
            {
                DicConsole.DebugWriteLine("CD full TOC decoder",
                                          "Expected CDFullTOC size ({0} bytes) is not received size ({1} bytes), not decoding",
                                          decoded.DataLength + 2, CDFullTOCResponse.Length);
                return null;
            }

            for(int i = 0; i < (decoded.DataLength - 2) / 11; i++)
            {
                decoded.TrackDescriptors[i].SessionNumber = CDFullTOCResponse[0 + i * 11 + 4];
                decoded.TrackDescriptors[i].ADR = (byte)((CDFullTOCResponse[1 + i * 11 + 4] & 0xF0) >> 4);
                decoded.TrackDescriptors[i].CONTROL = (byte)(CDFullTOCResponse[1 + i * 11 + 4] & 0x0F);
                decoded.TrackDescriptors[i].TNO = CDFullTOCResponse[2 + i * 11 + 4];
                decoded.TrackDescriptors[i].POINT = CDFullTOCResponse[3 + i * 11 + 4];
                decoded.TrackDescriptors[i].Min = CDFullTOCResponse[4 + i * 11 + 4];
                decoded.TrackDescriptors[i].Sec = CDFullTOCResponse[5 + i * 11 + 4];
                decoded.TrackDescriptors[i].Frame = CDFullTOCResponse[6 + i * 11 + 4];
                decoded.TrackDescriptors[i].Zero = CDFullTOCResponse[7 + i * 11 + 4];
                decoded.TrackDescriptors[i].HOUR = (byte)((CDFullTOCResponse[7 + i * 11 + 4] & 0xF0) >> 4);
                decoded.TrackDescriptors[i].PHOUR = (byte)(CDFullTOCResponse[7 + i * 11 + 4] & 0x0F);
                decoded.TrackDescriptors[i].PMIN = CDFullTOCResponse[8 + i * 11 + 4];
                decoded.TrackDescriptors[i].PSEC = CDFullTOCResponse[9 + i * 11 + 4];
                decoded.TrackDescriptors[i].PFRAME = CDFullTOCResponse[10 + i * 11 + 4];
            }

            return decoded;
        }

        public static string Prettify(CDFullTOC? CDFullTOCResponse)
        {
            if(CDFullTOCResponse == null) return null;

            CDFullTOC response = CDFullTOCResponse.Value;

            StringBuilder sb = new StringBuilder();

            int lastSession = 0;

            sb.AppendFormat("First complete session number: {0}", response.FirstCompleteSession).AppendLine();
            sb.AppendFormat("Last complete session number: {0}", response.LastCompleteSession).AppendLine();
            foreach(TrackDataDescriptor descriptor in response.TrackDescriptors)
                if((descriptor.CONTROL & 0x08) == 0x08 ||
                   descriptor.ADR != 1 && descriptor.ADR != 5 && descriptor.ADR != 4 && descriptor.ADR != 6 ||
                   descriptor.TNO != 0)
                {
                    sb.AppendLine("Unknown TOC entry format, printing values as-is");
                    sb.AppendFormat("SessionNumber = {0}", descriptor.SessionNumber).AppendLine();
                    sb.AppendFormat("ADR = {0}", descriptor.ADR).AppendLine();
                    sb.AppendFormat("CONTROL = {0}", descriptor.CONTROL).AppendLine();
                    sb.AppendFormat("TNO = {0}", descriptor.TNO).AppendLine();
                    sb.AppendFormat("POINT = {0}", descriptor.POINT).AppendLine();
                    sb.AppendFormat("Min = {0}", descriptor.Min).AppendLine();
                    sb.AppendFormat("Sec = {0}", descriptor.Sec).AppendLine();
                    sb.AppendFormat("Frame = {0}", descriptor.Frame).AppendLine();
                    sb.AppendFormat("HOUR = {0}", descriptor.HOUR).AppendLine();
                    sb.AppendFormat("PHOUR = {0}", descriptor.PHOUR).AppendLine();
                    sb.AppendFormat("PMIN = {0}", descriptor.PMIN).AppendLine();
                    sb.AppendFormat("PSEC = {0}", descriptor.PSEC).AppendLine();
                    sb.AppendFormat("PFRAME = {0}", descriptor.PFRAME).AppendLine();
                }
                else
                {
                    if(descriptor.SessionNumber > lastSession)
                    {
                        sb.AppendFormat("Session {0}", descriptor.SessionNumber).AppendLine();
                        lastSession = descriptor.SessionNumber;
                    }

                    switch(descriptor.ADR)
                    {
                        case 1:
                        case 4:
                        {
                            switch(descriptor.POINT)
                            {
                                case 0xA0:
                                {
                                    if(descriptor.ADR == 4)
                                    {
                                        sb.AppendFormat("First video track number: {0}", descriptor.PMIN).AppendLine();
                                        switch(descriptor.PSEC)
                                        {
                                            case 0x10:
                                                sb.AppendLine("CD-V single in NTSC format with digital stereo sound");
                                                break;
                                            case 0x11:
                                                sb.AppendLine("CD-V single in NTSC format with digital bilingual sound");
                                                break;
                                            case 0x12:
                                                sb.AppendLine("CD-V disc in NTSC format with digital stereo sound");
                                                break;
                                            case 0x13:
                                                sb.AppendLine("CD-V disc in NTSC format with digital bilingual sound");
                                                break;
                                            case 0x20:
                                                sb.AppendLine("CD-V single in PAL format with digital stereo sound");
                                                break;
                                            case 0x21:
                                                sb.AppendLine("CD-V single in PAL format with digital bilingual sound");
                                                break;
                                            case 0x22:
                                                sb.AppendLine("CD-V disc in PAL format with digital stereo sound");
                                                break;
                                            case 0x23:
                                                sb.AppendLine("CD-V disc in PAL format with digital bilingual sound");
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        sb.AppendFormat("First track number: {0} (", descriptor.PMIN);
                                        switch((TocControl)(descriptor.CONTROL & 0x0D))
                                        {
                                            case TocControl.TwoChanNoPreEmph:
                                                sb.Append(StereoNoPre);
                                                break;
                                            case TocControl.TwoChanPreEmph:
                                                sb.Append(StereoPreEm);
                                                break;
                                            case TocControl.FourChanNoPreEmph:
                                                sb.Append(QuadNoPreEm);
                                                break;
                                            case TocControl.FourChanPreEmph:
                                                sb.Append(QuadPreEmph);
                                                break;
                                            case TocControl.DataTrack:
                                                sb.Append(DataUnintrp);
                                                break;
                                            case TocControl.DataTrackIncremental:
                                                sb.Append(DataIncrtly);
                                                break;
                                        }

                                        sb.AppendLine(")");
                                        sb.AppendFormat("Disc type: {0}", descriptor.PSEC).AppendLine();
                                        //sb.AppendFormat("Absolute time: {3:D2}:{0:D2}:{1:D2}:{2:D2}", descriptor.Min, descriptor.Sec, descriptor.Frame, descriptor.HOUR).AppendLine();
                                    }

                                    break;
                                }
                                case 0xA1:
                                {
                                    if(descriptor.ADR == 4)
                                        sb.AppendFormat("Last video track number: {0}", descriptor.PMIN).AppendLine();
                                    else
                                    {
                                        sb.AppendFormat("Last track number: {0} (", descriptor.PMIN);
                                        switch((TocControl)(descriptor.CONTROL & 0x0D))
                                        {
                                            case TocControl.TwoChanNoPreEmph:
                                                sb.Append(StereoNoPre);
                                                break;
                                            case TocControl.TwoChanPreEmph:
                                                sb.Append(StereoPreEm);
                                                break;
                                            case TocControl.FourChanNoPreEmph:
                                                sb.Append(QuadNoPreEm);
                                                break;
                                            case TocControl.FourChanPreEmph:
                                                sb.Append(QuadPreEmph);
                                                break;
                                            case TocControl.DataTrack:
                                                sb.Append(DataUnintrp);
                                                break;
                                            case TocControl.DataTrackIncremental:
                                                sb.Append(DataIncrtly);
                                                break;
                                        }

                                        sb.AppendLine(")");
                                    }
                                    //sb.AppendFormat("Absolute time: {3:D2}:{0:D2}:{1:D2}:{2:D2}", descriptor.Min, descriptor.Sec, descriptor.Frame, descriptor.HOUR).AppendLine();
                                    break;
                                }
                                case 0xA2:
                                {
                                    if(descriptor.PHOUR > 0)
                                        sb.AppendFormat("Lead-out start position: {3:D2}:{0:D2}:{1:D2}:{2:D2}",
                                                        descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME,
                                                        descriptor.PHOUR).AppendLine();
                                    else
                                        sb.AppendFormat("Lead-out start position: {0:D2}:{1:D2}:{2:D2}",
                                                        descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME)
                                          .AppendLine();
                                    //sb.AppendFormat("Absolute time: {3:D2}:{0:D2}:{1:D2}:{2:D2}", descriptor.Min, descriptor.Sec, descriptor.Frame, descriptor.HOUR).AppendLine();

                                    switch((TocControl)(descriptor.CONTROL & 0x0D))
                                    {
                                        case TocControl.TwoChanNoPreEmph:
                                        case TocControl.TwoChanPreEmph:
                                        case TocControl.FourChanNoPreEmph:
                                        case TocControl.FourChanPreEmph:
                                            sb.AppendLine("Lead-out is audio type");
                                            break;
                                        case TocControl.DataTrack:
                                        case TocControl.DataTrackIncremental:
                                            sb.AppendLine("Lead-out is data type");
                                            break;
                                    }

                                    break;
                                }
                                case 0xF0:
                                {
                                    sb.AppendFormat("Book type: 0x{0:X2}", descriptor.PMIN);
                                    sb.AppendFormat("Material type: 0x{0:X2}", descriptor.PSEC);
                                    sb.AppendFormat("Moment of inertia: 0x{0:X2}", descriptor.PFRAME);
                                    if(descriptor.PHOUR > 0)
                                        sb.AppendFormat("Absolute time: {3:D2}:{0:D2}:{1:D2}:{2:D2}", descriptor.Min,
                                                        descriptor.Sec, descriptor.Frame, descriptor.HOUR).AppendLine();
                                    else
                                        sb.AppendFormat("Absolute time: {0:D2}:{1:D2}:{2:D2}", descriptor.Min,
                                                        descriptor.Sec, descriptor.Frame).AppendLine();
                                    break;
                                }
                                default:
                                {
                                    if(descriptor.POINT >= 0x01 && descriptor.POINT <= 0x63)
                                        if(descriptor.ADR == 4)
                                            sb.AppendFormat("Video track {3} starts at: {0:D2}:{1:D2}:{2:D2}",
                                                            descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME,
                                                            descriptor.POINT).AppendLine();
                                        else
                                        {
                                            string type = "Audio";

                                            if((TocControl)(descriptor.CONTROL & 0x0D) == TocControl.DataTrack ||
                                               (TocControl)(descriptor.CONTROL & 0x0D) ==
                                               TocControl.DataTrackIncremental) type = "Data";

                                            if(descriptor.PHOUR > 0)
                                                sb.AppendFormat("{5} track {3} starts at: {4:D2}:{0:D2}:{1:D2}:{2:D2} (",
                                                                descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME,
                                                                descriptor.POINT, descriptor.PHOUR, type);
                                            else
                                                sb.AppendFormat("{4} track {3} starts at: {0:D2}:{1:D2}:{2:D2} (",
                                                                descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME,
                                                                descriptor.POINT, type);

                                            switch((TocControl)(descriptor.CONTROL & 0x0D))
                                            {
                                                case TocControl.TwoChanNoPreEmph:
                                                    sb.Append(StereoNoPre);
                                                    break;
                                                case TocControl.TwoChanPreEmph:
                                                    sb.Append(StereoPreEm);
                                                    break;
                                                case TocControl.FourChanNoPreEmph:
                                                    sb.Append(QuadNoPreEm);
                                                    break;
                                                case TocControl.FourChanPreEmph:
                                                    sb.Append(QuadPreEmph);
                                                    break;
                                                case TocControl.DataTrack:
                                                    sb.Append(DataUnintrp);
                                                    break;
                                                case TocControl.DataTrackIncremental:
                                                    sb.Append(DataIncrtly);
                                                    break;
                                            }

                                            sb.AppendLine(")");
                                        }
                                    else
                                    {
                                        sb.AppendFormat("ADR = {0}", descriptor.ADR).AppendLine();
                                        sb.AppendFormat("CONTROL = {0}", descriptor.CONTROL).AppendLine();
                                        sb.AppendFormat("TNO = {0}", descriptor.TNO).AppendLine();
                                        sb.AppendFormat("POINT = {0}", descriptor.POINT).AppendLine();
                                        sb.AppendFormat("Min = {0}", descriptor.Min).AppendLine();
                                        sb.AppendFormat("Sec = {0}", descriptor.Sec).AppendLine();
                                        sb.AppendFormat("Frame = {0}", descriptor.Frame).AppendLine();
                                        sb.AppendFormat("HOUR = {0}", descriptor.HOUR).AppendLine();
                                        sb.AppendFormat("PHOUR = {0}", descriptor.PHOUR).AppendLine();
                                        sb.AppendFormat("PMIN = {0}", descriptor.PMIN).AppendLine();
                                        sb.AppendFormat("PSEC = {0}", descriptor.PSEC).AppendLine();
                                        sb.AppendFormat("PFRAME = {0}", descriptor.PFRAME).AppendLine();
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
                                        sb
                                            .AppendFormat("Start of next possible program in the recordable area of the disc: {3:D2}:{0:D2}:{1:D2}:{2:D2}",
                                                          descriptor.Min, descriptor.Sec, descriptor.Frame,
                                                          descriptor.HOUR).AppendLine();
                                        sb
                                            .AppendFormat("Maximum start of outermost Lead-out in the recordable area of the disc: {3:D2}:{0:D2}:{1:D2}:{2:D2}",
                                                          descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME,
                                                          descriptor.PHOUR).AppendLine();
                                    }
                                    else
                                    {
                                        sb
                                            .AppendFormat("Start of next possible program in the recordable area of the disc: {0:D2}:{1:D2}:{2:D2}",
                                                          descriptor.Min, descriptor.Sec, descriptor.Frame)
                                            .AppendLine();
                                        sb
                                            .AppendFormat("Maximum start of outermost Lead-out in the recordable area of the disc: {0:D2}:{1:D2}:{2:D2}",
                                                          descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME)
                                            .AppendLine();
                                    }
                                    break;
                                }
                                case 0xB1:
                                {
                                    sb.AppendFormat("Number of skip interval pointers: {0}", descriptor.PMIN)
                                      .AppendLine();
                                    sb.AppendFormat("Number of skip track pointers: {0}", descriptor.PSEC).AppendLine();
                                    break;
                                }
                                case 0xB2:
                                case 0xB3:
                                case 0xB4:
                                {
                                    sb.AppendFormat("Skip track {0}", descriptor.Min).AppendLine();
                                    sb.AppendFormat("Skip track {0}", descriptor.Sec).AppendLine();
                                    sb.AppendFormat("Skip track {0}", descriptor.Frame).AppendLine();
                                    sb.AppendFormat("Skip track {0}", descriptor.Zero).AppendLine();
                                    sb.AppendFormat("Skip track {0}", descriptor.PMIN).AppendLine();
                                    sb.AppendFormat("Skip track {0}", descriptor.PSEC).AppendLine();
                                    sb.AppendFormat("Skip track {0}", descriptor.PFRAME).AppendLine();
                                    break;
                                }
                                case 0xC0:
                                {
                                    sb.AppendFormat("Optimum recording power: 0x{0:X2}", descriptor.Min).AppendLine();
                                    if(descriptor.PHOUR > 0)
                                        sb
                                            .AppendFormat("Start time of the first Lead-in area in the disc: {3:D2}:{0:D2}:{1:D2}:{2:D2}",
                                                          descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME,
                                                          descriptor.PHOUR).AppendLine();
                                    else
                                        sb
                                            .AppendFormat("Start time of the first Lead-in area in the disc: {0:D2}:{1:D2}:{2:D2}",
                                                          descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME)
                                            .AppendLine();
                                    break;
                                }
                                case 0xC1:
                                {
                                    sb.AppendFormat("Copy of information of A1 from ATIP found");
                                    sb.AppendFormat("Min = {0}", descriptor.Min).AppendLine();
                                    sb.AppendFormat("Sec = {0}", descriptor.Sec).AppendLine();
                                    sb.AppendFormat("Frame = {0}", descriptor.Frame).AppendLine();
                                    sb.AppendFormat("Zero = {0}", descriptor.Zero).AppendLine();
                                    sb.AppendFormat("PMIN = {0}", descriptor.PMIN).AppendLine();
                                    sb.AppendFormat("PSEC = {0}", descriptor.PSEC).AppendLine();
                                    sb.AppendFormat("PFRAME = {0}", descriptor.PFRAME).AppendLine();
                                    break;
                                }
                                case 0xCF:
                                {
                                    if(descriptor.PHOUR > 0)
                                    {
                                        sb
                                            .AppendFormat("Start position of outer part lead-in area: {3:D2}:{0:D2}:{1:D2}:{2:D2}",
                                                          descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME,
                                                          descriptor.PHOUR).AppendLine();
                                        sb
                                            .AppendFormat("Stop position of inner part lead-out area: {3:D2}:{0:D2}:{1:D2}:{2:D2}",
                                                          descriptor.Min, descriptor.Sec, descriptor.Frame,
                                                          descriptor.HOUR).AppendLine();
                                    }
                                    else
                                    {
                                        sb
                                            .AppendFormat("Start position of outer part lead-in area: {0:D2}:{1:D2}:{2:D2}",
                                                          descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME)
                                            .AppendLine();
                                        sb
                                            .AppendFormat("Stop position of inner part lead-out area: {0:D2}:{1:D2}:{2:D2}",
                                                          descriptor.Min, descriptor.Sec, descriptor.Frame)
                                            .AppendLine();
                                    }
                                    break;
                                }
                                default:
                                {
                                    if(descriptor.POINT >= 0x01 && descriptor.POINT <= 0x40)
                                    {
                                        sb
                                            .AppendFormat("Start time for interval that should be skipped: {0:D2}:{1:D2}:{2:D2}",
                                                          descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME)
                                            .AppendLine();
                                        sb
                                            .AppendFormat("Ending time for interval that should be skipped: {0:D2}:{1:D2}:{2:D2}",
                                                          descriptor.Min, descriptor.Sec, descriptor.Frame)
                                            .AppendLine();
                                    }
                                    else
                                    {
                                        sb.AppendFormat("ADR = {0}", descriptor.ADR).AppendLine();
                                        sb.AppendFormat("CONTROL = {0}", descriptor.CONTROL).AppendLine();
                                        sb.AppendFormat("TNO = {0}", descriptor.TNO).AppendLine();
                                        sb.AppendFormat("POINT = {0}", descriptor.POINT).AppendLine();
                                        sb.AppendFormat("Min = {0}", descriptor.Min).AppendLine();
                                        sb.AppendFormat("Sec = {0}", descriptor.Sec).AppendLine();
                                        sb.AppendFormat("Frame = {0}", descriptor.Frame).AppendLine();
                                        sb.AppendFormat("HOUR = {0}", descriptor.HOUR).AppendLine();
                                        sb.AppendFormat("PHOUR = {0}", descriptor.PHOUR).AppendLine();
                                        sb.AppendFormat("PMIN = {0}", descriptor.PMIN).AppendLine();
                                        sb.AppendFormat("PSEC = {0}", descriptor.PSEC).AppendLine();
                                        sb.AppendFormat("PFRAME = {0}", descriptor.PFRAME).AppendLine();
                                    }
                                    break;
                                }
                            }

                            break;
                        }
                        case 6:
                        {
                            uint id = (uint)((descriptor.Min << 16) + (descriptor.Sec << 8) + descriptor.Frame);
                            sb.AppendFormat("Disc ID: {0:X6}", id & 0x00FFFFFF).AppendLine();
                            break;
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
    }
}