// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Cartridge.cs
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
    public static class Cartridge
    {
        public struct MediumStatus
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
            /// Byte 4, bit 7
            /// Medium is in a cartridge
            /// </summary>
            public bool Cartridge;
            /// <summary>
            /// Byte 4, bit 6
            /// Medium has been taken out/inserted in a cartridge
            /// </summary>
            public bool OUT;
            /// <summary>
            /// Byte 4, bits 5 to 4
            /// Reserved
            /// </summary>
            public byte Reserved3;
            /// <summary>
            /// Byte 4, bit 3
            /// Media is write protected by reason stablished in RAMSWI
            /// </summary>
            public bool MSWI;
            /// <summary>
            /// Byte 4, bit 2
            /// Media is write protected by cartridge
            /// </summary>
            public bool CWP;
            /// <summary>
            /// Byte 4, bit 1
            /// Media is persistently write protected
            /// </summary>
            public bool PWP;
            /// <summary>
            /// Byte 4, bit 0
            /// Reserved
            /// </summary>
            public bool Reserved4;
            /// <summary>
            /// Byte 5
            /// Writable status depending on cartridge
            /// </summary>
            public byte DiscType;
            /// <summary>
            /// Byte 6
            /// Reserved
            /// </summary>
            public byte Reserved5;
            /// <summary>
            /// Byte 7
            /// Reason of specific write protection, only defined 0x01 as "bare disc wp", and 0xFF as unspecified. Rest reserved.
            /// </summary>
            public byte RAMSWI;
        }
    }
}

