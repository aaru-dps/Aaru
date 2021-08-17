// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : PRI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes DVD pre-recorded information.
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
// Copyright © 2011-2021 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Helpers;

namespace Aaru.Decoders.DVD
{
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
    // ECMA 365
    [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class PRI
    {
        public static PreRecordedInformation? Decode(byte[] response)
        {
            if(response == null)
                return null;

            if(response.Length < 67)
                return null;

            var    pri = new PreRecordedInformation();
            byte[] tmp;

            pri.DataLength = (ushort)((response[0] << 8) + response[1]);
            pri.Reserved1  = response[2];
            pri.Reserved2  = response[3];

            pri.FieldId1 = response[4];
            pri.FieldId2 = response[12];
            pri.FieldId3 = response[20];
            pri.FieldId4 = response[28];
            pri.FieldId5 = response[36];

            if(pri.FieldId1 != 1 ||
               pri.FieldId2 != 2 ||
               pri.FieldId3 != 3 ||
               pri.FieldId4 != 4 ||
               pri.FieldId5 != 5)
                return null;

            pri.DiscApplicationCode             = response[5];
            pri.DiscPhysicalCode                = response[6];
            pri.LastAddressOfDataRecordableArea = (uint)((response[7] << 16) + (response[8] << 8) + response[9]);
            pri.PartVersion                     = (byte)(response[10] >> 4);
            pri.ExtensionCode                   = (byte)(response[10] & 0xF);
            pri.Reserved3                       = response[11];
            pri.OPCSuggestedCode                = response[13];
            pri.WaveLengthCode                  = response[14];

            pri.WriteStrategyCode =
                (uint)((response[15] << 24) + (response[16] << 16) + (response[17] << 8) + response[18]);

            pri.Reserved4       = response[19];
            pri.ManufacturerId1 = new byte[6];
            pri.Reserved5       = response[27];
            pri.ManufacturerId2 = new byte[6];
            pri.Reserved6       = response[35];
            pri.ManufacturerId3 = new byte[6];
            pri.Reserved7       = response[43];
            pri.Reserved8       = new byte[response.Length - 44];

            Array.Copy(response, 21, pri.ManufacturerId1, 0, 6);
            Array.Copy(response, 29, pri.ManufacturerId2, 0, 6);
            Array.Copy(response, 37, pri.ManufacturerId3, 0, 6);
            Array.Copy(response, 44, pri.Reserved8, 0, pri.Reserved8.Length);

            tmp = new byte[18];

            Array.Copy(response, 21, tmp, 0, 6);
            Array.Copy(response, 29, tmp, 6, 6);

            // If RW or has part version or has extension code, 3rd manufacturer ID is a write strategy code
            if((pri.DiscPhysicalCode & 0x2) > 0 ||
               pri.PartVersion              > 0 ||
               pri.ExtensionCode            > 0)
                pri.WriteStrategyCode2 =
                    (uint)((response[37] << 24) + (response[38] << 16) + (response[39] << 8) + response[40]);
            else
                Array.Copy(response, 37, tmp, 12, 6);

            pri.ManufacturerId = StringHandlers.CToString(tmp, Encoding.ASCII).Trim();

            return pri;
        }

        public static string Prettify(PreRecordedInformation? pri)
        {
            if(pri == null)
                return null;

            PreRecordedInformation decoded = pri.Value;
            var                    sb      = new StringBuilder();

            if((decoded.DiscApplicationCode & 0x40) > 0)
            {
                sb.AppendLine("Disc for unrestricted use.");

                if((decoded.DiscApplicationCode & 0x3F) > 0)
                    sb.AppendFormat("Invalid purpose field with value {0}", decoded.DiscApplicationCode & 0x3F).
                       AppendLine();
                else
                    sb.AppendLine("Consumer purpose disc for use in consumer purpose drives");
            }
            else
            {
                sb.AppendLine("Disc for restricted use.");

                if((decoded.DiscApplicationCode & 0x3F) > 0)
                    sb.AppendFormat("Disc for use in special drives according with purpose value {0}",
                                    decoded.DiscApplicationCode & 0x3F).AppendLine();
                else
                    sb.AppendLine("General purpose disc for use in general purpose drives");
            }

            sb.AppendLine((decoded.DiscPhysicalCode & 0x80) > 0 ? "Disc track pitch is 0,74 μm"
                              : "Unknown track pitch");

            sb.AppendLine((decoded.DiscPhysicalCode & 0x40) > 0 ? "Reference velocity is 3,49 m/s"
                              : "Unknown reference velocity");

            sb.AppendLine((decoded.DiscPhysicalCode & 0x20) > 0 ? "Disc has 80mm diameter" : "Disc has 120mm diameter");

            sb.AppendLine((decoded.DiscPhysicalCode & 0x10) > 0 ? "Disc reflectivity is between 18% and 30%"
                              : "Disc reflectivity is between 45% and 85%");

            sb.AppendLine((decoded.DiscPhysicalCode & 0x04) > 0 ? "Dye is organic" : "Dye is phase change");

            sb.AppendLine((decoded.DiscPhysicalCode & 0x02) > 0 ? "Disc is RW (rewritable)" : "Disc is R (recordable)");

            sb.AppendLine((decoded.DiscPhysicalCode & 0x01) > 0 ? "Wavelength is 650nm" : "Unknown wavelength");

            sb.AppendFormat("Last writable ECC block address: 0x{0:X6}", decoded.LastAddressOfDataRecordableArea).
               AppendLine();

            if(decoded.PartVersion > 0)
                sb.AppendFormat("Part version {0}", decoded.PartVersion).AppendLine();

            bool rw = (decoded.DiscPhysicalCode & 0x02) > 0;

            if(rw)
            {
                if((decoded.OPCSuggestedCode & 0xF) > 0)
                {
                    double recordingPower = 0;

                    switch(decoded.OPCSuggestedCode & 0xF)
                    {
                        case 1:
                            recordingPower = 7.0;

                            break;
                        case 2:
                            recordingPower = 7.5;

                            break;
                        case 3:
                            recordingPower = 8.0;

                            break;
                        case 4:
                            recordingPower = 8.5;

                            break;
                        case 5:
                            recordingPower = 9.0;

                            break;
                        case 6:
                            recordingPower = 9.5;

                            break;
                        case 7:
                            recordingPower = 10.0;

                            break;
                        case 8:
                            recordingPower = 10.5;

                            break;
                        case 9:
                            recordingPower = 11.0;

                            break;
                        case 10:
                            recordingPower = 11.5;

                            break;
                        case 11:
                            recordingPower = 12.0;

                            break;
                        case 12:
                            recordingPower = 12.5;

                            break;
                        case 13:
                            recordingPower = 13.0;

                            break;
                        case 14:
                            recordingPower = 13.5;

                            break;
                        case 15:
                            recordingPower = 14.0;

                            break;
                    }

                    sb.AppendFormat("Recommended recording power is {0} mW", recordingPower).AppendLine();
                }
                else
                {
                    sb.AppendLine("Recording power is not specified");
                }

                if((decoded.WaveLengthCode & 0xF) > 0)
                {
                    double erasingPower = 0;

                    switch(decoded.WaveLengthCode & 0xF)
                    {
                        case 1:
                            erasingPower = 0.38;

                            break;
                        case 2:
                            erasingPower = 0.40;

                            break;
                        case 3:
                            erasingPower = 0.42;

                            break;
                        case 4:
                            erasingPower = 0.44;

                            break;
                        case 5:
                            erasingPower = 0.46;

                            break;
                        case 6:
                            erasingPower = 0.48;

                            break;
                        case 7:
                            erasingPower = 0.50;

                            break;
                        case 8:
                            erasingPower = 0.52;

                            break;
                        case 9:
                            erasingPower = 0.54;

                            break;
                        case 10:
                            erasingPower = 0.56;

                            break;
                        case 11:
                            erasingPower = 0.58;

                            break;
                        case 12:
                            erasingPower = 0.60;

                            break;
                        case 13:
                            erasingPower = 0.62;

                            break;
                        case 14:
                            erasingPower = 0.64;

                            break;
                        case 15:
                            erasingPower = 0.66;

                            break;
                    }

                    sb.AppendFormat("Recommended erasing power ratio is {0} ε", erasingPower).AppendLine();
                }
                else
                {
                    sb.AppendLine("Erasing power ratio is not specified");
                }
            }
            else
            {
                if((decoded.OPCSuggestedCode & 0xF) > 0)
                {
                    double recordingPower = 0;

                    switch(decoded.OPCSuggestedCode & 0xF)
                    {
                        case 1:
                            recordingPower = 6.0;

                            break;
                        case 2:
                            recordingPower = 6.5;

                            break;
                        case 3:
                            recordingPower = 7.0;

                            break;
                        case 4:
                            recordingPower = 7.5;

                            break;
                        case 5:
                            recordingPower = 8.0;

                            break;
                        case 6:
                            recordingPower = 8.5;

                            break;
                        case 7:
                            recordingPower = 9.0;

                            break;
                        case 8:
                            recordingPower = 9.5;

                            break;
                        case 9:
                            recordingPower = 10.0;

                            break;
                        case 10:
                            recordingPower = 10.5;

                            break;
                        case 11:
                            recordingPower = 11.0;

                            break;
                        case 12:
                            recordingPower = 11.5;

                            break;
                        case 13:
                            recordingPower = 12.0;

                            break;
                    }

                    sb.AppendFormat("Recommended recording power is {0} mW", recordingPower).AppendLine();
                }

                if(decoded.WaveLengthCode > 0)
                {
                    int wavelength = 0;

                    switch(decoded.WaveLengthCode)
                    {
                        case 1:
                            wavelength = 645;

                            break;
                        case 2:
                            wavelength = 646;

                            break;
                        case 3:
                            wavelength = 647;

                            break;
                        case 4:
                            wavelength = 648;

                            break;
                        case 5:
                            wavelength = 649;

                            break;
                        case 6:
                            wavelength = 650;

                            break;
                        case 7:
                            wavelength = 651;

                            break;
                        case 8:
                            wavelength = 652;

                            break;
                        case 9:
                            wavelength = 653;

                            break;
                        case 10:
                            wavelength = 654;

                            break;
                        case 11:
                            wavelength = 655;

                            break;
                        case 12:
                            wavelength = 656;

                            break;
                        case 13:
                            wavelength = 657;

                            break;
                        case 14:
                            wavelength = 658;

                            break;
                        case 15:
                            wavelength = 659;

                            break;
                        case 16:
                            wavelength = 660;

                            break;
                    }

                    sb.AppendFormat("Recommended recording power is {0} mW", wavelength).AppendLine();
                }
            }

            sb.AppendFormat("Disc manufacturer is {0}", ManufacturerFromPrePit(decoded.ManufacturerId)).AppendLine();

            return sb.ToString();
        }

        public static string Prettify(byte[] response) => Prettify(Decode(response));

        public static string ManufacturerFromPrePit(string manufacturerId)
        {
            string manufacturer = "";

            // Bad thing is that it also includes a media code...
            if(manufacturerId.StartsWith("RITEK", StringComparison.Ordinal))
                manufacturer = "Ritek Co.";
            else if(manufacturerId.StartsWith("CMC", StringComparison.Ordinal))
                manufacturer = "CMC Magnetics Corporation";
            else if(manufacturerId.StartsWith("Dvsn-", StringComparison.Ordinal))
                manufacturer = "Digital Storage Technology Co., Ltd.";
            else if(manufacturerId.StartsWith("GSC", StringComparison.Ordinal))
                manufacturer = "Gigastore Corporation";
            else if(manufacturerId.StartsWith("INFOMEDIA", StringComparison.Ordinal))
                manufacturer = "InfoMedia Inc.";
            else if(manufacturerId.StartsWith("ISSM", StringComparison.Ordinal))
                manufacturer = "Info Source Digital Media (Zhongshan) Co., Ltd.";
            else if(manufacturerId.StartsWith("LEADDATA", StringComparison.Ordinal))
                manufacturer = "Lead Data Inc.";
            else if(manufacturerId.StartsWith("MCC", StringComparison.Ordinal) ||
                    manufacturerId.StartsWith("MKM", StringComparison.Ordinal))
                manufacturer = "Mitsubishi Kagaku Media Co., LTD.";
            else if(manufacturerId.StartsWith("MUST", StringComparison.Ordinal))
                manufacturer = "Must Technology Co., Ltd.";
            else if(manufacturerId.StartsWith("MXL", StringComparison.Ordinal))
                manufacturer = "Hitachi Maxell, Ltd.";
            else if(manufacturerId.StartsWith("PRINCO", StringComparison.Ordinal))
                manufacturer = "Princo Corporation";
            else if(manufacturerId.StartsWith("Prodisc", StringComparison.Ordinal))
                manufacturer = "Prodisc Technology Inc.";
            else if(manufacturerId.StartsWith("SONY", StringComparison.Ordinal) ||
                    manufacturerId.StartsWith("80SONY", StringComparison.Ordinal))
                manufacturer = "Sony Corporation";
            else if(manufacturerId.StartsWith("TCLDS", StringComparison.Ordinal))
                manufacturer = "TCL Technology";
            else if(manufacturerId.StartsWith("TMI", StringComparison.Ordinal))
                manufacturer = "ThaiMedia Co., Ltd. ";
            else if(manufacturerId.StartsWith("TY", StringComparison.Ordinal))
                manufacturer = "Taiyo Yuden Company Ltd.";
            else if(manufacturerId.StartsWith("UME", StringComparison.Ordinal))
                manufacturer = "Avic Umedisc HK Ltd.";
            else if(manufacturerId.StartsWith("DAXON", StringComparison.Ordinal))
                manufacturer = "Daxon Technology Inc.";
            else if(manufacturerId.StartsWith("FTI", StringComparison.Ordinal))
                manufacturer = "Falcon Technologies International L.L.C.";
            else if(manufacturerId.StartsWith("FUJIFILM", StringComparison.Ordinal))
                manufacturer = "Fuji Photo Film, Co., Ltd.";
            else if(manufacturerId.StartsWith("MBI", StringComparison.Ordinal))
                manufacturer = "Moser Baer India Ltd.";
            else if(manufacturerId.StartsWith("TT", StringComparison.Ordinal) ||
                    manufacturerId.StartsWith("TDK", StringComparison.Ordinal))
                manufacturer = "TDK Corporation";
            else if(manufacturerId.StartsWith("JVC", StringComparison.Ordinal))
                manufacturer = "Victor Advanced media Co., Ltd.";
            else if(manufacturerId.StartsWith("MEI", StringComparison.Ordinal))
                manufacturer = "Matsushita Electric Industrial Co., Ltd.";
            else if(manufacturerId.StartsWith("OPTODISC", StringComparison.Ordinal))
                manufacturer = "OptoDisc Ltd.";
            else if(manufacturerId.StartsWith("KIC", StringComparison.Ordinal))
                manufacturer = "Advance Media Corporation";
            else if(manufacturerId.StartsWith("IMC", StringComparison.Ordinal))
                manufacturer = "Intermedia Co., Ltd.";
            else if(manufacturerId.StartsWith("LGE", StringComparison.Ordinal))
                manufacturer = "LG Electronics Inc.";
            else if(manufacturerId.StartsWith("KDT", StringComparison.Ordinal))
                manufacturer = "King Disc Technology Corporation";
            else if(manufacturerId.StartsWith("POS", StringComparison.Ordinal))
                manufacturer = "POSTECH Corporation";
            else if(manufacturerId.StartsWith("VDSPMSAB", StringComparison.Ordinal))
                manufacturer = "Interaxia Digital Storage Materials AG";
            else if(manufacturerId.StartsWith("VANGUARD", StringComparison.Ordinal))
                manufacturer = "Vanguard Disc Inc.";
            else if(manufacturerId.StartsWith("MJC", StringComparison.Ordinal))
                manufacturer = "Megan Media Holdings Berhad";
            else if(manufacturerId.StartsWith("DKM", StringComparison.Ordinal) ||
                    manufacturerId.StartsWith("EDMA", StringComparison.Ordinal))
                manufacturer = "E-TOP Mediatek Inc.";
            else if(manufacturerId.StartsWith("BeAll", StringComparison.Ordinal))
                manufacturer = "BeALL Developers, Inc.";

            return manufacturer != "" ? $"{manufacturer} (\"{manufacturerId}\")" : $"\"{manufacturerId}\"";
        }

        public struct PreRecordedInformation
        {
            /// <summary>Bytes 0 to 1 Data length</summary>
            public ushort DataLength;
            /// <summary>Byte 2 Reserved</summary>
            public byte Reserved1;
            /// <summary>Byte 3 Reserved</summary>
            public byte Reserved2;
            /// <summary>Byte 4 == 1</summary>
            public byte FieldId1;
            /// <summary>Byte 5 disc application code </summary>
            public byte DiscApplicationCode;
            /// <summary>Byte 6 disc physical code </summary>
            public byte DiscPhysicalCode;
            /// <summary>Bytes 7 to 9 last address of data recordable area </summary>
            public uint LastAddressOfDataRecordableArea;
            /// <summary>Byte 10, bits 7 to 4 part version</summary>
            public byte PartVersion;
            /// <summary>Byte 10, bits 3 to 0 extension code</summary>
            public byte ExtensionCode;
            /// <summary>Byte 11 reserved</summary>
            public byte Reserved3;
            /// <summary>Byte 12 == 2</summary>
            public byte FieldId2;
            /// <summary>Byte 13 OPC suggested code </summary>
            public byte OPCSuggestedCode;
            /// <summary>Byte 14 wavelength code or second part of OPC suggested code</summary>
            public byte WaveLengthCode;
            /// <summary>Bytes 15 to 18 write strategy code</summary>
            public uint WriteStrategyCode;
            /// <summary>Byte 19 reserved</summary>
            public byte Reserved4;
            /// <summary>Byte 20 == 3</summary>
            public byte FieldId3;
            /// <summary>Bytes 21 to 26 first part of manufacturer ID</summary>
            public byte[] ManufacturerId1;
            /// <summary>Byte 27</summary>
            public byte Reserved5;
            /// <summary>Byte 28 == 4</summary>
            public byte FieldId4;
            /// <summary>Bytes 29 to 34 second part of manufacturer ID</summary>
            public byte[] ManufacturerId2;
            /// <summary>Byte 35 reserved</summary>
            public byte Reserved6;
            /// <summary>Byte 36 == 5</summary>
            public byte FieldId5;
            /// <summary>Bytes 37 to 42, third part of manufacturer code or write strategy code for RW and R later versions</summary>
            public byte[] ManufacturerId3;
            /// <summary>Byte 43 reserved</summary>
            public byte Reserved7;
            /// <summary>Bytes 44 to 68 reserved</summary>
            public byte[] Reserved8;

            public string ManufacturerId;
            public uint   WriteStrategyCode2;
        }
    }
}