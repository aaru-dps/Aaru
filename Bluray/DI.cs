// /***************************************************************************
// Aaru Data Preservation Suite
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
// Copyright © 2011-2022 Natalia Portillo
// ****************************************************************************/

namespace Aaru.Decoders.Bluray;

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Aaru.Console;
using Aaru.Helpers;

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
public static class DI
{
    public enum BluSize : byte
    {
        /// <summary>120mm</summary>
        OneTwenty = 0,
        /// <summary>80mm</summary>
        Eighty = 1
    }

    public enum ChannelLength : byte
    {
        /// <summary>74.5nm channel or 25Gb/layer</summary>
        Seventy = 1,
        /// <summary>69.0nm channel or 27Gb/layer</summary>
        Sixty = 2
    }

    public enum HybridLayer : byte
    {
        /// <summary>No hybrid layer</summary>
        None = 0,
        /// <summary>-ROM layer</summary>
        ReadOnly = 1,
        /// <summary>-R layer</summary>
        Recordable = 2,
        /// <summary>-RW layer</summary>
        Rewritable = 3
    }

    #region Private constants
    const string DiscTypeBDROM = "BDO";
    const string DiscTypeBDRE  = "BDW";
    const string DiscTypeBDR   = "BDR";

    /// <summary>Disc Information Unit Identifier "DI"</summary>
    const ushort DIUIdentifier = 0x4449;
    #endregion Private constants

