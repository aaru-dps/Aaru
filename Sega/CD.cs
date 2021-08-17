// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : CD.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes Sega CD (aka Mega CD) IP.BIN.
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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Aaru.Console;
using Marshal = Aaru.Helpers.Marshal;

namespace Aaru.Decoders.Sega
{
    /// <summary>Represents the IP.BIN from a SEGA CD / MEGA CD</summary>
    [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class CD
    {
        /// <summary>Decodes an IP.BIN sector in SEGA CD / MEGA CD format</summary>
        /// <param name="ipbin_sector">IP.BIN sector</param>
        /// <returns>Decoded IP.BIN</returns>
        public static IPBin? DecodeIPBin(byte[] ipbin_sector)
        {
            if(ipbin_sector == null)
                return null;

            if(ipbin_sector.Length < 512)
                return null;

            IPBin ipbin = Marshal.ByteArrayToStructureLittleEndian<IPBin>(ipbin_sector);

            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.volume_name = \"{0}\"",
                                       Encoding.ASCII.GetString(ipbin.volume_name));

            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.system_name = \"{0}\"",
                                       Encoding.ASCII.GetString(ipbin.system_name));

            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.volume_version = \"{0:X}\"",
                                       ipbin.volume_version);

            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.volume_type = 0x{0:X8}",
                                       ipbin.volume_type);

            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.system_version = 0x{0:X8}",
                                       ipbin.system_version);

            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.ip_address = 0x{0:X8}", ipbin.ip_address);
            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.ip_loadsize = {0}", ipbin.ip_loadsize);

            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.ip_entry_address = 0x{0:X8}",
                                       ipbin.ip_entry_address);

            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.ip_work_ram_size = {0}",
                                       ipbin.ip_work_ram_size);

            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.sp_address = 0x{0:X8}", ipbin.sp_address);
            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.sp_loadsize = {0}", ipbin.sp_loadsize);

            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.sp_entry_address = 0x{0:X8}",
                                       ipbin.sp_entry_address);

            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.sp_work_ram_size = {0}",
                                       ipbin.sp_work_ram_size);

            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.release_date = \"{0}\"",
                                       Encoding.ASCII.GetString(ipbin.release_date));

            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.release_date2 = \"{0}\"",
                                       Encoding.ASCII.GetString(ipbin.release_date2));

            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.developer_code = \"{0}\"",
                                       Encoding.ASCII.GetString(ipbin.developer_code));

            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.domestic_title = \"{0}\"",
                                       Encoding.ASCII.GetString(ipbin.domestic_title));

            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.overseas_title = \"{0}\"",
                                       Encoding.ASCII.GetString(ipbin.overseas_title));

            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.product_code = \"{0}\"",
                                       Encoding.ASCII.GetString(ipbin.product_code));

            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.peripherals = \"{0}\"",
                                       Encoding.ASCII.GetString(ipbin.peripherals));

            AaruConsole.DebugWriteLine("SegaCD IP.BIN Decoder", "segacd_ipbin.region_codes = \"{0}\"",
                                       Encoding.ASCII.GetString(ipbin.region_codes));

            string id = Encoding.ASCII.GetString(ipbin.SegaHardwareID);

            return id == "SEGADISCSYSTEM  " || id == "SEGADATADISC    " || id == "SEGAOS          " ? ipbin
                       : (IPBin?)null;
        }

        /// <summary>Pretty prints a decoded IP.BIN in SEGA CD / MEGA CD format</summary>
        /// <param name="decoded">Decoded IP.BIN</param>
        /// <returns>Description of the IP.BIN contents</returns>
        public static string Prettify(IPBin? decoded)
        {
            if(decoded == null)
                return null;

            IPBin ipbin = decoded.Value;

            var IPBinInformation = new StringBuilder();

            IPBinInformation.AppendLine("--------------------------------");
            IPBinInformation.AppendLine("SEGA IP.BIN INFORMATION:");
            IPBinInformation.AppendLine("--------------------------------");

            // Decoding all data
            DateTime    ipbindate = DateTime.MinValue;
            CultureInfo provider  = CultureInfo.InvariantCulture;

            try
            {
                ipbindate = DateTime.ParseExact(Encoding.ASCII.GetString(ipbin.release_date), "MMddyyyy", provider);
            }
            catch
            {
                try
                {
                    ipbindate = DateTime.ParseExact(Encoding.ASCII.GetString(ipbin.release_date2), "yyyy.MMM",
                                                    provider);
                }
                #pragma warning disable RECS0022 // A catch clause that catches System.Exception and has an empty body
                catch
                {
                    // ignored
                }
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
            IPBinInformation.AppendFormat("Initial program address: 0x{0:X8}", ipbin.ip_address).AppendLine();
            IPBinInformation.AppendFormat("Initial program load size: {0} bytes", ipbin.ip_loadsize).AppendLine();

            IPBinInformation.AppendFormat("Initial program entry address: 0x{0:X8}", ipbin.ip_entry_address).
                             AppendLine();

            IPBinInformation.AppendFormat("Initial program work RAM: {0} bytes", ipbin.ip_work_ram_size).AppendLine();
            IPBinInformation.AppendFormat("System program address: 0x{0:X8}", ipbin.sp_address).AppendLine();
            IPBinInformation.AppendFormat("System program load size: {0} bytes", ipbin.sp_loadsize).AppendLine();

            IPBinInformation.AppendFormat("System program entry address: 0x{0:X8}", ipbin.sp_entry_address).
                             AppendLine();

            IPBinInformation.AppendFormat("System program work RAM: {0} bytes", ipbin.sp_work_ram_size).AppendLine();

            if(ipbindate != DateTime.MinValue)
                IPBinInformation.AppendFormat("Release date: {0}", ipbindate).AppendLine();

            //IPBinInformation.AppendFormat("Release date (other format): {0}", Encoding.ASCII.GetString(release_date2)).AppendLine();
            IPBinInformation.AppendFormat("Hardware ID: {0}", Encoding.ASCII.GetString(ipbin.hardware_id)).AppendLine();

            IPBinInformation.AppendFormat("Developer code: {0}", Encoding.ASCII.GetString(ipbin.developer_code)).
                             AppendLine();

            IPBinInformation.AppendFormat("Domestic title: {0}", Encoding.ASCII.GetString(ipbin.domestic_title)).
                             AppendLine();

            IPBinInformation.AppendFormat("Overseas title: {0}", Encoding.ASCII.GetString(ipbin.overseas_title)).
                             AppendLine();

            IPBinInformation.AppendFormat("Product code: {0}", Encoding.ASCII.GetString(ipbin.product_code)).
                             AppendLine();

            IPBinInformation.AppendFormat("Peripherals:").AppendLine();

            foreach(byte peripheral in ipbin.peripherals)
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
                    case ' ': break;
                    default:
                        IPBinInformation.AppendFormat("Game supports unknown peripheral {0}.", peripheral).AppendLine();

                        break;
                }

            IPBinInformation.AppendLine("Regions supported:");

            foreach(byte region in ipbin.region_codes)
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
                    case ' ': break;
                    default:
                        IPBinInformation.AppendFormat("Game supports unknown region {0}.", region).AppendLine();

                        break;
                }

            return IPBinInformation.ToString();
        }

        // TODO: Check if it is big or little endian
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IPBin
        {
            /// <summary>Must be "SEGADISCSYSTEM  " or "SEGADATADISC    " or "SEGAOS          "</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] SegaHardwareID;
            /// <summary>0x010, Varies</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public byte[] volume_name;
            /// <summary>0x01B, 0x00</summary>
            public byte spare_space1;
            /// <summary>0x01C, Volume version in BCD. &lt;100 = Prerelease.</summary>
            public ushort volume_version;
            /// <summary>0x01E, Bit 0 = 1 => CD-ROM. Rest should be 0.</summary>
            public ushort volume_type;
            /// <summary>0x020, Unknown, varies!</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public byte[] system_name;
            /// <summary>0x02B, 0x00</summary>
            public byte spare_space2;
            /// <summary>0x02C, Should be 1</summary>
            public ushort system_version;
            /// <summary>0x02E, 0x0000</summary>
            public ushort spare_space3;
            /// <summary>0x030, Initial program address</summary>
            public uint ip_address;
            /// <summary>0x034, Load size of initial program</summary>
            public uint ip_loadsize;
            /// <summary>0x038, Initial program entry address</summary>
            public uint ip_entry_address;
            /// <summary>0x03C, Initial program work RAM size in bytes</summary>
            public uint ip_work_ram_size;
            /// <summary>0x040, System program address</summary>
            public uint sp_address;
            /// <summary>0x044, Load size of system program</summary>
            public uint sp_loadsize;
            /// <summary>0x048, System program entry address</summary>
            public uint sp_entry_address;
            /// <summary>0x04C, System program work RAM size in bytes</summary>
            public uint sp_work_ram_size;
            /// <summary>0x050, MMDDYYYY</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] release_date;
            /// <summary>0x058, Seems to be all 0x20s</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
            public byte[] unknown1;
            /// <summary>0x05F, 0x00 ?</summary>
            public byte spare_space4;
            /// <summary>0x060, System Reserved Area</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 160)]
            public byte[] system_reserved;
            /// <summary>0x100, Hardware ID</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] hardware_id;
            /// <summary>0x113 or 0x110, "SEGA" or "T-xx"</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
            public byte[] developer_code;
            /// <summary>0x118, Another release date, this with month in letters?</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] release_date2;
            /// <summary>0x120, Domestic version of the game title</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] domestic_title;
            /// <summary>0x150, Overseas version of the game title</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 48)]
            public byte[] overseas_title;
            /// <summary>0x180, Official product code</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 13)]
            public byte[] product_code;
            /// <summary>0x190, Supported peripherals, see above</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] peripherals;
            /// <summary>0x1A0, 0x20</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] spare_space6;
            /// <summary>0x1B0, Inside here should be modem information, but I need to get a modem-enabled game</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] spare_space7;
            /// <summary>0x1F0, Region codes, space-filled</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] region_codes;
        }
    }
}