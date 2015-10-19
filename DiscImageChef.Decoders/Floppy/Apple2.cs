// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Apple2.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Apple ][ floppy decoder
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Methods and structures for Apple ][ floppy decoding
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
    /// <summary>
    /// Methods and structures for Apple ][ floppy decoding
    /// </summary>
    public static class Apple2
    {
        /// <summary>
        /// GCR-encoded Apple ][ GCR floppy track
        /// </summary>
        public struct AppleOldGCRRawSectorRawTrack
        {
            /// <summary>
            /// Track preamble, set to self-sync 0xFF, between 40 and 95 bytes
            /// </summary>
            public byte[] gap;
            public AppleOldGCRRawSector[] sectors;
        }

        /// <summary>
        /// GCR-encoded Apple ][ GCR floppy sector
        /// </summary>
        public struct AppleOldGCRRawSector
        {
            /// <summary>
            /// Address field
            /// </summary>
            public AppleOldGCRRawAddressField addressField;
            /// <summary>
            /// Track preamble, set to self-sync 0xFF, between 5 and 10 bytes
            /// </summary>
            public byte[] innerGap;
            /// <summary>
            /// Data field
            /// </summary>
            public AppleOldGCRRawDataField dataField;
            /// <summary>
            /// Track preamble, set to self-sync 0xFF, between 14 and 24 bytes
            /// </summary>
            public byte[] gap;
        }

        /// <summary>
        /// GCR-encoded Apple ][ GCR floppy sector address field
        /// </summary>
        public struct AppleOldGCRRawAddressField
        {
            /// <summary>
            /// Always 0xD5, 0xAA, 0x96
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] prologue;
            /// <summary>
            /// Volume number encoded as:
            /// volume[0] = (decodedVolume >> 1) | 0xAA
            /// volume[1] = decodedVolume | 0xAA
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] volume;
            /// <summary>
            /// Track number encoded as:
            /// track[0] = (decodedTrack >> 1) | 0xAA
            /// track[1] = decodedTrack | 0xAA
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] track;
            /// <summary>
            /// Sector number encoded as:
            /// sector[0] = (decodedSector >> 1) | 0xAA
            /// sector[1] = decodedSector | 0xAA
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] sector;
            /// <summary>
            /// decodedChecksum = decodedVolume ^ decodedTrack ^ decodedSector
            /// checksum[0] = (decodedChecksum >> 1) | 0xAA
            /// checksum[1] = decodedChecksum | 0xAA
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] checksum;
            /// <summary>
            /// Always 0xDE, 0xAA, 0xEB
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] epilogue;
        }

        /// <summary>
        /// GCR-encoded Apple ][ GCR floppy sector data field
        /// </summary>
        public struct AppleOldGCRRawDataField
        {
            /// <summary>
            /// Always 0xD5, 0xAA, 0xAD
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] prologue;
            /// <summary>
            /// Encoded data bytes.
            /// 410 bytes for 5to3 (aka DOS 3.2) format
            /// 342 bytes for 6to2 (aka DOS 3.3) format
            /// </summary>
            public byte[] data;
            public byte checksum;
            /// <summary>
            /// Always 0xDE, 0xAA, 0xEB
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] epilogue;
        }
    }
}

