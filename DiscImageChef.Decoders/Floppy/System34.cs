// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : System34.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : IBM System 34 floppy decoder
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Methods and structures for IBM System 34 floppy decoding
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
    /// Methods and structures for IBM System 34 floppy decoding
    /// </summary>
    public static class System34
    {
        /// <summary>
        /// Track format for IBM System 34 floppy
        /// Used by IBM PC, Apple Macintosh (high-density only), and a lot others
        /// </summary>
        public struct IBMMFMTrack
        {
            /// <summary>
            /// Start of track
            /// </summary>
            public IBMMFMTrackPreamble trackStart;
            /// <summary>
            /// Track sectors
            /// </summary>
            public IBMMFMSector[] sectors;
            /// <summary>
            /// Undefined size
            /// </summary>
            public byte[] gap;
        }

        /// <summary>
        /// Start of IBM PC MFM floppy track
        /// Used by IBM PC, Apple Macintosh (high-density only), and a lot others
        /// </summary>
        public struct IBMMFMTrackPreamble
        {
            /// <summary>
            /// Gap from index pulse, 80 bytes set to 0x4E
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
            public byte[] gap;
            /// <summary>
            /// 12 bytes set to 0x00
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] zero;
            /// <summary>
            /// 3 bytes set to 0xC2
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] ctwo;
            /// <summary>
            /// Set to <see cref="IBMIdType.IndexMark"/> 
            /// </summary>
            public IBMIdType type;
            /// <summary>
            /// Gap until first sector, 50 bytes to 0x4E
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 50)]
            public byte[] gap1;
        }

        /// <summary>
        /// Raw demodulated format for IBM System 34 floppies
        /// </summary>
        public struct IBMMFMSector
        {
            /// <summary>
            /// Sector address mark
            /// </summary>
            public IBMMFMSectorAddressMark addressMark;
            /// <summary>
            /// 22 bytes set to 0x4E, set to 0x22 on Commodore 1581
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 22)]
            public byte[] innerGap;
            /// <summary>
            /// Sector data block
            /// </summary>
            public IBMMFMSectorAddressMark dataBlock;
            /// <summary>
            /// Variable bytes set to 0x4E, ECMA defines 54
            /// </summary>
            public byte[] outerGap;
        }

        /// <summary>
        /// Sector address mark for IBM System 34 floppies, contains sync word
        /// </summary>
        public struct IBMMFMSectorAddressMark
        {
            /// <summary>
            /// 12 bytes set to 0
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] zero;
            /// <summary>
            /// 3 bytes set to 0xA1
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] aone;
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
            /// CRC16 from <see cref="IBMMFMSectorAddressMark.aone"/> to end of <see cref="sectorSize"/> 
            /// </summary>
            public UInt16 crc;
        }

        /// <summary>
        /// Sector data block for IBM System 34 floppies
        /// </summary>
        public struct IBMMFMSectorDataBlock
        {
            /// <summary>
            /// 12 bytes set to 0
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 12)]
            public byte[] zero;
            /// <summary>
            /// 3 bytes set to 0xA1
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] aone;
            /// <summary>
            /// Set to <see cref="IBMIdType.DataMark"/> or to <see cref="IBMIdType.DeletedDataMark"/>
            /// </summary>
            public IBMIdType type;
            /// <summary>
            /// User data
            /// </summary>
            public byte[] data;
            /// <summary>
            /// CRC16 from <see cref="aone"/> to end of <see cref="data"/> 
            /// </summary>
            public UInt16 crc;
        }
    }
}

