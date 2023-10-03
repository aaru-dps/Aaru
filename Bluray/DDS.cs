// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DDS.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes Blu-ray Disc Definition Structure.
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

namespace Aaru.Decoders.Bluray;

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
[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "NotAccessedField.Global")]
public static class DDS
{
    #region Private constants
    /// <summary>Disc Definition Structure Identifier "DS"</summary>
    const ushort DDSIdentifier = 0x4453;
    const string MODULE_NAME = "BD DDS decoder";
    #endregion Private constants

    #region Public structures
    public struct DiscDefinitionStructure
    {
        /// <summary>Bytes 0 to 1 Data Length</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Bytes 4 to 5 "DS"</summary>
        public ushort Signature;
        /// <summary>Byte 6 DDS format</summary>
        public byte Format;
        /// <summary>Byte 7 Reserved</summary>
        public byte Reserved3;
        /// <summary>Bytes 8 to 11 DDS update count</summary>
        public uint UpdateCount;
        /// <summary>Bytes 12 to 19 Reserved</summary>
        public ulong Reserved4;
        /// <summary>Bytes 20 to 23 First PSN of Drive Area</summary>
        public uint DriveAreaPSN;
        /// <summary>Bytes 24 to 27 Reserved</summary>
        public uint Reserved5;
        /// <summary>Bytes 28 to 31 First PSN of Defect List</summary>
        public uint DefectListPSN;
        /// <summary>Bytes 32 to 35 Reserved</summary>
        public uint Reserved6;
        /// <summary>Bytes 36 to 39 PSN of LSN 0 of user data area</summary>
        public uint PSNofLSNZero;
        /// <summary>Bytes 40 to 43 Last LSN of user data area</summary>
        public uint LastUserAreaLSN;
        /// <summary>Bytes 44 to 47 ISA0 size</summary>
        public uint ISA0;
        /// <summary>Bytes 48 to 51 OSA size</summary>
        public uint OSA;
        /// <summary>Bytes 52 to 55 ISA1 size</summary>
        public uint ISA1;
        /// <summary>Byte 56 Spare Area full flags</summary>
        public byte SpareAreaFullFlags;
        /// <summary>Byte 57 Reserved</summary>
        public byte Reserved7;
        /// <summary>Byte 58 Disc type specific field</summary>
        public byte DiscTypeSpecificField1;
        /// <summary>Byte 59 Reserved</summary>
        public byte Reserved8;
        /// <summary>Byte 60 to 63 Disc type specific field</summary>
        public uint DiscTypeSpecificField2;
        /// <summary>Byte 64 to 67 Reserved</summary>
        public uint Reserved9;
        /// <summary>Bytes 68 to 99 Status bits of INFO1/2 and PAC1/2 on L0 and L1</summary>
        public byte[] StatusBits;
        /// <summary>Bytes 100 to end Disc type specific data</summary>
        public byte[] DiscTypeSpecificData;
    }
    #endregion Public structures

    #region Public methods
    public static DiscDefinitionStructure? Decode(byte[] DDSResponse)
    {
        if(DDSResponse == null)
            return null;

        var decoded = new DiscDefinitionStructure
        {
            DataLength = BigEndianBitConverter.ToUInt16(DDSResponse, 0),
            Reserved1  = DDSResponse[2],
            Reserved2  = DDSResponse[3],
            Signature  = BigEndianBitConverter.ToUInt16(DDSResponse, 4)
        };

        if(decoded.Signature != DDSIdentifier)
        {
            AaruConsole.DebugWriteLine(MODULE_NAME, Localization.Found_incorrect_DDS_signature_0,
                                       decoded.Signature);

            return null;
        }

        decoded.Format                 = DDSResponse[6];
        decoded.Reserved3              = DDSResponse[7];
        decoded.UpdateCount            = BigEndianBitConverter.ToUInt32(DDSResponse, 8);
        decoded.Reserved4              = BigEndianBitConverter.ToUInt64(DDSResponse, 12);
        decoded.DriveAreaPSN           = BigEndianBitConverter.ToUInt32(DDSResponse, 20);
        decoded.Reserved5              = BigEndianBitConverter.ToUInt32(DDSResponse, 24);
        decoded.DefectListPSN          = BigEndianBitConverter.ToUInt32(DDSResponse, 28);
        decoded.Reserved6              = BigEndianBitConverter.ToUInt32(DDSResponse, 32);
        decoded.PSNofLSNZero           = BigEndianBitConverter.ToUInt32(DDSResponse, 36);
        decoded.LastUserAreaLSN        = BigEndianBitConverter.ToUInt32(DDSResponse, 40);
        decoded.ISA0                   = BigEndianBitConverter.ToUInt32(DDSResponse, 44);
        decoded.OSA                    = BigEndianBitConverter.ToUInt32(DDSResponse, 48);
        decoded.ISA1                   = BigEndianBitConverter.ToUInt32(DDSResponse, 52);
        decoded.SpareAreaFullFlags     = DDSResponse[56];
        decoded.Reserved7              = DDSResponse[57];
        decoded.DiscTypeSpecificField1 = DDSResponse[58];
        decoded.Reserved8              = DDSResponse[59];
        decoded.DiscTypeSpecificField2 = BigEndianBitConverter.ToUInt32(DDSResponse, 60);
        decoded.Reserved9              = BigEndianBitConverter.ToUInt32(DDSResponse, 64);
        decoded.StatusBits             = new byte[32];
        Array.Copy(DDSResponse, 68, decoded.StatusBits, 0, 32);
        decoded.DiscTypeSpecificData = new byte[DDSResponse.Length - 100];
        Array.Copy(DDSResponse, 100, decoded.DiscTypeSpecificData, 0, DDSResponse.Length - 100);

        return decoded;
    }

