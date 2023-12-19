// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : AACS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes AACS structures.
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
using Aaru.Helpers;

namespace Aaru.Decoders.SCSI.MMC;

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
public static class AACS
{
    public static AACSVolumeIdentifier? DecodeAACSVolumeIdentifier(byte[] AACSVIResponse)
    {
        if(AACSVIResponse == null)
            return null;

        var decoded = new AACSVolumeIdentifier
        {
            VolumeIdentifier = new byte[AACSVIResponse.Length - 4],
            DataLength       = BigEndianBitConverter.ToUInt16(AACSVIResponse, 0),
            Reserved1        = AACSVIResponse[2],
            Reserved2        = AACSVIResponse[3]
        };

        Array.Copy(AACSVIResponse, 4, decoded.VolumeIdentifier, 0, AACSVIResponse.Length - 4);

        return decoded;
    }

    public static string PrettifyAACSVolumeIdentifier(AACSVolumeIdentifier? AACSVIResponse)
    {
        if(AACSVIResponse == null)
            return null;

        AACSVolumeIdentifier response = AACSVIResponse.Value;

        var sb = new StringBuilder();

    #if DEBUG
        if(response.Reserved1 != 0)
            sb.AppendFormat(Localization.Reserved1_equals_0_X8, response.Reserved1).AppendLine();

        if(response.Reserved2 != 0)
            sb.AppendFormat(Localization.Reserved2_equals_0_X8, response.Reserved2).AppendLine();
    #endif
        sb.AppendFormat(Localization.AACS_Volume_Identifier_in_hex_follows);
        sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.VolumeIdentifier, 80));

        return sb.ToString();
    }

    public static string PrettifyAACSVolumeIdentifier(byte[] AACSVIResponse)
    {
        AACSVolumeIdentifier? decoded = DecodeAACSVolumeIdentifier(AACSVIResponse);

        return PrettifyAACSVolumeIdentifier(decoded);
    }

    public static AACSMediaSerialNumber? DecodeAACSMediaSerialNumber(byte[] AACSMSNResponse)
    {
        if(AACSMSNResponse == null)
            return null;

        var decoded = new AACSMediaSerialNumber
        {
            MediaSerialNumber = new byte[AACSMSNResponse.Length - 4],
            DataLength        = BigEndianBitConverter.ToUInt16(AACSMSNResponse, 0),
            Reserved1         = AACSMSNResponse[2],
            Reserved2         = AACSMSNResponse[3]
        };

        Array.Copy(AACSMSNResponse, 4, decoded.MediaSerialNumber, 0, AACSMSNResponse.Length - 4);

        return decoded;
    }

    public static string PrettifyAACSMediaSerialNumber(AACSMediaSerialNumber? AACSMSNResponse)
    {
        if(AACSMSNResponse == null)
            return null;

        AACSMediaSerialNumber response = AACSMSNResponse.Value;

        var sb = new StringBuilder();

    #if DEBUG
        if(response.Reserved1 != 0)
            sb.AppendFormat(Localization.Reserved1_equals_0_X8, response.Reserved1).AppendLine();

        if(response.Reserved2 != 0)
            sb.AppendFormat(Localization.Reserved2_equals_0_X8, response.Reserved2).AppendLine();
    #endif
        sb.AppendFormat(Localization.AACS_Media_Serial_Number_in_hex_follows);
        sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.MediaSerialNumber, 80));

        return sb.ToString();
    }

    public static string PrettifyAACSMediaSerialNumber(byte[] AACSMSNResponse)
    {
        AACSMediaSerialNumber? decoded = DecodeAACSMediaSerialNumber(AACSMSNResponse);

        return PrettifyAACSMediaSerialNumber(decoded);
    }

    public static AACSMediaIdentifier? DecodeAACSMediaIdentifier(byte[] AACSMIResponse)
    {
        if(AACSMIResponse == null)
            return null;

        var decoded = new AACSMediaIdentifier
        {
            MediaIdentifier = new byte[AACSMIResponse.Length - 4],
            DataLength      = BigEndianBitConverter.ToUInt16(AACSMIResponse, 0),
            Reserved1       = AACSMIResponse[2],
            Reserved2       = AACSMIResponse[3]
        };

        Array.Copy(AACSMIResponse, 4, decoded.MediaIdentifier, 0, AACSMIResponse.Length - 4);

        return decoded;
    }

    public static string PrettifyAACSMediaIdentifier(AACSMediaIdentifier? AACSMIResponse)
    {
        if(AACSMIResponse == null)
            return null;

        AACSMediaIdentifier response = AACSMIResponse.Value;

        var sb = new StringBuilder();

    #if DEBUG
        if(response.Reserved1 != 0)
            sb.AppendFormat(Localization.Reserved1_equals_0_X8, response.Reserved1).AppendLine();

        if(response.Reserved2 != 0)
            sb.AppendFormat(Localization.Reserved2_equals_0_X8, response.Reserved2).AppendLine();
    #endif
        sb.AppendFormat(Localization.AACS_Media_Identifier_in_hex_follows);
        sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.MediaIdentifier, 80));

        return sb.ToString();
    }

    public static string PrettifyAACSMediaIdentifier(byte[] AACSMIResponse)
    {
        AACSMediaIdentifier? decoded = DecodeAACSMediaIdentifier(AACSMIResponse);

        return PrettifyAACSMediaIdentifier(decoded);
    }

    public static AACSMediaKeyBlock? DecodeAACSMediaKeyBlock(byte[] AACSMKBResponse)
    {
        if(AACSMKBResponse == null)
            return null;

        var decoded = new AACSMediaKeyBlock
        {
            MediaKeyBlockPacks = new byte[AACSMKBResponse.Length - 4],
            DataLength         = BigEndianBitConverter.ToUInt16(AACSMKBResponse, 0),
            Reserved           = AACSMKBResponse[2],
            TotalPacks         = AACSMKBResponse[3]
        };

        Array.Copy(AACSMKBResponse, 4, decoded.MediaKeyBlockPacks, 0, AACSMKBResponse.Length - 4);

        return decoded;
    }

    public static string PrettifyAACSMediaKeyBlock(AACSMediaKeyBlock? AACSMKBResponse)
    {
        if(AACSMKBResponse == null)
            return null;

        AACSMediaKeyBlock response = AACSMKBResponse.Value;

        var sb = new StringBuilder();

    #if DEBUG
        if(response.Reserved != 0)
            sb.AppendFormat(Localization.Reserved_equals_0_X2, response.Reserved).AppendLine();
    #endif
        sb.AppendFormat(Localization.Total_number_of_media_key_blocks_available_to_transfer_0, response.TotalPacks).
           AppendLine();

        sb.AppendFormat(Localization.AACS_Media_Key_Blocks_in_hex_follows);
        sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.MediaKeyBlockPacks, 80));

        return sb.ToString();
    }

    public static string PrettifyAACSMediaKeyBlock(byte[] AACSMKBResponse)
    {
        AACSMediaKeyBlock? decoded = DecodeAACSMediaKeyBlock(AACSMKBResponse);

        return PrettifyAACSMediaKeyBlock(decoded);
    }

    public static AACSDataKeys? DecodeAACSDataKeys(byte[] AACSDKResponse)
    {
        if(AACSDKResponse == null)
            return null;

        var decoded = new AACSDataKeys
        {
            DataKeys   = new byte[AACSDKResponse.Length - 4],
            DataLength = BigEndianBitConverter.ToUInt16(AACSDKResponse, 0),
            Reserved1  = AACSDKResponse[2],
            Reserved2  = AACSDKResponse[3]
        };

        Array.Copy(AACSDKResponse, 4, decoded.DataKeys, 0, AACSDKResponse.Length - 4);

        return decoded;
    }

    public static string PrettifyAACSDataKeys(AACSDataKeys? AACSDKResponse)
    {
        if(AACSDKResponse == null)
            return null;

        AACSDataKeys response = AACSDKResponse.Value;

        var sb = new StringBuilder();

    #if DEBUG
        if(response.Reserved1 != 0)
            sb.AppendFormat(Localization.Reserved1_equals_0_X8, response.Reserved1).AppendLine();

        if(response.Reserved2 != 0)
            sb.AppendFormat(Localization.Reserved2_equals_0_X8, response.Reserved2).AppendLine();
    #endif
        sb.AppendFormat(Localization.AACS_Data_Keys_in_hex_follows);
        sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.DataKeys, 80));

        return sb.ToString();
    }

    public static string PrettifyAACSDataKeys(byte[] AACSDKResponse)
    {
        AACSDataKeys? decoded = DecodeAACSDataKeys(AACSDKResponse);

        return PrettifyAACSDataKeys(decoded);
    }

    public static AACSLBAExtentsResponse? DecodeAACSLBAExtents(byte[] AACSLBAExtsResponse)
    {
        if(AACSLBAExtsResponse == null)
            return null;

        var decoded = new AACSLBAExtentsResponse
        {
            DataLength    = BigEndianBitConverter.ToUInt16(AACSLBAExtsResponse, 0),
            Reserved      = AACSLBAExtsResponse[2],
            MaxLBAExtents = AACSLBAExtsResponse[3]
        };

        if((AACSLBAExtsResponse.Length - 4) % 16 != 0)
            return decoded;

        decoded.Extents = new AACSLBAExtent[(AACSLBAExtsResponse.Length - 4) / 16];

        for(var i = 0; i < (AACSLBAExtsResponse.Length - 4) / 16; i++)
        {
            decoded.Extents[i].Reserved = new byte[8];
            Array.Copy(AACSLBAExtsResponse, 0 + i * 16 + 4, decoded.Extents[i].Reserved, 0, 8);
            decoded.Extents[i].StartLBA = BigEndianBitConverter.ToUInt32(AACSLBAExtsResponse, 8  + i * 16 + 4);
            decoded.Extents[i].LBACount = BigEndianBitConverter.ToUInt32(AACSLBAExtsResponse, 12 + i * 16 + 4);
        }

        return decoded;
    }

    public static string PrettifyAACSLBAExtents(AACSLBAExtentsResponse? AACSLBAExtsResponse)
    {
        if(AACSLBAExtsResponse == null)
            return null;

        AACSLBAExtentsResponse response = AACSLBAExtsResponse.Value;

        var sb = new StringBuilder();

        if(response.MaxLBAExtents == 0)
        {
            sb.AppendLine(response.DataLength > 2
                              ? Localization.Drive_can_store_256_LBA_Extents
                              : Localization.Drive_cannot_store_LBA_Extents);
        }
        else
            sb.AppendFormat(Localization.Drive_can_store_0_LBA_Extents, response.MaxLBAExtents).AppendLine();

        for(var i = 0; i < response.Extents.Length; i++)
        {
            sb.AppendFormat(Localization.LBA_Extent_0_starts_at_LBA_1_and_goes_for_2_sectors, i,
                            response.Extents[i].StartLBA, response.Extents[i].LBACount);
        }

        return sb.ToString();
    }

    public static string PrettifyAACSLBAExtents(byte[] AACSLBAExtsResponse)
    {
        AACSLBAExtentsResponse? decoded = DecodeAACSLBAExtents(AACSLBAExtsResponse);

        return PrettifyAACSLBAExtents(decoded);
    }

