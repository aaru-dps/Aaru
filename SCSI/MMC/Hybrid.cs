// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Hybrid.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes SCSI MMC hybrid structures.
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

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Helpers;

namespace Aaru.Decoders.SCSI.MMC
{
    // Information from the following standards:
    // ANSI X3.304-1997
    // T10/1048-D revision 9.0
    // T10/1048-D revision 10a
    // T10/1228-D revision 7.0c
    // T10/1228-D revision 11a
    // T10/1363-D revision 10g
    // T10/1545-D revision 1d
    // T10/1545-D revision 5
    // T10/1545-D revision 5a
    // T10/1675-D revision 2c
    // T10/1675-D revision 4
    // T10/1836-D revision 2g
    [SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
     SuppressMessage("ReSharper", "MemberCanBePrivate.Global"), SuppressMessage("ReSharper", "NotAccessedField.Global")]
    public static class Hybrid
    {
        public static RecognizedFormatLayers? DecodeFormatLayers(byte[] FormatLayersResponse)
        {
            if(FormatLayersResponse == null)
                return null;

            if(FormatLayersResponse.Length < 8)
                return null;

            var decoded = new RecognizedFormatLayers
            {
                DataLength         = BigEndianBitConverter.ToUInt16(FormatLayersResponse, 0),
                Reserved1          = FormatLayersResponse[2],
                Reserved2          = FormatLayersResponse[3],
                NumberOfLayers     = FormatLayersResponse[4],
                Reserved3          = (byte)((FormatLayersResponse[5] & 0xC0) >> 6),
                DefaultFormatLayer = (byte)((FormatLayersResponse[5] & 0x30) >> 4),
                Reserved4          = (byte)((FormatLayersResponse[5] & 0x0C) >> 2),
                OnlineFormatLayer  = (byte)(FormatLayersResponse[5] & 0x03),
                FormatLayers       = new ushort[(FormatLayersResponse.Length - 6) / 2]
            };

            for(int i = 0; i < (FormatLayersResponse.Length - 6) / 2; i++)
                decoded.FormatLayers[i] = BigEndianBitConverter.ToUInt16(FormatLayersResponse, (i * 2) + 6);

            return decoded;
        }

        public static string PrettifyFormatLayers(RecognizedFormatLayers? FormatLayersResponse)
        {
            if(FormatLayersResponse == null)
                return null;

            RecognizedFormatLayers response = FormatLayersResponse.Value;

            var sb = new StringBuilder();

            sb.AppendFormat("{0} format layers recognized", response.NumberOfLayers);

            for(int i = 0; i < response.FormatLayers.Length; i++)
                switch(response.FormatLayers[i])
                {
                    case (ushort)FormatLayerTypeCodes.BDLayer:
                    {
                        sb.AppendFormat("Layer {0} is of type Blu-ray", i).AppendLine();

                        if(response.DefaultFormatLayer == i)
                            sb.AppendLine("This is the default layer.");

                        if(response.OnlineFormatLayer == i)
                            sb.AppendLine("This is the layer actually in use.");

                        break;
                    }

                    case (ushort)FormatLayerTypeCodes.CDLayer:
                    {
                        sb.AppendFormat("Layer {0} is of type CD", i).AppendLine();

                        if(response.DefaultFormatLayer == i)
                            sb.AppendLine("This is the default layer.");

                        if(response.OnlineFormatLayer == i)
                            sb.AppendLine("This is the layer actually in use.");

                        break;
                    }

                    case (ushort)FormatLayerTypeCodes.DVDLayer:
                    {
                        sb.AppendFormat("Layer {0} is of type DVD", i).AppendLine();

                        if(response.DefaultFormatLayer == i)
                            sb.AppendLine("This is the default layer.");

                        if(response.OnlineFormatLayer == i)
                            sb.AppendLine("This is the layer actually in use.");

                        break;
                    }

                    case (ushort)FormatLayerTypeCodes.HDDVDLayer:
                    {
                        sb.AppendFormat("Layer {0} is of type HD DVD", i).AppendLine();

                        if(response.DefaultFormatLayer == i)
                            sb.AppendLine("This is the default layer.");

                        if(response.OnlineFormatLayer == i)
                            sb.AppendLine("This is the layer actually in use.");

                        break;
                    }

                    default:
                    {
                        sb.AppendFormat("Layer {0} is of unknown type 0x{1:X4}", i, response.FormatLayers[i]).
                           AppendLine();

                        if(response.DefaultFormatLayer == i)
                            sb.AppendLine("This is the default layer.");

                        if(response.OnlineFormatLayer == i)
                            sb.AppendLine("This is the layer actually in use.");

                        break;
                    }
                }

            return sb.ToString();
        }

        public static string PrettifyFormatLayers(byte[] FormatLayersResponse)
        {
            RecognizedFormatLayers? decoded = DecodeFormatLayers(FormatLayersResponse);

            return PrettifyFormatLayers(decoded);
        }

        public struct RecognizedFormatLayers
        {
            /// <summary>Bytes 0 to 1 Data Length</summary>
            public ushort DataLength;
            /// <summary>Byte 2 Reserved</summary>
            public byte Reserved1;
            /// <summary>Byte 3 Reserved</summary>
            public byte Reserved2;
            /// <summary>Byte 4 Number of format layers in hybrid disc identified by drive</summary>
            public byte NumberOfLayers;
            /// <summary>Byte 5, bits 7 to 6 Reserved</summary>
            public byte Reserved3;
            /// <summary>Byte 5, bits 5 to 4 Layer no. used when disc is inserted</summary>
            public byte DefaultFormatLayer;
            /// <summary>Byte 5, bits 3 to 2 Reserved</summary>
            public byte Reserved4;
            /// <summary>Byte 5, bits 1 to 0 Layer no. currently in use</summary>
            public byte OnlineFormatLayer;
            /// <summary>Bytes 6 to end Recognized format layers</summary>
            public ushort[] FormatLayers;
        }
    }
}