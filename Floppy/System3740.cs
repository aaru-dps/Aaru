// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : System3740.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : IBM System 3740 floppy decoder
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Methods and structures for IBM System 3740 floppy decoding
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

namespace DiscImageChef.Decoders.Floppy
{
    // Information from:
    // National Semiconductor PC87332VLJ datasheet
    // SMsC FDC37C78 datasheet
    // Intel 82078 datasheet
    // Intel 82077AA datasheet
    // Toshiba TC8566AF datasheet
    // Fujitsu MB8876A datasheet
    // ECMA-147
    // ECMA-100

    /// <summary>
    /// Methods and structures for IBM System 3740 floppy decoding
    /// </summary>
    public static class System3740
    {
        /// <summary>
        /// Track format for IBM System 3740 floppy
        /// </summary>
        public struct IBMFMTrack
        {
            /// <summary>
            /// Start of track
            /// </summary>
            public IBMFMTrackPreamble trackStart;
            /// <summary>
            /// Track sectors
            /// </summary>
            public IBMFMSector[] sectors;
            /// <summary>
            /// Undefined size
            /// </summary>
            public byte[] gap;
        }

        /// <summary>
        /// Start of IBM PC FM floppy track
        /// </summary>
        public struct IBMFMTrackPreamble
        {
            /// <summary>
            /// Gap from index pulse, 80 bytes set to 0xFF
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
            public byte[] gap;
            /// <summary>
            /// 6 bytes set to 0x00
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] zero;
            /// <summary>
            /// Set to <see cref="IBMIdType.IndexMark"/> 
            /// </summary>
            public IBMIdType type;
            /// <summary>
            /// Gap until first sector, 26 bytes to 0xFF
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 26)]
            public byte[] gap1;
        }

        /// <summary>
        /// Raw demodulated format for IBM System 3740 floppies
        /// </summary>
        public struct IBMFMSector
        {
            /// <summary>
            /// Sector address mark
            /// </summary>
            public IBMFMSectorAddressMark addressMark;
            /// <summary>
            /// 11 bytes set to 0xFF
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 11)]
            public byte[] innerGap;
            /// <summary>
            /// Sector data block
            /// </summary>
            public IBMFMSectorAddressMark dataBlock;
            /// <summary>
            /// Variable bytes set to 0xFF
            /// </summary>
            public byte[] outerGap;
        }

        /// <summary>
        /// Sector address mark for IBM System 3740 floppies, contains sync word
        /// </summary>
        public struct IBMFMSectorAddressMark
        {
            /// <summary>
            /// 6 bytes set to 0
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public byte[] zero;
            /// <summary>
            /// Set to <see cref="IBMIdType.AddressMark"/>
            /// </summary>
            public IBMIdType type;
            /// <summary>
            /// Track number
            /// </summary>
            public byte track;
            /// <summary>
            /// Side number
            /// </summary>
            public byte side;
            /// <summary>
            /// Sector number
            /// </summary>
            public byte sector;
            /// <summary>
            /// <see cref="IBMSectorSizeCode"/> 
            /// </summary>
            public IBMSectorSizeCode sectorSize;
            /// <summary>
            /// CRC16 from <see cref="type"/> to end of <see cref="sectorSize"/> 
            /// </summary>
            public UInt16 crc;
        }

        /// <summary>
        /// Sector data block for IBM System 3740 floppies
        /// </summary>
        public struct IBMFMSectorDataBlock
        {
            /// <summary>
            /// 12 bytes set to 0
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] zero;
            /// <summary>
            /// Set to <see cref="IBMIdType.DataMark"/> or to <see cref="IBMIdType.DeletedDataMark"/>
            /// </summary>
            public IBMIdType type;
            /// <summary>
            /// User data
            /// </summary>
            public byte[] data;
            /// <summary>
            /// CRC16 from <see cref="type"/> to end of <see cref="data"/> 
            /// </summary>
            public UInt16 crc;
        }
    }
}

