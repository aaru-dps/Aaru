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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Decoders.CD;

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Console;
using Aaru.Helpers;

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
            AaruConsole.DebugWriteLine("CD Session Info decoder",
                                       "Expected CDSessionInfo size ({0} bytes) is not received size ({1} bytes), not decoding",
                                       decoded.DataLength + 2, CDSessionInfoResponse.Length);

            return null;
        }

        for(var i = 0; i < (decoded.DataLength - 2) / 8; i++)
        {
            decoded.TrackDescriptors[i].Reserved1   = CDSessionInfoResponse[0 + i * 8 + 4];
            decoded.TrackDescriptors[i].ADR         = (byte)((CDSessionInfoResponse[1 + i * 8 + 4] & 0xF0) >> 4);
            decoded.TrackDescriptors[i].CONTROL     = (byte)(CDSessionInfoResponse[1 + i * 8 + 4] & 0x0F);
            decoded.TrackDescriptors[i].TrackNumber = CDSessionInfoResponse[2 + i * 8 + 4];
            decoded.TrackDescriptors[i].Reserved2   = CDSessionInfoResponse[3 + i * 8 + 4];

            decoded.TrackDescriptors[i].TrackStartAddress =
                BigEndianBitConverter.ToUInt32(CDSessionInfoResponse, 4 + i * 8 + 4);
        }

        return decoded;
    }

    public static string Prettify(CDSessionInfo? CDSessionInfoResponse)
    {
        if(CDSessionInfoResponse == null)
            return null;

        CDSessionInfo response = CDSessionInfoResponse.Value;

        var sb = new StringBuilder();

        sb.AppendFormat("First complete session number: {0}", response.FirstCompleteSession).AppendLine();
        sb.AppendFormat("Last complete session number: {0}", response.LastCompleteSession).AppendLine();

        foreach(TrackDataDescriptor descriptor in response.TrackDescriptors)
        {
            sb.AppendFormat("First track number in last complete session: {0}", descriptor.TrackNumber).AppendLine();

            sb.AppendFormat("Track starts at LBA {0}, or MSF {1:X2}:{2:X2}:{3:X2}", descriptor.TrackStartAddress,
                            (descriptor.TrackStartAddress & 0x0000FF00) >> 8,
                            (descriptor.TrackStartAddress & 0x00FF0000) >> 16,
                            (descriptor.TrackStartAddress & 0xFF000000) >> 24).AppendLine();

            switch((TocAdr)descriptor.ADR)
            {
                case TocAdr.NoInformation:
                    sb.AppendLine("Q subchannel mode not given");

                    break;
                case TocAdr.CurrentPosition:
                    sb.AppendLine("Q subchannel stores current position");

                    break;
                case TocAdr.ISRC:
                    sb.AppendLine("Q subchannel stores ISRC");

                    break;
                case TocAdr.MediaCatalogNumber:
                    sb.AppendLine("Q subchannel stores media catalog number");

                    break;
            }

            if((descriptor.CONTROL & (byte)TocControl.ReservedMask) == (byte)TocControl.ReservedMask)
                sb.AppendFormat("Reserved flags 0x{0:X2} set", descriptor.CONTROL).AppendLine();
            else
            {
                switch((TocControl)(descriptor.CONTROL & 0x0D))
                {
                    case TocControl.TwoChanNoPreEmph:
                        sb.AppendLine("Stereo audio track with no pre-emphasis");

                        break;
                    case TocControl.TwoChanPreEmph:
                        sb.AppendLine("Stereo audio track with 50/15 μs pre-emphasis");

                        break;
                    case TocControl.FourChanNoPreEmph:
                        sb.AppendLine("Quadraphonic audio track with no pre-emphasis");

                        break;
                    case TocControl.FourChanPreEmph:
                        sb.AppendLine("Stereo audio track with 50/15 μs pre-emphasis");

                        break;
                    case TocControl.DataTrack:
                        sb.AppendLine("Data track, recorded uninterrupted");

                        break;
                    case TocControl.DataTrackIncremental:
                        sb.AppendLine("Data track, recorded incrementally");

                        break;
                }

                sb.AppendLine((descriptor.CONTROL & (byte)TocControl.CopyPermissionMask) ==
                              (byte)TocControl.CopyPermissionMask ? "Digital copy of track is permitted"
                                  : "Digital copy of track is prohibited");

            #if DEBUG
                if(descriptor.Reserved1 != 0)
                    sb.AppendFormat("Reserved1 = 0x{0:X2}", descriptor.Reserved1).AppendLine();

                if(descriptor.Reserved2 != 0)
                    sb.AppendFormat("Reserved2 = 0x{0:X2}", descriptor.Reserved2).AppendLine();
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