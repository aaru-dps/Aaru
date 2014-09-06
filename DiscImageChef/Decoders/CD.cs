/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : CD.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Decoders.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Decodes CD and DDCD structures.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$
using System;
using System.Text;

namespace DiscImageChef.Decoders
{
    /// <summary>
    /// Information from the following standards:
    /// ANSI X3.304-1997
    /// T10/1048-D revision 9.0
    /// T10/1048-D revision 10a
    /// T10/1228-D revision 7.0c
    /// T10/1228-D revision 11a
    /// T10/1363-D revision 10g
    /// T10/1545-D revision 1d
    /// T10/1545-D revision 5
    /// T10/1545-D revision 5a
    /// T10/1675-D revision 2c
    /// T10/1675-D revision 4
    /// T10/1836-D revision 2g
    /// </summary>
    public static class CD
    {
        #region Enumerations

        public enum TOC_ADR : byte
        {
            /// <summary>
            /// Q Sub-channel mode information not supplied
            /// </summary>
            NoInformation = 0x00,
            /// <summary>
            /// Q Sub-channel encodes current position data
            /// </summary>
            CurrentPosition = 0x01,
            /// <summary>
            /// Q Sub-channel encodes the media catalog number
            /// </summary>
            MediaCatalogNumber = 0x02,
            /// <summary>
            /// Q Sub-channel encodes the ISRC
            /// </summary>
            ISRC = 0x03
        }

        public enum TOC_CONTROL : byte
        {
            /// <summary>
            /// Stereo audio, no pre-emphasis
            /// </summary>
            TwoChanNoPreEmph = 0x00,
            /// <summary>
            /// Stereo audio with pre-emphasis
            /// </summary>
            TwoChanPreEmph = 0x01,
            /// <summary>
            /// If mask applied, track can be copied
            /// </summary>
            CopyPermissionMask = 0x02,
            /// <summary>
            /// Data track, recorded uninterrumpted
            /// </summary>
            DataTrack = 0x04,
            /// <summary>
            /// Data track, recorded incrementally
            /// </summary>
            DataTrackIncremental = 0x05,
            /// <summary>
            /// Quadraphonic audio, no pre-emphasis
            /// </summary>
            FourChanNoPreEmph = 0x08,
            /// <summary>
            /// Quadraphonic audio with pre-emphasis
            /// </summary>
            FourChanPreEmph = 0x09,
            /// <summary>
            /// Reserved mask
            /// </summary>
            ReservedMask = 0x0C
        }

        public enum CDTextPackTypeIndicator : byte
        {
            /// <summary>
            /// Title of the track (or album if track == 0)
            /// </summary>
            Title = 0x80,
            /// <summary>
            /// Performer
            /// </summary>
            Performer = 0x81,
            /// <summary>
            /// Songwriter
            /// </summary>
            Songwriter = 0x82,
            /// <summary>
            /// Composer
            /// </summary>
            Composer = 0x83,
            /// <summary>
            /// Arranger
            /// </summary>
            Arranger = 0x84,
            /// <summary>
            /// Message from the content provider or artist
            /// </summary>
            Message = 0x85,
            /// <summary>
            /// Disc identification information
            /// </summary>
            DiscIdentification = 0x86,
            /// <summary>
            /// Genre identification
            /// </summary>
            GenreIdentification = 0x87,
            /// <summary>
            /// Table of content information
            /// </summary>
            TOCInformation = 0x88,
            /// <summary>
            /// Second table of content information
            /// </summary>
            SecondTOCInformation = 0x89,
            /// <summary>
            /// Reserved
            /// </summary>
            Reserved1 = 0x8A,
            /// <summary>
            /// Reserved
            /// </summary>
            Reserved2 = 0x8B,
            /// <summary>
            /// Reserved
            /// </summary>
            Reserved3 = 0x8C,
            /// <summary>
            /// Reserved for content provider only
            /// </summary>
            ReservedForContentProvider = 0x8D,
            /// <summary>
            /// UPC of album or ISRC of track
            /// </summary>
            UPCorISRC = 0x8E,
            /// <summary>
            /// Size information of the block
            /// </summary>
            BlockSizeInformation = 0x8F
        }

        #endregion Enumerations

        #region Public methods

        public static CDTOC? DecodeCDTOC(byte[] CDTOCResponse)
        {
            if (CDTOCResponse == null)
                return null;

            CDTOC decoded = new CDTOC();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.DataLength = BigEndianBitConverter.ToUInt16(CDTOCResponse, 0);
            decoded.FirstTrack = CDTOCResponse[2];
            decoded.LastTrack = CDTOCResponse[3];
            decoded.TrackDescriptors = new CDTOCTrackDataDescriptor[(decoded.DataLength - 2) / 8];

            if (decoded.DataLength + 2 != CDTOCResponse.Length)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (CDTOC Decoder): Expected CDTOC size ({0} bytes) is not received size ({1} bytes), not decoding", decoded.DataLength + 2, CDTOCResponse.Length);
                return null;
            }

            for (int i = 0; i < ((decoded.DataLength - 2) / 8); i++)
            {
                decoded.TrackDescriptors[i].Reserved1 = CDTOCResponse[0 + i * 8 + 4];
                decoded.TrackDescriptors[i].ADR = (byte)((CDTOCResponse[1 + i * 8 + 4] & 0xF0) >> 4);
                decoded.TrackDescriptors[i].CONTROL = (byte)(CDTOCResponse[1 + i * 8 + 4] & 0x0F);
                decoded.TrackDescriptors[i].TrackNumber = CDTOCResponse[2 + i * 8 + 4];
                decoded.TrackDescriptors[i].Reserved2 = CDTOCResponse[3 + i * 8 + 4];
                decoded.TrackDescriptors[i].TrackStartAddress = BigEndianBitConverter.ToUInt32(CDTOCResponse, 4 + i * 8 + 4);
            }

            return decoded;
        }

