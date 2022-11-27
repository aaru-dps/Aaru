// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Saturn.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes Sega Saturn IP.BIN.
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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.Console;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Decoders.Sega;

/// <summary>Represents the IP.BIN from a SEGA Saturn</summary>
[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class Saturn
{
    /// <summary>Decodes an IP.BIN sector in Saturn format</summary>
    /// <param name="ipbin_sector">IP.BIN sector</param>
    /// <returns>Decoded IP.BIN</returns>
    public static IPBin? DecodeIPBin(byte[] ipbin_sector)
    {
        if(ipbin_sector == null)
            return null;

        if(ipbin_sector.Length < 512)
            return null;

        IPBin ipbin = Marshal.ByteArrayToStructureLittleEndian<IPBin>(ipbin_sector);

        AaruConsole.DebugWriteLine("Saturn IP.BIN Decoder", "saturn_ipbin.maker_id = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.maker_id));

        AaruConsole.DebugWriteLine("Saturn IP.BIN Decoder", "saturn_ipbin.product_no = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.product_no));

        AaruConsole.DebugWriteLine("Saturn IP.BIN Decoder", "saturn_ipbin.product_version = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.product_version));

        AaruConsole.DebugWriteLine("Saturn IP.BIN Decoder", "saturn_ipbin.release_date = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.release_date));

        AaruConsole.DebugWriteLine("Saturn IP.BIN Decoder", "saturn_ipbin.saturn_media = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.saturn_media));

        AaruConsole.DebugWriteLine("Saturn IP.BIN Decoder", "saturn_ipbin.disc_no = {0}", (char)ipbin.disc_no);

        AaruConsole.DebugWriteLine("Saturn IP.BIN Decoder", "saturn_ipbin.disc_no_separator = \"{0}\"",
                                   (char)ipbin.disc_no_separator);

        AaruConsole.DebugWriteLine("Saturn IP.BIN Decoder", "saturn_ipbin.disc_total_nos = {0}",
                                   (char)ipbin.disc_total_nos);

        AaruConsole.DebugWriteLine("Saturn IP.BIN Decoder", "saturn_ipbin.release_date = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.release_date));

        AaruConsole.DebugWriteLine("Saturn IP.BIN Decoder", "saturn_ipbin.spare_space1 = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.spare_space1));

        AaruConsole.DebugWriteLine("Saturn IP.BIN Decoder", "saturn_ipbin.region_codes = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.region_codes));

        AaruConsole.DebugWriteLine("Saturn IP.BIN Decoder", "saturn_ipbin.peripherals = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.peripherals));

        AaruConsole.DebugWriteLine("Saturn IP.BIN Decoder", "saturn_ipbin.product_name = \"{0}\"",
                                   Encoding.ASCII.GetString(ipbin.product_name));

        return Encoding.ASCII.GetString(ipbin.SegaHardwareID) == "SEGA SEGASATURN " ? ipbin : null;
    }

    /// <summary>Pretty prints a decoded IP.BIN in Saturn format</summary>
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

        IPBinInformation.AppendFormat("Product number: {0}", Encoding.ASCII.GetString(ipbin.product_no)).AppendLine();

        IPBinInformation.AppendFormat(Localization.Product_version_0, Encoding.ASCII.GetString(ipbin.product_version)).
                         AppendLine();

        IPBinInformation.AppendFormat(Localization.Release_date_0, ipbindate).AppendLine();

        IPBinInformation.AppendFormat(Localization.Disc_number_0_of_1, (char)ipbin.disc_no, (char)ipbin.disc_total_nos).
                         AppendLine();

        IPBinInformation.AppendFormat(Localization.Peripherals).AppendLine();

        foreach(byte peripheral in ipbin.peripherals)
            switch((char)peripheral)
            {
                case 'A':
                    IPBinInformation.AppendLine(Localization.Game_supports_analog_controller);

                    break;
                case 'J':
                    IPBinInformation.AppendLine(Localization.Game_supports_JoyPad);

                    break;
                case 'K':
                    IPBinInformation.AppendLine(Localization.Game_supports_keyboard);

                    break;
                case 'M':
                    IPBinInformation.AppendLine(Localization.Game_supports_mouse);

                    break;
                case 'S':
                    IPBinInformation.AppendLine(Localization.Game_supports_analog_steering_controller);

                    break;
                case 'T':
                    IPBinInformation.AppendLine(Localization.Game_supports_multitap);

                    break;
                case ' ': break;
                default:
                    IPBinInformation.AppendFormat(Localization.Game_supports_unknown_peripheral_0, peripheral).
                                     AppendLine();

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
                case 'T':
                    IPBinInformation.AppendLine(Localization.Asia_NTSC);

                    break;
                case ' ': break;
                default:
                    IPBinInformation.AppendFormat(Localization.Game_supports_unknown_region_0, region).AppendLine();

                    break;
            }

        return IPBinInformation.ToString();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct IPBin
    {
        /// <summary>Must be "SEGA SEGASATURN "</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] SegaHardwareID;
        /// <summary>0x010, "SEGA ENTERPRISES"</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] maker_id;
        /// <summary>0x020, Product number</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public byte[] product_no;
        /// <summary>0x02A, Product version</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] product_version;
        /// <summary>0x030, YYYYMMDD</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] release_date;
        /// <summary>0x038, "CD-"</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] saturn_media;
        /// <summary>0x03B, Disc number</summary>
        public byte disc_no;
        /// <summary>// 0x03C, '/'</summary>
        public byte disc_no_separator;
        /// <summary>// 0x03D, Total number of discs</summary>
        public byte disc_total_nos;
        /// <summary>0x03E, "  "</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] spare_space1;
        /// <summary>0x040, Region codes, space-filled</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] region_codes;
        /// <summary>0x050, Supported peripherals, see above</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] peripherals;
        /// <summary>0x060, Game name, space-filled</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 112)]
        public byte[] product_name;
    }
}