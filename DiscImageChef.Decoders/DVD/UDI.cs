// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : UDI.cs
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
    public static class UDI
    {
        public struct UniqueDiscIdentifier
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
            /// Byte 4
            /// Reserved
            /// </summary>
            public byte Reserved3;
            /// <summary>
            /// Byte 5
            /// Reserved
            /// </summary>
            public byte Reserved4;
            /// <summary>
            /// Bytes 6 to 7
            /// Random number
            /// </summary>
            public UInt16 RandomNumber;
            /// <summary>
            /// Byte 8 to 11
            /// Year
            /// </summary>
            public UInt32 Year;
            /// <summary>
            /// Byte 12 to 13
            /// Month
            /// </summary>
            public UInt16 Month;
            /// <summary>
            /// Byte 14 to 15
            /// Day
            /// </summary>
            public UInt16 Day;
            /// <summary>
            /// Byte 16 to 17
            /// Hour
            /// </summary>
            public UInt16 Hour;
            /// <summary>
            /// Byte 18 to 19
            /// Minute
            /// </summary>
            public UInt16 Minute;
            /// <summary>
            /// Byte 20 to 21
            /// Second
            /// </summary>
            public UInt16 Second;
        }
    }
}

