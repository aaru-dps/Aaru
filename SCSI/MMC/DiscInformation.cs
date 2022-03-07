// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : DiscInformation.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes MMC GET DISC INFORMATION structures.
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

namespace Aaru.Decoders.SCSI.MMC;

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;

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
public static class DiscInformation
{
    public static StandardDiscInformation? Decode000b(byte[] response)
    {
        if(response.Length < 32)
            return null;

        if((response[2] & 0xE0) != 0)
            return null;

        var decoded = new StandardDiscInformation
        {
            DataLength = (ushort)((response[0] << 8) + response[1])
        };

        if(decoded.DataLength + 2 != response.Length)
            return null;

        decoded.DataType              =  (byte)((response[2] & 0xE0) >> 5);
        decoded.Erasable              |= (response[2] & 0x10) == 0x10;
        decoded.LastSessionStatus     =  (byte)((response[2] & 0x0C) >> 2);
        decoded.DiscStatus            =  (byte)(response[2] & 0x03);
        decoded.FirstTrackNumber      =  response[3];
        decoded.Sessions              =  (ushort)((response[9]  << 8) + response[4]);
        decoded.FirstTrackLastSession =  (ushort)((response[10] << 8) + response[5]);
        decoded.LastTrackLastSession  =  (ushort)((response[11] << 8) + response[6]);

        decoded.DID_V          |= (response[7]       & 0x80) == 0x80;
        decoded.DBC_V          |= (response[7]       & 0x40) == 0x40;
        decoded.URU            |= (response[7]       & 0x20) == 0x20;
        decoded.DAC_V          |= (response[7]       & 0x10) == 0x10;
        decoded.Reserved       |= (response[7]       & 0x08) == 0x08;
        decoded.Dbit           |= (response[7]       & 0x04) == 0x04;
        decoded.BGFormatStatus =  (byte)(response[7] & 0x03);

        decoded.DiscIdentification =
            (uint)((response[12] << 24) + (response[13] << 16) + (response[14] << 8) + response[15]);

        decoded.LastSessionLeadInStartLBA =
            (uint)((response[16] << 24) + (response[17] << 16) + (response[18] << 8) + response[19]);

        decoded.LastPossibleLeadOutStartLBA =
            (uint)((response[20] << 24) + (response[21] << 16) + (response[22] << 8) + response[23]);

        var temp = new byte[8];
        Array.Copy(response, 24, temp, 0, 8);
        Array.Reverse(temp);
        decoded.DiscBarcode = BitConverter.ToUInt64(temp, 0);

        if(response.Length < 34)
            return null;

        decoded.DiscApplicationCode = response[32];
        decoded.OPCTablesNumber     = response[33];

        if(decoded.OPCTablesNumber <= 0 ||
           response.Length         != decoded.OPCTablesNumber * 8 + 34)
            return decoded;

        decoded.OPCTables = new OPCTable[decoded.OPCTablesNumber];

        for(var i = 0; i < decoded.OPCTablesNumber; i++)
        {
            decoded.OPCTables[i].Speed = (ushort)((response[34 + i * 8 + 0] << 16) + response[34 + i * 8 + 1]);

            decoded.OPCTables[i].OPCValues = new byte[6];
            Array.Copy(response, 34 + i * 8 + 2, decoded.OPCTables[i].OPCValues, 0, 6);
        }

        return decoded;
    }

