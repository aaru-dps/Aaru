// /***************************************************************************
// The Disc Image Chef
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
// Copyright © 2011-2018 Natalia Portillo
// ****************************************************************************/

using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using DiscImageChef.Console;

namespace DiscImageChef.Decoders.Sega
{
    public static class Dreamcast
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct IPBin
        {
            /// <summary>Must be "SEGA SEGAKATANA "</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] SegaHardwareID;
            /// <summary>0x010, "SEGA ENTERPRISES"</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] maker_id;
            /// <summary>0x020, CRC of product_no and product_version</summary>
            public uint dreamcast_crc;
            /// <summary>0x024, " "</summary>
            public byte spare_space1;
            /// <summary>0x025, "GD-ROM"</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public byte[] dreamcast_media;
            /// <summary>0x02B, Disc number</summary>
            public byte disc_no;
            /// <summary>0x02C, '/'</summary>
            public byte disc_no_separator;
            /// <summary>0x02D, Total number of discs</summary>
            public byte disc_total_nos;
            /// <summary>0x02E, "  "</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)] public byte[] spare_space2;
            /// <summary>0x030, Region codes, space-filled</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] region_codes;
            /// <summary>0x038, Supported peripherals, bitwise</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)] public byte[] peripherals;
            /// <summary>0x03F, ' '</summary>
            public byte spare_space3;
            /// <summary>0x040, Product number</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public byte[] product_no;
            /// <summary>0x04A, Product version</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public byte[] product_version;
            /// <summary>0x050, YYYYMMDD</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] release_date;
            /// <summary>0x058, "  "</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)] public byte[] spare_space4;
            /// <summary>0x060, Usually "1ST_READ.BIN" or "0WINCE.BIN  "</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)] public byte[] boot_filename;
            /// <summary>0x06C, "  "</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] spare_space5;
            /// <summary>0x070, Game producer, space-filled</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)] public byte[] producer;
            /// <summary>0x080, Game name, space-filled</summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 128)] public byte[] product_name;
        }

