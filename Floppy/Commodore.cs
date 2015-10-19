// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Commodore.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Commodore GCR floppy decoder
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Methods and structures for Commodore GCR floppy decoding
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
    /// Methods and structures for Commodore GCR floppy decoding
    /// </summary>
    public static class Commodore
    {
        /// <summary>
        /// Decoded Commodore GCR sector header
        /// </summary>
        public struct CommodoreSectorHeader
        {
            /// <summary>
            /// Always 0x08
            /// </summary>
            public byte id;
            /// <summary>
            /// XOR of following fields
            /// </summary>
            public byte checksum;
            /// <summary>
            /// Sector number
            /// </summary>
            public byte sector;
            /// <summary>
            /// Track number
            /// </summary>
            public byte track;
            /// <summary>
            /// Format ID, unknown meaning
            /// </summary>
            public UInt16 format;
            /// <summary>
            /// Filled with 0x0F
            /// </summary>
            public UInt16 fill;
        }

        /// <summary>
        /// Decoded Commodore GCR sector data
        /// </summary>
        public struct CommodoreSectorData
        {
            /// <summary>
            /// Always 0x07
            /// </summary>
            public byte id;
            /// <summary>
            /// User data
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte data;
            /// <summary>
            /// XOR of <see cref="data"/>
            /// </summary>
            public byte checksum;
            /// <summary>
            /// Filled with 0x0F
            /// </summary>
            public UInt16 fill;
        }
    }
}

