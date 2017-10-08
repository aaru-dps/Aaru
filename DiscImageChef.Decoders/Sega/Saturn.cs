// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Saturn.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Component
//
// --[ Description ] ----------------------------------------------------------
//
//     Description
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
// Copyright © 2011-2017 Natalia Portillo
// ****************************************************************************/
using System;
using System.Globalization;
using System.Text;
using DiscImageChef.Console;

namespace DiscImageChef.Decoders.Sega
{
    public static class Saturn
    {
        public struct IPBin
        {
            public byte[] SegaHardwareID; //16
            public byte[] maker_id; //[16         // 0x010, "SEGA ENTERPRISES"
            public byte[] product_no; //[10       // 0x020, Product number
            public byte[] product_version; //[6   // 0x02A, Product version
            public byte[] release_date; //[8      // 0x030, YYYYMMDD
            public byte[] saturn_media; //[3      // 0x038, "CD-"
            public byte[] disc_no; //[1           // 0x03B, Disc number
            public byte[] disc_no_separator; //[1 // 0x03C, '/'
            public byte[] disc_total_nos; //[1    // 0x03D, Total number of discs
            public byte[] spare_space1; //[2      // 0x03E, "  "
            public byte[] region_codes; //[16     // 0x040, Region codes, space-filled
            public byte[] peripherals; //[16      // 0x050, Supported peripherals, see above
            public byte[] product_name; //[112    // 0x060, Game name, space-filled
        }

        public static IPBin? DecodeIPBin(byte[] ipbin_sector)
        {
            if(ipbin_sector == null)
                return null;

            if(ipbin_sector.Length < 512)
                return null;

            IPBin ipbin = new IPBin();
            ipbin.SegaHardwareID = new byte[16];
            // Definitions following
            ipbin.maker_id = new byte[16];         // 0x010, "SEGA ENTERPRISES"
            ipbin.product_no = new byte[10];       // 0x020, Product number
            ipbin.product_version = new byte[6];   // 0x02A, Product version
            ipbin.release_date = new byte[8];      // 0x030, YYYYMMDD
            ipbin.saturn_media = new byte[3];      // 0x038, "CD-"
            ipbin.disc_no = new byte[1];           // 0x03B, Disc number
            ipbin.disc_no_separator = new byte[1]; // 0x03C, '/'
            ipbin.disc_total_nos = new byte[1];    // 0x03D, Total number of discs
            ipbin.spare_space1 = new byte[2];      // 0x03E, "  "
            ipbin.region_codes = new byte[16];     // 0x040, Region codes, space-filled
            ipbin.peripherals = new byte[16];      // 0x050, Supported peripherals, see above
            ipbin.product_name = new byte[112];    // 0x060, Game name, space-filled
            // Reading all data
            Array.Copy(ipbin_sector, 0x010, ipbin.maker_id, 0, 16);         // "SEGA ENTERPRISES"
            Array.Copy(ipbin_sector, 0x020, ipbin.product_no, 0, 10);       // Product number
            Array.Copy(ipbin_sector, 0x02A, ipbin.product_version, 0, 6);   // Product version
            Array.Copy(ipbin_sector, 0x030, ipbin.release_date, 0, 8);      // YYYYMMDD
            Array.Copy(ipbin_sector, 0x038, ipbin.saturn_media, 0, 3);      // "CD-"
            Array.Copy(ipbin_sector, 0x03B, ipbin.disc_no, 0, 1);           // Disc number
            Array.Copy(ipbin_sector, 0x03C, ipbin.disc_no_separator, 0, 1); // '/'
            Array.Copy(ipbin_sector, 0x03D, ipbin.disc_total_nos, 0, 1);    // Total number of discs
            Array.Copy(ipbin_sector, 0x03E, ipbin.spare_space1, 0, 2);      // "  "
            Array.Copy(ipbin_sector, 0x040, ipbin.region_codes, 0, 16);     // Region codes, space-filled
            Array.Copy(ipbin_sector, 0x050, ipbin.peripherals, 0, 16);      // Supported peripherals, see above
            Array.Copy(ipbin_sector, 0x060, ipbin.product_name, 0, 112);    // Game name, space-filled

            DicConsole.DebugWriteLine("ISO9660 plugin", "saturn_ipbin.maker_id = \"{0}\"", Encoding.ASCII.GetString(ipbin.maker_id));
            DicConsole.DebugWriteLine("ISO9660 plugin", "saturn_ipbin.product_no = \"{0}\"", Encoding.ASCII.GetString(ipbin.product_no));
            DicConsole.DebugWriteLine("ISO9660 plugin", "saturn_ipbin.product_version = \"{0}\"", Encoding.ASCII.GetString(ipbin.product_version));
            DicConsole.DebugWriteLine("ISO9660 plugin", "saturn_ipbin.release_datedate = \"{0}\"", Encoding.ASCII.GetString(ipbin.release_date));
            DicConsole.DebugWriteLine("ISO9660 plugin", "saturn_ipbin.saturn_media = \"{0}\"", Encoding.ASCII.GetString(ipbin.saturn_media));
            DicConsole.DebugWriteLine("ISO9660 plugin", "saturn_ipbin.disc_no = {0}", Encoding.ASCII.GetString(ipbin.disc_no));
            DicConsole.DebugWriteLine("ISO9660 plugin", "saturn_ipbin.disc_no_separator = \"{0}\"", Encoding.ASCII.GetString(ipbin.disc_no_separator));
            DicConsole.DebugWriteLine("ISO9660 plugin", "saturn_ipbin.disc_total_nos = {0}", Encoding.ASCII.GetString(ipbin.disc_total_nos));
            DicConsole.DebugWriteLine("ISO9660 plugin", "saturn_ipbin.release_date = \"{0}\"", Encoding.ASCII.GetString(ipbin.release_date));
            DicConsole.DebugWriteLine("ISO9660 plugin", "saturn_ipbin.spare_space1 = \"{0}\"", Encoding.ASCII.GetString(ipbin.spare_space1));
            DicConsole.DebugWriteLine("ISO9660 plugin", "saturn_ipbin.region_codes = \"{0}\"", Encoding.ASCII.GetString(ipbin.region_codes));
            DicConsole.DebugWriteLine("ISO9660 plugin", "saturn_ipbin.peripherals = \"{0}\"", Encoding.ASCII.GetString(ipbin.peripherals));
            DicConsole.DebugWriteLine("ISO9660 plugin", "saturn_ipbin.product_name = \"{0}\"", Encoding.ASCII.GetString(ipbin.product_name));

            string id = Encoding.ASCII.GetString(ipbin.SegaHardwareID);

            if(id == "SEGA SEGASATURN ")
                return ipbin;
            else
                return null;
        }