    public static string Prettify000b(StandardDiscInformation? information)
    {
        if(information?.DataType != 0)
            return null;

        var sb = new StringBuilder();

        switch(information.Value.DiscType)
        {
            case 0x00:
                sb.AppendLine("Disc type declared as CD-DA or CD-ROM");

                break;
            case 0x10:
                sb.AppendLine("Disc type declared as CD-i");

                break;
            case 0x20:
                sb.AppendLine("Disc type declared as CD-ROM XA");

                break;
            case 0xFF:
                sb.AppendLine("Disc type is undefined");

                break;
            default:
                sb.AppendFormat("Unknown disc type {0:X2}h", information.Value.DiscType).AppendLine();

                break;
        }

        switch(information.Value.DiscStatus)
        {
            case 0:
                sb.AppendLine("Disc is empty");

                break;
            case 1:
                sb.AppendLine("Disc is incomplete");

                break;
            case 2:
                sb.AppendLine("Disc is finalized");

                break;
        }

        if(information.Value.Erasable)
            sb.AppendLine("Disc is erasable");

        switch(information.Value.LastSessionStatus)
        {
            case 0:
                sb.AppendLine("Last session is empty");

                break;
            case 1:
                sb.AppendLine("Last session is incomplete");

                break;
            case 2:
                sb.AppendLine("Last session is damaged");

                break;
            case 3:
                sb.AppendLine("Last session is complete");

                break;
        }

        switch(information.Value.BGFormatStatus)
        {
            case 1:
                sb.AppendLine("Media was being formatted in the background but it is stopped and incomplete");

                break;
            case 2:
                sb.AppendLine("Media is currently being formatted in the background");

                break;
            case 3:
                sb.AppendLine("Media background formatting has completed");

                break;
        }

        if(information.Value.Dbit)
            sb.AppendLine("MRW is dirty");

        sb.AppendFormat("First track on disc is track {0}", information.Value.FirstTrackNumber).AppendLine();
        sb.AppendFormat("Disc has {0} sessions", information.Value.Sessions).AppendLine();

        sb.AppendFormat("First track in last session is track {0}", information.Value.FirstTrackLastSession).
           AppendLine();

        sb.AppendFormat("Last track in last session is track {0}", information.Value.LastTrackLastSession).AppendLine();

        sb.AppendFormat("Last session Lead-In address is {0} (as LBA) or {1:X2}:{2:X2}:{3:X2}",
                        information.Value.LastSessionLeadInStartLBA,
                        (information.Value.LastSessionLeadInStartLBA & 0xFF0000) >> 16,
                        (information.Value.LastSessionLeadInStartLBA & 0xFF00)   >> 8,
                        information.Value.LastSessionLeadInStartLBA & 0xFF).AppendLine();

        sb.AppendFormat("Last possible Lead-Out address is {0} (as LBA) or {1:X2}:{2:X2}:{3:X2}",
                        information.Value.LastPossibleLeadOutStartLBA,
                        (information.Value.LastPossibleLeadOutStartLBA & 0xFF0000) >> 16,
                        (information.Value.LastPossibleLeadOutStartLBA & 0xFF00)   >> 8,
                        information.Value.LastPossibleLeadOutStartLBA & 0xFF).AppendLine();

        sb.AppendLine(information.Value.URU ? "Disc is defined for unrestricted use"
                          : "Disc is defined for restricted use");

        if(information.Value.DID_V)
            sb.AppendFormat("Disc ID: {0:X6}", information.Value.DiscIdentification & 0x00FFFFFF).AppendLine();

        if(information.Value.DBC_V)
            sb.AppendFormat("Disc barcode: {0:X16}", information.Value.DiscBarcode).AppendLine();

        if(information.Value.DAC_V)
            sb.AppendFormat("Disc application code: {0}", information.Value.DiscApplicationCode).AppendLine();

        if(information.Value.OPCTables == null)
            return sb.ToString();

        foreach(OPCTable table in information.Value.OPCTables)
            sb.AppendFormat("OPC values for {0}Kbit/sec.: {1}, {2}, {3}, {4}, {5}, {6}", table.Speed,
                            table.OPCValues[0], table.OPCValues[1], table.OPCValues[2], table.OPCValues[3],
                            table.OPCValues[4], table.OPCValues[5]).AppendLine();

        return sb.ToString();
    }

