// /***************************************************************************
// Aaru Data Preservation Suite
// ----------------------------------------------------------------------------
//
// Filename       : System3740.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes IBM System 3740 floppy structures.
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

/// <summary>Methods and structures for IBM System 3740 floppy decoding</summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "MemberCanBeInternal")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedMember.Global")]
[SuppressMessage("ReSharper", "UnusedType.Global")]
public static class System3740
{
#region Nested type: AddressMark

    /// <summary>Sector address mark for IBM System 3740 floppies, contains sync word</summary>
    public struct AddressMark
    {
        /// <summary>6 bytes set to 0</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] zero;
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
        /// <summary>CRC16 from <see cref="type" /> to end of <see cref="sectorSize" /></summary>
        public ushort crc;
    }

#endregion

#region Nested type: DataBlock

    /// <summary>Sector data block for IBM System 3740 floppies</summary>
    public struct DataBlock
    {
        /// <summary>12 bytes set to 0</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
        public byte[] zero;
        /// <summary>Set to <see cref="IBMIdType.DataMark" /> or to <see cref="IBMIdType.DeletedDataMark" /></summary>
        public IBMIdType type;
        /// <summary>User data</summary>
        public byte[] data;
        /// <summary>CRC16 from <see cref="type" /> to end of <see cref="data" /></summary>
        public ushort crc;
    }

#endregion

#region Nested type: Sector

    /// <summary>Raw demodulated format for IBM System 3740 floppies</summary>
    public struct Sector
    {
        /// <summary>Sector address mark</summary>
        public AddressMark addressMark;
        /// <summary>11 bytes set to 0xFF</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
        public byte[] innerGap;
        /// <summary>Sector data block</summary>
        public DataBlock dataBlock;
        /// <summary>Variable bytes set to 0xFF</summary>
        public byte[] outerGap;
    }

#endregion

#region Nested type: Track

    /// <summary>Track format for IBM System 3740 floppy</summary>
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

    /// <summary>Start of IBM PC FM floppy track</summary>
    public struct TrackPreamble
    {
        /// <summary>Gap from index pulse, 80 bytes set to 0xFF</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
        public byte[] gap;
        /// <summary>6 bytes set to 0x00</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
        public byte[] zero;
        /// <summary>Set to <see cref="IBMIdType.IndexMark" /></summary>
        public IBMIdType type;
        /// <summary>Gap until first sector, 26 bytes to 0xFF</summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
        public byte[] gap1;
    }

#endregion
}