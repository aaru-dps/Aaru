// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CDTextOnLeadIn.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes CD-TEXT on Lead-In.
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

using System;
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
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class CDTextOnLeadIn
{
#region PackTypeIndicator enum

    public enum PackTypeIndicator : byte
    {
        /// <summary>Title of the track (or album if track == 0)</summary>
        Title = 0x80,
        /// <summary>Performer</summary>
        Performer = 0x81,
        /// <summary>Songwriter</summary>
        Songwriter = 0x82,
        /// <summary>Composer</summary>
        Composer = 0x83,
        /// <summary>Arranger</summary>
        Arranger = 0x84,
        /// <summary>Message from the content provider or artist</summary>
        Message = 0x85,
        /// <summary>Disc identification information</summary>
        DiscIdentification = 0x86,
        /// <summary>Genre identification</summary>
        GenreIdentification = 0x87,
        /// <summary>Table of content information</summary>
        TOCInformation = 0x88,
        /// <summary>Second table of content information</summary>
        SecondTOCInformation = 0x89,
        /// <summary>Reserved</summary>
        Reserved1 = 0x8A,
        /// <summary>Reserved</summary>
        Reserved2 = 0x8B,
        /// <summary>Reserved</summary>
        Reserved3 = 0x8C,
        /// <summary>Reserved for content provider only</summary>
        ReservedForContentProvider = 0x8D,
        /// <summary>UPC of album or ISRC of track</summary>
        UPCorISRC = 0x8E,
        /// <summary>Size information of the block</summary>
        BlockSizeInformation = 0x8F
    }

#endregion

    const string MODULE_NAME = "CD-TEXT decoder";

    public static CDText? Decode(byte[] CDTextResponse)
    {
        if(CDTextResponse is not { Length: > 4 })
            return null;

        var decoded = new CDText
        {
            DataLength = BigEndianBitConverter.ToUInt16(CDTextResponse, 0),
            Reserved1  = CDTextResponse[2],
            Reserved2  = CDTextResponse[3]
        };

        decoded.DataPacks = new CDTextPack[(decoded.DataLength - 2) / 18];

        if(decoded.DataLength == 2)
            return null;

        if(decoded.DataLength + 2 != CDTextResponse.Length)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME,
                                       Localization.
                                           Expected_CD_TEXT_size_0_bytes_is_not_received_size_1_bytes_not_decoding,
                                       decoded.DataLength + 2, CDTextResponse.Length);

            return null;
        }

        for(var i = 0; i < (decoded.DataLength - 2) / 18; i++)
        {
            decoded.DataPacks[i].HeaderID1         = CDTextResponse[0 + i * 18 + 4];
            decoded.DataPacks[i].HeaderID2         = CDTextResponse[1 + i * 18 + 4];
            decoded.DataPacks[i].HeaderID3         = CDTextResponse[2 + i * 18 + 4];
            decoded.DataPacks[i].DBCC              = Convert.ToBoolean(CDTextResponse[3 + i * 18 + 4] & 0x80);
            decoded.DataPacks[i].BlockNumber       = (byte)((CDTextResponse[3 + i * 18 + 4] & 0x70) >> 4);
            decoded.DataPacks[i].CharacterPosition = (byte)(CDTextResponse[3 + i * 18 + 4] & 0x0F);
            decoded.DataPacks[i].TextDataField     = new byte[12];
            Array.Copy(CDTextResponse, 4 + i * 18 + 4, decoded.DataPacks[i].TextDataField, 0, 12);
            decoded.DataPacks[i].CRC = BigEndianBitConverter.ToUInt16(CDTextResponse, 16 + i * 18 + 4);
        }

        return decoded;
    }

    public static string Prettify(CDText? CDTextResponse)
    {
        if(CDTextResponse == null)
            return null;

        CDText response = CDTextResponse.Value;
        var    sb       = new StringBuilder();

    #if DEBUG
        if(response.Reserved1 != 0)
            sb.AppendFormat(Localization.Reserved1_equals_0_X8, response.Reserved1).AppendLine();

        if(response.Reserved2 != 0)
            sb.AppendFormat(Localization.Reserved2_equals_0_X8, response.Reserved2).AppendLine();
    #endif

        foreach(CDTextPack descriptor in response.DataPacks)
        {
            if((descriptor.HeaderID1 & 0x80) != 0x80)
            {
                // Ignore NOPs
                if((descriptor.HeaderID1 & 0x80) != 0)
                {
                    sb.AppendFormat(Localization.Incorrect_CD_Text_pack_type_0_not_decoding, descriptor.HeaderID1).
                       AppendLine();
                }
            }
            else
            {
                switch(descriptor.HeaderID1)
                {
                    case 0x80:
                    {
                        if(descriptor.HeaderID2 == 0x00)
                            sb.AppendLine(Localization.CD_Text_pack_contains_title_for_album);
                        else
                        {
                            sb.AppendFormat(Localization.CD_Text_pack_contains_title_for_track_0, descriptor.HeaderID2).
                               AppendLine();
                        }

                        break;
                    }

                    case 0x81:
                    {
                        if(descriptor.HeaderID2 == 0x00)
                            sb.AppendLine(Localization.CD_Text_pack_contains_performer_for_album);
                        else
                        {
                            sb.AppendFormat(Localization.CD_Text_pack_contains_performer_for_track_0,
                                            descriptor.HeaderID2).
                               AppendLine();
                        }

                        break;
                    }

                    case 0x82:
                    {
                        if(descriptor.HeaderID2 == 0x00)
                            sb.AppendLine(Localization.CD_Text_pack_contains_songwriter_for_album);
                        else
                        {
                            sb.AppendFormat(Localization.CD_Text_pack_contains_songwriter_for_track_0,
                                            descriptor.HeaderID2).
                               AppendLine();
                        }

                        break;
                    }

                    case 0x83:
                    {
                        if(descriptor.HeaderID2 == 0x00)
                            sb.AppendLine(Localization.album);
                        else
                            sb.AppendFormat(Localization.track_0, descriptor.HeaderID2).AppendLine();

                        break;
                    }

                    case 0x84:
                    {
                        if(descriptor.HeaderID2 == 0x00)
                            sb.AppendLine(Localization.CD_Text_pack_contains_arranger_for_album);
                        else
                        {
                            sb.AppendFormat(Localization.CD_Text_pack_contains_arranger_for_track_0,
                                            descriptor.HeaderID2).
                               AppendLine();
                        }

                        break;
                    }

                    case 0x85:
                    {
                        if(descriptor.HeaderID2 == 0x00)
                            sb.AppendLine(Localization.CD_Text_pack_contains_content_provider_message_for_album);
                        else
                        {
                            sb.AppendFormat(Localization.CD_Text_pack_contains_content_provider_message_for_track_0,
                                            descriptor.HeaderID2).
                               AppendLine();
                        }

                        break;
                    }

                    case 0x86:
                    {
                        sb.AppendLine(Localization.CD_Text_pack_contains_disc_identification_information);

                        break;
                    }

                    case 0x87:
                    {
                        sb.AppendLine(Localization.CD_Text_pack_contains_genre_identification_information);

                        break;
                    }

                    case 0x88:
                    {
                        sb.AppendLine(Localization.CD_Text_pack_contains_table_of_contents_information);

                        break;
                    }

                    case 0x89:
                    {
                        sb.AppendLine(Localization.CD_Text_pack_contains_second_table_of_contents_information);

                        break;
                    }

                    case 0x8A:
                    case 0x8B:
                    case 0x8C:
                    {
                        sb.AppendLine(Localization.CD_Text_pack_contains_reserved_data);

                        break;
                    }

                    case 0x8D:
                    {
                        sb.AppendLine(Localization.CD_Text_pack_contains_data_reserved_for_content_provider_only);

                        break;
                    }

                    case 0x8E:
                    {
                        if(descriptor.HeaderID2 == 0x00)
                            sb.AppendLine(Localization.CD_Text_pack_contains_UPC);
                        else
                            sb.AppendFormat(Localization.track_0, descriptor.HeaderID2).AppendLine();

                        break;
                    }

                    case 0x8F:
                    {
                        sb.AppendLine(Localization.CD_Text_pack_contains_size_block_information);

                        break;
                    }
                }

                switch(descriptor.HeaderID1)
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
                        if(descriptor.DBCC)
                            sb.AppendLine(Localization.Double_Byte_Character_Code_is_used);

                        sb.AppendFormat(Localization.Block_number_0,       descriptor.BlockNumber).AppendLine();
                        sb.AppendFormat(Localization.Character_position_0, descriptor.CharacterPosition).AppendLine();

                        sb.AppendFormat(Localization.Text_field_0,
                                        StringHandlers.CToString(descriptor.TextDataField,
                                                                 Encoding.GetEncoding("iso-8859-1"))).
                           AppendLine();

                        break;
                    }

                    default:
                    {
                        sb.AppendFormat(Localization.Binary_contents_0,
                                        PrintHex.ByteArrayToHexArrayString(descriptor.TextDataField, 28)).
                           AppendLine();

                        break;
                    }
                }

                sb.AppendFormat(Localization.CRC_0_X4, descriptor.CRC).AppendLine();
            }
        }

        return sb.ToString();
    }

    public static string Prettify(byte[] CDTextResponse)
    {
        CDText? decoded = Decode(CDTextResponse);

        return Prettify(decoded);
    }

