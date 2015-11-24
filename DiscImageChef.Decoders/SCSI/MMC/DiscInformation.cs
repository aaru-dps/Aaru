// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : DiscInformation.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Component
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Description
//
// --[ License ] --------------------------------------------------------------
//
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as
//     published by the Free Software Foundation, either version 3 of the
//     License, or (at your option) any later version.
//
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
//
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright (C) 2011-2015 Claunia.com
// ****************************************************************************/
// //$Id$
using System;

namespace DiscImageChef.Decoders.SCSI.MMC
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
    public static class DiscInformation
    {
        public struct StandardDiscInformation
        {
            /// <summary>
            /// Bytes 0 to 1
            /// 32 + OPCTablesNumber*8
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2, bits 7 to 5
            /// 000b
            /// </summary>
            public byte DataType;
            /// <summary>
            /// Byte 2, bit 4
            /// If set, disc is erasable
            /// </summary>
            public bool Erasable;
            /// <summary>
            /// Byte 2, bits 3 to 2
            /// Status of last session
            /// </summary>
            public byte LastSessionStatus;
            /// <summary>
            /// Byte 2, bits 1 to 0
            /// Status of disc
            /// </summary>
            public byte DiscStatus;
            /// <summary>
            /// Byte 3
            /// Number of logical track that contains LBA 0
            /// </summary>
            public byte FirstTrackNumber;
            /// <summary>
            /// Byte 9 (MSB) and byte 4 (LSB)
            /// Number of sessions
            /// </summary>
            public byte Sessions;
            /// <summary>
            /// Byte 10 (MSB) and byte 5 (LSB)
            /// Number of first track in last session
            /// </summary>
            public byte FirstTrackLastSession;
            /// <summary>
            /// Byte 11 (MSB) and byte 6 (LSB)
            /// Number of last track in last session
            /// </summary>
            public byte LastTrackLastSession;
            /// <summary>
            /// Byte 7, bit 7
            /// If set, DiscIdentification is valid
            /// </summary>
            public bool DID_V;
            /// <summary>
            /// Byte 7, bit 6
            /// If set, DiscBarcode is valid
            /// </summary>
            public bool DBC_V;
            /// <summary>
            /// Byte 7, bit 5
            /// If set, disc is unrestricted
            /// </summary>
            public bool URU;
            /// <summary>
            /// Byte 7, bit 4
            /// If set DiscApplicationCode is valid
            /// </summary>
            public bool DAC_V;
            /// <summary>
            /// Byte 7, bit 3
            /// Reserved
            /// </summary>
            public bool Reserved;
            /// <summary>
            /// Byte 7, bit 2
            /// Copy of dirty bit from MRW status
            /// </summary>
            public bool Dbit;
            /// <summary>
            /// Byte 7, bits 1 to 0
            /// Background format status
            /// </summary>
            public byte BGFormatStatus;
            /// <summary>
            /// Byte 8
            /// Disc type code
            /// </summary>
            public byte DiscType;
            /// <summary>
            /// Bytes 12 to 15
            /// Disc identification number from PMA
            /// </summary>
            public UInt32 DiscIdentification;
            /// <summary>
            /// Bytes 16 to 19
            /// Last Session Lead-in Start Address (MSF for CD, LBA for others)
            /// </summary>
            public UInt32 LastSessionLeadInStartLBA;
            /// <summary>
            /// Bytes 20 to 23
            /// Last Possible Lead-out Start Address (MSF for CD, LBA for others)
            /// </summary>
            public UInt32 LastPossibleLeadOutStartLBA;
            /// <summary>
            /// Bytes 24 to 31
            /// Disc barcode
            /// </summary>
            public UInt64 DiscBarcode;
            /// <summary>
            /// Byte 32
            /// Disc application code
            /// </summary>
            public byte DiscApplicationCode;
            /// <summary>
            /// Byte 33
            /// How many OPC tables are
            /// </summary>
            public byte OPCTablesNumber;
            /// <summary>
            /// Bytes 34 to end
            /// OPC tables (8 bytes each)
            /// </summary>
            public OPCTable[] OPCTables;
        }

        public struct OPCTable
        {
            /// <summary>
            /// Bytes 0 to 1
            /// kilobytes/sec this OPC table applies to
            /// </summary>
            public UInt16 Speed;
            /// <summary>
            /// Bytes 2 to 7
            /// OPC values
            /// </summary>
            public byte[] OPCValues;
        }

        public struct TrackResourcesInformation
        {
            /// <summary>
            /// Bytes 0 to 1
            /// 10
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2, bits 7 to 5
            /// 001b
            /// </summary>
            public byte DataType;
            /// <summary>
            /// Byte 2, bits 4 to 0
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
            /// Maximum possible number of the tracks on the disc
            /// </summary>
            public UInt16 MaxTracks;
            /// <summary>
            /// Bytes 6 to 7
            /// Number of the assigned tracks on the disc
            /// </summary>
            public UInt16 AssignedTracks;
            /// <summary>
            /// Bytes 8 to 9
            /// Maximum possible number of appendable tracks on the disc
            /// </summary>
            public UInt16 MaxAppendableTracks;
            /// <summary>
            /// Bytes 10 to 11
            /// Current number of appendable tracks on the disc
            /// </summary>
            public UInt16 AppendableTracks;
        }

        public struct POWResourcesInformation
        {
            /// <summary>
            /// Bytes 0 to 1
            /// 14
            /// </summary>
            public UInt16 DataLength;
            /// <summary>
            /// Byte 2, bits 7 to 5
            /// 010b
            /// </summary>
            public byte DataType;
            /// <summary>
            /// Byte 2, bits 4 to 0
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
            /// Remaining POW replacements
            /// </summary>
            public UInt32 RemainingPOWReplacements;
            /// <summary>
            /// Bytes 8 to 11
            /// Remaining POW reallocation map entries
            /// </summary>
            public UInt32 RemainingPOWReallocation;
            /// <summary>
            /// Bytes 12 to 15
            /// Number of remaining POW updates
            /// </summary>
            public UInt32 RemainingPOWUpdates;
        }
    }
}

