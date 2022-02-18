// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : ATIP.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes CD Absolute-Time-In-Pregroove
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

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Console;
using Aaru.Helpers;

namespace Aaru.Decoders.CD
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
    [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "NotAccessedField.Global")]
    public static class ATIP
    {
        public static CDATIP Decode(byte[] CDATIPResponse)
        {
            if(CDATIPResponse        == null ||
               CDATIPResponse.Length <= 4)
                return null;

            var decoded = new CDATIP();

            if(CDATIPResponse.Length != 32 &&
               CDATIPResponse.Length != 28)
            {
                AaruConsole.DebugWriteLine("CD ATIP decoder",
                                           "Expected CD ATIP size (32 bytes) is not received size ({0} bytes), not decoding",
                                           CDATIPResponse.Length);

                return null;
            }

            decoded.DataLength     = BigEndianBitConverter.ToUInt16(CDATIPResponse, 0);
            decoded.Reserved1      = CDATIPResponse[2];
            decoded.Reserved2      = CDATIPResponse[3];
            decoded.ITWP           = (byte)((CDATIPResponse[4] & 0xF0) >> 4);
            decoded.DDCD           = Convert.ToBoolean(CDATIPResponse[4] & 0x08);
            decoded.ReferenceSpeed = (byte)(CDATIPResponse[4] & 0x07);
            decoded.AlwaysZero     = Convert.ToBoolean(CDATIPResponse[5] & 0x80);
            decoded.URU            = Convert.ToBoolean(CDATIPResponse[5] & 0x40);
            decoded.Reserved3      = (byte)(CDATIPResponse[5] & 0x3F);

            decoded.AlwaysOne   = Convert.ToBoolean(CDATIPResponse[6] & 0x80);
            decoded.DiscType    = Convert.ToBoolean(CDATIPResponse[6] & 0x40);
            decoded.DiscSubType = (byte)((CDATIPResponse[6] & 0x38) >> 3);
            decoded.A1Valid     = Convert.ToBoolean(CDATIPResponse[6] & 0x04);
            decoded.A2Valid     = Convert.ToBoolean(CDATIPResponse[6] & 0x02);
            decoded.A3Valid     = Convert.ToBoolean(CDATIPResponse[6] & 0x01);

            decoded.Reserved4         = CDATIPResponse[7];
            decoded.LeadInStartMin    = CDATIPResponse[8];
            decoded.LeadInStartSec    = CDATIPResponse[9];
            decoded.LeadInStartFrame  = CDATIPResponse[10];
            decoded.Reserved5         = CDATIPResponse[11];
            decoded.LeadOutStartMin   = CDATIPResponse[12];
            decoded.LeadOutStartSec   = CDATIPResponse[13];
            decoded.LeadOutStartFrame = CDATIPResponse[14];
            decoded.Reserved6         = CDATIPResponse[15];

            decoded.A1Values = new byte[3];
            decoded.A2Values = new byte[3];
            decoded.A3Values = new byte[3];

            Array.Copy(CDATIPResponse, 16, decoded.A1Values, 0, 3);
            Array.Copy(CDATIPResponse, 20, decoded.A2Values, 0, 3);
            Array.Copy(CDATIPResponse, 24, decoded.A3Values, 0, 3);

            decoded.Reserved7 = CDATIPResponse[19];
            decoded.Reserved8 = CDATIPResponse[23];
            decoded.Reserved9 = CDATIPResponse[27];

            if(CDATIPResponse.Length < 32)
                return decoded.AlwaysOne ? decoded : null;

            decoded.S4Values = new byte[3];
            Array.Copy(CDATIPResponse, 28, decoded.S4Values, 0, 3);
            decoded.Reserved10 = CDATIPResponse[31];

            return decoded.AlwaysOne ? decoded : null;
        }

        public static string Prettify(CDATIP response)
        {
            if(response == null)
                return null;

            var sb = new StringBuilder();

            if(response.DDCD)
            {
                sb.AppendFormat("Indicative Target Writing Power: 0x{0:X2}", response.ITWP).AppendLine();
                sb.AppendLine(response.DiscType ? "Disc is DDCD-RW" : "Disc is DDCD-R");

                switch(response.ReferenceSpeed)
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

                sb.AppendFormat("ATIP Start time of Lead-in: 0x{0:X6}",
                                (response.LeadInStartMin << 16) + (response.LeadInStartSec << 8) +
                                response.LeadInStartFrame).AppendLine();

                sb.AppendFormat("ATIP Last possible start time of Lead-out: 0x{0:X6}",
                                (response.LeadOutStartMin << 16) + (response.LeadOutStartSec << 8) +
                                response.LeadOutStartFrame).AppendLine();

                sb.AppendFormat("S4 value: 0x{0:X6}",
                                (response.S4Values[0] << 16) + (response.S4Values[1] << 8) + response.S4Values[2]).
                   AppendLine();
            }
            else
            {
                sb.AppendFormat("Indicative Target Writing Power: 0x{0:X2}", response.ITWP & 0x07).AppendLine();

                if(response.DiscType)
                {
                    switch(response.DiscSubType)
                    {
                        case 0:
                            sb.AppendLine("Disc is CD-RW");

                            break;
                        case 1:
                            sb.AppendLine("Disc is High-Speed CD-RW");

                            break;
                        case 2:
                            sb.AppendLine("Disc is Ultra-Speed CD-RW");

                            break;
                        case 3:
                            sb.AppendLine("Disc is Ultra-Speed+ CD-RW");

                            break;
                        case 4:
                            sb.AppendLine("Disc is medium type B, low beta category (B-) CD-RW");

                            break;
                        case 5:
                            sb.AppendLine("Disc is medium type B, high beta category (B+) CD-RW");

                            break;
                        case 6:
                            sb.AppendLine("Disc is medium type C, low beta category (C-) CD-RW");

                            break;
                        case 7:
                            sb.AppendLine("Disc is medium type C, high beta category (C+) CD-RW");

                            break;
                        default:
                            sb.AppendFormat("Unknown CD-RW disc subtype: {0}", response.DiscSubType).AppendLine();

                            break;
                    }

                    switch(response.ReferenceSpeed)
                    {
                        case 1:
                            sb.AppendLine("Reference speed is 2x");

                            break;
                        default:
                            sb.AppendFormat("Reference speed set is unknown: {0}", response.ReferenceSpeed).
                               AppendLine();

                            break;
                    }
                }
                else
                {
                    sb.AppendLine("Disc is CD-R");

                    switch(response.DiscSubType)
                    {
                        case 0:
                            sb.AppendLine("Disc is normal speed (CLV) CD-R");

                            break;
                        case 1:
                            sb.AppendLine("Disc is high speed (CAV) CD-R");

                            break;
                        case 2:
                            sb.AppendLine("Disc is medium type A, low beta category (A-) CD-R");

                            break;
                        case 3:
                            sb.AppendLine("Disc is medium type A, high beta category (A+) CD-R");

                            break;
                        case 4:
                            sb.AppendLine("Disc is medium type B, low beta category (B-) CD-R");

                            break;
                        case 5:
                            sb.AppendLine("Disc is medium type B, high beta category (B+) CD-R");

                            break;
                        case 6:
                            sb.AppendLine("Disc is medium type C, low beta category (C-) CD-R");

                            break;
                        case 7:
                            sb.AppendLine("Disc is medium type C, high beta category (C+) CD-R");

                            break;
                        default:
                            sb.AppendFormat("Unknown CD-R disc subtype: {0}", response.DiscSubType).AppendLine();

                            break;
                    }
                }

                sb.AppendLine(response.URU ? "Disc use is unrestricted" : "Disc use is restricted");

                sb.AppendFormat("ATIP Start time of Lead-in: {0}:{1:D2}:{2:D2}", response.LeadInStartMin,
                                response.LeadInStartSec, response.LeadInStartFrame).AppendLine();

                sb.AppendFormat("ATIP Last possible start time of Lead-out: {0}:{1:D2}:{2:D2}",
                                response.LeadOutStartMin, response.LeadOutStartSec, response.LeadOutStartFrame).
                   AppendLine();

                if(response.A1Valid)
                    sb.AppendFormat("A1 value: 0x{0:X6}",
                                    (response.A1Values[0] << 16) + (response.A1Values[1] << 8) + response.A1Values[2]).
                       AppendLine();

                if(response.A2Valid)
                    sb.AppendFormat("A2 value: 0x{0:X6}",
                                    (response.A2Values[0] << 16) + (response.A2Values[1] << 8) + response.A2Values[2]).
                       AppendLine();

                if(response.A3Valid)
                    sb.AppendFormat("A3 value: 0x{0:X6}",
                                    (response.A3Values[0] << 16) + (response.A3Values[1] << 8) + response.A3Values[2]).
                       AppendLine();

                if(response.S4Values != null)
                    sb.AppendFormat("S4 value: 0x{0:X6}",
                                    (response.S4Values[0] << 16) + (response.S4Values[1] << 8) + response.S4Values[2]).
                       AppendLine();
            }

            if(response.LeadInStartMin != 97)
                return sb.ToString();

            int type = response.LeadInStartFrame % 10;
            int frm  = response.LeadInStartFrame - type;

            if(response.DiscType)
                sb.AppendLine("Disc uses phase change");
            else
                sb.AppendLine(type < 5 ? "Disc uses long strategy type dye (Cyanine, AZO, etc...)"
                                  : "Disc uses short strategy type dye (Phthalocyanine, etc...)");

            string manufacturer = ManufacturerFromATIP(response.LeadInStartSec, frm);

            if(manufacturer != "")
                sb.AppendFormat("Disc manufactured by: {0}", manufacturer).AppendLine();

            return sb.ToString();
        }

        public static string Prettify(byte[] CDATIPResponse)
        {
            CDATIP decoded = Decode(CDATIPResponse);

            return Prettify(decoded);
        }

        public static string ManufacturerFromATIP(byte sec, int frm)
        {
            switch(sec)
            {
                case 10:
                    switch(frm)
                    {
                        case 00: return "Ritek Co.";
                    }

                    break;
                case 15:
                    switch(frm)
                    {
                        case 00: return "TDK Corporation";
                        case 10: return "Ritek Co.";
                        case 20: return "Mitsubishi Chemical Corporation";
                        case 30: return "NAN-YA Plastics Corporation";
                    }

                    break;
                case 16:
                    switch(frm)
                    {
                        case 20: return "Shenzen SG&Gast Digital Optical Discs";
                        case 30: return "Grand Advance Technology Ltd.";
                    }

                    break;
                case 17:
                    if(frm == 00)
                        return "Moser Baer India Ltd.";

                    break;
                case 18:
                    switch(frm)
                    {
                        case 10: return "Wealth Fair Investment Ltd.";
                        case 60: return "Taroko International Co. Ltd.";
                    }

                    break;
                case 20:
                    if(frm == 10)
                        return "CDA Datenträger Albrechts GmbH";

                    break;
                case 21:
                    switch(frm)
                    {
                        case 10: return "Grupo Condor S.L.";
                        case 20: return "E-TOP Mediatek Inc.";
                        case 30: return "Bestdisc Technology Corporation";
                        case 40: return "Optical Disc Manufacturing Equipment";
                        case 50: return "Sound Sound Multi-Media Development Ltd.";
                    }

                    break;
                case 22:
                    switch(frm)
                    {
                        case 00: return "Woongjin Media Corp.";
                        case 10: return "Seantram Technology Inc.";
                        case 20: return "Advanced Digital Media";
                        case 30: return "EXIMPO";
                        case 40: return "CIS Technology Inc.";
                        case 50: return "Hong Kong Digital Technology Co., Ltd.";
                        case 60: return "Acer Media Technology, Inc.";
                    }

                    break;
                case 23:
                    switch(frm)
                    {
                        case 00: return "Matsushita Electric Industrial Co., Ltd.";
                        case 10: return "Doremi Media Co., Ltd.";
                        case 20: return "Nacar Media s.r.l.";
                        case 30: return "Audio Distributors Co., Ltd.";
                        case 40: return "Victor Company of Japan, Ltd.";
                        case 50: return "Optrom Inc.";
                        case 60: return "Customer Pressing Oosterhout";
                    }

                    break;
                case 24:
                    switch(frm)
                    {
                        case 00: return "Taiyo Yuden Company Ltd.";
                        case 10: return "SONY Corporation";
                        case 20: return "Computer Support Italy s.r.l.";
                        case 30: return "Unitech Japan Inc.";
                        case 40: return "kdg mediatech AG";
                        case 50: return "Guann Yinn Co., Ltd.";
                        case 60: return "Harmonic Hall Optical Disc Ltd.";
                    }

                    break;
                case 25:
                    switch(frm)
                    {
                        case 00: return "MPO";
                        case 20: return "Hitachi Maxell, Ltd.";
                        case 30: return "Infodisc Technology Co. Ltd.";
                        case 40: return "Vivastar AG";
                        case 50: return "AMS Technology Inc.";
                        case 60: return "Xcitec Inc.";
                    }

                    break;
                case 26:
                    switch(frm)
                    {
                        case 00: return "Fornet International Pte Ltd.";
                        case 10: return "POSTECH Corporation";
                        case 20: return "SKC Co., Ltd.";
                        case 30: return "Optical Disc Corporation";
                        case 40: return "FUJI Photo Film Co., Ltd.";
                        case 50: return "Lead Data Inc.";
                        case 60: return "CMC Magnetics Corporation";
                    }

                    break;
                case 27:
                    switch(frm)
                    {
                        case 00: return "Digital Storage Technology Co., Ltd.";
                        case 10: return "Plasmon Data systems Ltd.";
                        case 20: return "Princo Corporation";
                        case 30: return "Pioneer Video Corporation";
                        case 40: return "Kodak Japan Ltd.";
                        case 50: return "Mitsui Chemicals, Inc.";
                        case 60: return "Ricoh Company Ltd.";
                    }

                    break;
                case 28:
                    switch(frm)
                    {
                        case 00: return "Opti.Me.S. S.p.A.";
                        case 10: return "Gigastore Corporation";
                        case 20: return "Multi Media Masters & Machinary SA";
                        case 30: return "Auvistar Industry Co., Ltd.";
                        case 40: return "King Pro Mediatek Inc.";
                        case 50: return "Delphi Technology Inc.";
                        case 60: return "Friendly CD-Tek Co.";
                    }

                    break;
                case 29:
                    switch(frm)
                    {
                        case 00: return "Taeil Media Co., Ltd.";
                        case 10: return "Vanguard Disc Inc.";
                        case 20: return "Unidisc Technology Co., Ltd.";
                        case 30: return "Hile Optical Disc Technology Corp.";
                        case 40: return "Viva Magnetics Ltd.";
                        case 50: return "General Magnetics Ltd.";
                    }

                    break;
                case 30:
                    if(frm == 10)
                        return "CDA Datenträger Albrechts GmbH";

                    break;
                case 31:
                    switch(frm)
                    {
                        case 00: return "Ritek Co.";
                        case 30: return "Grand Advance Technology Ltd.";
                    }

                    break;
                case 32:
                    switch(frm)
                    {
                        case 00: return "TDK Corporation";
                        case 10: return "Prodisc Technology Inc.";
                    }

                    break;
                case 34:
                    switch(frm)
                    {
                        case 20:
                        case 22: return "Mitsubishi Chemical Corporation";
                    }

                    break;
                case 36:
                    switch(frm)
                    {
                        case 00: return "Gish International Co., Ltd.";
                    }

                    break;
                case 42:
                    if(frm == 20)
                        return "Advanced Digital Media";

                    break;
                case 45:
                    switch(frm)
                    {
                        case 00: return "Fornet International Pte Ltd.";
                        case 10: return "Unitech Japan Inc.";
                        case 20: return "Acer Media Technology, Inc.";
                        case 40: return "CIS Technology Inc.";
                        case 50: return "Guann Yinn Co., Ltd.";
                        case 60: return "Xcitec Inc.";
                    }

                    break;
                case 46:
                    switch(frm)
                    {
                        case 00: return "Taiyo Yuden Company Ltd.";
                        case 10: return "Hong Kong Digital Technology Co., Ltd.";
                        case 20: return "Multi Media Masters & Machinary SA";
                        case 30: return "Computer Support Italy s.r.l.";
                        case 40: return "FUJI Photo Film Co., Ltd.";
                        case 50: return "Auvistar Industry Co., Ltd.";
                        case 60: return "CMC Magnetics Corporation";
                    }

                    break;
                case 47:
                    switch(frm)
                    {
                        case 10: return "Hitachi Maxell, Ltd.";
                        case 20: return "Princo Corporation";
                        case 40: return "POSTECH Corporation";
                        case 50: return "Ritek Co.";
                        case 60: return "Prodisc Technology Inc.";
                    }

                    break;
                case 48:
                    switch(frm)
                    {
                        case 00: return "Ricoh Company Ltd.";
                        case 10: return "Kodak Japan Ltd.";
                        case 20: return "Plasmon Data systems Ltd.";
                        case 30: return "Pioneer Video Corporation";
                        case 40: return "Digital Storage Technology Co., Ltd.";
                        case 50: return "Mitsui Chemicals, Inc.";
                        case 60: return "Lead Data Inc.";
                    }

                    break;
                case 49:
                    switch(frm)
                    {
                        case 00: return "TDK Corporation";
                        case 10: return "Gigastore Corporation";
                        case 20: return "King Pro Mediatek Inc.";
                        case 30: return "Opti.Me.S. S.p.A.";
                        case 40: return "Victor Company of Japan, Ltd.";
                        case 60: return "Matsushita Electric Industrial Co., Ltd.";
                    }

                    break;
                case 50:
                    switch(frm)
                    {
                        case 10: return "Vanguard Disc Inc.";
                        case 20: return "Mitsubishi Chemical Corporation";
                        case 30: return "CDA Datenträger Albrechts GmbH";
                    }

                    break;
                case 51:
                    switch(frm)
                    {
                        case 10: return "Grand Advance Technology Ltd.";
                        case 20: return "Infodisc Technology Co. Ltd.";
                        case 50: return "Hile Optical Disc Technology Corp.";
                    }

                    break;
            }

            return "";
        }

        public class CDATIP
        {
            /// <summary>Byte 6, bit 2 A1 values are valid</summary>
            public bool A1Valid;
            /// <summary>Bytes 16 to 18 A1 values</summary>
            public byte[] A1Values;
            /// <summary>Byte 6, bit 1 A2 values are valid</summary>
            public bool A2Valid;
            /// <summary>Bytes 20 to 22 A2 values</summary>
            public byte[] A2Values;
            /// <summary>Byte 6, bit 0 A3 values are valid</summary>
            public bool A3Valid;
            /// <summary>Bytes 24 to 26 A3 values</summary>
            public byte[] A3Values;
            /// <summary>Byte 6, bit 7 Always set</summary>
            public bool AlwaysOne;
            /// <summary>Byte 5, bit 7 Always unset</summary>
            public bool AlwaysZero;
            /// <summary>Bytes 1 to 0 Total size of returned session information minus this field</summary>
            public ushort DataLength;
            /// <summary>Byte 4, bit 3 Set if DDCD</summary>
            public bool DDCD;
            /// <summary>Byte 6, bits 5 to 3 Disc subtype</summary>
            public byte DiscSubType;
            /// <summary>Byte 6, bit 6 Set if rewritable (CD-RW or DDCD-RW)</summary>
            public bool DiscType;
            /// <summary>Byte 4, bits 7 to 4 Indicative target writing power</summary>
            public byte ITWP;
            /// <summary>Byte 10 ATIP Start time of Lead-In (Frame)</summary>
            public byte LeadInStartFrame;
            /// <summary>Byte 8 ATIP Start time of Lead-In (Minute)</summary>
            public byte LeadInStartMin;
            /// <summary>Byte 9 ATIP Start time of Lead-In (Second)</summary>
            public byte LeadInStartSec;
            /// <summary>Byte 14 ATIP Last possible start time of Lead-Out (Frame)</summary>
            public byte LeadOutStartFrame;
            /// <summary>Byte 12 ATIP Last possible start time of Lead-Out (Minute)</summary>
            public byte LeadOutStartMin;
            /// <summary>Byte 13 ATIP Last possible start time of Lead-Out (Second)</summary>
            public byte LeadOutStartSec;
            /// <summary>Byte 4, bits 2 to 0 Reference speed</summary>
            public byte ReferenceSpeed;
            /// <summary>Byte 2 Reserved</summary>
            public byte Reserved1;
            /// <summary>Byte 31 Reserved</summary>
            public byte Reserved10;
            /// <summary>Byte 3 Reserved</summary>
            public byte Reserved2;
            /// <summary>Byte 5, bits 5 to 0 Reserved</summary>
            public byte Reserved3;
            /// <summary>Byte 7 Reserved</summary>
            public byte Reserved4;
            /// <summary>Byte 11 Reserved</summary>
            public byte Reserved5;
            /// <summary>Byte 15 Reserved</summary>
            public byte Reserved6;
            /// <summary>Byte 19 Reserved</summary>
            public byte Reserved7;
            /// <summary>Byte 23 Reserved</summary>
            public byte Reserved8;
            /// <summary>Byte 27 Reserved</summary>
            public byte Reserved9;
            /// <summary>Bytes 28 to 30 S4 values</summary>
            public byte[] S4Values;
            /// <summary>Byte 5, bit 6 Unrestricted media</summary>
            public bool URU;
        }
    }
}