#region Nested type: AACSDataKeys

    public struct AACSDataKeys
    {
        /// <summary>Bytes 0 to 1 Data length</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Bytes 4 to end AACS data keys</summary>
        public byte[] DataKeys;
    }

#endregion

#region Nested type: AACSLBAExtent

    public struct AACSLBAExtent
    {
        /// <summary>Bytes 0 to 7 Reserved</summary>
        public byte[] Reserved;
        /// <summary>Bytes 8 to 11 Start LBA of extent</summary>
        public uint StartLBA;
        /// <summary>Bytes 12 to 15 Extent length</summary>
        public uint LBACount;
    }

#endregion

#region Nested type: AACSLBAExtentsResponse

    public struct AACSLBAExtentsResponse
    {
        /// <summary>Bytes 0 to 1 Data Length</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved;
        /// <summary>
        ///     Byte 3 Number of LBA extents the drive can store. if(MaxLBAExtents == 0 &amp;&amp; DataLength > 2), 256
        ///     extents can be stored
        /// </summary>
        public byte MaxLBAExtents;
        /// <summary>Bytes 4 to end LBA Extents</summary>
        public AACSLBAExtent[] Extents;
    }

#endregion

#region Nested type: AACSMediaIdentifier

    public struct AACSMediaIdentifier
    {
        /// <summary>Bytes 0 to 1 Data length</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Bytes 4 to end AACS media identifier data</summary>
        public byte[] MediaIdentifier;
    }

#endregion

#region Nested type: AACSMediaKeyBlock

    public struct AACSMediaKeyBlock
    {
        /// <summary>Bytes 0 to 1 Data length</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved;
        /// <summary>Byte 3 Number of MKB packs available to transfer</summary>
        public byte TotalPacks;
        /// <summary>Bytes 4 to end AACS media key block packs</summary>
        public byte[] MediaKeyBlockPacks;
    }

#endregion

#region Nested type: AACSMediaSerialNumber

    public struct AACSMediaSerialNumber
    {
        /// <summary>Bytes 0 to 1 Data length</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Bytes 4 to end AACS media serial number</summary>
        public byte[] MediaSerialNumber;
    }

#endregion

#region Nested type: AACSVolumeIdentifier

    public struct AACSVolumeIdentifier
    {
        /// <summary>Bytes 0 to 1 Data length</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Bytes 4 to end AACS volume identifier data</summary>
        public byte[] VolumeIdentifier;
    }

#endregion
}