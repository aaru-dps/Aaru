// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : VendorCode.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes PCMCIA vendor code.
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

namespace Aaru.Decoders.PCMCIA;

[SuppressMessage("ReSharper", "MemberCanBeInternal")]
public static class VendorCode
{
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public static string Prettify(ushort id)
    {
        switch(id)
        {
#region JEDEC

            case 0x01:
                return "AMD";
            case 0x02:
                return "AMI";
            case 0x83:
                return "Fairchild";
            case 0x04:
                return "Fujitsu";
            case 0x85:
                return "GTE";
            case 0x86:
                return "Harris";
            case 0x07:
                return "Hitachi";
            case 0x08:
                return "Inmos";
            case 0x89:
                return "Intel";
            case 0x8A:
                return "I.T.T.";
            case 0x0B:
                return "Intersil";
            case 0x8C:
                return "Monolithic Memories";
            case 0x0D:
                return "Mostek";
            case 0x0E:
                return "Freescale";
            case 0x8F:
                return "National";
            case 0x10:
                return "NEC";
            case 0x91:
                return "RCA";
            case 0x92:
                return "Raytheon";
            case 0x13:
                return "Conexant";
            case 0x94:
                return "Seeq";
            case 0x15:
                return "NXP";
            case 0x16:
                return "Synertek";
            case 0x97:
                return "Texas Instruments";
            case 0x98:
                return "Toshiba";
            case 0x19:
                return "Xicor";
            case 0x1A:
                return "Zilog";
            case 0x9B:
                return "Eurotechnique";
            case 0x1C:
                return "Mitsubishi2";
            case 0x9D:
                return "Lucent";
            case 0x9E:
                return "Exel";
            case 0x1F:
                return "Atmel";
            case 0x20:
                return "SGS/Thomson";
            case 0xA1:
                return "Lattice Semiconductor";
            case 0xA2:
                return "NCR";
            case 0x23:
                return "Wafer Scale Integration";
            case 0xA4:
                return "International Business Machines";
            case 0x25:
                return "Tristar";
            case 0x26:
                return "Visic";
            case 0xA7:
                return "International CMOS Technology";
            case 0xA8:
                return "SSSI";
            case 0x29:
                return "Microchip Technology";
            case 0x2A:
                return "Ricoh";
            case 0xAB:
                return "VLSI";
            case 0x2C:
                return "Micron Technology";
            case 0xAD:
                return "Hynix Semiconductor";
            case 0xAE:
                return "OKI Semiconductor";
            case 0x2F:
                return "ACTEL";
            case 0xB0:
                return "Sharp";
            case 0x31:
                return "Catalyst";
            case 0x32:
                return "Panasonic";
            case 0xB3:
                return "IDT";
            case 0x34:
                return "Cypress";
            case 0xB5:
                return "Digital Equipment Corporation";
            case 0xB6:
                return "LSI Logic";
            case 0x37:
                return "Zarlink";
            case 0x38:
                return "UTMC";
            case 0xB9:
                return "Thinking Machine";
            case 0xBA:
                return "Thomson CSF";
            case 0x3B:
                return "Integrated CMOS";
            case 0xBC:
                return "Honeywell";
            case 0x3D:
                return "Tektronix";
            case 0x3E:
                return "Oracle Corporation";
            case 0xBF:
                return "Silicon Storage Technology";
            case 0x40:
                return "ProMos";
            case 0xC1:
                return "Infineon";
            case 0xC2:
                return "Macronix";
            case 0x43:
                return "Xerox";
            case 0xC4:
                return "Plus Logic";
            case 0x45:
                return "SanDisk Corporation";
            case 0x46:
                return "Elan Circuit Technology";
            case 0xC7:
                return "European Silicon";
            case 0xC8:
                return "Apple";
            case 0x49:
                return "Xilinx";
            case 0x4A:
                return "Compaq";
            case 0xCB:
                return "Protocol Engines";
            case 0x4C:
                return "SCI";
            case 0xCD:
                return "Seiko Instruments";
            case 0xCE:
                return "Samsung";
            case 0x4F:
                return "I3 Design System";
            case 0xD0:
                return "Klic";
            case 0x51:
                return "Crosspoint Solutions";
            case 0x52:
                return "Alliance Semiconductor";
            case 0xD3:
                return "Tandem";
            case 0x54:
                return "Hewlett-Packard";
            case 0xD5:
                return "Integrated Silicon Solutions";
            case 0xD6:
                return "Brooktree";
            case 0x57:
                return "New Media";
            case 0x58:
                return "MHS Electronic";
            case 0xD9:
                return "Performance Semiconductors";
            case 0xDA:
                return "Winbond Electronic";
            case 0x5B:
                return "Kawasaki Steel";
            case 0x5D:
                return "TECMAR";
            case 0x5E:
                return "Exar";
            case 0xDF:
                return "PCMCIA";
            case 0xE0:
                return "LG Semiconductor";
            case 0x61:
                return "Northern Telecom";
            case 0x62:
                return "Sanyo2";
            case 0xE3:
                return "Array Microsystems";
            case 0x64:
                return "Crystal Semiconductor";
            case 0xE5:
                return "Analog Devices";
            case 0xE6:
                return "PMC-Sierra";
            case 0x67:
                return "Asparix";
            case 0x68:
                return "Convex Computer";
            case 0xE9:
                return "Nimbus Technology";
            case 0x6B:
                return "Transwitch";
            case 0xEC:
                return "Micronas";
            case 0x6D:
                return "Canon";
            case 0x6E:
                return "Altera";
            case 0xEF:
                return "NEXCOM";
            case 0x70:
                return "Qualcomm";
            case 0xF1:
                return "Sony";
            case 0xF2:
                return "Cray Research";
            case 0x73:
                return "AMS";
            case 0xF4:
                return "Vitesse";
            case 0x75:
                return "Aster Electronics";
            case 0x76:
                return "Bay Networks";
            case 0xF7:
                return "Zentrum";
            case 0xF8:
                return "TRW";
            case 0x79:
                return "Thesys";
            case 0x7A:
                return "Solbourne Computer";
            case 0xFB:
                return "Allied-Signal";
            case 0x7C:
                return "Dialog Semiconductor";
            case 0xFD:
                return "Media Vision";
            case 0xFE:
                return "Numonyx Corporation";
            case 0x7F01:
                return "Cirrus Logic";
            case 0x7F02:
                return "National Instruments";
            case 0x7F04:
                return "Alcatel Mietec";
            case 0x7F07:
                return "JTAG Technologies";
            case 0x7F08:
                return "Loral";
            case 0x7F0B:
                return "Bestlink Systems";
            case 0x7F0D:
                return "GENNUM";
            case 0x7F0E:
                return "VideoLogic";
            case 0x7F10:
                return "Chip Express";
            case 0x7F13:
                return "TCSI";
            case 0x7F15:
                return "Hughes Aircraft";
            case 0x7F16:
                return "Lanstar Semiconductor";
            case 0x7F19:
                return "Music Semi";
            case 0x7F1A:
                return "Ericsson Components";
            case 0x7F1C:
                return "Eon Silicon Devices";
            case 0x7F1F:
                return "Integ.Memories Tech.";
            case 0x7F20:
                return "Corollary Inc.";
            case 0x7F23:
                return "EIV(Switzerland)";
            case 0x7F25:
                return "Zarlink(formerly Mitel)";
            case 0x7F26:
                return "Clearpoint";
            case 0x7F29:
                return "Vanguard";
            case 0x7F2A:
                return "Hagiwara Sys-Com";
            case 0x7F2C:
                return "Celestica";
            case 0x7F2F:
                return "Rohm Company Ltd.";
            case 0x7F31:
                return "Libit Signal Processing";
            case 0x7F32:
                return "Enhanced Memories Inc.";
            case 0x7F34:
                return "Adaptec Inc.";
            case 0x7F37:
                return "AMIC Technology";
            case 0x7F38:
                return "Adobe Systems";
            case 0x7F3B:
                return "Newport Digital";
            case 0x7F3D:
                return "T Square";
            case 0x7F3E:
                return "Seiko Epson";
            case 0x7F40:
                return "Viking Components";
            case 0x7F43:
                return "Suwa Electronics";
            case 0x7F45:
                return "Micron CMS";
            case 0x7F46:
                return "American Computer &Digital Components Inc";
            case 0x7F49:
                return "CPU Design";
            case 0x7F4A:
                return "Price Point";
            case 0x7F4C:
                return "Tellabs";
            case 0x7F4F:
                return "Transcend Information";
            case 0x7F51:
                return "CKD Corporation Ltd.";
            case 0x7F52:
                return "Capital Instruments, Inc.";
            case 0x7F54:
                return "Linvex Technology";
            case 0x7F57:
                return "Dynamem, Inc.";
            case 0x7F58:
                return "NERA ASA";
            case 0x7F5B:
                return "Acorn Computers";
            case 0x7F5D:
                return "Oak Technology, Inc.";
            case 0x7F5E:
                return "Itec Memory";
            case 0x7F61:
                return "Wintec Industries";
            case 0x7F62:
                return "Super PC Memory";
            case 0x7F64:
                return "Galvantech";
            case 0x7F67:
                return "GateField";
            case 0x7F68:
                return "Integrated Memory System";
            case 0x7F6B:
                return "Goldenram";
            case 0x7F6D:
                return "Cimaron Communications";
            case 0x7F6E:
                return "Nippon Steel Semi.Corp.";
            case 0x7F70:
                return "AMCC";
            case 0x7F73:
                return "Digital Microwave";
            case 0x7F75:
                return "MIMOS Semiconductor";
            case 0x7F76:
                return "Advanced Fibre";
            case 0x7F79:
                return "Acbel Polytech Inc.";
            case 0x7F7A:
                return "Apacer Technology";
            case 0x7F7C:
                return "FOXCONN";
            case 0x7F83:
                return "ILC Data Device";
            case 0x7F85:
                return "Micro Linear";
            case 0x7F86:
                return "Univ.Of NC";
            case 0x7F89:
                return "Nchip";
            case 0x7F8A:
                return "Galileo Tech";
            case 0x7F8C:
                return "Graychip";
            case 0x7F8F:
                return "Robert Bosch";
            case 0x7F91:
                return "DATARAM";
            case 0x7F92:
                return "United Microelec Corp.";
            case 0x7F94:
                return "Smart Modular";
            case 0x7F97:
                return "Qlogic";
            case 0x7F98:
                return "Kingston";
            case 0x7F9B:
                return "SpaSE";
            case 0x7F9D:
                return "Programmable Micro Corp";
            case 0x7F9E:
                return "DoD";
            case 0x7FA1:
                return "Dallas Semiconductor";
            case 0x7FA2:
                return "Omnivision";
            case 0x7FA4:
                return "Novatel Wireless";
            case 0x7FA7:
                return "Cabletron";
            case 0x7FA8:
                return "Silicon Technology";
            case 0x7FAB:
                return "Vantis";
            case 0x7FAD:
                return "Century";
            case 0x7FAE:
                return "Hal Computers";
            case 0x7FB0:
                return "Juniper Networks";
            case 0x7FB3:
                return "Tundra Semiconductor";
            case 0x7FB5:
                return "LightSpeed Semi.";
            case 0x7FB6:
                return "ZSP Corp.";
            case 0x7FB9:
                return "Dynachip";
            case 0x7FBA:
                return "PNY Electronics";
            case 0x7FBC:
                return "MMC Networks";
            case 0x7FBF:
                return "Broadcom";
            case 0x7FC1:
                return "V3 Semiconductor";
            case 0x7FC2:
                return "Flextronics(formerly Orbit)";
            case 0x7FC4:
                return "Transmeta";
            case 0x7FC7:
                return "Enhance 3000 Inc";
            case 0x7FC8:
                return "Tower Semiconductor";
            case 0x7FCB:
                return "Maxim Integrated Product";
            case 0x7FCD:
                return "Centaur Technology";
            case 0x7FCE:
                return "Unigen Corporation";
            case 0x7FD0:
                return "Memory Card Technology";
            case 0x7FD3:
                return "Aica Kogyo, Ltd.";
            case 0x7FD5:
                return "MSC Vertriebs GmbH";
            case 0x7FD6:
                return "AKM Company, Ltd.";
            case 0x7FD9:
                return "GSI Technology";
            case 0x7FDA:
                return "Dane-Elec (C Memory)";
            case 0x7FDC:
                return "Lara Technology";
            case 0x7FDF:
                return "Tanisys Technology";
            case 0x7FE0:
                return "Truevision";
            case 0x7FE3:
                return "MGV Memory";
            case 0x7FE5:
                return "Gadzoox Networks";
            case 0x7FE6:
                return "Multi Dimensional Cons.";
            case 0x7FE9:
                return "Triscend";
            case 0x7FEA:
                return "XaQti";
            case 0x7FEC:
                return "Clear Logic";
            case 0x7FEF:
                return "Advantage Memory";
            case 0x7FF1:
                return "LeCroy";
            case 0x7FF2:
                return "Yamaha Corporation";
            case 0x7FF4:
                return "NetLogic Microsystems";
            case 0x7FF7:
                return "BF Goodrich Data.";
            case 0x7FF8:
                return "Epigram";
            case 0x7FFB:
                return "Admor Memory";
            case 0x7FFD:
                return "Quadratics Superconductor";
            case 0x7FFE:
                return "3COM";

#endregion JEDEC

            case 0x0100:
                return "Digital Equipment Corporation";
            case 0x0101:
                return "3Com Corporation";
            case 0x0102:
                return "Megahertz Corporation";
            case 0x0104:
                return "Socket Communications";
            case 0x0105:
                return "TDK Corporation";
            case 0x0108:
                return "Standard Microsystems Corporation";
            case 0x0109:
                return "Motorola Corporation";
            case 0x010b:
                return "National Instruments";
            case 0x0115:
                return "US Robotics Corporation";
            case 0x0121:
                return "Olicom";
            case 0x0126:
                return "Proxim";
            case 0x0128:
                return "Megahertz Corporation";
            case 0x012F:
                return "Adaptec Corporation";
            case 0x0137:
                return "Quatech";
            case 0x0138:
                return "Compaq";
            case 0x0140:
                return "Ositech";
            case 0x0143:
                return "D-Link";
            case 0x0149:
                return "Netgear";
            case 0x014D:
                return "Simple Technology";
            case 0x0156:
                return "Lucent Technologies";
            case 0x015F:
                return "Aironet Wireless Communications";
            case 0x016B:
                return "Ericsson";
            case 0x016C:
                return "Psion";
            case 0x0183:
                return "Compaq";
            case 0x0186:
                return "Kingston";
            case 0x0192:
                return "Sierra Wireless";
            case 0x0194:
                return "Dayna Corporation";
            case 0x01a6:
                return "Raytheon";
            case 0x01BF:
                return "Belkin";
            case 0x01EB:
                return "Bay Networks";
            case 0x0200:
                return "Farallon Communications";
            case 0x021B:
                return "Telecom Device";
            case 0x023D:
                return "Nokia Communications";
            case 0x0250:
                return "Samsung";
            case 0x0264:
                return "Anycom";
            case 0x0268:
                return "Alvarion Ltd.";
            case 0x026C:
                return "Symbol";
            case 0x026F:
                return "BUFFALO";
            case 0x0274:
                return "The Linksys Group";
            case 0x0288:
                return "NEC Infrontia";
            case 0x028A:
                return "I-O DATA";
            case 0x02AA:
                return "Asustek Computer";
            case 0x02AC:
                return "Siemens";
            case 0x02D2:
                return "Microsoft Corporation";
            case 0x02DF:
                return "AmbiCom Inc";
            case 0x0a02:
                return "BreezeCOM";
            case 0x10CD:
                return "NewMedia";
            case 0x1668:
                return "ACTIONTEC";
            case 0x3401:
                return "Lasat Communications A/S";
            case 0x4E01:
                return "Lexar Media";
            case 0x5241:
                return "Archos";
            case 0x890F:
                return "Dual";
            case 0x8A01:
                return "Compex Corporation";
            case 0xC001:
                return "Contec";
            case 0xC00B:
                return "MACNICA";
            case 0xC00C:
                return "Roland";
            case 0xC00F:
                return "Corega K.K.";
            case 0xC012:
                return "Hagiwara SYS-COM";
            case 0xC015:
                return "RATOC System Inc.";
            case 0xC020:
                return "NextCom K.K.";
            case 0xC250:
                return "EMTAC Technology Corporation";
            case 0xD601:
                return "Elsa";
            default:
                return string.Format(Localization.Unknown_vendor_id_0, id);
        }
    }
}