    #region Public methods
    public static DiscInformation? Decode(byte[] DIResponse)
    {
        if(DIResponse == null)
            return null;

        if(DIResponse.Length != 4100)
        {
            AaruConsole.DebugWriteLine("BD Disc Information decoder",
                                       "Found incorrect Blu-ray Disc Information size ({0} bytes)", DIResponse.Length);

            return null;
        }

        var decoded = new DiscInformation
        {
            DataLength = BigEndianBitConverter.ToUInt16(DIResponse, 0),
            Reserved1  = DIResponse[2],
            Reserved2  = DIResponse[3]
        };

        var offset = 4;
        var units  = new List<DiscInformationUnits>();

        while(true)
        {
            if(offset >= 4100)
                break;

            var unit = new DiscInformationUnits
            {
                Signature = BigEndianBitConverter.ToUInt16(DIResponse, 0 + offset)
            };

            if(unit.Signature != DIUIdentifier)
                break;

            unit.Format             = DIResponse[2 + offset];
            unit.UnitsPerBlock      = (byte)((DIResponse[3 + offset] & 0xF8) >> 3);
            unit.Layer              = (byte)(DIResponse[3 + offset] & 0x07);
            unit.Legacy             = DIResponse[4 + offset];
            unit.Sequence           = DIResponse[5 + offset];
            unit.Continuation       = (DIResponse[6       + offset] & 0x80) == 0x80;
            unit.Length             = (byte)(DIResponse[6 + offset] & 0x7F);
            unit.Reserved           = DIResponse[7 + offset];
            unit.DiscTypeIdentifier = new byte[3];
            Array.Copy(DIResponse, 8 + offset, unit.DiscTypeIdentifier, 0, 3);
            unit.DiscSize         = (BluSize)((DIResponse[11 + offset] & 0xC0) >> 6);
            unit.DiscClass        = (byte)((DIResponse[11    + offset] & 0x30) >> 4);
            unit.DiscVersion      = (byte)(DIResponse[11 + offset] & 0x0F);
            unit.Layers           = (byte)((DIResponse[12        + offset] & 0xF0) >> 4);
            unit.DvdLayer         = (HybridLayer)((DIResponse[13 + offset] & 0xC0) >> 6);
            unit.CdLayer          = (HybridLayer)((DIResponse[13 + offset] & 0x30) >> 4);
            unit.ChannelLength    = (ChannelLength)(DIResponse[13 + offset] & 0x0F);
            unit.Polarity         = DIResponse[14 + offset];
            unit.RecordedPolarity = DIResponse[14 + offset];
            unit.Bca              = (byte)(DIResponse[16 + offset] & 0x0F);
            unit.MaxTransfer      = DIResponse[17 + offset];

            unit.LastPsn = (uint)((DIResponse[20 + offset] << 24) + (DIResponse[21 + offset] << 16) +
                                  (DIResponse[22 + offset] << 8)  + DIResponse[23 + offset]);

            // TODO: In -R/-RE how does this relate to layer size???
            unit.FirstAun = (uint)((DIResponse[24 + offset] << 24) + (DIResponse[25 + offset] << 16) +
                                   (DIResponse[26 + offset] << 8)  + DIResponse[27 + offset]);

            unit.LastAun = (uint)((DIResponse[28 + offset] << 24) + (DIResponse[29 + offset] << 16) +
                                  (DIResponse[30 + offset] << 8)  + DIResponse[31 + offset]);

            switch(Encoding.ASCII.GetString(unit.DiscTypeIdentifier))
            {
                case DiscTypeBDROM:
                {
                    unit.FormatDependentContents = new byte[32];
                    Array.Copy(DIResponse, 32 + offset, unit.FormatDependentContents, 0, 32);

                    break;
                }

                case DiscTypeBDRE:
                case DiscTypeBDR:
                {
                    unit.FormatDependentContents = new byte[66];
                    Array.Copy(DIResponse, 32 + offset, unit.FormatDependentContents, 0, 66);
                    unit.ManufacturerID = new byte[6];
                    Array.Copy(DIResponse, 100 + offset, unit.ManufacturerID, 0, 6);
                    unit.MediaTypeID = new byte[3];
                    Array.Copy(DIResponse, 106 + offset, unit.MediaTypeID, 0, 3);
                    unit.TimeStamp             = BigEndianBitConverter.ToUInt16(DIResponse, 109 + offset);
                    unit.ProductRevisionNumber = DIResponse[111                                 + offset];

                    offset += 14;

                    break;
                }

                default:
                {
                    AaruConsole.DebugWriteLine("BD Disc Information decoder",
                                               "Found unknown disc type identifier \"{0}\"",
                                               Encoding.ASCII.GetString(unit.DiscTypeIdentifier));

                    break;
                }
            }

            units.Add(unit);

            offset += unit.Length;
        }

        if(units.Count <= 0)
            return decoded;

        decoded.Units = new DiscInformationUnits[units.Count];

        for(var i = 0; i < units.Count; i++)
            decoded.Units[i] = units[i];

        return decoded;
    }

