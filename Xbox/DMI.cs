// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DMI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes Xbox discs DMI structure.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System;
using System.Text;

namespace DiscImageChef.Decoders.Xbox
{
    public static class DMI
    {
        public static bool IsXbox(byte[] dmi)
        {
            if(dmi == null)
                return false;
            if(dmi.Length != 2052)
                return false;

            // TODO: Need to implement it
            return false;
        }

        public static bool IsXbox360(byte[] dmi)
        {
            if(dmi == null)
                return false;
            if(dmi.Length != 2052)
                return false;

            uint signature = BitConverter.ToUInt32(dmi, 0x7EC);

            // "XBOX" swapped as .NET is little endian
            return signature == 0x584F4258;
        }

        public struct Xbox360DMI
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public ushort DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved1;
            /// <summary>
            /// Byte 3
            /// Reserved
            /// </summary>
            public byte Reserved2;

            /// <summary>
            /// Bytes 4 to 7
            /// 0x02 in XGD2 and XGD3
            /// </summary>
            public uint Version;

            /// <summary>
            /// Bytes 20 to 27
            /// DMI timestamp
            /// </summary>
            public long Timestamp;

            /// <summary>
            /// Bytes 36 to 51
            /// Media ID in hex XXXXXXXXXXXX-XXXXXXXX
            /// </summary>
            public byte[] MediaID;

            /// <summary>
            /// Bytes 68 to 83
            /// Catalogue number in XX-XXXX-XX-XXY-XXX, Y not always exists
            /// </summary>
            public string CatalogNumber;
        }

        public static Xbox360DMI? DecodeXbox360(byte[] response)
        {
            bool isX360 = IsXbox360(response);
            if(!isX360)
                return null;

            Xbox360DMI dmi = new Xbox360DMI();

            dmi.DataLength = (ushort)((response[0] << 8) + response[1]);
            dmi.Reserved1 = response[2];
            dmi.Reserved2 = response[3];

            dmi.Version = BitConverter.ToUInt32(response, 4);
            dmi.Timestamp = BitConverter.ToInt64(response, 20);
            dmi.MediaID = new byte[16];
            Array.Copy(response, 36, dmi.MediaID, 0, 16);
            byte[] tmp = new byte[16];
            Array.Copy(response, 68, tmp, 0, 16);
            dmi.CatalogNumber = StringHandlers.CToString(tmp);

            if(dmi.CatalogNumber == null || dmi.CatalogNumber.Length < 13)
                return null;

            return dmi;
        }

        public static string PrettifyXbox360(Xbox360DMI? dmi)
        {
            if(dmi == null)
                return null;

            Xbox360DMI decoded = dmi.Value;
            StringBuilder sb = new StringBuilder();

            sb.Append("Catalogue number: ");
            for(int i = 0; i < 2; i++)
                sb.AppendFormat("{0}", decoded.CatalogNumber[i]);
            sb.Append("-");
            for(int i = 2; i < 6; i++)
                sb.AppendFormat("{0}", decoded.CatalogNumber[i]);
            sb.Append("-");
            for(int i = 6; i < 8; i++)
                sb.AppendFormat("{0}", decoded.CatalogNumber[i]);
            sb.Append("-");

            if(decoded.CatalogNumber.Length == 13)
            {
                for(int i = 8; i < 10; i++)
                    sb.AppendFormat("{0}", decoded.CatalogNumber[i]);
                sb.Append("-");
                for(int i = 10; i < 13; i++)
                    sb.AppendFormat("{0}", decoded.CatalogNumber[i]);
            }
            else if(decoded.CatalogNumber.Length == 14)
            {
                for(int i = 8; i < 11; i++)
                    sb.AppendFormat("{0}", decoded.CatalogNumber[i]);
                sb.Append("-");
                for(int i = 11; i < 14; i++)
                    sb.AppendFormat("{0}", decoded.CatalogNumber[i]);
            }
            else
            {
                for(int i = 8; i < decoded.CatalogNumber.Length - 3; i++)
                    sb.AppendFormat("{0}", decoded.CatalogNumber[i]);
                sb.Append("-");
                for(int i = decoded.CatalogNumber.Length - 3; i < decoded.CatalogNumber.Length; i++)
                    sb.AppendFormat("{0}", decoded.CatalogNumber[i]);
            }
            sb.AppendLine();

            sb.Append("Media ID: ");
            for(int i = 0; i < 12; i++)
                sb.AppendFormat("{0:X2}", decoded.MediaID[i]);
            sb.Append("-");
            for(int i = 12; i < 16; i++)
                sb.AppendFormat("{0:X2}", decoded.MediaID[i]);
            sb.AppendLine();

            sb.AppendFormat("Timestamp: {0}", DateTime.FromFileTimeUtc(decoded.Timestamp)).AppendLine();

            return sb.ToString();
        }

        public static string PrettifyXbox360(byte[] response)
        {
            return PrettifyXbox360(DecodeXbox360(response));
        }
    }
}