    public static TrackResourcesInformation? Decode001b(byte[] response)
    {
        if(response.Length != 12)
            return null;

        if((response[2] & 0xE0) != 0x20)
            return null;

        var decoded = new TrackResourcesInformation
        {
            DataLength = (ushort)((response[0] << 8) + response[1])
        };

        if(decoded.DataLength + 2 != response.Length)
            return null;

        decoded.DataType            = (byte)((response[2] & 0xE0) >> 5);
        decoded.MaxTracks           = (ushort)((response[4]  << 8) + response[5]);
        decoded.AssignedTracks      = (ushort)((response[6]  << 8) + response[7]);
        decoded.MaxAppendableTracks = (ushort)((response[8]  << 8) + response[9]);
        decoded.AppendableTracks    = (ushort)((response[10] << 8) + response[11]);

        return decoded;
    }

    public static string Prettify001b(TrackResourcesInformation? information)
    {
        if(information?.DataType != 1)
            return null;

        var sb = new StringBuilder();

        sb.AppendFormat("{0} maximum possible tracks on the disc", information.Value.MaxTracks).AppendLine();
        sb.AppendFormat("{0} assigned tracks on the disc", information.Value.AssignedTracks).AppendLine();

        sb.AppendFormat("{0} maximum possible appendable tracks on the disc", information.Value.AppendableTracks).
           AppendLine();

        sb.AppendFormat("{0} current appendable tracks on the disc", information.Value.MaxAppendableTracks).
           AppendLine();

        return sb.ToString();
    }

    public static POWResourcesInformation? Decode010b(byte[] response)
    {
        if(response.Length != 16)
            return null;

        if((response[2] & 0xE0) != 0x40)
            return null;

        var decoded = new POWResourcesInformation
        {
            DataLength = (ushort)((response[0] << 8) + response[1])
        };

        if(decoded.DataLength + 2 != response.Length)
            return null;

        decoded.DataType = (byte)((response[2] & 0xE0) >> 5);

        decoded.RemainingPOWReplacements =
            (ushort)((response[4] << 24) + (response[5] << 16) + (response[6] << 8) + response[7]);

        decoded.RemainingPOWReallocation =
            (ushort)((response[8] << 24) + (response[9] << 16) + (response[10] << 8) + response[11]);

        decoded.RemainingPOWUpdates =
            (ushort)((response[12] << 24) + (response[13] << 16) + (response[14] << 8) + response[15]);

        return decoded;
    }

    public static string Prettify010b(POWResourcesInformation? information)
    {
        if(information?.DataType != 1)
            return null;

        var sb = new StringBuilder();

        sb.AppendFormat("{0} remaining POW replacements", information.Value.RemainingPOWReplacements).AppendLine();

        sb.AppendFormat("{0} remaining POW reallocation map entries", information.Value.RemainingPOWReallocation).
           AppendLine();

        sb.AppendFormat("{0} remaining POW updates", information.Value.RemainingPOWUpdates).AppendLine();

        return sb.ToString();
    }

    public static string Prettify(byte[] response)
    {
        if(response == null)
            return null;

        if(response.Length < 12)
            return null;

        switch(response[2] & 0xE0)
        {
            case 0x00: return Prettify000b(Decode000b(response));
            case 0x20: return Prettify001b(Decode001b(response));
            case 0x40: return Prettify010b(Decode010b(response));
        }

        return null;
    }

