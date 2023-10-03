// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Dreamcast.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes Sega Dreamcast IP.BIN.
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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.Console;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Decoders.Sega;

/// <summary>Represents the IP.BIN from a SEGA Dreamcast</summary>
[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class Dreamcast
{
    const string MODULE_NAME = "Dreamcast IP.BIN Decoder";

    /// <summary>Decodes an IP.BIN sector in Dreamcast format</summary>
    /// <param name="ipbin_sector">IP.BIN sector</param>
    /// <returns>Decoded IP.BIN</returns>
    public static IPBin? DecodeIPBin(byte[] ipbin_sector)
    {
        if(ipbin_sector == null)
            return null;

        if(ipbin_sector.Length < 512)
            return null;

        IPBin ipbin = Marshal.ByteArrayToStructureLittleEndian<IPBin>(ipbin_sector);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dreamcast_ipbin.maker_id = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.maker_id));

        AaruConsole.DebugWriteLine(MODULE_NAME, "dreamcast_ipbin.spare_space1 = \"{0}\"",
                                   (char)ipbin.spare_space1);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dreamcast_ipbin.dreamcast_media = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.dreamcast_media));

        AaruConsole.DebugWriteLine(MODULE_NAME, "dreamcast_ipbin.disc_no = {0}", (char)ipbin.disc_no);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dreamcast_ipbin.disc_no_separator = \"{0}\"",
                                   (char)ipbin.disc_no_separator);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dreamcast_ipbin.disc_total_nos = \"{0}\"",
                                   (char)ipbin.disc_total_nos);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dreamcast_ipbin.spare_space2 = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.spare_space2));

        AaruConsole.DebugWriteLine(MODULE_NAME, "dreamcast_ipbin.region_codes = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.region_codes));

        AaruConsole.DebugWriteLine(MODULE_NAME, "dreamcast_ipbin.peripherals = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.peripherals));

        AaruConsole.DebugWriteLine(MODULE_NAME, "dreamcast_ipbin.product_no = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.product_no));

        AaruConsole.DebugWriteLine(MODULE_NAME, "dreamcast_ipbin.product_version = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.product_version));

        AaruConsole.DebugWriteLine(MODULE_NAME, "dreamcast_ipbin.release_date = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.release_date));

        AaruConsole.DebugWriteLine(MODULE_NAME, "dreamcast_ipbin.spare_space3 = \"{0}\"",
                                   (char)ipbin.spare_space3);

        AaruConsole.DebugWriteLine(MODULE_NAME, "dreamcast_ipbin.boot_filename = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.boot_filename));

        AaruConsole.DebugWriteLine(MODULE_NAME, "dreamcast_ipbin.producer = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.producer));

        AaruConsole.DebugWriteLine(MODULE_NAME, "dreamcast_ipbin.product_name = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.product_name));

        return Encoding.ASCII.GetString(ipbin.SegaHardwareID) == "SEGA SEGAKATANA " ? ipbin : null;
    }

    /// <summary>Pretty prints a decoded IP.BIN in Dreamcast format</summary>
    /// <param name="decoded">Decoded IP.BIN</param>
    /// <returns>Description of the IP.BIN contents</returns>
    public static string Prettify(IPBin? decoded)
    {
        if(decoded == null)
            return null;

        IPBin ipbin = decoded.Value;

        var IPBinInformation = new StringBuilder();

        IPBinInformation.AppendLine("--------------------------------");
        IPBinInformation.AppendLine(Localization.SEGA_IP_BIN_INFORMATION);
        IPBinInformation.AppendLine("--------------------------------");

        // Decoding all data
        CultureInfo provider  = CultureInfo.InvariantCulture;
        var         ipbindate = DateTime.ParseExact(Encoding.ASCII.GetString(ipbin.release_date), "yyyyMMdd", provider);

        IPBinInformation.AppendFormat(Localization.Product_name_0, Encoding.ASCII.GetString(ipbin.product_name)).
                         AppendLine();

        IPBinInformation.AppendFormat(Localization.Product_version_0, Encoding.ASCII.GetString(ipbin.product_version)).
                         AppendLine();

        IPBinInformation.AppendFormat(Localization.Product_CRC_0, ipbin.dreamcast_crc).AppendLine();
        IPBinInformation.AppendFormat(Localization.Producer_0, Encoding.ASCII.GetString(ipbin.producer)).AppendLine();

        IPBinInformation.AppendFormat(Localization.Disc_media_0, Encoding.ASCII.GetString(ipbin.dreamcast_media)).
                         AppendLine();

        IPBinInformation.AppendFormat(Localization.Disc_number_0_of_1, (char)ipbin.disc_no, (char)ipbin.disc_total_nos).
                         AppendLine();

        IPBinInformation.AppendFormat(Localization.Release_date_0, ipbindate).AppendLine();

        switch(Encoding.ASCII.GetString(ipbin.boot_filename))
        {
            case "1ST_READ.BIN":
                IPBinInformation.AppendLine(Localization.Disc_boots_natively);

                break;
            case "0WINCE.BIN  ":
                IPBinInformation.AppendLine(Localization.Disc_boots_using_Windows_CE);

                break;
            default:
                IPBinInformation.AppendFormat(Localization.Disc_boots_using_unknown_loader_0,
                                              Encoding.ASCII.GetString(ipbin.boot_filename)).AppendLine();

                break;
        }

        IPBinInformation.AppendLine(Localization.Regions_supported);

        foreach(byte region in ipbin.region_codes)
            switch((char)region)
            {
                case 'J':
                    IPBinInformation.AppendLine(Localization.Japanese_NTSC);

                    break;
                case 'U':
                    IPBinInformation.AppendLine(Localization.North_America_NTSC);

                    break;
                case 'E':
                    IPBinInformation.AppendLine(Localization.Europe_PAL);

                    break;
                case ' ': break;
                default:
                    IPBinInformation.AppendFormat(Localization.Game_supports_unknown_region_0, region).AppendLine();

                    break;
            }

        int iPeripherals = int.Parse(Encoding.ASCII.GetString(ipbin.peripherals), NumberStyles.HexNumber);

        if((iPeripherals & 0x00000001) == 0x00000001)
            IPBinInformation.AppendLine(Localization.Game_uses_Windows_CE);

        IPBinInformation.AppendFormat(Localization.Peripherals).AppendLine();

        if((iPeripherals & 0x00000010) == 0x00000010)
            IPBinInformation.AppendLine(Localization.Game_supports_the_VGA_Box);

        if((iPeripherals & 0x00000100) == 0x00000100)
            IPBinInformation.AppendLine(Localization.Game_supports_other_expansion);

        if((iPeripherals & 0x00000200) == 0x00000200)
            IPBinInformation.AppendLine(Localization.Game_supports_Puru_Puru_pack);

        if((iPeripherals & 0x00000400) == 0x00000400)
            IPBinInformation.AppendLine(Localization.Game_supports_Mike_Device);

        if((iPeripherals & 0x00000800) == 0x00000800)
            IPBinInformation.AppendLine(Localization.Game_supports_Memory_Card);

        if((iPeripherals & 0x00001000) == 0x00001000)
            IPBinInformation.AppendLine(Localization.Game_requires_A_B_Start_buttons_and_D_Pad);

        if((iPeripherals & 0x00002000) == 0x00002000)
            IPBinInformation.AppendLine(Localization.Game_requires_C_button);

        if((iPeripherals & 0x00004000) == 0x00004000)
            IPBinInformation.AppendLine(Localization.Game_requires_D_button);

        if((iPeripherals & 0x00008000) == 0x00008000)
            IPBinInformation.AppendLine(Localization.Game_requires_X_button);

        if((iPeripherals & 0x00010000) == 0x00010000)
            IPBinInformation.AppendLine(Localization.Game_requires_Y_button);

        if((iPeripherals & 0x00020000) == 0x00020000)
            IPBinInformation.AppendLine(Localization.Game_requires_Z_button);

        if((iPeripherals & 0x00040000) == 0x00040000)
            IPBinInformation.AppendLine(Localization.Game_requires_expanded_direction_buttons);

        if((iPeripherals & 0x00080000) == 0x00080000)
            IPBinInformation.AppendLine(Localization.Game_requires_analog_R_trigger);

        if((iPeripherals & 0x00100000) == 0x00100000)
            IPBinInformation.AppendLine(Localization.Game_requires_analog_L_trigger);

        if((iPeripherals & 0x00200000) == 0x00200000)
            IPBinInformation.AppendLine(Localization.Game_requires_analog_horizontal_controller);

        if((iPeripherals & 0x00400000) == 0x00400000)
            IPBinInformation.AppendLine(Localization.Game_requires_analog_vertical_controller);

        if((iPeripherals & 0x00800000) == 0x00800000)
            IPBinInformation.AppendLine(Localization.Game_requires_expanded_analog_horizontal_controller);

        if((iPeripherals & 0x01000000) == 0x01000000)
            IPBinInformation.AppendLine(Localization.Game_requires_expanded_analog_vertical_controller);

        if((iPeripherals & 0x02000000) == 0x02000000)
            IPBinInformation.AppendLine(Localization.Game_supports_Gun);

        if((iPeripherals & 0x04000000) == 0x04000000)
            IPBinInformation.AppendLine(Localization.Game_supports_keyboard);

        if((iPeripherals & 0x08000000) == 0x08000000)
            IPBinInformation.AppendLine(Localization.Game_supports_mouse);

        if((iPeripherals & 0xEE) != 0)
            IPBinInformation.AppendFormat(Localization.Game_supports_unknown_peripherals_mask_0, iPeripherals & 0xEE);

        return IPBinInformation.ToString();
    }

    /// <summary>SEGA IP.BIN format for Dreamcast</summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IPBin
    {
        /// <summary>Must be "SEGA SEGAKATANA "</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] SegaHardwareID;
        /// <summary>0x010, "SEGA ENTERPRISES"</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] maker_id;
        /// <summary>0x020, CRC of product_no and product_version</summary>
        public uint dreamcast_crc;
        /// <summary>0x024, " "</summary>
        public byte spare_space1;
        /// <summary>0x025, "GD-ROM"</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] dreamcast_media;
        /// <summary>0x02B, Disc number</summary>
        public byte disc_no;
        /// <summary>0x02C, '/'</summary>
        public byte disc_no_separator;
        /// <summary>0x02D, Total number of discs</summary>
        public byte disc_total_nos;
        /// <summary>0x02E, "  "</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] spare_space2;
        /// <summary>0x030, Region codes, space-filled</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] region_codes;
        /// <summary>0x038, Supported peripherals, bitwise</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public byte[] peripherals;
        /// <summary>0x03F, ' '</summary>
        public byte spare_space3;
        /// <summary>0x040, Product number</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public byte[] product_no;
        /// <summary>0x04A, Product version</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] product_version;
        /// <summary>0x050, YYYYMMDD</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] release_date;
        /// <summary>0x058, "  "</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] spare_space4;
        /// <summary>0x060, Usually "1ST_READ.BIN" or "0WINCE.BIN  "</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] boot_filename;
        /// <summary>0x06C, "  "</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] spare_space5;
        /// <summary>0x070, Game producer, space-filled</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] producer;
        /// <summary>0x080, Game name, space-filled</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)]
        public byte[] product_name;
    }
}