        public static string PrettifyCDTOC(CDTOC? CDTOCResponse)
        {
            if (CDTOCResponse == null)
                return null;

            CDTOC response = CDTOCResponse.Value;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("First track number in first complete session: {0}", response.FirstTrack).AppendLine();
            sb.AppendFormat("Last track number in last complete session: {0}", response.LastTrack).AppendLine();
            foreach (CDTOCTrackDataDescriptor descriptor in response.TrackDescriptors)
            {
                sb.AppendFormat("Track number: {0}", descriptor.TrackNumber);
                sb.AppendFormat("Track starts at LBA {0}, or MSF {1:X2}:{2:X2}:{3:X2}", descriptor.TrackStartAddress,
                    (descriptor.TrackStartAddress & 0x0000FF00) >> 8,
                    (descriptor.TrackStartAddress & 0x00FF0000) >> 16,
                    (descriptor.TrackStartAddress & 0xFF000000) >> 24);

                switch ((TOC_ADR)descriptor.ADR)
                {
                    case TOC_ADR.NoInformation:
                        sb.AppendLine("Q subchannel mode not given");
                        break;
                    case TOC_ADR.CurrentPosition:
                        sb.AppendLine("Q subchannel stores current position");
                        break;
                    case TOC_ADR.ISRC:
                        sb.AppendLine("Q subchannel stores ISRC");
                        break;
                    case TOC_ADR.MediaCatalogNumber:
                        sb.AppendLine("Q subchannel stores media catalog number");
                        break;
                }

                if((descriptor.CONTROL & (byte)TOC_CONTROL.ReservedMask) == (byte)TOC_CONTROL.ReservedMask)
                    sb.AppendFormat("Reserved flags 0x{0:X2} set", descriptor.CONTROL).AppendLine();
                else
                {
                    switch ((TOC_CONTROL)(descriptor.CONTROL & 0x0D))
                    {
                        case TOC_CONTROL.TwoChanNoPreEmph:
                            sb.AppendLine("Stereo audio track with no pre-emphasis");
                            break;
                        case TOC_CONTROL.TwoChanPreEmph:
                            sb.AppendLine("Stereo audio track with 50/15 μs pre-emphasis");
                            break;
                        case TOC_CONTROL.FourChanNoPreEmph:
                            sb.AppendLine("Quadraphonic audio track with no pre-emphasis");
                            break;
                        case TOC_CONTROL.FourChanPreEmph:
                            sb.AppendLine("Stereo audio track with 50/15 μs pre-emphasis");
                            break;
                        case TOC_CONTROL.DataTrack:
                            sb.AppendLine("Data track, recorded uninterrupted");
                            break;
                        case TOC_CONTROL.DataTrackIncremental:
                            sb.AppendLine("Data track, recorded incrementally");
                            break;
                    }

                    if ((descriptor.CONTROL & (byte)TOC_CONTROL.CopyPermissionMask) == (byte)TOC_CONTROL.CopyPermissionMask)
                        sb.AppendLine("Digital copy of track is permitted");
                    else
                        sb.AppendLine("Digital copy of track is prohibited");

                    if (MainClass.isDebug)
                    {
                        sb.AppendFormat("Reserved1: {0:X2}", descriptor.Reserved1).AppendLine();
                        sb.AppendFormat("Reserved2: {0:X2}", descriptor.Reserved2).AppendLine();
                    }

                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        public static string PrettifyCDTOC(byte[] CDTOCResponse)
        {
            CDTOC? decoded = DecodeCDTOC(CDTOCResponse);
            return PrettifyCDTOC(decoded);
        }

        public static CDSessionInfo? DecodeCDSessionInfo(byte[] CDSessionInfoResponse)
        {
            if (CDSessionInfoResponse == null)
                return null;

            CDSessionInfo decoded = new CDSessionInfo();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.DataLength = BigEndianBitConverter.ToUInt16(CDSessionInfoResponse, 0);
            decoded.FirstCompleteSession = CDSessionInfoResponse[2];
            decoded.LastCompleteSession = CDSessionInfoResponse[3];
            decoded.TrackDescriptors = new CDSessionInfoTrackDataDescriptor[(decoded.DataLength - 2) / 8];

            if (decoded.DataLength + 2 != CDSessionInfoResponse.Length)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (CDSessionInfo Decoder): Expected CDSessionInfo size ({0} bytes) is not received size ({1} bytes), not decoding", decoded.DataLength + 2, CDSessionInfoResponse.Length);
                return null;
            }

            for (int i = 0; i < ((decoded.DataLength - 2) / 8); i++)
            {
                decoded.TrackDescriptors[i].Reserved1 = CDSessionInfoResponse[0 + i * 8 + 4];
                decoded.TrackDescriptors[i].ADR = (byte)((CDSessionInfoResponse[1 + i * 8 + 4] & 0xF0) >> 4);
                decoded.TrackDescriptors[i].CONTROL = (byte)(CDSessionInfoResponse[1 + i * 8 + 4] & 0x0F);
                decoded.TrackDescriptors[i].TrackNumber = CDSessionInfoResponse[2 + i * 8 + 4];
                decoded.TrackDescriptors[i].Reserved2 = CDSessionInfoResponse[3 + i * 8 + 4];
                decoded.TrackDescriptors[i].TrackStartAddress = BigEndianBitConverter.ToUInt32(CDSessionInfoResponse, 4 + i * 8 + 4);
            }

            return decoded;
        }

        public static string PrettifyCDSessionInfo(CDSessionInfo? CDSessionInfoResponse)
        {
            if (CDSessionInfoResponse == null)
                return null;

            CDSessionInfo response = CDSessionInfoResponse.Value;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("First complete session number: {0}", response.FirstCompleteSession).AppendLine();
            sb.AppendFormat("Last complete session number: {0}", response.LastCompleteSession).AppendLine();
            foreach (CDSessionInfoTrackDataDescriptor descriptor in response.TrackDescriptors)
            {
                sb.AppendFormat("First track number in last complete session: {0}", descriptor.TrackNumber);
                sb.AppendFormat("Track starts at LBA {0}, or MSF {1:X2}:{2:X2}:{3:X2}", descriptor.TrackStartAddress,
                    (descriptor.TrackStartAddress & 0x0000FF00) >> 8,
                    (descriptor.TrackStartAddress & 0x00FF0000) >> 16,
                    (descriptor.TrackStartAddress & 0xFF000000) >> 24);

                switch ((TOC_ADR)descriptor.ADR)
                {
                    case TOC_ADR.NoInformation:
                        sb.AppendLine("Q subchannel mode not given");
                        break;
                    case TOC_ADR.CurrentPosition:
                        sb.AppendLine("Q subchannel stores current position");
                        break;
                    case TOC_ADR.ISRC:
                        sb.AppendLine("Q subchannel stores ISRC");
                        break;
                    case TOC_ADR.MediaCatalogNumber:
                        sb.AppendLine("Q subchannel stores media catalog number");
                        break;
                }

                if((descriptor.CONTROL & (byte)TOC_CONTROL.ReservedMask) == (byte)TOC_CONTROL.ReservedMask)
                    sb.AppendFormat("Reserved flags 0x{0:X2} set", descriptor.CONTROL).AppendLine();
                else
                {
                    switch ((TOC_CONTROL)(descriptor.CONTROL & 0x0D))
                    {
                        case TOC_CONTROL.TwoChanNoPreEmph:
                            sb.AppendLine("Stereo audio track with no pre-emphasis");
                            break;
                        case TOC_CONTROL.TwoChanPreEmph:
                            sb.AppendLine("Stereo audio track with 50/15 μs pre-emphasis");
                            break;
                        case TOC_CONTROL.FourChanNoPreEmph:
                            sb.AppendLine("Quadraphonic audio track with no pre-emphasis");
                            break;
                        case TOC_CONTROL.FourChanPreEmph:
                            sb.AppendLine("Stereo audio track with 50/15 μs pre-emphasis");
                            break;
                        case TOC_CONTROL.DataTrack:
                            sb.AppendLine("Data track, recorded uninterrupted");
                            break;
                        case TOC_CONTROL.DataTrackIncremental:
                            sb.AppendLine("Data track, recorded incrementally");
                            break;
                    }

                    if ((descriptor.CONTROL & (byte)TOC_CONTROL.CopyPermissionMask) == (byte)TOC_CONTROL.CopyPermissionMask)
                        sb.AppendLine("Digital copy of track is permitted");
                    else
                        sb.AppendLine("Digital copy of track is prohibited");

                    if (MainClass.isDebug)
                    {
                        sb.AppendFormat("Reserved1: {0:X2}", descriptor.Reserved1).AppendLine();
                        sb.AppendFormat("Reserved2: {0:X2}", descriptor.Reserved2).AppendLine();
                    }

                    sb.AppendLine();
                }
            }

            return sb.ToString();
        }

        public static string PrettifyCDSessionInfo(byte[] CDSessionInfoResponse)
        {
            CDSessionInfo? decoded = DecodeCDSessionInfo(CDSessionInfoResponse);
            return PrettifyCDSessionInfo(decoded);
        }

        public static CDFullTOC? DecodeCDFullTOC(byte[] CDFullTOCResponse)
        {
            if (CDFullTOCResponse == null)
                return null;

            CDFullTOC decoded = new CDFullTOC();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.DataLength = BigEndianBitConverter.ToUInt16(CDFullTOCResponse, 0);
            decoded.FirstCompleteSession = CDFullTOCResponse[2];
            decoded.LastCompleteSession = CDFullTOCResponse[3];
            decoded.TrackDescriptors = new CDFullTOCInfoTrackDataDescriptor[(decoded.DataLength - 2) / 11];

            if (decoded.DataLength + 2 != CDFullTOCResponse.Length)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (CDFullTOC Decoder): Expected CDFullTOC size ({0} bytes) is not received size ({1} bytes), not decoding", decoded.DataLength + 2, CDFullTOCResponse.Length);
                return null;
            }

            for (int i = 0; i < ((decoded.DataLength - 2) / 11); i++)
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

        public static string PrettifyCDFullTOC(CDFullTOC? CDFullTOCResponse)
        {
            if (CDFullTOCResponse == null)
                return null;

            CDFullTOC response = CDFullTOCResponse.Value;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("First complete session number: {0}", response.FirstCompleteSession).AppendLine();
            sb.AppendFormat("Last complete session number: {0}", response.LastCompleteSession).AppendLine();
            foreach (CDFullTOCInfoTrackDataDescriptor descriptor in response.TrackDescriptors)
            {
                if ((descriptor.CONTROL != 4 && descriptor.CONTROL != 6) ||
                    (descriptor.ADR != 1 && descriptor.ADR != 5) ||
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
                    sb.AppendFormat("Session {0}", descriptor.SessionNumber).AppendLine();
                    switch (descriptor.ADR)
                    {
                        case 1:
                            {
                                switch (descriptor.POINT)
                                {
                                    case 0xA0:
                                        {
                                            sb.AppendFormat("First track number: {0}", descriptor.PMIN).AppendLine();
                                            sb.AppendFormat("Disc type: {0}", descriptor.PSEC).AppendLine();
                                            sb.AppendFormat("Absolute time: {3:X2}:{0:X2}:{1:X2}:{2:X2}", descriptor.Min, descriptor.Sec, descriptor.Frame, descriptor.HOUR).AppendLine();
                                            break;
                                        }
                                    case 0xA1:
                                        {
                                            sb.AppendFormat("Last track number: {0}", descriptor.PMIN).AppendLine();
                                            sb.AppendFormat("Absolute time: {3:X2}:{0:X2}:{1:X2}:{2:X2}", descriptor.Min, descriptor.Sec, descriptor.Frame, descriptor.HOUR).AppendLine();
                                            break;
                                        }
                                    case 0xA2:
                                        {
                                            sb.AppendFormat("Lead-out start position: {3:X2}:{0:X2}:{1:X2}:{2:X2}", descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME, descriptor.PHOUR).AppendLine();
                                            sb.AppendFormat("Absolute time: {3:X2}:{0:X2}:{1:X2}:{2:X2}", descriptor.Min, descriptor.Sec, descriptor.Frame, descriptor.HOUR).AppendLine();
                                            break;
                                        }
                                    case 0xF0:
                                        {
                                            sb.AppendFormat("Book type: 0x{0:X2}", descriptor.PMIN);
                                            sb.AppendFormat("Material type: 0x{0:X2}", descriptor.PSEC);
                                            sb.AppendFormat("Moment of inertia: 0x{0:X2}", descriptor.PFRAME);
                                            sb.AppendFormat("Absolute time: {3:X2}:{0:X2}:{1:X2}:{2:X2}", descriptor.Min, descriptor.Sec, descriptor.Frame, descriptor.HOUR).AppendLine();
                                            break;
                                        }
                                    default:
                                        {
                                            if (descriptor.POINT >= 0x01 && descriptor.POINT <= 0x63)
                                            {
                                                sb.AppendFormat("Track start position for track {3}: {4:X2}:{0:X2}:{1:X2}:{2:X2}", descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME, descriptor.POINT, descriptor.PHOUR).AppendLine();
                                                sb.AppendFormat("Absolute time: {3:X2}:{0:X2}:{1:X2}:{2:X2}", descriptor.Min, descriptor.Sec, descriptor.Frame, descriptor.HOUR).AppendLine();
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
                                switch (descriptor.POINT)
                                {
                                    case 0xB0:
                                        {
                                            sb.AppendFormat("Start of next possible program in the recordable area of the disc: {3:X2}:{0:X2}:{1:X2}:{2:X2}", descriptor.Min, descriptor.Sec, descriptor.Frame, descriptor.HOUR).AppendLine();
                                            sb.AppendFormat("Maximum start of outermost Lead-out in the recordable area of the disc: {3:X2}:{0:X2}:{1:X2}:{2:X2}", descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME, descriptor.PHOUR).AppendLine();
                                            break;
                                        }
                                    case 0xB1:
                                        {
                                            sb.AppendFormat("Number of skip interval pointers: {0:X2}", descriptor.PMIN).AppendLine();
                                            sb.AppendFormat("Number of skip track pointers: {0:X2}", descriptor.PSEC).AppendLine();
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
                                            sb.AppendFormat("Start time of the first Lead-in area in the disc: {3:X2}:{0:X2}:{1:X2}:{2:X2}", descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME, descriptor.PHOUR).AppendLine();
                                            break;
                                        }
                                    case 0xC1:
                                        {
                                            sb.AppendFormat("Copy of information of A1 from ATIP found");
                                            if (MainClass.isDebug)
                                            {
                                                sb.AppendFormat("Min = {0}", descriptor.Min).AppendLine();
                                                sb.AppendFormat("Sec = {0}", descriptor.Sec).AppendLine();
                                                sb.AppendFormat("Frame = {0}", descriptor.Frame).AppendLine();
                                                sb.AppendFormat("Zero = {0}", descriptor.Zero).AppendLine();
                                                sb.AppendFormat("PMIN = {0}", descriptor.PMIN).AppendLine();
                                                sb.AppendFormat("PSEC = {0}", descriptor.PSEC).AppendLine();
                                                sb.AppendFormat("PFRAME = {0}", descriptor.PFRAME).AppendLine();
                                            }
                                            break;
                                        }
                                    case 0xCF:
                                        {
                                            sb.AppendFormat("Start position of outer part lead-in area: {3:X2}:{0:X2}:{1:X2}:{2:X2}", descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME, descriptor.PHOUR).AppendLine();
                                            sb.AppendFormat("Stop position of inner part lead-out area: {3:X2}:{0:X2}:{1:X2}:{2:X2}", descriptor.Min, descriptor.Sec, descriptor.Frame, descriptor.HOUR).AppendLine();
                                            break;
                                        }
                                    default:
                                        {
                                            if (descriptor.POINT >= 0x01 && descriptor.POINT <= 0x40)
                                            {
                                                sb.AppendFormat("Start time for interval that should be skipped: {0:X2}:{1:X2}:{2:X2}", descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME).AppendLine();
                                                sb.AppendFormat("Ending time for interval that should be skipped: {0:X2}:{1:X2}:{2:X2}", descriptor.Min, descriptor.Sec, descriptor.Frame).AppendLine();
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
                    }
                }
            }

            return sb.ToString();
        }

        public static string PrettifyCDFullTOC(byte[] CDFullTOCResponse)
        {
            CDFullTOC? decoded = DecodeCDFullTOC(CDFullTOCResponse);
            return PrettifyCDFullTOC(decoded);
        }

        public static CDPMA? DecodeCDPMA(byte[] CDPMAResponse)
        {
            if (CDPMAResponse == null)
                return null;

            CDPMA decoded = new CDPMA();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.DataLength = BigEndianBitConverter.ToUInt16(CDPMAResponse, 0);
            decoded.Reserved1 = CDPMAResponse[2];
            decoded.Reserved2 = CDPMAResponse[3];
            decoded.PMADescriptors = new CDPMADescriptors[(decoded.DataLength - 2) / 11];

            if (decoded.DataLength + 2 != CDPMAResponse.Length)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (CDPMA Decoder): Expected CDPMA size ({0} bytes) is not received size ({1} bytes), not decoding", decoded.DataLength + 2, CDPMAResponse.Length);
                return null;
            }

            for (int i = 0; i < ((decoded.DataLength - 2) / 11); i++)
            {
                decoded.PMADescriptors[i].Reserved = CDPMAResponse[0 + i * 11 + 4];
                decoded.PMADescriptors[i].ADR = (byte)((CDPMAResponse[1 + i * 11 + 4] & 0xF0) >> 4);
                decoded.PMADescriptors[i].CONTROL = (byte)(CDPMAResponse[1 + i * 11 + 4] & 0x0F);
                decoded.PMADescriptors[i].TNO = CDPMAResponse[2 + i * 11 + 4];
                decoded.PMADescriptors[i].POINT = CDPMAResponse[3 + i * 11 + 4];
                decoded.PMADescriptors[i].Min = CDPMAResponse[4 + i * 11 + 4];
                decoded.PMADescriptors[i].Sec = CDPMAResponse[5 + i * 11 + 4];
                decoded.PMADescriptors[i].Frame = CDPMAResponse[6 + i * 11 + 4];
                decoded.PMADescriptors[i].HOUR = (byte)((CDPMAResponse[7 + i * 11 + 4] & 0xF0) >> 4);
                decoded.PMADescriptors[i].PHOUR = (byte)(CDPMAResponse[7 + i * 11 + 4] & 0x0F);
                decoded.PMADescriptors[i].PMIN = CDPMAResponse[8 + i * 11 + 4];
                decoded.PMADescriptors[i].PSEC = CDPMAResponse[9 + i * 11 + 4];
                decoded.PMADescriptors[i].PFRAME = CDPMAResponse[10 + i * 11 + 4];
            }

            return decoded;
        }

        public static string PrettifyCDPMA(CDPMA? CDPMAResponse)
        {
            if (CDPMAResponse == null)
                return null;

            CDPMA response = CDPMAResponse.Value;

            StringBuilder sb = new StringBuilder();

            if (MainClass.isDebug)
            {
                sb.AppendFormat("Reserved1: 0x{0:X2}", response.Reserved1).AppendLine();
                sb.AppendFormat("Reserved2: 0x{0:X2}", response.Reserved2).AppendLine();
            }
            foreach (CDPMADescriptors descriptor in response.PMADescriptors)
            {
                if (MainClass.isDebug)
                    sb.AppendFormat("Reserved1: 0x{0:X2}", descriptor.Reserved).AppendLine();
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

            return sb.ToString();
        }

        public static string PrettifyCDPMA(byte[] CDPMAResponse)
        {
            CDPMA? decoded = DecodeCDPMA(CDPMAResponse);
            return PrettifyCDPMA(decoded);
        }

        public static CDATIP? DecodeCDATIP(byte[] CDATIPResponse)
        {
            if (CDATIPResponse == null)
                return null;

            CDATIP decoded = new CDATIP();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            if (CDATIPResponse.Length != 32)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (CDATIP Decoder): Expected CDATIP size (32 bytes) is not received size ({0} bytes), not decoding", CDATIPResponse.Length);
                return null;
            }

            decoded.DataLength = BigEndianBitConverter.ToUInt16(CDATIPResponse, 0);
            decoded.Reserved1 = CDATIPResponse[2];
            decoded.Reserved2 = CDATIPResponse[3];
            decoded.ITWP = (byte)((CDATIPResponse[4] & 0xF0) >> 4);
            decoded.DDCD = Convert.ToBoolean(CDATIPResponse[4] & 0x08);
            decoded.ReferenceSpeed = (byte)(CDATIPResponse[4] & 0x07);
            decoded.AlwaysZero = Convert.ToBoolean(CDATIPResponse[5] & 0x80);
            decoded.URU = Convert.ToBoolean(CDATIPResponse[5] & 0x40);
            decoded.Reserved3 = (byte)(CDATIPResponse[5] & 0x3F);

            decoded.AlwaysOne = Convert.ToBoolean(CDATIPResponse[6] & 0x80);
            decoded.DiscType = Convert.ToBoolean(CDATIPResponse[6] & 0x40);
            decoded.DiscSubType = (byte)((CDATIPResponse[6] & 0x38) >> 3);
            decoded.A1Valid = Convert.ToBoolean(CDATIPResponse[6] & 0x04);
            decoded.A2Valid = Convert.ToBoolean(CDATIPResponse[6] & 0x02);
            decoded.A3Valid = Convert.ToBoolean(CDATIPResponse[6] & 0x01);

            decoded.Reserved4 = CDATIPResponse[7];
            decoded.LeadInStartMin = CDATIPResponse[8];
            decoded.LeadInStartSec = CDATIPResponse[9];
            decoded.LeadInStartFrame = CDATIPResponse[10];
            decoded.Reserved5 = CDATIPResponse[11];
            decoded.LeadOutStartMin = CDATIPResponse[12];
            decoded.LeadOutStartSec = CDATIPResponse[13];
            decoded.LeadOutStartFrame = CDATIPResponse[14];
            decoded.Reserved6 = CDATIPResponse[15];

            decoded.A1Values = new byte[3];
            decoded.A2Values = new byte[3];
            decoded.A3Values = new byte[3];
            decoded.S4Values = new byte[3];

            Array.Copy(CDATIPResponse, 16, decoded.A1Values, 0, 3);
            Array.Copy(CDATIPResponse, 20, decoded.A1Values, 0, 3);
            Array.Copy(CDATIPResponse, 24, decoded.A1Values, 0, 3);
            Array.Copy(CDATIPResponse, 28, decoded.A1Values, 0, 3);

            decoded.Reserved7 = CDATIPResponse[19];
            decoded.Reserved8 = CDATIPResponse[23];
            decoded.Reserved9 = CDATIPResponse[27];
            decoded.Reserved10 = CDATIPResponse[31];

            return decoded;
        }

        public static string PrettifyCDATIP(CDATIP? CDATIPResponse)
        {
            if (CDATIPResponse == null)
                return null;

            CDATIP response = CDATIPResponse.Value;

            StringBuilder sb = new StringBuilder();

            if (response.DDCD)
            {
                sb.AppendFormat("Indicative Target Writing Power: 0x{0:X2}", response.ITWP).AppendLine();
                if (response.DiscType)
                    sb.AppendLine("Disc is DDCD-RW");
                else
                    sb.AppendLine("Disc is DDCD-R");
                switch (response.ReferenceSpeed)
                {
                    case 2:
                        sb.AppendLine("Reference speed is 4x");
                        break;
                    case 3:
                        sb.AppendLine("Reference speed is 8x");
                        break;
                    default:
                        sb.AppendFormat("Reference speed set is unknown: {0}", response.ReferenceSpeed).AppendLine();
                        break;
                }
                sb.AppendFormat("ATIP Start time of Lead-in: 0x{0:X6}", (response.LeadInStartMin << 16) + (response.LeadInStartSec << 8) + response.LeadInStartFrame).AppendLine();
                sb.AppendFormat("ATIP Last possible start time of Lead-out: 0x{0:X6}", (response.LeadOutStartMin << 16) + (response.LeadOutStartSec << 8) + response.LeadOutStartFrame).AppendLine();
                sb.AppendFormat("S4 value: 0x{0:X6}", (response.S4Values[0] << 16) + (response.S4Values[1] << 8) + response.S4Values[2]).AppendLine();
            }
            else
            {
                sb.AppendFormat("Indicative Target Writing Power: 0x{0:X2}", response.ITWP & 0x07).AppendLine();
                if (response.DiscType)
                {
                    switch (response.DiscSubType)
                    {
                        case 0:
                            sb.AppendLine("Disc is CD-RW");
                            break;
                        case 1:
                            sb.AppendLine("Disc is High-Speed CD-RW");
                            break;
                        default:
                            sb.AppendFormat("Unknown CD-RW disc subtype: {0}", response.DiscSubType).AppendLine();
                            break;
                    }
                    switch (response.ReferenceSpeed)
                    {
                        case 1:
                            sb.AppendLine("Reference speed is 2x");
                            break;
                        default:
                            sb.AppendFormat("Reference speed set is unknown: {0}", response.ReferenceSpeed).AppendLine();
                            break;
                    }
                }
                else
                {
                    sb.AppendLine("Disc is CD-R");
                    switch (response.DiscSubType)
                    {
                        default:
                            sb.AppendFormat("Unknown CD-R disc subtype: {0}", response.DiscSubType).AppendLine();
                            break;
                    }
                }

                if (response.URU)
                    sb.AppendLine("Disc use is unrestricted");
                else
                    sb.AppendLine("Disc use is restricted");

                sb.AppendFormat("ATIP Start time of Lead-in: {0:X2}:{1:X2}:{2:X2}", response.LeadInStartMin, response.LeadInStartSec, response.LeadInStartFrame).AppendLine();
                sb.AppendFormat("ATIP Last possible start time of Lead-out: {0:X2}:{1:X2}:{2:X2}", response.LeadOutStartMin, response.LeadOutStartSec, response.LeadOutStartFrame).AppendLine();
                if(response.A1Valid)
                    sb.AppendFormat("A1 value: 0x{0:X6}", (response.A1Values[0] << 16) + (response.A1Values[1] << 8) + response.A1Values[2]).AppendLine();
                if(response.A2Valid)
                    sb.AppendFormat("A2 value: 0x{0:X6}", (response.A2Values[0] << 16) + (response.A2Values[1] << 8) + response.A2Values[2]).AppendLine();
                if(response.A3Valid)
                    sb.AppendFormat("A3 value: 0x{0:X6}", (response.A3Values[0] << 16) + (response.A3Values[1] << 8) + response.A3Values[2]).AppendLine();
                sb.AppendFormat("S4 value: 0x{0:X6}", (response.S4Values[0] << 16) + (response.S4Values[1] << 8) + response.S4Values[2]).AppendLine();
            }

            return sb.ToString();
        }

        public static string PrettifyCDATIP(byte[] CDATIPResponse)
        {
            CDATIP? decoded = DecodeCDATIP(CDATIPResponse);
            return PrettifyCDATIP(decoded);
        }

        public static CDTextLeadIn? DecodeCDTextLeadIn(byte[] CDTextResponse)
        {
            if (CDTextResponse == null)
                return null;

            CDTextLeadIn decoded = new CDTextLeadIn();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.DataLength = BigEndianBitConverter.ToUInt16(CDTextResponse, 0);
            decoded.Reserved1 = CDTextResponse[2];
            decoded.Reserved2 = CDTextResponse[3];
            decoded.DataPacks = new CDTextPack[(decoded.DataLength - 2) / 18];

            if (decoded.DataLength + 2 != CDTextResponse.Length)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (CD-TEXT Decoder): Expected CD-TEXT size ({0} bytes) is not received size ({1} bytes), not decoding", decoded.DataLength + 2, CDTextResponse.Length);
                return null;
            }

            for (int i = 0; i < ((decoded.DataLength - 2) / 18); i++)
            {
                decoded.DataPacks[i].HeaderID1 = CDTextResponse[0 + i * 18 + 4];
                decoded.DataPacks[i].HeaderID2 = CDTextResponse[1 + i * 18 + 4];
                decoded.DataPacks[i].HeaderID3 = CDTextResponse[2 + i * 18 + 4];
                decoded.DataPacks[i].DBCC = Convert.ToBoolean(CDTextResponse[3 + i * 8 + 4] & 0x80);
                decoded.DataPacks[i].BlockNumber = (byte)((CDTextResponse[3 + i * 8 + 4] & 0x70) >> 4);
                decoded.DataPacks[i].CharacterPosition = (byte)(CDTextResponse[3 + i * 8 + 4] & 0x0F);
                decoded.DataPacks[i].TextDataField = new byte[12];
                Array.Copy(CDTextResponse, 4, decoded.DataPacks[i].TextDataField, 0, 12);
                decoded.DataPacks[i].CRC = BigEndianBitConverter.ToUInt16(CDTextResponse, 16 + i * 8 + 4);
            }

            return decoded;
        }

        public static string PrettifyCDTextLeadIn(CDTextLeadIn? CDTextResponse)
        {
            if (CDTextResponse == null)
                return null;

            CDTextLeadIn response = CDTextResponse.Value;

            StringBuilder sb = new StringBuilder();

            if (MainClass.isDebug)
            {
                sb.AppendFormat("Reserved1: 0x{0:X2}", response.Reserved1).AppendLine();
                sb.AppendFormat("Reserved2: 0x{0:X2}", response.Reserved2).AppendLine();
            }
            foreach (CDTextPack descriptor in response.DataPacks)
            {
                if ((descriptor.HeaderID1 & 0x80) != 0x80)
                    sb.AppendFormat("Incorrect CD-Text pack type {0}, not decoding", descriptor.HeaderID1).AppendLine();
                else
                {
                    switch (descriptor.HeaderID1)
                    {
                        case 0x80:
                            {
                                sb.Append("CD-Text pack contains title for ");
                                if (descriptor.HeaderID2 == 0x00)
                                    sb.AppendLine("album");
                                else
                                    sb.AppendFormat("track {0}", descriptor.HeaderID2).AppendLine();
                                break;
                            }
                        case 0x81:
                            {
                                sb.Append("CD-Text pack contains performer for ");
                                if (descriptor.HeaderID2 == 0x00)
                                    sb.AppendLine("album");
                                else
                                    sb.AppendFormat("track {0}", descriptor.HeaderID2).AppendLine();
                                break;
                            }
                        case 0x82:
                            {
                                sb.Append("CD-Text pack contains songwriter for ");
                                if (descriptor.HeaderID2 == 0x00)
                                    sb.AppendLine("album");
                                else
                                    sb.AppendFormat("track {0}", descriptor.HeaderID2).AppendLine();
                                break;
                            }
                        case 0x83:
                            {
                                sb.Append("CD-Text pack contains composer for ");
                                if (descriptor.HeaderID2 == 0x00)
                                    sb.AppendLine("album");
                                else
                                    sb.AppendFormat("track {0}", descriptor.HeaderID2).AppendLine();
                                break;
                            }
                        case 0x84:
                            {
                                sb.Append("CD-Text pack contains arranger for ");
                                if (descriptor.HeaderID2 == 0x00)
                                    sb.AppendLine("album");
                                else
                                    sb.AppendFormat("track {0}", descriptor.HeaderID2).AppendLine();
                                break;
                            }
                        case 0x85:
                            {
                                sb.Append("CD-Text pack contains content provider's message for ");
                                if (descriptor.HeaderID2 == 0x00)
                                    sb.AppendLine("album");
                                else
                                    sb.AppendFormat("track {0}", descriptor.HeaderID2).AppendLine();
                                break;
                            }
                        case 0x86:
                            {
                                sb.AppendLine("CD-Text pack contains disc identification information");
                                break;
                            }
                        case 0x87:
                            {
                                sb.AppendLine("CD-Text pack contains genre identification information");
                                break;
                            }
                        case 0x88:
                            {
                                sb.AppendLine("CD-Text pack contains table of contents information");
                                break;
                            }
                        case 0x89:
                            {
                                sb.AppendLine("CD-Text pack contains second table of contents information");
                                break;
                            }
                        case 0x8A:
                        case 0x8B:
                        case 0x8C:
                            {
                                sb.AppendLine("CD-Text pack contains reserved data");
                                break;
                            }
                        case 0x8D:
                            {
                                sb.AppendLine("CD-Text pack contains data reserved for content provider only");
                                break;
                            }
                        case 0x8E:
                            {
                                if (descriptor.HeaderID2 == 0x00)
                                    sb.AppendLine("CD-Text pack contains UPC");
                                else
                                    sb.AppendFormat("CD-Text pack contains ISRC for track {0}", descriptor.HeaderID2).AppendLine();
                                break;
                            }
                        case 0x8F:
                            {
                                sb.AppendLine("CD-Text pack contains size block information");
                                break;
                            }
                    }

                    switch (descriptor.HeaderID1)
                    {
                        case 0x80:
                        case 0x81:
                        case 0x82:
                        case 0x83:
                        case 0x84:
                        case 0x85:
                        case 0x86:
                        case 0x87:
                        case 0x8E:
                            {
                                if (descriptor.DBCC)
                                    sb.AppendLine("Double Byte Character Code is used");
                                sb.AppendFormat("Block number {0}", descriptor.BlockNumber).AppendLine();
                                sb.AppendFormat("Character position {0}", descriptor.CharacterPosition).AppendLine();
                                sb.AppendFormat("Text field: \"{0}\"", StringHandlers.CToString(descriptor.TextDataField)).AppendLine();
                                break;
                            }
                        default:
                            {
                                sb.AppendFormat("Binary contents: {0}", PrintHex.ByteArrayToHexArrayString(descriptor.TextDataField, 28)).AppendLine();
                                break;
                            }
                    }

                    sb.AppendFormat("CRC: 0x{0:X4}", descriptor.CRC).AppendLine();
                }
            }

            return sb.ToString();
        }

        public static string PrettifyCDTextLeadIn(byte[] CDTextResponse)
        {
            CDTextLeadIn? decoded = DecodeCDTextLeadIn(CDTextResponse);
            return PrettifyCDTextLeadIn(decoded);
        }

        #endregion Public methods

        #region Public structures

        public struct CDTOC
        {
            /// <summary>
            /// Total size of returned TOC minus this field
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// First track number in hex
            /// </summary>
            public byte FirstTrack;
            /// <summary>
            /// Last track number in hex
            /// </summary>
            public byte LastTrack;
            /// <summary>
            /// Track descriptors
            /// </summary>
            public CDTOCTrackDataDescriptor[] TrackDescriptors;
        }

        public struct CDTOCTrackDataDescriptor
        {
            /// <summary>
            /// Byte 0
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 1, bits 7 to 4
            /// Type of information in Q subchannel of block where this TOC entry was found
            /// </summary>
            public byte ADR;
            /// <summary>
            /// Byte 1, bits 3 to 0
            /// Track attributes
            /// </summary>
            public byte CONTROL;
            /// <summary>
            /// Byte 2
            /// Track number
            /// </summary>
            public byte TrackNumber;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Bytes 4 to 7
            /// The track start address in LBA or in MSF
            /// </summary>
            public UInt32 TrackStartAddress;
        }

        public struct CDSessionInfo
        {
            /// <summary>
            /// Total size of returned session information minus this field
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// First track number in hex
            /// </summary>
            public byte FirstCompleteSession;
            /// <summary>
            /// Last track number in hex
            /// </summary>
            public byte LastCompleteSession;
            /// <summary>
            /// Track descriptors
            /// </summary>
            public CDSessionInfoTrackDataDescriptor[] TrackDescriptors;
        }

        public struct CDSessionInfoTrackDataDescriptor
        {
            /// <summary>
            /// Byte 0
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 1, bits 7 to 4
            /// Type of information in Q subchannel of block where this TOC entry was found
            /// </summary>
            public byte ADR;
            /// <summary>
            /// Byte 1, bits 3 to 0
            /// Track attributes
            /// </summary>
            public byte CONTROL;
            /// <summary>
            /// Byte 2
            /// First track number in last complete session
            /// </summary>
            public byte TrackNumber;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Bytes 4 to 7
            /// First track number in last complete session start address in LBA or in MSF
            /// </summary>
            public UInt32 TrackStartAddress;
        }

        public struct CDFullTOC
        {
            /// <summary>
            /// Total size of returned session information minus this field
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// First complete session number in hex
            /// </summary>
            public byte FirstCompleteSession;
            /// <summary>
            /// Last complete session number in hex
            /// </summary>
            public byte LastCompleteSession;
            /// <summary>
            /// Track descriptors
            /// </summary>
            public CDFullTOCInfoTrackDataDescriptor[] TrackDescriptors;
        }

        public struct CDFullTOCInfoTrackDataDescriptor
        {
            /// <summary>
            /// Byte 0
            /// Session number in hex
            /// </summary>
            public byte SessionNumber;
            /// <summary>
            /// Byte 1, bits 7 to 4
            /// Type of information in Q subchannel of block where this TOC entry was found
            /// </summary>
            public byte ADR;
            /// <summary>
            /// Byte 1, bits 3 to 0
            /// Track attributes
            /// </summary>
            public byte CONTROL;
            /// <summary>
            /// Byte 2
            /// </summary>
            public byte TNO;
            /// <summary>
            /// Byte 3
            /// </summary>
            public byte POINT;
            /// <summary>
            /// Byte 4
            /// </summary>
            public byte Min;
            /// <summary>
            /// Byte 5
            /// </summary>
            public byte Sec;
            /// <summary>
            /// Byte 6
            /// </summary>
            public byte Frame;
            /// <summary>
            /// Byte 7, CD only
            /// </summary>
            public byte Zero;
            /// <summary>
            /// Byte 7, bits 7 to 4, DDCD only
            /// </summary>
            public byte HOUR;
            /// <summary>
            /// Byte 7, bits 3 to 0, DDCD only
            /// </summary>
            public byte PHOUR;
            /// <summary>
            /// Byte 8
            /// </summary>
            public byte PMIN;
            /// <summary>
            /// Byte 9
            /// </summary>
            public byte PSEC;
            /// <summary>
            /// Byte 10
            /// </summary>
            public byte PFRAME;
        }

        public struct CDPMA
        {
            /// <summary>
            /// Total size of returned session information minus this field
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Track descriptors
            /// </summary>
            public CDPMADescriptors[] PMADescriptors;
        }

        public struct CDPMADescriptors
        {
            /// <summary>
            /// Byte 0
            /// Reserved
            /// </summary>
            public byte Reserved;
            /// <summary>
            /// Byte 1, bits 7 to 4
            /// Type of information in Q subchannel of block where this TOC entry was found
            /// </summary>
            public byte ADR;
            /// <summary>
            /// Byte 1, bits 3 to 0
            /// Track attributes
            /// </summary>
            public byte CONTROL;
            /// <summary>
            /// Byte 2
            /// </summary>
            public byte TNO;
            /// <summary>
            /// Byte 3
            /// </summary>
            public byte POINT;
            /// <summary>
            /// Byte 4
            /// </summary>
            public byte Min;
            /// <summary>
            /// Byte 5
            /// </summary>
            public byte Sec;
            /// <summary>
            /// Byte 6
            /// </summary>
            public byte Frame;
            /// <summary>
            /// Byte 7, bits 7 to 4
            /// </summary>
            public byte HOUR;
            /// <summary>
            /// Byte 7, bits 3 to 0
            /// </summary>
            public byte PHOUR;
            /// <summary>
            /// Byte 8
            /// </summary>
            public byte PMIN;
            /// <summary>
            /// Byte 9
            /// </summary>
            public byte PSEC;
            /// <summary>
            /// Byte 10
            /// </summary>
            public byte PFRAME;
        }

        public struct CDATIP
        {
            /// <summary>
            /// Bytes 1 to 0
            /// Total size of returned session information minus this field
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// Byte 4, bits 7 to 4
            /// Indicative target writing power
            /// </summary>
            public byte ITWP;
            /// <summary>
            /// Byte 4, bit 3
            /// Set if DDCD
            /// </summary>
            public bool DDCD;
            /// <summary>
            /// Byte 4, bits 2 to 0
            /// Reference speed
            /// </summary>
            public byte ReferenceSpeed;
            /// <summary>
            /// Byte 5, bit 7
            /// Always unset
            /// </summary>
            public bool AlwaysZero;
            /// <summary>
            /// Byte 5, bit 6
            /// Unrestricted media
            /// </summary>
            public bool URU;
            /// <summary>
            /// Byte 5, bits 5 to 0
            /// Reserved
            /// </summary>
            public byte Reserved3;
            /// <summary>
            /// Byte 6, bit 7
            /// Always set
            /// </summary>
            public bool AlwaysOne;
            /// <summary>
            /// Byte 6, bit 6
            /// Set if rewritable (CD-RW or DDCD-RW)
            /// </summary>
            public bool DiscType;
            /// <summary>
            /// Byte 6, bits 5 to 3
            /// Disc subtype
            /// </summary>
            public byte DiscSubType;
            /// <summary>
            /// Byte 6, bit 2
            /// A1 values are valid
            /// </summary>
            public bool A1Valid;
            /// <summary>
            /// Byte 6, bit 1
            /// A2 values are valid
            /// </summary>
            public bool A2Valid;
            /// <summary>
            /// Byte 6, bit 0
            /// A3 values are valid
            /// </summary>
            public bool A3Valid;
            /// <summary>
            /// Byte 7
            /// Reserved
            /// </summary>
            public byte Reserved4;
            /// <summary>
            /// Byte 8
            /// ATIP Start time of Lead-In (Minute)
            /// </summary>
            public byte LeadInStartMin;
            /// <summary>
            /// Byte 9
            /// ATIP Start time of Lead-In (Second)
            /// </summary>
            public byte LeadInStartSec;
            /// <summary>
            /// Byte 10
            /// ATIP Start time of Lead-In (Frame)
            /// </summary>
            public byte LeadInStartFrame;
            /// <summary>
            /// Byte 11
            /// Reserved
            /// </summary>
            public byte Reserved5;
            /// <summary>
            /// Byte 12
            /// ATIP Last possible start time of Lead-Out (Minute)
            /// </summary>
            public byte LeadOutStartMin;
            /// <summary>
            /// Byte 13
            /// ATIP Last possible start time of Lead-Out (Second)
            /// </summary>
            public byte LeadOutStartSec;
            /// <summary>
            /// Byte 14
            /// ATIP Last possible start time of Lead-Out (Frame)
            /// </summary>
            public byte LeadOutStartFrame;
            /// <summary>
            /// Byte 15
            /// Reserved
            /// </summary>
            public byte Reserved6;
            /// <summary>
            /// Bytes 16 to 18
            /// A1 values
            /// </summary>
            public byte[] A1Values;
            /// <summary>
            /// Byte 19
            /// Reserved
            /// </summary>
            public byte Reserved7;
            /// <summary>
            /// Bytes 20 to 22
            /// A2 values
            /// </summary>
            public byte[] A2Values;
            /// <summary>
            /// Byte 23
            /// Reserved
            /// </summary>
            public byte Reserved8;
            /// <summary>
            /// Bytes 24 to 26
            /// A3 values
            /// </summary>
            public byte[] A3Values;
            /// <summary>
            /// Byte 27
            /// Reserved
            /// </summary>
            public byte Reserved9;
            /// <summary>
            /// Bytes 28 to 30
            /// S4 values
            /// </summary>
            public byte[] S4Values;
            /// <summary>
            /// Byte 31
            /// Reserved
            /// </summary>
            public byte Reserved10;
        }

        public struct CDTextLeadIn
        {
            /// <summary>
            /// Total size of returned CD-Text information minus this field
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Reserved
            /// </summary>
            public byte Reserved2;
            /// <summary>
            /// CD-Text data packs
            /// </summary>
            public CDTextPack[] DataPacks;
        }

        public struct CDTextPack
        {
            /// <summary>
            /// Byte 0
            /// Pack ID1 (Pack Type)
            /// </summary>
            public byte HeaderID1;
            /// <summary>
            /// Byte 1
            /// Pack ID2 (Track number)
            /// </summary>
            public byte HeaderID2;
            /// <summary>
            /// Byte 2
            /// Pack ID3
            /// </summary>
            public byte HeaderID3;
            /// <summary>
            /// Byte 3, bit 7
            /// Double Byte Character Code
            /// </summary>
            public bool DBCC;
            /// <summary>
            /// Byte 3, bits 6 to 4
            /// Block number
            /// </summary>
            public byte BlockNumber;
            /// <summary>
            /// Byte 3, bits 3 to 0
            /// Character position
            /// </summary>
            public byte CharacterPosition;
            /// <summary>
            /// Bytes 4 to 15
            /// Text data
            /// </summary>
            public byte[] TextDataField;
            /// <summary>
            /// Bytes 16 to 17
            /// CRC16
            /// </summary>
            public UInt16 CRC;
        }

        #endregion Public structures
    }
}

