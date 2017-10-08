// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Dreamcast.cs
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
    public class Dreamcast
    {
        public struct IPBin
        {
            public byte[] SegaHardwareID; //16
            public byte[] maker_id; //16         // 0x010, "SEGA ENTERPRISES"
            public byte[] dreamcast_crc; //4     // 0x020, CRC of product_no and product_version
            public byte[] spare_space1; //1      // 0x024, " "
            public byte[] dreamcast_media; //6   // 0x025, "GD-ROM"
            public byte[] disc_no; //1           // 0x02B, Disc number
            public byte[] disc_no_separator; //1 // 0x02C, '/'
            public byte[] disc_total_nos; //1    // 0x02D, Total number of discs
            public byte[] spare_space2; //2     // 0x02E, "  "
            public byte[] region_codes; //8      // 0x030, Region codes, space-filled
            public byte[] peripherals; //7       // 0x038, Supported peripherals, bitwise
            public byte[] product_no; //10       // 0x03C, Product number
            public byte[] product_version; //6  // 0x046, Product version
            public byte[] release_date; //8      // 0x04C, YYYYMMDD
            public byte[] spare_space3; //8      // 0x054, "  "
            public byte[] boot_filename; //12    // 0x05C, Usually "1ST_READ.BIN" or "0WINCE.BIN  "
            public byte[] producer; //16         // 0x068, Game producer, space-filled
            public byte[] product_name; //128    // 0x078, Game name, space-filled
        }

        public static IPBin? DecodeIPBin(byte[] ipbin_sector)
        {
            if(ipbin_sector == null)
                return null;

            if(ipbin_sector.Length < 512)
                return null;

            IPBin ipbin = new IPBin();
            ipbin.SegaHardwareID = new byte[16];
            // Declarations following
            ipbin.maker_id = new byte[16];         // 0x010, "SEGA ENTERPRISES"
            ipbin.dreamcast_crc = new byte[4];     // 0x020, CRC of product_no and product_version
            ipbin.spare_space1 = new byte[1];      // 0x024, " "
            ipbin.dreamcast_media = new byte[6];   // 0x025, "GD-ROM"
            ipbin.disc_no = new byte[1];           // 0x02B, Disc number
            ipbin.disc_no_separator = new byte[1]; // 0x02C, '/'
            ipbin.disc_total_nos = new byte[1];    // 0x02D, Total number of discs
            ipbin.spare_space2 = new byte[2];       // 0x02E, "  "
            ipbin.region_codes = new byte[8];      // 0x030, Region codes, space-filled
            ipbin.peripherals = new byte[7];       // 0x038, Supported peripherals, bitwise
            ipbin.product_no = new byte[10];       // 0x03C, Product number
            ipbin.product_version = new byte[6];    // 0x046, Product version
            ipbin.release_date = new byte[8];      // 0x04C, YYYYMMDD
            ipbin.spare_space3 = new byte[8];      // 0x054, "  "
            ipbin.boot_filename = new byte[12];    // 0x05C, Usually "1ST_READ.BIN" or "0WINCE.BIN  "
            ipbin.producer = new byte[16];         // 0x068, Game producer, space-filled
            ipbin.product_name = new byte[128];    // 0x078, Game name, space-filled
                                                   // Reading all data
            Array.Copy(ipbin_sector, 0x010, ipbin.maker_id, 0, 16);      // "SEGA ENTERPRISES"
            Array.Copy(ipbin_sector, 0x020, ipbin.dreamcast_crc, 0, 4);         // CRC of product_no and product_version (hex)
            Array.Copy(ipbin_sector, 0x024, ipbin.spare_space1, 0, 1);       // " "
            Array.Copy(ipbin_sector, 0x025, ipbin.dreamcast_media, 0, 6);          // "GD-ROM"
            Array.Copy(ipbin_sector, 0x02B, ipbin.disc_no, 0, 1);         // Disc number
            Array.Copy(ipbin_sector, 0x02C, ipbin.disc_no_separator, 0, 1);       // '/'
            Array.Copy(ipbin_sector, 0x02D, ipbin.disc_total_nos, 0, 1);         // Total number of discs
            Array.Copy(ipbin_sector, 0x02E, ipbin.spare_space2, 0, 2);          // "  "
            Array.Copy(ipbin_sector, 0x030, ipbin.region_codes, 0, 8);          // Region codes, space-filled
            Array.Copy(ipbin_sector, 0x038, ipbin.peripherals, 0, 7);     // Supported peripherals, hexadecimal
            Array.Copy(ipbin_sector, 0x040, ipbin.product_no, 0, 10);     // Product number
            Array.Copy(ipbin_sector, 0x04A, ipbin.product_version, 0, 6);      // Product version
            Array.Copy(ipbin_sector, 0x050, ipbin.release_date, 0, 8);     // YYYYMMDD
            Array.Copy(ipbin_sector, 0x058, ipbin.spare_space3, 0, 8);        // "  "
            Array.Copy(ipbin_sector, 0x060, ipbin.boot_filename, 0, 12);     // Usually "1ST_READ.BIN" or "0WINCE.BIN  "
            Array.Copy(ipbin_sector, 0x070, ipbin.producer, 0, 16);     // Game producer, space-filled
            Array.Copy(ipbin_sector, 0x080, ipbin.product_name, 0, 128);     // Game name, space-filled

            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.maker_id = \"{0}\"", Encoding.ASCII.GetString(ipbin.maker_id));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.dreamcast_crc = 0x{0}", Encoding.ASCII.GetString(ipbin.dreamcast_crc));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.spare_space1 = \"{0}\"", Encoding.ASCII.GetString(ipbin.spare_space1));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.dreamcast_media = \"{0}\"", Encoding.ASCII.GetString(ipbin.dreamcast_media));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.disc_no = {0}", Encoding.ASCII.GetString(ipbin.disc_no));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.disc_no_separator = \"{0}\"", Encoding.ASCII.GetString(ipbin.disc_no_separator));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.disc_total_nos = \"{0}\"", Encoding.ASCII.GetString(ipbin.disc_total_nos));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.spare_space2 = \"{0}\"", Encoding.ASCII.GetString(ipbin.spare_space2));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.region_codes = \"{0}\"", Encoding.ASCII.GetString(ipbin.region_codes));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.peripherals = \"{0}\"", Encoding.ASCII.GetString(ipbin.peripherals));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.product_no = \"{0}\"", Encoding.ASCII.GetString(ipbin.product_no));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.product_version = \"{0}\"", Encoding.ASCII.GetString(ipbin.product_version));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.release_date = \"{0}\"", Encoding.ASCII.GetString(ipbin.release_date));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.spare_space3 = \"{0}\"", Encoding.ASCII.GetString(ipbin.spare_space3));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.boot_filename = \"{0}\"", Encoding.ASCII.GetString(ipbin.boot_filename));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.producer = \"{0}\"", Encoding.ASCII.GetString(ipbin.producer));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.product_name = \"{0}\"", Encoding.ASCII.GetString(ipbin.product_name));

            string id = Encoding.ASCII.GetString(ipbin.SegaHardwareID);

            if(id == "SEGA SEGAKATANA ")
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
            IPBinInformation.AppendFormat("Product version: {0}", Encoding.ASCII.GetString(ipbin.product_version)).AppendLine();
            IPBinInformation.AppendFormat("Producer: {0}", Encoding.ASCII.GetString(ipbin.producer)).AppendLine();
            IPBinInformation.AppendFormat("Disc media: {0}", Encoding.ASCII.GetString(ipbin.dreamcast_media)).AppendLine();
            IPBinInformation.AppendFormat("Disc number {0} of {1}", Encoding.ASCII.GetString(ipbin.disc_no), Encoding.ASCII.GetString(ipbin.disc_total_nos)).AppendLine();
            IPBinInformation.AppendFormat("Release date: {0}", ipbindate).AppendLine();
            switch(Encoding.ASCII.GetString(ipbin.boot_filename))
            {
                case "1ST_READ.BIN":
                    IPBinInformation.AppendLine("Disc boots natively.");
                    break;
                case "0WINCE.BIN  ":
                    IPBinInformation.AppendLine("Disc boots using Windows CE.");
                    break;
                default:
                    IPBinInformation.AppendFormat("Disc boots using unknown loader: {0}.", Encoding.ASCII.GetString(ipbin.boot_filename)).AppendLine();
                    break;
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
                    case ' ':
                        break;
                    default:
                        IPBinInformation.AppendFormat("Game supports unknown region {0}.", region).AppendLine();
                        break;
                }
            }

            int iPeripherals = int.Parse(Encoding.ASCII.GetString(ipbin.peripherals), NumberStyles.HexNumber);

            if((iPeripherals & 0x00000001) == 0x00000001)
                IPBinInformation.AppendLine("Game uses Windows CE.");

            IPBinInformation.AppendFormat("Peripherals:").AppendLine();

            if((iPeripherals & 0x00000010) == 0x00000010)
                IPBinInformation.AppendLine("Game supports the VGA Box.");
            if((iPeripherals & 0x00000100) == 0x00000100)
                IPBinInformation.AppendLine("Game supports other expansion.");
            if((iPeripherals & 0x00000200) == 0x00000200)
                IPBinInformation.AppendLine("Game supports Puru Puru pack.");
            if((iPeripherals & 0x00000400) == 0x00000400)
                IPBinInformation.AppendLine("Game supports Mike Device.");
            if((iPeripherals & 0x00000800) == 0x00000800)
                IPBinInformation.AppendLine("Game supports Memory Card.");
            if((iPeripherals & 0x00001000) == 0x00001000)
                IPBinInformation.AppendLine("Game requires A + B + Start buttons and D-Pad.");
            if((iPeripherals & 0x00002000) == 0x00002000)
                IPBinInformation.AppendLine("Game requires C button.");
            if((iPeripherals & 0x00004000) == 0x00004000)
                IPBinInformation.AppendLine("Game requires D button.");
            if((iPeripherals & 0x00008000) == 0x00008000)
                IPBinInformation.AppendLine("Game requires X button.");
            if((iPeripherals & 0x00010000) == 0x00010000)
                IPBinInformation.AppendLine("Game requires Y button.");
            if((iPeripherals & 0x00020000) == 0x00020000)
                IPBinInformation.AppendLine("Game requires Z button.");
            if((iPeripherals & 0x00040000) == 0x00040000)
                IPBinInformation.AppendLine("Game requires expanded direction buttons.");
            if((iPeripherals & 0x00080000) == 0x00080000)
                IPBinInformation.AppendLine("Game requires analog R trigger.");
            if((iPeripherals & 0x00100000) == 0x00100000)
                IPBinInformation.AppendLine("Game requires analog L trigger.");
            if((iPeripherals & 0x00200000) == 0x00200000)
                IPBinInformation.AppendLine("Game requires analog horizontal controller.");
            if((iPeripherals & 0x00400000) == 0x00400000)
                IPBinInformation.AppendLine("Game requires analog vertical controller.");
            if((iPeripherals & 0x00800000) == 0x00800000)
                IPBinInformation.AppendLine("Game requires expanded analog horizontal controller.");
            if((iPeripherals & 0x01000000) == 0x01000000)
                IPBinInformation.AppendLine("Game requires expanded analog vertical controller.");
            if((iPeripherals & 0x02000000) == 0x02000000)
                IPBinInformation.AppendLine("Game supports Gun.");
            if((iPeripherals & 0x04000000) == 0x04000000)
                IPBinInformation.AppendLine("Game supports Keyboard.");
            if((iPeripherals & 0x08000000) == 0x08000000)
                IPBinInformation.AppendLine("Game supports Mouse.");

            if((iPeripherals & 0xF00000EE) != 0)
                IPBinInformation.AppendFormat("Game supports unknown peripherals mask {0:X2}", (iPeripherals & 0xF00000EE));

            return IPBinInformation.ToString();
        }
    }
}
