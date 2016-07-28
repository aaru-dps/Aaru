// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : AppleSony.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : Device structures decoders.
//
// --[ Description ] ----------------------------------------------------------
//
//     Decodes Apple/Sony floppy structures.
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
// Copyright © 2011-2016 Natalia Portillo
// ****************************************************************************/

using System.Runtime.InteropServices;

namespace DiscImageChef.Decoders.Floppy
{
    // Information from:
    // Inside Macintosh, Volume II, ISBN 0-201-17732-3

    /// <summary>
    /// Methods and structures for Apple Sony GCR floppy decoding
    /// </summary>
    public static class AppleSony
    {
        /// <summary>
        /// GCR-encoded Apple Sony GCR floppy track
        /// </summary>
        public struct RawTrack
        {
            /// <summary>
            /// Track preamble, set to self-sync 0xFF, 36 bytes
            /// </summary>
            public byte[] gap;
            public RawSector[] sectors;
        }

        /// <summary>
        /// GCR-encoded Apple Sony GCR floppy sector
        /// </summary>
        public struct RawSector
        {
            /// <summary>
            /// Address field
            /// </summary>
            public RawAddressField addressField;
            /// <summary>
            /// Track preamble, set to self-sync 0xFF, 6 bytes
            /// </summary>
            public byte[] innerGap;
            /// <summary>
            /// Data field
            /// </summary>
            public RawDataField dataField;
            /// <summary>
            /// Track preamble, set to self-sync 0xFF, unknown size
            /// </summary>
            public byte[] gap;
        }

        /// <summary>
        /// GCR-encoded Apple Sony GCR floppy sector address field
        /// </summary>
        public struct RawAddressField
        {
            /// <summary>
            /// Always 0xD5, 0xAA, 0x96
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] prologue;
            /// <summary>
            /// Encoded (decodedTrack &amp; 0x3F)
            /// </summary>
            public byte track;
            /// <summary>
            /// Encoded sector number
            /// </summary>
            public byte sector;
            /// <summary>
            /// Encoded side number
            /// </summary>
            public byte side;
            /// <summary>
            /// Disk format
            /// </summary>
            public AppleEncodedFormat format;
            /// <summary>
            /// Checksum
            /// </summary>
            public byte checksum;
            /// <summary>
            /// Always 0xDE, 0xAA
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] epilogue;
        }

        /// <summary>
        /// GCR-encoded Apple ][ GCR floppy sector data field
        /// </summary>
        public struct RawDataField
        {
            /// <summary>
            /// Always 0xD5, 0xAA, 0xAD
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] prologue;
            /// <summary>
            /// Spare, usually <see cref="RawAddressField.sector"/> 
            /// </summary>
            public byte spare;
            /// <summary>
            /// Encoded data bytes.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 698)]
            public byte[] data;
            /// <summary>
            /// Checksum
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public byte[] checksum;
            /// <summary>
            /// Always 0xDE, 0xAA
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] epilogue;
        }
    }
}

