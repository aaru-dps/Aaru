/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : MMC.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Decoders.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Decodes common structures to DVD, HD DVD and BD.
 
--[ License ] --------------------------------------------------------------
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as
    published by the Free Software Foundation, either version 3 of the
    License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.

----------------------------------------------------------------------------
Copyright (C) 2011-2014 Claunia.com
****************************************************************************/
//$Id$
using System;
using System.Text;

namespace DiscImageChef.Decoders
{
    /// <summary>
    /// Information from the following standards:
    /// ANSI X3.304-1997
    /// T10/1048-D revision 9.0
    /// T10/1048-D revision 10a
    /// T10/1228-D revision 7.0c
    /// T10/1228-D revision 11a
    /// T10/1363-D revision 10g
    /// T10/1545-D revision 1d
    /// T10/1545-D revision 5
    /// T10/1545-D revision 5a
    /// T10/1675-D revision 2c
    /// T10/1675-D revision 4
    /// T10/1836-D revision 2g
    /// </summary>
    public static class MMC
    {
        #region Public enumerations
        public enum FormatLayerTypeCodes : ushort
        {
            CDLayer = 0x0008,
            DVDLayer = 0x0010,
            BDLayer = 0x0040,
            HDDVDLayer = 0x0050
        }
        #endregion

        #region Public methods
        public static AACSVolumeIdentifier? DecodeAACSVolumeIdentifier(byte[] AACSVIResponse)
        {
            if (AACSVIResponse == null)
                return null;

            AACSVolumeIdentifier decoded = new AACSVolumeIdentifier();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.VolumeIdentifier = new byte[AACSVIResponse.Length - 4];

            decoded.DataLength = BigEndianBitConverter.ToUInt16(AACSVIResponse, 0);
            decoded.Reserved1 = AACSVIResponse[2];
            decoded.Reserved2 = AACSVIResponse[3];
            Array.Copy(AACSVIResponse, 4, decoded.VolumeIdentifier, 0, AACSVIResponse.Length - 4);

            return decoded;
        }

        public static string PrettifyAACSVolumeIdentifier(AACSVolumeIdentifier? AACSVIResponse)
        {
            if (AACSVIResponse == null)
                return null;

            AACSVolumeIdentifier response = AACSVIResponse.Value;

            StringBuilder sb = new StringBuilder();

            if (MainClass.isDebug)
            {
                sb.AppendFormat("DEBUG (AACS Volume Identifier): Reserved1 = 0x{0:X2}", response.Reserved1).AppendLine();
                sb.AppendFormat("DEBUG (AACS Volume Identifier): Reserved2 = 0x{0:X2}", response.Reserved2).AppendLine();
            }
            sb.AppendFormat("AACS Volume Identifier in hex follows:");
            sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.VolumeIdentifier, 80));