        public static string Prettify(IPBin? decoded)
        {
            if(decoded == null)
                return null;

            IPBin ipbin = decoded.Value;

            StringBuilder IPBinInformation = new StringBuilder();

            IPBinInformation.AppendLine("--------------------------------");
            IPBinInformation.AppendLine("SEGA IP.BIN INFORMATION:");
            IPBinInformation.AppendLine("--------------------------------");

            // Decoding all data
            DateTime ipbindate;
            CultureInfo provider = CultureInfo.InvariantCulture;
            ipbindate = DateTime.ParseExact(Encoding.ASCII.GetString(ipbin.release_date), "yyyyMMdd", provider);
            IPBinInformation.AppendFormat("Product name: {0}", Encoding.ASCII.GetString(ipbin.product_name)).AppendLine();
            IPBinInformation.AppendFormat("Product number: {0}", Encoding.ASCII.GetString(ipbin.product_no)).AppendLine();
            IPBinInformation.AppendFormat("Product version: {0}", Encoding.ASCII.GetString(ipbin.product_version)).AppendLine();
            IPBinInformation.AppendFormat("Release date: {0}", ipbindate).AppendLine();
            IPBinInformation.AppendFormat("Disc number {0} of {1}", Encoding.ASCII.GetString(ipbin.disc_no), Encoding.ASCII.GetString(ipbin.disc_total_nos)).AppendLine();

            IPBinInformation.AppendFormat("Peripherals:").AppendLine();
            foreach(byte peripheral in ipbin.peripherals)
            {
                switch((char)peripheral)
                {
                    case 'A':
                        IPBinInformation.AppendLine("Game supports analog controller.");
                        break;
                    case 'J':
                        IPBinInformation.AppendLine("Game supports JoyPad.");
                        break;
                    case 'K':
                        IPBinInformation.AppendLine("Game supports keyboard.");
                        break;
                    case 'M':
                        IPBinInformation.AppendLine("Game supports mouse.");
                        break;
                    case 'S':
                        IPBinInformation.AppendLine("Game supports analog steering controller.");
                        break;
                    case 'T':
                        IPBinInformation.AppendLine("Game supports multitap.");
                        break;
                    case ' ':
                        break;
                    default:
                        IPBinInformation.AppendFormat("Game supports unknown peripheral {0}.", peripheral).AppendLine();
                        break;
                }
            }
            IPBinInformation.AppendLine("Regions supported:");
            foreach(byte region in ipbin.region_codes)
            {
                switch((char)region)
                {
                    case 'J':
                        IPBinInformation.AppendLine("Japanese NTSC.");
                        break;
                    case 'U':
                        IPBinInformation.AppendLine("North America NTSC.");
                        break;
                    case 'E':
                        IPBinInformation.AppendLine("Europe PAL.");
                        break;
                    case 'T':
                        IPBinInformation.AppendLine("Asia NTSC.");
                        break;
                    case ' ':
                        break;
                    default:
                        IPBinInformation.AppendFormat("Game supports unknown region {0}.", region).AppendLine();
                        break;
                }
            }

            return IPBinInformation.ToString();
        }
    }
}
