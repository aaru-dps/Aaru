/***************************************************************************
The Disc Image Chef
----------------------------------------------------------------------------
 
Filename       : BD.cs
Version        : 1.0
Author(s)      : Natalia Portillo
 
Component      : Decoders.

Revision       : $Revision$
Last change by : $Author$
Date           : $Date$
 
--[ Description ] ----------------------------------------------------------
 
Decodes Blu-ray and DDCD structures.
 
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
using System.Collections.Generic;

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
    public static class BD
    {
        #region Private constants
        const string DiscTypeBDROM = "BDO";
        const string DiscTypeBDRE = "BDW";
        const string DiscTypeBDR = "BDR";

        /// <summary>
        /// Disc Definition Structure Identifier "DS"
        /// </summary>
        const UInt16 DDSIdentifier = 0x4453;
        /// <summary>
        /// Disc Information Unit Identifier "DI"
        /// </summary>
        const UInt16 DIUIdentifier = 0x4449;
        #endregion Private constants

        #region Public methods
        public static DiscInformation? DecodeDiscInformation(byte[] DIResponse)
        {
            if (DIResponse == null)
                return null;

            if (DIResponse.Length != 4100)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (BD Disc Information): Found incorrect Blu-ray Disc Information size ({0} bytes)", DIResponse.Length);

                return null;
            }

            DiscInformation decoded = new DiscInformation();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.DataLength = BigEndianBitConverter.ToUInt16(DIResponse, 0);
            decoded.Reserved1 = DIResponse[2];
            decoded.Reserved2 = DIResponse[3];

            int offset = 4;
            List<DiscInformationUnits> units = new List<DiscInformationUnits>();

            while (true)
            {
                if (offset >= 100)
                    break;

                DiscInformationUnits unit = new DiscInformationUnits();
                unit.Signature = BigEndianBitConverter.ToUInt16(DIResponse, 0 + offset);

                if (unit.Signature != DIUIdentifier)
                    break;

                unit.Format = DIResponse[2 + offset];
                unit.UnitsPerBlock = DIResponse[3 + offset];
                unit.Legacy = DIResponse[4 + offset];
                unit.Sequence = DIResponse[5 + offset];
                unit.Length = DIResponse[6 + offset];
                unit.Reserved = DIResponse[7 + offset];
                unit.DiscTypeIdentifier = new byte[3];
                Array.Copy(DIResponse, 8 + offset, unit.DiscTypeIdentifier, 0, 3);
                unit.DiscSizeClassVersion = DIResponse[11 + offset];
                switch (Encoding.ASCII.GetString(unit.DiscTypeIdentifier))
                {
                    case DiscTypeBDROM:
                        {
                            unit.FormatDependentContents = new byte[52];
                            Array.Copy(DIResponse, 12 + offset, unit.DiscTypeIdentifier, 0, 52);
                            break;
                        }
                    case DiscTypeBDRE:
                    case DiscTypeBDR:
                        {
                            unit.FormatDependentContents = new byte[88];
                            Array.Copy(DIResponse, 12 + offset, unit.DiscTypeIdentifier, 0, 88);
                            unit.ManufacturerID = new byte[6];
                            Array.Copy(DIResponse, 100 + offset, unit.ManufacturerID, 0, 6);
                            unit.MediaTypeID = new byte[3];
                            Array.Copy(DIResponse, 106 + offset, unit.MediaTypeID, 0, 3);
                            unit.TimeStamp = BigEndianBitConverter.ToUInt16(DIResponse, 109 + offset);
                            unit.ProductRevisionNumber = DIResponse[111 + offset];
                            break;
                        }
                    default:
                        {
                            if (MainClass.isDebug)
                                Console.WriteLine("DEBUG (BD Disc Information): Found unknown disc type identifier \"{0}\"", Encoding.ASCII.GetString(unit.DiscTypeIdentifier));
                            break;
                        }
                }

                units.Add(unit);

                offset += unit.Length;
            }

            if (units.Count > 0)
            {
                decoded.Units = new DiscInformationUnits[units.Count];
                for (int i = 0; i < units.Count; i++)
                    decoded.Units[i] = units[i];
            }

            return decoded;
        }

        public static string PrettifyDiscInformation(DiscInformation? DIResponse)
        {
            if (DIResponse == null)
                return null;

            DiscInformation response = DIResponse.Value;

            StringBuilder sb = new StringBuilder();

            foreach (DiscInformationUnits unit in response.Units)
            {
                sb.AppendFormat("DI Unit Sequence: {0}", unit.Sequence).AppendLine();
                sb.AppendFormat("DI Unit Format: 0x{0:X2}", unit.Format).AppendLine();
                sb.AppendFormat("There are {0} per block", unit.UnitsPerBlock).AppendLine();
                if (Encoding.ASCII.GetString(unit.DiscTypeIdentifier) != DiscTypeBDROM)
                    sb.AppendFormat("Legacy value: 0x{0:X2}", unit.Legacy).AppendLine();
                sb.AppendFormat("DI Unit is {0} bytes", unit.Length).AppendLine();
                sb.AppendFormat("Disc type identifier: \"{0}\"", Encoding.ASCII.GetString(unit.DiscTypeIdentifier)).AppendLine();
                sb.AppendFormat("Disc size/class/version: {0}", unit.DiscSizeClassVersion).AppendLine();
                if (Encoding.ASCII.GetString(unit.DiscTypeIdentifier) == DiscTypeBDR ||
                   Encoding.ASCII.GetString(unit.DiscTypeIdentifier) == DiscTypeBDRE)
                {
                    sb.AppendFormat("Disc manufacturer ID: \"{0}\"", Encoding.ASCII.GetString(unit.ManufacturerID)).AppendLine();
                    sb.AppendFormat("Disc media type ID: \"{0}\"", Encoding.ASCII.GetString(unit.MediaTypeID)).AppendLine();
                    sb.AppendFormat("Disc timestamp: 0x{0:X2}", unit.TimeStamp).AppendLine();
                    sb.AppendFormat("Disc product revison number: {0}", unit.ProductRevisionNumber).AppendLine();
                }

                sb.AppendFormat("Blu-ray DI Unit format dependent contents as hex follows:");
                sb.AppendLine(PrintHex.ByteArrayToHexArrayString(unit.FormatDependentContents, 80));
            }

            return sb.ToString();
        }

        public static string PrettifyDiscInformation(byte[] DIResponse)
        {
            DiscInformation? decoded = DecodeDiscInformation(DIResponse);
            return PrettifyDiscInformation(decoded);
        }

        public static BurstCuttingArea? DecodeBurstCuttingArea(byte[] BCAResponse)
        {
            if (BCAResponse == null)
                return null;

            if (BCAResponse.Length != 68)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (BD BCA): Found incorrect Blu-ray BCA size ({0} bytes)", BCAResponse.Length);

                return null;
            }

            BurstCuttingArea decoded = new BurstCuttingArea();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.DataLength = BigEndianBitConverter.ToUInt16(BCAResponse, 0);
            decoded.Reserved1 = BCAResponse[2];
            decoded.Reserved2 = BCAResponse[3];
            decoded.BCA = new byte[64];
            Array.Copy(BCAResponse, 4, decoded.BCA, 0, 64);

            return decoded;
        }

        public static string PrettifyBurstCuttingArea(BurstCuttingArea? BCAResponse)
        {
            if (BCAResponse == null)
                return null;

            BurstCuttingArea response = BCAResponse.Value;

            StringBuilder sb = new StringBuilder();

            if (MainClass.isDebug)
            {
                sb.AppendFormat("DEBUG (BD BCA): Reserved1 = 0x{0:X2}", response.Reserved1).AppendLine();
                sb.AppendFormat("DEBUG (BD BCA): Reserved2 = 0x{0:X2}", response.Reserved2).AppendLine();
            }
            sb.AppendFormat("Blu-ray Burst Cutting Area in hex follows:");
            sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.BCA, 80));

            return sb.ToString();
        }

        public static string PrettifyBurstCuttingArea(byte[] BCAResponse)
        {
            BurstCuttingArea? decoded = DecodeBurstCuttingArea(BCAResponse);
            return PrettifyBurstCuttingArea(decoded);
        }

        public static DiscDefinitionStructure? DecodeDDS(byte[] DDSResponse)
        {
            if (DDSResponse == null)
                return null;

            DiscDefinitionStructure decoded = new DiscDefinitionStructure();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.DataLength = BigEndianBitConverter.ToUInt16(DDSResponse, 0);
            decoded.Reserved1 = DDSResponse[2];
            decoded.Reserved2 = DDSResponse[3];
            decoded.Signature = BigEndianBitConverter.ToUInt16(DDSResponse, 4);
            if (decoded.Signature != DDSIdentifier)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (BD DDS): Found incorrect DDS signature (0x{0:X4})", decoded.Signature);

                return null;

            }
            decoded.Format = DDSResponse[6];
            decoded.Reserved3 = DDSResponse[7];
            decoded.UpdateCount = BigEndianBitConverter.ToUInt32(DDSResponse, 8);
            decoded.Reserved4 = BigEndianBitConverter.ToUInt64(DDSResponse, 12);
            decoded.DriveAreaPSN = BigEndianBitConverter.ToUInt32(DDSResponse, 20);
            decoded.Reserved5 = BigEndianBitConverter.ToUInt32(DDSResponse, 24);
            decoded.DefectListPSN = BigEndianBitConverter.ToUInt32(DDSResponse, 28);
            decoded.Reserved6 = BigEndianBitConverter.ToUInt32(DDSResponse, 32);
            decoded.PSNofLSNZero = BigEndianBitConverter.ToUInt32(DDSResponse, 36);
            decoded.LastUserAreaLSN = BigEndianBitConverter.ToUInt32(DDSResponse, 40);
            decoded.ISA0 = BigEndianBitConverter.ToUInt32(DDSResponse, 44);
            decoded.OSA = BigEndianBitConverter.ToUInt32(DDSResponse, 48);
            decoded.ISA1 = BigEndianBitConverter.ToUInt32(DDSResponse, 52);
            decoded.SpareAreaFullFlags = DDSResponse[56];
            decoded.Reserved7 = DDSResponse[57];
            decoded.DiscTypeSpecificField1 = DDSResponse[58];
            decoded.Reserved8 = DDSResponse[59];
            decoded.DiscTypeSpecificField2 = BigEndianBitConverter.ToUInt32(DDSResponse, 60);
            decoded.Reserved9 = BigEndianBitConverter.ToUInt32(DDSResponse, 64);
            decoded.StatusBits = new byte[32];
            Array.Copy(DDSResponse, 68, decoded.StatusBits, 0, 32);
            decoded.DiscTypeSpecificData = new byte[DDSResponse.Length - 100];
            Array.Copy(DDSResponse, 100, decoded.DiscTypeSpecificData, 0, DDSResponse.Length - 100);

            return decoded;
        }

        public static string PrettifyDDS(DiscDefinitionStructure? DDSResponse)
        {
            if (DDSResponse == null)
                return null;

            DiscDefinitionStructure response = DDSResponse.Value;

            StringBuilder sb = new StringBuilder();

            if (MainClass.isDebug)
            {
                sb.AppendFormat("DEBUG (BD Disc Definition Structure): Reserved1 = 0x{0:X2}", response.Reserved1).AppendLine();
                sb.AppendFormat("DEBUG (BD Disc Definition Structure): Reserved2 = 0x{0:X2}", response.Reserved2).AppendLine();
                sb.AppendFormat("DEBUG (BD Disc Definition Structure): Reserved3 = 0x{0:X2}", response.Reserved3).AppendLine();
                sb.AppendFormat("DEBUG (BD Disc Definition Structure): Reserved4 = 0x{0:X16}", response.Reserved4).AppendLine();
                sb.AppendFormat("DEBUG (BD Disc Definition Structure): Reserved5 = 0x{0:X8}", response.Reserved5).AppendLine();
                sb.AppendFormat("DEBUG (BD Disc Definition Structure): Reserved6 = 0x{0:X8}", response.Reserved6).AppendLine();
                sb.AppendFormat("DEBUG (BD Disc Definition Structure): Reserved7 = 0x{0:X2}", response.Reserved7).AppendLine();
                sb.AppendFormat("DEBUG (BD Disc Definition Structure): Reserved8 = 0x{0:X2}", response.Reserved8).AppendLine();
                sb.AppendFormat("DEBUG (BD Disc Definition Structure): Reserved9 = 0x{0:X8}", response.Reserved9).AppendLine();
            }

            sb.AppendFormat("DDS Format: 0x{0:X2}", response.Format).AppendLine();
            sb.AppendFormat("DDS has ben updated {0} times", response.UpdateCount).AppendLine();
            sb.AppendFormat("First PSN of Drive Area: 0x{0:X8}", response.DriveAreaPSN).AppendLine();
            sb.AppendFormat("First PSN of Defect List: 0x{0:X8}", response.DefectListPSN).AppendLine();
            sb.AppendFormat("PSN of User Data Area's LSN 0: 0x{0:X8}", response.PSNofLSNZero).AppendLine();
            sb.AppendFormat("Last User Data Area's LSN 0: 0x{0:X8}", response.LastUserAreaLSN).AppendLine();
            sb.AppendFormat("ISA0 size: {0}", response.ISA0).AppendLine();
            sb.AppendFormat("OSA size: {0}", response.OSA).AppendLine();
            sb.AppendFormat("ISA1 size: {0}", response.ISA1).AppendLine();
            sb.AppendFormat("Spare Area Full Flags: 0x{0:X2}", response.SpareAreaFullFlags).AppendLine();
            sb.AppendFormat("Disc Type Specific Field 1: 0x{0:X2}", response.DiscTypeSpecificField1).AppendLine();
            sb.AppendFormat("Disc Type Specific Field 2: 0x{0:X8}", response.DiscTypeSpecificField2).AppendLine();
            sb.AppendFormat("Blu-ray DDS Status Bits in hex follows:");
            sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.StatusBits, 80));
            sb.AppendFormat("Blu-ray DDS Disc Type Specific Data in hex follows:");
            sb.AppendLine(PrintHex.ByteArrayToHexArrayString(response.DiscTypeSpecificData, 80));

            return sb.ToString();
        }

        public static string PrettifyDDS(byte[] DDSResponse)
        {
            DiscDefinitionStructure? decoded = DecodeDDS(DDSResponse);
            return PrettifyDDS(decoded);
        }

        public static CartridgeStatus? DecodeCartridgeStatus(byte[] CSResponse)
        {
            if (CSResponse == null)
                return null;

            if (CSResponse.Length != 8)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (BD Cartridge Status): Found incorrect Blu-ray Spare Area Information size ({0} bytes)", CSResponse.Length);

                return null;
            }

            CartridgeStatus decoded = new CartridgeStatus();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.DataLength = BigEndianBitConverter.ToUInt16(CSResponse, 0);
            decoded.Reserved1 = CSResponse[2];
            decoded.Reserved2 = CSResponse[3];
            decoded.Cartridge = Convert.ToBoolean(CSResponse[4] & 0x80);
            decoded.OUT = Convert.ToBoolean(CSResponse[4]&0x40);
            decoded.Reserved3 = (byte)((CSResponse[4] & 0x38) >> 3);
            decoded.OUT = Convert.ToBoolean(CSResponse[4]&0x04);
            decoded.Reserved4 = (byte)(CSResponse[4] & 0x03);
            decoded.Reserved5 = CSResponse[5];
            decoded.Reserved6 = CSResponse[6];
            decoded.Reserved7 = CSResponse[7];

            return decoded;
        }

        public static string PrettifyCartridgeStatus(CartridgeStatus? CSResponse)
        {
            if (CSResponse == null)
                return null;

            CartridgeStatus response = CSResponse.Value;

            StringBuilder sb = new StringBuilder();

            if (MainClass.isDebug)
            {
                sb.AppendFormat("DEBUG (BD Cartridge Status): Reserved1 = 0x{0:X2}", response.Reserved1).AppendLine();
                sb.AppendFormat("DEBUG (BD Cartridge Status): Reserved2 = 0x{0:X2}", response.Reserved2).AppendLine();
                sb.AppendFormat("DEBUG (BD Cartridge Status): Reserved3 = 0x{0:X8}", response.Reserved3).AppendLine();
                sb.AppendFormat("DEBUG (BD Cartridge Status): Reserved4 = 0x{0:X8}", response.Reserved4).AppendLine();
                sb.AppendFormat("DEBUG (BD Cartridge Status): Reserved5 = 0x{0:X8}", response.Reserved5).AppendLine();
                sb.AppendFormat("DEBUG (BD Cartridge Status): Reserved6 = 0x{0:X8}", response.Reserved6).AppendLine();
                sb.AppendFormat("DEBUG (BD Cartridge Status): Reserved7 = 0x{0:X8}", response.Reserved7).AppendLine();
            }

            if (response.Cartridge)
            {
                sb.AppendLine("Media is inserted in a cartridge");
                if (response.OUT)
                    sb.AppendLine("Media has been taken out, or inserted in, the cartridge");
                if (response.CWP)
                    sb.AppendLine("Media is write protected");
            }
            else
            {
                sb.AppendLine("Media is not in a cartridge");
                if (MainClass.isDebug)
                {
                    if (response.OUT)
                        sb.AppendLine("Media has out bit marked, shouldn't");
                    if (response.CWP)
                        sb.AppendLine("Media has write protection bit marked, shouldn't");
                }
            }
            return sb.ToString();
        }

        public static string PrettifyCartridgeStatus(byte[] CSResponse)
        {
            CartridgeStatus? decoded = DecodeCartridgeStatus(CSResponse);
            return PrettifyCartridgeStatus(decoded);
        }

        public static SpareAreaInformation? DecodeCDTOC(byte[] SAIResponse)
        {
            if (SAIResponse == null)
                return null;

            if (SAIResponse.Length != 16)
            {
                if (MainClass.isDebug)
                    Console.WriteLine("DEBUG (BD Spare Area Information): Found incorrect Blu-ray Spare Area Information size ({0} bytes)", SAIResponse.Length);

                return null;
            }

            SpareAreaInformation decoded = new SpareAreaInformation();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.DataLength = BigEndianBitConverter.ToUInt16(SAIResponse, 0);
            decoded.Reserved1 = SAIResponse[2];
            decoded.Reserved2 = SAIResponse[3];
            decoded.Reserved3 = BigEndianBitConverter.ToUInt32(SAIResponse, 4);
            decoded.FreeSpareBlocks = BigEndianBitConverter.ToUInt32(SAIResponse, 8);
            decoded.AllocatedSpareBlocks = BigEndianBitConverter.ToUInt32(SAIResponse, 12);

            return decoded;
        }

        public static string PrettifySpareAreaInformation(SpareAreaInformation? SAIResponse)
        {
            if (SAIResponse == null)
                return null;

            SpareAreaInformation response = SAIResponse.Value;

            StringBuilder sb = new StringBuilder();

            if (MainClass.isDebug)
            {
                sb.AppendFormat("DEBUG (BD Spare Area Information): Reserved1 = 0x{0:X2}", response.Reserved1).AppendLine();
                sb.AppendFormat("DEBUG (BD Spare Area Information): Reserved2 = 0x{0:X2}", response.Reserved2).AppendLine();
                sb.AppendFormat("DEBUG (BD Spare Area Information): Reserved3 = 0x{0:X8}", response.Reserved3).AppendLine();
            }
            sb.AppendFormat("{0} free spare blocks", response.FreeSpareBlocks).AppendLine();
            sb.AppendFormat("{0} allocated spare blocks", response.AllocatedSpareBlocks).AppendLine();

            return sb.ToString();
        }

        public static string PrettifySpareAreaInformation(byte[] SAIResponse)
        {
            SpareAreaInformation? decoded = DecodeCDTOC(SAIResponse);
            return PrettifySpareAreaInformation(decoded);
        }
        #endregion Public methods

        #region Public structures
        public struct DiscInformation
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Always 4098
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
            /// Byte 4 to 4099
            /// Disc information units
            /// </summary>
            public DiscInformationUnits[] Units;
        }

        public struct DiscInformationUnits
        {
            /// <summary>
            /// Byte 0
            /// "DI"
            /// </summary>
            public UInt16 Signature;
            /// <summary>
            /// Byte 2
            /// Disc information format
            /// </summary>
            public byte Format;
            /// <summary>
            /// Byte 3
            /// Number of DI units per block
            /// </summary>
            public byte UnitsPerBlock;
            /// <summary>
            /// Byte 4
            /// Reserved for BD-ROM, legacy information for BD-R/-RE
            /// </summary>
            public byte Legacy;
            /// <summary>
            /// Byte 5
            /// Sequence number for this DI unit
            /// </summary>
            public byte Sequence;
            /// <summary>
            /// Byte 6
            /// Number of bytes used by this DI unit, should be 64 for BD-ROM and 112 for BD-R/-RE
            /// </summary>
            public byte Length;
            /// <summary>
            /// Byte 7
            /// Reserved
            /// </summary>
            public byte Reserved;
            /// <summary>
            /// Bytes 8 to 10
            /// Disc type identifier
            /// </summary>
            public byte[] DiscTypeIdentifier;
            /// <summary>
            /// Byte 11
            /// Disc size/class/version
            /// </summary>
            public byte DiscSizeClassVersion;
            /// <summary>
            /// Bytes 12 to 63 for BD-ROM, bytes 12 to 99 for BD-R/-RE
            /// Format dependent contents, disclosed in private blu-ray specifications
            /// </summary>
            public byte[] FormatDependentContents;
            /// <summary>
            /// Bytes 100 to 105, BD-R/-RE only
            /// Manufacturer ID
            /// </summary>
            public byte[] ManufacturerID;
            /// <summary>
            /// Bytes 106 to 108, BD-R/-RE only
            /// Media type ID
            /// </summary>
            public byte[] MediaTypeID;
            /// <summary>
            /// Bytes 109 to 110, BD-R/-RE only
            /// Timestamp
            /// </summary>
            public UInt16 TimeStamp;
            /// <summary>
            /// Byte 111
            /// Product revision number
            /// </summary>
            public byte ProductRevisionNumber;
        }

        public struct BurstCuttingArea
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Always 66
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
            /// Byte 4 to 67
            /// BCA data
            /// </summary>
            public byte[] BCA;
        }

        public struct DiscDefinitionStructure
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
            /// Bytes 4 to 5
            /// "DS"
            /// </summary>
            public UInt16 Signature;
            /// <summary>
            /// Byte 6
            /// DDS format
            /// </summary>
            public byte Format;
            /// <summary>
            /// Byte 7
            /// Reserved
            /// </summary>
            public byte Reserved3;
            /// <summary>
            /// Bytes 8 to 11
            /// DDS update count
            /// </summary>
            public UInt32 UpdateCount;
            /// <summary>
            /// Bytes 12 to 19
            /// Reserved
            /// </summary>
            public UInt64 Reserved4;
            /// <summary>
            /// Bytes 20 to 23
            /// First PSN of Drive Area
            /// </summary>
            public UInt32 DriveAreaPSN;
            /// <summary>
            /// Bytes 24 to 27
            /// Reserved
            /// </summary>
            public UInt32 Reserved5;
            /// <summary>
            /// Bytes 28 to 31
            /// First PSN of Defect List
            /// </summary>
            public UInt32 DefectListPSN;
            /// <summary>
            /// Bytes 32 to 35
            /// Reserved
            /// </summary>
            public UInt32 Reserved6;
            /// <summary>
            /// Bytes 36 to 39
            /// PSN of LSN 0 of user data area
            /// </summary>
            public UInt32 PSNofLSNZero;
            /// <summary>
            /// Bytes 40 to 43
            /// Last LSN of user data area
            /// </summary>
            public UInt32 LastUserAreaLSN;
            /// <summary>
            /// Bytes 44 to 47
            /// ISA0 size
            /// </summary>
            public UInt32 ISA0;
            /// <summary>
            /// Bytes 48 to 51
            /// OSA size
            /// </summary>
            public UInt32 OSA;
            /// <summary>
            /// Bytes 52 to 55
            /// ISA1 size
            /// </summary>
            public UInt32 ISA1;
            /// <summary>
            /// Byte 56
            /// Spare Area full flags
            /// </summary>
            public byte SpareAreaFullFlags;
            /// <summary>
            /// Byte 57 
            /// Reserved
            /// </summary>
            public byte Reserved7;
            /// <summary>
            /// Byte 58
            /// Disc type specific field
            /// </summary>
            public byte DiscTypeSpecificField1;
            /// <summary>
            /// Byte 59
            /// Reserved
            /// </summary>
            public byte Reserved8;
            /// <summary>
            /// Byte 60 to 63
            /// Disc type specific field
            /// </summary>
            public UInt32 DiscTypeSpecificField2;
            /// <summary>
            /// Byte 64 to 67
            /// Reserved
            /// </summary>
            public UInt32 Reserved9;
            /// <summary>
            /// Bytes 68 to 99
            /// Status bits of INFO1/2 and PAC1/2 on L0 and L1
            /// </summary>
            public byte[] StatusBits;
            /// <summary>
            /// Bytes 100 to end
            /// Disc type specific data
            /// </summary>
            public byte[] DiscTypeSpecificData;
        }

        public struct CartridgeStatus
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Always 6
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
            /// Byte 4, bit 7
            /// Medium is inserted in a cartridge
            /// </summary>
            public bool Cartridge;
            /// <summary>
            /// Byte 4, bit 6
            /// Medium taken out / put in a cartridge
            /// </summary>
            public bool OUT;
            /// <summary>
            /// Byte 4, bits 5 to 3
            /// Reserved
            /// </summary>
            public byte Reserved3;
            /// <summary>
            /// Byte 4, bit 2
            /// Cartridge sets write protection
            /// </summary>
            public bool CWP;
            /// <summary>
            /// Byte 4, bits 1 to 0
            /// Reserved
            /// </summary>
            public byte Reserved4;
            /// <summary>
            /// Byte 5
            /// Reserved
            /// </summary>
            public byte Reserved5;
            /// <summary>
            /// Byte 6
            /// Reserved
            /// </summary>
            public byte Reserved6;
            /// <summary>
            /// Byte 7
            /// Reserved
            /// </summary>
            public byte Reserved7;
        }

        public struct SpareAreaInformation
        {
            /// <summary>
            /// Bytes 0 to 1
            /// Always 14
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
            /// Bytes 4 to 7
            /// Reserved
            /// </summary>
            public UInt32 Reserved3;
            /// <summary>
            /// Bytes 8 to 11
            /// Free spare blocks
            /// </summary>
            public UInt32 FreeSpareBlocks;
            /// <summary>
            /// Bytes 12 to 15
            /// Allocated spare blocks
            /// </summary>
            public UInt32 AllocatedSpareBlocks;
        }
        #endregion Public structures
    }
}

