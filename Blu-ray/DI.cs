// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DI.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes Blu-ray Disc Information.
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
using DiscImageChef.Console;
using System.Collections.Generic;
using System.Text;

namespace DiscImageChef.Decoders.Bluray
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
    public static class DI
    {
        #region Private constants
        const string DiscTypeBDROM = "BDO";
        const string DiscTypeBDRE = "BDW";
        const string DiscTypeBDR = "BDR";

        /// <summary>
        /// Disc Information Unit Identifier "DI"
        /// </summary>
        const UInt16 DIUIdentifier = 0x4449;
        #endregion Private constants

        #region Public methods
        public static DiscInformation? Decode(byte[] DIResponse)
        {
            if(DIResponse == null)
                return null;

            if(DIResponse.Length != 4100)
            {
                DicConsole.DebugWriteLine("BD Disc Information decoder", "Found incorrect Blu-ray Disc Information size ({0} bytes)", DIResponse.Length);
                return null;
            }

            DiscInformation decoded = new DiscInformation();

            BigEndianBitConverter.IsLittleEndian = BitConverter.IsLittleEndian;

            decoded.DataLength = BigEndianBitConverter.ToUInt16(DIResponse, 0);
            decoded.Reserved1 = DIResponse[2];
            decoded.Reserved2 = DIResponse[3];

            int offset = 4;
            List<DiscInformationUnits> units = new List<DiscInformationUnits>();

            while(true)
            {
                if(offset >= 100)
                    break;

                DiscInformationUnits unit = new DiscInformationUnits();
                unit.Signature = BigEndianBitConverter.ToUInt16(DIResponse, 0 + offset);

                if(unit.Signature != DIUIdentifier)
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
                switch(Encoding.ASCII.GetString(unit.DiscTypeIdentifier))
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
                            DicConsole.DebugWriteLine("BD Disc Information decoder", "Found unknown disc type identifier \"{0}\"", Encoding.ASCII.GetString(unit.DiscTypeIdentifier));
                            break;
                        }
                }

                units.Add(unit);

                offset += unit.Length;
            }

            if(units.Count > 0)
            {
                decoded.Units = new DiscInformationUnits[units.Count];
                for(int i = 0; i < units.Count; i++)
                    decoded.Units[i] = units[i];
            }

            return decoded;
        }

        public static string Prettify(DiscInformation? DIResponse)
        {
            if(DIResponse == null)
                return null;

            DiscInformation response = DIResponse.Value;

            StringBuilder sb = new StringBuilder();

            foreach(DiscInformationUnits unit in response.Units)
            {
                sb.AppendFormat("DI Unit Sequence: {0}", unit.Sequence).AppendLine();
                sb.AppendFormat("DI Unit Format: 0x{0:X2}", unit.Format).AppendLine();
                sb.AppendFormat("There are {0} per block", unit.UnitsPerBlock).AppendLine();
                if(Encoding.ASCII.GetString(unit.DiscTypeIdentifier) != DiscTypeBDROM)
                    sb.AppendFormat("Legacy value: 0x{0:X2}", unit.Legacy).AppendLine();
                sb.AppendFormat("DI Unit is {0} bytes", unit.Length).AppendLine();
                sb.AppendFormat("Disc type identifier: \"{0}\"", Encoding.ASCII.GetString(unit.DiscTypeIdentifier)).AppendLine();
                sb.AppendFormat("Disc size/class/version: {0}", unit.DiscSizeClassVersion).AppendLine();
                if(Encoding.ASCII.GetString(unit.DiscTypeIdentifier) == DiscTypeBDR ||
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

        public static string Prettify(byte[] DIResponse)
        {
            return Prettify(Decode(DIResponse));
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
        #endregion Public structures
    }
}