#region Nested type: CDText

    public struct CDText
    {
        /// <summary>Total size of returned CD-Text information minus this field</summary>
        public ushort DataLength;
        /// <summary>Reserved</summary>
        public byte Reserved1;
        /// <summary>Reserved</summary>
        public byte Reserved2;
        /// <summary>CD-Text data packs</summary>
        public CDTextPack[] DataPacks;
    }

#endregion

#region Nested type: CDTextPack

    public struct CDTextPack
    {
        /// <summary>Byte 0 Pack ID1 (Pack Type)</summary>
        public byte HeaderID1;
        /// <summary>Byte 1 Pack ID2 (Track number)</summary>
        public byte HeaderID2;
        /// <summary>Byte 2 Pack ID3</summary>
        public byte HeaderID3;
        /// <summary>Byte 3, bit 7 Double Byte Character Code</summary>
        public bool DBCC;
        /// <summary>Byte 3, bits 6 to 4 Block number</summary>
        public byte BlockNumber;
        /// <summary>Byte 3, bits 3 to 0 Character position</summary>
        public byte CharacterPosition;
        /// <summary>Bytes 4 to 15 Text data</summary>
        public byte[] TextDataField;
        /// <summary>Bytes 16 to 17 CRC16</summary>
        public ushort CRC;
    }

#endregion
}