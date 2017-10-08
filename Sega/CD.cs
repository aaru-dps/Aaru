// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : CD.cs
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
    public static class CD
    {
        public struct IPBin
        {
            public byte[] SegaHardwareID; //16
            public byte[] volume_name; //11      // 0x010, Varies
            public byte[] spare_space1; //1      // 0x01B, 0x00
            public byte[] volume_version; //2    // 0x01C, Volume version in BCD. <100 = Prerelease.
            public byte[] volume_type; //2       // 0x01E, Bit 0 = 1 => CD-ROM. Rest should be 0.
            public byte[] system_name; //11      // 0x020, Unknown, varies!
            public byte[] spare_space2; //1      // 0x02B, 0x00
            public byte[] system_version; //2    // 0x02C, Should be 1
            public byte[] spare_space3; //2      // 0x02E, 0x0000
            public byte[] ip_address; //4        // 0x030, Initial program address
            public byte[] ip_loadsize; //4       // 0x034, Load size of initial program
            public byte[] ip_entry_address; //4  // 0x038, Initial program entry address
            public byte[] ip_work_ram_size; //4  // 0x03C, Initial program work RAM size in bytes
            public byte[] sp_address; //4        // 0x040, System program address
            public byte[] sp_loadsize; //4       // 0x044, Load size of system program
            public byte[] sp_entry_address; //4  // 0x048, System program entry address
            public byte[] sp_work_ram_size; //4  // 0x04C, System program work RAM size in bytes
            public byte[] release_date; //8      // 0x050, MMDDYYYY
            public byte[] unknown1; //7          // 0x058, Seems to be all 0x20s
            public byte[] spare_space4; //1      // 0x05F, 0x00 ?
            public byte[] system_reserved; //160 // 0x060, System Reserved Area
            public byte[] hardware_id; //16      // 0x100, Hardware ID
            public byte[] copyright; //3         // 0x110, "(C)" -- Can be the developer code directly!, if that is the code release date will be displaced
            public byte[] developer_code; //5    // 0x113 or 0x110, "SEGA" or "T-xx"
            public byte[] release_date2; //8     // 0x118, Another release date, this with month in letters?
            public byte[] domestic_title; //48   // 0x120, Domestic version of the game title
            public byte[] overseas_title; //48   // 0x150, Overseas version of the game title
            public byte[] product_code; //13     // 0x180, Official product code
            public byte[] peripherals; //16      // 0x190, Supported peripherals, see above
            public byte[] spare_space6; //16     // 0x1A0, 0x20
            public byte[] spare_space7; //64     // 0x1B0, Inside here should be modem information, but I need to get a modem-enabled game
            public byte[] region_codes; //16     // 0x1F0, Region codes, space-filled
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
            ipbin.volume_name = new byte[11];      // 0x010, Varies
            ipbin.spare_space1 = new byte[1];      // 0x01B, 0x00
            ipbin.volume_version = new byte[2];    // 0x01C, Volume version in BCD. <100 = Prerelease.
            ipbin.volume_type = new byte[2];       // 0x01E, Bit 0 = 1 => CD-ROM. Rest should be 0.
            ipbin.system_name = new byte[11];      // 0x020, Unknown, varies!
            ipbin.spare_space2 = new byte[1];      // 0x02B, 0x00
            ipbin.system_version = new byte[2];    // 0x02C, Should be 1
            ipbin.spare_space3 = new byte[2];      // 0x02E, 0x0000
            ipbin.ip_address = new byte[4];        // 0x030, Initial program address
            ipbin.ip_loadsize = new byte[4];       // 0x034, Load size of initial program
            ipbin.ip_entry_address = new byte[4];  // 0x038, Initial program entry address
            ipbin.ip_work_ram_size = new byte[4];  // 0x03C, Initial program work RAM size in bytes
            ipbin.sp_address = new byte[4];        // 0x040, System program address
            ipbin.sp_loadsize = new byte[4];       // 0x044, Load size of system program
            ipbin.sp_entry_address = new byte[4];  // 0x048, System program entry address
            ipbin.sp_work_ram_size = new byte[4];  // 0x04C, System program work RAM size in bytes
            ipbin.release_date = new byte[8];      // 0x050, MMDDYYYY
            ipbin.unknown1 = new byte[7];          // 0x058, Seems to be all 0x20s
            ipbin.spare_space4 = new byte[1];      // 0x05F, 0x00 ?
            ipbin.system_reserved = new byte[160]; // 0x060, System Reserved Area
            ipbin.hardware_id = new byte[16];      // 0x100, Hardware ID
            ipbin.copyright = new byte[3];         // 0x110, "(C)" -- Can be the developer code directly!, if that is the code release date will be displaced
            ipbin.developer_code = new byte[5];    // 0x113 or 0x110, "SEGA" or "T-xx"
            ipbin.release_date2 = new byte[8];     // 0x118, Another release date, this with month in letters?
            ipbin.domestic_title = new byte[48];   // 0x120, Domestic version of the game title
            ipbin.overseas_title = new byte[48];   // 0x150, Overseas version of the game title
            ipbin.product_code = new byte[13];     // 0x180, Official product code
            ipbin.peripherals = new byte[16];      // 0x190, Supported peripherals, see above
            ipbin.spare_space6 = new byte[16];     // 0x1A0, 0x20
            ipbin.spare_space7 = new byte[64];     // 0x1B0, Inside here should be modem information, but I need to get a modem-enabled game
            ipbin.region_codes = new byte[16];     // 0x1F0, Region codes, space-filled

            //Reading all data
            Array.Copy(ipbin_sector, 0x000, ipbin.SegaHardwareID, 0, 16);
            Array.Copy(ipbin_sector, 0x010, ipbin.volume_name, 0, 11);      // Varies
            Array.Copy(ipbin_sector, 0x01B, ipbin.spare_space1, 0, 1);         // 0x00
            Array.Copy(ipbin_sector, 0x01C, ipbin.volume_version, 0, 2);       // Volume version in BCD. <100 = Prerelease.
            Array.Copy(ipbin_sector, 0x01E, ipbin.volume_type, 0, 2);          // Bit 0 = 1 => CD-ROM. Rest should be 0.
            Array.Copy(ipbin_sector, 0x020, ipbin.system_name, 0, 11);      // Unknown, varies!
            Array.Copy(ipbin_sector, 0x02B, ipbin.spare_space2, 0, 1);         // 0x00
            Array.Copy(ipbin_sector, 0x02C, ipbin.system_version, 0, 2);       // Should be 1
            Array.Copy(ipbin_sector, 0x02E, ipbin.spare_space3, 0, 2);         // 0x0000
            Array.Copy(ipbin_sector, 0x030, ipbin.ip_address, 0, 4);        // Initial program address
            Array.Copy(ipbin_sector, 0x034, ipbin.ip_loadsize, 0, 4);          // Load size of initial program
            Array.Copy(ipbin_sector, 0x038, ipbin.ip_entry_address, 0, 4);     // Initial program entry address
            Array.Copy(ipbin_sector, 0x03C, ipbin.ip_work_ram_size, 0, 4);     // Initial program work RAM size in bytes
            Array.Copy(ipbin_sector, 0x040, ipbin.sp_address, 0, 4);        // System program address
            Array.Copy(ipbin_sector, 0x044, ipbin.sp_loadsize, 0, 4);          // Load size of system program
            Array.Copy(ipbin_sector, 0x048, ipbin.sp_entry_address, 0, 4);     // System program entry address
            Array.Copy(ipbin_sector, 0x04C, ipbin.sp_work_ram_size, 0, 4);     // System program work RAM size in bytes
            Array.Copy(ipbin_sector, 0x050, ipbin.release_date, 0, 8);      // MMDDYYYY
            Array.Copy(ipbin_sector, 0x058, ipbin.unknown1, 0, 7);          // Seems to be all 0x20s
            Array.Copy(ipbin_sector, 0x05F, ipbin.spare_space4, 0, 1);         // 0x00 ?
            Array.Copy(ipbin_sector, 0x060, ipbin.system_reserved, 0, 160); // System Reserved Area
            Array.Copy(ipbin_sector, 0x100, ipbin.hardware_id, 0, 16);      // Hardware ID
            Array.Copy(ipbin_sector, 0x110, ipbin.copyright, 0, 3);         // "(C)" -- Can be the developer code directly!, if that is the code release date will be displaced
            if(Encoding.ASCII.GetString(ipbin.copyright) == "(C)")
                Array.Copy(ipbin_sector, 0x113, ipbin.developer_code, 0, 5);    // "SEGA" or "T-xx"
            else
                Array.Copy(ipbin_sector, 0x110, ipbin.developer_code, 0, 5);    // "SEGA" or "T-xx"
            Array.Copy(ipbin_sector, 0x118, ipbin.release_date2, 0, 8);     // Another release date, this with month in letters?
            Array.Copy(ipbin_sector, 0x120, ipbin.domestic_title, 0, 48);   // Domestic version of the game title
            Array.Copy(ipbin_sector, 0x150, ipbin.overseas_title, 0, 48);   // Overseas version of the game title
                                                                      //Array.Copy(ipbin_sector, 0x000, application_type, 0, 2);  // Application type
                                                                      //Array.Copy(ipbin_sector, 0x000, space_space5, 0, 1);         // 0x20
            Array.Copy(ipbin_sector, 0x180, ipbin.product_code, 0, 13);      // Official product code
            Array.Copy(ipbin_sector, 0x190, ipbin.peripherals, 0, 16);      // Supported peripherals, see above
            Array.Copy(ipbin_sector, 0x1A0, ipbin.spare_space6, 0, 16);     // 0x20
            Array.Copy(ipbin_sector, 0x1B0, ipbin.spare_space7, 0, 64);     // Inside here should be modem information, but I need to get a modem-enabled game
            Array.Copy(ipbin_sector, 0x1F0, ipbin.region_codes, 0, 16);     // Region codes, space-filled

            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.volume_name = \"{0}\"", Encoding.ASCII.GetString(ipbin.volume_name));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.system_name = \"{0}\"", Encoding.ASCII.GetString(ipbin.system_name));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.volume_version = \"{0}\"", Encoding.ASCII.GetString(ipbin.volume_version));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.volume_type = 0x{0}", BitConverter.ToInt16(ipbin.volume_type, 0).ToString("X"));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.system_version = 0x{0}", BitConverter.ToInt16(ipbin.system_version, 0).ToString("X"));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.ip_address = 0x{0}", BitConverter.ToInt32(ipbin.ip_address, 0).ToString("X"));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.ip_loadsize = {0}", BitConverter.ToInt32(ipbin.ip_loadsize, 0));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.ip_entry_address = 0x{0}", BitConverter.ToInt32(ipbin.ip_entry_address, 0).ToString("X"));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.ip_work_ram_size = {0}", BitConverter.ToInt32(ipbin.ip_work_ram_size, 0));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.sp_address = 0x{0}", BitConverter.ToInt32(ipbin.sp_address, 0).ToString("X"));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.sp_loadsize = {0}", BitConverter.ToInt32(ipbin.sp_loadsize, 0));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.sp_entry_address = 0x{0}", BitConverter.ToInt32(ipbin.sp_entry_address, 0).ToString("X"));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.sp_work_ram_size = {0}", BitConverter.ToInt32(ipbin.sp_work_ram_size, 0));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.release_date = \"{0}\"", Encoding.ASCII.GetString(ipbin.release_date));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.release_date2 = \"{0}\"", Encoding.ASCII.GetString(ipbin.release_date2));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.developer_code = \"{0}\"", Encoding.ASCII.GetString(ipbin.developer_code));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.domestic_title = \"{0}\"", Encoding.ASCII.GetString(ipbin.domestic_title));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.overseas_title = \"{0}\"", Encoding.ASCII.GetString(ipbin.overseas_title));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.product_code = \"{0}\"", Encoding.ASCII.GetString(ipbin.product_code));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.peripherals = \"{0}\"", Encoding.ASCII.GetString(ipbin.peripherals));
            DicConsole.DebugWriteLine("ISO9660 plugin", "segacd_ipbin.region_codes = \"{0}\"", Encoding.ASCII.GetString(ipbin.region_codes));

            string id = Encoding.ASCII.GetString(ipbin.SegaHardwareID);

            if(id == "SEGADISCSYSTEM  " || id == "SEGADATADISC    " || id == "SEGAOS          ")
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
            DateTime ipbindate = DateTime.MinValue;
            CultureInfo provider = CultureInfo.InvariantCulture;
            try
            {
                ipbindate = DateTime.ParseExact(Encoding.ASCII.GetString(ipbin.release_date), "MMddyyyy", provider);
            }
            catch
            {
                try
                {
                    ipbindate = DateTime.ParseExact(Encoding.ASCII.GetString(ipbin.release_date2), "yyyy.MMM", provider);
                }
#pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch { }
#pragma warning restore RECS0022 // A catch clause that catches System.Exception and has an empty body
            }

            /*
            switch (Encoding.ASCII.GetString(application_type))
            {
                case "GM":
                    IPBinInformation.AppendLine("Disc is a game.");
                    break;
                case "AI":
                    IPBinInformation.AppendLine("Disc is an application.");
                    break;
                default:
                    IPBinInformation.AppendLine("Disc is from unknown type.");
                    break;
            }
            */

            IPBinInformation.AppendFormat("Volume name: {0}", Encoding.ASCII.GetString(ipbin.volume_name)).AppendLine();
            //IPBinInformation.AppendFormat("Volume version: {0}", Encoding.ASCII.GetString(ipbin.volume_version)).AppendLine();
            //IPBinInformation.AppendFormat("{0}", Encoding.ASCII.GetString(ipbin.volume_type)).AppendLine();
            IPBinInformation.AppendFormat("System name: {0}", Encoding.ASCII.GetString(ipbin.system_name)).AppendLine();
            //IPBinInformation.AppendFormat("System version: {0}", Encoding.ASCII.GetString(ipbin.system_version)).AppendLine();
            IPBinInformation.AppendFormat("Initial program address: 0x{0}", BitConverter.ToInt32(ipbin.ip_address, 0).ToString("X")).AppendLine();
            IPBinInformation.AppendFormat("Initial program load size: {0} bytes", BitConverter.ToInt32(ipbin.ip_loadsize, 0)).AppendLine();
            IPBinInformation.AppendFormat("Initial program entry address: 0x{0}", BitConverter.ToInt32(ipbin.ip_entry_address, 0).ToString("X")).AppendLine();
            IPBinInformation.AppendFormat("Initial program work RAM: {0} bytes", BitConverter.ToInt32(ipbin.ip_work_ram_size, 0)).AppendLine();
            IPBinInformation.AppendFormat("System program address: 0x{0}", BitConverter.ToInt32(ipbin.sp_address, 0).ToString("X")).AppendLine();
            IPBinInformation.AppendFormat("System program load size: {0} bytes", BitConverter.ToInt32(ipbin.sp_loadsize, 0)).AppendLine();
            IPBinInformation.AppendFormat("System program entry address: 0x{0}", BitConverter.ToInt32(ipbin.sp_entry_address, 0).ToString("X")).AppendLine();
            IPBinInformation.AppendFormat("System program work RAM: {0} bytes", BitConverter.ToInt32(ipbin.sp_work_ram_size, 0)).AppendLine();
            if(ipbindate != DateTime.MinValue)
                IPBinInformation.AppendFormat("Release date: {0}", ipbindate).AppendLine();
            //IPBinInformation.AppendFormat("Release date (other format): {0}", Encoding.ASCII.GetString(release_date2)).AppendLine();
            IPBinInformation.AppendFormat("Hardware ID: {0}", Encoding.ASCII.GetString(ipbin.hardware_id)).AppendLine();
            IPBinInformation.AppendFormat("Developer code: {0}", Encoding.ASCII.GetString(ipbin.developer_code)).AppendLine();
            IPBinInformation.AppendFormat("Domestic title: {0}", Encoding.ASCII.GetString(ipbin.domestic_title)).AppendLine();
            IPBinInformation.AppendFormat("Overseas title: {0}", Encoding.ASCII.GetString(ipbin.overseas_title)).AppendLine();
            IPBinInformation.AppendFormat("Product code: {0}", Encoding.ASCII.GetString(ipbin.product_code)).AppendLine();
            IPBinInformation.AppendFormat("Peripherals:").AppendLine();
            foreach(byte peripheral in ipbin.peripherals)
            {
                switch((char)peripheral)
                {
                    case 'A':
                        IPBinInformation.AppendLine("Game supports analog controller.");
                        break;
                    case 'B':
                        IPBinInformation.AppendLine("Game supports trackball.");
                        break;
                    case 'G':
                        IPBinInformation.AppendLine("Game supports light gun.");
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
                    case 'O':
                        IPBinInformation.AppendLine("Game supports Master System's JoyPad.");
                        break;
                    case 'P':
                        IPBinInformation.AppendLine("Game supports printer interface.");
                        break;
                    case 'R':
                        IPBinInformation.AppendLine("Game supports serial (RS-232C) interface.");
                        break;
                    case 'T':
                        IPBinInformation.AppendLine("Game supports tablet interface.");
                        break;
                    case 'V':
                        IPBinInformation.AppendLine("Game supports paddle controller.");
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
                        IPBinInformation.AppendLine("USA NTSC.");
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

            return IPBinInformation.ToString();
        }
    }
}