    public static string Prettify(DiscDefinitionStructure? DDSResponse)
    {
        if(DDSResponse == null)
            return null;

        DiscDefinitionStructure response = DDSResponse.Value;

        var sb = new StringBuilder();

        sb.AppendFormat(Localization.DDS_Format_0, response.Format).AppendLine();
        sb.AppendFormat(Localization.DDS_has_been_updated_0_times, response.UpdateCount).AppendLine();
        sb.AppendFormat(Localization.First_PSN_of_Drive_Area_0, response.DriveAreaPSN).AppendLine();
        sb.AppendFormat(Localization.First_PSN_of_Defect_List_0, response.DefectListPSN).AppendLine();
        sb.AppendFormat(Localization.PSN_of_User_Data_Areas_LSN_0_0, response.PSNofLSNZero).AppendLine();
        sb.AppendFormat(Localization.Last_User_Data_Areas_LSN_0_0, response.LastUserAreaLSN).AppendLine();
        sb.AppendFormat(Localization.ISA0_size_0, response.ISA0).AppendLine();
        sb.AppendFormat(Localization.OSA_size_0, response.OSA).AppendLine();
        sb.AppendFormat(Localization.ISA1_size_0, response.ISA1).AppendLine();
        sb.AppendFormat(Localization.Spare_Area_Full_Flags_0, response.SpareAreaFullFlags).AppendLine();
        sb.AppendFormat(Localization.Disc_Type_Specific_Field_1_0, response.DiscTypeSpecificField1).AppendLine();
        sb.AppendFormat(Localization.Disc_Type_Specific_Field_2_0, response.DiscTypeSpecificField2).AppendLine();
        sb.AppendFormat(Localization.Blu_ray_DDS_Status_Bits_in_hex_follows);
        sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.StatusBits, 80));
        sb.AppendFormat(Localization.Blu_ray_DDS_Disc_Type_Specific_Data_in_hex_follows);
        sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.DiscTypeSpecificData, 80));

    #if DEBUG
        if(response.Reserved1 != 0)
            sb.AppendFormat(Localization.Reserved1_equals_0_X8, response.Reserved1).AppendLine();

        if(response.Reserved2 != 0)
            sb.AppendFormat(Localization.Reserved2_equals_0_X8, response.Reserved2).AppendLine();

        if(response.Reserved3 != 0)
            sb.AppendFormat(Localization.Reserved_3_equals_0_X2, response.Reserved3).AppendLine();

        if(response.Reserved4 != 0)
            sb.AppendFormat(Localization.Reserved4_equals_0_X16, response.Reserved4).AppendLine();

        if(response.Reserved5 != 0)
            sb.AppendFormat(Localization.Reserved5_equals_0_X8, response.Reserved5).AppendLine();

        if(response.Reserved6 != 0)
            sb.AppendFormat(Localization.Reserved6_equals_0_X8, response.Reserved6).AppendLine();

        if(response.Reserved7 != 0)
            sb.AppendFormat(Localization.Reserved7_equals_0_X2, response.Reserved7).AppendLine();

        if(response.Reserved8 != 0)
            sb.AppendFormat(Localization.Reserved8_equals_0_X2, response.Reserved8).AppendLine();

        if(response.Reserved9 != 0)
            sb.AppendFormat(Localization.Reserved9_equals_0_X8, response.Reserved9).AppendLine();
    #endif

        return sb.ToString();
    }

    public static string Prettify(byte[] DDSResponse) => Prettify(Decode(DDSResponse));
    #endregion Public methods
}