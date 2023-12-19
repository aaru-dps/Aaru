// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : Perpendicular.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes perpendicular recording floppy structures.
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
using System.Runtime.InteropServices;

namespace Aaru.Decoders.Floppy;

// Information from:
// National Semiconductor PC87332VLJ datasheet
// SMsC FDC37C78 datasheet
// Intel 82078 datasheet
// Intel 82077AA datasheet
// Toshiba TC8566AF datasheet
// Fujitsu MB8876A datasheet
// ECMA-147
// ECMA-100

/// <summary>Methods and structures for perpendicular MFM floppy decoding</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class Perpendicular
{
#region Nested type: AddressMark

    /// <summary>Sector address mark for IBM System 34 floppies, contains sync word</summary>
    public struct AddressMark
    {
        /// <summary>12 bytes set to 0</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] zero;
        /// <summary>3 bytes set to 0xA1</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] aone;
        /// <summary>Set to <see cref="IBMIdType.AddressMark" /></summary>
        public IBMIdType type;
        /// <summary>Track number</summary>
        public byte track;
        /// <summary>Side number</summary>
        public byte side;
        /// <summary>Sector number</summary>
        public byte sector;
        /// <summary>
        ///     <see cref="IBMSectorSizeCode" />
        /// </summary>
        public IBMSectorSizeCode sectorSize;
        /// <summary>CRC16 from <see cref="aone" /> to end of <see cref="sectorSize" /></summary>
        public ushort crc;
    }

#endregion

#region Nested type: DataBlock

    /// <summary>Sector data block for IBM System 34 floppies</summary>
    public struct DataBlock
    {
        /// <summary>12 bytes set to 0</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] zero;
        /// <summary>3 bytes set to 0xA1</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] aone;
        /// <summary>Set to <see cref="IBMIdType.DataMark" /> or to <see cref="IBMIdType.DeletedDataMark" /></summary>
        public IBMIdType type;
        /// <summary>User data</summary>
        public byte[] data;
        /// <summary>CRC16 from <see cref="aone" /> to end of <see cref="data" /></summary>
        public ushort crc;
    }

#endregion

#region Nested type: Sector

    /// <summary>Raw demodulated format for perpendicular floppies</summary>
    public struct Sector
    {
        /// <summary>Sector address mark</summary>
        public AddressMark addressMark;
        /// <summary>41 bytes set to 0x4E</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 41)]
        public byte[] innerGap;
        /// <summary>Sector data block</summary>
        public DataBlock dataBlock;
        /// <summary>Variable-sized inter-sector gap, ECMA defines 83 bytes</summary>
        public byte[] outerGap;
    }

#endregion

#region Nested type: Track

    /// <summary>Perpendicular floppy track</summary>
    public struct Track
    {
        /// <summary>Start of track</summary>
        public TrackPreamble trackStart;
        /// <summary>Track sectors</summary>
        public Sector[] sectors;
        /// <summary>Undefined size</summary>
        public byte[] gap;
    }

#endregion

#region Nested type: TrackPreamble

    /// <summary>Start of IBM PC MFM floppy track Used by IBM PC, Apple Macintosh (high-density only), and a lot others</summary>
    public struct TrackPreamble
    {
        /// <summary>Gap from index pulse, 80 bytes set to 0x4E</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
        public byte[] gap;
        /// <summary>12 bytes set to 0x00</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] zero;
        /// <summary>3 bytes set to 0xC2</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] ctwo;
        /// <summary>Set to <see cref="IBMIdType.IndexMark" /></summary>
        public IBMIdType type;
        /// <summary>Gap until first sector, 50 bytes to 0x4E</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
        public byte[] gap1;
    }

#endregion
}