            return sb.ToString();
        }

        public static string PrettifyAACSVolumeIdentifier(byte[] AACSVIResponse)
        {
            AACSVolumeIdentifier? decoded = DecodeAACSVolumeIdentifier(AACSVIResponse);
            return PrettifyAACSVolumeIdentifier(decoded);
        }

        public static AACSMediaSerialNumber? DecodeAACSMediaSerialNumber(byte[] AACSMSNResponse)
        {
            if (AACSMSNResponse == null)
                return null;

            AACSMediaSerialNumber decoded = new AACSMediaSerialNumber();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.MediaSerialNumber = new byte[AACSMSNResponse.Length - 4];

            decoded.DataLength = BigEndianBitConverter.ToUInt16(AACSMSNResponse, 0);
            decoded.Reserved1 = AACSMSNResponse[2];
            decoded.Reserved2 = AACSMSNResponse[3];
            Array.Copy(AACSMSNResponse, 4, decoded.MediaSerialNumber, 0, AACSMSNResponse.Length - 4);

            return decoded;
        }

        public static string PrettifyAACSMediaSerialNumber(AACSMediaSerialNumber? AACSMSNResponse)
        {
            if (AACSMSNResponse == null)
                return null;

            AACSMediaSerialNumber response = AACSMSNResponse.Value;

            StringBuilder sb = new StringBuilder();

            if (MainClass.isDebug)
            {
                sb.AppendFormat("DEBUG (AACS Media Serial Number): Reserved1 = 0x{0:X2}", response.Reserved1).AppendLine();
                sb.AppendFormat("DEBUG (AACS Media Serial Number): Reserved2 = 0x{0:X2}", response.Reserved2).AppendLine();
            }
            sb.AppendFormat("AACS Media Serial Number in hex follows:");
            sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.MediaSerialNumber, 80));

            return sb.ToString();
        }

        public static string PrettifyAACSMediaSerialNumber(byte[] AACSMSNResponse)
        {
            AACSMediaSerialNumber? decoded = DecodeAACSMediaSerialNumber(AACSMSNResponse);
            return PrettifyAACSMediaSerialNumber(decoded);
        }

        public static AACSMediaIdentifier? DecodeAACSMediaIdentifier(byte[] AACSMIResponse)
        {
            if (AACSMIResponse == null)
                return null;

            AACSMediaIdentifier decoded = new AACSMediaIdentifier();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.MediaIdentifier = new byte[AACSMIResponse.Length - 4];

            decoded.DataLength = BigEndianBitConverter.ToUInt16(AACSMIResponse, 0);
            decoded.Reserved1 = AACSMIResponse[2];
            decoded.Reserved2 = AACSMIResponse[3];
            Array.Copy(AACSMIResponse, 4, decoded.MediaIdentifier, 0, AACSMIResponse.Length - 4);

            return decoded;
        }

        public static string PrettifyAACSMediaIdentifier(AACSMediaIdentifier? AACSMIResponse)
        {
            if (AACSMIResponse == null)
                return null;

            AACSMediaIdentifier response = AACSMIResponse.Value;

            StringBuilder sb = new StringBuilder();

            if (MainClass.isDebug)
            {
                sb.AppendFormat("DEBUG (AACS Media Identifier): Reserved1 = 0x{0:X2}", response.Reserved1).AppendLine();
                sb.AppendFormat("DEBUG (AACS Media Identifier): Reserved2 = 0x{0:X2}", response.Reserved2).AppendLine();
            }
            sb.AppendFormat("AACS Media Identifier in hex follows:");
            sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.MediaIdentifier, 80));

            return sb.ToString();
        }

        public static string PrettifyAACSMediaIdentifier(byte[] AACSMIResponse)
        {
            AACSMediaIdentifier? decoded = DecodeAACSMediaIdentifier(AACSMIResponse);
            return PrettifyAACSMediaIdentifier(decoded);
        }

        public static AACSMediaKeyBlock? DecodeAACSMediaKeyBlock(byte[] AACSMKBResponse)
        {
            if (AACSMKBResponse == null)
                return null;

            AACSMediaKeyBlock decoded = new AACSMediaKeyBlock();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.MediaKeyBlockPacks = new byte[AACSMKBResponse.Length - 4];

            decoded.DataLength = BigEndianBitConverter.ToUInt16(AACSMKBResponse, 0);
            decoded.Reserved = AACSMKBResponse[2];
            decoded.TotalPacks = AACSMKBResponse[3];
            Array.Copy(AACSMKBResponse, 4, decoded.MediaKeyBlockPacks, 0, AACSMKBResponse.Length - 4);

            return decoded;
        }

        public static string PrettifyAACSMediaKeyBlock(AACSMediaKeyBlock? AACSMKBResponse)
        {
            if (AACSMKBResponse == null)
                return null;

            AACSMediaKeyBlock response = AACSMKBResponse.Value;

            StringBuilder sb = new StringBuilder();

            if (MainClass.isDebug)
            {
                sb.AppendFormat("DEBUG (AACS Media Key Block): Reserved = 0x{0:X2}", response.Reserved).AppendLine();
            }
            sb.AppendFormat("Total number of media key blocks available to transfer {0}", response.TotalPacks).AppendLine();
            sb.AppendFormat("AACS Media Key Blocks in hex follows:");
            sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.MediaKeyBlockPacks, 80));

            return sb.ToString();
        }

        public static string PrettifyAACSMediaKeyBlock(byte[] AACSMKBResponse)
        {
            AACSMediaKeyBlock? decoded = DecodeAACSMediaKeyBlock(AACSMKBResponse);
            return PrettifyAACSMediaKeyBlock(decoded);
        }

        public static AACSDataKeys? DecodeAACSDataKeys(byte[] AACSDKResponse)
        {
            if (AACSDKResponse == null)
                return null;

            AACSDataKeys decoded = new AACSDataKeys();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.DataKeys = new byte[AACSDKResponse.Length - 4];

            decoded.DataLength = BigEndianBitConverter.ToUInt16(AACSDKResponse, 0);
            decoded.Reserved1 = AACSDKResponse[2];
            decoded.Reserved2 = AACSDKResponse[3];
            Array.Copy(AACSDKResponse, 4, decoded.DataKeys, 0, AACSDKResponse.Length - 4);

            return decoded;
        }

        public static string PrettifyAACSDataKeys(AACSDataKeys? AACSDKResponse)
        {
            if (AACSDKResponse == null)
                return null;

            AACSDataKeys response = AACSDKResponse.Value;

            StringBuilder sb = new StringBuilder();

            if (MainClass.isDebug)
            {
                sb.AppendFormat("DEBUG (AACS Data Keys): Reserved1 = 0x{0:X2}", response.Reserved1).AppendLine();
                sb.AppendFormat("DEBUG (AACS Data Keys): Reserved2 = 0x{0:X2}", response.Reserved2).AppendLine();
            }
            sb.AppendFormat("AACS Data Keys in hex follows:");
            sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.DataKeys, 80));

            return sb.ToString();
        }

        public static string PrettifyAACSDataKeys(byte[] AACSDKResponse)
        {
            AACSDataKeys? decoded = DecodeAACSDataKeys(AACSDKResponse);
            return PrettifyAACSDataKeys(decoded);
        }

        public static AACSLBAExtentsResponse? DecodeAACSLBAExtents(byte[] AACSLBAExtsResponse)
        {
            if (AACSLBAExtsResponse == null)
                return null;

            AACSLBAExtentsResponse decoded = new AACSLBAExtentsResponse();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.DataLength = BigEndianBitConverter.ToUInt16(AACSLBAExtsResponse, 0);
            decoded.Reserved = AACSLBAExtsResponse[2];
            decoded.MaxLBAExtents = AACSLBAExtsResponse[3];

            if ((AACSLBAExtsResponse.Length - 4) % 16 != 0)
                return decoded;

            decoded.Extents = new AACSLBAExtent[(AACSLBAExtsResponse.Length - 4) / 16];

            for (int i = 0; i < (AACSLBAExtsResponse.Length - 4) / 16; i++)
            {
                decoded.Extents[i].Reserved = new byte[8];
                Array.Copy(AACSLBAExtsResponse, 0 + i * 16 + 4, decoded.Extents[i].Reserved, 0, 8);
                decoded.Extents[i].StartLBA = BigEndianBitConverter.ToUInt32(AACSLBAExtsResponse, 8 + i * 16 + 4);
                decoded.Extents[i].LBACount = BigEndianBitConverter.ToUInt32(AACSLBAExtsResponse, 12 + i * 16 + 4);
            }

            return decoded;
        }

        public static string PrettifyAACSLBAExtents(AACSLBAExtentsResponse? AACSLBAExtsResponse)
        {
            if (AACSLBAExtsResponse == null)
                return null;

            AACSLBAExtentsResponse response = AACSLBAExtsResponse.Value;

            StringBuilder sb = new StringBuilder();

            if (response.MaxLBAExtents == 0)
            {
                if (response.DataLength > 2)
                    sb.AppendLine("Drive can store 256 LBA Extents");
                else
                    sb.AppendLine("Drive cannot store LBA Extents");
            }
            else
                sb.AppendFormat("Drive can store {0} LBA Extents", response.MaxLBAExtents).AppendLine();

            for (int i = 0; i < response.Extents.Length; i++)
                sb.AppendFormat("LBA Extent {0} starts at LBA {1} and goes for {2} sectors", i, response.Extents[i].StartLBA, response.Extents[i].LBACount);

            return sb.ToString();
        }

        public static string PrettifyAACSLBAExtents(byte[] AACSLBAExtsResponse)
        {
            AACSLBAExtentsResponse? decoded = DecodeAACSLBAExtents(AACSLBAExtsResponse);
            return PrettifyAACSLBAExtents(decoded);
        }

        public static CPRMMediaKeyBlock? DecodeCPRMMediaKeyBlock(byte[] CPRMMKBResponse)
        {
            if (CPRMMKBResponse == null)
                return null;

            CPRMMediaKeyBlock decoded = new CPRMMediaKeyBlock();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.MKBPackData = new byte[CPRMMKBResponse.Length - 4];

            decoded.DataLength = BigEndianBitConverter.ToUInt16(CPRMMKBResponse, 0);
            decoded.Reserved = CPRMMKBResponse[2];
            decoded.TotalPacks = CPRMMKBResponse[3];
            Array.Copy(CPRMMKBResponse, 4, decoded.MKBPackData, 0, CPRMMKBResponse.Length - 4);

            return decoded;
        }

        public static string PrettifyCPRMMediaKeyBlock(CPRMMediaKeyBlock? CPRMMKBResponse)
        {
            if (CPRMMKBResponse == null)
                return null;

            CPRMMediaKeyBlock response = CPRMMKBResponse.Value;

            StringBuilder sb = new StringBuilder();

            if (MainClass.isDebug)
            {
                sb.AppendFormat("DEBUG (CPRM Media Key Block): Reserved1 = 0x{0:X2}", response.Reserved).AppendLine();
            }
            sb.AppendFormat("Total number of CPRM Media Key Blocks available to transfer: {0}", response.TotalPacks).AppendLine();
            sb.AppendFormat("CPRM Media Key Blocks in hex follows:");
            sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.MKBPackData, 80));

            return sb.ToString();
        }

        public static string PrettifyCPRMMediaKeyBlock(byte[] CPRMMKBResponse)
        {
            CPRMMediaKeyBlock? decoded = DecodeCPRMMediaKeyBlock(CPRMMKBResponse);
            return PrettifyCPRMMediaKeyBlock(decoded);
        }

        public static RecognizedFormatLayers? DecodeFormatLayers(byte[] FormatLayersResponse)
        {
            if (FormatLayersResponse == null)
                return null;

            if (FormatLayersResponse.Length < 8)
                return null;

            RecognizedFormatLayers decoded = new RecognizedFormatLayers();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.DataLength = BigEndianBitConverter.ToUInt16(FormatLayersResponse, 0);
            decoded.Reserved1 = FormatLayersResponse[2];
            decoded.Reserved2 = FormatLayersResponse[3];
            decoded.NumberOfLayers = FormatLayersResponse[4];
            decoded.Reserved3 = (byte)((FormatLayersResponse[5] & 0xC0) >> 6);
            decoded.DefaultFormatLayer = (byte)((FormatLayersResponse[5] & 0x30) >> 4);
            decoded.Reserved4 = (byte)((FormatLayersResponse[5] & 0x0C) >> 2);
            decoded.OnlineFormatLayer = (byte)(FormatLayersResponse[5] & 0x03);

            decoded.FormatLayers = new UInt16[(FormatLayersResponse.Length - 6) / 2];

            for (int i = 0; i < (FormatLayersResponse.Length - 6) / 2; i++)
            {
                decoded.FormatLayers[i] = BigEndianBitConverter.ToUInt16(FormatLayersResponse, i * 2 + 6);
            }

            return decoded;
        }

        public static string PrettifyFormatLayers(RecognizedFormatLayers? FormatLayersResponse)
        {
                if (FormatLayersResponse == null)
                return null;

            RecognizedFormatLayers response = FormatLayersResponse.Value;

            StringBuilder sb = new StringBuilder();

            sb.AppendFormat("{0} format layers recognized", response.NumberOfLayers);

            for (int i = 0; i < response.FormatLayers.Length; i++)
            {
                switch (response.FormatLayers[i])
                {
                    case (UInt16)FormatLayerTypeCodes.BDLayer:
                        {
                            sb.AppendFormat("Layer {0} is of type Blu-ray", i).AppendLine();
                            if (response.DefaultFormatLayer == i)
                                sb.AppendLine("This is the default layer.");
                            if (response.OnlineFormatLayer == i)
                                sb.AppendLine("This is the layer actually in use.");
                            break;
                        }
                    case (UInt16)FormatLayerTypeCodes.CDLayer:
                        {
                            sb.AppendFormat("Layer {0} is of type CD", i).AppendLine();
                            if (response.DefaultFormatLayer == i)
                                sb.AppendLine("This is the default layer.");
                            if (response.OnlineFormatLayer == i)
                                sb.AppendLine("This is the layer actually in use.");
                            break;
                        }
                    case (UInt16)FormatLayerTypeCodes.DVDLayer:
                        {
                            sb.AppendFormat("Layer {0} is of type DVD", i).AppendLine();
                            if (response.DefaultFormatLayer == i)
                                sb.AppendLine("This is the default layer.");
                            if (response.OnlineFormatLayer == i)
                                sb.AppendLine("This is the layer actually in use.");
                            break;
                        }
                    case (UInt16)FormatLayerTypeCodes.HDDVDLayer:
                        {
                            sb.AppendFormat("Layer {0} is of type HD DVD", i).AppendLine();
                            if (response.DefaultFormatLayer == i)
                                sb.AppendLine("This is the default layer.");
                            if (response.OnlineFormatLayer == i)
                                sb.AppendLine("This is the layer actually in use.");
                            break;
                        }
                    default:
                        {
                            sb.AppendFormat("Layer {0} is of unknown type 0x{1:X4}", i, response.FormatLayers[i]).AppendLine();
                            if (response.DefaultFormatLayer == i)
                                sb.AppendLine("This is the default layer.");
                            if (response.OnlineFormatLayer == i)
                                sb.AppendLine("This is the layer actually in use.");
                            break;
                        }
                }
            }

            return sb.ToString();
        }

        public static string PrettifyFormatLayers(byte[] FormatLayersResponse)
        {
            RecognizedFormatLayers? decoded = DecodeFormatLayers(FormatLayersResponse);
            return PrettifyFormatLayers(decoded);
        }

        public static WriteProtectionStatus? DecodeWriteProtectionStatus(byte[] WPSResponse)
        {
            if (WPSResponse == null)
                return null;

            WriteProtectionStatus decoded = new WriteProtectionStatus();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.DataLength = BigEndianBitConverter.ToUInt16(WPSResponse, 0);
            decoded.Reserved1 = WPSResponse[2];
            decoded.Reserved2 = WPSResponse[3];
            decoded.Reserved3 = (byte)((WPSResponse[4] & 0xF0) >> 4);
            decoded.MSWI = Convert.ToBoolean(WPSResponse[4] & 0x08);
            decoded.CWP = Convert.ToBoolean(WPSResponse[4] & 0x04);
            decoded.PWP = Convert.ToBoolean(WPSResponse[4] & 0x02);
            decoded.SWPP = Convert.ToBoolean(WPSResponse[4] & 0x01);
            decoded.Reserved4 = WPSResponse[5];
            decoded.Reserved5 = WPSResponse[6];
            decoded.Reserved6 = WPSResponse[7];

            return decoded;
        }

        public static string PrettifyWriteProtectionStatus(WriteProtectionStatus? WPSResponse)
        {
            if (WPSResponse == null)
                return null;

            WriteProtectionStatus response = WPSResponse.Value;

            StringBuilder sb = new StringBuilder();

            if (response.MSWI)
                sb.AppendLine("Writing inhibited by media specific reason");
            if (response.CWP)
                sb.AppendLine("Cartridge sets write protection");
            if (response.PWP)
                sb.AppendLine("Media surface sets write protection");
            if (response.SWPP)
                sb.AppendLine("Software write protection is set until power down");

            if (MainClass.isDebug)
            {
                sb.AppendFormat("DEBUG (Write Protection Status): Reserved1 = 0x{0:X2}", response.Reserved1).AppendLine();
                sb.AppendFormat("DEBUG (Write Protection Status): Reserved2 = 0x{0:X2}", response.Reserved2).AppendLine();
                sb.AppendFormat("DEBUG (Write Protection Status): Reserved3 = 0x{0:X2}", response.Reserved3).AppendLine();
                sb.AppendFormat("DEBUG (Write Protection Status): Reserved4 = 0x{0:X2}", response.Reserved4).AppendLine();
                sb.AppendFormat("DEBUG (Write Protection Status): Reserved5 = 0x{0:X2}", response.Reserved5).AppendLine();
                sb.AppendFormat("DEBUG (Write Protection Status): Reserved6 = 0x{0:X2}", response.Reserved6).AppendLine();
            }

            return sb.ToString();
        }

        public static string PrettifyWriteProtectionStatus(byte[] WPSResponse)
        {
            WriteProtectionStatus? decoded = DecodeWriteProtectionStatus(WPSResponse);
            return PrettifyWriteProtectionStatus(decoded);
        }
        #endregion Public methods

        #region Public structures
        public struct AACSVolumeIdentifier
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
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
            /// Bytes 4 to end
            /// AACS volume identifier data
            /// </summary>
            public byte[] VolumeIdentifier;
        }

        public struct AACSMediaSerialNumber
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
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
            /// Bytes 4 to end
            /// AACS media serial number
            /// </summary>
            public byte[] MediaSerialNumber;
        }

        public struct AACSMediaIdentifier
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
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
            /// Bytes 4 to end
            /// AACS media identifier data
            /// </summary>
            public byte[] MediaIdentifier;
        }

        public struct AACSMediaKeyBlock
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved;
            /// <summary>
            /// Byte 3
            /// Number of MKB packs available to transfer
            /// </summary>
            public byte TotalPacks;
            /// <summary>
            /// Bytes 4 to end
            /// AACS media key block packs
            /// </summary>
            public byte[] MediaKeyBlockPacks;
        }

        public struct AACSDataKeys
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data length
            /// </summary>
            public UInt16 DataLength;
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
            /// Bytes 4 to end
            /// AACS data keys
            /// </summary>
            public byte[] DataKeys;
        }

        public struct AACSLBAExtentsResponse
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data Length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved;
            /// <summary>
            /// Byte 3
            /// Number of LBA extents the drive can store.
            /// if(MaxLBAExtents == 0 && DataLength > 2), 256 extents can be stored
            /// </summary>
            public byte MaxLBAExtents;
            /// <summary>
            /// Bytes 4 to end
            /// LBA Extents
            /// </summary>
            public AACSLBAExtent[] Extents;
        }

        public struct AACSLBAExtent
        {
            /// <summary>
            /// Bytes 0 to 7
            /// Reserved
            /// </summary>
            public byte[] Reserved;
            /// <summary>
            /// Bytes 8 to 11
            /// Start LBA of extent
            /// </summary>
            public UInt32 StartLBA;
            /// <summary>
            /// Bytes 12 to 15
            /// Extent length
            /// </summary>
            public UInt32 LBACount;
        }

        public struct CPRMMediaKeyBlock
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data Length
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2
            /// Reserved
            /// </summary>
            public byte Reserved;
            /// <summary>
            /// Byte 3
            /// Number of MKB packs available to transfer
            /// </summary>
            public byte TotalPacks;
            /// <summary>
            /// Byte 4
            /// MKB Packs
            /// </summary>
            public byte[] MKBPackData;
        }

        public struct RecognizedFormatLayers
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data Length
            /// </summary>
            public UInt16 DataLength;
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
            /// Byte 4
            /// Number of format layers in hybrid disc identified by drive
            /// </summary>
            public byte NumberOfLayers;
            /// <summary>
            /// Byte 5, bits 7 to 6
            /// Reserved
            /// </summary>
            public byte Reserved3;
            /// <summary>
            /// Byte 5, bits 5 to 4
            /// Layer no. used when disc is inserted
            /// </summary>
            public byte DefaultFormatLayer;
            /// <summary>
            /// Byte 5, bits 3 to 2
            /// Reserved
            /// </summary>
            public byte Reserved4;
            /// <summary>
            /// Byte 5, bits 1 to 0
            /// Layer no. currently in use
            /// </summary>
            public byte OnlineFormatLayer;
            /// <summary>
            /// Bytes 6 to end
            /// Recognized format layers
            /// </summary>
            public UInt16[] FormatLayers;
        }

        public struct WriteProtectionStatus
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Data Length
            /// </summary>
            public UInt16 DataLength;
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
            /// Byte 4, bits 7 to 4
            /// Reserved
            /// </summary>
            public byte Reserved3;
            /// <summary>
            /// Byte 4, bit 3
            /// Writing inhibited by media specific reason
            /// </summary>
            public bool MSWI;
            /// <summary>
            /// Byte 4, bit 2
            /// Cartridge sets write protection
            /// </summary>
            public bool CWP;
            /// <summary>
            /// Byte 4, bit 1
            /// Media surface sets write protection
            /// </summary>
            public bool PWP;
            /// <summary>
            /// Byte 4, bit 0
            /// Software write protection until power down
            /// </summary>
            public bool SWPP;
            /// <summary>
            /// Byte 5
            /// Reserved
            /// </summary>
            public byte Reserved4;
            /// <summary>
            /// Byte 6
            /// Reserved
            /// </summary>
            public byte Reserved5;
            /// <summary>
            /// Byte 7
            /// Reserved
            /// </summary>
            public byte Reserved6;
        }
        #endregion Public structures
    }
}

