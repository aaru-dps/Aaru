// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : Enums.cs
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

namespace DiscImageChef.Decoders.Floppy
{
    /// <summary>
    /// In-sector code for sector size
    /// </summary>
    public enum IBMSectorSizeCode : byte
    {
        /// <summary>
        /// 128 bytes/sector
        /// </summary>
        EighthKilo = 0,
        /// <summary>
        /// 256 bytes/sector
        /// </summary>
        QuarterKilo = 1,
        /// <summary>
        /// 512 bytes/sector
        /// </summary>
        HalfKilo = 2,
        /// <summary>
        /// 1024 bytes/sector
        /// </summary>
        Kilo = 3,
        /// <summary>
        /// 2048 bytes/sector
        /// </summary>
        TwiceKilo = 4,
        /// <summary>
        /// 4096 bytes/sector
        /// </summary>
        FriceKilo = 5,
        /// <summary>
        /// 8192 bytes/sector
        /// </summary>
        TwiceFriceKilo = 6,
        /// <summary>
        /// 16384 bytes/sector
        /// </summary>
        FricelyFriceKilo = 7
    }

    public enum IBMIdType : byte
    {
        IndexMark = 0xFC,
        AddressMark = 0xFE,
        DataMark = 0xFB,
        DeletedDataMark = 0xF8
    }

    public enum AppleEncodedFormat : byte
    {
        /// <summary>
        /// Disk is an Apple II 3.5" disk
        /// </summary>
        AppleII = 0x96,
        /// <summary>
        /// Disk is an Apple Lisa 3.5" disk
        /// </summary>
        Lisa = 0x97,
        /// <summary>
        /// Disk is an Apple Macintosh single-sided 3.5" disk
        /// </summary>
        MacSingleSide = 0x9A,
        /// <summary>
        /// Disk is an Apple Macintosh double-sided 3.5" disk
        /// </summary>
        MacDoubleSide = 0xD9
    }
}

