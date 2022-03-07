// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : 0F.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MODE PAGE 0Fh: Data compression page.
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

namespace Aaru.Decoders.SCSI;

using System.Diagnostics.CodeAnalysis;
using System.Text;

[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static partial class Modes
{
    #region Mode Page 0x0F: Data compression page
    /// <summary>Data compression page Page code 0x0F 16 bytes in SSC-1, SSC-2, SSC-3</summary>
    public struct ModePage_0F
    {
        /// <summary>Parameters can be saved</summary>
        public bool PS;
        /// <summary>Data compression enabled</summary>
        public bool DCE;
        /// <summary>Data compression capable</summary>
        public bool DCC;
        /// <summary>Data decompression enabled</summary>
        public bool DDE;
        /// <summary>Report exception on decompression</summary>
        public byte RED;
        /// <summary>Compression algorithm</summary>
        public uint CompressionAlgo;
        /// <summary>Decompression algorithm</summary>
        public uint DecompressionAlgo;
    }

    public static ModePage_0F? DecodeModePage_0F(byte[] pageResponse)
    {
        if((pageResponse?[0] & 0x40) == 0x40)
            return null;

        if((pageResponse?[0] & 0x3F) != 0x0F)
            return null;

        if(pageResponse[1] + 2 != pageResponse.Length)
            return null;

        if(pageResponse.Length < 16)
            return null;

        var decoded = new ModePage_0F();

        decoded.PS |= (pageResponse[0] & 0x80) == 0x80;

        decoded.DCE |= (pageResponse[2] & 0x80) == 0x80;
        decoded.DCC |= (pageResponse[2] & 0x40) == 0x40;
        decoded.DDE |= (pageResponse[3] & 0x80) == 0x80;
        decoded.RED =  (byte)((pageResponse[3] & 0x60) >> 5);

        decoded.CompressionAlgo = (uint)((pageResponse[4] << 24) + (pageResponse[5] << 16) + (pageResponse[6] << 8) +
                                         pageResponse[7]);

        decoded.DecompressionAlgo = (uint)((pageResponse[8] << 24) + (pageResponse[9] << 16) + (pageResponse[10] << 8) +
                                           pageResponse[11]);

        return decoded;
    }

    public static string PrettifyModePage_0F(byte[] pageResponse) =>
        PrettifyModePage_0F(DecodeModePage_0F(pageResponse));

    public static string PrettifyModePage_0F(ModePage_0F? modePage)
    {
        if(!modePage.HasValue)
            return null;

        ModePage_0F page = modePage.Value;
        var         sb   = new StringBuilder();

        sb.AppendLine("SCSI Data compression page:");

        if(page.PS)
            sb.AppendLine("\tParameters can be saved");

        if(page.DCC)
        {
            sb.AppendLine("\tDrive supports data compression");

            if(page.DCE)
            {
                sb.Append("\tData compression is enabled with ");

                switch(page.CompressionAlgo)
                {
                    case 3:
                        sb.AppendLine("IBM ALDC with 512 byte buffer");

                        break;
                    case 4:
                        sb.AppendLine("IBM ALDC with 1024 byte buffer");

                        break;
                    case 5:
                        sb.AppendLine("IBM ALDC with 2048 byte buffer");

                        break;
                    case 0x10:
                        sb.AppendLine("IBM IDRC");

                        break;
                    case 0x20:
                        sb.AppendLine("DCLZ");

                        break;
                    case 0xFF:
                        sb.AppendLine("an unregistered compression algorithm");

                        break;
                    default:
                        sb.AppendFormat("an unknown algorithm coded {0}", page.CompressionAlgo).AppendLine();

                        break;
                }
            }

            if(page.DDE)
            {
                sb.AppendLine("\tData decompression is enabled");

                if(page.DecompressionAlgo == 0)
                    sb.AppendLine("\tLast data read was uncompressed");
                else
                {
                    sb.Append("\tLast data read was compressed with ");

                    switch(page.CompressionAlgo)
                    {
                        case 3:
                            sb.AppendLine("IBM ALDC with 512 byte buffer");

                            break;
                        case 4:
                            sb.AppendLine("IBM ALDC with 1024 byte buffer");

                            break;
                        case 5:
                            sb.AppendLine("IBM ALDC with 2048 byte buffer");

                            break;
                        case 0x10:
                            sb.AppendLine("IBM IDRC");

                            break;
                        case 0x20:
                            sb.AppendLine("DCLZ");

                            break;
                        case 0xFF:
                            sb.AppendLine("an unregistered compression algorithm");

                            break;
                        default:
                            sb.AppendFormat("an unknown algorithm coded {0}", page.CompressionAlgo).AppendLine();

                            break;
                    }
                }
            }

            sb.AppendFormat("\tReport exception on compression is set to {0}", page.RED).AppendLine();
        }
        else
            sb.AppendLine("\tDrive does not support data compression");

        return sb.ToString();
    }
    #endregion Mode Page 0x0F: Data compression page
}