    public static string Prettify(DiscInformation? DIResponse)
    {
        if(DIResponse == null)
            return null;

        DiscInformation response = DIResponse.Value;

        var sb = new StringBuilder();

        foreach(DiscInformationUnits unit in response.Units)
        {
            sb.AppendFormat("DI Unit Sequence: {0}", unit.Sequence).AppendLine();
            sb.AppendFormat("DI Unit Format: 0x{0:X2}", unit.Format).AppendLine();
            sb.AppendFormat("There are {0} per block", unit.UnitsPerBlock).AppendLine();
            sb.AppendFormat("This DI refers to layer {0}", unit.Layer).AppendLine();

            if(Encoding.ASCII.GetString(unit.DiscTypeIdentifier) == DiscTypeBDRE)
                sb.AppendFormat("Legacy value: 0x{0:X2}", unit.Legacy).AppendLine();

            sb.AppendLine(unit.Continuation ? "This DI continues previous unit" : "This DI starts a new unit");
            sb.AppendFormat("DI Unit is {0} bytes", unit.Length).AppendLine();

            sb.AppendFormat("Disc type identifier: \"{0}\"", Encoding.ASCII.GetString(unit.DiscTypeIdentifier)).
               AppendLine();

            switch(unit.DiscSize)
            {
                case BluSize.OneTwenty:
                    sb.AppendLine("Disc size: 120mm");

                    break;
                case BluSize.Eighty:
                    sb.AppendLine("Disc size: 80mm");

                    break;
                default:
                    sb.AppendFormat("Disc size: Unknown code {0}", (byte)unit.DiscSize).AppendLine();

                    break;
            }

            sb.AppendFormat("Disc class: {0}", unit.DiscClass).AppendLine();
            sb.AppendFormat("Disc version: {0}", unit.DiscVersion).AppendLine();
            sb.AppendFormat("This disc has {0} layers", unit.Layers).AppendLine();

            switch(unit.DvdLayer)
            {
                case HybridLayer.None:
                    sb.AppendLine("This disc does not contain a DVD layer.");

                    break;
                case HybridLayer.ReadOnly:
                    sb.AppendLine("This disc contains a DVD-ROM layer.");

                    break;
                case HybridLayer.Recordable:
                    sb.AppendLine("This disc contains a DVD-R layer.");

                    break;
                case HybridLayer.Rewritable:
                    sb.AppendLine("This disc contains a DVD-RW layer.");

                    break;
            }

            switch(unit.CdLayer)
            {
                case HybridLayer.None:
                    sb.AppendLine("This disc does not contain a CD layer.");

                    break;
                case HybridLayer.ReadOnly:
                    sb.AppendLine("This disc contains a CD-ROM layer.");

                    break;
                case HybridLayer.Recordable:
                    sb.AppendLine("This disc contains a CD-R layer.");

                    break;
                case HybridLayer.Rewritable:
                    sb.AppendLine("This disc contains a CD-RW layer.");

                    break;
            }

            switch(unit.ChannelLength)
            {
                case ChannelLength.Seventy:
                    sb.AppendLine("Disc uses a 74.5nm channel giving 25 Gb per layer.");

                    break;
                case ChannelLength.Sixty:
                    sb.AppendLine("Disc uses a 69.0nm channel giving 27 Gb per layer.");

                    break;
                default:
                    sb.AppendFormat("Disc uses unknown channel length with code {0}", (byte)unit.ChannelLength).
                       AppendLine();

                    break;
            }

            switch(unit.Polarity)
            {
                case 0:
                    sb.AppendLine("Disc uses positive polarity.");

                    break;
                case 1:
                    sb.AppendLine("Disc uses negative polarity.");

                    break;
                default:
                    sb.AppendFormat("Disc uses unknown polarity with code {0}", unit.Polarity).AppendLine();

                    break;
            }

            if(Encoding.ASCII.GetString(unit.DiscTypeIdentifier) == DiscTypeBDR)
                switch(unit.RecordedPolarity)
                {
                    case 0:
                        sb.AppendLine("Recorded marks have a lower reflectivity than unrecorded ones (HTL disc).");

                        break;
                    case 1:
                        sb.AppendLine("Recorded marks have a higher reflectivity than unrecorded ones (LTH disc).");

                        break;
                    default:
                        sb.AppendFormat("Disc uses unknown recorded reflectivity polarity with code {0}",
                                        unit.RecordedPolarity).AppendLine();

                        break;
                }

            switch(unit.Bca)
            {
                case 0:
                    sb.AppendLine("Disc doesn't have a BCA.");

                    break;
                case 1:
                    sb.AppendLine("Disc has a BCA.");

                    break;
                default:
                    sb.AppendFormat("Disc uses unknown BCA code {0}", unit.Bca).AppendLine();

                    break;
            }

            if(unit.MaxTransfer > 0)
                sb.AppendFormat("Disc has a maximum transfer rate of {0} Mbit/sec.", unit.MaxTransfer).AppendLine();
            else
                sb.AppendLine("Disc does not specify a maximum transfer rate.");

            sb.AppendFormat("Last user data PSN for disc: {0}", unit.LastPsn).AppendLine();

            sb.AppendFormat("First address unit number of data zone in this layer: {0}", unit.FirstAun).AppendLine();

            sb.AppendFormat("Last address unit number of data zone in this layer: {0}", unit.LastAun).AppendLine();

            if(Encoding.ASCII.GetString(unit.DiscTypeIdentifier) == DiscTypeBDR ||
               Encoding.ASCII.GetString(unit.DiscTypeIdentifier) == DiscTypeBDRE)
            {
                sb.AppendFormat("Disc manufacturer ID: \"{0}\"", Encoding.ASCII.GetString(unit.ManufacturerID)).
                   AppendLine();

                sb.AppendFormat("Disc media type ID: \"{0}\"", Encoding.ASCII.GetString(unit.MediaTypeID)).AppendLine();

                sb.AppendFormat("Disc timestamp: 0x{0:X2}", unit.TimeStamp).AppendLine();
                sb.AppendFormat("Disc product revision number: {0}", unit.ProductRevisionNumber).AppendLine();
            }

            sb.AppendFormat("Blu-ray DI Unit format dependent contents as hex follows:");
            sb.AppendLine(PrintHex.ByteArrayToHexArrayString(unit.FormatDependentContents, 80));
        }

        return sb.ToString();
    }

