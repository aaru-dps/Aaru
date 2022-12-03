// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Layers.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Records DVD layers structures.
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

namespace Aaru.Decoders.DVD;

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
// ECMA 365
[SuppressMessage("ReSharper", "InconsistentNaming"), SuppressMessage("ReSharper", "MemberCanBeInternal"),
 SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public static class Layers
{
    public struct LayerCapacity
    {
        /// <summary>Bytes 0 to 1 Data length</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Byte 4, bit 7 If set, L0 capacity is immutable</summary>
        public bool InitStatus;
        /// <summary>Byte 4, bits 6 to 0 Reserved</summary>
        public byte Reserved3;
        /// <summary>Byte 5 Reserved</summary>
        public byte Reserved4;
        /// <summary>Byte 6 Reserved</summary>
        public byte Reserved5;
        /// <summary>Byte 7 Reserved</summary>
        public byte Reserved6;
        /// <summary>Byte 8 to 11 L0 Data Area Capacity</summary>
        public uint Capacity;
    }

    public struct MiddleZoneStartAddress
    {
        /// <summary>Bytes 0 to 1 Data length = 10</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Byte 4, bit 7 If set, L0 shifter middle area is immutable</summary>
        public bool InitStatus;
        /// <summary>Byte 4, bits 6 to 0 Reserved</summary>
        public byte Reserved3;
        /// <summary>Byte 5 Reserved</summary>
        public byte Reserved4;
        /// <summary>Byte 6 Reserved</summary>
        public byte Reserved5;
        /// <summary>Byte 7 Reserved</summary>
        public byte Reserved6;
        /// <summary>Byte 8 to 11 Start LBA of Shifted Middle Area on L0</summary>
        public uint ShiftedMiddleAreaStartAddress;
    }

    public struct JumpIntervalSize
    {
        /// <summary>Bytes 0 to 1 Data length = 10</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Byte 4 Reserved</summary>
        public byte Reserved3;
        /// <summary>Byte 5 Reserved</summary>
        public byte Reserved4;
        /// <summary>Byte 6 Reserved</summary>
        public byte Reserved5;
        /// <summary>Byte 7 Reserved</summary>
        public byte Reserved6;
        /// <summary>Byte 8 to 11 Jump Interval size for the Regular Interval Layer Jump</summary>
        public uint Size;
    }

    public struct ManualLayerJumpAddress
    {
        /// <summary>Bytes 0 to 1 Data length = 10</summary>
        public ushort DataLength;
        /// <summary>Byte 2 Reserved</summary>
        public byte Reserved1;
        /// <summary>Byte 3 Reserved</summary>
        public byte Reserved2;
        /// <summary>Byte 4 Reserved</summary>
        public byte Reserved3;
        /// <summary>Byte 5 Reserved</summary>
        public byte Reserved4;
        /// <summary>Byte 6 Reserved</summary>
        public byte Reserved5;
        /// <summary>Byte 7 Reserved</summary>
        public byte Reserved6;
        /// <summary>Byte 8 to 11 LBA for the manual layer jump</summary>
        public uint LBA;
    }
}