    public struct StandardDiscInformation
    {
        /// <summary>Bytes 0 to 1 32 + OPCTablesNumber*8</summary>
        public ushort DataLength;
        /// <summary>Byte 2, bits 7 to 5 000b</summary>
        public byte DataType;
        /// <summary>Byte 2, bit 4 If set, disc is erasable</summary>
        public bool Erasable;
        /// <summary>Byte 2, bits 3 to 2 Status of last session</summary>
        public byte LastSessionStatus;
        /// <summary>Byte 2, bits 1 to 0 Status of disc</summary>
        public byte DiscStatus;
        /// <summary>Byte 3 Number of logical track that contains LBA 0</summary>
        public byte FirstTrackNumber;
        /// <summary>Byte 9 (MSB) and byte 4 (LSB) Number of sessions</summary>
        public ushort Sessions;
        /// <summary>Byte 10 (MSB) and byte 5 (LSB) Number of first track in last session</summary>
        public ushort FirstTrackLastSession;
        /// <summary>Byte 11 (MSB) and byte 6 (LSB) Number of last track in last session</summary>
        public ushort LastTrackLastSession;
        /// <summary>Byte 7, bit 7 If set, DiscIdentification is valid</summary>
        public bool DID_V;
        /// <summary>Byte 7, bit 6 If set, DiscBarcode is valid</summary>
        public bool DBC_V;
        /// <summary>Byte 7, bit 5 If set, disc is unrestricted</summary>
        public bool URU;
        /// <summary>Byte 7, bit 4 If set DiscApplicationCode is valid</summary>
        public bool DAC_V;
        /// <summary>Byte 7, bit 3 Reserved</summary>
        public bool Reserved;
        /// <summary>Byte 7, bit 2 Copy of dirty bit from MRW status</summary>
        public bool Dbit;
        /// <summary>Byte 7, bits 1 to 0 Background format status</summary>
        public byte BGFormatStatus;
        /// <summary>Byte 8 Disc type code</summary>
        public byte DiscType;
        /// <summary>Bytes 12 to 15 Disc identification number from PMA</summary>
        public uint DiscIdentification;
        /// <summary>Bytes 16 to 19 Last Session Lead-in Start Address (MSF for CD, LBA for others)</summary>
        public uint LastSessionLeadInStartLBA;
        /// <summary>Bytes 20 to 23 Last Possible Lead-out Start Address (MSF for CD, LBA for others)</summary>
        public uint LastPossibleLeadOutStartLBA;
        /// <summary>Bytes 24 to 31 Disc barcode</summary>
        public ulong DiscBarcode;
        /// <summary>Byte 32 Disc application code</summary>
        public byte DiscApplicationCode;
        /// <summary>Byte 33 How many OPC tables are</summary>
        public byte OPCTablesNumber;
        /// <summary>Bytes 34 to end OPC tables (8 bytes each)</summary>
        public OPCTable[] OPCTables;
    }

    public struct OPCTable
    {
        /// <summary>Bytes 0 to 1 kilobytes/sec this OPC table applies to</summary>
        public ushort Speed;
        /// <summary>Bytes 2 to 7 OPC values</summary>
        public byte[] OPCValues;
    }

    public struct TrackResourcesInformation
    {
        /// <summary>Bytes 0 to 1 10</summary>
        public ushort DataLength;
        /// <summary>Byte 2, bits 7 to 5 001b</summary>
        public byte DataType;
        /// <summary>Byte 2, bits 4 to 0 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Bytes 4 to 5 Maximum possible number of the tracks on the disc</summary>
        public ushort MaxTracks;
        /// <summary>Bytes 6 to 7 Number of the assigned tracks on the disc</summary>
        public ushort AssignedTracks;
        /// <summary>Bytes 8 to 9 Maximum possible number of appendable tracks on the disc</summary>
        public ushort MaxAppendableTracks;
        /// <summary>Bytes 10 to 11 Current number of appendable tracks on the disc</summary>
        public ushort AppendableTracks;
    }

    public struct POWResourcesInformation
    {
        /// <summary>Bytes 0 to 1 14</summary>
        public ushort DataLength;
        /// <summary>Byte 2, bits 7 to 5 010b</summary>
        public byte DataType;
        /// <summary>Byte 2, bits 4 to 0 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Bytes 4 to 7 Remaining POW replacements</summary>
        public uint RemainingPOWReplacements;
        /// <summary>Bytes 8 to 11 Remaining POW reallocation map entries</summary>
        public uint RemainingPOWReallocation;
        /// <summary>Bytes 12 to 15 Number of remaining POW updates</summary>
        public uint RemainingPOWUpdates;
    }
}