    public static string Prettify(byte[] DIResponse) => Prettify(Decode(DIResponse));

    public static string ManufacturerFromDI(string manufacturerId)
    {
        var manufacturer = "";

        // ReSharper disable StringLiteralTypo
        switch(manufacturerId)
        {
            case "AMESOB":
            case "OTCBDR":
                manufacturer = "Amethystum Storage Technology Co., Ltd.";

                break;
            case "UMEBDR":
            case "ANWELL":
                manufacturer = "Avic Umedisc HK Ltd.";

                break;
            case "MAXELL":
                manufacturer = "Hitachi Maxell, Ltd.";

                break;
            case "CMCMAG":
                manufacturer = "CMC Magnetics Corporation";

                break;
            case "ISMMBD":
                manufacturer = "Info Source Digital Media (Zhong Shan) Co., Ltd.";

                break;
            case "LGEBRA":
                manufacturer = "LG Electronics Inc.";

                break;
            case "MILLEN":
                manufacturer = "Millenniata, Inc.";

                break;
            case "VERBAT":
            case "VAMKM":
                manufacturer = "Mitsubishi Chemical Media Co., Ltd.";

                break;
            case "PHILIP":
            case "MBI":
                manufacturer = "Moser Baer India Ltd.";

                break;
            case "MEI":
            case "PAN":
                manufacturer = "Matsushita Electric Industrial Co., Ltd.";

                break;
            case "PRODIS":
                manufacturer = "Prodisc Technology Inc.";

                break;
            case "RITEK":
                manufacturer = "Ritek Co.";

                break;
            case "SONY":
                manufacturer = "Sony Corporation";

                break;
            case "TYG-BD":
                manufacturer = "Taiyo Yuden Company Ltd.";

                break;
            case "TDKBLD":
                manufacturer = "TDK Corporation";

                break;
            case "JVC-AM":
            case "JVCVAM":
                manufacturer = "Victor Advanced media Co., Ltd.";

                break;
            case "JVCRE1":
                manufacturer = "JVC KENWOOD Corporation";

                break;
            case "INFOME":
                manufacturer = "InfoMedia Inc.";

                break;
        }

        // ReSharper restore StringLiteralTypo

        return manufacturer != "" ? $"{manufacturer} (\"{manufacturerId}\")" : $"\"{manufacturerId}\"";
    }
    #endregion Public methods

    #region Public structures
    public struct DiscInformation
    {
        /// <summary>Bytes 0 to 1 Always 4098</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Byte 4 to 4099 Disc information units</summary>
        public DiscInformationUnits[] Units;
    }

