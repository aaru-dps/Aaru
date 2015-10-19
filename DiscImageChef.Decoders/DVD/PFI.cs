// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : PFI.cs
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

namespace DiscImageChef.Decoders.DVD
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
    /// ECMA 365
    /// </summary>
    public static class PFI
    {
        public struct PhysicalFormatInformation
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
            /// Byte 4, bits 7 to 4
            /// Disk category field
            /// </summary>
            public byte DiskCategory;
            /// <summary>
            /// Byte 4, bits 3 to 0
            /// Media version
            /// </summary>
            public byte PartVersion;
            /// <summary>
            /// Byte 5, bits 7 to 4
            /// 120mm if 0, 80mm if 1. If UMD (60mm) 0 also. Reserved rest of values
            /// </summary>
            public byte DiscSize;
            /// <summary>
            /// Byte 5, bits 3 to 0
            /// Maximum data rate
            /// </summary>
            public byte MaximumRate;
            /// <summary>
            /// Byte 6, bit 7
            /// Reserved
            /// </summary>
            public bool Reserved3;
            /// <summary>
            /// Byte 6, bits 6 to 5
            /// Number of layers
            /// </summary>
            public byte Layers;
            /// <summary>
            /// Byte 6, bit 4
            /// Track path
            /// </summary>
            public bool TrackPath;
            /// <summary>
            /// Byte 6, bits 3 to 0
            /// Layer type
            /// </summary>
            public byte LayerType;
            /// <summary>
            /// Byte 7, bits 7 to 4
            /// Linear density field
            /// </summary>
            public byte LinearDensity;
            /// <summary>
            /// Byte 7, bits 3 to 0
            /// Track density field
            /// </summary>
            public byte TrackDensity;
            /// <summary>
            /// Bytes 8 to 11
            /// PSN where Data Area starts
            /// </summary>
            public UInt32 DataAreaStartPSN;
            /// <summary>
            /// Bytes 12 to 15
            /// PSN where Data Area ends
            /// </summary>
            public UInt32 DataAreaEndPSN;
            /// <summary>
            /// Bytes 16 to 19
            /// PSN where Data Area ends in Layer 0
            /// </summary>
            public UInt32 Layer0EndPSN;
            /// <summary>
            /// Byte 20, bit 7
            /// True if BCA exists. GC/Wii discs do not have this bit set, but there is a BCA, making it unreadable in normal DVD drives
            /// </summary>
            public bool BCA;
            /// <summary>
            /// Byte 20, bits 6 to 0
            /// Reserved
            /// </summary>
            public byte Reserved4;
            /// <summary>
            /// Bytes 21 to 22
            /// UMD only, media attribute, application-defined, part of media specific in rest of discs
            /// </summary>
            public UInt16 MediaAttribute;
            /// <summary>
            /// Bytes 21 to 2051, set to zeroes in UMD (at least according to ECMA).
            /// Media specific
            /// </summary>
            public byte[] MediaSpecific;
        }

        public struct PhysicalFormatInformationForWritables
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
            /// Byte 4, bits 7 to 4
            /// Disk category field
            /// </summary>
            public byte DiskCategory;
            /// <summary>
            /// Byte 4, bits 3 to 0
            /// Media version
            /// </summary>
            public byte PartVersion;
            /// <summary>
            /// Byte 5, bits 7 to 4
            /// 120mm if 0, 80mm if 1
            /// </summary>
            public byte DiscSize;
            /// <summary>
            /// Byte 5, bits 3 to 0
            /// Maximum data rate
            /// </summary>
            public byte MaximumRate;
            /// <summary>
            /// Byte 6, bit 7
            /// Reserved
            /// </summary>
            public bool Reserved3;
            /// <summary>
            /// Byte 6, bits 6 to 5
            /// Number of layers
            /// </summary>
            public byte Layers;
            /// <summary>
            /// Byte 6, bit 4
            /// Track path
            /// </summary>
            public bool TrackPath;
            /// <summary>
            /// Byte 6, bits 3 to 0
            /// Layer type
            /// </summary>
            public byte LayerType;
            /// <summary>
            /// Byte 7, bits 7 to 4
            /// Linear density field
            /// </summary>
            public byte LinearDensity;
            /// <summary>
            /// Byte 7, bits 3 to 0
            /// Track density field
            /// </summary>
            public byte TrackDensity;
            /// <summary>
            /// Bytes 8 to 11
            /// PSN where Data Area starts
            /// </summary>
            public UInt32 DataAreaStartPSN;
            /// <summary>
            /// Bytes 12 to 15
            /// PSN where Data Area ends
            /// </summary>
            public UInt32 DataAreaEndPSN;
            /// <summary>
            /// Bytes 16 to 19
            /// PSN where Data Area ends in Layer 0
            /// </summary>
            public UInt32 Layer0EndPSN;
            /// <summary>
            /// Byte 20, bit 7
            /// True if BCA exists
            /// </summary>
            public bool BCA;
            /// <summary>
            /// Byte 20, bits 6 to 0
            /// Reserved
            /// </summary>
            public byte Reserved4;
            /// <summary>
            /// Bytes 21 to 2051
            /// Media specific, content defined in each specification
            /// </summary>
            public byte[] MediaSpecific;
        }
    }
}

