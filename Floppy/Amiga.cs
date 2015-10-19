﻿// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Amiga.cs
// Version        : 1.0
// Author(s)      : Natalia Portillo
//
// Component      : Commodore Amiga floppy decoder
//
// Revision       : $Revision$
// Last change by : $Author$
// Date           : $Date$
//
// --[ Description ] ----------------------------------------------------------
//
// Methods and structures for Commodore Amiga floppy decoding
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
using System.Runtime.InteropServices;

namespace DiscImageChef.Decoders.Floppy
{
    /// <summary>
    /// Methods and structures for Commodore Amiga decoding
    /// </summary>
    public static class Amiga
    {
        public struct Sector
        {
            /// <summary>
            /// Set to 0x00
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] zero;
            /// <summary>
            /// Set to 0xA1
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public byte[] sync;
            /// <summary>
            /// Set to 0xFF
            /// </summary>
            public byte amiga;
            /// <summary>
            /// Track number
            /// </summary>
            public byte track;
            /// <summary>
            /// Sector number
            /// </summary>
            public byte sector;
            /// <summary>
            /// Remaining sectors til end of writing
            /// </summary>
            public byte remaining;
            /// <summary>
            /// OS dependent tag
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] label;
            /// <summary>
            /// Checksum from <see cref="amiga"/> to <see cref="label"/> 
            /// </summary>
            public UInt32 headerChecksum;
            /// <summary>
            /// Checksum from <see cref="data"/>
            /// </summary>
            public UInt32 dataChecksum;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 512)]
            public byte[] data;
        }
    }
}