    public struct DiscInformationUnits
    {
        /// <summary>Byte 0 "DI"</summary>
        public ushort Signature;
        /// <summary>Byte 2 Disc information format</summary>
        public byte Format;
        /// <summary>Byte 3, bits 7 to 3 Number of DI units per block</summary>
        public byte UnitsPerBlock;
        /// <summary>Byte 3, bits 2 to 0 Layer this DI refers to</summary>
        public byte Layer;
        /// <summary>Byte 4 Reserved for BD-ROM, legacy information for BD-R/-RE</summary>
        public byte Legacy;
        /// <summary>Byte 5 Sequence number for this DI unit</summary>
        public byte Sequence;
        /// <summary>Byte 6, bit 7 If set this DI is a continuation of the previous one</summary>
        public bool Continuation;
        /// <summary>Byte 6, bits 6 to 0 Number of bytes used by this DI unit, should be 64 for BD-ROM and 112 for BD-R/-RE</summary>
        public byte Length;
        /// <summary>Byte 7 Reserved</summary>
        public byte Reserved;
        /// <summary>Bytes 8 to 10 Disc type identifier</summary>
        public byte[] DiscTypeIdentifier;
        /// <summary>Byte 11, bits 7 to 6 Disc size</summary>
        public BluSize DiscSize;
        /// <summary>Byte 11, bits 5 to 4 Disc class</summary>
        public byte DiscClass;
        /// <summary>Byte 11, bits 3 to 0 Disc version</summary>
        public byte DiscVersion;
        /// <summary>Byte 12, bits 7 to 4 Layers in this disc</summary>
        public byte Layers;
        /// <summary>Byte 12, bits 3 to 0 Reserved</summary>
        public byte Reserved2;
        /// <summary>Byte 13, bits 7 to 6 DVD layer</summary>
        public HybridLayer DvdLayer;
        /// <summary>Byte 13, bits 5 to 4 CD layer</summary>
        public HybridLayer CdLayer;
        /// <summary>Byte 13, bits 3 to 0 Channel length</summary>
        public ChannelLength ChannelLength;
        /// <summary>Byte 14 Polarity</summary>
        public byte Polarity;
        /// <summary>Byte 15 Recorded polarity</summary>
        public byte RecordedPolarity;
        /// <summary>Byte 16, bits 7 to 4 Reserved</summary>
        public byte Reserved3;
        /// <summary>Byte 16, bits 3 to 0 If 0 no BCA, if 1 BCA, rest not defined</summary>
        public byte Bca;
        /// <summary>Byte 17 Maximum transfer speed in megabits/second, 0 if no maximum</summary>
        public byte MaxTransfer;
        /// <summary>Bytes 18 to 19 Reserved</summary>
        public ushort Reserved4;
        /// <summary>Bytes 20 to 23 Last user data PSN for disc</summary>
        public uint LastPsn;
        /// <summary>Bytes 24 to 27 First address unit number of data zone in this layer</summary>
        public uint FirstAun;
        /// <summary>Bytes 28 to 31 Last address unit number of data zone in this layer</summary>
        public uint LastAun;
        /// <summary>
        ///     Bytes 32 to 63 for BD-ROM, bytes 32 to 99 for BD-R/-RE Format dependent contents, disclosed in private blu-ray
        ///     specifications
        /// </summary>
        public byte[] FormatDependentContents;
        /// <summary>Bytes 100 to 105, BD-R/-RE only Manufacturer ID</summary>
        public byte[] ManufacturerID;
        /// <summary>Bytes 106 to 108, BD-R/-RE only Media type ID</summary>
        public byte[] MediaTypeID;
        /// <summary>Bytes 109 to 110, BD-R/-RE only Timestamp</summary>
        public ushort TimeStamp;
        /// <summary>Byte 111 Product revision number</summary>
        public byte ProductRevisionNumber;
    }
    #endregion Public structures
}