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
// Copyright © 2011-2023 Natalia Portillo
// ****************************************************************************/

using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Aaru.Decoders.SCSI;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
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

        sb.AppendLine(Localization.SCSI_Data_compression_page);

        if(page.PS)
            sb.AppendLine("\t" + Localization.Parameters_can_be_saved);

        if(page.DCC)
        {
            sb.AppendLine("\t" + Localization.Drive_supports_data_compression);

            if(page.DCE)
            {
                sb.Append("\t" + Localization.Data_compression_is_enabled_with);

                switch(page.CompressionAlgo)
                {
                    case 3:
                        sb.AppendLine(Localization.IBM_ALDC_with_512_byte_buffer);

                        break;
                    case 4:
                        sb.AppendLine(Localization.IBM_ALDC_with_1024_byte_buffer);

                        break;
                    case 5:
                        sb.AppendLine(Localization.IBM_ALDC_with_2048_byte_buffer);

                        break;
                    case 0x10:
                        sb.AppendLine(Localization.IBM_IDRC);

                        break;
                    case 0x20:
                        sb.AppendLine(Localization.DCLZ);

                        break;
                    case 0xFF:
                        sb.AppendLine(Localization.an_unregistered_compression_algorithm);

                        break;
                    default:
                        sb.AppendFormat(Localization.an_unknown_algorithm_coded_0, page.CompressionAlgo).AppendLine();

                        break;
                }
            }

            if(page.DDE)
            {
                sb.AppendLine("\t" + Localization.Data_decompression_is_enabled);

                if(page.DecompressionAlgo == 0)
                    sb.AppendLine("\t" + Localization.Last_data_read_was_uncompressed);
                else
                {
                    sb.Append("\t" + Localization.Last_data_read_was_compressed_with_);

                    switch(page.CompressionAlgo)
                    {
                        case 3:
                            sb.AppendLine(Localization.IBM_ALDC_with_512_byte_buffer);

                            break;
                        case 4:
                            sb.AppendLine(Localization.IBM_ALDC_with_1024_byte_buffer);

                            break;
                        case 5:
                            sb.AppendLine(Localization.IBM_ALDC_with_2048_byte_buffer);

                            break;
                        case 0x10:
                            sb.AppendLine(Localization.IBM_IDRC);

                            break;
                        case 0x20:
                            sb.AppendLine(Localization.DCLZ);

                            break;
                        case 0xFF:
                            sb.AppendLine(Localization.an_unregistered_compression_algorithm);

                            break;
                        default:
                            sb.AppendFormat(Localization.an_unknown_algorithm_coded_0, page.CompressionAlgo).
                               AppendLine();

                            break;
                    }
                }
            }

            sb.AppendFormat("\t" + Localization.Report_exception_on_compression_is_set_to_0, page.RED).AppendLine();
        }
        else
            sb.AppendLine("\t" + Localization.Drive_does_not_support_data_compression);

        return sb.ToString();
    }

#endregion Mode Page 0x0F: Data compression page
}