// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Helpers;

namespace Aaru.Decoders.Xbox;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "NotAccessedField.Global")]
public static class DMI
{
    public static bool IsXbox(byte[] dmi)
    {
        if(dmi?.Length != 2052)
            return false;

        // Version is 1
        if(BitConverter.ToUInt32(dmi, 4) != 1)
            return false;

        // Catalogue number is two letters, five numbers, one letter
        for(var i = 12; i < 14; i++)
        {
            if(dmi[i] < 0x41 ||
               dmi[i] > 0x5A)
                return false;
        }

        for(var i = 14; i < 19; i++)
        {
            if(dmi[i] < 0x30 ||
               dmi[i] > 0x39)
                return false;
        }

        if(dmi[19] < 0x41 ||
           dmi[19] > 0x5A)
            return false;

        var timestamp = BitConverter.ToInt64(dmi, 20);

        // Game cannot exist before the Xbox
        return timestamp >= 0x1BD164833DFC000;
    }

    public static bool IsXbox360(byte[] dmi)
    {
        if(dmi?.Length != 2052)
            return false;

        var signature = BitConverter.ToUInt32(dmi, 0x7EC);

        // "XBOX" swapped as .NET is little endian
        return signature == 0x584F4258;
    }

    public static XboxDMI? DecodeXbox(byte[] response)
    {
        bool isXbox = IsXbox(response);

        if(!isXbox)
            return null;

        var dmi = new XboxDMI
        {
            DataLength = (ushort)((response[0] << 8) + response[1]),
            Reserved1  = response[2],
            Reserved2  = response[3],
            Version    = BitConverter.ToUInt32(response, 4),
            Timestamp  = BitConverter.ToInt64(response, 20)
        };

        var tmp = new byte[8];
        Array.Copy(response, 12, tmp, 0, 8);
        dmi.CatalogNumber = StringHandlers.CToString(tmp);

        return dmi;
    }

    public static Xbox360DMI? DecodeXbox360(byte[] response)
    {
        bool isX360 = IsXbox360(response);

        if(!isX360)
            return null;

        var dmi = new Xbox360DMI
        {
            DataLength = (ushort)((response[0] << 8) + response[1]),
            Reserved1  = response[2],
            Reserved2  = response[3],
            Version    = BitConverter.ToUInt32(response, 4),
            Timestamp  = BitConverter.ToInt64(response, 20),
            MediaID    = new byte[16]
        };

        Array.Copy(response, 36, dmi.MediaID, 0, 16);
        var tmp = new byte[16];
        Array.Copy(response, 68, tmp, 0, 16);
        dmi.CatalogNumber = StringHandlers.CToString(tmp);

        return dmi.CatalogNumber == null || dmi.CatalogNumber.Length < 13 ? null : dmi;
    }

    public static string PrettifyXbox(XboxDMI? dmi)
    {
        if(dmi == null)
            return null;

        XboxDMI decoded = dmi.Value;
        var     sb      = new StringBuilder();

        sb.Append(Localization.Catalogue_number);

        for(var i = 0; i < 2; i++)
            sb.Append($"{decoded.CatalogNumber[i]}");

        sb.Append("-");

        for(var i = 2; i < 7; i++)
            sb.Append($"{decoded.CatalogNumber[i]}");

        sb.Append("-");
        sb.Append($"{decoded.CatalogNumber[7]}");
        sb.AppendLine();

        sb.AppendFormat(Localization.Timestamp_0, DateTime.FromFileTimeUtc(decoded.Timestamp)).AppendLine();

        return sb.ToString();
    }

    public static string PrettifyXbox360(Xbox360DMI? dmi)
    {
        if(dmi == null)
            return null;

        Xbox360DMI decoded = dmi.Value;
        var        sb      = new StringBuilder();

        sb.Append(Localization.Catalogue_number);

        for(var i = 0; i < 2; i++)
            sb.Append($"{decoded.CatalogNumber[i]}");

        sb.Append("-");

        for(var i = 2; i < 6; i++)
            sb.Append($"{decoded.CatalogNumber[i]}");

        sb.Append("-");

        for(var i = 6; i < 8; i++)
            sb.Append($"{decoded.CatalogNumber[i]}");

        sb.Append("-");

        switch(decoded.CatalogNumber.Length)
        {
            case 13:
                for(var i = 8; i < 10; i++)
                    sb.Append($"{decoded.CatalogNumber[i]}");

                sb.Append("-");

                for(var i = 10; i < 13; i++)
                    sb.Append($"{decoded.CatalogNumber[i]}");

                break;
            case 14:
                for(var i = 8; i < 11; i++)
                    sb.Append($"{decoded.CatalogNumber[i]}");

                sb.Append("-");

                for(var i = 11; i < 14; i++)
                    sb.Append($"{decoded.CatalogNumber[i]}");

                break;
            default:
                for(var i = 8; i < decoded.CatalogNumber.Length - 3; i++)
                    sb.Append($"{decoded.CatalogNumber[i]}");

                sb.Append("-");

                for(int i = decoded.CatalogNumber.Length - 3; i < decoded.CatalogNumber.Length; i++)
                    sb.Append($"{decoded.CatalogNumber[i]}");

                break;
        }

        sb.AppendLine();

        sb.Append(Localization.Media_ID);

        for(var i = 0; i < 12; i++)
            sb.Append($"{decoded.MediaID[i]:X2}");

        sb.Append("-");

        for(var i = 12; i < 16; i++)
            sb.Append($"{decoded.MediaID[i]:X2}");

        sb.AppendLine();

        sb.AppendFormat(Localization.Timestamp_0, DateTime.FromFileTimeUtc(decoded.Timestamp)).AppendLine();

        return sb.ToString();
    }

    public static string PrettifyXbox(byte[] response) => PrettifyXbox(DecodeXbox(response));

    public static string PrettifyXbox360(byte[] response) => PrettifyXbox360(DecodeXbox360(response));

#region Nested type: Xbox360DMI

    public struct Xbox360DMI
    {
        /// <summary>Bytes 0 to 1 Data length</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;

        /// <summary>Bytes 4 to 7 0x02 in XGD2 and XGD3</summary>
        public uint Version;

        /// <summary>Bytes 20 to 27 DMI timestamp</summary>
        public long Timestamp;

        /// <summary>Bytes 36 to 51 Media ID in hex XXXXXXXXXXXX-XXXXXXXX</summary>
        public byte[] MediaID;

        /// <summary>Bytes 68 to 83 Catalogue number in XX-XXXX-XX-XXY-XXX, Y not always exists</summary>
        public string CatalogNumber;
    }

#endregion

#region Nested type: XboxDMI

    public struct XboxDMI
    {
        /// <summary>Bytes 0 to 1 Data length</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;

        /// <summary>Bytes 4 to 7 0x01 in XGD</summary>
        public uint Version;

        /// <summary>Bytes 12 to 16 Catalogue number in XX-XXXXX-X</summary>
        public string CatalogNumber;

        /// <summary>Bytes 20 to 27 DMI timestamp</summary>
        public long Timestamp;
    }

#endregion
}