        public static IPBin? DecodeIPBin(byte[] ipbin_sector)
        {
            if(ipbin_sector == null) return null;

            if(ipbin_sector.Length < 512) return null;

            IPBin ipbin = new IPBin();
            IntPtr ptr = Marshal.AllocHGlobal(512);
            Marshal.Copy(ipbin_sector, 0, ptr, 512);
            ipbin = (IPBin)Marshal.PtrToStructure(ptr, typeof(IPBin));
            Marshal.FreeHGlobal(ptr);

            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.maker_id = \"{0}\"",
                                      Encoding.ASCII.GetString(ipbin.maker_id));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.spare_space1 = \"{0}\"",
                                      (char)ipbin.spare_space1);
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.dreamcast_media = \"{0}\"",
                                      Encoding.ASCII.GetString(ipbin.dreamcast_media));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.disc_no = {0}", (char)ipbin.disc_no);
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.disc_no_separator = \"{0}\"",
                                      (char)ipbin.disc_no_separator);
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.disc_total_nos = \"{0}\"",
                                      (char)ipbin.disc_total_nos);
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.spare_space2 = \"{0}\"",
                                      Encoding.ASCII.GetString(ipbin.spare_space2));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.region_codes = \"{0}\"",
                                      Encoding.ASCII.GetString(ipbin.region_codes));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.peripherals = \"{0}\"",
                                      Encoding.ASCII.GetString(ipbin.peripherals));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.product_no = \"{0}\"",
                                      Encoding.ASCII.GetString(ipbin.product_no));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.product_version = \"{0}\"",
                                      Encoding.ASCII.GetString(ipbin.product_version));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.release_date = \"{0}\"",
                                      Encoding.ASCII.GetString(ipbin.release_date));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.spare_space3 = \"{0}\"",
                                      (char)ipbin.spare_space3);
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.boot_filename = \"{0}\"",
                                      Encoding.ASCII.GetString(ipbin.boot_filename));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.producer = \"{0}\"",
                                      Encoding.ASCII.GetString(ipbin.producer));
            DicConsole.DebugWriteLine("ISO9660 plugin", "dreamcast_ipbin.product_name = \"{0}\"",
                                      Encoding.ASCII.GetString(ipbin.product_name));

            return Encoding.ASCII.GetString(ipbin.SegaHardwareID) == "SEGA SEGAKATANA " ? ipbin : (IPBin?)null;
        }

        public static string Prettify(IPBin? decoded)
        {
            if(decoded == null) return null;

            IPBin ipbin = decoded.Value;

            StringBuilder IPBinInformation = new StringBuilder();

            IPBinInformation.AppendLine("--------------------------------");
            IPBinInformation.AppendLine("SEGA IP.BIN INFORMATION:");
            IPBinInformation.AppendLine("--------------------------------");

            // Decoding all data
            DateTime ipbindate;
            CultureInfo provider = CultureInfo.InvariantCulture;
            ipbindate = DateTime.ParseExact(Encoding.ASCII.GetString(ipbin.release_date), "yyyyMMdd", provider);
            IPBinInformation.AppendFormat("Product name: {0}", Encoding.ASCII.GetString(ipbin.product_name))
                            .AppendLine();
            IPBinInformation.AppendFormat("Product version: {0}", Encoding.ASCII.GetString(ipbin.product_version))
                            .AppendLine();
            IPBinInformation.AppendFormat("Product CRC: 0x{0:X8}", ipbin.dreamcast_crc).AppendLine();
            IPBinInformation.AppendFormat("Producer: {0}", Encoding.ASCII.GetString(ipbin.producer)).AppendLine();
            IPBinInformation.AppendFormat("Disc media: {0}", Encoding.ASCII.GetString(ipbin.dreamcast_media))
                            .AppendLine();
            IPBinInformation.AppendFormat("Disc number {0} of {1}", (char)ipbin.disc_no, (char)ipbin.disc_total_nos)
                            .AppendLine();
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
                    IPBinInformation.AppendFormat("Disc boots using unknown loader: {0}.",
                                                  Encoding.ASCII.GetString(ipbin.boot_filename)).AppendLine();
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
                        IPBinInformation.AppendLine("North America NTSC.");
                        break;
                    case 'E':
                        IPBinInformation.AppendLine("Europe PAL.");
                        break;
                    case ' ': break;
                    default:
                        IPBinInformation.AppendFormat("Game supports unknown region {0}.", region).AppendLine();
                        break;
                }

            int iPeripherals = int.Parse(Encoding.ASCII.GetString(ipbin.peripherals), NumberStyles.HexNumber);

            if((iPeripherals & 0x00000001) == 0x00000001) IPBinInformation.AppendLine("Game uses Windows CE.");

            IPBinInformation.AppendFormat("Peripherals:").AppendLine();

            if((iPeripherals & 0x00000010) == 0x00000010) IPBinInformation.AppendLine("Game supports the VGA Box.");
            if((iPeripherals & 0x00000100) == 0x00000100) IPBinInformation.AppendLine("Game supports other expansion.");
            if((iPeripherals & 0x00000200) == 0x00000200) IPBinInformation.AppendLine("Game supports Puru Puru pack.");
            if((iPeripherals & 0x00000400) == 0x00000400) IPBinInformation.AppendLine("Game supports Mike Device.");
            if((iPeripherals & 0x00000800) == 0x00000800) IPBinInformation.AppendLine("Game supports Memory Card.");
            if((iPeripherals & 0x00001000) == 0x00001000)
                IPBinInformation.AppendLine("Game requires A + B + Start buttons and D-Pad.");
            if((iPeripherals & 0x00002000) == 0x00002000) IPBinInformation.AppendLine("Game requires C button.");
            if((iPeripherals & 0x00004000) == 0x00004000) IPBinInformation.AppendLine("Game requires D button.");
            if((iPeripherals & 0x00008000) == 0x00008000) IPBinInformation.AppendLine("Game requires X button.");
            if((iPeripherals & 0x00010000) == 0x00010000) IPBinInformation.AppendLine("Game requires Y button.");
            if((iPeripherals & 0x00020000) == 0x00020000) IPBinInformation.AppendLine("Game requires Z button.");
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
            if((iPeripherals & 0x02000000) == 0x02000000) IPBinInformation.AppendLine("Game supports Gun.");
            if((iPeripherals & 0x04000000) == 0x04000000) IPBinInformation.AppendLine("Game supports Keyboard.");
            if((iPeripherals & 0x08000000) == 0x08000000) IPBinInformation.AppendLine("Game supports Mouse.");

            if((iPeripherals & 0xEE) != 0)
                IPBinInformation.AppendFormat("Game supports unknown peripherals mask {0:X2}", iPeripherals & 0xEE);

            return IPBinInformation.ToString();
        }
    }
}