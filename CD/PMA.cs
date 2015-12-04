// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : PMA.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;
using DiscImageChef.Console;
using System.Text;

namespace DiscImageChef.Decoders.CD
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
    public static class PMA
    {
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

        public static CDPMA? Decode(byte[] CDPMAResponse)
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
                DicConsole.DebugWriteLine("CD PMA decoder", "Expected CDPMA size ({0} bytes) is not received size ({1} bytes), not decoding", decoded.DataLength + 2, CDPMAResponse.Length);
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

        public static string Prettify(CDPMA? CDPMAResponse)
        {
            if (CDPMAResponse == null)
                return null;

            CDPMA response = CDPMAResponse.Value;

            StringBuilder sb = new StringBuilder();

            #if DEBUG
            if(response.Reserved1 != 0)
                sb.AppendFormat("Reserved1 = 0x{0:X2}", response.Reserved1).AppendLine();
            if(response.Reserved2 != 0)
                sb.AppendFormat("Reserved2 = 0x{0:X2}", response.Reserved2).AppendLine();
            #endif

            foreach (CDPMADescriptors descriptor in response.PMADescriptors)
            {
                #if DEBUG
                if(descriptor.Reserved != 0)
                    sb.AppendFormat("Reserved = 0x{0:X2}", descriptor.Reserved).AppendLine();
                #endif

                switch (descriptor.ADR)
                {
                    case 1:
                        if (descriptor.POINT > 0)
                        {
                            sb.AppendFormat("Track {0}", descriptor.POINT);
                            switch ((TOC_CONTROL)(descriptor.CONTROL & 0x0D))
                            {
                                case TOC_CONTROL.TwoChanNoPreEmph:
                                    sb.Append(" (Stereo audio track with no pre-emphasis)");
                                    break;
                                case TOC_CONTROL.TwoChanPreEmph:
                                    sb.Append(" (Stereo audio track with 50/15 μs pre-emphasis)");
                                    break;
                                case TOC_CONTROL.FourChanNoPreEmph:
                                    sb.Append(" (Quadraphonic audio track with no pre-emphasis)");
                                    break;
                                case TOC_CONTROL.FourChanPreEmph:
                                    sb.Append(" (Quadraphonic audio track with 50/15 μs pre-emphasis)");
                                    break;
                                case TOC_CONTROL.DataTrack:
                                    sb.Append(" (Data track, recorded uninterrupted)");
                                    break;
                                case TOC_CONTROL.DataTrackIncremental:
                                    sb.Append(" (Data track, recorded incrementally)");
                                    break;
                            }
                            if (descriptor.PHOUR > 0)
                                sb.AppendFormat(" starts at {3}:{0:D2}:{1:D2}:{2:D2}", descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME, descriptor.PHOUR);
                            else
                                sb.AppendFormat(" starts at {0:D2}:{1:D2}:{2:D2}", descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME);
                            if (descriptor.PHOUR > 0)
                                sb.AppendFormat(" and ends at {3}:{0:D2}:{1:D2}:{2:D2}", descriptor.Min, descriptor.Sec, descriptor.Frame, descriptor.HOUR);
                            else
                                sb.AppendFormat(" and ends at {0:D2}:{1:D2}:{2:D2}", descriptor.Min, descriptor.Sec, descriptor.Frame);
                        }
                        else
                            goto default;
                        break;
                    case 2:
                        uint id = (uint)((descriptor.Min << 16) + (descriptor.Sec << 8) + descriptor.Frame);
                        sb.AppendFormat("Disc ID: {0:X6}", id & 0x00FFFFFF).AppendLine();
                        break;
                    case 3:
                        sb.AppendFormat("Skip track assignment {0} says that tracks ", descriptor.POINT);
                        if(descriptor.Min > 0)
                            sb.AppendFormat("{0} ", descriptor.Min);
                        if(descriptor.Sec > 0)
                            sb.AppendFormat("{0} ", descriptor.Sec);
                        if(descriptor.Frame > 0)
                            sb.AppendFormat("{0} ", descriptor.Frame);
                        if(descriptor.PMIN > 0)
                            sb.AppendFormat("{0} ", descriptor.PMIN);
                        if(descriptor.PSEC > 0)
                            sb.AppendFormat("{0} ", descriptor.PSEC);
                        if(descriptor.PFRAME > 0)
                            sb.AppendFormat("{0} ", descriptor.PFRAME);
                        sb.AppendLine("should be skipped");
                        break;
                    case 4:
                        sb.AppendFormat("Unskip track assignment {0} says that tracks ", descriptor.POINT);
                        if(descriptor.Min > 0)
                            sb.AppendFormat("{0} ", descriptor.Min);
                        if(descriptor.Sec > 0)
                            sb.AppendFormat("{0} ", descriptor.Sec);
                        if(descriptor.Frame > 0)
                            sb.AppendFormat("{0} ", descriptor.Frame);
                        if(descriptor.PMIN > 0)
                            sb.AppendFormat("{0} ", descriptor.PMIN);
                        if(descriptor.PSEC > 0)
                            sb.AppendFormat("{0} ", descriptor.PSEC);
                        if(descriptor.PFRAME > 0)
                            sb.AppendFormat("{0} ", descriptor.PFRAME);
                        sb.AppendLine("should not be skipped");
                        break;
                    case 5:
                        sb.AppendFormat("Skip time interval assignment {0} says that from ", descriptor.POINT);
                        if(descriptor.PHOUR > 0)
                            sb.AppendFormat("{3}:{0:D2}:{1:D2}:{2:D2} to ", descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME, descriptor.PHOUR);
                        else
                            sb.AppendFormat("{0:D2}:{1:D2}:{2:D2} to ", descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME);
                        if(descriptor.PHOUR > 0)
                            sb.AppendFormat("{3}:{0:D2}:{1:D2}:{2:D2} ", descriptor.Min, descriptor.Sec, descriptor.Frame, descriptor.HOUR);
                        else
                            sb.AppendFormat("{0:D2}:{1:D2}:{2:D2} ", descriptor.Min, descriptor.Sec, descriptor.Frame);
                        sb.AppendLine("should be skipped");
                        break;
                    case 6:
                        sb.AppendFormat("Unskip time interval assignment {0} says that from ", descriptor.POINT);
                        if(descriptor.PHOUR > 0)
                            sb.AppendFormat("{3}:{0:D2}:{1:D2}:{2:D2} to ", descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME, descriptor.PHOUR);
                        else
                            sb.AppendFormat("{0:D2}:{1:D2}:{2:D2} to ", descriptor.PMIN, descriptor.PSEC, descriptor.PFRAME);
                        if(descriptor.PHOUR > 0)
                            sb.AppendFormat("{3}:{0:D2}:{1:D2}:{2:D2} ", descriptor.Min, descriptor.Sec, descriptor.Frame, descriptor.HOUR);
                        else
                            sb.AppendFormat("{0:D2}:{1:D2}:{2:D2} ", descriptor.Min, descriptor.Sec, descriptor.Frame);
                        sb.AppendLine("should not be skipped");
                        break;
                    default:

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
    }
}

